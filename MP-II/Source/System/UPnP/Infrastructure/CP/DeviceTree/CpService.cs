using System.Collections.Generic;
using System.Xml;
using MediaPortal.Utilities.Exceptions;
using UPnP.Infrastructure.Common;

namespace UPnP.Infrastructure.CP.DeviceTree
{
  /// <summary>
  /// Delegate which gets called when an action is invoked.
  /// </summary>
  /// <param name="action">Action instance which was invoked.</param>
  /// <param name="inParams">Parameters of the action invocation.</param>
  /// <param name="state">State which will can be used to match the async result or error to this invocation.</param>
  public delegate void ActionCalledDlgt(CpAction action, IList<object> inParams, object state);

  /// <summary>
  /// Delegate which gets called when an action result is available from a device.
  /// </summary>
  /// <param name="action">Action instance which was invoked.</param>
  /// <param name="outParams">Output parameters from the actions. The output parameters will match the data types described in
  /// <see cref="CpAction.OutArguments"/>.</param>
  /// <param name="handle">Call handle which was provided in the action invocation.</param>
  public delegate void ActionResultDlgt(CpAction action, IList<object> outParams, object handle);

  /// <summary>
  /// Delegate which gets called when an action invocation returned with an error.
  /// </summary>
  /// <param name="action">Action instance which was invoked.</param>
  /// <param name="error">Returned UPnP error.</param>
  /// <param name="handle">Call handle which was provided in the action invocation.</param>
  public delegate void ActionErrorResultDlgt(CpAction action, UPnPError error, object handle);

  /// <summary>
  /// Delegate which gets called when the service wants to subscribe to or unsubscribe from state variable changes.
  /// </summary>
  /// <param name="service">UPnP service which wants to subscribe to or unsubscribe from state variable changes.</param>
  public delegate void ChangeStateVariablesSubscribtionDlgt(CpService service);

  /// <summary>
  /// Delegate which gets called when an event subscription to a service wasn't successful.
  /// </summary>
  /// <param name="service">The service for that the event subscription didn't succeed.</param>
  /// <param name="error">A UPnP error code and error description.</param>
  public delegate void EventSubscriptionFailedDlgt(CpService service, UPnPError error);

  /// <summary>
  /// Delegate to be used for the client side state variable change event.
  /// </summary>
  /// <param name="stateVariable">State variable which was changed.</param>
  public delegate void StateVariableChangedDlgt(CpStateVariable stateVariable);

  /// <summary>
  /// UPnP service template which gets instantiated at the client (control point) side for each service
  /// the control point wants to connect to.
  /// </summary>
  /// <remarks>
  /// Parts of this class are intentionally parallel to the implementation in <see cref="UPnP.Infrastructure.Dv.DeviceTree.DvService"/>.
  /// </remarks>
  public class CpService
  {
    protected CpDevice _parentDevice;
    protected string _serviceType;
    protected int _serviceTypeVersion;
    protected string _serviceId;
    protected ActionResultDlgt _actionResult;
    protected ActionErrorResultDlgt _actionErrorResult;
    protected EventSubscriptionFailedDlgt _eventSubscriptionFailed;
    protected IDictionary<string, CpAction> _actions = new Dictionary<string, CpAction>();
    protected IDictionary<string, CpStateVariable> _stateVariables = new Dictionary<string, CpStateVariable>();
    protected bool _isOptional = true;
    protected DeviceConnection _connection = null;
    
    /// <summary>
    /// Creates a new UPnP service instance at the client (control point) side.
    /// </summary>
    /// <param name="connection">Device connection instance which attends the connection with the server side.</param>
    /// <param name="parentDevice">Instance of the device which contains the new service.</param>
    /// <param name="serviceType">Type of the service instance, in the format "schemas-upnp-org:service:[service-type]" or
    /// "vendor-domain:service:[service-type]". Note that in vendor-defined types, all dots in the vendors domain are
    /// replaced by hyphens.</param>
    /// <param name="serviceTypeVersion">Version of the implemented service type.</param>
    /// <param name="serviceId">Service id in the format "urn:upnp-org:serviceId:[service-id]" (for standard services) or
    /// "urn:domain-name:serviceId:[service-id]" (for vendor-defined service types).</param>
    public CpService(DeviceConnection connection, CpDevice parentDevice, string serviceType, int serviceTypeVersion, string serviceId)
    {
      _connection = connection;
      _parentDevice = parentDevice;
      _serviceType = serviceType;
      _serviceTypeVersion = serviceTypeVersion;
      _serviceId = serviceId;
    }

