#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.ComponentModel;

namespace System.Windows
{
	[TypeConverter(typeof(DependencyPropertyConverter))]
	public sealed class DependencyProperty
	{
		#region Constructors

		private DependencyProperty()
		{
		}

		private DependencyProperty(DependencyProperty dp, PropertyMetadata defaultMetadata)
		{
			_defaultMetadata = defaultMetadata;
			_name = dp._name;
			_ownerType = dp._ownerType;
			_propertyType = dp._propertyType;
			_validateValueCallback = dp._validateValueCallback;

			_properties[_name + _ownerType] = this;
		}

		private DependencyProperty(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			_name = name;
			_propertyType = propertyType;
			_ownerType = ownerType;
			_defaultMetadata = defaultMetadata;
			_validateValueCallback = validateValueCallback;

			_properties[name + ownerType] = this;
		}

		#endregion Constructors

		#region Methods

		public DependencyProperty AddOwner(Type ownerType)
		{
			return AddOwner(ownerType, _defaultMetadata);
		}

		public DependencyProperty AddOwner(Type ownerType, PropertyMetadata defaultMetadata)
		{
			return new DependencyProperty(this, defaultMetadata);
		}
			
		public static DependencyProperty FromName(string name, Type ownerType)
		{
			if(ownerType == typeof(MediaPortal.GUI.Library.GUIControl))
				return (DependencyProperty)_properties[name + typeof(MediaPortal.Controls.FrameworkElement)];

			return (DependencyProperty)_properties[name + ownerType];
		}

		public override int GetHashCode()
		{
			return _globalIndex;
		}

		public PropertyMetadata GetMetadata(DependencyObject d)
		{
			throw new NotImplementedException();
		}

		public PropertyMetadata GetMetadata(DependencyObjectType type)
		{
			throw new NotImplementedException();
		}

		public PropertyMetadata GetMetadata(Type ownerType)
		{
			DependencyProperty dp = (DependencyProperty)_properties[_name + ownerType];
			
			if(dp == null)
				return null;
			
			return dp._defaultMetadata;
		}

		public bool IsValidType(object value)
		{
			return _propertyType.IsInstanceOfType(value);
		}

		public bool IsValidValue(object value)
		{
			if(value == UnsetValue)
				return false;

			if(_validateValueCallback == null)
				return true;

			return _validateValueCallback(value);
		}

		public void OverrideMetadata(Type ownerType, PropertyMetadata defaultMetadata)
		{
			OverrideMetadata(ownerType, defaultMetadata, null);
		}

		public void OverrideMetadata(Type ownerType, PropertyMetadata defaultMetadata, DependencyPropertyKey key)
		{
			DependencyProperty dp = (DependencyProperty)_properties[_name + ownerType];
			
			if(dp == null)
				return;

			dp._defaultMetadata = defaultMetadata;

			// PropertyMetadata.Merge
		}
		
		public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
		{
			return DependencyProperty.Register(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return DependencyProperty.Register(name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			return new DependencyProperty(name, propertyType, ownerType, defaultMetadata, validateValueCallback);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
		{
			return DependencyProperty.RegisterAttached(name, propertyType, ownerType, null, null);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return DependencyProperty.RegisterAttached(name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			// TODO: What should differ for attached properties???
			return Register(name, propertyType, ownerType, defaultMetadata, validateValueCallback);
		}

		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return DependencyProperty.RegisterAttachedReadOnly(name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyPropertyKey RegisterAttachedReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			throw new NotImplementedException();
		}

		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata)
		{
			return DependencyProperty.RegisterReadOnly(name, propertyType, ownerType, defaultMetadata, null);
		}

		public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, ValidateValueCallback validateValueCallback)
		{
			return new DependencyPropertyKey(name, propertyType, ownerType, defaultMetadata, validateValueCallback);
		}

		#endregion Methods

		#region Properties

		public PropertyMetadata DefaultMetadata
		{
			get { return _defaultMetadata; }
		}

		public int GlobalIndex
		{
			get { return _globalIndex; }
		}

		public string Name
		{
			get { return _name; }
		}

		public Type OwnerType
		{
			get { return _ownerType; }
		}

		public Type PropertyType
		{
			get { return _propertyType; }
		}

		public ValidateValueCallback ValidateValueCallback
		{
			get { return _validateValueCallback; }
		}

		#endregion Properties

		#region Fields

		PropertyMetadata			_defaultMetadata = null;
		readonly int				_globalIndex = _globalIndexNext++;
		static int					_globalIndexNext = 0;
		string						_name = string.Empty;
		Type						_ownerType = null;
		static Hashtable			_properties = new Hashtable(100);
		static Hashtable			_propertiesReadOnly = new Hashtable(100);
		static Hashtable			_propertiesAttached = new Hashtable(100);
		Hashtable					_metadata = new Hashtable();
		Type						_propertyType = null;
		ValidateValueCallback		_validateValueCallback = null;

		public static readonly object UnsetValue = new object();

		#endregion Fields
	}
}
