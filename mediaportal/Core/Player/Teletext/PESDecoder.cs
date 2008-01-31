using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal.Player.Teletext
{
    delegate void PESCallback(int streamid, byte[] header, int headerlen,
                                       byte[] data, int datalen, bool isStart, UInt64 presentTime);

    class PESDecoder
    {
        public const byte TS_PACKET_SYNC = 0x47;
        public const int MAX_PES_PACKET = 65535;

        private PESCallback cb = null;
        bool m_bStart;
        int m_iPesHeaderLen;
        byte[] m_pesHeader = new byte[256];
        int m_pid; // we dont need this i think..
        byte[] m_pesBuffer;
        int m_iWritePos; // next free position in buffer (and therefore also length of already written data)
        int m_iStreamId;
        int m_iPesLength;
        bool hasPayloadStart; // do we have a packet in progress

        public PESDecoder(PESCallback cb)
        {
            Log.Debug("PESDecoder ctor");
            this.cb = cb;
            m_pid = -1;
            m_pesBuffer = new byte[MAX_PES_PACKET];
            m_iWritePos = 0;
            m_iStreamId = -1;
            m_iPesHeaderLen = 0;
            m_iPesLength = 0; 
            hasPayloadStart = false;
        }

        public void Reset()
        {
            Log.Debug("PESDecoder.Reset");
            m_iWritePos = 0;
            m_iPesHeaderLen = 0;
            hasPayloadStart = false;
        }


        public void SetPID(int pid)
        {
            m_pid = pid;
        }

        public void SetStreamId(int streamId)
        {
            m_iStreamId = streamId;
        }

        private bool SanityCheck(TSHeader header, byte[] tsPacket)
        {
            //LogDebug("PesDecoder::OnTsPacket %i", tsPacketCount++);
            if (tsPacket == null)
            {
                Log.Debug("tsPacket null!");
                return false;
            }


            // Assume that correct pid is passed!
            /*if (header.Pid != m_pid) 
            {
                Log.Debug("Header Pid is %i, expected %i", header.Pid, m_pid);
                return false;
            }*/

            if (header.SyncByte != TS_PACKET_SYNC)
            {
                Log.Debug("pesdecoder pid:%x sync error", m_pid);
                return false;
            }

            if (header.TransportError)
            {
                m_bStart = false;
                m_iWritePos = 0;
                m_iPesLength = 0;
                Log.Debug("pesdecoder pid:%x transport error", m_pid);
                return false;
            }

            bool scrambled = (header.TScrambling != 0);
            if (scrambled)
            {
                Log.Debug("pesdecoder scrambled!");
                return false;
            }
            if (header.AdaptionFieldOnly())
            {
                Log.Debug("pesdecoder AdaptionFieldOnly!");
                return false;
            }
            return true;
        }

        public void assert(bool b, string msg) {
            if (!b) {
                Log.Error("Assertion failed in PESDecoder: " + msg);
            }
        }

        public void OnTsPacket(byte[] tsPacket, UInt64 presentTime)
        {
           // Log.Debug("PESDECODER ONTSPACKET");
    	    TSHeader header = new TSHeader(tsPacket);
            if (!SanityCheck(header,tsPacket)) return;

	        int pos = header.PayLoadStart; // where in the pes packet does the payload data start?
 
	        if (header.PayloadUnitStart) // if this header starts a new PES packet
	        {
                //Log.Debug("PESDECODER: PayLoadUnitStart");
                hasPayloadStart = true;
		        if (tsPacket[pos+0]==0 && tsPacket[pos+1]==0 && tsPacket[pos+2]==1)
		        {
                    int streamId = tsPacket[pos + 3];

			        if (m_iStreamId<0){ //if stream id not set yet, get it from this 
                        m_iStreamId = streamId;
                        if (m_iStreamId < 0)
                        {
                            Log.Warn("Stream id less than zero :" + m_iStreamId);
                        }
			        }
                    else if (streamId != m_iStreamId)
                    {
                        if (streamId != 0xBE) // if not a padding stream
                        {
                            Log.Warn("PES decoder - wrong stream ID received! - {0}", streamId);
                        }
                        return;
                    }

                    if(m_iWritePos != 0){
                        //throw new Exception("Buffer is not empty, but new packet is being received!");
                        Log.Warn("PESDECODER: Buffer is not empty, but new packet is being received!");
                    }
			        m_iWritePos = 0;

			        m_iPesHeaderLen=tsPacket[pos+8]+9;
                    
                    if (m_pesHeader.Length < m_iPesHeaderLen) {
                        Log.Error("PESDecoder: Reported header length is bigger than header buffer! : {0} vs {1}", m_pesHeader.Length, m_iPesHeaderLen);
                    }
                    Array.Copy(tsPacket, pos, m_pesHeader, 0, m_iPesHeaderLen);
			       //above replaces -> memcpy(m_pesHeader,&tsPacket[pos],m_iPesHeaderLen);
			        
                    pos += (m_iPesHeaderLen);
			        m_bStart=true;

			        int a = m_pesHeader[4];
			        int b = m_pesHeader[5];

			        m_iPesLength = ( a << 8  ) + b - ( m_iPesHeaderLen - 6); // calculate expected actual payload length
		        }
	        }
            else if (!hasPayloadStart) {
                //Log.Debug("PACKET DISCARDED: END OF PACKET FOR WHICH WE DONT HAVE START");
                return;
            }

	        if (m_iWritePos < 0){
		        Log.Debug("m_iWritePos < 0");
		        return;
	        }
	        if (m_iStreamId <= 0){
		        Log.Debug("m_iStreamId <= 0");
                return;
	        }

            assert(pos > 0 && pos < 188, "Pos error : " + pos);
	        assert(m_iWritePos + 188-pos <= MAX_PES_PACKET, "About to exceed buffer size!"); // check that the buffer is not overrunning

            int bytesToWrite = 188 -pos;
            assert(bytesToWrite < 188, "Bytes to write too big : " + bytesToWrite);
            Array.Copy(tsPacket,pos,m_pesBuffer,m_iWritePos, bytesToWrite);
	        m_iWritePos += bytesToWrite;

	        if ( m_iPesLength == m_iWritePos  ) // we have the expected data
	        {
               // Log.Debug("PESDECODER: GOT COMPLETE PACKET");

               // assert(cb != null, "cb is null!");
		        if (m_iWritePos>0 && cb !=null)
		        {
                    //Log.Debug("PESDECODER: CALLING CALLBACK");
			        cb(m_iStreamId,m_pesHeader, m_iPesHeaderLen,  m_pesBuffer, m_iWritePos, m_bStart,presentTime);

			        m_bStart=false;
				    m_iWritePos=0;
                    hasPayloadStart = false;
		        }
	        }
        }
    }
}



