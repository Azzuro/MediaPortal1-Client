using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;


using Mpe.Controls.Properties;

namespace Mpe.Controls {
	
	[DefaultPropertyAttribute("Font")]
	public class MpeFontViewer : MpeControl {
		#region Variables
		// Editor settings
		private MpeFont mpeFont;
		private FileInfo backImageFile;
		private Bitmap backImageTexture;
		private bool showBackground;
		private SolidBrush selectedBrush;
		private SolidBrush hoverBrush;
		private bool showBorders;
		private int selectedIndex;
		private int hoverIndex;
		#endregion

		#region Constructors
		public MpeFontViewer(MpeFont font, FileInfo background) : base() {
			MpeLog.Debug("MpeFontViewer()");
			id = 1;
			controlLock.Location = true;
			controlLock.Size = true;
			mpeFont = font;
			showBackground = true;
			showBorders = false;
			selectedBrush = new SolidBrush(Color.DarkGray);
			hoverBrush = new SolidBrush(Color.Gray);
			borderPen = new Pen(Color.Lime,1.0f);
			selectedIndex = -1;
			hoverIndex = -1;
			BackImageFile = background;
			Prepare();
			SelectedIndex = 0;
			Modified = false;
		}
		#endregion
	
		#region Events and Delegates
		public delegate void SelectedIndexChangedHandler(int oldIndex, int newIndex);
		public event SelectedIndexChangedHandler SelectedIndexChanged;
		#endregion

		#region Properties
		[Category("Font"),
		Browsable(true),
		ReadOnly(false),
		RefreshProperties(RefreshProperties.Repaint)]
		public override Font Font {
			get {
				return mpeFont.SystemFont;
			}
			set {
				SelectedIndex = 0;
				mpeFont.SystemFont = value;
				Modified = true;
				FirePropertyValueChanged("Font");
				Prepare();
			}
		}
		[Category("CharacterSet"),
		TypeConverter(typeof(MpeFontCharacterConverter)),
		RefreshProperties(RefreshProperties.Repaint)]
		public char First {
			get {
				return (char)mpeFont.StartCharacter;
			}
			set {
				SelectedIndex = 0;
				mpeFont.StartCharacter = (int)value;
				MpeLog.Info("FirstCharacter changed to [" + value + "]");
				Modified = true;
				FirePropertyValueChanged("FirstCharacter");
				Prepare();
			}
		}
		[Category("CharacterSet"),
		TypeConverter(typeof(MpeFontCharacterConverter)),
		RefreshProperties(RefreshProperties.Repaint)]
		public char Last {
			get {
				return (char)(mpeFont.EndCharacter - 1);
			}
			set {
				SelectedIndex = 0;
				mpeFont.EndCharacter = (int)value + 1;
				MpeLog.Info("LastCharacter changed to [" + value + "]");
				Modified = true;
				FirePropertyValueChanged("LastCharacter");
				Prepare();
			}
		}
		[Browsable(false)]
		public FileInfo BackImageFile {
			get {
				return backImageFile;
			}
			set {
				if (value != null && value.Exists) {
					backImageFile = value;
					backImageTexture = new Bitmap(backImageFile.FullName);
				} else {
					backImageFile = null;
					backImageTexture = null;
				}
			}
		}
		[Category("Designer"),DefaultValue(true)]
		public bool ShowBackground {
			get {
				return showBackground;
			}
			set {
				showBackground = value;
				Invalidate(false);
			}
		}
		[Category("Designer")]
		public Color BorderColor {
			get {
				return borderPen.Color;
			}
			set {
				borderPen.Color = value;
				Invalidate(false);
			}
		}
		[Category("Designer")]
		public bool ShowBorders {
			get {
				return showBorders;
			}
			set {
				if (value != showBorders) {
					showBorders = value;
					Invalidate(false);
				}
			}
		}
		[Category("Designer")]
		public Color HoverColor {
			get {
				return hoverBrush.Color;
			}
			set {
				hoverBrush.Color = value;
				Invalidate(false);
			}
		}
		[Category("Designer")]
		public Color SelectedColor {
			get {
				return selectedBrush.Color;
			}
			set {
				selectedBrush.Color = value;
				Invalidate(false);
			}
		}
		[Category("Textures")]
		public FileInfo TextureFile {
			get {
				return mpeFont.TextureFile;
			}
		}
		[Category("Textures")]
		public Image Texture {
			get {
				return mpeFont.Texture;
			}
		}
		[Category("Textures")]
		public FileInfo TextureDataFile {
			get {
				return mpeFont.TextureDataFile;
			}
		}
		[Browsable(false)]
		public Rectangle[] TextureData {
			get {
				return mpeFont.TextureData;
			}
		}
		[Browsable(false)]
		public override MpeControlLock Locked {
			get {
				return base.Locked;
			}
			set {
				base.Locked = value;
			}
		}
		[Browsable(false)]
		public int SelectedIndex {
			get {
				return selectedIndex;
			}
			set {
				if (selectedIndex != value && mpeFont != null) {
					int old = selectedIndex;
					selectedIndex = value;
					FireEvent(selectedIndex, value);
					Invalidate(false);
				}
			}
		}
		[Category("Designer"),ReadOnly(true)]
		public Rectangle SelectedRegion {
			get {
				if (SelectedIndex < 0)
					return Rectangle.Empty;
				return TextureData[SelectedIndex];
			} 
			set {
				TextureData[SelectedIndex] = value;
				Invalidate(false);
			}
		}
		#endregion

