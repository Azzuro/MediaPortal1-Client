using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Class which can do transformations for video windows
	/// currently it supports Zoom,normal,stretch, original, letterbox 4:3 and panscan 4:3
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
    int   m_iImageWidth=100;				// width of the video window or image
    int   m_iImageHeight=100;				// height of the height window or image
    int   m_ScreenWidth=100;				// width of the screen
    int   m_ScreenHeight=100;				// height of the screen
    Type  m_eType=Type.Normal;			// type of transformation used
    float m_fPixelRatio=1.0f;				// pixelratio correction 
    

		/// <summary>
		/// Empty constructor
		/// </summary>
    public Geometry()
    {
    }

		/// <summary>
		/// property to get/set the width of the video/image
		/// </summary>
    public int ImageWidth
    {
      get { return m_iImageWidth;}
      set { m_iImageWidth=value;}
    }
    
		/// <summary>
		/// property to get/set the height of the video/image
		/// </summary>
    public int ImageHeight
    {
      get { return m_iImageHeight;}
      set { m_iImageHeight=value;}
    }

		/// <summary>
		/// property to get/set the width of the screen
		/// </summary>
    public int ScreenWidth
    {
      get { return m_ScreenWidth;}
      set { m_ScreenWidth=value;}
    }
    
		/// <summary>
		/// property to get/set the height of the screen
		/// </summary>
    public int ScreenHeight
    {
      get { return m_ScreenHeight;}
      set { m_ScreenHeight=value;}
    }

		/// <summary>
		/// property to get/set the transformation type
		/// </summary>
    public Geometry.Type ARType
    {
      get { return m_eType;}
      set { m_eType=value;}
    }

		/// <summary>
		/// property to get/set the pixel ratio 
		/// </summary>
    public float PixelRatio
    {
      get { return m_fPixelRatio;}
      set {m_fPixelRatio=value;}
    }

		/// <summary>
		/// Method todo the transformation.
		/// It will calculate 2 rectangles. A source and destination rectangle based on the
		/// current transformation , image width/height and screen width/height
		/// the returned source rectangle specifies which part of the image/video should be copied
		/// the returned destination rectangle specifies where the copied part should be presented on screen
		/// </summary>
		/// <param name="rSource">rectangle containing the source rectangle of the image/video</param>
		/// <param name="rDest">rectangle  containing the destination rectangle of the image/video</param>
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

		
		/// <summary>
		/// Calculates the aspect ratio for the current image/video window
		/// <returns>float value containing the aspect ratio
		/// </returns>
		/// </summary>
    float CalculateFrameAspectRatio()
    {
      return  (float)ImageWidth / (float)ImageHeight;
    }

  }
}
