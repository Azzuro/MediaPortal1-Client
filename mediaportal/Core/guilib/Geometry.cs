using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
  public class Geometry
  {
    public enum Type
    {
      Zoom,     //widescreen
      Normal,   //pan scan
      Stretch,  //letterbox
      Original, //original source format
      LetterBox43, // Letterbox 4:3
      PanScan43 // Pan&Scan 4:3
    }
    int   m_iImageWidth=100;
    int   m_iImageHeight=100;
    int   m_ScreenWidth=100;
    int   m_ScreenHeight=100;
    Type  m_eType=Type.Normal;
    float m_fPixelRatio=1.0f;
    

    public Geometry()
    {
    }

    public int ImageWidth
    {
      get { return m_iImageWidth;}
      set { m_iImageWidth=value;}
    }
    
    public int ImageHeight
    {
      get { return m_iImageHeight;}
      set { m_iImageHeight=value;}
    }

    public int ScreenWidth
    {
      get { return m_ScreenWidth;}
      set { m_ScreenWidth=value;}
    }
    
    public int ScreenHeight
    {
      get { return m_ScreenHeight;}
      set { m_ScreenHeight=value;}
    }

    public Geometry.Type ARType
    {
      get { return m_eType;}
      set { m_eType=value;}
    }

    public float PixelRatio
    {
      get { return m_fPixelRatio;}
      set {m_fPixelRatio=value;}
    }

    public void GetWindow(out System.Drawing.Rectangle rSource,out System.Drawing.Rectangle rDest)
    {
      float fSourceFrameRatio=CalculateFrameAspectRatio();
      float fOutputFrameRatio = fSourceFrameRatio /PixelRatio;
      
      switch (ARType)
      {
        case Type.Stretch:
        {
          rSource=new System.Drawing.Rectangle(0,0,ImageWidth,ImageHeight);
          rDest=new System.Drawing.Rectangle(0,0,ScreenWidth,ScreenHeight);
        }
        break;

        case Type.Zoom:
        {  
          // calculate AR compensation (see http://www.iki.fi/znark/video/conversion)
          // assume that the movie is widescreen first, so use full height
          float fVertBorder=0;
          float fNewHeight = (float)( ScreenHeight);
          float fNewWidth  =  fNewHeight*fOutputFrameRatio;
          float fHorzBorder= (fNewWidth-(float)ScreenWidth)/2.0f;
          float fFactor = fNewWidth / ((float)ImageWidth);
          fHorzBorder = fHorzBorder/fFactor;

          if ( (int)fNewWidth < ScreenWidth )
          {
            fHorzBorder=0;
            fNewWidth  = (float)( ScreenWidth);
            fNewHeight = fNewWidth/fOutputFrameRatio;
            fVertBorder= (fNewHeight-(float)ScreenHeight)/2.0f;
            fFactor = fNewWidth / ((float)ImageWidth);
            fVertBorder = fVertBorder/fFactor;
          }
          
          rSource=new System.Drawing.Rectangle((int)fHorzBorder,
                                               (int)fVertBorder,
                                               (int)((float)ImageWidth-2.0f*fHorzBorder),
                                               (int)((float)ImageHeight-2.0f*fVertBorder));
          rDest=new System.Drawing.Rectangle(0,0,ScreenWidth,ScreenHeight);
        }
        break;
  
        case Type.Normal:
        {
          // maximize the movie width
          float fNewWidth  = (float)ScreenWidth;
          float fNewHeight = (float)(fNewWidth/fOutputFrameRatio);

          if (fNewHeight > ScreenHeight)
          {
            fNewHeight = ScreenHeight;
            fNewWidth = fNewHeight*fOutputFrameRatio;
          }

          // this shouldnt happen, but just make sure that everything still fits onscreen
          if (fNewWidth > ScreenWidth || fNewHeight > ScreenHeight)
          {
            fNewWidth=(float)ImageWidth;
            fNewHeight=(float)ImageHeight;
          }

          // Centre the movie
          float iPosY = (ScreenHeight - fNewHeight)/2;
          float iPosX = (ScreenWidth  - fNewWidth)/2;

          rSource=new System.Drawing.Rectangle(0,0,ImageWidth,ImageHeight);
          rDest=new System.Drawing.Rectangle((int)iPosX,(int)iPosY,(int)(fNewWidth+0.5f),(int)(fNewHeight+0.5f) );
        }
        break;
  
        case Type.Original:
        {
          // maximize the movie width
          float fNewWidth  = (float)ImageWidth;
          float fNewHeight = (float)(fNewWidth/fOutputFrameRatio);

          if (fNewHeight > ScreenHeight)
          {
            fNewHeight = ImageHeight;
            fNewWidth = fNewHeight*fOutputFrameRatio;
          }

          // this shouldnt happen, but just make sure that everything still fits onscreen
          if (fNewWidth > ScreenWidth || fNewHeight > ScreenHeight)
          {
            goto case Type.Normal;
          }

          // Centre the movie
          float iPosY = (ScreenHeight - fNewHeight)/2;
          float iPosX = (ScreenWidth  - fNewWidth)/2;

          rSource=new System.Drawing.Rectangle(0,0,ImageWidth,ImageHeight);
          rDest=new System.Drawing.Rectangle((int)iPosX,(int)iPosY,(int)(fNewWidth+0.5f),(int)(fNewHeight+0.5f) );
        }
          break;

        case Type.LetterBox43:
        {
          // shrink movie 33% vertically
          float fNewWidth  = (float)ScreenWidth;
          float fNewHeight = (float)(fNewWidth/fOutputFrameRatio);
          fNewHeight*= (1.0f-0.33333333333f);

          if (fNewHeight > ScreenHeight)
          {
            fNewHeight = ScreenHeight;
            fNewHeight*= (1.0f-0.33333333333f);
            fNewWidth = fNewHeight*fOutputFrameRatio;
          }

          // this shouldnt happen, but just make sure that everything still fits onscreen
          if (fNewWidth > ScreenWidth || fNewHeight > ScreenHeight)
          {
            fNewWidth=(float)ImageWidth;
            fNewHeight=(float)ImageHeight;
          }

          // Centre the movie
          float iPosY = (ScreenHeight - fNewHeight)/2;
          float iPosX = (ScreenWidth  - fNewWidth)/2;

          rSource=new System.Drawing.Rectangle(0,0,ImageWidth,ImageHeight);
          rDest=new System.Drawing.Rectangle((int)iPosX,(int)iPosY,(int)(fNewWidth+0.5f),(int)(fNewHeight+0.5f) );

        }
        break;
  
        case Type.PanScan43:
        {  
          // assume that the movie is widescreen first, so use full height
          float fVertBorder=0;
          float fNewHeight = (float)( ScreenHeight);
          float fNewWidth  =  fNewHeight*fOutputFrameRatio*1.66666666667f;
          float fHorzBorder= (fNewWidth-(float)ScreenWidth)/2.0f;
          float fFactor = fNewWidth / ((float)ImageWidth);
          fHorzBorder = fHorzBorder/fFactor;

          if ( (int)fNewWidth < ScreenWidth )
          {
            fHorzBorder=0;
            fNewWidth  = (float)( ScreenWidth);
            fNewHeight = fNewWidth/fOutputFrameRatio;
            fVertBorder= (fNewHeight-(float)ScreenHeight)/2.0f;
            fFactor = fNewWidth / ((float)ImageWidth);
            fVertBorder = fVertBorder/fFactor;
          }
          
          rSource=new System.Drawing.Rectangle( (int)fHorzBorder,
                                                (int)fVertBorder,
                                                (int)((float)ImageWidth-2.0f*fHorzBorder),
                                                (int)((float)ImageHeight-2.0f*fVertBorder));
          rDest=new System.Drawing.Rectangle(0,0,ScreenWidth,ScreenHeight);
        }
        break;

        default:
        {
          rSource=new System.Drawing.Rectangle(0,0,ImageWidth,ImageHeight);
          rDest=new System.Drawing.Rectangle(0,0,ScreenWidth,ScreenHeight);
        }
        break;
      }
    }

    float CalculateFrameAspectRatio()
    {
      return  (float)ImageWidth / (float)ImageHeight;
    }

  }
}
