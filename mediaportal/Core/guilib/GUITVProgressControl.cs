using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// todo : 
	/// - specify fill colors 2,3,4
	/// - seperate graphic for big-tick displaying current position
	/// - specify x/y position for fill color start
	/// </summary>
	public class GUITVProgressControl : GUIControl
	{
		GUIImage 							m_guiTop=null;
		GUIImage 							m_guiLogo=null;
		GUIImage 							m_guiBottom=null;
		GUIImage 							m_guiTick=null;
		GUIImage 							m_guiFillBackground=null;
		GUIImage 							m_guiFill1=null;
		GUIImage 							m_guiFill2=null;
		GUIImage 							m_guiFill3=null;
		GUIImage 							m_guiLeft=null;
		GUIImage 							m_guiMid=null;
		GUIImage 							m_guiRight=null;
		int      							m_iPercent1=0;
		int      							m_iPercent2=0;
		int      							m_iPercent3=0;

		
		[XMLSkinElement("label")] string	 			m_strProperty="";
		[XMLSkinElement("textcolor")]		protected long  	m_dwTextColor=0xFFFFFFFF;
		[XMLSkinElement("font")]			protected string	m_strFontName="";
		GUIFont						m_pFont=null;
		[XMLSkinElement("startlabel")]	string                m_strLabelLeft="";
		[XMLSkinElement("endlabel")]	string                m_strLabelRight="";
		[XMLSkinElement("toplabel")]	string                m_strLabelTop="";
		[XMLSkinElement("fillbgxoff")]		protected int			m_iFillX;
		[XMLSkinElement("fillbgyoff")]		protected int			m_iFillY;
		[XMLSkinElement("fillheight")]		protected int			m_iFillHeight;
		
		
		[XMLSkinElement("label")]		string                m_strLabel1="";
		[XMLSkinElement("label1")]		string                m_strLabel2="";
		[XMLSkinElement("label2")]		string                m_strLabel3="";
		[XMLSkinElement("TextureOffsetY")]		protected int    m_iTopTextureYOffset=0;
		[XMLSkinElement("toptexture")]			protected string m_strTextureTop;
		[XMLSkinElement("bottomtexture")]		protected string m_strTextureBottom;
		[XMLSkinElement("fillbackgroundtexture")]protected string m_strFillBG;
		[XMLSkinElement("lefttexture")]			protected string m_strLeft;
		[XMLSkinElement("midtexture")]			protected string m_strMid;
		[XMLSkinElement("righttexture")]		protected string m_strRight;
		[XMLSkinElement("texturetick")]			protected string m_strTickTexture;
		[XMLSkinElement("filltexture1")]		protected string m_strTickFill1;
		[XMLSkinElement("filltexture2")]		protected string m_strTickFill2;
		[XMLSkinElement("filltexture3")]		protected string m_strTickFill3;
		[XMLSkinElement("logotexture")]			protected string m_strLogo;
    public GUITVProgressControl(int dwParentID)
      :base(dwParentID)
    {
    }

		public GUITVProgressControl(int dwParentID, int dwControlId, int dwPosX, 
			int dwPosY, int dwWidth, int dwHeight, 
			string strBackGroundTexture,string strBackBottomTexture,
			string strTextureFillBackground,
			string strLeftTexture,string strMidTexture,
			string strRightTexture,string strTickTexure,
			string strTextureFill1,string strTextureFill2,string strTextureFill3,
			string strLogoTextureName)
			:base(dwParentID, dwControlId, dwPosX, dwPosY,dwWidth, dwHeight)
		{
			m_strTextureTop = strBackGroundTexture;
			m_strTextureBottom = strBackBottomTexture;
			m_strFillBG = strTextureFillBackground;
			m_strLeft = strLeftTexture;
			m_strRight = strRightTexture;
			m_strTickTexture = strTickTexure;
			m_strTickFill1 = strTextureFill1;
			m_strTickFill2 = strTextureFill2;
			m_strTickFill3 = strTextureFill3;
			m_strLogo = strLogoTextureName;
			FinalizeConstruction();
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
      if (m_strTextureTop==null) m_strTextureTop=String.Empty;
      if (m_strTextureBottom==null) m_strTextureBottom=String.Empty;
      if (m_strLeft==null) m_strLeft=String.Empty;
      if (m_strMid==null) m_strMid=String.Empty;
      if (m_strRight==null) m_strRight=String.Empty;
      if (m_strTickTexture==null) m_strTickTexture=String.Empty;
      if (m_strTickFill1==null) m_strTickFill1=String.Empty;
      if (m_strTickFill2==null) m_strTickFill2=String.Empty;
      if (m_strTickFill3==null) m_strTickFill3=String.Empty;
      if (m_strFillBG==null) m_strFillBG=String.Empty;
      if (m_strLogo==null) m_strLogo=String.Empty;
			m_guiTop	= new GUIImage(m_dwParentID, m_dwControlID, 0, 0,0, 0,m_strTextureTop,0);
			m_guiBottom	= new GUIImage(m_dwParentID, m_dwControlID, 0, 0,0, 0,m_strTextureBottom,0);
			m_guiLeft	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strLeft,0);
			m_guiMid	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strMid,0);
			m_guiRight	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strRight,0);
			m_guiTick	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strTickTexture,0);
			m_guiFill1	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strTickFill1,0);
			m_guiFill2	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strTickFill2,0);
			m_guiFill3	= new GUIImage(m_dwParentID, m_dwControlID, m_dwPosX, m_dwPosY,0, 0,m_strTickFill3,0);
			m_guiFillBackground= new GUIImage(m_dwParentID, m_dwControlID, 0, 0,0, 0,m_strFillBG,0);
			m_guiTop.KeepAspectRatio=false;
			m_guiBottom.KeepAspectRatio=false;
			m_guiMid.KeepAspectRatio=false;
			m_guiRight.KeepAspectRatio=false;
			m_guiTick.KeepAspectRatio=false;
			m_guiFill1.KeepAspectRatio=false;
			m_guiFill2.KeepAspectRatio=false;
			m_guiFill3.KeepAspectRatio=false;
			m_guiFillBackground.KeepAspectRatio=false;
			m_guiLogo = new GUIImage(m_dwParentID, m_dwControlID, 0, 0,0, 0,m_strLogo,0);
			FontName = m_strFontName;
		}

		public override void ScaleToScreenResolution()
		{

			base.ScaleToScreenResolution ();
			GUIGraphicsContext.ScalePosToScreenResolution(ref m_iFillX, ref m_iFillY);
			GUIGraphicsContext.ScaleVertical(ref m_iFillHeight);
			GUIGraphicsContext.ScaleVertical(ref m_iTopTextureYOffset);
		}

    public override void Render()
    {
      if (GUIGraphicsContext.EditMode==false)
      {
        if (!IsVisible) return;
        if (Disabled) return;
      }
			if (m_strProperty.Length>0)
			{
				string m_strText=GUIPropertyManager.Parse(m_strProperty);
				if(m_strText.Length>0)
				{
					try
					{
						Percentage1=Int32.Parse(m_strText);
					}
					catch(Exception)
          {
          }
          if (Percentage1<0 || Percentage1>100) Percentage1=0;
				}
			}
      if (Label1.Length>0)
      {
        string strText=GUIPropertyManager.Parse(Label1);
        if(strText.Length>0)
        {
          try
          {
            Percentage1=Int32.Parse(strText);
          }
          catch(Exception){}
          if (Percentage1<0 || Percentage1>100) Percentage1=0;
        }
      }

      if (Label2.Length>0)
      {
        string strText=GUIPropertyManager.Parse(Label2);
        if(strText.Length>0)
        {
          try
          {
            Percentage2=Int32.Parse(strText);
          }
          catch(Exception){}
          if (Percentage2<0 || Percentage2>100) Percentage2=0;
        }
      }
      if (Label3.Length>0)
      {
        string strText=GUIPropertyManager.Parse(Label3);
        if(strText.Length>0)
        {
          try
          {
            Percentage3=Int32.Parse(strText);
          }
          catch(Exception){}
          if (Percentage3<0 || Percentage3>100) Percentage3=0;
        }
      }

			int xPos=m_dwPosX;
			m_guiLeft.SetPosition(xPos,m_dwPosY);
			
			xPos=m_dwPosX+m_guiLeft.TextureWidth;
			m_guiMid.SetPosition(xPos,m_dwPosY);
			
			int iWidth=m_dwWidth-(m_guiLeft.TextureWidth+m_guiRight.TextureWidth);
			m_guiMid.Width=iWidth;
      m_guiMid.DoUpdate();

			xPos=iWidth+m_dwPosX+m_guiLeft.TextureWidth;
			m_guiRight.SetPosition(xPos,m_dwPosY);

			m_guiLeft.Render();
			m_guiRight.Render();
			m_guiMid.Render();

      int iWidth1=0,iWidth2=0,iWidth3=0;

      iWidth -= 2*m_iFillX;
      float fWidth=iWidth;
      int iCurPos=0;
      // render fillbkg

      xPos=m_dwPosX+m_guiLeft.TextureWidth+m_iFillX;
      m_guiFillBackground.Width=iWidth;
      m_guiFillBackground.Height=m_guiMid.TextureHeight-m_iFillY*2;
      m_guiFillBackground.SetPosition(xPos,m_dwPosY+m_iFillY);
      m_guiFillBackground.DoUpdate();
      m_guiFillBackground.Render();

      // render first color
      int xoff=5;
      GUIGraphicsContext.ScaleHorizontal(ref xoff);
      xPos=m_dwPosX+m_guiLeft.TextureWidth+m_iFillX+xoff;
      int yPos=m_guiFillBackground.YPosition+(m_guiFillBackground.Height/2)-(m_iFillHeight/2);
      fWidth=(float)iWidth;
      fWidth/=100.0f;
      fWidth*=(float)Percentage1;
      iWidth1=(int)Math.Floor(fWidth);
      if (iWidth>0)
      {
        m_guiFill1.Height=m_iFillHeight;
        m_guiFill1.Width=iWidth1;
        m_guiFill1.SetPosition(xPos,yPos);
        m_guiFill1.DoUpdate();
        m_guiFill1.Render();// red
      }
      iCurPos=iWidth1+xPos;

			//render 2nd color
      int iPercent;
      if (Percentage2>=Percentage1)
      {
        iPercent=Percentage2-Percentage1;
      }
      else iPercent=0;
      fWidth=(float)iWidth;
      fWidth/=100.0f;
      fWidth*=(float)iPercent;
      iWidth2=(int)Math.Floor(fWidth);
      if (iWidth2>0)
      {
        m_guiFill2.Width=iWidth2;
        m_guiFill2.Height=m_guiFill1.Height;
        m_guiFill2.SetPosition(iCurPos,m_guiFill1.YPosition);
        m_guiFill2.DoUpdate();
        m_guiFill2.Render();
      }
      iCurPos=iWidth1+iWidth2+xPos;

      if (Percentage3 >= Percentage2)
      {
        //render 3th color
        iPercent=Percentage3-Percentage2;
      }
      else iPercent=0;
      fWidth=(float)iWidth;
      fWidth/=100.0f;
      fWidth*=(float)iPercent;
      iWidth3=(int)Math.Floor(fWidth);
      if (iWidth3>0)
      {
        m_guiFill3.Width=iWidth3;
        m_guiFill3.Height=m_guiFill2.Height;
        m_guiFill3.SetPosition(iCurPos,m_guiFill2.YPosition);
        m_guiFill3.DoUpdate();
        m_guiFill3.Render();
      }

			// render ticks
      m_guiTick.Height=m_guiTick.TextureHeight;
      m_guiTick.Width=m_guiTick.TextureWidth;
      int posx1=10;
      int posx2=20;
      int posy1=3;
      GUIGraphicsContext.ScaleHorizontal(ref posx1);
      GUIGraphicsContext.ScaleHorizontal(ref posx2);
      GUIGraphicsContext.ScaleVertical(ref posy1);
      for (int i=0; i <= 100; i+=10)
      {
        float fpos=(float)m_dwPosX+m_guiLeft.TextureWidth+posx1;
        fWidth=(float)(iWidth-posx2);	
        fWidth/=100.0f;
        fWidth*= (float)i;
        m_guiTick.SetPosition( (int)(fpos+fWidth),(int)m_dwPosY+posy1);
        m_guiTick.DoUpdate();
        m_guiTick.Render();
      }

			// render top
      xPos=iCurPos - (m_guiTop.TextureWidth/2);
      m_guiTop.SetPosition(xPos, m_dwPosY - m_guiTop.TextureHeight+m_iTopTextureYOffset);
      m_guiTop.Render();

      //render tick @ current position
      m_guiTick.Height=m_guiFillBackground.TextureHeight;
      m_guiTick.Width=m_guiTick.TextureWidth*2;
      m_guiTick.SetPosition( (int)(m_guiTop.XPosition+(m_guiTop.TextureWidth/2)-(m_guiTick.Width/2)),(int)m_guiFillBackground.YPosition);
      m_guiTick.DoUpdate();
      m_guiTick.Render();
      
      // render bottom
      xPos=m_guiTop.XPosition + (m_guiTop.TextureWidth/2) - (m_guiBottom.TextureWidth/2);
      m_guiBottom.SetPosition(xPos, m_dwPosY+m_guiMid.TextureHeight);
      m_guiBottom.Render();


      //render logo
      float fx=(float)m_guiBottom.XPosition;
      fx += ( ( (float)m_guiBottom.TextureWidth)/2f);
      fx -= ( ( (float)m_guiLogo.TextureWidth)/2f);

      float fy=(float)m_guiBottom.YPosition;
      fy += ( ( (float)m_guiBottom.TextureHeight)/2f);
      fy -= ( ( (float)m_guiLogo.TextureHeight)/2f);
      m_guiLogo.SetPosition((int)fx, (int)fy);
      m_guiLogo.DoUpdate();
      m_guiLogo.Render();
      
			if (m_pFont!=null)
			{
				float fW=0,fH=0;
				float fHeight=0;
				string strText="";

				// render top text
				if (m_strLabelTop.Length>0)
				{
					strText=GUIPropertyManager.Parse(m_strLabelTop);
					m_pFont.GetTextExtent(strText,ref  fW,ref fH);
					fW /= 2.0f;
					fH /= 2.0f;
					fWidth = ((float)m_guiTop.TextureWidth)/2.0f;
					fHeight= ((float)m_guiTop.TextureHeight)/2.0f;
					fWidth  -= fW;
					fHeight -= fH;
					m_pFont.DrawText((float)m_guiTop.XPosition+fWidth,(float)2+m_guiTop.YPosition+fHeight,m_dwTextColor,strText,GUIControl.Alignment.ALIGN_LEFT);
				}


				// render left text
				if (m_strLabelLeft.Length>0)
				{
					strText=GUIPropertyManager.Parse(m_strLabelLeft);
					m_pFont.GetTextExtent(strText,ref  fW,ref fH);
					fW /= 2.0f;
					fH /= 2.0f;
					fWidth = ((float)m_guiLeft.TextureWidth)/2.0f;
					fHeight= ((float)m_guiLeft.TextureHeight)/2.0f;
					fWidth  -= fW;
					fHeight -= fH;
					m_pFont.DrawText((float)m_dwPosX+fWidth,(float)m_dwPosY+fHeight,m_dwTextColor,strText,GUIControl.Alignment.ALIGN_LEFT);
				}

				// render right text
				if (m_strLabelRight.Length>0)
				{
					strText=GUIPropertyManager.Parse(m_strLabelRight);
					m_pFont.GetTextExtent(strText,ref  fW,ref fH);
					fW /= 2.0f;
					fH /= 2.0f;
					fWidth = ((float)m_guiRight.TextureWidth)/2.0f;
					fHeight= ((float)m_guiRight.TextureHeight)/2.0f;
					fWidth  -= fW;
					fHeight -= fH;
					m_pFont.DrawText((float)m_guiRight.XPosition+fWidth,(float)m_guiRight.YPosition+fHeight,m_dwTextColor,strText,GUIControl.Alignment.ALIGN_LEFT);
				}
			}
    }

    public override bool  CanFocus()
    {
      return false;
    }

    public int Percentage1
    {
      get { return m_iPercent1;}
      set 
      {  
        m_iPercent1=value;
        if (m_iPercent1<0) m_iPercent1=0;
        if (m_iPercent1>100) m_iPercent1=100;
      }
    }
		public int Percentage2
		{
			get { return m_iPercent2;}
      set 
      {  
        m_iPercent2=value;
        if (m_iPercent2<0) m_iPercent2=0;
        if (m_iPercent2>100) m_iPercent2=100;
      }		
    }
		public int Percentage3
		{
			get { return m_iPercent3;}
      set 
      {  
        m_iPercent3=value;
        if (m_iPercent3<0) m_iPercent3=0;
        if (m_iPercent3>100) m_iPercent3=100;
      }
		}

    public override void FreeResources()
    {
      base.FreeResources();
      m_guiTop.FreeResources();
      m_guiMid.FreeResources();
      m_guiRight.FreeResources();
      m_guiLeft.FreeResources();
			m_guiFill1.FreeResources();
			m_guiFill2.FreeResources();
			m_guiFill3.FreeResources();
      m_guiFillBackground.FreeResources();
			m_guiTick.FreeResources();
			m_guiBottom.FreeResources();
			m_guiLogo.FreeResources();
    }

    public override void PreAllocResources()
    {
      base.PreAllocResources();
      m_guiTop.PreAllocResources();
			m_guiBottom.PreAllocResources();
      m_guiMid.PreAllocResources();
      m_guiRight.PreAllocResources();
			m_guiLeft.PreAllocResources();
      m_guiFillBackground.PreAllocResources();
			m_guiFill1.PreAllocResources();
			m_guiFill2.PreAllocResources();
			m_guiFill3.PreAllocResources();
			m_guiTick.PreAllocResources();
			m_guiLogo.PreAllocResources();
    }

    public override void AllocResources()
    {
      base.AllocResources();
      m_guiTop.AllocResources();
			m_guiBottom.AllocResources();
      m_guiMid.AllocResources();
      m_guiRight.AllocResources();
      m_guiLeft.AllocResources();
      m_guiFillBackground.AllocResources();
			m_guiFill1.AllocResources();
			m_guiFill2.AllocResources();
			m_guiFill3.AllocResources();
			m_guiTick.AllocResources();
			m_guiLogo.AllocResources();

			m_guiTop.Filtering=false;
			m_guiBottom.Filtering=false;
			m_guiMid.Filtering=false;
			m_guiRight.Filtering=false;
			m_guiLeft.Filtering=false;
			m_guiFill1.Filtering=false;
			m_guiFill2.Filtering=false;
			m_guiFill3.Filtering=false;
			m_guiTick.Filtering=false;
			if (m_dwHeight==0)
			{
				m_dwHeight=m_guiRight.TextureHeight;
			}
//      m_guiTop.Height=m_dwHeight;
      m_guiRight.Height=m_dwHeight;
      m_guiLeft.Height=m_dwHeight;
      m_guiMid.Height=m_dwHeight;
			m_guiFill1.Height=m_dwHeight-6;
			m_guiFill2.Height=m_dwHeight-6;
			m_guiFill3.Height=m_dwHeight-6;
			//m_guiTick.Height=m_dwHeight;
    }
    public string FillBackGroundName
    {
      get { return m_guiFillBackground.FileName;}
    }

    public string Fill1TextureName
    {
      get { return m_guiFill1.FileName;}
		}
		public string Fill2TextureName
		{
			get { return m_guiFill2.FileName;}
		}
		public string Fill3TextureName
		{
			get { return m_guiFill3.FileName;}
		}

		public string TickTextureName
		{
			get { return m_guiTick.FileName;}
		}
		public string TopTextureName
		{
			get { return m_guiTop.FileName;}
		}
		public string BottomTextureName
		{
			get { return m_guiBottom.FileName;}
		}
    public string BackTextureLeftName
    {
      get { return m_guiLeft.FileName;}
    }
    public string BackTextureMidName
    {
      get { return m_guiMid.FileName;}
    }
    public string BackTextureRightName
    {
      get { return m_guiRight.FileName;}
    }
    public string LogoTextureName
    {
      get { return m_guiLogo.FileName;}
    }

		/// <summary>
		/// Get/set the text of the label.
		/// </summary>
		public string Property
		{
			get { return m_strProperty; }
      set 
      { 
        if (value==null) return;
        m_strProperty=value;
      }
		}
		public string LabelLeft
		{
			get { return m_strLabelLeft; }
      set 
      { 
        if (value==null) return;
        m_strLabelLeft=value;
      }
		}
		
		public string LabelTop
		{
			get { return m_strLabelTop; }
      set 
      { 
        if (value==null) return;
        m_strLabelTop=value;
      }
		}
		public string LabelRight
		{
			get { return m_strLabelRight; }
      set 
      { 
        if (value==null) return;
        m_strLabelRight=value;
      }
		}

		/// <summary>
		/// Get/set the color of the text
		/// </summary>
		public long	TextColor
		{ 
			get { return m_dwTextColor;}
			set { m_dwTextColor=value;}
		}

		/// <summary>
		/// Get/set the name of the font.
		/// </summary>
		public string FontName
		{
			get { return m_pFont.FontName; }
      set 
      {  
        if (value==null) return;
        m_pFont=GUIFontManager.GetFont(value);
      }
		}

    public int	FillX
    { 
      get { return m_iFillX;}
      set 
      {  
        if (value<0) return;
        m_iFillX=value;
      }
    }
    public int	FillY
    { 
      get { return m_iFillY;}
      set 
      {  
        if (value<0) return;
        m_iFillY=value;
      }
    }
    public int	FillHeight
    { 
      get { return m_iFillHeight;}
      set 
      {  
        if (value<0) return;
        m_iFillHeight=value;
      }
    }
    public string Label1
    {
      get {return m_strLabel1;}
      set 
      { 
        if (value==null) return;
        m_strLabel1=value;
      }
    }
    public string Label2
    {
      get {return m_strLabel2;}
      set 
      { 
        if (value==null) return;
        m_strLabel2=value;
      }
    }
    public string Label3
    {
      get {return m_strLabel3;}
      set 
      { 
        if (value==null) return;
        m_strLabel3=value;
      }
    }
    public int TopTextureYOffset
    {
      get { return m_iTopTextureYOffset;}
      set { 
        if (value<0) return;
        m_iTopTextureYOffset=value;
      }
    }
    
  }
}
