#region Copyright (C) 2007-2008 Team MediaPortal

/*
    Copyright (C) 2007-2008 Team MediaPortal
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

using MediaPortal.SkinEngine.Commands;

namespace MediaPortal.SkinEngine.MarkupExtensions
{

  /// <summary>
  /// Implements the MPF CommandStencil markup extension.
  /// </summary>
  public class CommandStencilMarkupExtension : CommandBaseMarkupExtension, ICommandStencil
  {
    #region Ctor

    public CommandStencilMarkupExtension(): base()
    { }

    public CommandStencilMarkupExtension(string path): base(path)
    { }

    #endregion

    #region Protected properties and methods

    protected override string CommandTypeName
    {
      get { return "CommandStencil"; }
    }

    #endregion

    #region ICommandStencil implementation

    // method Execute(IList<object> parameters) inherited from CommandBaseMarkupExtension

    #endregion
  }
}
