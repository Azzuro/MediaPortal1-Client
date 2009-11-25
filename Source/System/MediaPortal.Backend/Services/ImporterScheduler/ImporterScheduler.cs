#region Copyright (C) 2007-2009 Team MediaPortal

/*
    Copyright (C) 2007-2009 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal II

    MediaPortal II is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal II is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal II.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using MediaPortal.Backend.ImporterScheduler;
using MediaPortal.Core.General;
using MediaPortal.Core.MediaManagement;

namespace MediaPortal.Backend.Services.ImporterScheduler
{
  public class ImporterScheduler : IImporterScheduler
  {
    #region IImporterScheduler implementation

    public void InvalidatePath(SystemName nativeSystem, ResourcePath path)
    {
      weiter:
      - Alle Media-Items unter gegebener Location auf dirty setzen
      - Import einplanen (persistent - wenn Server beendet wird, Import fortsetzen (ist es notwendig, dass ImporterWorker_ und
        diese Klasse miteinander �ber jedes importierte Verzeichnis kommunizieren?)
    }

    #endregion
  }
}