    /// <summary>
    /// Gets or sets the delegate function which will be called when an action result is available.
    /// Has to be set for concrete service implementations.
    /// </summary>
    public ActionResultDlgt ActionResult
    {
      get { return _actionResult; }
      set { _actionResult = value; }
    }

    /// <summary>
    /// Gets or sets the delegate function which will be called when an action error result is available.
    /// Has to be set for concrete service implementations.
    /// </summary>
    public ActionErrorResultDlgt ActionErrorResult
    {
      get { return _actionErrorResult; }
      set { _actionErrorResult = value; }
    }

    /// <summary>
    /// Gets invoked when one of the state variables of this service has changed. Can be set for concrete service
    /// implementations.
    /// </summary>
    public event StateVariableChangedDlgt StateVariableChanged;

    /// <summary>
    /// Gets or sets a flag which controls the control point's matching behaviour.
    /// If <see cref="IsOptional"/> is set to <c>true</c>, the control point will also return devices from the network
    /// which don't implement this service. If this flag is set to <c>false</c>, devices without a service matching this
    /// service template won't be considered as matching devices.
    /// </summary>
    public bool IsOptional
    {
      get { return _isOptional; }
      set { _isOptional = value; }
    }

    /// <summary>
    /// Returns the information if this service template is connected to a matching UPnP service. Will be set by the UPnP system.
    /// </summary>
    public bool IsConnected
    {
      get { return _connection != null; }
    }

    /// <summary>
    /// Returns the device which contains this service.
    /// </summary>
    public CpDevice ParentDevice
    {
      get { return _parentDevice; }
    }

    /// <summary>
    /// Returns the full qualified name of this service in the form "[DeviceName].[ServiceType]:[ServiceVersion]".
    /// </summary>
    public string FullQualifiedName
    {
      get { return _parentDevice.FullQualifiedName + "." + _serviceType + ":" + _serviceTypeVersion; }
    }

    /// <summary>
    /// Returns the service type, in the format "schemas-upnp-org:service:[service-type]" or
    /// "vendor-domain:service:[service-type]".
    /// </summary>
    public string ServiceType
    {
      get { return _serviceType; }
    }
  
    /// <summary>
    /// Returns the version of the type of this service.
    /// </summary>
    public int ServiceTypeVersion
    {
      get { return _serviceTypeVersion; }
    }

    /// <summary>
    /// Returns the service type URN with version, in the format "urn:schemas-upnp-org:service:[service-type]:[version]".
    /// </summary>
    public string ServiceTypeVersion_URN
    {
      get { return "urn:" + _serviceType + ":" + _serviceTypeVersion; }
    }

    /// <summary>
    /// Returns the service id, in the format "urn:upnp-org:serviceId:[service-id]" (for standard services) or
    /// "urn:domain-name:serviceId:[service-id]".
    /// The service id is specified by the UPnP Forum working committee or the UPnP vendor for the service type.
    /// </summary>
    public string ServiceId
    {
      get { return _serviceId; }
    }

    /// <summary>
    /// Returns a dictionary which maps action names to actions.
    /// </summary>
    public IDictionary<string, CpAction> Actions
    {
      get { return _actions; }
    }

    /// <summary>
    /// Returns a dictionary which maps state variable names to state variables.
    /// </summary>
    public IDictionary<string, CpStateVariable> StateVariables
    {
      get { return _stateVariables; }
    }

