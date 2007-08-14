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
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.MPInstaller;

namespace MediaPortal.MPInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string fil = String.Empty;
            if (args.Length > 0)
                fil = args[0];
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
          
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\MediaPortal.exe"))
            {
                if (!String.IsNullOrEmpty(fil))
                {
                    if (Path.GetExtension(fil) == ".mpi")
                    {
                        wizard_1 wiz = new wizard_1();
                        wiz.package.LoadFromFile(fil);
                        if (wiz.package.isValid)
                        {
                            wiz.starStep();
                        }
                        else
                            MessageBox.Show("Invalid package !");
                    }
                    if (Path.GetExtension(fil) == ".xmp")
                    {
                        Form1 create_dlg = new Form1(Path.GetFullPath(fil));
                        create_dlg.ShowDialog();
                    }
                }
                else
                    Application.Run(new start_form());
            }
            else
            {
                MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + @"\MediaPortal.exe not found. Program Exit.....");
            }
        }
    }
}