using System;
using System.Drawing;
using System.Collections;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
	public class GUISpinControl : GUIControl
	{
		public enum SpinType
		{
			SPIN_CONTROL_TYPE_INT,
			SPIN_CONTROL_TYPE_FLOAT,
			SPIN_CONTROL_TYPE_TEXT
		};
		public enum SpinSelect
		{
			SPIN_BUTTON_DOWN,
			SPIN_BUTTON_UP
		};

		public enum eOrientation
		{
			Horizontal,
			Vertical
		};
		[XMLSkinElement("showrange")]		protected bool			m_bShowRange=true;
		[XMLSkinElement("digits")]			protected int			m_iDigits=-1;
		[XMLSkinElement("reverse")]			protected bool			m_bReverse=false;
		[XMLSkinElement("textcolor")]		protected long  		m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("font")]			protected string		m_strFont="";
		[XMLSkinElement("textureUp")]		protected string		m_strUp;
		[XMLSkinElement("textureDown")]		protected string		m_strDown;
		[XMLSkinElement("textureUpFocus")]	protected string		m_strUpFocus; 
		[XMLSkinElement("textureDownFocus")]protected string		m_strDownFocus;

		[XMLSkinElement("align")]			
		protected Alignment		m_dwAlign = Alignment.ALIGN_LEFT;
		[XMLSkinElement("subtype")]			
		protected SpinType		m_iType = SpinType.SPIN_CONTROL_TYPE_TEXT;
		[XMLSkinElement("orientation")]		
		protected eOrientation	m_orientation = eOrientation.Horizontal;
		
		protected int       m_iStart=0;
		protected int       m_iEnd=100;
		protected float     m_fStart=0.0f;
		protected float     m_fEnd=1.0f;
		protected int       m_iValue=0;
		protected float     m_fValue=0.0f;
		
		protected SpinSelect m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
		protected float     m_fInterval=0.1f;
		protected ArrayList m_vecLabels = new ArrayList ();
		protected ArrayList m_vecValues= new ArrayList ();
		protected GUIImage m_imgspinUp=null;
		protected GUIImage m_imgspinDown=null;
		protected GUIImage m_imgspinUpFocus=null;
		protected GUIImage m_imgspinDownFocus=null;
	  
    
		protected GUIFont  m_pFont=null;
    
	  
		
    
		protected string   m_szTyped="";
		protected  int       m_iTypedPos=0;
	
		public GUISpinControl (int dwParentID) : base(dwParentID)
		{
		}
    public GUISpinControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strUp, string strDown, string strUpFocus, string strDownFocus, string strFont, long dwTextColor, SpinType iType,GUIControl.Alignment dwAlign)
      :base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      
      m_dwTextColor=dwTextColor;
      m_strFont=strFont;
      m_dwAlign=dwAlign;
      m_iType=iType;
	  
	  m_strDown = strDown;
	  m_strUp = strUp;
	  m_strUpFocus = strUpFocus;
	  m_strDownFocus = strDownFocus;

	  FinalizeConstruction();
    }
	  public override void FinalizeConstruction()
	  {
		  base.FinalizeConstruction();
		  m_imgspinUp		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strUp,0);
		  m_imgspinDown		= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strDown,0);
		  m_imgspinUpFocus	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strUpFocus,0);
		  m_imgspinDownFocus= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,m_dwWidth, m_dwHeight,m_strDownFocus,0);
      
      m_imgspinUp.Filtering=false;
      m_imgspinDown.Filtering=false;
      m_imgspinUpFocus.Filtering=false;
      m_imgspinDownFocus.Filtering=false;
	  }

    public override void 	Render()
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible)
        {
          m_iTypedPos=0;
          m_szTyped=String.Empty;
          return;
        }
      }
      if (!Focus)
      {
        m_iTypedPos=0;
        m_szTyped=String.Empty;
      }
      int dwPosX=m_dwPosX;
      string wszText;

      if (m_iType == SpinType.SPIN_CONTROL_TYPE_INT)
      {
        string strValue=m_iValue.ToString();
        if (m_iDigits>1)
        {
          while (strValue.Length<m_iDigits) strValue="0"+ strValue;
        }
        if (m_bShowRange)
          wszText=strValue+ "/"+ m_iEnd.ToString();
        else
          wszText=strValue.ToString();
      }
      else if (m_iType==SpinType.SPIN_CONTROL_TYPE_FLOAT)
        wszText=String.Format("{0:2}/{1:2}",m_fValue, m_fEnd);
      else
      {
        wszText="";
        if (m_iValue < m_vecLabels.Count )
        {
          if (m_bShowRange)
          {
            wszText=String.Format("({0}/{1}) {2}", m_iValue+1,(int)m_vecLabels.Count,m_vecLabels[m_iValue] );
          }
          else
          {
            wszText=(string) m_vecLabels[m_iValue] ;
          }
        }
        else String.Format("?{0}?",m_iValue);
          
      }

			int iTextXPos=m_dwPosX;
			int iTextYPos=m_dwPosY;
      if ( m_dwAlign== GUIControl.Alignment.ALIGN_LEFT)
      {
          if (m_pFont!=null)
          {
						if (wszText!=null && wszText.Length>0)
						{
							float fTextHeight=0,fTextWidth=0;
							m_pFont.GetTextExtent( wszText, ref fTextWidth, ref fTextHeight);
              if (Orientation==eOrientation.Horizontal)
              {
                m_imgspinUpFocus.SetPosition((int)fTextWidth + 5+dwPosX+ m_imgspinDown.Width, m_dwPosY);
                m_imgspinUp.SetPosition((int)fTextWidth + 5+dwPosX+ m_imgspinDown.Width, m_dwPosY);
                m_imgspinDownFocus.SetPosition((int)fTextWidth + 5+dwPosX, m_dwPosY);
                m_imgspinDown.SetPosition((int)fTextWidth + 5+dwPosX, m_dwPosY);
              }
              else
              {
                m_imgspinUpFocus.SetPosition((int)fTextWidth + 5+dwPosX, m_dwPosY-(Height/2));
                m_imgspinUp.SetPosition((int)fTextWidth + 5+dwPosX, m_dwPosY-(Height/2));
                m_imgspinDownFocus.SetPosition((int)fTextWidth + 5+dwPosX, m_dwPosY+(Height/2));
                m_imgspinDown.SetPosition((int)fTextWidth + 5+dwPosX, m_dwPosY+(Height/2));
              }
						}
          }
      }
			if ( m_dwAlign== GUIControl.Alignment.ALIGN_CENTER)
			{
				if (m_pFont!=null)
				{
					float fTextHeight=1,fTextWidth=1;
					if (wszText!=null && wszText.Length>0)
					{
						m_pFont.GetTextExtent( wszText, ref fTextWidth, ref fTextHeight);
					}
					if (Orientation==eOrientation.Horizontal)
					{
						iTextXPos=dwPosX+m_imgspinUp.Width;
						iTextYPos=m_dwPosY;
						m_imgspinDownFocus.SetPosition((int)dwPosX, m_dwPosY);
						m_imgspinDown.SetPosition((int)dwPosX, m_dwPosY);
						m_imgspinUpFocus.SetPosition((int)fTextWidth+m_imgspinUp.Width + dwPosX, m_dwPosY);
						m_imgspinUp.SetPosition((int)fTextWidth +m_imgspinUp.Width+ dwPosX, m_dwPosY);

						fTextHeight/=2.0f;
						float fPosY = ((float)m_dwHeight)/2.0f;
						fPosY-=fTextHeight;
						fPosY+=(float)iTextYPos;
						iTextYPos=(int)fPosY;

					}
					else
					{
						iTextXPos=dwPosX;
						iTextYPos=m_dwPosY+Height;
						m_imgspinUpFocus.SetPosition((int)+dwPosX		, m_dwPosY-(Height+(int)fTextHeight)/2);
						m_imgspinUp.SetPosition((int)dwPosX					, m_dwPosY-(Height+(int)fTextHeight)/2);
						m_imgspinDownFocus.SetPosition((int)dwPosX	, m_dwPosY+(Height+(int)fTextHeight)/2);
						m_imgspinDown.SetPosition((int)dwPosX				, m_dwPosY+(Height+(int)fTextHeight)/2);
					}
				}
			}

      if (m_iSelect==SpinSelect.SPIN_BUTTON_UP)
      {
          if (m_bReverse)
          {
              if ( !CanMoveDown() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
          }
          else
          {
              if ( !CanMoveUp() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
          }
      }

      if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN)
      {
          if (m_bReverse)
          {
              if ( !CanMoveUp() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_UP;
          }
          else
          {
              if ( !CanMoveDown() )
                  m_iSelect=SpinSelect.SPIN_BUTTON_UP;
          }
      }

      if ( Focus )
      {
          bool bShow=CanMoveUp();
          if (m_bReverse)
              bShow = CanMoveDown();

          if (m_iSelect==SpinSelect.SPIN_BUTTON_UP && bShow )
              m_imgspinUpFocus.Render();
          else
              m_imgspinUp.Render();

          bShow=CanMoveDown();
          if (m_bReverse)
              bShow = CanMoveUp();
          if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN && bShow)
              m_imgspinDownFocus.Render();
          else
              m_imgspinDown.Render();
      }
      else
      {
          m_imgspinUp.Render();
          m_imgspinDown.Render();
      }

			if (m_pFont!=null)
			{

				if ( m_dwAlign!= GUIControl.Alignment.ALIGN_CENTER)
				{
					if (wszText!=null && wszText.Length>0)
					{
						float fWidth=0,fHeight=0;
						m_pFont.GetTextExtent( wszText, ref fWidth,ref fHeight);
						fHeight/=2.0f;
						float fPosY = ((float)m_dwHeight)/2.0f;
						fPosY-=fHeight;
						fPosY+=(float)m_dwPosY;


						m_pFont.DrawText((float)m_dwPosX-3, (float)fPosY,m_dwTextColor,wszText,m_dwAlign);
					}
				}
				else
				{
					m_pFont.DrawText((float)iTextXPos, (float)iTextYPos,m_dwTextColor,wszText,GUIControl.Alignment.ALIGN_LEFT);
				}
      }


    }
    public override void 	OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.REMOTE_0:
        case Action.ActionType.REMOTE_1:
        case Action.ActionType.REMOTE_2:
        case Action.ActionType.REMOTE_3:
        case Action.ActionType.REMOTE_4:
        case Action.ActionType.REMOTE_5:
        case Action.ActionType.REMOTE_6:
        case Action.ActionType.REMOTE_7:
        case Action.ActionType.REMOTE_8:
        case Action.ActionType.REMOTE_9:
        {
          if ( m_szTyped.Length >= 3)
          {
            m_iTypedPos=0;
            m_szTyped="";
          }
          int iNumber = action.wID - Action.ActionType.REMOTE_0;
     
          m_szTyped+= (char)(iNumber+'0');
          int iValue;
          iValue=Int32.Parse(m_szTyped);
          switch (m_iType)
          {
            case SpinType.SPIN_CONTROL_TYPE_INT:
            {
              if (iValue < m_iStart || iValue > m_iEnd)
              {
                m_iTypedPos=0;
                m_szTyped+=iNumber.ToString();
                iValue=Int32.Parse(m_szTyped);
                if (iValue < m_iStart || iValue > m_iEnd)
                {
                  m_iTypedPos=0;
                  m_szTyped="";
                  return;
                }
              }
              m_iValue=iValue;
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
            }  
              break;

            case SpinType.SPIN_CONTROL_TYPE_TEXT:
            {
              if (iValue < 0|| iValue >= m_vecLabels.Count)
              {
                m_iTypedPos=0;
                m_szTyped+= iNumber.ToString();
                iValue=Int32.Parse(m_szTyped);
                if (iValue < 0|| iValue >= (int)m_vecLabels.Count)
                {
                  m_iTypedPos=0;
                  m_szTyped="";
                  return;
                }
              }
              m_iValue=iValue;
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
            }  
              break;

          }
        }
          break;
      }
      if (action.wID == Action.ActionType.ACTION_PAGE_UP)
      {
        if (!m_bReverse)
          PageDown();
        else
          PageUp();
        return;
      }
      if (action.wID == Action.ActionType.ACTION_PAGE_DOWN)
      {
        if (!m_bReverse)
          PageUp();
        else
          PageDown();
        return;
      }
      bool bUp=false;
      bool bDown=false;
      if (Orientation==eOrientation.Horizontal && action.wID == Action.ActionType.ACTION_MOVE_LEFT)
        bUp=true;
      if (Orientation==eOrientation.Vertical && action.wID == Action.ActionType.ACTION_MOVE_DOWN)
        bUp=true;
      if (bUp)
      {
        if (m_iSelect==SpinSelect.SPIN_BUTTON_UP)
        {
          if (m_bReverse)
          {
            if (CanMoveUp() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return;
            }
          }
          else
          {
            if (CanMoveDown() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return;
            }
          }
        }
      }
      if (Orientation==eOrientation.Horizontal && action.wID == Action.ActionType.ACTION_MOVE_RIGHT)
        bDown=true;
      if (Orientation==eOrientation.Vertical && action.wID == Action.ActionType.ACTION_MOVE_UP)
        bDown=true;

      if (bDown)
      {
        if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN)
        {
          if (m_bReverse)
          {
            if (CanMoveDown() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return;
            }
          }
          else
          {
            if (CanMoveUp() )
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return;
            }
          }
        }
      }
      if (Focus)
      {
        if (action.wID==Action.ActionType.ACTION_MOUSE_CLICK||action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (m_iSelect==SpinSelect.SPIN_BUTTON_UP)
          {
            if (m_bReverse)
              MoveDown();
            else
              MoveUp();
            return;
          }
          if (m_iSelect==SpinSelect.SPIN_BUTTON_DOWN)
          {
            if (m_bReverse)
              MoveUp();
            else
              MoveDown();
            return;
          }
        }
      }
      base.OnAction(action);
    }

    public override bool 	OnMessage(GUIMessage message)
    {
      if (base.OnMessage(message) )
      {
          if (!Focus)
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
          else
            if (message.Param1 == (int)SpinSelect.SPIN_BUTTON_UP)
              m_iSelect = SpinSelect.SPIN_BUTTON_UP; 
            else 
              m_iSelect = SpinSelect.SPIN_BUTTON_DOWN;
          return true;
      }
      if (message.TargetControlId == GetID )
      {
          switch (message.Message)
          {
              case GUIMessage.MessageType.GUI_MSG_ITEM_SELECT:
                Value= (int)message.Param1;
                return true;
              

              case GUIMessage.MessageType.GUI_MSG_LABEL_RESET:
              {
                  m_vecLabels.Clear();
                  m_vecValues.Clear();
                  Value=0;
                  return true;
              }

          case GUIMessage.MessageType.GUI_MSG_SHOWRANGE:
              if (message.Param1!=0 )
                  m_bShowRange=true;
              else
                  m_bShowRange=false;
              break;

              case GUIMessage.MessageType.GUI_MSG_LABEL_ADD:
              {
                  AddLabel(message.Label, (int)message.Param1);
                  return true;
              }

              case GUIMessage.MessageType.GUI_MSG_ITEM_SELECTED:
              {
                  message.Param1= (int)Value ;
                  message.Param2=(int)m_iSelect;

                  if (m_iType==SpinType.SPIN_CONTROL_TYPE_TEXT)
                  {
                      if ( m_iValue>= 0 && m_iValue < m_vecLabels.Count )
                          message.Label=(string)m_vecLabels[m_iValue];

                      if ( m_iValue>= 0 && m_iValue < m_vecValues.Count )
                          message.Param1=(int)m_vecValues[m_iValue];
                  }
                  return true;
              }
          }
      }
      return false;

    }
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_imgspinUp.PreAllocResources();
      m_imgspinUpFocus.PreAllocResources();
      m_imgspinDown.PreAllocResources();
      m_imgspinDownFocus.PreAllocResources();

    }
    public override void 	AllocResources()
    {
      base.AllocResources();
      m_imgspinUp.AllocResources();
      m_imgspinUpFocus.AllocResources();
      m_imgspinDown.AllocResources();
      m_imgspinDownFocus.AllocResources();

      m_pFont=GUIFontManager.GetFont(m_strFont);
      SetPosition(m_dwPosX, m_dwPosY);

    }
    public override void 	FreeResources()
    {
      base.FreeResources();
      m_imgspinUp.FreeResources();
      m_imgspinUpFocus.FreeResources();
      m_imgspinDown.FreeResources();
      m_imgspinDownFocus.FreeResources();
      m_iTypedPos=0;
      m_szTyped="";

    }
    public override void 	SetPosition(int dwPosX, int dwPosY)
    {
      base.SetPosition(dwPosX, dwPosY);

      if (Orientation==eOrientation.Horizontal)
      {
        m_imgspinDownFocus.SetPosition(dwPosX, dwPosY);
        m_imgspinDown.SetPosition(dwPosX, dwPosY);

        m_imgspinUp.SetPosition(m_dwPosX + m_imgspinDown.Width,m_dwPosY);
        m_imgspinUpFocus.SetPosition(m_dwPosX + m_imgspinDownFocus.Width,m_dwPosY);
      }
      else
      {
        m_imgspinUp.SetPosition(m_dwPosX ,m_dwPosY+Height/2);
        m_imgspinUpFocus.SetPosition(m_dwPosX ,m_dwPosY+Height/2);

        m_imgspinDownFocus.SetPosition(dwPosX, dwPosY-Height/2);
        m_imgspinDown.SetPosition(dwPosX, dwPosY-Height/2);

      }
    }
    public override int Width
    {
      get { 
        if (Orientation==eOrientation.Horizontal)
        {
          return m_imgspinDown.Width * 2 ;
        }
        else
        {
          return m_imgspinDown.Width;
        }
      }
    }

    public void SetRange(int iStart, int iEnd)
    {
      m_iStart=iStart;
      m_iEnd=iEnd;
    }
    public void SetFloatRange(float fStart, float fEnd)
    {
      m_fStart=fStart;
      m_fEnd=fEnd;
    }
    public int Value
    {
      get { return m_iValue;}
      set { m_iValue=value;}
    }
    public float FloatValue
    {
      get { return m_fValue;}
      set { m_fValue=value;}
    }
    public void AddLabel(string strLabel, int  iValue)
    {
      m_vecLabels.Add(strLabel);
      m_vecValues.Add(iValue);
    }
		public void Reset()
		{
			m_vecLabels.Clear();
			m_vecValues.Clear();
			Value=0;
		}
    public string GetLabel()
    {
      if (m_iValue <0 || m_iValue >= m_vecLabels.Count) return "";
      string strLabel=(string)m_vecLabels[ m_iValue];
      return strLabel;
    
    }
    public override bool Focus
    {
      get { return m_bHasFocus;}
      set 
      { 
        m_bHasFocus=value;
      }
    }
    public void SetReverse(bool bOnOff)
    {
      m_bReverse=bOnOff;
    }
    public int GetMaximum()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          return m_iEnd;
          
    
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
          return (int)m_vecLabels.Count;
          
        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          return (int)(m_fEnd*10.0f);
          
      }
      return 100;

    }
    public int GetMinimum()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
          return m_iStart;
          
    
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
          return 1;
          
        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          return (int)(m_fStart*10.0f);
          
      }
      return 0;
    }
    public string TexutureUpName 
    { 
      get {return m_imgspinUp.FileName; }
    }
    public string TexutureDownName 
    { 
      get {return m_imgspinDown.FileName; }
    }
    public string TexutureUpFocusName 
    { 
      get {return m_imgspinUpFocus.FileName;} 
    }
    public string TexutureDownFocusName
    { 
      get {return m_imgspinDownFocus.FileName; }
    }
    public long TextColor 
    { 
      get {return m_dwTextColor;}
    }
    public string FontName
    { 
      get {return m_strFont; }
    }
    public GUIControl.Alignment TextAlignment 
    { 
      get { return m_dwAlign;}
    }
    public GUISpinControl.SpinType UpDownType 
    { 
      get { return m_iType;}
	  set { m_iType=value;}
    }
    public int SpinWidth 
    { 
      get { return m_imgspinUp.Width; }
    }
    public int SpinHeight
    { 
      get { return m_imgspinUp.Height; }
    }
    public float FloatInterval
    {
      get {return m_fInterval;}
      set {m_fInterval=value;}
    }

    public bool ShowRange
    {
      get { return m_bShowRange;}
      set {m_bShowRange=value;}
    }
    public int Digits
    {
      get { return m_iDigits;}
      set {m_iDigits=value;}
    }

    protected void PageUp()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
            if (m_iValue-10 >= m_iStart)
                m_iValue-=10;
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
            GUIGraphicsContext.SendMessage(msg);
            return;
        }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        {
            if (m_iValue-10 >= 0)
                m_iValue-=10;
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
            GUIGraphicsContext.SendMessage(msg);
            return;
        }
      }
    }

    protected void PageDown()
    {
      switch (m_iType)
      {
          case SpinType.SPIN_CONTROL_TYPE_INT:
          {
              if (m_iValue+10 <= m_iEnd)
                  m_iValue+=10;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }
          case SpinType.SPIN_CONTROL_TYPE_TEXT:
          {
              if (m_iValue+10 < (int)m_vecLabels.Count )
                  m_iValue+=10;
              GUIMessage msg=new GUIMessage (GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }
      }

    }
    protected bool			CanMoveDown()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
          if (m_iValue+1 <= m_iEnd)
            return true;
          return false;
        }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
        {
          if (m_fValue+m_fInterval <= m_fEnd)
            return true;
          return false;
        }

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        {
          if (m_iValue+1 < (int)m_vecLabels.Count)
            return true;
          return false;
        }
      }
      return false;
    }

    protected bool CanMoveUp()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
          if (m_iValue-1 >= m_iStart)
            return true;
          return false;
        }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
        {
          if (m_fValue-m_fInterval >= m_fStart)
            return true;
          return false;
        }
         
        case SpinType.SPIN_CONTROL_TYPE_TEXT:
        {
          if (m_iValue-1 >= 0)
            return true;
          return false;
        }
      }
      return false;
    }
    protected void			MoveUp()
    {
      switch (m_iType)
      {
        case SpinType.SPIN_CONTROL_TYPE_INT:
        {
            if (m_iValue-1 >= m_iStart)
                m_iValue--;
            GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
            msg.Param1=m_iValue;
            GUIGraphicsContext.SendMessage(msg);
            return;
        }

        case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
              if (m_fValue-m_fInterval >= m_fStart)
                  m_fValue-=m_fInterval;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }
         

        case SpinType.SPIN_CONTROL_TYPE_TEXT:
          {
              if (m_iValue-1 >= 0)
                  m_iValue--;
          
              if (m_iValue< m_vecLabels.Count)
              {
                GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
                msg.Label=(string)m_vecLabels[m_iValue];
                GUIGraphicsContext.SendMessage(msg);
              }
              return;
          }
      }

    }

    protected void			MoveDown()
    {
      switch (m_iType)
      {
          case SpinType.SPIN_CONTROL_TYPE_INT:
          {
              if (m_iValue+1 <= m_iEnd)
                  m_iValue++;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              GUIGraphicsContext.SendMessage(msg);
              return;
          }

          case SpinType.SPIN_CONTROL_TYPE_FLOAT:
          {
              if (m_fValue+m_fInterval <= m_fEnd)
                  m_fValue+=m_fInterval;
              GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
              msg.Param1=m_iValue;
              GUIGraphicsContext.SendMessage(msg);
              return;
          }

          case SpinType.SPIN_CONTROL_TYPE_TEXT:
          {
              if (m_iValue+1 < (int)m_vecLabels.Count)
                  m_iValue++;
              if (m_iValue < (int)m_vecLabels.Count)
              {
                GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED,WindowId, GetID, ParentID,0,0,null);
                msg.Label=(string)m_vecLabels[m_iValue];
                GUIGraphicsContext.SendMessage(msg);
              }
              return;
          }
      }
    }

    public override bool InControl(int x, int y, out int iControlId)
    {
      iControlId=GetID;
      if (x >= m_imgspinUp.XPosition && x <= m_imgspinUp.XPosition+m_imgspinUp.RenderWidth)
      {
        if (y >= m_imgspinUp.YPosition && y <= m_imgspinUp.YPosition+m_imgspinUp.RenderHeight)
        {
          return true;
        }
      }
      if (x >= m_imgspinDown.XPosition && x <= m_imgspinDown.XPosition+m_imgspinDown.RenderWidth)
      {
        if (y >= m_imgspinDown.YPosition && y <= m_imgspinDown.YPosition+m_imgspinDown.RenderHeight)
        {
          return true;
        }
      }
      return false;
    }

    public override bool HitTest(int x,int y,out int controlID, out bool focused)
    {
      controlID=GetID;
      focused=Focus;
			if (x >= m_imgspinUp.XPosition && x <= m_imgspinUp.XPosition+m_imgspinUp.RenderWidth)
			{
				if (y >= m_imgspinUp.YPosition && y <= m_imgspinUp.YPosition+m_imgspinUp.RenderHeight)
				{
          if (m_bReverse)
          {
            if (CanMoveDown())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return true;
            }
          }
          else
          {
            if (CanMoveUp())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_UP;
              return true;
            }
          }
				}
			}
			if (x >= m_imgspinDown.XPosition && x <= m_imgspinDown.XPosition+m_imgspinDown.RenderWidth)
			{
				if (y >= m_imgspinDown.YPosition && y <= m_imgspinDown.YPosition+m_imgspinDown.RenderHeight)
				{
          if (m_bReverse)
          {
            if (CanMoveUp())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return true;
            }
          }
          else
          {
            if (CanMoveDown())
            {
              m_iSelect=SpinSelect.SPIN_BUTTON_DOWN;
              return true;
            }
          }
				}
			}
			Focus=false;
			return false;
		}

    public eOrientation Orientation
    {
      get { return m_orientation;}
      set { m_orientation=value;}
    }

    public override bool CanFocus()
    {
      if (!IsVisible) return false;
      if (Disabled) return false;
      if (m_iType==SpinType.SPIN_CONTROL_TYPE_INT)
      {
        if (m_iStart==m_iEnd) return false; 
      }
      
      if (m_iType==SpinType.SPIN_CONTROL_TYPE_TEXT)
      {
        if (m_vecLabels.Count < 2) return false;
      }
      return true;
    }
	}
}