    /// <summary>
    /// Returns <c>true</c> if at least one state variable of this service is of an extended data type and doesn't support
    /// a string equivalent.
    /// </summary>
    public bool HasComplexStateVariables
    {
      get
      {
        foreach (CpStateVariable stateVariable in _stateVariables.Values)
        {
          CpExtendedDataType edt = stateVariable.DataType as CpExtendedDataType;
          if (edt != null && !edt.SupportsStringEquivalent)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Returns the information whether the state variables are subscribed for change events.
    /// See <see cref="DeviceConnection.IsServiceSubscribedForEvents"/> for more hints.
    /// </summary>
    public bool IsStateVariablesSubscribed
    {
      get
      {
        DeviceConnection connection = _connection;
        return connection != null && connection.IsServiceSubscribedForEvents(this);
      }
    }

    /// <summary>
    /// Returns the information if this service is compatible with the specified service <paramref name="type"/> and
    /// <paramref name="version"/>. A given <paramref name="type"/> and <paramref name="version"/> combination is compatible
    /// if the given <paramref name="type"/> matches exactly the <see cref="ServiceType"/> and the given
    /// <paramref name="version"/> is equal or higher than this service's <see cref="ServiceTypeVersion"/>.
    /// </summary>
    /// <param name="type">Type of the service to check.</param>
    /// <param name="version">Version of the service to check.</param>
    /// <returns><c>true</c>, if the specified <paramref name="type"/> is equal to our <see cref="ServiceType"/> and
    /// the specified <paramref name="version"/> is equal or higher than our <see cref="ServiceTypeVersion"/>, else
    /// <c>false</c>.</returns>
    public bool IsCompatible(string type, int version)
    {
      return _serviceType == type && _serviceTypeVersion >= version;
    }

    /// <summary>
    /// Subscribes for change events for all state variables of this service.
    /// </summary>
    /// <exception cref="IllegalCallException">If the state variables are already subscribed
    /// (see <see cref="IsStateVariablesSubscribed"/>).</exception>
    public void SubscribeStateVariables()
    {
      DeviceConnection connection = _connection;
      if (connection == null)
        throw new IllegalCallException("UPnP service is not connected to a UPnP network service");
      if (connection.IsServiceSubscribedForEvents(this))
        throw new IllegalCallException("State variables are already subscribed");
      connection.OnSubscribeEvents(this);
    }

    /// <summary>
    /// Unsubscribes change events for all state variables of this service.
    /// </summary>
    /// <exception cref="IllegalCallException">If the state variables are not subscribed
    /// (see <see cref="IsStateVariablesSubscribed"/>).</exception>
    public void UnsubscribeStateVariables()
    {
      DeviceConnection connection = _connection;
      if (connection == null)
        throw new IllegalCallException("UPnP service is not connected to a UPnP network service");
      if (!connection.IsServiceSubscribedForEvents(this))
        throw new IllegalCallException("State variables are not subscribed");
      connection.OnUnsubscribeEvents(this);
    }

    internal void InvokeAction_Async(CpAction action, IList<object> inParameters, object state)
    {
      DeviceConnection connection = _connection;
      if (connection == null)
        throw new IllegalCallException("UPnP service is not connected to a UPnP network service");
      connection.OnActionCalled(action, inParameters, state);
    }

    internal void InvokeActionResult(CpAction action, IList<object> outParams, object handle)
    {
      if (_actionResult != null)
        _actionResult(action, outParams, handle);
    }

    internal void InvokeActionErrorResult(CpAction action, UPnPError error, object handle)
    {
      if (_actionErrorResult != null)
        _actionErrorResult(action, error, handle);
    }

    internal void InvokeStateVariableChanged(CpStateVariable variable)
    {
      StateVariableChangedDlgt stateVariableChanged = StateVariableChanged;
      if (stateVariableChanged != null)
        stateVariableChanged(variable);
    }

    internal void InvokeEventSubscriptionFailed(UPnPError error)
    {
      if (_eventSubscriptionFailed != null)
        _eventSubscriptionFailed(this, error);
    }

    #region Connection

    /// <summary>
    /// Adds the specified <paramref name="action"/> instance to match to this service template.
    /// </summary>
    /// <param name="action">Action template to be added.</param>
    internal void AddAction(CpAction action)
    {
      _actions.Add(action.Name, action);
    }

    /// <summary>
    /// Adds the specified state <paramref name="variable"/> instance to match to this service template.
    /// </summary>
    /// <param name="variable">UPnP state variable to add.</param>
    internal void AddStateVariable(CpStateVariable variable)
    {
      _stateVariables.Add(variable.Name, variable);
    }

    internal static CpService ConnectService(DeviceConnection connection, CpDevice parentDevice,
        ServiceDescriptor serviceDescriptor, DataTypeResolverDlgt dataTypeResolver)
    {
      lock (connection.CPData.SyncObj)
      {
        CpService result = new CpService(connection, parentDevice, serviceDescriptor.ServiceType, serviceDescriptor.ServiceTypeVersion,
            serviceDescriptor.ServiceId);
        // State variables must be connected first because they are needed from the action's arguments
        foreach (XmlElement stateVariableElement in serviceDescriptor.ServiceDescription.DocumentElement.SelectNodes("serviceStateTable/stateVariable"))
          result.AddStateVariable(CpStateVariable.ConnectStateVariable(connection, result, stateVariableElement, dataTypeResolver));
        foreach (XmlElement actionElement in serviceDescriptor.ServiceDescription.DocumentElement.SelectNodes("actionList/action"))
          result.AddAction(CpAction.ConnectAction(connection, result, actionElement));
        return result;
      }
    }

    internal void Disconnect()
    {
      DeviceConnection connection = _connection;
      if (connection == null)
        return;
      lock (connection.CPData.SyncObj)
        _connection = null;
    }

    #endregion
  }
}
