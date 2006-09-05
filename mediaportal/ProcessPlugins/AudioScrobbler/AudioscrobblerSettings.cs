#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web.Security;
using System.Windows.Forms;
using MediaPortal.Music.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Util;


namespace MediaPortal.AudioScrobbler
{
  public partial class AudioscrobblerSettings : Form
  {
    private AudioscrobblerUtils lastFmLookup;
    List<Song> songList = null;
    List<Song> similarList = null;
    private string _currentUser = String.Empty;

    public AudioscrobblerSettings()
    {
      InitializeComponent();
      LoadSettings();
    }

    #region Serialisation
    protected void LoadSettings()
    {
      try
      {
        MusicDatabase mdb = new MusicDatabase();
        List<string> scrobbleusers = new List<string>();
        string tmpuser = "";
        string tmppass = "";

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
        {
          tmpuser = xmlreader.GetValueAsString("audioscrobbler", "user", "");

          scrobbleusers = mdb.GetAllScrobbleUsers();
          // no users in database
          if (scrobbleusers.Count == 0)
          {
            tabControlLiveFeeds.Enabled = false;
            tabControlSettings.TabPages.RemoveAt(1);
            tabControlSettings.TabPages.RemoveAt(1);
            labelNoUser.Visible = true;
          }
          // only load settings if a user is present
          else
          {
            int selected = 0;
            int count = 0;
            foreach (string scrobbler in scrobbleusers)
            {
              if (!comboBoxUserName.Items.Contains(scrobbler))
                comboBoxUserName.Items.Add(scrobbler);

              if (scrobbler == tmpuser)
                selected = count;
              count++;
            }
            comboBoxUserName.SelectedIndex = selected;
            buttonDelUser.Enabled = true;

            tmppass = mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(_currentUser)), "");

            EncryptDecrypt Crypter = new EncryptDecrypt();

            if (tmppass != String.Empty)
            {
              try
              {
                EncryptDecrypt DCrypter = new EncryptDecrypt();
                maskedTextBoxASPassword.Text = DCrypter.Decrypt(tmppass);
              }
              catch (Exception)
              {
                //Log.Info("Audioscrobbler: Password decryption failed {0}", ex.Message);
              }
            }
            checkBoxLimitPlaylist.Checked = xmlreader.GetValueAsBool("audioscrobbler", "playlistlimit", true);

            int tmpNMode = 1;
            int tmpRand = 77;
            int tmpArtists = 2;
            int tmpPreferTracks = 2;
            int tmpOfflineMode = 0;
            string tmpUserID = Convert.ToString(mdb.AddScrobbleUser(_currentUser));

            checkBoxLogVerbose.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iDebugLog", -1) == 1) ? true : false;
            tmpRand = mdb.AddScrobbleUserSettings(tmpUserID, "iRandomness", -1);
            checkBoxEnableSubmits.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iSubmitOn", -1) == 1) ? true : false;
            checkBoxScrobbleDefault.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iScrobbleDefault", -1) == 1) ? true : false;
            tmpArtists = mdb.AddScrobbleUserSettings(tmpUserID, "iAddArtists", -1);
            //numericUpDownTracksPerArtist.Value = mdb.AddScrobbleUserSettings(tmpUserID, "iAddTracks", -1);
            tmpNMode = mdb.AddScrobbleUserSettings(tmpUserID, "iNeighbourMode", -1);

            tmpOfflineMode = mdb.AddScrobbleUserSettings(tmpUserID, "iOfflineMode", -1);
            tmpPreferTracks = mdb.AddScrobbleUserSettings(tmpUserID, "iPreferCount", -1);
            checkBoxLimitPlaylist.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iPlaylistLimit", -1) == 1) ? true : false;
            checkBoxReAddArtist.Checked = (mdb.AddScrobbleUserSettings(tmpUserID, "iRememberStartArtist", -1) == 1) ? true : false;

            numericUpDownSimilarArtist.Value = (tmpArtists > 0) ? tmpArtists : 2;
            trackBarRandomness.Value = (tmpRand >= 25) ? tmpRand : 25;
            trackBarConsiderCount.Value = (tmpPreferTracks >= 0) ? tmpPreferTracks : 2;
            comboBoxOfflineMode.SelectedIndex = tmpOfflineMode;

            lastFmLookup = new AudioscrobblerUtils();

            switch (tmpNMode)
            {
              case 3:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.topartists;
                comboBoxNeighbourMode.SelectedIndex = 0;
                comboBoxNModeSelect.SelectedIndex = 0;
                break;
              case 1:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
                comboBoxNeighbourMode.SelectedIndex = 1;
                comboBoxNModeSelect.SelectedIndex = 1;
                break;
              case 0:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.recenttracks;
                comboBoxNeighbourMode.SelectedIndex = 2;
                comboBoxNModeSelect.SelectedIndex = 2;
                break;
              default:
                lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
                comboBoxNeighbourMode.SelectedIndex = 1;
                comboBoxNModeSelect.SelectedIndex = 1;
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Audioscrobbler settings could not be loaded: {0}", ex.Message);
      }
    }

    protected bool CheckOrSetDefaultDBSettings(string userName_)
    {
      MusicDatabase mdb = new MusicDatabase();
      bool defaultsNeeded = false;

      String tmpUserID = Convert.ToString(mdb.AddScrobbleUser(userName_));

      // disable log
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iDebugLog", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iDebugLog", 0);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iDebugLog");
      }
      // set randomness
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iRandomness", -1) < 1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iRandomness", 77);
        defaultsNeeded = true;
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iRandomness");
      }     
      // enable scrobbling
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iSubmitOn", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iSubmitOn", 1);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iSubmitOn");
      }
      // disable Scrobble On on startup
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iScrobbleDefault", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iScrobbleDefault", 0);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iScrobbleDefault");
      }
      // consider 3 artists to add
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iAddArtists", -1) < 1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iAddArtists", 3);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iAddArtists");
      }
      // consider adding 1 track per artist
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iAddTracks", -1) < 1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iAddTracks", 1);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iAddTracks");
      }
      // set neighbour mode to weekly artists
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iNeighbourMode", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iNeighbourMode", 1);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iNeighbourMode");
      }

      if (mdb.AddScrobbleUserSettings(tmpUserID, "iOfflineMode", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iOfflineMode", 0);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iOfflineMode");
      }
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iPlaylistLimit", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iPlaylistLimit", 1);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iPlaylistLimit");
      }
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iPreferCount", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iPreferCount", 2);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iPreferCount");
      }
      if (mdb.AddScrobbleUserSettings(tmpUserID, "iRememberStartArtist", -1) == -1)
      {
        mdb.AddScrobbleUserSettings(tmpUserID, "iRememberStartArtist", 1);
        //Log.Info("Audioscrobbler: sql setting for option: {0} didn't exist using defaults", "iRememberStartArtist");
      }

      return defaultsNeeded;
    }

    protected void SaveSettings()
    {
      MusicDatabase mdb = new MusicDatabase();
      int usedebuglog = 0;
      int submitsenabled = 1;
      int scrobbledefault = 0;
      int randomness = 77;
      int artisttoadd = 3;
      int trackstoadd = 1;
      int neighbourmode = 1;

      int offlinemode = 0;
      int playlistlimit = 1;
      int prefercount = 2;
      int rememberstartartist = 1;

      if (comboBoxUserName.Text != String.Empty)
      {
        using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
        {
          xmlwriter.SetValue("audioscrobbler", "user", comboBoxUserName.Text);

          string tmpPass = "";
          string tmpUserID = "";
          try
          {
            EncryptDecrypt Crypter = new EncryptDecrypt();
            tmpPass = Crypter.Encrypt(maskedTextBoxASPassword.Text);
          }
          catch (Exception)
          {
            //Log.Info("Audioscrobbler: Password encryption failed {0}", ex.Message);
          }

          // checks and adds the user if necessary + updates the password;
          mdb.AddScrobbleUserPassword(Convert.ToString(mdb.AddScrobbleUser(comboBoxUserName.Text)), tmpPass);

          // if defaults were added do not overwrite them..
          if (!CheckOrSetDefaultDBSettings(comboBoxUserName.Text))
          {
            if (checkBoxDisableRandom.Checked)
              xmlwriter.SetValueAsBool("audioscrobbler", "usesimilarrandom", false);
            else
              xmlwriter.SetValueAsBool("audioscrobbler", "usesimilarrandom", true);

            if (checkBoxLogVerbose != null)
              usedebuglog = checkBoxLogVerbose.Checked ? 1 : 0;
            if (checkBoxEnableSubmits != null)
              submitsenabled = checkBoxEnableSubmits.Checked ? 1 : 0;
            if (checkBoxScrobbleDefault != null)
              scrobbledefault = checkBoxScrobbleDefault.Checked ? 1 : 0;
            if (trackBarRandomness != null)
              randomness = trackBarRandomness.Value;
            if (numericUpDownSimilarArtist != null)
              artisttoadd = (int)numericUpDownSimilarArtist.Value;
            //if (numericUpDownTracksPerArtist != null)
            //  trackstoadd = (int)numericUpDownTracksPerArtist.Value;
            if (lastFmLookup != null)
              neighbourmode = (int)lastFmLookup.CurrentNeighbourMode;
            else
              Log.Info("DEBUG *** lastFMLookup was null. neighbourmode: {0}", Convert.ToString(neighbourmode));

            if (comboBoxOfflineMode != null)
              offlinemode = comboBoxOfflineMode.SelectedIndex;
            if (checkBoxLimitPlaylist != null)
              playlistlimit = checkBoxLimitPlaylist.Checked ? 1 : 0;
            if (trackBarConsiderCount != null)
              prefercount = trackBarConsiderCount.Value;
            if (checkBoxReAddArtist != null)
              rememberstartartist = checkBoxReAddArtist.Checked ? 1 : 0;
            
            tmpUserID = Convert.ToString(mdb.AddScrobbleUser(comboBoxUserName.Text));
            mdb.AddScrobbleUserSettings(tmpUserID, "iDebugLog", usedebuglog);
            mdb.AddScrobbleUserSettings(tmpUserID, "iRandomness", randomness);
            mdb.AddScrobbleUserSettings(tmpUserID, "iSubmitOn", submitsenabled);
            mdb.AddScrobbleUserSettings(tmpUserID, "iScrobbleDefault", scrobbledefault);
            mdb.AddScrobbleUserSettings(tmpUserID, "iAddArtists", artisttoadd);
            mdb.AddScrobbleUserSettings(tmpUserID, "iAddTracks", trackstoadd);
            mdb.AddScrobbleUserSettings(tmpUserID, "iNeighbourMode", neighbourmode);
            mdb.AddScrobbleUserSettings(tmpUserID, "iOfflineMode", offlinemode);
            mdb.AddScrobbleUserSettings(tmpUserID, "iPlaylistLimit", playlistlimit);
            mdb.AddScrobbleUserSettings(tmpUserID, "iPreferCount", prefercount);
            mdb.AddScrobbleUserSettings(tmpUserID, "iRememberStartArtist", rememberstartartist);
          }
        }
      }
    }
    #endregion

    #region control events

    // Implements the manual sorting of items by columns.
    class ListViewItemComparer : IComparer
    {
      private int col;
      public ListViewItemComparer()
      {
        col = 0;
      }
      public ListViewItemComparer(int column)
      {
        col = column;
      }
      public int Compare(object x, object y)
      {
        try
        {
          if (Convert.ToInt16(((ListViewItem)y).SubItems[col].Text) == Convert.ToInt16(((ListViewItem)x).SubItems[col].Text))
            return 0;
          if (Convert.ToInt16(((ListViewItem)y).SubItems[col].Text) > Convert.ToInt16(((ListViewItem)x).SubItems[col].Text))
            return 1;
          else
            return -1;
        }
        catch (Exception)
        {
          return 0;
        }
      }
    }

    private void trackBarRandomness_MouseHover(object sender, EventArgs e)
    {
      toolTipRandomness.SetToolTip(trackBarRandomness, "If you lower the percentage value you'll get results more similar");
      toolTipRandomness.Active = true;
    }

    private void trackBarRandomness_MouseLeave(object sender, EventArgs e)
    {
      toolTipRandomness.Active = false;
    }

    private void labelSimilarArtistsUpDown_MouseHover(object sender, EventArgs e)
    {
      toolTipRandomness.SetToolTip(labelSimilarArtistsUpDown, "Increase if you do not get enough songs - lower if the playlist grows too fast");
      toolTipRandomness.Active = true;
    }

    private void labelSimilarArtistsUpDown_MouseLeave(object sender, EventArgs e)
    {
      toolTipRandomness.Active = false;
    }

    private void linkLabelMPGroup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      // Determine which link was clicked within the LinkLabel.
      this.linkLabelMPGroup.Links[linkLabelMPGroup.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "http://www.last.fm/group/MediaPortal%2BUsers");
      }
      catch
      {
      }
    }

    private void linkLabelNewUser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      this.linkLabelNewUser.Links[linkLabelNewUser.Links.IndexOf(e.Link)].Visited = true;
      try
      {
        Help.ShowHelp(this, "https://www.last.fm/join/");
      }
      catch
      {
      }
    }

    private void trackBarArtistMatch_ValueChanged(object sender, EventArgs e)
    {
      labelTrackBarValue.Text = Convert.ToString(trackBarArtistMatch.Value);
    }

    private void trackBarRandomness_ValueChanged(object sender, EventArgs e)
    {
      labelPercRand.Text = Convert.ToString(trackBarRandomness.Value);
    }

    private void comboBoxNeighbourMode_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBoxNeighbourMode.SelectedIndex)
      {
        case 0:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.topartists;
          break;
        case 1:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
          break;
        case 2:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.recenttracks;
          break;
      }
    }

    private void comboBoxNModeSelect_SelectedIndexChanged(object sender, EventArgs e)
    {
      switch (comboBoxNModeSelect.SelectedIndex)
      {
        case 0:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.topartists;
          break;
        case 1:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.weeklyartistchart;
          break;
        case 2:
          lastFmLookup.CurrentNeighbourMode = lastFMFeed.recenttracks;
          break;
      }
    }

    private void trackBarConsiderCount_ValueChanged(object sender, EventArgs e)
    {
      switch (trackBarConsiderCount.Value)
      {
        case 0:
          labelPlaycountHint.Text = "add only unheard tracks for artist";
          break;
        case 1:
          labelPlaycountHint.Text = "add only rarely heard tracks for artist";
          break;
        case 2:
          labelPlaycountHint.Text = "add totally random tracks for artist";
          break;
        case 3:
          labelPlaycountHint.Text = "add only popular tracks for artist";
          break;
      }
    }

    private void textBoxTagToSearch_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        buttonGetTaggedArtists_Click(sender, e);
    }

    #endregion

    private ListViewItem BuildListViewItemSingleTag(Song song_)
    {
      ListViewItem listItem = new ListViewItem(song_.Artist);          
      listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewArtist(Song song_, bool showPlayed_, bool showMatch_)
    {
      ListViewItem listItem = new ListViewItem(song_.Artist);
      if (showPlayed_)
        listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      if (showMatch_)
        listItem.SubItems.Add(song_.LastFMMatch);
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewArtistAlbum(Song song_)
    {
      ListViewItem listItem = new ListViewItem(song_.Artist);      
      listItem.SubItems.Add(song_.Album);
      listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewTrackArtist(Song song_, bool showPlayed_, bool showDate_)
    {
      ListViewItem listItem = new ListViewItem(song_.Title);
      listItem.SubItems.Add(song_.Artist);
      if (showPlayed_)
        listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      if (showDate_)
        listItem.SubItems.Add(song_.getQueueTime());
      listItem.Tag = song_;
      return listItem;
    }

    private ListViewItem BuildListViewFullTrack(Song song_)
    {
      ListViewItem listItem = new ListViewItem(song_.Title);
      listItem.SubItems.Add(song_.Artist);
      listItem.SubItems.Add(song_.Album);
      listItem.SubItems.Add(Convert.ToString(song_.TimesPlayed));
      listItem.Tag = song_;
      return listItem;
    }

    #region Button events

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      SaveSettings();
      this.Close();
    }

    private void buttonTagsRefresh_Click(object sender, EventArgs e)
    {
      buttonTagsRefresh.Enabled = false;
      listViewTags.Clear();
      ListViewItem listItem = new ListViewItem();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.toptags, "");
      listViewTags.BeginUpdate();
      listViewTags.Columns.Add("Your tags", 170);
      listViewTags.Columns.Add("Popularity", 70); 
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(BuildListViewItemSingleTag(songList[i]));        
      listViewTags.EndUpdate();
      buttonTagsRefresh.Enabled = true;
    }

    private void buttonGetTaggedArtists_Click(object sender, EventArgs e)
    {
      buttonGetTaggedArtists.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedartists, System.Web.HttpUtility.UrlEncode(textBoxTagToSearch.Text), checkBoxTagRandomize.Checked);
      listViewTags.Columns.Add("Artist", 170);
      listViewTags.Columns.Add("Popularity", 70);
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(BuildListViewArtist(songList[i], true, false));
      buttonGetTaggedArtists.Enabled = true;
    }

    private void buttonTaggedAlbums_Click(object sender, EventArgs e)
    {
      buttonTaggedAlbums.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedalbums, System.Web.HttpUtility.UrlEncode(textBoxTagToSearch.Text), checkBoxTagRandomize.Checked);
      listViewTags.Columns.Add("Artist", 170);
      listViewTags.Columns.Add("Album", 170);
      listViewTags.Columns.Add("Popularity", 70); 
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(BuildListViewArtistAlbum(songList[i]));
      buttonTaggedAlbums.Enabled = true;
    }

    private void buttonTaggedTracks_Click(object sender, EventArgs e)
    {
      buttonTaggedTracks.Enabled = false;
      listViewTags.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getSimilarToTag(lastFMFeed.taggedtracks, System.Web.HttpUtility.UrlEncode(textBoxTagToSearch.Text), checkBoxTagRandomize.Checked);
      listViewTags.Columns.Add("Track", 170);
      listViewTags.Columns.Add("Artist", 170);
      listViewTags.Columns.Add("Popularity", 70);
      for (int i = 0; i < songList.Count; i++)
        listViewTags.Items.Add(BuildListViewTrackArtist(songList[i], true, false));
      buttonTaggedTracks.Enabled = true;
    }

    private void buttonRefreshRecent_Click(object sender, EventArgs e)
    {
      buttonRefreshRecent.Enabled = false;
      listViewRecentTracks.Clear();
      songList = new List<Song>(); ;
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.recenttracks, "");
      listViewRecentTracks.Columns.Add("Track", 155);
      listViewRecentTracks.Columns.Add("Artist", 155);
      listViewRecentTracks.Columns.Add("Date", 115);
      
      for (int i = 0; i < songList.Count; i++)
        listViewRecentTracks.Items.Add(BuildListViewTrackArtist(songList[i], false, true));
      buttonRefreshRecent.Enabled = true;
    }

    private void buttonArtistsRefresh_Click(object sender, EventArgs e)
    {
      buttonArtistsRefresh.Enabled = false;
      listViewTopArtists.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.topartists, "");
      listViewTopArtists.Columns.Add("Artist", 200);
      listViewTopArtists.Columns.Add("Play count", 100);
      for (int i = 0; i < songList.Count; i++)
        listViewTopArtists.Items.Add(BuildListViewArtist(songList[i], true, false));
      buttonArtistsRefresh.Enabled = true;
    }

    private void buttonRefreshWeeklyArtists_Click(object sender, EventArgs e)
    {
      buttonRefreshWeeklyArtists.Enabled = false;
      listViewWeeklyArtists.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.weeklyartistchart, "");
      listViewWeeklyArtists.Columns.Add("Artist", 200);
      listViewWeeklyArtists.Columns.Add("Play count", 100);
      for (int i = 0; i < songList.Count; i++)
        listViewWeeklyArtists.Items.Add(BuildListViewArtist(songList[i], true, false));
      buttonRefreshWeeklyArtists.Enabled = true;
    }

    private void buttonTopTracks_Click(object sender, EventArgs e)
    {
      buttonTopTracks.Enabled = false;
      listViewTopTracks.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.toptracks, "");
      listViewTopTracks.Columns.Add("Track", 170);
      listViewTopTracks.Columns.Add("Artist", 170);
      listViewTopTracks.Columns.Add("Play count", 70);
      for (int i = 0; i < songList.Count; i++)
        listViewTopTracks.Items.Add(BuildListViewTrackArtist(songList[i], true, false));
      buttonTopTracks.Enabled = true;
    }

    private void buttonRefreshWeeklyTracks_Click(object sender, EventArgs e)
    {
      buttonRefreshWeeklyTracks.Enabled = false;
      listViewWeeklyTracks.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.weeklytrackchart, "");
      listViewWeeklyTracks.Columns.Add("Track", 170);
      listViewWeeklyTracks.Columns.Add("Artist", 170);
      listViewWeeklyTracks.Columns.Add("Play count", 70);
      for (int i = 0; i < songList.Count; i++)
        listViewWeeklyTracks.Items.Add(BuildListViewTrackArtist(songList[i], true, false));
      buttonRefreshWeeklyTracks.Enabled = true;
    }

    private void changeControlsSuggestions(bool runningNow_)
    {
      if (runningNow_)
      {
        buttonRefreshSuggestions.Enabled = false;
        trackBarArtistMatch.Hide();
        labelArtistMatch.Hide();
        labelTrackBarValue.Hide();
        progressBarSuggestions.Value = 0;
        progressBarSuggestions.Visible = true;
        listViewSuggestions.Clear();
      }
      else
      {
        progressBarSuggestions.Visible = false;
        trackBarArtistMatch.Show();
        labelArtistMatch.Show();
        labelTrackBarValue.Show();
        buttonRefreshSuggestions.Enabled = true;
      }
    }

    private void buttonRefreshSuggestions_Click(object sender, EventArgs e)
    {
      changeControlsSuggestions(true);
      lastFmLookup.ArtistMatchPercent = trackBarArtistMatch.Value;

      progressBarSuggestions.PerformStep();
      songList = new List<Song>();
      similarList = new List<Song>();

      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.topartists, "");
      progressBarSuggestions.PerformStep();

      listViewSuggestions.Columns.Add("Artist", 250);
      listViewSuggestions.Columns.Add("Match", 100);

      if (songList.Count > 7)
      {
        for (int i = 0; i <= 7; i++)
        {
          similarList.AddRange(lastFmLookup.getSimilarArtists(songList[i].ToURLArtistString(), false));
          progressBarSuggestions.PerformStep();
        }

        for (int i = 0; i < similarList.Count; i++)
        {
          bool foundDoubleEntry = false;
          for (int j = 0; j < listViewSuggestions.Items.Count; j++)
          {
            if (listViewSuggestions.Items[j].Text == similarList[i].Artist)
            {
              foundDoubleEntry = true;
              break;
            }
          }
          if (!foundDoubleEntry)
            listViewSuggestions.Items.Add(BuildListViewArtist(similarList[i], false, true));
        }
        //listViewSuggestions.Sorting = SortOrder.Descending;
        //listViewSuggestions.AllowColumnReorder = true;
        listViewSuggestions.ListViewItemSorter = new ListViewItemComparer(1);

      }
      else
        listViewSuggestions.Items.Add("Not enough overall top artists found");
      progressBarSuggestions.PerformStep();
      changeControlsSuggestions(false);
    }

    private void buttonRefreshNeighbours_Click(object sender, EventArgs e)
    {
      buttonRefreshNeighbours.Enabled = false;
      listViewNeighbours.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getAudioScrobblerFeed(lastFMFeed.neighbours, "");

      listViewNeighbours.Columns.Add("Your neighbours", 250);
      listViewNeighbours.Columns.Add("Match", 100);

      for (int i = 0; i < songList.Count; i++)
        listViewNeighbours.Items.Add(BuildListViewArtist(songList[i], false, true));
      buttonRefreshNeighbours.Enabled = true;

      if (listViewNeighbours.Items.Count > 0)
      {
        buttonRefreshNeigboursArtists.Enabled = true;
        comboBoxNeighbourMode.Enabled = true;
      }
      else
      {
        buttonRefreshNeigboursArtists.Enabled = false;
        comboBoxNeighbourMode.Enabled = false;
      }
    }

    private void buttonRefreshNeigboursArtists_Click(object sender, EventArgs e)
    {
      buttonRefreshNeigboursArtists.Enabled = false;
      listViewNeighbours.Clear();
      songList = new List<Song>();
      songList = lastFmLookup.getNeighboursArtists(false);
      listViewNeighbours.Columns.Add("Artist", 170);
      listViewNeighbours.Columns.Add("Play count", 70);

      for (int i = 0; i < songList.Count; i++)
      {
        bool foundDoubleEntry = false;
        for (int j = 0; j < listViewNeighbours.Items.Count; j++)
        {
          if (listViewNeighbours.Items[j].Text == songList[i].Artist)
          {
            foundDoubleEntry = true;
            break;
          }
        }
        if (!foundDoubleEntry)
          listViewNeighbours.Items.Add(BuildListViewArtist(songList[i], true, false));
      }
      buttonRefreshNeigboursArtists.Enabled = true;
      if (listViewNeighbours.Items.Count > 0)
      {
        listViewNeighbours.ListViewItemSorter = new ListViewItemComparer(1);
        buttonNeighboursFilter.Enabled = true;
      }
      else
        buttonNeighboursFilter.Enabled = false;
    }

    private void buttonNeighboursFilter_Click(object sender, EventArgs e)
    {
      //      buttonNeighboursFilter.Enabled = false;
      listViewNeighbours.Clear();
      tabControlLiveFeeds.Enabled = false;
      ArrayList artistsInDB = new ArrayList();
      songList = new List<Song>();
      songList = lastFmLookup.getNeighboursArtists(false);
      listViewNeighbours.Columns.Add("Artist", 170);
      for (int i = 0; i < songList.Count; i++)
      {
        MusicDatabase mdb = new MusicDatabase();
        if (!mdb.GetArtists(4, songList[i].Artist, ref artistsInDB))
        {
          bool foundDoubleEntry = false;
          for (int j = 0; j < listViewNeighbours.Items.Count; j++)
          {
            if (listViewNeighbours.Items[j].Text == songList[i].Artist)
            {
              foundDoubleEntry = true;
              break;
            }
          }
          if (!foundDoubleEntry)
            listViewNeighbours.Items.Add(BuildListViewArtist(songList[i], false, false));
        }
        //else
        //  MessageBox.Show("Artist " + songList[i].Artist + " already in DB!");
      }
      tabControlLiveFeeds.Enabled = true;
    }

    #endregion

    private void comboBoxUserName_SelectedIndexChanged(object sender, EventArgs e)
    {
      _currentUser = comboBoxUserName.Text;
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.Get(Config.Dir.Config) + "MediaPortal.xml"))
        xmlwriter.SetValue("audioscrobbler", "user", _currentUser);
      LoadSettings();
    }

    private void buttonAddUser_Click(object sender, EventArgs e)
    {
      if (tabControlSettings.TabCount > 1)
      {
        tabControlLiveFeeds.Enabled = false;
        tabControlSettings.TabPages.RemoveAt(1);
        tabControlSettings.TabPages.RemoveAt(1);
      }
      labelNewUserHint.Visible = true;
      comboBoxUserName.DropDownStyle = ComboBoxStyle.DropDown;
      comboBoxUserName.Text = String.Empty;
      maskedTextBoxASPassword.Text = String.Empty;
      groupBoxOptions.Enabled = false;
      buttonAddUser.Enabled = false;
      buttonDelUser.Enabled = false;
      comboBoxUserName.Focus();
    }

    private void maskedTextBoxASPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        buttonOk_Click(sender, e);
    }


    private void buttonDelUser_Click(object sender, EventArgs e)
    {
      MusicDatabase mdb = new MusicDatabase();
      mdb.DeleteScrobbleUser(comboBoxUserName.Text);
      if (comboBoxUserName.Items.Count <= 1)
      {
        buttonDelUser.Enabled = false;
        maskedTextBoxASPassword.Clear();
      }
      comboBoxUserName.Items.Clear();
      LoadSettings();
    }
    
  }
}