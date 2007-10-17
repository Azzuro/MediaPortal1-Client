#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using MediaPortal.GUI.Library;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Util;
using MediaPortal.Player;
using System.Collections;

#pragma warning disable 618
namespace DShowNET.Helper
{
  /// <summary>
  /// 
  /// </summary>
  public class DirectShowUtil
  {
    const int magicConstant = -759872593;

    static DirectShowUtil()
    {
    }

    static public IBaseFilter AddFilterToGraph(IGraphBuilder graphBuilder, string strFilterName)
    {
      try
      {
        IBaseFilter NewFilter = null;
        foreach (Filter filter in Filters.LegacyFilters)
        {
          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);

            int hr = graphBuilder.AddFilter(NewFilter, strFilterName);
            if (hr < 0)
            {
              Log.Error("failed:unable to add filter:{0} to graph", strFilterName);
              NewFilter = null;
            }
            else
            {
              Log.Info("added filter:{0} to graph", strFilterName);
            }
            break;
          }
        }
        if (NewFilter == null)
        {
          Log.Error("failed filter:{0} not found", strFilterName);
        }
        return NewFilter;
      }
      catch (Exception ex)
      {
        Log.Error("failed filter:{0} not found {0}", strFilterName, ex.Message);
        return null;
      }
    }

    static public IBaseFilter AddAudioRendererToGraph(IGraphBuilder graphBuilder, string strFilterName, bool setAsReferenceClock)
    {
      try
      {

        IPin pinOut = null;
        IBaseFilter NewFilter = null;
        Log.Info("add filter:{0} to graph clock:{1}", strFilterName, setAsReferenceClock);

        //check first if audio renderer exists!
        bool bRendererExists = false;
        foreach (Filter filter in Filters.AudioRenderers)
        {

          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            bRendererExists = true;
            Log.Info("DirectShowUtils: found renderer - {0}", filter.Name);
          }
        }
        if (!bRendererExists)
        {
          Log.Error("FAILED: audio renderer:{0} doesnt exists", strFilterName);
          return null;
        }

        // first remove all audio renderers
        bool bAllRemoved = false;
        bool bNeedAdd = true;
        IEnumFilters enumFilters;
        HResult hr = new HResult(graphBuilder.EnumFilters(out enumFilters));

        if (hr >= 0 && enumFilters != null)
        {
          int iFetched;
          enumFilters.Reset();
          while (!bAllRemoved)
          {

            IBaseFilter[] pBasefilter = new IBaseFilter[2];
            hr.Set(enumFilters.Next(1, pBasefilter, out iFetched));
            if (hr < 0 || iFetched != 1 || pBasefilter[0] == null) break;

            foreach (Filter filter in Filters.AudioRenderers)
            {

              Guid classId1;
              Guid classId2;

              pBasefilter[0].GetClassID(out classId1);
              //Log.Info("Filter Moniker string -  " + filter.Name);
              if (filter.Name == "ReClock Audio Renderer")
              {
                Log.Warn("Reclock is installed - if this method fails, reinstall and regsvr32 /u reclock and then uninstall");
                //   return null;

              }

              try
              {
                NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);
                if (NewFilter == null)
                {
                  Log.Info("NewFilter = null");
                  continue;
                }

              }
              catch (Exception e)
              {
                Log.Info("Exception in BindToMoniker({0}): {1}", filter.MonikerString, e.Message);
                continue;
              }
              NewFilter.GetClassID(out classId2);
              Marshal.ReleaseComObject(NewFilter);
              NewFilter = null;

              if (classId1.Equals(classId2))
              {
                if (filter.Name == strFilterName)
                {
                  Log.Info("filter already in graph");

                  if (setAsReferenceClock)
                  {
                    hr.Set((graphBuilder as IMediaFilter).SetSyncSource(pBasefilter[0] as IReferenceClock));
                    Log.Info("setAsReferenceClock sync source " + hr.ToDXString());
                  }
                  Marshal.ReleaseComObject(pBasefilter[0]);
                  pBasefilter[0] = null;
                  bNeedAdd = false;
                  break;
                }
                else
                {
                  Log.Info("remove " + filter.Name + " from graph");
                  pinOut = FindSourcePinOf(pBasefilter[0]);
                  graphBuilder.RemoveFilter(pBasefilter[0]);
                  bAllRemoved = true;
                  break;
                }
              }//if (classId1.Equals(classId2))
            }//foreach (Filter filter in filters.AudioRenderers)
            if (pBasefilter[0] != null)
              Marshal.ReleaseComObject(pBasefilter[0]);
          }//while(!bAllRemoved)
          Marshal.ReleaseComObject(enumFilters);
        }//if (hr>=0 && enumFilters!=null)
        Log.Info("DirectShowUtils: Passed removing audio renderer");
        if (!bNeedAdd) return null;
        // next add the new one...
        foreach (Filter filter in Filters.AudioRenderers)
        {
          if (String.Compare(filter.Name, strFilterName, true) == 0)
          {
            Log.Info("DirectShowUtils: Passed finding Audio Renderer");
            NewFilter = (IBaseFilter)Marshal.BindToMoniker(filter.MonikerString);
            hr.Set(graphBuilder.AddFilter(NewFilter, strFilterName));
            if (hr < 0)
            {
              Log.Error("failed:unable to add filter:{0} to graph", strFilterName);
              NewFilter = null;
            }
            else
            {
              Log.Info("added filter:{0} to graph", strFilterName);
              if (pinOut != null)
              {
                hr.Set(graphBuilder.Render(pinOut));
                if (hr == 0) Log.Info(" pinout rendererd");
                else Log.Error(" failed: pinout render");
              }
              if (setAsReferenceClock)
              {
                hr.Set((graphBuilder as IMediaFilter).SetSyncSource(NewFilter as IReferenceClock));
                Log.Info("setAsReferenceClock sync source " + hr.ToDXString());
              }
              return NewFilter;
            }
          }//if (String.Compare(filter.Name,strFilterName,true) ==0)
        }//foreach (Filter filter in filters.AudioRenderers)
        if (NewFilter == null)
        {
          Log.Error("failed filter:{0} not found", strFilterName);
        }
      }
      catch (Exception ex)
      {
        Log.Error("DirectshowUtil. Failed to add filter:{0} to graph :{1} {2} {3}",
              strFilterName, ex.Message, ex.Source, ex.StackTrace);
      }
      return null;
    }



    static public IPin FindSourcePinOf(IBaseFilter filter)
    {
      int hr = 0;
      IEnumPins pinEnum;
      hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        do
        {
          // Get the next pin
          hr = pinEnum.Next(1, pins, out f);
          if ((hr == 0) && (pins[0] != null))
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir == PinDirection.Input)
            {
              IPin pSourcePin = null;
              hr = pins[0].ConnectedTo(out pSourcePin);
              if (hr >= 0)
              {
                Marshal.ReleaseComObject(pinEnum);
                return pSourcePin;
              }
            }
            Marshal.ReleaseComObject(pins[0]);
          }
        }
        while (hr == 0);
        Marshal.ReleaseComObject(pinEnum);
      }
      return null;
    }

    static private void ListMediaTypes(IPin pin)
    {
      IEnumMediaTypes types;
      pin.EnumMediaTypes(out types);
      types.Reset();
      while (true)
      {
        AMMediaType[] mediaTypes = new AMMediaType[1];
        int typesFetched;
        int hr = types.Next(1, mediaTypes, out typesFetched);
        if (hr != 0 || typesFetched == 0) break;
        Log.Info("Has output type: {0}, {1}", mediaTypes[0].majorType,
          mediaTypes[0].subType);
      }
      Marshal.ReleaseComObject(types);
      Log.Info("-----EndofTypes");
    }


    static private bool TestMediaTypes(IPin pin, IPin receiver)
    {
      bool ret = false;
      IEnumMediaTypes types;
      pin.EnumMediaTypes(out types);
      types.Reset();
      while (true)
      {
        AMMediaType[] mediaTypes = new AMMediaType[1];
        int typesFetched;
        int hr = types.Next(1, mediaTypes, out typesFetched);
        if (hr != 0 || typesFetched == 0) break;
        //Log.Info("Check output type: {0}, {1}", mediaTypes[0].majorType,
        //  mediaTypes[0].subType);
        if (receiver.QueryAccept(mediaTypes[0]) == 0 )
        {
          //Log.Info("Accepted!");
          ret = true;
          break;
        }
      }
      Marshal.ReleaseComObject(types);
      //Log.Info("-----EndofTypes");
      return ret;
    }

    
    static private bool TryConnect(IGraphBuilder graphBuilder, string filtername, IPin outputPin)
    {
      return TryConnect(graphBuilder, filtername, outputPin, true);
    }

    static private bool CheckFilterIsLoaded(IGraphBuilder graphBuilder, String name)
    {
      int hr;
      bool ret = false;
      IEnumFilters enumFilters;
      graphBuilder.EnumFilters(out enumFilters);
      do
      {
        int ffetched;
        IBaseFilter[] filters = new IBaseFilter[1];
        hr = enumFilters.Next(1, filters, out ffetched);
        if (hr == 0 && ffetched > 0)
        {
          FilterInfo info;
          filters[0].QueryFilterInfo(out info);
          string filtername = info.achName;
          Marshal.ReleaseComObject(filters[0]);
          if (filtername.Equals(name))
          {
            ret = true;
            break;
          }
        }
        else
        {
          break;
        }
          
      } while (true);
      Marshal.ReleaseComObject(enumFilters);
      return ret;
    }

    static private bool HasConnection(IPin pin)
    {
      IPin pinInConnected;
      int hr = pin.ConnectedTo(out pinInConnected);
      if (hr != 0 || pinInConnected == null)
      {
        return false;
      }
      else
      {
        Marshal.ReleaseComObject(pinInConnected);
        return true;
      }
    }

    static private bool TryConnect(IGraphBuilder graphBuilder, string filtername, IPin outputPin, IBaseFilter to)
    {
      bool ret = false;
      int hr;
      FilterInfo info;
      PinInfo outputInfo;
      to.QueryFilterInfo(out info);
      outputPin.QueryPinInfo(out outputInfo);
      if (info.achName.Equals(filtername)) return false; //do not connect to self
      Log.Info("Testing filter: {0}", info.achName);
      
      IEnumPins enumPins;
      IPin[] pins = new IPin[1];
      to.EnumPins(out enumPins);
      do
      {
        int pinsFetched;
        hr = enumPins.Next(1, pins, out pinsFetched);
        if (hr != 0 || pinsFetched == 0) break;
        PinDirection direction;
        pins[0].QueryDirection(out direction);
        if (direction == PinDirection.Input && !HasConnection(pins[0])) // && TestMediaTypes(outputPin, pins[0]))
        {
            PinInfo pinInfo;
            pins[0].QueryPinInfo(out pinInfo);
            Log.Info("Trying to connect to {0}",
              pinInfo.name);
            //ListMediaTypes(pins[0]);
            //hr =  outputPin.Connect(pins[0], null);
            hr = graphBuilder.ConnectDirect(outputPin, pins[0], null);
            if (hr == 0)
            {
              Log.Info("Connection succeeded");
              if (RenderOutputPins(graphBuilder, to))
              {
                Log.Info("Successfully rendered pin {0}:{1} to {2}:{3}.", 
                  filtername, outputInfo.name, info.achName, pinInfo.name);
                ret = true;
                Marshal.ReleaseComObject(pins[0]);
                break;
              }
              else
              {
                Log.Info("Rendering got stuck. Trying next filter, and disconnecting {0}!",  outputInfo.name);
                outputPin.Disconnect();
                pins[0].Disconnect();
              }
            }
            else
            {
              Log.Info("Connection failed: {0:x}", hr);
            }
        }
        Marshal.ReleaseComObject(pins[0]);
      } while (true);
      Marshal.ReleaseComObject(enumPins);
      if (!ret)
      {
        Log.Info("Dead end. Could not successfully connect pin {0} to filter {1}!", outputInfo.name, info.achName);
      }
      return ret;
    }

    static ArrayList GetFilters(IGraphBuilder graphBuilder)
    {
      ArrayList ret = new ArrayList();
      IEnumFilters enumFilters;
      graphBuilder.EnumFilters(out enumFilters);
      for (;;) {
        int ffetched;
        IBaseFilter[] filters = new IBaseFilter[1];
        int hr = enumFilters.Next(1, filters, out ffetched);
        if (hr == 0 && ffetched > 0)
        {
          ret.Add(filters[0]);
        }
        else
        {
          break;
        }
      }
      Marshal.ReleaseComObject(enumFilters);
      return ret;
    }

    static void ReleaseFilters(ArrayList filters)
    {
      foreach (IBaseFilter filter in filters)
      {
        Marshal.ReleaseComObject(filter);
      }
    }

    static private bool TryConnect(IGraphBuilder graphBuilder, string filtername, IPin outputPin, bool TryNewFilters)
    {
      int hr;
      Log.Info("----------------TryConnect-------------");
      PinInfo outputInfo;
      outputPin.QueryPinInfo(out outputInfo);
      //ListMediaTypes(outputPin);
      ArrayList currentfilters = GetFilters(graphBuilder);
      foreach ( IBaseFilter filter in currentfilters )
      {
        if (TryConnect(graphBuilder, filtername, outputPin, filter))
        {
          ReleaseFilters(currentfilters);
          return true;
        }
      }
      ReleaseFilters(currentfilters);
      //not found, try new filter from registry
      Log.Info("No preloaded filter could be connected. Trying to load new one from registry");
      IEnumMediaTypes enumTypes;
      hr = outputPin.EnumMediaTypes(out enumTypes);
      if (hr != 0)
      {
        Log.Info("Failed: {0:x}", hr);
        return false;
      }
      Log.Info("Got enum");
      ArrayList major = new ArrayList();
      ArrayList sub = new ArrayList();
      if (TryNewFilters)
      {
        Log.Info("Getting corresponding filters");
        for (; ; )
        {
          AMMediaType[] mediaTypes = new AMMediaType[1];
          int typesFetched;
          hr = enumTypes.Next(1, mediaTypes, out typesFetched);
          if (hr != 0 || typesFetched == 0) break;
          major.Add(mediaTypes[0].majorType);
          sub.Add(mediaTypes[0].subType);
        }
        Marshal.ReleaseComObject(enumTypes);
        Log.Info("Found {0} media types", major.Count);
        Guid[] majorTypes = (Guid[])major.ToArray(typeof(Guid));
        Guid[] subTypes = (Guid[])sub.ToArray(typeof(Guid));
        Log.Info("Loading filters");
        ArrayList filters = FilterHelper.GetFilters(majorTypes, subTypes, (Merit)0x00400000);
        Log.Info("Loaded {0} filters", filters.Count);
        foreach (string name in filters)
        {
          if (!CheckFilterIsLoaded(graphBuilder, name))
          {
            Log.Info("Loading filter: {0}", name);
            IBaseFilter f = DirectShowUtil.AddFilterToGraph(graphBuilder, name);
            if (f != null)
            {
              if (TryConnect(graphBuilder, filtername, outputPin, f))
              {
                Marshal.ReleaseComObject(f);
                return true;
              }
              else
              {
                graphBuilder.RemoveFilter(f);
                Marshal.ReleaseComObject(f);
              }
            }
          }
          else
          {
            Log.Info("Ignoring filter {0}. Already in graph.", name);
          }
        }
      }
      Log.Info("TryConnect failed.");
      return outputInfo.name.StartsWith("~");
    }

    static public bool RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      return RenderOutputPins(graphBuilder, filter, 100);
    }
    static public bool RenderOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter, int maxPinsToRender)
    {
      int pinsRendered = 0;
      bool bAllConnected = true;
      IEnumPins pinEnum;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              if (hr == 0)
              {
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
                Marshal.ReleaseComObject(pinInfo.filter);
              }
              else
              {
                Log.Info("  got pin:?");
              }
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                IPin pConnectPin = null;
                hr = pins[0].ConnectedTo(out pConnectPin);
                if (hr != 0 || pConnectPin == null)
                {
                  hr = 0;
                  if (TryConnect(graphBuilder, info.achName, pins[0]))
                  //if ((hr=graphBuilder.Render(pins[0])) == 0)
                  {
                    Log.Info("  render ok");
                  }
                  else
                  {
                    Log.Error("  render {0} failed:{1:x}", pinInfo.name, hr);
                    bAllConnected = false;
                  }
                  pinsRendered++;
                }
                if (pConnectPin != null)
                  Marshal.ReleaseComObject(pConnectPin);
                pConnectPin = null;
                //else Log.Info("pin is already connected");
              }
              Marshal.ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else iFetched = 0;
        } while (iFetched == 1 && pinsRendered < maxPinsToRender && bAllConnected);
        Marshal.ReleaseComObject(pinEnum);
      }
      return bAllConnected;
    }

    static public void DisconnectOutputPins(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        //Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              //Log.Info("  find pin info");
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              if (hr >= 0)
              {
                //Marshal.ReleaseComObject(pinInfo.filter);
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
              }
              else
                Log.Info("  got pin:?");
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                //Log.Info("  is output");
                IPin pConnectPin = null;
                hr = pins[0].ConnectedTo(out pConnectPin);
                if (hr == 0 && pConnectPin != null)
                {
                  //Log.Info("  pin is connected ");
                  hr = pins[0].Disconnect();
                  if (hr == 0) Log.Info("  disconnected ok");
                  else
                  {
                    Log.Error("  disconnected failed ({0:x})", hr);
                  }
                  Marshal.ReleaseComObject(pConnectPin);
                  pConnectPin = null;
                }
                //else Log.Info("pin is already connected");
              }
              Marshal.ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else iFetched = 0;
        } while (iFetched == 1);
        Marshal.ReleaseComObject(pinEnum);
      }
    }

    static public bool DisconnectAllPins(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      IEnumPins pinEnum;
      int hr = filter.EnumPins(out pinEnum);
      if (hr != 0 || pinEnum == null) return false;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      Log.Info("Disconnecting all pins from filter {0}", info.achName);
      bool allDisconnected = true;
      for (; ; )
      {
        IPin[] pins = new IPin[1];
        int fetched;
        hr = pinEnum.Next(1, pins, out fetched);
        if (hr != 0 || fetched == 0) break;
        PinInfo pinInfo;
        pins[0].QueryPinInfo(out pinInfo);
        if (pinInfo.dir == PinDirection.Output)
        {
          if (!DisconnectPin(graphBuilder, pins[0]))
            allDisconnected = false;
        }
        Marshal.ReleaseComObject(pins[0]);
      }
      Marshal.ReleaseComObject(pinEnum);
      return allDisconnected;
    }

    static public bool DisconnectPin(IGraphBuilder graphBuilder, IPin pin)
    {
      IPin other;
      int hr = pin.ConnectedTo(out other);
      bool allDisconnected = true;
      PinInfo info;
      pin.QueryPinInfo(out info);
      Log.Info("Disconnecting pin {0}", info.name);
      if (hr == 0 && other != null)
      {
        other.QueryPinInfo(out info);
        if (!DisconnectAllPins(graphBuilder, info.filter))
          allDisconnected = false;
        hr = pin.Disconnect();
        if (hr != 0)
        {
          allDisconnected = false;
          Log.Error("Error disconnecting: {0:x}", hr);
        }
        hr = other.Disconnect();
        if (hr != 0)
        {
          allDisconnected = false;
          Log.Error("Error disconnecting other: {0:x}", hr);
        }
        Marshal.ReleaseComObject(other);
      }
      else
      {
        Log.Info("  Not connected");
      }
      return allDisconnected;
    }

    static public bool QueryConnect(IPin pin, IPin other)
    {
      IEnumMediaTypes enumTypes;
      int hr = pin.EnumMediaTypes(out enumTypes);
      if (hr != 0 || enumTypes == null) return false;
      int count = 0;
      for (; ; )
      {
        AMMediaType[] types = new AMMediaType[1];
        int fetched;
        hr = enumTypes.Next(1, types, out fetched);
        if (hr != 0 || fetched == 0) break;
        count++;
        if (other.QueryAccept(types[0]) == 0)
        {
          return true;
        }
      }
      PinInfo info;
      PinInfo infoOther;
      pin.QueryPinInfo(out info);
      other.QueryPinInfo(out infoOther);
      Log.Info("Pins {0} and {1} do not accept each other. Tested {2} media types", info.name, infoOther.name, count);
      return false;
    }

    static public bool ReRenderAll(IGraphBuilder graphBuilder, IBaseFilter filter)
    {
      int pinsRendered = 0;
      bool bAllConnected = true;
      IEnumPins pinEnum;
      FilterInfo info;
      filter.QueryFilterInfo(out info);
      int hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        Log.Info("got pins");
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int iFetched;
        int iPinNo = 0;
        do
        {
          // Get the next pin
          //Log.Info("  get pin:{0}",iPinNo);
          iPinNo++;
          hr = pinEnum.Next(1, pins, out iFetched);
          if (hr == 0)
          {
            if (iFetched == 1 && pins[0] != null)
            {
              PinInfo pinInfo = new PinInfo();
              hr = pins[0].QueryPinInfo(out pinInfo);
              if (hr == 0)
              {
                Log.Info("  got pin#{0}:{1}", iPinNo - 1, pinInfo.name);
                Marshal.ReleaseComObject(pinInfo.filter);
              }
              else
              {
                Log.Info("  got pin:?");
              }
              PinDirection pinDir;
              pins[0].QueryDirection(out pinDir);
              if (pinDir == PinDirection.Output)
              {
                IPin pConnectPin = null;
                
                if (DisconnectPin(graphBuilder, pins[0]))
                {
                  hr = 0;
                  if (TryConnect(graphBuilder, info.achName, pins[0]))
                  //if ((hr = graphBuilder.Render(pins[0])) == 0)
                  {
                    Log.Info("  render ok");
                  }
                  else
                  {
                    Log.Error("  render {0} failed:{1:x}", pinInfo.name, hr);
                    bAllConnected = false;
                  }
                  pinsRendered++;
                }
                if (pConnectPin != null)
                  Marshal.ReleaseComObject(pConnectPin);
                pConnectPin = null;
                //else Log.Info("pin is already connected");
              }
              Marshal.ReleaseComObject(pins[0]);
            }
            else
            {
              iFetched = 0;
              Log.Info("no pins?");
              break;
            }
          }
          else iFetched = 0;
        } while (iFetched == 1);
        Marshal.ReleaseComObject(pinEnum);
      }
      return bAllConnected;
    }

    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    static public void SetARMode(IGraphBuilder graphBuilder, AspectRatioMode ARRatioMode)
    {
      int hr;
      IBaseFilter overlay;
      graphBuilder.FindFilterByName("Overlay Mixer2", out overlay);

      if (overlay != null)
      {
        IPin iPin;
        overlay.FindPin("Input0", out iPin);
        if (iPin != null)
        {
          IMixerPinConfig pMC = iPin as IMixerPinConfig;
          if (pMC != null)
          {
            AspectRatioMode mode;
            hr = pMC.SetAspectRatioMode(ARRatioMode);
            hr = pMC.GetAspectRatioMode(out mode);
            //Marshal.ReleaseComObject(pMC);
          }
          Marshal.ReleaseComObject(iPin);
        }
        Marshal.ReleaseComObject(overlay);
      }


      IEnumFilters enumFilters;
      hr = graphBuilder.EnumFilters(out enumFilters);
      if (hr >= 0 && enumFilters != null)
      {
        int iFetched;
        enumFilters.Reset();
        IBaseFilter[] pBasefilter = new IBaseFilter[2];
        do
        {
          pBasefilter = null;
          hr = enumFilters.Next(1, pBasefilter, out iFetched);
          if (hr == 0 && iFetched == 1 && pBasefilter[0] != null)
          {

            IVMRAspectRatioControl pARC = pBasefilter[0] as IVMRAspectRatioControl;
            if (pARC != null)
            {
              pARC.SetAspectRatioMode(VMRAspectRatioMode.None);
            }
            IVMRAspectRatioControl9 pARC9 = pBasefilter[0] as IVMRAspectRatioControl9;
            if (pARC9 != null)
            {
              pARC9.SetAspectRatioMode(VMRAspectRatioMode.None);
            }

            IEnumPins pinEnum;
            hr = pBasefilter[0].EnumPins(out pinEnum);
            if ((hr == 0) && (pinEnum != null))
            {
              pinEnum.Reset();
              IPin[] pins = new IPin[1];
              int f;
              do
              {
                // Get the next pin
                hr = pinEnum.Next(1, pins, out f);
                if (f == 1 && hr == 0 && pins[0] != null)
                {
                  IMixerPinConfig pMC = pins[0] as IMixerPinConfig;
                  if (null != pMC)
                  {
                    pMC.SetAspectRatioMode(ARRatioMode);
                  }
                  Marshal.ReleaseComObject(pins[0]);
                }
              } while (f == 1);
              Marshal.ReleaseComObject(pinEnum);
            }
            Marshal.ReleaseComObject(pBasefilter[0]);
          }
        } while (iFetched == 1 && pBasefilter[0] != null);
        Marshal.ReleaseComObject(enumFilters);
      }
    }

    static bool IsInterlaced(uint x)
    {
      return ((x) & ((uint)AMInterlace.IsInterlaced)) != 0;
    }
    static bool IsSingleField(uint x)
    {
      return ((x) & ((uint)AMInterlace.OneFieldPerSample)) != 0;
    }
    static bool IsField1First(uint x)
    {
      return ((x) & ((uint)AMInterlace.Field1First)) != 0;
    }

    static VMR9SampleFormat ConvertInterlaceFlags(uint dwInterlaceFlags)
    {
      if (IsInterlaced(dwInterlaceFlags))
      {
        if (IsSingleField(dwInterlaceFlags))
        {
          if (IsField1First(dwInterlaceFlags))
          {
            return VMR9SampleFormat.FieldSingleEven;
          }
          else
          {
            return VMR9SampleFormat.FieldSingleOdd;
          }
        }
        else
        {
          if (IsField1First(dwInterlaceFlags))
          {
            return VMR9SampleFormat.FieldInterleavedEvenFirst;
          }
          else
          {
            return VMR9SampleFormat.FieldInterleavedOddFirst;
          }
        }
      }
      else
      {
        return VMR9SampleFormat.ProgressiveFrame;  // Not interlaced.
      }
    }
    /// <summary>
    /// Find the overlay mixer and/or the VMR9 windowless filters
    /// and tell them we dont want a fixed Aspect Ratio
    /// Mediaportal handles AR itself
    /// </summary>
    /// <param name="graphBuilder"></param>
    static public void EnableDeInterlace(IGraphBuilder graphBuilder)
    {
      //not used anymore
    }

    static public IPin FindVideoPort(ref ICaptureGraphBuilder2 captureGraphBuilder, ref IBaseFilter videoDeviceFilter, ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.VideoPort);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0, out pPin);
      if (hr >= 0 && pPin != null)
        Log.Info("Found videoport pin");
      return pPin;
    }

    static public IPin FindPreviewPin(ref ICaptureGraphBuilder2 captureGraphBuilder, ref IBaseFilter videoDeviceFilter, ref Guid mediaType)
    {
      IPin pPin;
      DsGuid cat = new DsGuid(PinCategory.Preview);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0, out pPin);
      if (hr >= 0 && pPin != null)
        Log.Info("Found preview pin");
      return pPin;
    }

    static public IPin FindCapturePin(ref ICaptureGraphBuilder2 captureGraphBuilder, ref IBaseFilter videoDeviceFilter, ref Guid mediaType)
    {
      IPin pPin = null;
      DsGuid cat = new DsGuid(PinCategory.Capture);
      int hr = captureGraphBuilder.FindPin(videoDeviceFilter, PinDirection.Output, cat, new DsGuid(mediaType), false, 0, out pPin);
      if (hr >= 0 && pPin != null)
        Log.Info("Found capture pin");
      return pPin;
    }

    static public IBaseFilter GetFilterByName(IGraphBuilder graphBuilder, string name)
    {
      int hr = 0;
      IEnumFilters ienumFilt = null;
      IBaseFilter[] foundfilter = new IBaseFilter[2];
      int iFetched = 0;
      try
      {
        hr = graphBuilder.EnumFilters(out ienumFilt);
        if (hr == 0 && ienumFilt != null)
        {
          ienumFilt.Reset();
          do
          {
            hr = ienumFilt.Next(1, foundfilter, out iFetched);
            if (hr == 0 && iFetched == 1)
            {
              FilterInfo filter_infos = new FilterInfo();
              foundfilter[0].QueryFilterInfo(out filter_infos);

              Log.Info("GetFilterByName: {0}, {1}", name, filter_infos.achName);

              if (filter_infos.achName.LastIndexOf(name) != -1)
              {
                Marshal.ReleaseComObject(ienumFilt); ienumFilt = null;
                return foundfilter[0];
              }
              Marshal.ReleaseComObject(foundfilter[0]);
            }
          } while (iFetched == 1 && hr == 0);
          if (ienumFilt != null)
            Marshal.ReleaseComObject(ienumFilt);
          ienumFilt = null;
        }
      }
      catch (Exception)
      {
      }
      finally
      {
        if (ienumFilt != null)
          Marshal.ReleaseComObject(ienumFilt);
      }
      return null;
    }

    static public void RemoveFilters(IGraphBuilder m_graphBuilder)
    {
      int hr;
      if (m_graphBuilder == null) return;
      for (int counter = 0; counter < 100; counter++)
      {
        bool bFound = false;
        IEnumFilters ienumFilt = null;
        try
        {
          hr = m_graphBuilder.EnumFilters(out ienumFilt);
          if (hr == 0)
          {
            int iFetched;
            IBaseFilter[] filter = new IBaseFilter[2]; ;
            ienumFilt.Reset();
            do
            {
              hr = ienumFilt.Next(1, filter, out iFetched);
              if (hr == 0 && iFetched == 1)
              {
                m_graphBuilder.RemoveFilter(filter[0]);
                int hres = Marshal.ReleaseComObject(filter[0]);
                filter[0] = null;
                bFound = true;
              }
            } while (iFetched == 1 && hr == 0);
            if (ienumFilt != null)
              Marshal.ReleaseComObject(ienumFilt);
            ienumFilt = null;

          }
          if (!bFound) return;
        }
        catch (Exception)
        {
          return;
        }
        finally
        {
          if (ienumFilt != null)
            hr = Marshal.ReleaseComObject(ienumFilt);
        }
      }
    }

    public static IntPtr GetUnmanagedSurface(Microsoft.DirectX.Direct3D.Surface surface)
    {
      return surface.GetObjectByValue(magicConstant);
    }
    public static IntPtr GetUnmanagedDevice(Microsoft.DirectX.Direct3D.Device device)
    {
      return device.GetObjectByValue(magicConstant);
    }
    public static IntPtr GetUnmanagedTexture(Microsoft.DirectX.Direct3D.Texture texture)
    {
      return texture.GetObjectByValue(magicConstant);
    }
    static public void FindFilterByClassID(IGraphBuilder m_graphBuilder, Guid classID, out IBaseFilter filterFound)
    {
      filterFound = null;

      if (m_graphBuilder == null) return;
      IEnumFilters ienumFilt = null;
      try
      {
        int hr = m_graphBuilder.EnumFilters(out ienumFilt);
        if (hr == 0 && ienumFilt != null)
        {
          int iFetched;
          IBaseFilter[] filter = new IBaseFilter[2];
          ienumFilt.Reset();
          do
          {
            hr = ienumFilt.Next(1, filter, out iFetched);
            if (hr == 0 && iFetched == 1)
            {
              Guid filterGuid;
              filter[0].GetClassID(out filterGuid);
              if (filterGuid == classID)
              {
                filterFound = filter[0];
                return;
              }
              Marshal.ReleaseComObject(filter[0]);
              filter[0] = null;
            }
          } while (iFetched == 1 && hr == 0);
          if (ienumFilt != null)
            Marshal.ReleaseComObject(ienumFilt);
          ienumFilt = null;
        }
      }
      catch (Exception)
      {
      }
      finally
      {
        if (ienumFilt != null)
          Marshal.ReleaseComObject(ienumFilt);
      }
      return;
    }
    public static string GetFriendlyName(IMoniker mon)
    {
      if (mon == null) return string.Empty;
      object bagObj = null;
      IPropertyBag bag = null;
      try
      {
        IErrorLog errorLog = null;
        Guid bagId = typeof(IPropertyBag).GUID;
        mon.BindToStorage(null, null, ref bagId, out bagObj);
        bag = (IPropertyBag)bagObj;
        object val = "";
        int hr = bag.Read("FriendlyName", out val, errorLog);
        if (hr != 0)
          Marshal.ThrowExceptionForHR(hr);
        string ret = val as string;
        if ((ret == null) || (ret.Length < 1))
          throw new NotImplementedException("Device FriendlyName");
        return ret;
      }
      catch (Exception)
      {
        return null;
      }
      finally
      {
        bag = null;
        if (bagObj != null)
          Marshal.ReleaseComObject(bagObj); bagObj = null;
      }
    }
    static public IPin FindPin(IBaseFilter filter, PinDirection dir, string strPinName)
    {
      int hr = 0;

      IEnumPins pinEnum;
      hr = filter.EnumPins(out pinEnum);
      if ((hr == 0) && (pinEnum != null))
      {
        pinEnum.Reset();
        IPin[] pins = new IPin[1];
        int f;
        do
        {
          // Get the next pin
          hr = pinEnum.Next(1, pins, out f);
          if ((hr == 0) && (pins[0] != null))
          {
            PinDirection pinDir;
            pins[0].QueryDirection(out pinDir);
            if (pinDir == dir)
            {
              PinInfo info;
              pins[0].QueryPinInfo(out info);
              //Marshal.ReleaseComObject(info.filter);
              if (String.Compare(info.name, strPinName) == 0)
              {
                Marshal.ReleaseComObject(pinEnum);
                return pins[0];
              }
            }
            Marshal.ReleaseComObject(pins[0]);
          }
        }
        while (hr == 0);
        Marshal.ReleaseComObject(pinEnum);
      }
      return null;
    }
    static public void RemoveDownStreamFilters(IGraphBuilder graphBuilder, IBaseFilter fromFilter, bool remove)
    {
      IEnumPins enumPins;
      fromFilter.EnumPins(out enumPins);
      if (enumPins == null) return;
      IPin[] pins = new IPin[2];
      int fetched;
      while (enumPins.Next(1, pins, out fetched) == 0)
      {
        if (fetched != 1) break;
        PinDirection dir;
        pins[0].QueryDirection(out dir);
        if (dir != PinDirection.Output)
        {
          Marshal.ReleaseComObject(pins[0]);
          continue;
        }
        IPin pinConnected;
        pins[0].ConnectedTo(out pinConnected);
        if (pinConnected == null)
        {
          Marshal.ReleaseComObject(pins[0]);
          continue;
        }
        PinInfo info;
        pinConnected.QueryPinInfo(out info);
        if (info.filter != null)
        {
          RemoveDownStreamFilters(graphBuilder, info.filter, true);
        }
        Marshal.ReleaseComObject(pins[0]);
      }
      if (remove)
        graphBuilder.RemoveFilter(fromFilter);
      Marshal.ReleaseComObject(enumPins);
    }
    static public void RemoveDownStreamFilters(IGraphBuilder graphBuilder, IPin pin)
    {
      IPin pinConnected;
      pin.ConnectedTo(out pinConnected);
      if (pinConnected == null)
      {
        return;
      }
      PinInfo info;
      pinConnected.QueryPinInfo(out info);
      if (info.filter != null)
      {
        RemoveDownStreamFilters(graphBuilder, info.filter, true);
      }
    }
  }
}