		#region Properties - Hidden
		[Browsable(false)]
		public override MpeControlType Type {
			get {
				return base.Type;
			}
			set {
				base.Type = value;
			}
		}
		[Browsable(false)]
		public override MpeTagCollection Tags {
			get {
				return base.Tags;
			}
			set {
				base.Tags = value;
			}
		}
		[Browsable(false)]
		public override MpeControlAlignment Alignment {
			get {
				return base.Alignment;
			}
			set {
				base.Alignment = value;
			}
		}
		[Browsable(false)]
		public override bool AutoSize {
			get {
				return base.AutoSize;
			}
			set {
				base.AutoSize = value;
			}
		}
		[Browsable(false)]
		public new Size Size {
			get {
				return base.Size;
			}
			set {
				base.Size = value;
			}
		}
		[Browsable(false)]
		public new Point Location {
			get {
				return base.Location;
			}
			set {
				base.Location = value;
			}
		}
		[Browsable(false)]
		public override int Id {
			get {
				return base.Id;
			}
			set {
				base.Id = value;
			}
		}
		[Browsable(false)]
		public override string Description {
			get {
				return base.Description;
			}
			set {
				base.Description = value;
			}
		}
		[Browsable(false)]
		public override bool Enabled {
			get {
				return base.Enabled;
			}
			set {
				base.Enabled = value;
			}
		}
		[Browsable(false)]
		public override bool Focused {
			get {
				return base.Focused;
			}
			set {
				base.Focused = value;
			}
		}
		[Browsable(false)]
		public override int OnLeft {
			get {
				return base.OnLeft;
			}
			set {
				base.OnLeft = value;
			}
		}
		[Browsable(false)]
		public override int OnRight {
			get {
				return base.OnRight;
			}
			set {
				base.OnRight = value;
			}
		}
		[Browsable(false)]
		public override int OnUp {
			get {
				return base.OnUp;
			}
			set {
				base.OnUp = value;
			}
		}
		[Browsable(false)]
		public override int OnDown {
			get {
				return base.OnDown;
			}
			set {
				base.OnDown = value;
			}
		}
		[Browsable(false)]
		public override bool Visible {
			get {
				return base.Visible;
			}
			set {
				base.Visible = value;
			}
		}
		[Browsable(false)]
		public override Color DiffuseColor {
			get {
				return base.DiffuseColor;
			}
			set {
				base.DiffuseColor = value;
			}
		}
		[Browsable(false)]
		public override MpeControlPadding Padding {
			get {
				return base.Padding;
			}
			set {
				base.Padding = value;
			}
		}
		#endregion

		#region Methods
		protected void FireEvent(int oldIndex, int newIndex) {
			if (SelectedIndexChanged != null) {
				SelectedIndexChanged(oldIndex, newIndex);
			}
		}
		protected override void PrepareControl() {
			base.PrepareControl();
			if (mpeFont != null && mpeFont.Texture != null) {
				Size = Texture.Size;
			} else {
				Size = new Size(128,128);
			}
			Invalidate(true);
		}
		public override void Destroy() {
			base.Destroy();
			if (mpeFont != null)
				mpeFont.Destroy();
		}

		public override void Load(System.Xml.XPath.XPathNodeIterator iterator, MpeParser parser) {
			//
		}
		public override void Save(System.Xml.XmlDocument doc, System.Xml.XmlNode node, MpeParser parser, MpeControl reference) {
			//
		}
		#endregion

		#region Event Handlers
		protected override void OnMouseMove(MouseEventArgs e) {
			for (int i = 0; TextureData != null && i < TextureData.Length; i++) {
				if (TextureData[i].Contains(e.X,e.Y)) {
					hoverIndex = i;
					Invalidate(false);
					return;
				}
			}
			hoverIndex = -1;
			Invalidate(false);
		}
		protected override void OnMouseUp(MouseEventArgs e) {
			SelectedIndex = hoverIndex;
		}
		protected override void OnClick(EventArgs e) {
			//
		}
		protected override void OnMouseLeave(EventArgs e) {
			hoverIndex = -1;
			Invalidate(false);
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (backImageTexture != null && showBackground) {
				e.Graphics.DrawImage(backImageTexture,0,0,Width,Height);
			}
			if (mpeFont != null && mpeFont.Texture != null) {
				if (selectedIndex >= 0) {
					e.Graphics.FillRectangle(selectedBrush,mpeFont.TextureData[selectedIndex]);
				}
				if (hoverIndex >= 0) {
					e.Graphics.FillRectangle(hoverBrush,mpeFont.TextureData[hoverIndex]);
				}
				e.Graphics.DrawImage(mpeFont.Texture,0,0,Width,Height);
				if (showBorders && mpeFont.TextureData != null) {
					Rectangle[] d = mpeFont.TextureData;
					for (int i = 0; i < d.Length; i++) {
						e.Graphics.DrawRectangle(borderPen,d[i].Left,d[i].Top,d[i].Width-1,d[i].Height-1);
					}
				}
				
			}
		}
		
		#endregion
	}

	#region MpeFontCharacterConverter
	internal class MpeFontCharacterConverter : TypeConverter {
		
		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
			char[] list = new char[256-32];
			int index = 0;
			for (int i = 32; i < 256; i++) {
				list[index++] = (char)i;
			}
			return new StandardValuesCollection(list);
		}
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
			return true;
		}
		
		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType) {
			if( sourceType == typeof(string) )
				return true;
			else 
				return base.CanConvertFrom(context, sourceType);
		}
		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if( value.GetType() == typeof(string) && context.Instance is MpeControl ) {
				string s = (string)value;
				return (s.ToCharArray())[0];
			} else {
				return base.ConvertFrom(context, culture, value);
			}
		}
	}
	#endregion

}
