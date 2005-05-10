using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Util;
using MediaPortal.TagReader;
using MediaPortal.Music.Database;

namespace MediaPortal.Configuration.Sections
{
	public class Wizard_SelectPlugins : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelHD;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label labelFolder;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label labelMusicCount;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label labelPhotoCount;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label labelMovieCount;
		private System.Windows.Forms.Button buttonStopStart;
		private System.ComponentModel.IContainer components = null;

		const int MaximumShares=20;
		long totalAudio;
		long totalVideo;
		long totalPhotos;
		ArrayList sharesVideos = new ArrayList();
		ArrayList sharesMusic = new ArrayList();
		ArrayList sharesPhotos = new ArrayList();
		bool stopScanning=false;
		private System.Windows.Forms.ProgressBar progressBar1;
		bool isScanning=false;
		public Wizard_SelectPlugins() : this("Media Search")
		{
		}

		public Wizard_SelectPlugins(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.buttonStopStart = new System.Windows.Forms.Button();
			this.labelMovieCount = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.labelPhotoCount = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.labelMusicCount = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.labelFolder = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.labelHD = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.progressBar1);
			this.groupBox1.Controls.Add(this.buttonStopStart);
			this.groupBox1.Controls.Add(this.labelMovieCount);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.labelPhotoCount);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.labelMusicCount);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.labelFolder);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.labelHD);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(480, 360);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Find local media";
			this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
			// 
			// buttonStopStart
			// 
			this.buttonStopStart.Location = new System.Drawing.Point(320, 224);
			this.buttonStopStart.Name = "buttonStopStart";
			this.buttonStopStart.Size = new System.Drawing.Size(88, 24);
			this.buttonStopStart.TabIndex = 11;
			this.buttonStopStart.Text = "Scan";
			this.buttonStopStart.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// labelMovieCount
			// 
			this.labelMovieCount.Location = new System.Drawing.Point(176, 232);
			this.labelMovieCount.Name = "labelMovieCount";
			this.labelMovieCount.Size = new System.Drawing.Size(112, 16);
			this.labelMovieCount.TabIndex = 10;
			this.labelMovieCount.Text = "-";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(24, 232);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(128, 16);
			this.label6.TabIndex = 9;
			this.label6.Text = "Total movies found:";
			// 
			// labelPhotoCount
			// 
			this.labelPhotoCount.Location = new System.Drawing.Point(176, 208);
			this.labelPhotoCount.Name = "labelPhotoCount";
			this.labelPhotoCount.Size = new System.Drawing.Size(112, 16);
			this.labelPhotoCount.TabIndex = 8;
			this.labelPhotoCount.Text = "-";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(24, 208);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(128, 16);
			this.label5.TabIndex = 7;
			this.label5.Text = "Total photo\'s found:";
			// 
			// labelMusicCount
			// 
			this.labelMusicCount.Location = new System.Drawing.Point(176, 184);
			this.labelMusicCount.Name = "labelMusicCount";
			this.labelMusicCount.Size = new System.Drawing.Size(112, 16);
			this.labelMusicCount.TabIndex = 6;
			this.labelMusicCount.Text = "-";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(24, 184);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = "Total music files found:";
			// 
			// labelFolder
			// 
			this.labelFolder.Location = new System.Drawing.Point(128, 104);
			this.labelFolder.Name = "labelFolder";
			this.labelFolder.Size = new System.Drawing.Size(328, 64);
			this.labelFolder.TabIndex = 4;
			this.labelFolder.Text = "-";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 104);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 16);
			this.label3.TabIndex = 3;
			this.label3.Text = "Folder:";
			// 
			// labelHD
			// 
			this.labelHD.Location = new System.Drawing.Point(128, 80);
			this.labelHD.Name = "labelHD";
			this.labelHD.Size = new System.Drawing.Size(112, 16);
			this.labelHD.TabIndex = 2;
			this.labelHD.Text = "-";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Harddisk";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(408, 40);
			this.label1.TabIndex = 0;
			this.label1.Text = "Mediaportal will now search your harddisk(s) for any music, photo\'s and movies";
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(32, 264);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(400, 16);
			this.progressBar1.TabIndex = 12;
			this.progressBar1.Visible = false;
			// 
			// Wizard_SelectPlugins
			// 
			this.Controls.Add(this.groupBox1);
			this.Name = "Wizard_SelectPlugins";
			this.Size = new System.Drawing.Size(496, 384);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion



		enum DriveType
		{
			Removable = 2,
			Fixed = 3,
			RemoteDisk = 4,
			CD = 5,
			DVD = 5,
			RamDisk = 6
		}

		private void buttonStop_Click(object sender, System.EventArgs e)
		{
			if (!isScanning)
			{
				buttonStopStart.Text="Stop";
				DoScan();
				buttonStopStart.Text="Scan...";
			}
			else 
			{
				stopScanning=true;
			}
		}

		void DoScan()
		{
			isScanning=true;
			stopScanning=false;
			totalAudio=0;
			totalPhotos=0;
			totalVideo=0;
			string[] drives = Environment.GetLogicalDrives();

			foreach(string drive in drives)
			{
				int driveType=Util.Utils.getDriveType(drive) ;
				if(driveType != (int)DriveType.Fixed) continue;
				labelHD.Text=String.Format("{0}",drive);
				ScanFolder(labelHD.Text,true,true,true);
				if (stopScanning) break;
			}
			SaveShare(sharesMusic,"music");
			SaveShare(sharesVideos,"movies");
			SaveShare(sharesPhotos,"pictures");

			ScanMusic();
			labelMovieCount.Text = totalVideo.ToString();
			labelPhotoCount.Text = totalPhotos.ToString();
			labelMusicCount.Text = totalAudio.ToString();
			labelHD.Text="";
			labelFolder.Text="";
			isScanning=false;
		}

		void ScanTags(string folder, ref ArrayList extractedTags)
		{
			string[] files;
			string[] folders;
			try
			{
				files = Directory.GetFiles(folder);
				folders= Directory.GetDirectories(folder);
			}
			catch(Exception)
			{
				return;
			}
			string strLastThumb="";
			foreach (string file in files)
			{
				byte[] imageBytes=null;
				MusicTag tag = TagReader.TagReader.ReadTag( file, ref imageBytes );

				if(tag != null)
				{
					//
					// Add to list of extracted tags
					//
					extractedTags.Add(new MusicDatabase.MusicData(file, tag));
					progressBar1.Value=extractedTags.Count;
					Application.DoEvents();
					// What to call the thumb?
					string name = String.Format( "{0}-{1}", tag.Artist, tag.Album );
					string strSmallThumb =  Utils.GetCoverArtName( Thumbs.MusicAlbum, name );

					if ( strSmallThumb != strLastThumb )
					{
						// Use the folder.jpg if it's there
						string strImageFile = Utils.GetFolderThumb( file );

						try
						{
							if ( System.IO.File.Exists( strImageFile ) )
							{
								MediaPortal.Util.Picture.CreateThumbnail( strImageFile, strSmallThumb, 128, 128, 0 );
								// MediaPortal.Util.Picture.CreateThumbnail( strImageFile, strLargeThumb, 512, 512, 0 );

								strLastThumb = strSmallThumb;
							}
							else if ( imageBytes != null )
							{
								MemoryStream memoryStream = new MemoryStream( imageBytes );
								Image image = Image.FromStream( memoryStream );

								MediaPortal.Util.Picture.CreateThumbnail( image, strSmallThumb, 128, 128, 0 );
								// MediaPortal.Util.Picture.CreateThumbnail( image, strLargeThumb, 512, 512, 0 );

								strLastThumb = strSmallThumb;
							}
						}
						catch ( Exception )
						{
							// Just skip this one
						}
					}
				}
			}
			foreach (string subfolder in folders)
			{
				if (subfolder!="." && subfolder!="..")
				{
					ScanTags (subfolder,ref extractedTags);
				}
			}
		}

		void ScanMusic()
		{
			progressBar1.Minimum=0;
			progressBar1.Maximum=(int)totalAudio;
			progressBar1.Visible=true;
			ArrayList extractedTags = new ArrayList();
			foreach (Shares.ShareData share in sharesMusic)
			{
				ScanTags(share.Folder,ref extractedTags);
			}
			MediaPortal.Music.Database.MusicDatabase database = new MediaPortal.Music.Database.MusicDatabase();
			database.BeginTransaction();
			foreach(MusicDatabase.MusicData data in extractedTags)
			{
				Song song         = new Song();
				song.FileName     = data.FilePath;
				song.Title		    = data.Tag.Title;
				song.Genre		    = data.Tag.Genre;
				song.Artist	      = data.Tag.Artist;
				song.Album		    = data.Tag.Album;
				song.Year			    =	data.Tag.Year;
				song.Track			  = data.Tag.Track;
				song.Duration	    = data.Tag.Duration;
				database.AddSong(song, true);

			}
			database.CommitTransaction();
			progressBar1.Visible=false;
		}

		void ScanFolder(string folder, bool scanForAudio, bool scanForVideo, bool scanForPhotos)
		{
			string[] files;
			string[] folders;
			try
			{
				string systemFolder=Environment.SystemDirectory;
				int pos=systemFolder.LastIndexOf(@"\");
				string windowsFolder=systemFolder.Substring(0,pos);
				if (folder.IndexOf(windowsFolder)>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Cookies))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Favorites))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.History))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))>=0) return;
				if (folder.IndexOf(Environment.GetFolderPath(Environment.SpecialFolder.Recent))>=0) return;
				files = Directory.GetFiles(folder);
				folders= Directory.GetDirectories(folder);
			}
			catch(Exception)
			{
				return;
			}
			bool isAudioFolder=false;
			bool isVideoFolder=false;
			bool isPhotoFolder=false;
			labelFolder.Text=folder;
			labelMovieCount.Text = totalVideo.ToString();
			labelPhotoCount.Text = totalPhotos.ToString();
			labelMusicCount.Text = totalAudio.ToString();
			Application.DoEvents();
			long videoCount=totalVideo;
			long audioCount=totalAudio;
			long photoCount=totalPhotos;
			foreach (string file in files)
			{
				if (stopScanning) return;
				if (Utils.IsAudio(file))
				{
					totalAudio++;
					if (scanForAudio)
					{
						isAudioFolder=true;
						scanForAudio=false;//no need to scan subfolders
					}
				}
				if (Utils.IsVideo(file))
				{
					totalVideo++;
					if (scanForVideo)
					{
						isVideoFolder=true;
						scanForVideo=false;//no need to scan subfolders
					}
				}
				if (Utils.IsPicture(file))
				{
					if (file.ToLower() !="folder.jpg")
					{
						FileInfo info = new FileInfo(file);
						if (info.Length>= 500*1024) // > 500KByte
						{
							totalPhotos++;
							if (scanForPhotos)
							{
								isPhotoFolder=true;
								scanForPhotos=false;//no need to scan subfolders
							}
						}
					}
				}
			}
			foreach (string subfolder in folders)
			{
				if (stopScanning) return;
				if (subfolder!="." && subfolder!="..")
				{
					ScanFolder (subfolder,scanForAudio,scanForVideo,scanForPhotos);
				}
			}
			if (isAudioFolder)
			{
				audioCount= (totalAudio-audioCount);
				if (audioCount>=5)
					AddAudioShare(folder);
			}
			if (isVideoFolder)
			{
				videoCount= (totalVideo-videoCount);
				if (videoCount>=5)
				AddVideoShare(folder);
			}
			if (isPhotoFolder)
			{
				photoCount= (totalPhotos-photoCount);
				if (photoCount>=5)
					AddPhotoShare(folder);
			}
		}
		void AddAudioShare(string folder)
		{
			string name=folder;
			int pos=folder.LastIndexOf(@"\");
			if (pos>0)
			{
				name=name.Substring(pos+1);
			}
			Shares.ShareData share = new Shares.ShareData(name,folder,"");
			sharesMusic.Add(share);
		}
		void AddVideoShare(string folder)
		{
			string name=folder;
			int pos=folder.LastIndexOf(@"\");
			if (pos>0)
			{
				name=name.Substring(pos+1);
			}
			Shares.ShareData share = new Shares.ShareData(name,folder,"");
			sharesVideos.Add(share);
		}
		void AddPhotoShare(string folder)
		{
			string name=folder;
			int pos=folder.LastIndexOf(@"\");
			if (pos>0)
			{
				name=name.Substring(pos+1);
			}
			Shares.ShareData share = new Shares.ShareData(name,folder,"");
			sharesPhotos.Add(share);
		}

		void SaveShare(ArrayList sharesList,string mediaType)
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string defaultShare = String.Empty;

				for(int index = 0; index < MaximumShares; index++)
				{
					string shareName = String.Format("sharename{0}", index);
					string sharePath = String.Format("sharepath{0}", index);
					string sharePin  = String.Format("pincode{0}", index);

					string shareType = String.Format("sharetype{0}", index);
					string shareServer = String.Format("shareserver{0}", index);
					string shareLogin = String.Format("sharelogin{0}", index);
					string sharePwd  = String.Format("sharepassword{0}", index);
					string sharePort = String.Format("shareport{0}", index);
					string shareRemotePath = String.Format("shareremotepath{0}", index);

					string shareNameData = String.Empty;
					string sharePathData = String.Empty;
					string sharePinData  = String.Empty;

					bool   shareTypeData = false;
					string shareServerData = String.Empty;
					string shareLoginData = String.Empty;
					string sharePwdData = String.Empty;
					int    sharePortData = 21;
					string shareRemotePathData = String.Empty;

					if(sharesList != null && sharesList.Count > index)
					{
						Shares.ShareData shareData = sharesList[index] as Shares.ShareData;

						if(shareData != null)
						{
							shareNameData = shareData.Name;
							sharePathData = shareData.Folder;
							sharePinData  = shareData.PinCode;

							shareTypeData = shareData.IsRemote;
							shareServerData = shareData.Server;
							shareLoginData = shareData.LoginName;
							sharePwdData = shareData.PassWord;
							sharePortData = shareData.Port;
							shareRemotePathData=shareData.RemoteFolder;

							if(index==0)
								defaultShare = shareNameData;
						}
					}

					xmlwriter.SetValue(mediaType, shareName, shareNameData);
					xmlwriter.SetValue(mediaType, sharePath, sharePathData);
					xmlwriter.SetValue(mediaType, sharePin, sharePinData);
          
					xmlwriter.SetValueAsBool(mediaType, shareType, shareTypeData);
					xmlwriter.SetValue(mediaType, shareServer, shareServerData);
					xmlwriter.SetValue(mediaType, shareLogin, shareLoginData);
					xmlwriter.SetValue(mediaType, sharePwd, sharePwdData);
					xmlwriter.SetValue(mediaType, sharePort, sharePortData.ToString());
					xmlwriter.SetValue(mediaType, shareRemotePath, shareRemotePathData);
				}

				xmlwriter.SetValue(mediaType, "default", defaultShare);
			}
		}

		private void groupBox1_Enter(object sender, System.EventArgs e)
		{
		
		}
	}
}

