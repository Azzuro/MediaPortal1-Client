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
using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib.SBE;
using TvLibrary;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using TvLibrary.Implementations.Analog;
using TvLibrary.Implementations.DVB;
using TvLibrary.Channels;

using TvDatabase;
namespace TvService
{
  /// <summary>
  /// Class which can be used to sort Cards bases on priority
  /// </summary>
  public class CardDetail : IComparable<CardDetail>
  {
    int _cardId;
    Card _card;
    IChannel _detail;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="id">card id</param>
    /// <param name="card">card dataaccess object</param>
    /// <param name="detail">tuning detail</param>
    public CardDetail(int id, Card card, IChannel detail)
    {
      _cardId = id;
      _card = card;
      _detail = detail;
    }

    /// <summary>
    /// gets the id of the card
    /// </summary>
    public int Id
    {
      get
      {
        return _cardId;
      }
    }

    /// <summary>
    /// gets the card
    /// </summary>
    public Card Card
    {
      get
      {
        return _card;
      }
    }
    /// <summary>
    /// gets the tuning detail
    /// </summary>
    public IChannel TuningDetail
    {
      get
      {
        return _detail;
      }
    }

    #region IComparable<CardInfo> Members

    public int CompareTo(CardDetail other)
    {
      if (other.Card.Priority > _card.Priority) return -1;
      if (other.Card.Priority < _card.Priority) return 1;
      return 0;
    }

    #endregion
  }
}
