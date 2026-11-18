namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Manages all <see cref="Kaizen.InputEventAction"/> which can be created/modified from the project settings menu <b>Project &gt; Project Settings &gt; Input Map</b> or in code with <see cref="Kaizen.InputMapInstance.AddAction(StringName, float)"/> and <see cref="Kaizen.InputMapInstance.ActionAddEvent(StringName, InputEvent)"/>. See <see cref="Kaizen.Node._Input(InputEvent)"/>.</para>
/// </summary>
[KaizenClassName("InputMap")]
public partial class InputMapInstance : KaizenObject
{
    private static readonly System.Type CachedType = typeof(InputMapInstance);

    private static readonly StringName NativeName = "InputMap";

    internal InputMapInstance() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal InputMapInstance(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal InputMapInstance(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasAction, 2619796661ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.InputMap"/> has a registered action with the given name.</para>
    /// </summary>
    public bool HasAction(StringName action)
    {
        bool ret = NativeCalls.kaizen_icall_1_129(MethodBind0, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActions, 2915620761ul);

    /// <summary>
    /// <para>Returns an array of all actions in the <see cref="Kaizen.InputMap"/>.</para>
    /// </summary>
    public Kaizen.Collections.Array<StringName> GetActions()
    {
        Kaizen.Collections.Array<StringName> ret = new Kaizen.Collections.Array<StringName>(NativeCalls.kaizen_icall_0_146(MethodBind1, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddAction, 1195233573ul);

    /// <summary>
    /// <para>Adds an empty action to the <see cref="Kaizen.InputMap"/> with a configurable <paramref name="deadzone"/>.</para>
    /// <para>An <see cref="Kaizen.InputEvent"/> can then be added to this action with <see cref="Kaizen.InputMapInstance.ActionAddEvent(StringName, InputEvent)"/>.</para>
    /// </summary>
    public void AddAction(StringName action, float deadzone = 0.20000000298023f)
    {
        NativeCalls.kaizen_icall_2_789(MethodBind2, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), deadzone);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.EraseAction, 3304788590ul);

    /// <summary>
    /// <para>Removes an action from the <see cref="Kaizen.InputMap"/>.</para>
    /// </summary>
    public void EraseAction(StringName action)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind3, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActionDescription, 957595536ul);

    /// <summary>
    /// <para>Returns the human-readable description of the given action.</para>
    /// </summary>
    public string GetActionDescription(StringName action)
    {
        string ret = NativeCalls.kaizen_icall_1_793(MethodBind4, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionSetDeadzone, 4135858297ul);

    /// <summary>
    /// <para>Sets a deadzone value for the action.</para>
    /// </summary>
    public void ActionSetDeadzone(StringName action, float deadzone)
    {
        NativeCalls.kaizen_icall_2_789(MethodBind5, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), deadzone);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionGetDeadzone, 1391627649ul);

    /// <summary>
    /// <para>Returns a deadzone value for the action.</para>
    /// </summary>
    public float ActionGetDeadzone(StringName action)
    {
        float ret = NativeCalls.kaizen_icall_1_794(MethodBind6, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionAddEvent, 518302593ul);

    /// <summary>
    /// <para>Adds an <see cref="Kaizen.InputEvent"/> to an action. This <see cref="Kaizen.InputEvent"/> will trigger the action.</para>
    /// </summary>
    public void ActionAddEvent(StringName action, InputEvent @event)
    {
        NativeCalls.kaizen_icall_2_186(MethodBind7, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), KaizenObject.GetPtr(@event));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionHasEvent, 1185871985ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the action has the given <see cref="Kaizen.InputEvent"/> associated with it.</para>
    /// </summary>
    public bool ActionHasEvent(StringName action, InputEvent @event)
    {
        bool ret = NativeCalls.kaizen_icall_2_795(MethodBind8, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), KaizenObject.GetPtr(@event)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionEraseEvent, 518302593ul);

    /// <summary>
    /// <para>Removes an <see cref="Kaizen.InputEvent"/> from an action.</para>
    /// </summary>
    public void ActionEraseEvent(StringName action, InputEvent @event)
    {
        NativeCalls.kaizen_icall_2_186(MethodBind9, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), KaizenObject.GetPtr(@event));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionEraseEvents, 3304788590ul);

    /// <summary>
    /// <para>Removes all events from an action.</para>
    /// </summary>
    public void ActionEraseEvents(StringName action)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind10, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ActionGetEvents, 689397652ul);

    /// <summary>
    /// <para>Returns an array of <see cref="Kaizen.InputEvent"/>s associated with a given action.</para>
    /// <para><b>Note:</b> When used in the editor (e.g. a tool script or <c>EditorPlugin</c>), this method will return events for the editor action. If you want to access your project's input binds from the editor, read the <c>input/*</c> settings from <see cref="Kaizen.ProjectSettings"/>.</para>
    /// </summary>
    public Kaizen.Collections.Array<InputEvent> ActionGetEvents(StringName action)
    {
        Kaizen.Collections.Array<InputEvent> ret = new Kaizen.Collections.Array<InputEvent>(NativeCalls.kaizen_icall_1_726(MethodBind11, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.EventIsAction, 3193353650ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given event is part of an existing action. This method ignores keyboard modifiers if the given <see cref="Kaizen.InputEvent"/> is not pressed (for proper release detection). See <see cref="Kaizen.InputMapInstance.ActionHasEvent(StringName, InputEvent)"/> if you don't want this behavior.</para>
    /// <para>If <paramref name="exactMatch"/> is <see langword="false"/>, it ignores additional input modifiers for <see cref="Kaizen.InputEventKey"/> and <see cref="Kaizen.InputEventMouseButton"/> events, and the direction for <see cref="Kaizen.InputEventJoypadMotion"/> events.</para>
    /// </summary>
    public bool EventIsAction(InputEvent @event, StringName action, bool exactMatch = false)
    {
        bool ret = NativeCalls.kaizen_icall_3_796(MethodBind12, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@event), (kaizen_string_name)(action?.NativeValue ?? default), exactMatch.ToKaizenBool()).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.LoadFromProjectSettings, 3218959716ul);

    /// <summary>
    /// <para>Clears all <see cref="Kaizen.InputEventAction"/> in the <see cref="Kaizen.InputMap"/> and load it anew from <see cref="Kaizen.ProjectSettings"/>.</para>
    /// </summary>
    public void LoadFromProjectSettings()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    /// <summary>
    /// <para>Emitted when the <see cref="Kaizen.ProjectSettings"/> <see cref="Kaizen.InputMap"/> has been loaded.</para>
    /// </summary>
    public event Action ProjectSettingsLoaded
    {
        add => Connect(SignalName.ProjectSettingsLoaded, Callable.From(value));
        remove => Disconnect(SignalName.ProjectSettingsLoaded, Callable.From(value));
    }

    protected void EmitSignalProjectSettingsLoaded()
    {
        EmitSignal(SignalName.ProjectSettingsLoaded);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_project_settings_loaded = "ProjectSettingsLoaded";

    /// <summary>
    /// Invokes the method with the given name, using the given arguments.
    /// This method is used by Kaizen to invoke methods from the engine side.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="method">Name of the method to invoke.</param>
    /// <param name="args">Arguments to use with the invoked method.</param>
    /// <param name="ret">Value returned by the invoked method.</param>
#pragma warning disable CS0618 // Member is obsolete
    protected internal override bool InvokeKaizenClassMethod(in kaizen_string_name method, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        return base.InvokeKaizenClassMethod(method, args, out ret);
    }
#pragma warning restore CS0618

    /// <summary>
    /// Check if the type contains a method with the given name.
    /// This method is used by Kaizen to check if a method exists before invoking it.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="method">Name of the method to check for.</param>

    protected internal override bool HasKaizenClassMethod(in kaizen_string_name method)
    {
        return base.HasKaizenClassMethod(method);
    }

    /// <summary>
    /// Check if the type contains a signal with the given name.
    /// This method is used by Kaizen to check if a signal exists before raising it.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="signal">Name of the signal to check for.</param>

    protected internal override bool HasKaizenClassSignal(in kaizen_string_name signal)
    {
        if (signal == SignalName.ProjectSettingsLoaded)
        {
            if (HasKaizenClassSignal(SignalProxyName_project_settings_loaded.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : KaizenObject.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
        /// <summary>
        /// Cached name for the 'has_action' method.
        /// </summary>
        public static readonly StringName HasAction = "has_action";
        /// <summary>
        /// Cached name for the 'get_actions' method.
        /// </summary>
        public static readonly StringName GetActions = "get_actions";
        /// <summary>
        /// Cached name for the 'add_action' method.
        /// </summary>
        public static readonly StringName AddAction = "add_action";
        /// <summary>
        /// Cached name for the 'erase_action' method.
        /// </summary>
        public static readonly StringName EraseAction = "erase_action";
        /// <summary>
        /// Cached name for the 'get_action_description' method.
        /// </summary>
        public static readonly StringName GetActionDescription = "get_action_description";
        /// <summary>
        /// Cached name for the 'action_set_deadzone' method.
        /// </summary>
        public static readonly StringName ActionSetDeadzone = "action_set_deadzone";
        /// <summary>
        /// Cached name for the 'action_get_deadzone' method.
        /// </summary>
        public static readonly StringName ActionGetDeadzone = "action_get_deadzone";
        /// <summary>
        /// Cached name for the 'action_add_event' method.
        /// </summary>
        public static readonly StringName ActionAddEvent = "action_add_event";
        /// <summary>
        /// Cached name for the 'action_has_event' method.
        /// </summary>
        public static readonly StringName ActionHasEvent = "action_has_event";
        /// <summary>
        /// Cached name for the 'action_erase_event' method.
        /// </summary>
        public static readonly StringName ActionEraseEvent = "action_erase_event";
        /// <summary>
        /// Cached name for the 'action_erase_events' method.
        /// </summary>
        public static readonly StringName ActionEraseEvents = "action_erase_events";
        /// <summary>
        /// Cached name for the 'action_get_events' method.
        /// </summary>
        public static readonly StringName ActionGetEvents = "action_get_events";
        /// <summary>
        /// Cached name for the 'event_is_action' method.
        /// </summary>
        public static readonly StringName EventIsAction = "event_is_action";
        /// <summary>
        /// Cached name for the 'load_from_project_settings' method.
        /// </summary>
        public static readonly StringName LoadFromProjectSettings = "load_from_project_settings";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
        /// <summary>
        /// Cached name for the 'project_settings_loaded' signal.
        /// </summary>
        public static readonly StringName ProjectSettingsLoaded = "project_settings_loaded";
    }
}
