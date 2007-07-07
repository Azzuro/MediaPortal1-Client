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
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.Picture.Database;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Shows a dialog box with an OK button  
  /// </summary>
  public class GUIDialogExif : GUIDialogWindow
  {
    [SkinControl(2)]
    protected GUILabelControl lblHeading = null;
    [SkinControlAttribute(3)]
    protected GUIImage imgPicture = null;
    [SkinControlAttribute(20)]
    protected GUILabelControl lblImgTitle = null;
    [SkinControlAttribute(21)]
    protected GUILabelControl lblImgDimensions = null;
    [SkinControlAttribute(22)]
    protected GUILabelControl lblResolutions = null;
    [SkinControlAttribute(23)]
    protected GUIFadeLabel lblFlash = null;
    [SkinControlAttribute(24)]
    protected GUIFadeLabel lblMeteringMode = null;
    [SkinControlAttribute(25)]
    protected GUIFadeLabel lblExposureCompensation = null;
    [SkinControlAttribute(26)]
    protected GUIFadeLabel lblShutterSpeed = null;
    [SkinControlAttribute(27)]
    protected GUILabelControl lblDateTakenLabel = null;
    [SkinControlAttribute(28)]
    protected GUILabelControl lblFstop = null;
    [SkinControlAttribute(29)]
    protected GUILabelControl lblExposureTime = null;
    [SkinControlAttribute(30)]
    protected GUIFadeLabel lblCameraModel = null;
    [SkinControlAttribute(31)]
    protected GUIFadeLabel lblEquipmentMake = null;
    [SkinControlAttribute(32)]
    protected GUILabelControl lblViewComments = null;

    int m_iTextureWidth, m_iTextureHeight;
    string fileName;
    Texture m_pTexture = null;

    public GUIDialogExif()
    {
      GetID = (int)Window.WINDOW_DIALOG_EXIF;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogPictureInfo.xml");
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            base.OnMessage(message);
            Update();
            return true;
          }
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          if (m_pTexture != null) m_pTexture.Dispose();
          m_pTexture = null;
          base.OnMessage(message);
          return true;
        }
      }
      return base.OnMessage(message);
    }
        
    public void SetHeading(string strLine)
    {
      LoadSkin();
      AllocResources();
      InitControls();

      lblHeading.Label = strLine;
    }

    public void SetHeading(int iString)
    {
      if (iString == 0) SetHeading(String.Empty);
      else SetHeading(GUILocalizeStrings.Get(iString));
    }


    public string FileName
    {
      get { return fileName; }
      set { fileName = value; }
    }

    void Update()
    {
      if (m_pTexture != null) m_pTexture.Dispose();

      PictureDatabase dbs = new PictureDatabase();
      int iRotate = dbs.GetRotation(FileName);

      m_pTexture = Util.Picture.Load(FileName, iRotate, 512, 512, true, false, out m_iTextureWidth, out m_iTextureHeight);

      lblCameraModel.Label = String.Empty;
      lblDateTakenLabel.Label = String.Empty;
      lblEquipmentMake.Label = String.Empty;
      lblExposureCompensation.Label = String.Empty;
      lblExposureTime.Label = String.Empty;
      lblFlash.Label = String.Empty;
      lblFstop.Label = String.Empty;
      lblImgDimensions.Label = String.Empty;
      lblImgTitle.Label = String.Empty;
      lblMeteringMode.Label = String.Empty;
      lblResolutions.Label = String.Empty;
      lblShutterSpeed.Label = String.Empty;
      lblViewComments.Label = String.Empty;

      using (ExifMetadata extractor = new ExifMetadata())
      {
        ExifMetadata.Metadata metaData = extractor.GetExifMetadata(FileName);

        lblCameraModel.Label = metaData.CameraModel.DisplayValue;
        lblDateTakenLabel.Label = metaData.DatePictureTaken.DisplayValue;
        lblEquipmentMake.Label = metaData.EquipmentMake.DisplayValue;
        lblExposureCompensation.Label = metaData.ExposureCompensation.DisplayValue;
        lblExposureTime.Label = metaData.ExposureTime.DisplayValue;
        lblFlash.Label = metaData.Flash.DisplayValue;
        lblFstop.Label = metaData.Fstop.DisplayValue;
        lblImgDimensions.Label = metaData.ImageDimensions.DisplayValue;
        lblImgTitle.Label = Path.GetFileNameWithoutExtension(FileName);
        lblMeteringMode.Label = metaData.MeteringMode.DisplayValue;
        lblResolutions.Label = metaData.Resolution.DisplayValue;
        lblShutterSpeed.Label = metaData.ShutterSpeed.DisplayValue;
        lblViewComments.Label = metaData.ViewerComments.DisplayValue;

        imgPicture.IsVisible = false;
      }
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
      if (null == m_pTexture) return;
      float x = imgPicture.XPosition;
      float y = imgPicture.YPosition;
      int width;
      int height;
      GUIGraphicsContext.Correct(ref x, ref y);

      GUIFontManager.Present();
      GUIGraphicsContext.GetOutputRect(m_iTextureWidth, m_iTextureHeight, imgPicture.Width, imgPicture.Height, out width, out height);
      Util.Picture.RenderImage(m_pTexture, (int)x, (int)y, width, height, m_iTextureWidth, m_iTextureHeight, 0, 0, true);
    }
  }
}
