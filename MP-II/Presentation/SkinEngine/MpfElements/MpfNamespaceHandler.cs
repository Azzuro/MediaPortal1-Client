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
using System.Collections.Generic;
using System.Reflection;
using Presentation.SkinEngine.General;
using Presentation.SkinEngine.General.Exceptions;
using Presentation.SkinEngine.XamlParser.Interfaces;
using Presentation.SkinEngine.MpfElements;

namespace Presentation.SkinEngine.MpfElements
{
  public class MpfNamespaceHandler: INamespaceHandler
  {
    #region Protected methods

    internal IDataDescriptor GetAttachedProperty(string propertyProvider,
        string propertyName, object targetObject)
    {
      return new MpfAttachedPropertyDataDescriptor(this, targetObject, propertyProvider, propertyName);
    }

    internal static MethodInfo GetAttachedPropertyGetter(string propertyProvider,
        string propertyName)
    {
      Type type = GetElementType(propertyProvider);
      return type.GetMethod("Get" + propertyName + "AttachedProperty",
        BindingFlags.Public | BindingFlags.Static);
    }

    internal static Type GetElementType(string typeName)
    {
      try
      {
        return Registration.ObjectClassRegistrations[typeName];
      }
      catch
      {
        throw new XamlParserException("Element type '{0}' is not present in MpfNamespaceHandler",
          typeName);
      }
    }

    internal static bool HasAttachedProperty(string propertyProvider,
        string propertyName, object targetObject)
    {
      return GetAttachedPropertyGetter(propertyProvider, propertyName) != null;
    }

    #endregion

    #region INamespaceHandler implementation

    public object InstantiateElement(IParserContext context, string typeName, string namespaceURI,
        IList<object> parameters)
    {
      try
      {
        Type t = GetElementType(typeName, namespaceURI);
        object[] parameterObjects = new object[parameters.Count];
        parameters.CopyTo(parameterObjects, 0);
        return Activator.CreateInstance(t, parameterObjects);
      }
      catch (Exception e)
      {
        if (e is XamlParserException)
          throw;
        throw new XamlParserException("Error creating element type '{0}' in namespace '{1}'",
          e, typeName, namespaceURI);
      }
    }

    public Type GetElementType(string typeName, string namespaceURI)
    {
      return GetElementType(typeName);
    }

    public bool HasAttachedProperty(string propertyProvider,
        string propertyName, object targetObject, string namespaceURI)
    {
      return GetAttachedPropertyGetter(propertyProvider, propertyName) != null;
    }

    public IDataDescriptor GetAttachedProperty(string propertyProvider,
      string propertyName, object targetObject, string namespaceURI)
    {
      MpfAttachedPropertyDataDescriptor result;
      if (!MpfAttachedPropertyDataDescriptor.CreateAttachedPropertyDataDescriptor(
          this, targetObject, propertyProvider, propertyName, out result))
        throw new InvalidOperationException(string.Format(
            "Attached property '{0}.{1}' is not available on target object '{2}'",
            propertyProvider, propertyName, targetObject));
      return result;
    }

    #endregion
  }
}
