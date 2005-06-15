using System;
using MediaPortal.GUI.Library;
using MediaPortal.Player ;
using MediaPortal.TV.Recording;
namespace WindowPlugins.GUITV
{
	/// <summary>
	/// Summary description for GUITVCompressSettings.
	/// </summary>
	public class GUITVCompressSettings : GUIWindow	
	{
		[SkinControlAttribute(3)]				protected GUISpinControl spinType=null;
		[SkinControlAttribute(5)]				protected GUISpinControl spinQuality=null;
		[SkinControlAttribute(7)]				protected GUISpinControl spinScreenSize=null;
		[SkinControlAttribute(9)]				protected GUISpinControl spinFPS=null;
		[SkinControlAttribute(11)]			protected GUISpinControl spinBitrate=null;
		[SkinControlAttribute(13)]			protected GUISpinControl spinPriority=null;
		[SkinControlAttribute(15)]			protected GUICheckMarkControl checkDeleteOriginal=null;

		public GUITVCompressSettings()
		{
			GetID=(int)GUIWindow.Window.WINDOW_TV_COMPRESS_SETTINGS;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\mytvcompresssettings.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadSettings();
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			base.OnPageDestroy (newWindowId);
			SaveSettings();
		}
		void LoadSettings()
		{
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				spinBitrate.Value = xmlreader.GetValueAsInt("compression","bitrate",4);
				spinFPS.Value		 = xmlreader.GetValueAsInt("compression","fps",1);
				spinPriority.Value= xmlreader.GetValueAsInt("compression","priority",0);
				spinQuality.Value= xmlreader.GetValueAsInt("compression","quality",3);
				spinScreenSize.Value= xmlreader.GetValueAsInt("compression","screensize",1);
				spinType.Value= xmlreader.GetValueAsInt("compression","type",0);
				checkDeleteOriginal.Selected= xmlreader.GetValueAsBool("compression","deleteoriginal",true);
			}
			UpdateButtons();
		}

		void SaveSettings()
		{
			
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlreader.SetValue("compression","bitrate",spinBitrate.Value);
				xmlreader.SetValue("compression","fps",spinFPS.Value);
				xmlreader.SetValue("compression","priority",spinPriority.Value);
				xmlreader.SetValue("compression","quality",spinQuality.Value);
				xmlreader.SetValue("compression","screensize",spinScreenSize.Value);
				xmlreader.SetValue("compression","type",spinType.Value);
				xmlreader.SetValueAsBool("compression","deleteoriginal",checkDeleteOriginal.Selected);
			}
		}

		public override void OnAction(Action action)
		{
			switch (action.wID)
			{
				case Action.ActionType.ACTION_SHOW_GUI:
					if ( !g_Player.Playing && Recorder.IsViewing())
					{
						//if we're watching tv
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing && g_Player.IsTV && !g_Player.IsTVRecording)
					{
						//if we're watching a tv recording
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
					}
					else if (g_Player.Playing&&g_Player.HasVideo)
					{
						GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
					}
					break;

			}
			base.OnAction(action);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			base.OnClicked (controlId, control, actionType);
			UpdateButtons();
		}
		void UpdateButtons()
		{
			bool isMpeg2=(spinType.Value==0);
			bool isWMV=(spinType.Value==1);
			bool isXVID=(spinType.Value==2);
			spinBitrate.Disabled=(isMpeg2);
			spinFPS.Disabled=(isMpeg2);
			spinQuality.Disabled=(isMpeg2);
			spinScreenSize.Disabled=(isMpeg2);

			if (isWMV||isXVID)
			{
				bool isCustom=(spinQuality.Value==4);
				spinBitrate.Disabled=!isCustom;
				spinFPS.Disabled=!isCustom;
				spinScreenSize.Disabled=!isCustom;

			}
		}

	}
}
