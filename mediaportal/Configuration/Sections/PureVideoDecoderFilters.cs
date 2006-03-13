#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
#pragma warning disable 108
namespace MediaPortal.Configuration.Sections
{

    public class PureVideoDecoderFilters : MediaPortal.Configuration.SectionSettings
    {
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxSpeakerSetup;
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxHeadphones;
        private MediaPortal.UserInterface.Controls.MPGroupBox AudioDecoderSettings;
        private MediaPortal.UserInterface.Controls.MPGroupBox VideoDecoderSettings;
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDeInterlaceControl;
        private MediaPortal.UserInterface.Controls.MPLabel DeinterlaceControl;
        private MediaPortal.UserInterface.Controls.MPCheckBox checkBoxDxVA;
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDeInterlaceMode;
        private MediaPortal.UserInterface.Controls.MPGroupBox DisplayType;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDTAnamorphic;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDTPan;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDTLetterbox;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonDTDefault;
        private MediaPortal.UserInterface.Controls.MPLabel DeinterlaceMode;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonHeadphones;
        private MediaPortal.UserInterface.Controls.MPLabel ProLogicII;
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxProLogicII;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonSpeakers;
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxDRC;
        private MediaPortal.UserInterface.Controls.MPLabel DynamicRangeControl;
        private MediaPortal.UserInterface.Controls.MPRadioButton radioButtonReceiver;
        private MediaPortal.UserInterface.Controls.MPComboBox comboBoxOutPutMode;
        private MediaPortal.UserInterface.Controls.MPGroupBox SpeakerSetup;
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 
        /// </summary>
        public PureVideoDecoderFilters()
            : this("NVIDIA PureVideo Decoder Settings")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public PureVideoDecoderFilters(string name)
            : base(name)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.VideoDecoderSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.DisplayType = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.radioButtonDTAnamorphic = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.radioButtonDTPan = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.radioButtonDTLetterbox = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.radioButtonDTDefault = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.DeinterlaceMode = new MediaPortal.UserInterface.Controls.MPLabel();
            this.comboBoxDeInterlaceMode = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.comboBoxDeInterlaceControl = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.DeinterlaceControl = new MediaPortal.UserInterface.Controls.MPLabel();
            this.checkBoxDxVA = new MediaPortal.UserInterface.Controls.MPCheckBox();
            this.AudioDecoderSettings = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.comboBoxOutPutMode = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.DynamicRangeControl = new MediaPortal.UserInterface.Controls.MPLabel();
            this.ProLogicII = new MediaPortal.UserInterface.Controls.MPLabel();
            this.comboBoxProLogicII = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.comboBoxDRC = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.comboBoxSpeakerSetup = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.comboBoxHeadphones = new MediaPortal.UserInterface.Controls.MPComboBox();
            this.SpeakerSetup = new MediaPortal.UserInterface.Controls.MPGroupBox();
            this.radioButtonReceiver = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.radioButtonHeadphones = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.radioButtonSpeakers = new MediaPortal.UserInterface.Controls.MPRadioButton();
            this.VideoDecoderSettings.SuspendLayout();
            this.DisplayType.SuspendLayout();
            this.AudioDecoderSettings.SuspendLayout();
            this.SpeakerSetup.SuspendLayout();
            this.SuspendLayout();
            // 
            // VideoDecoderSettings
            // 
            this.VideoDecoderSettings.Controls.Add(this.DisplayType);
            this.VideoDecoderSettings.Controls.Add(this.DeinterlaceMode);
            this.VideoDecoderSettings.Controls.Add(this.comboBoxDeInterlaceMode);
            this.VideoDecoderSettings.Controls.Add(this.comboBoxDeInterlaceControl);
            this.VideoDecoderSettings.Controls.Add(this.DeinterlaceControl);
            this.VideoDecoderSettings.Controls.Add(this.checkBoxDxVA);
            this.VideoDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.VideoDecoderSettings.Location = new System.Drawing.Point(0, 14);
            this.VideoDecoderSettings.Name = "VideoDecoderSettings";
            this.VideoDecoderSettings.Size = new System.Drawing.Size(472, 161);
            this.VideoDecoderSettings.TabIndex = 3;
            this.VideoDecoderSettings.TabStop = false;
            this.VideoDecoderSettings.Text = "Video Decoder Settings";
            // 
            // DisplayType
            // 
            this.DisplayType.Controls.Add(this.radioButtonDTAnamorphic);
            this.DisplayType.Controls.Add(this.radioButtonDTPan);
            this.DisplayType.Controls.Add(this.radioButtonDTLetterbox);
            this.DisplayType.Controls.Add(this.radioButtonDTDefault);
            this.DisplayType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.DisplayType.Location = new System.Drawing.Point(18, 23);
            this.DisplayType.Name = "DisplayType";
            this.DisplayType.Size = new System.Drawing.Size(217, 120);
            this.DisplayType.TabIndex = 6;
            this.DisplayType.TabStop = false;
            this.DisplayType.Text = "Display Type";
            // 
            // radioButtonDTAnamorphic
            // 
            this.radioButtonDTAnamorphic.AutoSize = true;
            this.radioButtonDTAnamorphic.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonDTAnamorphic.Location = new System.Drawing.Point(20, 91);
            this.radioButtonDTAnamorphic.Name = "radioButtonDTAnamorphic";
            this.radioButtonDTAnamorphic.Size = new System.Drawing.Size(149, 17);
            this.radioButtonDTAnamorphic.TabIndex = 3;
            this.radioButtonDTAnamorphic.TabStop = true;
            this.radioButtonDTAnamorphic.Text = "Anamorphic / Raw Aspect";
            this.radioButtonDTAnamorphic.UseVisualStyleBackColor = true;
            // 
            // radioButtonDTPan
            // 
            this.radioButtonDTPan.AutoSize = true;
            this.radioButtonDTPan.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonDTPan.Location = new System.Drawing.Point(20, 69);
            this.radioButtonDTPan.Name = "radioButtonDTPan";
            this.radioButtonDTPan.Size = new System.Drawing.Size(92, 17);
            this.radioButtonDTPan.TabIndex = 2;
            this.radioButtonDTPan.TabStop = true;
            this.radioButtonDTPan.Text = "Pan and Scan";
            this.radioButtonDTPan.UseVisualStyleBackColor = true;
            // 
            // radioButtonDTLetterbox
            // 
            this.radioButtonDTLetterbox.AutoSize = true;
            this.radioButtonDTLetterbox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonDTLetterbox.Location = new System.Drawing.Point(20, 45);
            this.radioButtonDTLetterbox.Name = "radioButtonDTLetterbox";
            this.radioButtonDTLetterbox.Size = new System.Drawing.Size(68, 17);
            this.radioButtonDTLetterbox.TabIndex = 1;
            this.radioButtonDTLetterbox.TabStop = true;
            this.radioButtonDTLetterbox.Text = "Letterbox";
            this.radioButtonDTLetterbox.UseVisualStyleBackColor = true;
            // 
            // radioButtonDTDefault
            // 
            this.radioButtonDTDefault.AutoSize = true;
            this.radioButtonDTDefault.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonDTDefault.Location = new System.Drawing.Point(20, 21);
            this.radioButtonDTDefault.Name = "radioButtonDTDefault";
            this.radioButtonDTDefault.Size = new System.Drawing.Size(96, 17);
            this.radioButtonDTDefault.TabIndex = 0;
            this.radioButtonDTDefault.TabStop = true;
            this.radioButtonDTDefault.Text = "Content default";
            this.radioButtonDTDefault.UseVisualStyleBackColor = true;
            // 
            // DeinterlaceMode
            // 
            this.DeinterlaceMode.AutoSize = true;
            this.DeinterlaceMode.Location = new System.Drawing.Point(253, 103);
            this.DeinterlaceMode.Name = "DeinterlaceMode";
            this.DeinterlaceMode.Size = new System.Drawing.Size(95, 13);
            this.DeinterlaceMode.TabIndex = 5;
            this.DeinterlaceMode.Text = "De-Interlace Mode";
            // 
            // comboBoxDeInterlaceMode
            // 
            this.comboBoxDeInterlaceMode.FormattingEnabled = true;
            this.comboBoxDeInterlaceMode.Items.AddRange(new object[] {
            "VMR default",
            "VMR pixel adaptive",
            "VMR vertical stretch"});
            this.comboBoxDeInterlaceMode.Location = new System.Drawing.Point(256, 122);
            this.comboBoxDeInterlaceMode.Name = "comboBoxDeInterlaceMode";
            this.comboBoxDeInterlaceMode.Size = new System.Drawing.Size(198, 21);
            this.comboBoxDeInterlaceMode.TabIndex = 4;
            // 
            // comboBoxDeInterlaceControl
            // 
            this.comboBoxDeInterlaceControl.FormattingEnabled = true;
            this.comboBoxDeInterlaceControl.Items.AddRange(new object[] {
            "Automatic",
            "Film",
            "Video",
            "Smart"});
            this.comboBoxDeInterlaceControl.Location = new System.Drawing.Point(256, 72);
            this.comboBoxDeInterlaceControl.Name = "comboBoxDeInterlaceControl";
            this.comboBoxDeInterlaceControl.Size = new System.Drawing.Size(198, 21);
            this.comboBoxDeInterlaceControl.TabIndex = 3;
            // 
            // DeinterlaceControl
            // 
            this.DeinterlaceControl.AutoSize = true;
            this.DeinterlaceControl.Location = new System.Drawing.Point(253, 53);
            this.DeinterlaceControl.Name = "DeinterlaceControl";
            this.DeinterlaceControl.Size = new System.Drawing.Size(101, 13);
            this.DeinterlaceControl.TabIndex = 2;
            this.DeinterlaceControl.Text = "De-Interlace Control";
            // 
            // checkBoxDxVA
            // 
            this.checkBoxDxVA.AutoSize = true;
            this.checkBoxDxVA.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBoxDxVA.Location = new System.Drawing.Point(256, 23);
            this.checkBoxDxVA.Name = "checkBoxDxVA";
            this.checkBoxDxVA.Size = new System.Drawing.Size(149, 17);
            this.checkBoxDxVA.TabIndex = 0;
            this.checkBoxDxVA.Text = "Use Hardware Accelerator";
            this.checkBoxDxVA.UseVisualStyleBackColor = true;
            // 
            // AudioDecoderSettings
            // 
            this.AudioDecoderSettings.Controls.Add(this.comboBoxOutPutMode);
            this.AudioDecoderSettings.Controls.Add(this.DynamicRangeControl);
            this.AudioDecoderSettings.Controls.Add(this.ProLogicII);
            this.AudioDecoderSettings.Controls.Add(this.comboBoxProLogicII);
            this.AudioDecoderSettings.Controls.Add(this.comboBoxDRC);
            this.AudioDecoderSettings.Controls.Add(this.comboBoxSpeakerSetup);
            this.AudioDecoderSettings.Controls.Add(this.comboBoxHeadphones);
            this.AudioDecoderSettings.Controls.Add(this.SpeakerSetup);
            this.AudioDecoderSettings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AudioDecoderSettings.Location = new System.Drawing.Point(0, 185);
            this.AudioDecoderSettings.Name = "AudioDecoderSettings";
            this.AudioDecoderSettings.Size = new System.Drawing.Size(472, 207);
            this.AudioDecoderSettings.TabIndex = 2;
            this.AudioDecoderSettings.TabStop = false;
            this.AudioDecoderSettings.Text = "Audio Decoder Settings";
            // 
            // comboBoxOutPutMode
            // 
            this.comboBoxOutPutMode.FormattingEnabled = true;
            this.comboBoxOutPutMode.Items.AddRange(new object[] {
            "SPDIF Mode",
            "Pro Logic Mode"});
            this.comboBoxOutPutMode.Location = new System.Drawing.Point(214, 104);
            this.comboBoxOutPutMode.Name = "comboBoxOutPutMode";
            this.comboBoxOutPutMode.Size = new System.Drawing.Size(183, 21);
            this.comboBoxOutPutMode.TabIndex = 12;
            this.comboBoxOutPutMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxOutPutMode_SelectedIndexChanged);
            // 
            // DynamicRangeControl
            // 
            this.DynamicRangeControl.AutoSize = true;
            this.DynamicRangeControl.Location = new System.Drawing.Point(243, 146);
            this.DynamicRangeControl.Name = "DynamicRangeControl";
            this.DynamicRangeControl.Size = new System.Drawing.Size(119, 13);
            this.DynamicRangeControl.TabIndex = 10;
            this.DynamicRangeControl.Text = "Dynamic Range Control";
            // 
            // ProLogicII
            // 
            this.ProLogicII.AutoSize = true;
            this.ProLogicII.Location = new System.Drawing.Point(22, 147);
            this.ProLogicII.Name = "ProLogicII";
            this.ProLogicII.Size = new System.Drawing.Size(61, 13);
            this.ProLogicII.TabIndex = 8;
            this.ProLogicII.Text = "Pro Logic II";
            // 
            // comboBoxProLogicII
            // 
            this.comboBoxProLogicII.FormattingEnabled = true;
            this.comboBoxProLogicII.Items.AddRange(new object[] {
            "Off",
            "Pro Logic",
            "Music Mode",
            "Movie Mode",
            "Matrix Mode"});
            this.comboBoxProLogicII.Location = new System.Drawing.Point(38, 167);
            this.comboBoxProLogicII.Name = "comboBoxProLogicII";
            this.comboBoxProLogicII.Size = new System.Drawing.Size(183, 21);
            this.comboBoxProLogicII.TabIndex = 7;
            // 
            // comboBoxDRC
            // 
            this.comboBoxDRC.FormattingEnabled = true;
            this.comboBoxDRC.Items.AddRange(new object[] {
            "Normal",
            "Late Night",
            "Theatre"});
            this.comboBoxDRC.Location = new System.Drawing.Point(262, 167);
            this.comboBoxDRC.Name = "comboBoxDRC";
            this.comboBoxDRC.Size = new System.Drawing.Size(192, 21);
            this.comboBoxDRC.TabIndex = 4;
            // 
            // comboBoxSpeakerSetup
            // 
            this.comboBoxSpeakerSetup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSpeakerSetup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpeakerSetup.Items.AddRange(new object[] {
            "Mono",
            "Stereo",
            "3 Speakers (2.1)",
            "4 Speakers",
            "5 Speakers",
            "6 Speakers (6.1)"});
            this.comboBoxSpeakerSetup.Location = new System.Drawing.Point(214, 46);
            this.comboBoxSpeakerSetup.Name = "comboBoxSpeakerSetup";
            this.comboBoxSpeakerSetup.Size = new System.Drawing.Size(183, 21);
            this.comboBoxSpeakerSetup.TabIndex = 1;
            this.comboBoxSpeakerSetup.SelectedIndexChanged += new System.EventHandler(this.comboBoxSpeakerSetup_SelectedIndexChanged);
            // 
            // comboBoxHeadphones
            // 
            this.comboBoxHeadphones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxHeadphones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHeadphones.DropDownWidth = 183;
            this.comboBoxHeadphones.Items.AddRange(new object[] {
            "None",
            "Dolby Headphone 1",
            "Dolby Headphone 2",
            "Dolby Headphone 3"});
            this.comboBoxHeadphones.Location = new System.Drawing.Point(214, 76);
            this.comboBoxHeadphones.Name = "comboBoxHeadphones";
            this.comboBoxHeadphones.Size = new System.Drawing.Size(183, 21);
            this.comboBoxHeadphones.TabIndex = 0;
            // 
            // SpeakerSetup
            // 
            this.SpeakerSetup.Controls.Add(this.radioButtonReceiver);
            this.SpeakerSetup.Controls.Add(this.radioButtonHeadphones);
            this.SpeakerSetup.Controls.Add(this.radioButtonSpeakers);
            this.SpeakerSetup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SpeakerSetup.Location = new System.Drawing.Point(18, 19);
            this.SpeakerSetup.Name = "SpeakerSetup";
            this.SpeakerSetup.Size = new System.Drawing.Size(436, 114);
            this.SpeakerSetup.TabIndex = 13;
            this.SpeakerSetup.TabStop = false;
            this.SpeakerSetup.Text = "SpeakerSetup";
            // 
            // radioButtonReceiver
            // 
            this.radioButtonReceiver.AutoSize = true;
            this.radioButtonReceiver.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonReceiver.Location = new System.Drawing.Point(20, 86);
            this.radioButtonReceiver.Name = "radioButtonReceiver";
            this.radioButtonReceiver.Size = new System.Drawing.Size(108, 17);
            this.radioButtonReceiver.TabIndex = 11;
            this.radioButtonReceiver.TabStop = true;
            this.radioButtonReceiver.Text = "External Receiver";
            this.radioButtonReceiver.UseVisualStyleBackColor = true;
            this.radioButtonReceiver.CheckedChanged += new System.EventHandler(this.radioButtonReceiver_CheckedChanged);
            // 
            // radioButtonHeadphones
            // 
            this.radioButtonHeadphones.AutoSize = true;
            this.radioButtonHeadphones.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonHeadphones.Location = new System.Drawing.Point(20, 58);
            this.radioButtonHeadphones.Name = "radioButtonHeadphones";
            this.radioButtonHeadphones.Size = new System.Drawing.Size(85, 17);
            this.radioButtonHeadphones.TabIndex = 9;
            this.radioButtonHeadphones.TabStop = true;
            this.radioButtonHeadphones.Text = "Headphones";
            this.radioButtonHeadphones.UseVisualStyleBackColor = true;
            this.radioButtonHeadphones.CheckedChanged += new System.EventHandler(this.radioButtonHeadphones_CheckedChanged);
            // 
            // radioButtonSpeakers
            // 
            this.radioButtonSpeakers.AutoSize = true;
            this.radioButtonSpeakers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.radioButtonSpeakers.Location = new System.Drawing.Point(20, 31);
            this.radioButtonSpeakers.Name = "radioButtonSpeakers";
            this.radioButtonSpeakers.Size = new System.Drawing.Size(117, 17);
            this.radioButtonSpeakers.TabIndex = 6;
            this.radioButtonSpeakers.TabStop = true;
            this.radioButtonSpeakers.Text = "Computer Speakers";
            this.radioButtonSpeakers.UseVisualStyleBackColor = true;
            this.radioButtonSpeakers.CheckedChanged += new System.EventHandler(this.radioButtonSpeakers_CheckedChanged);
            // 
            // PureVideoDecoderFilters
            // 
            this.Controls.Add(this.VideoDecoderSettings);
            this.Controls.Add(this.AudioDecoderSettings);
            this.Name = "PureVideoDecoderFilters";
            this.Size = new System.Drawing.Size(472, 408);
            this.VideoDecoderSettings.ResumeLayout(false);
            this.VideoDecoderSettings.PerformLayout();
            this.DisplayType.ResumeLayout(false);
            this.DisplayType.PerformLayout();
            this.AudioDecoderSettings.ResumeLayout(false);
            this.AudioDecoderSettings.PerformLayout();
            this.SpeakerSetup.ResumeLayout(false);
            this.SpeakerSetup.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion


