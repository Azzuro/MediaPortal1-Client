using System;
using System.IO;
using DotMSN;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Dialogs;

namespace MediaPortal.GUI.MSN
{
	/// <summary>
	/// 
	/// </summary>
	public class GUIMSNChatWindow: GUIWindow
	{
    enum Controls:int
    {
      Status=2,
      List=50
    }

    static int MessageIndex=0;
    static string[] MessageList;

		public GUIMSNChatWindow()
		{
      GetID = (int)GUIWindow.Window.WINDOW_MSN_CHAT;
    }
   
    public override bool Init()
    {
      MessageList = new string[30];

      return Load(GUIGraphicsContext.Skin + @"\my messenger chat.xml");
    }
    public override void OnAction(Action action)
    {
      if (action.wID == Action.ActionType.ACTION_PREVIOUS_MENU)
      {      
				GUIMessage msg= new GUIMessage (GUIMessage.MessageType.GUI_MSG_MSN_CLOSECONVERSATION, (int)GUIWindow.Window.WINDOW_MSN, GetID, 0,0,0,null );
				msg.SendToTargetWindow = true;
				GUIGraphicsContext.SendMessage(msg);
				
				GUIMSNPlugin.CloseConversation();

				GUIWindowManager.PreviousWindow();
        return;

      }

      base.OnAction(action);
    }


    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT : 
          base.OnMessage(message);

          GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
          list.WordWrap=true;         
          
          GUIControl.ClearControl(GetID,(int)Controls.List);
          int j=MessageIndex-30;
          if (j<0) 
            j=0;
          for (int i=0; i < 30;++i)
          {
            AddToList(MessageList[j]);
            j++;
            if (j>MessageList.Length)
              j=0;
          }
          list.ScrollToEnd();
/*          if (g_Player.Playing && !g_Player.Paused)
          {
            if (g_Player.IsVideo || g_Player.IsDVD) g_Player.Pause();
          }
*/          
          return true;

        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT : 
/*          if (g_Player.Playing && g_Player.Paused)
          {
            if (g_Player.IsVideo || g_Player.IsDVD) g_Player.Pause();
          }
*/          
          break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          int iControl=message.SenderControlId;
        break;

				case GUIMessage.MessageType.GUI_MSG_MSN_MESSAGE:
					if ((GUIWindowManager.ActiveWindow != GetID) && !GUIGraphicsContext.IsFullScreenVideo)
					{
						GUIWindowManager.ActivateWindow(GetID);
					}
					AddMessageToList(message.Label);
					break;
				
				case GUIMessage.MessageType.GUI_MSG_NEW_LINE_ENTERED:
          Conversation conversation=GUIMSNPlugin.CurrentConversation;
          if (conversation==null) return true;
          if (conversation.Connected==false) return true;
          conversation.SendMessage(message.Label);

          string text=String.Format(">{0}", message.Label);
          AddToList(text);

          // Store
          MessageList[MessageIndex] = text;
          MessageIndex++;
          if (MessageIndex >= MessageList.Length) 
            MessageIndex = 0;
        break;
      }
      return base.OnMessage(message);
    }

    void Update()
    {
      Conversation conversation=GUIMSNPlugin.CurrentConversation;
      if (conversation==null) return;
      if (conversation.Connected==false) return;

/*      if (GUIMSNPlugin.IsTyping)
      {
        string text=String.Format("{0} {1}", GUIMSNPlugin.ContactName, GUILocalizeStrings.Get(908) );
        GUIControl.SetControlLabel(GetID,(int)Controls.Status,text);
      }
      else 
        GUIControl.SetControlLabel(GetID,(int)Controls.Status,"");
*/
    }

    public override void Process()
    {
      Conversation conversation=GUIMSNPlugin.CurrentConversation;
      if (conversation!=null)
      {
        if (!conversation.Connected || !conversation.Messenger.Connected)
        {
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
          return;
        }
      }
      else
      {
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_MSN);
        return;
      }
      Update();
    }

    private void AddMessageToList(string FormattedText)
    {
      AddToList(FormattedText);

      // Store
      MessageList[MessageIndex] = FormattedText;
      MessageIndex++;
      if (MessageIndex >= MessageList.Length) 
        MessageIndex = 0;
    }
    void AddToList(string text)
    {
      //TODO: add wordwrapping
      GUIListItem item =new GUIListItem(text);
      item.IsFolder=false;
      GUIControl.AddListItemControl(GetID,(int)Controls.List,item);
      GUIListControl list= (GUIListControl)GetControl((int)Controls.List);
			if (list!=null)
			{
				list.ScrollToEnd();
				list.Disabled=true;
			}
    }
  }
}
