/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TvControl;


using Gentle.Common;
using Gentle.Framework;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class FormEditCard : Form
  {
    Card _card;
    public FormEditCard()
    {
      InitializeComponent();
    }

    public Card Card
    {
      get
      {
        return _card;
      }
      set
      {
        _card = value;
      }
    }

    private void FormEditCard_Load(object sender, EventArgs e)
    {
      numericUpDownDecryptLimit.Value = _card.DecryptLimit;
      checkBoxAllowEpgGrab.Checked = _card.GrabEPG;
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      _card.DecryptLimit = Convert.ToInt32(numericUpDownDecryptLimit.Value);
      _card.GrabEPG = checkBoxAllowEpgGrab.Checked;
      this.Close();
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}
