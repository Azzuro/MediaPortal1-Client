using System;
using System.Drawing;
using System.Collections;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class implementing a GUICheckMarkControl.
	/// </summary>
  public class GUICheckMarkControl: GUIControl
  {
		//TODO: make use of GUILabelControl for drawing text
		[XMLSkinElement("textureCheckmarkNoFocus")] 
												protected string	m_strCheckMarkNoFocus;
		[XMLSkinElement("textureCheckmark")]	protected string	m_strCheckMark;
		[XMLSkinElement("MarkWidth")]			protected int		m_iCheckMarkWidth;
		[XMLSkinElement("MarkHeight")]			protected int		m_iCheckMarkHeight;
		[XMLSkinElement("font")]				protected string	m_strFontName;
		[XMLSkinElement("textcolor")]			protected long  	m_dwTextColor=0xFFFFFFFF;
    [XMLSkinElement("label")]				protected string	m_strLabel="";
    [XMLSkinElement("disabledcolor")]		protected long		m_dwDisabledColor=0xFF606060;
    [XMLSkinElement("align")]				protected Alignment m_dwAlign=Alignment.ALIGN_RIGHT;  
    [XMLSkinElement("shadow")]				protected bool		m_bShadow=false;
											protected GUIImage	m_imgCheckMark=null;
											protected GUIImage	m_imgCheckMarkNoFocus=null;
											protected GUIFont   m_pFont=null;
	
	  public GUICheckMarkControl (int dwParentID) : base(dwParentID)
	  {
	  }
	    /// <summary>
		/// The constructor of the GUICheckMarkControl class.
		/// </summary>
		/// <param name="dwParentID">The parent of this control.</param>
		/// <param name="dwControlId">The ID of this control.</param>
		/// <param name="dwPosX">The X position of this control.</param>
		/// <param name="dwPosY">The Y position of this control.</param>
		/// <param name="dwWidth">The width of this control.</param>
		/// <param name="dwHeight">The height of this control.</param>
		/// <param name="strTextureCheckMark">The filename containing the checked texture.</param>
		/// <param name="strTextureCheckMarkNF">The filename containing the not checked texture.</param>
		/// <param name="dwCheckWidth">The width of the checkmark texture.</param>
		/// <param name="dwCheckHeight">The height of the checkmark texture.</param>
		/// <param name="dwAlign">The alignment of the control.</param>
    public GUICheckMarkControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTextureCheckMark, string strTextureCheckMarkNF,int dwCheckWidth, int dwCheckHeight,GUIControl.Alignment dwAlign)
    :base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
    {
      m_bSelected=false;
      m_dwAlign=dwAlign;
			m_iCheckMarkHeight = dwCheckHeight;
			m_iCheckMarkWidth = dwCheckWidth;
			m_strCheckMark = strTextureCheckMark;
			m_strCheckMarkNoFocus = strTextureCheckMarkNF;
			FinalizeConstruction();
    }

		/// <summary>
		/// This method gets called when the control is created and all properties has been set
		/// It allows the control todo any initialization
		/// </summary>
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction ();
		  
		  m_imgCheckMark = new GUIImage
			  (m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,
			   m_iCheckMarkWidth, m_iCheckMarkHeight, m_strCheckMark ,0);
		  
		  m_imgCheckMarkNoFocus = new GUIImage
			  (m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,
			   m_iCheckMarkWidth, m_iCheckMarkHeight, m_strCheckMarkNoFocus,0);
		  
		  if (m_strFontName!="" && m_strFontName!="-")
			  m_pFont=GUIFontManager.GetFont(m_strFontName);
		  
		  GUILocalizeStrings.LocalizeLabel(ref m_strLabel);
	  }
	   
		/// <summary>
		/// Renders the GUICheckMarkControl.
		/// </summary>
    public override void Render(float timePassed)
    {
			// Do not render if not visible.
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
      }
      int dwTextPosX=m_dwPosX;
      int dwCheckMarkPosX=m_dwPosX;
      if (null!=m_pFont) 
      {
        if (m_dwAlign==GUIControl.Alignment.ALIGN_LEFT)
        {
					// calculate the position of the checkmark if the text appears at the left side of the checkmark
          float fTextHeight=0,fTextWidth=0;
          m_pFont.GetTextExtent( m_strLabel, ref fTextWidth,ref fTextHeight);
          dwCheckMarkPosX += ( (int)(fTextWidth)+5);
        }
        else
        {
					// put text at the right side of the checkmark
          dwTextPosX += m_imgCheckMark.Width +5;
        }
        if (Disabled )
        {
					// If disabled, draw the text in the disabled color.
					m_pFont.DrawText((float)dwTextPosX, (float)m_dwPosY, m_dwDisabledColor, m_strLabel,m_dwAlign,-1);
        }
        else
        {
					// Draw focused text and shadow
          if (Focus)
          {
            if (m_bShadow)
              m_pFont.DrawShadowText((float)dwTextPosX, (float)m_dwPosY, m_dwTextColor, m_strLabel,m_dwAlign,5,5,0xff000000);
            else
              m_pFont.DrawText((float)dwTextPosX, (float)m_dwPosY, m_dwTextColor, m_strLabel,m_dwAlign,-1);
          }
					// Draw non-focused text and shadow
          else
          {
            if (m_bShadow)
              m_pFont.DrawShadowText((float)dwTextPosX, (float)m_dwPosY, m_dwDisabledColor, m_strLabel,m_dwAlign,5,5,0xff000000);
            else
              m_pFont.DrawText((float)dwTextPosX, (float)m_dwPosY, m_dwDisabledColor, m_strLabel,m_dwAlign,-1);
          }
        }
      }
			
			// Render the selected checkmark image
      if (m_bSelected)
      {
        m_imgCheckMark.SetPosition(dwCheckMarkPosX, m_dwPosY); 
        m_imgCheckMark.Render(timePassed);
      }
      else
      {
				// Render the non-selected checkmark image
				m_imgCheckMarkNoFocus.SetPosition(dwCheckMarkPosX, m_dwPosY); 
        m_imgCheckMarkNoFocus.Render(timePassed);
      }
    }

		/// <summary>
		/// OnAction() method. This method gets called when there's a new action like a 
		/// keypress or mousemove or... By overriding this method, the control can respond
		/// to any action
		/// </summary>
		/// <param name="action">action : contains the action</param>
    public override void OnAction(Action action) 
    {
      base.OnAction(action);
      if (Focus)
      {
        if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
					// Send a message that the checkbox was clicked.
					m_bSelected=!m_bSelected;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID, (int)action.wID,0,null);
          GUIGraphicsContext.SendMessage(msg);
        }
      }
    }

		/// <summary>
		/// OnMessage() This method gets called when there's a new message. 
		/// Controls send messages to notify their parents about their state (changes)
		/// By overriding this method a control can respond to the messages of its controls
		/// </summary>
		/// <param name="message">message : contains the message</param>
		/// <returns>true if the message was handled, false if it wasnt</returns>
		public override bool OnMessage(GUIMessage message) 
    {
      if ( message.TargetControlId==GetID )
      {
				// Set the label.
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label!=null)
			      m_strLabel=message.Label;
          return true;
        }
      }
			// Let the base class handle the other messages
			if (base.OnMessage(message)) return true;
      return false;
    }

		/// <summary>
		/// Preallocates the control its DirectX resources.
		/// </summary>
    public override void PreAllocResources() 
    {
      base.PreAllocResources();
      m_imgCheckMark.PreAllocResources();
      m_imgCheckMarkNoFocus.PreAllocResources();
    }

		/// <summary>
		/// Allocates the control its DirectX resources.
		/// </summary>
    public override void AllocResources() 
    {
      base.AllocResources();
      m_imgCheckMark.AllocResources();
      m_imgCheckMarkNoFocus.AllocResources();
      m_pFont=GUIFontManager.GetFont(m_strFontName);
    }

		/// <summary>
		/// Frees the control its DirectX resources.
		/// </summary>
		public override void FreeResources() 
    {
      base.FreeResources();
      m_imgCheckMark.FreeResources();
      m_imgCheckMarkNoFocus.FreeResources();
    }

		/// <summary>
		/// Get/set the color of the text when the control is disabled.
		/// </summary>
    public long DisabledColor
    {
      get { return m_dwDisabledColor;}
      set {m_dwDisabledColor=value;}
    }

		/// <summary>
		/// Set the text of the control. 
		/// </summary>
		/// <param name="strFontName">The font name.</param>
		/// <param name="strLabel">The text.</param>
		/// <param name="dwColor">The font color.</param>
    public void SetLabel(string strFontName,string strLabel,long dwColor)
    {
      if (strFontName==null || strLabel==null) return;
      m_strLabel=strLabel;
	    m_dwTextColor=dwColor;
      m_strFontName=strFontName;
	    m_pFont=GUIFontManager.GetFont(m_strFontName);
    }

		/// <summary>
		/// Set the color of the text on the control. 
		/// </summary>
    public long TextColor 
    { 
      get { return m_dwTextColor;}
      set {m_dwTextColor=value;}
    }

		/// <summary>
		/// Set the alignment of the text on the control. 
		/// </summary>
    public GUIControl.Alignment TextAlignment 
    { 
      get { return m_dwAlign;}
      set { m_dwAlign=value;}
    }

		/// <summary>
		/// Get/set the name of the font of the text of the control.
		/// </summary>
    public string FontName 
    { 
      get { return m_strFontName; }
    }
   
		/// <summary>
		/// Get/set the text of the control.
		/// </summary> 
    public string Label 
    { 
      get { return m_strLabel; }
      set { 
        if (value==null) return;
        m_strLabel=value;
      }
    }

		/// <summary>
		/// Get the width of the texture of the control.
		/// </summary>
    public int CheckMarkWidth
    { 
      get { return m_imgCheckMark.Width; }
    }

		/// <summary>
		/// Get the height of the texture of the control.
		/// </summary>
    public int CheckMarkHeight 
    { 
      get { return m_imgCheckMark.Height ;}
    }
    
		/// <summary>
		/// Get the filename of the checked texture of the control.
		/// </summary>
    public string CheckMarkTextureName
    { 
      get { return m_imgCheckMark.FileName; }
    }
    
		/// <summary>
		/// Get the filename of the not checked texture of the control.
		/// </summary>
    public string CheckMarkTextureNameNF
    { 
      get { return m_imgCheckMarkNoFocus.FileName; }
    }

		/// <summary>
		/// Get/set if the control text needs to be rendered with shadow.
		/// </summary>
    public bool Shadow
    {
      get { return m_bShadow;}
      set { m_bShadow=value;}
    }
 
	}
}
