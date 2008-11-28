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

using System.Collections.Generic;
using MediaPortal.Presentation.Commands;
using MediaPortal.Presentation.Localisation;
using MediaPortal.Utilities;

namespace MediaPortal.Presentation.DataObjects
{
  public delegate void ListItemChangedHandler(ListItem item);

  /// <summary>
  /// Common class for wrapping single "items" to be displayed in the GUI.
  /// The term "item" in this context stands for a the (localized) text contents of a single,
  /// to be displayed menu point, button, tree view item or similar object.
  /// It can hold one or more named "labels", which are typically short texts for different
  /// properties/attributes of the item, like the name, the artist and the play time of a song.
  /// Labels can be accessed by their label name.
  /// </summary>
  /// <remarks>
  /// Instances of the <see cref="ListItem"/> class can store different information about an item:
  /// <list type="bullet">
  /// <item>Different named "labels"</item>
  /// <item>A selection state</item>
  /// <item>An associated command, to be executed when this item is choosen for example</item>
  /// <item>Change listeners which can be called when the item's contents change</item>
  /// </list>
  /// Changes do <b>not</b> automatically trigger the <see cref="OnChanged"/> event; this event
  /// has to be explicitly triggered by modifying clients.
  /// </remarks>
  /// TODO: Add methods/constructor to add unlocalized labels
  public class ListItem
  {
    #region Protected fields

    protected Property _commandProperty = new Property(typeof(ICommand), null);
    protected Property _commandParameterProperty = new Property(typeof(ICommandParameter), null);
    protected IDictionary<string, IResourceString> _labels = new Dictionary<string, IResourceString>();
    protected Property _selectedProperty = new Property(typeof(bool), false);

    /// <summary>
    /// Event to track changes to this item.
    /// </summary>
    public event ListItemChangedHandler OnChanged;

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ListItem"/> class with one
    /// localized or unlocalized string label. This constructor will determine if the
    /// specified string references a localized resource.
    /// This is a convenience constructor for calling <see cref="ListItem(string, StringId)"/> or
    /// <see cref="ListItem(string, IResourceString)"/>.
    /// </summary>
    /// <param name="name">The name of the label to be set to <paramref name="value"/>.</param>
    /// <param name="value">The value to create the label with. If <paramref name="value"/>
    /// references a localized string resource, a localized label will be created. Else
    /// an unlocalized label will be used.</param>
    public ListItem(string name, string value)
    {
      SetLabel(name, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListItem"/> class with one
    /// localized string label.
    /// </summary>
    /// <param name="name">The name of the label to be set to <paramref name="value"/>.</param>
    /// <param name="value">Localized string value of the new item.</param>
    public ListItem(string name, StringId value) : this(name, new LocalizedStringBuilder(value)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListItem"/> class with a resource string,
    /// which itself may be localized or unlocalized.
    /// </summary>
    /// <param name="name">The name of the label to be set to <paramref name="value"/>.</param>
    /// <param name="value">Localized or unlocalized resource string value of the new item.</param>
    public ListItem(string name, IResourceString value)
    {
      _labels[name] = value;
    }

    /// <summary>
    /// Initializes a new empty instance of the <see cref="ListItem"/> class.
    /// All attributes are set to default values.
    /// </summary>
    public ListItem() { }

    /// <summary>
    /// Determines whether this item contains a label with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>
    /// <c>true</c>, if this item contains a label with the specified name, otherwise <c>false</c>.
    /// </returns>
    public bool Contains(string name)
    {
      return _labels.ContainsKey(name);
    }

    /// <summary>
    /// Returns the label property for the label with the specified name.
    /// If this itemd doesn't contain a label with the specified name, the <paramref name="defValue"/>
    /// will be returned.
    /// </summary>
    /// <param name="name">Name of the label to return.</param>
    /// <param name="defValue">Label string to be returned if the label with the specified name is not
    /// present in this item. If this value references a localized string, a localized label will be returned.</param>
    /// <returns>Label property instance with the specified name or a new label property with the default value.</returns>
    public IResourceString Label(string name, string defValue)
    {
      return _labels.ContainsKey(name) ? _labels[name] : LocalizationHelper.CreateLabelProperty(defValue);
    }

    /// <summary>
    /// Adds a named label property to this item. The specified <paramref name="value"/>
    /// may be a localized string; in this case this method will add it as localized
    /// label property.
    /// </summary>
    /// <param name="name">The name for the new label.</param>
    /// <param name="value">The string label to be added. If this parameter references a
    /// localized resource, the new label will be a localized string label.</param>
    public void SetLabel(string name, string value)
    {
      _labels[name] = LocalizationHelper.CreateLabelProperty(value);
    }

    /// <summary>
    /// Gets or sets the dictionary of named labels for this item.
    /// </summary>
    public IDictionary<string, IResourceString> Labels
    {
      get { return _labels; }
      set { _labels = value; }
    }

    /// <summary>
    /// Executes the command associated with this item.
    /// </summary>
    public virtual void Execute()
    {
      if (Command != null)
        Command.Execute(CommandParameter);
    }

    public Property CommandProperty
    {
      get { return _commandProperty; }
    }

    /// <summary>
    /// Gets or sets the associated command which can be executed.
    /// </summary>
    public ICommand Command
    {
      get { return (ICommand) _commandProperty.GetValue(); }
      set { _commandProperty.SetValue(value); }
    }

    public Property CommandParameterProperty
    {
      get { return _commandParameterProperty; }
    }

    /// <summary>
    /// Gets or sets the command parameter.
    /// </summary>
    public ICommandParameter CommandParameter
    {
      get { return (ICommandParameter) _commandParameterProperty.GetValue(); }
      set { _commandParameterProperty.SetValue(value); }
    }

    public Property SelectedProperty
    {
      get { return _selectedProperty; }
      set { _selectedProperty = value; }
    }

    /// <summary>
    /// Gets or sets the selected state of this item.
    /// </summary>
    public bool Selected
    {
      get { return (bool)_selectedProperty.GetValue(); }
      set { _selectedProperty.SetValue(value); }
    }

    /// <summary>
    /// Fires the <see cref="OnChanged"/> event.
    /// </summary>
    public void FireChange()
    {
      if (OnChanged != null)
        OnChanged(this);
    }

    /// <summary>
    /// Returns the label string for the current locale with the specified name, or an empty
    /// string, if the name is not present.
    /// </summary>
    public string this[string name]
    {
      get { return _labels.ContainsKey(name) ? _labels[name].Evaluate() : string.Empty; }
    }

    public override string ToString()
    {
      IList<string> l = new List<string>();
      foreach (KeyValuePair<string, IResourceString> kvp in _labels)
        l.Add(kvp.Key + "=" + kvp.Value.Evaluate());
      return StringUtils.Join(", ", l);
    }
  }
}
