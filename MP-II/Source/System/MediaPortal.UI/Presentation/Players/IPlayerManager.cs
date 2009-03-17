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

using System;

namespace MediaPortal.Presentation.Players
{
  public delegate void PlayerSlotWorkerDelegate(IPlayerSlotController slotController);

  /// <summary>
  /// Slot 0: Primary Video
  /// Slot 1: Secondary Video
  /// Audio comes from the player in the slot denoted by <see cref="AudioSlotIndex"/>
  /// </summary>
  /// <remarks>
  /// At the moment, this service is not specified to be thread-safe.
  /// </remarks>
  public interface IPlayerManager : IDisposable
  {
    /// <summary>
    /// Returns the number of open player slots (0, 1 or 2).
    /// </summary>
    int NumOpenSlots { get; }

    /// <summary>
    /// Returns <c>true</c>, if the number of open slots (see <see cref="NumOpenSlots"/>) is less than 2. Else,
    /// returns <c>false</c>.
    /// </summary>
    bool CanOpenSlot { get; }

    /// <summary>
    /// Gets or sets the index of the slot which provides the audio signal. If there is no active slot
    /// at the moment, then <see cref="AudioSlotIndex"/> will be <c>-1</c>.
    /// </summary>
    int AudioSlotIndex { get; set; }

    /// <summary>
    /// Opens a player slot.
    /// This method succeeds if <see cref="CanOpenSlot"/> returned <c>true</c>.
    /// </summary>
    /// <remarks>
    /// This method should be used to start the first or second player slot.
    /// </remarks>
    /// <param name="slotIndex">Returns the index of the new slot, if the preparation was successful.</param>
    /// <param name="controller">Returns the slot controller or the new slot.</param>
    /// <returns><c>true</c>, if the new slot could be opened, else <c>false</c>.</returns>
    bool OpenSlot(out int slotIndex, out IPlayerSlotController controller);

    /// <summary>
    /// Releases the player of the specified <paramref name="slotIndex"/> and closes the slot.
    /// </summary>
    /// <remarks>
    /// If the specified <paramref name="slotIndex"/> provides the audio signal, the audio flag will go to
    /// the remaining slot, if present. If the specified <paramref name="slotIndex"/> is the first/primary player
    /// slot, then after closing it the secondary slot will become the primary slot.
    /// </remarks>
    /// <param name="slotIndex">Index of the slot to close.</param>
    void CloseSlot(int slotIndex);

    /// <summary>
    /// Stops and releases all active players and closes their slots.
    /// </summary>
    void CloseAllSlots();

    /// <summary>
    /// Gets the player slot instance for the slot of the specified <paramref name="slotIndex"/> index.
    /// </summary>
    /// <param name="slotIndex">Index of the slot to return the controller instance for.</param>
    /// <returns>The controller instance for the specified slot.</returns>
    IPlayerSlotController GetSlot(int slotIndex);

    /// <summary>
    /// Gets the player at the specified player slot, or <c>null</c>, if there is no player in the slot.
    /// </summary>
    IPlayer this[int slotIndex] { get; }

    /// <summary>
    /// Switches the primary and secondary player slots. The slot controller, which was located in slot 0,
    /// will be moved to slot 1 and vice-versa. This method only succeeds if there are exactly two open slots.
    /// </summary>
    void SwitchPlayers();

    /// <summary>
    /// Executes the given method on each active slot.
    /// </summary>
    /// <param name="execute">Method to execute.</param>
    void ForEach(PlayerSlotWorkerDelegate execute);
  }
}