        public override void LoadSettings()
        {
            #region Video Settings
            int iIndex = 0;
            Int32 regValue = 0;

            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey subkey = hklm.CreateSubKey(@"Software\NVIDIA Corporation\Filters\Video");

            if (subkey != null)
            {
                try
                {
                    regValue = (Int32)subkey.GetValue("DisplayType", 0);
                    radioButtonDTDefault.Checked = (regValue == 0);
                    radioButtonDTLetterbox.Checked = (regValue == 1);
                    radioButtonDTPan.Checked = (regValue == 2);
                    radioButtonDTAnamorphic.Checked = (regValue == 3);

                    regValue = (Int32)subkey.GetValue("EnableDXVA", 1);
                    if (regValue == 0) checkBoxDxVA.Checked = false;
                    if (regValue == 1) checkBoxDxVA.Checked = true;

                    regValue = (Int32)subkey.GetValue("DeinterlaceControl", 3);
                    comboBoxDeInterlaceControl.SelectedIndex = regValue;

                    regValue = (Int32)subkey.GetValue("DeinterlaceMode", 0);
                    Int32 regAdaptive = (Int32)subkey.GetValue("VMRDeinterlace", 2);
                    if (regValue >= 0)
                    {
                        if (regValue == 0) comboBoxDeInterlaceMode.SelectedIndex = 0;
                        if (regValue == 5 && regAdaptive == 40) comboBoxDeInterlaceMode.SelectedIndex = 1;
                        if (regValue == 5 && regAdaptive == 2) comboBoxDeInterlaceMode.SelectedIndex = 2;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    subkey.Close();
                }
            #endregion
            }

            #region Audio Settings
            Int32 regMaxOut = 0;
            Int32 regAC3Output = 0;
            Int32 regMonoOutput = 0;
            Int32 regCenterPresent = 0;
            Int32 regBackPresent = 0;
            Int32 regSubwooferPresent = 0;
            Int32 regProLogic2Mode = 0;
            Int32 regDHProp = 0;
            Int32 regSPDIF = 0;

            RegistryKey subkey2 = hklm.CreateSubKey(@"Software\NVIDIA Corporation\Filters\Audio");
            {
                regValue = (Int32)subkey2.GetValue("ConnectedDevicePropControl", 0);
                regMaxOut = (Int32)subkey2.GetValue("MaxOutChannels", 2);
                regAC3Output = (Int32)subkey2.GetValue("AC3OutputMode", -1);
                regMonoOutput = (Int32)subkey2.GetValue("MonoOutput", -1);
                regCenterPresent = (Int32)subkey2.GetValue("CenterPresent", -1);
                regBackPresent = (Int32)subkey2.GetValue("BackPresent", -1);
                regSubwooferPresent = (Int32)subkey2.GetValue("SubwooferPresent", -1);
                regProLogic2Mode = (Int32)subkey2.GetValue("ProLogic2Mode", -1);
                regDHProp = (Int32)subkey2.GetValue("DHPPropControl", -1);
                regSPDIF = (Int32)subkey2.GetValue("EnableSPDIFPassThru", -1);
                }

            if (subkey2 != null)
            {
                try
                {
                    // Speakers Setup
                    switch (regValue)
                    {
                        // Computer Speakers
                        case 0:
                            #region Computer Speakers
                            radioButtonSpeakers.Checked = true;
                            comboBoxSpeakerSetup.Enabled = true;
                            if (regMonoOutput == 1) iIndex = 0;
                            if (regMaxOut == 2) iIndex = 1;
                            if (regMaxOut == 6 && regBackPresent == 0 && regSubwooferPresent == 1 && regCenterPresent == 0) iIndex = 2;
                            if (regMaxOut == 6 && regBackPresent == 1 && regSubwooferPresent == 0 && regCenterPresent == 0) iIndex = 3;
                            if (regMaxOut == 6 && regBackPresent == 1 && regSubwooferPresent == 0 && regCenterPresent == 1) iIndex = 4;
                            if (regMaxOut == 6 && regBackPresent == 1 && regSubwooferPresent == 1 && regCenterPresent == 1) iIndex = 5;
                            if (iIndex <= 1) comboBoxProLogicII.Enabled = false;
                            comboBoxSpeakerSetup.SelectedIndex = iIndex;
                            #endregion
                            break;

                        //Headphones                            
                        case 1:
                            #region Headphones
                            radioButtonHeadphones.Checked = true;
                            comboBoxHeadphones.Enabled = true;
                            comboBoxHeadphones.SelectedIndex = regDHProp;
                            #endregion
                            break;

                        // Receiver
                        case 2:
                            #region Receiver
                            radioButtonReceiver.Checked = true;
                            comboBoxOutPutMode.Enabled = true;
                            if (regSPDIF == 1) comboBoxOutPutMode.SelectedIndex = 0;
                            if (regSPDIF == 0) comboBoxOutPutMode.SelectedIndex = 1;
                            #endregion
                            break;
                    }
                    {
                        // Dynamic Range Control
                        Int32 regDRC = (Int32)subkey2.GetValue("AC3CompressionMode", 2);
                        Int32 regAC3DRH = (Int32)subkey2.GetValue("AC3DynamicRangeHigh", 10000);
                        if (regDRC == 2 && regAC3DRH == 10000) comboBoxDRC.SelectedIndex = 0;
                        if (regDRC == 3) comboBoxDRC.SelectedIndex = 1;
                        if (regDRC == 2 && regAC3DRH == 0) comboBoxDRC.SelectedIndex = 2;

                        // Pro Logic II
                        Int32 regPL2Mode = (Int32)subkey2.GetValue("ProLogic2Mode", 0);
                        comboBoxProLogicII.SelectedIndex = regPL2Mode;
                    }
                    subkey2.Close();
                }
                catch (Exception)
                {
                }
            #endregion
            }
        }

        public override void SaveSettings()
        {
            #region Video Settings
            Int32 regValue = 0;

            RegistryKey hklm = Registry.LocalMachine;
            RegistryKey subkey = hklm.CreateSubKey(@"Software\NVIDIA Corporation\Filters\Video");
            if (subkey != null)
            {
                // Display Type
                if (radioButtonDTDefault.Checked) regValue = 0;
                if (radioButtonDTLetterbox.Checked) regValue = 1;
                if (radioButtonDTPan.Checked) regValue = 2;
                if (radioButtonDTAnamorphic.Checked) regValue = 3;
                //else regValue = 0;
                subkey.SetValue("DisplayType", regValue);

                // Hardware Acceleration
                if (checkBoxDxVA.Checked) regValue = 1;
                else regValue = 0;
                subkey.SetValue("EnableDXVA", regValue);

                // De-Interlace Control
                regValue = comboBoxDeInterlaceControl.SelectedIndex;
                subkey.SetValue("DeinterlaceControl", regValue);
                
                // We force VMR9 on to suit MediaPortal
                subkey.SetValue("EnableVMR", 2);

                // De-Interlace Mode
                int DeIntMode;
                Int32 regDeIntMode;
                DeIntMode = comboBoxDeInterlaceMode.SelectedIndex;
                if (DeIntMode == 0)
                {
                    regDeIntMode = 0;
                    subkey.SetValue("DeinterlaceMode", regDeIntMode);
                    subkey.SetValue("VMRDeinterlace", 2);
                }
                if (DeIntMode == 1)
                {
                    regDeIntMode = 5;
                    subkey.SetValue("DeinterlaceMode", regDeIntMode);
                    subkey.SetValue("VMRDeinterlace", 40);
                }
                if (DeIntMode == 2)
                {
                    regDeIntMode = 5;
                    subkey.SetValue("DeinterlaceMode", regDeIntMode);
                    subkey.SetValue("VMRDeinterlace", 2);
                }
                {
                }
                subkey.Close();
            #endregion
            }

            #region Audio Settings
            RegistryKey subkey2 = hklm.CreateSubKey(@"Software\NVIDIA Corporation\Filters\Audio");

            if (subkey2 != null)
            {
                // Computer Speakers
                if (radioButtonSpeakers.Checked) subkey2.SetValue("ConnectedDevicePropControl", 0);

                // Headphones
                if (radioButtonHeadphones.Checked) subkey2.SetValue("ConnectedDevicePropControl", 1);

                // External Receiver
                if (radioButtonReceiver.Checked) subkey2.SetValue("ConnectedDevicePropControl", 2);

                //Dynamic Range Control
                int DRC;
                Int32 regDRC;
                DRC = comboBoxDRC.SelectedIndex;
                if (DRC == 0)
                {
                    regDRC = 2;
                    subkey2.SetValue("AC3CompressionMode", regDRC);
                    regDRC = 10000;
                    subkey2.SetValue("AC3DynamicRangeHigh", regDRC);
                    subkey2.SetValue("AC3DynamicRangeLow", regDRC);
                }
                if (DRC == 1)
                {
                    regDRC = 3;
                    subkey2.SetValue("AC3CompressionMode", regDRC);
                    regDRC = 0;
                    subkey2.SetValue("AC3DynamicRangeHigh", regDRC);
                    subkey2.SetValue("AC3DynamicRangeLow", regDRC);
                }
                if (DRC == 2)
                {
                    regDRC = 2;
                    subkey2.SetValue("AC3CompressionMode", regDRC);
                    regDRC = 0;
                    subkey2.SetValue("AC3DynamicRangeHigh", regDRC);
                    subkey2.SetValue("AC3DynamicRangeLow", regDRC);
                }

                //Pro Logic II
                Int32 regProLogicII;
                regProLogicII = comboBoxProLogicII.SelectedIndex;
                subkey2.SetValue("ProLogic2Mode", regProLogicII);

                // Speaker Setup
                switch (comboBoxSpeakerSetup.SelectedIndex)
                {
                    //Mono
                    case 0:
                        #region Mono
                        regValue = 1;
                        subkey2.SetValue("MonoOutput", regValue);
                        subkey2.SetValue("MonoPropControl", regValue);
                        regValue = 2;
                        subkey2.SetValue("NoiseTarget", regValue);
                        #endregion
                        break;

                    // Stereo
                    case 1:
                        #region Stereo
                        regValue = 0;
                        subkey2.SetValue("CenterPresent", regValue);
                        subkey2.SetValue("BackPresent", regValue);
                        subkey2.SetValue("SubwooferPresent", regValue);
                        subkey2.SetValue("MonoOutput", regValue);
                        subkey2.SetValue("MonoPropControl", regValue);
                        subkey2.SetValue("NoiseTarget", regValue);
                        subkey2.SetValue("AC3LfeOn", regValue);
                        subkey2.SetValue("AC3OutputMode", regValue);
                        regValue = 1;
                        subkey2.SetValue("FrontSpeakerSize", regValue);
                        regValue = 2;
                        subkey2.SetValue("MaxOutChannels", regValue);
                        #endregion
                        break;

                    // 3 Speakers (2.1)
                    case 2:
                        #region 3 Speakers (2.1)
                        regValue = 6;
                        subkey2.SetValue("MaxOutChannels", regValue);
                        regValue = 0;
                        subkey2.SetValue("CenterPresent", regValue);
                        subkey2.SetValue("BackPresent", regValue);
                        subkey2.SetValue("MonoOutput", regValue);
                        subkey2.SetValue("MonoPropControl", regValue);
                        subkey2.SetValue("NoiseTarget", regValue);
                        subkey2.SetValue("FrontSpeakerSize", regValue);
                        regValue = 1;
                        subkey2.SetValue("SubwooferPresent", regValue);
                        subkey2.SetValue("AC3LfeOn", regValue);
                        regValue = 2;
                        subkey2.SetValue("AC3OutputMode", regValue);
                        subkey2.SetValue("CenterSpeakerSize", regValue);
                        subkey2.SetValue("RearSpeakerSize", regValue);
                        #endregion
                        break;

                    // 4 Speakers
                    case 3:
                        #region 4 Speakers
                        regValue = 6;
                        subkey2.SetValue("MaxOutChannels", regValue);
                        subkey2.SetValue("AC3OutputMode", regValue);
                        regValue = 0;
                        subkey2.SetValue("CenterPresent", regValue);
                        subkey2.SetValue("SubwooferPresent", regValue);
                        subkey2.SetValue("MonoOutput", regValue);
                        subkey2.SetValue("MonoPropControl", regValue);
                        subkey2.SetValue("NoiseTarget", regValue);
                        subkey2.SetValue("AC3LfeOn", regValue);
                        regValue = 1;
                        subkey2.SetValue("FrontSpeakerSize", regValue);
                        subkey2.SetValue("BackPresent", regValue);
                        subkey2.SetValue("RearSpeakerSize", regValue);
                        regValue = 2;
                        subkey2.SetValue("CenterSpeakerSize", regValue);
                        #endregion
                        break;

                    // 5 Speakers
                    case 4:
                        #region 5 Speakers
                        regValue = 6;
                        subkey2.SetValue("MaxOutChannels", regValue);
                        regValue = 7;
                        subkey2.SetValue("AC3OuputMode", regValue);
                        regValue = 1;
                        subkey2.SetValue("CenterPresent", regValue);
                        subkey2.SetValue("CenterSpeakerSize", regValue);
                        subkey2.SetValue("BackPresent", regValue);
                        subkey2.SetValue("RearSpeakerSize", regValue);
                        subkey2.SetValue("FrontSpeakerSize", regValue);
                        regValue = 0;
                        subkey2.SetValue("MonoOutput", regValue);
                        subkey2.SetValue("MonoPropControl", regValue);
                        subkey2.SetValue("NoiseTarget", regValue);
                        subkey2.SetValue("SubwooferPresent", regValue);
                        subkey2.SetValue("AC3LfeOn", regValue);
                        #endregion
                        break;

                    // 6 Speakers (5.1)
                    case 5:
                        #region 6 Speakers (5.1)
                        regValue = 6;
                        subkey2.SetValue("MaxOutChannels", regValue);
                        regValue = 7;
                        subkey2.SetValue("AC3OuputMode", regValue);
                        regValue = 1;
                        subkey2.SetValue("CenterPresent", regValue);
                        subkey2.SetValue("BackPresent", regValue);
                        subkey2.SetValue("SubwooferPresent", regValue);
                        subkey2.SetValue("AC3LfeOn", regValue);
                        regValue = 0;
                        subkey2.SetValue("MonoOutput", regValue);
                        subkey2.SetValue("MonoPropControl", regValue);
                        subkey2.SetValue("NoiseTarget", regValue);
                        subkey2.SetValue("CenterSpeakerSize", regValue);
                        subkey2.SetValue("FrontSpeakerSize", regValue);
                        subkey2.SetValue("RearSpeakerSize", regValue);
                        #endregion
                        break;
                }
                    
                // Headphones
                regValue = comboBoxHeadphones.SelectedIndex;
                if (regValue >= 0)
                {
                    subkey2.SetValue("DHPPropControl", regValue);
                    subkey2.SetValue("DolbyHeadphoneMode", regValue);
                }

                //Receiver
                int Receiver;
                Receiver = comboBoxOutPutMode.SelectedIndex;
                if (Receiver == 0)
                {
                    subkey2.SetValue("EnableSPDIFPassThru", 1);
                    subkey2.SetValue("SPDIFPropControl", 1);
                    subkey2.SetValue("SurroundPropControl", 0);
                }
                if (Receiver == 1)
                {
                    subkey2.SetValue("EnableSPDIFPassThru", 0);
                    subkey2.SetValue("SPDIFPropControl", 0);
                    subkey2.SetValue("SurroundPropControl", 1);
                }
                subkey2.Close();
            #endregion
            }
        }

        private void comboBoxSpeakerSetup_SelectedIndexChanged(object sender, EventArgs e)
        {
            int SpeakerChoice;
            SpeakerChoice = (comboBoxSpeakerSetup.SelectedIndex);
            if (SpeakerChoice >= 2) comboBoxProLogicII.Enabled = true;
            else comboBoxProLogicII.Enabled = false;
        }

        private void radioButtonReceiver_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxOutPutMode.Enabled = true;
            comboBoxHeadphones.Enabled = false;
            comboBoxSpeakerSetup.Enabled = false;
            comboBoxProLogicII.Enabled = false;
        }

        private void comboBoxOutPutMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int Receiver;
            Receiver = comboBoxOutPutMode.SelectedIndex;
            if (Receiver == 0)
            {
                comboBoxDRC.Enabled = false;
                comboBoxProLogicII.Enabled = false;
            }
            if (Receiver == 1)
            {
                comboBoxDRC.Enabled = true;
                comboBoxProLogicII.Enabled = true;
            }
        }

        private void radioButtonSpeakers_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxOutPutMode.Enabled = false;
            comboBoxHeadphones.Enabled = false;
            comboBoxSpeakerSetup.Enabled = true;
            comboBoxDRC.Enabled = true;
            if (comboBoxSpeakerSetup.SelectedIndex >= 2) comboBoxProLogicII.Enabled = true;
        }

        private void radioButtonHeadphones_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxOutPutMode.Enabled = false;
            comboBoxHeadphones.Enabled = true;
            comboBoxSpeakerSetup.Enabled = false;
            comboBoxProLogicII.Enabled = false;
            comboBoxDRC.Enabled = true;
        }
    }
}