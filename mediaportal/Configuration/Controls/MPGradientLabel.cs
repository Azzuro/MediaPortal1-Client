using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

namespace MediaPortal.UserInterface.Controls
{
	/// <summary>
	/// Summary description for GradientLabel.
	/// </summary>
	[ToolboxBitmap(typeof(Label))]
	public class MPGradientLabel : System.Windows.Forms.UserControl
	{
		Color firstColor;
		Color lastColor;

		int paddingTop, paddingLeft;

		private System.Windows.Forms.Label workingLabel;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MPGradientLabel()
		{
			this.SetStyle(ControlStyles.DoubleBuffer,true);

			//
			// This call is required by the Windows.Forms Form Designer.
			//
			InitializeComponent();

			firstColor	= System.Drawing.SystemColors.InactiveCaption;
			lastColor	= System.Drawing.Color.White;

			workingLabel.Paint += new PaintEventHandler(workingLabel_Paint);
		}
		
		private void DrawBackground(Graphics graphics)
		{
			//
			// Create gradient brush
			//
			Brush gradientBrush = new LinearGradientBrush(workingLabel.ClientRectangle,
				firstColor,
				lastColor,
				LinearGradientMode.Horizontal);

			//
			// Draw brush
			//
			graphics.FillRectangle(gradientBrush, this.ClientRectangle);
			gradientBrush.Dispose();
		}

		private void DrawForeground(Graphics graphics)
		{
			//
			// Draw bevelbox
			//
			System.Drawing.Pen grayPen = new Pen(Color.FromArgb(200, 200, 200));
			graphics.DrawLine(grayPen, 0, 0, this.Width - 1, 0);
			graphics.DrawLine(System.Drawing.Pens.WhiteSmoke, 0, this.Height - 1, this.Width - 1, this.Height - 1);
			grayPen.Dispose();

			//
			// Draw caption
			//
			graphics.DrawString(Caption,
								TextFont,
								new SolidBrush(TextColor),
								paddingLeft, paddingTop);
		}

		[Browsable(true), Category("Gradient")]
		public Color FirstColor
		{
			get { return firstColor; }
			set { firstColor = value; 
				workingLabel.Invalidate();
			}
		}

		[Browsable(true), Category("Gradient")]
		public Color LastColor
		{
			get { return lastColor; }
			set { lastColor = value; 
				workingLabel.Invalidate();
			}
		}

		[Browsable(true), Category("Gradient")]
		public string Caption
		{
			get { return workingLabel.Text; }
			set { workingLabel.Text = value; 
				workingLabel.Invalidate();
			}
		}

		[Browsable(true), Category("Gradient")]
		public Color TextColor
		{
			get { return workingLabel.ForeColor; }
			set { workingLabel.ForeColor = value; 
				workingLabel.Invalidate();
			}
		}

		[Browsable(true), Category("Gradient")]
		public Font TextFont
		{
			get { return workingLabel.Font; }
			set { workingLabel.Font = value; 
				workingLabel.Invalidate();
			}
		}

		[Browsable(true), Category("Gradient"), DefaultValue(0)]
		public int PaddingTop
		{
			get { return paddingTop; }
			set { paddingTop = value; 
				workingLabel.Invalidate();
			}
		}

		[Browsable(true), Category("Gradient"), DefaultValue(0)]
		public int PaddingLeft
		{
			get { return paddingLeft; }
			set { paddingLeft = value; 
				workingLabel.Invalidate();
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.workingLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// workingLabel
			// 
			this.workingLabel.BackColor = System.Drawing.SystemColors.Control;
			this.workingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workingLabel.Location = new System.Drawing.Point(0, 0);
			this.workingLabel.Name = "workingLabel";
			this.workingLabel.Size = new System.Drawing.Size(150, 150);
			this.workingLabel.TabIndex = 0;
			// 
			// GradientLabel
			// 
			this.Controls.Add(this.workingLabel);
			this.Name = "GradientLabel";
			this.ResumeLayout(false);

		}
		#endregion

		private void workingLabel_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(System.Drawing.SystemColors.Control);

			DrawBackground(e.Graphics);
			DrawForeground(e.Graphics);
		}
	}
}
