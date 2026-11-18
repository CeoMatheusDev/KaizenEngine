namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Abstract base class of all types of input events. See <see cref="Kaizen.Node._Input(InputEvent)"/>.</para>
/// </summary>
public partial class InputEvent : Resource
{
    /// <summary>
    /// <para>Device ID used for emulated mouse input from a touchscreen, or for emulated touch input from a mouse. This can be used to distinguish emulated mouse input from physical mouse input, or emulated touch input from physical touch input.</para>
    /// </summary>
    public const long DeviceIdEmulation = -1;
    /// <summary>
    /// <para>Device ID used for input from a keyboard. This can be used to distinguish keyboard input events from joypad input events.</para>
    /// </summary>
    public const long DeviceIdKeyboard = 16;
    /// <summary>
    /// <para>Device ID used for input from a mouse. This can be used to distinguish mouse input events from joypad input events.</para>
    /// </summary>
    public const long DeviceIdMouse = 32;

    /// <summary>
    /// <para>The event's device ID.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.InputEvent.Device"/> can be negative for special use cases that don't refer to devices physically present on the system. See <see cref="Kaizen.InputEvent.DeviceIdEmulation"/>.</para>
    /// </summary>
    public int Device
    {
        get
        {
            return GetDevice();
        }
        set
        {
            SetDevice(value);
        }
    }

    private static readonly System.Type CachedType = typeof(InputEvent);

    private static readonly StringName NativeName = "InputEvent";

    internal InputEvent() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal InputEvent(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal InputEvent(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDevice, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDevice(int device)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), device);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDevice, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetDevice()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAction, 1558498928ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this input event matches a pre-defined action of any type.</para>
    /// <para>If <paramref name="exactMatch"/> is <see langword="false"/>, it ignores additional input modifiers for <see cref="Kaizen.InputEventKey"/> and <see cref="Kaizen.InputEventMouseButton"/> events, and the direction for <see cref="Kaizen.InputEventJoypadMotion"/> events.</para>
    /// </summary>
    public bool IsAction(StringName action, bool exactMatch = false)
    {
        bool ret = NativeCalls.kaizen_icall_2_782(MethodBind2, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), exactMatch.ToKaizenBool()).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsActionPressed, 1631499404ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given action matches this event and is being pressed (and is not an echo event for <see cref="Kaizen.InputEventKey"/> events, unless <paramref name="allowEcho"/> is <see langword="true"/>). Not relevant for events of type <see cref="Kaizen.InputEventMouseMotion"/> or <see cref="Kaizen.InputEventScreenDrag"/>.</para>
    /// <para>If <paramref name="exactMatch"/> is <see langword="false"/>, it ignores additional input modifiers for <see cref="Kaizen.InputEventKey"/> and <see cref="Kaizen.InputEventMouseButton"/> events, and the direction for <see cref="Kaizen.InputEventJoypadMotion"/> events.</para>
    /// <para><b>Note:</b> Due to keyboard ghosting, <see cref="Kaizen.InputEvent.IsActionPressed(StringName, bool, bool)"/> may return <see langword="false"/> even if one of the action's keys is pressed. See <a href="$DOCS_URL/tutorials/inputs/input_examples.html#keyboard-events">Input examples</a> in the documentation for more information.</para>
    /// </summary>
    public bool IsActionPressed(StringName action, bool allowEcho = false, bool exactMatch = false)
    {
        bool ret = NativeCalls.kaizen_icall_3_790(MethodBind3, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), allowEcho.ToKaizenBool(), exactMatch.ToKaizenBool()).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsActionReleased, 1558498928ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given action matches this event and is released (i.e. not pressed). Not relevant for events of type <see cref="Kaizen.InputEventMouseMotion"/> or <see cref="Kaizen.InputEventScreenDrag"/>.</para>
    /// <para>If <paramref name="exactMatch"/> is <see langword="false"/>, it ignores additional input modifiers for <see cref="Kaizen.InputEventKey"/> and <see cref="Kaizen.InputEventMouseButton"/> events, and the direction for <see cref="Kaizen.InputEventJoypadMotion"/> events.</para>
    /// </summary>
    public bool IsActionReleased(StringName action, bool exactMatch = false)
    {
        bool ret = NativeCalls.kaizen_icall_2_782(MethodBind4, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), exactMatch.ToKaizenBool()).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActionStrength, 801543509ul);

    /// <summary>
    /// <para>Returns a value between <c>0.0</c> and <c>1.0</c> depending on the given action's state. Useful for getting the value of events of type <see cref="Kaizen.InputEventJoypadMotion"/>.</para>
    /// <para>If <paramref name="exactMatch"/> is <see langword="false"/>, it ignores additional input modifiers for <see cref="Kaizen.InputEventKey"/> and <see cref="Kaizen.InputEventMouseButton"/> events, and the direction for <see cref="Kaizen.InputEventJoypadMotion"/> events.</para>
    /// </summary>
    public float GetActionStrength(StringName action, bool exactMatch = false)
    {
        float ret = NativeCalls.kaizen_icall_2_784(MethodBind5, KaizenObject.GetPtr(this), (kaizen_string_name)(action?.NativeValue ?? default), exactMatch.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCanceled, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this input event has been canceled.</para>
    /// </summary>
    public bool IsCanceled()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind6, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsPressed, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this input event is pressed. Not relevant for events of type <see cref="Kaizen.InputEventMouseMotion"/> or <see cref="Kaizen.InputEventScreenDrag"/>.</para>
    /// <para><b>Note:</b> Due to keyboard ghosting, <see cref="Kaizen.InputEvent.IsPressed()"/> may return <see langword="false"/> even if one of the action's keys is pressed. See <a href="$DOCS_URL/tutorials/inputs/input_examples.html#keyboard-events">Input examples</a> in the documentation for more information.</para>
    /// </summary>
    public bool IsPressed()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind7, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsReleased, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this input event is released. Not relevant for events of type <see cref="Kaizen.InputEventMouseMotion"/> or <see cref="Kaizen.InputEventScreenDrag"/>.</para>
    /// </summary>
    public bool IsReleased()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind8, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsEcho, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this input event is an echo event (only for events of type <see cref="Kaizen.InputEventKey"/>). An echo event is a repeated key event sent when the user is holding down the key. Any other event type returns <see langword="false"/>.</para>
    /// <para><b>Note:</b> The rate at which echo events are sent is typically around 20 events per second (after holding down the key for roughly half a second). However, the key repeat delay/speed can be changed by the user or disabled entirely in the operating system settings. To ensure your project works correctly on all configurations, do not assume the user has a specific key repeat configuration in your project's behavior.</para>
    /// </summary>
    public bool IsEcho()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind9, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AsText, 201670096ul);

    /// <summary>
    /// <para>Returns a <see cref="string"/> representation of the event.</para>
    /// </summary>
    public string AsText()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsMatch, 1754951977ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the specified <paramref name="event"/> matches this event. Only valid for action events, which include key (<see cref="Kaizen.InputEventKey"/>), button (<see cref="Kaizen.InputEventMouseButton"/> or <see cref="Kaizen.InputEventJoypadButton"/>), axis <see cref="Kaizen.InputEventJoypadMotion"/>, and action (<see cref="Kaizen.InputEventAction"/>) events.</para>
    /// <para>If <paramref name="exactMatch"/> is <see langword="false"/>, the check ignores additional input modifiers for <see cref="Kaizen.InputEventKey"/> and <see cref="Kaizen.InputEventMouseButton"/> events, and the direction for <see cref="Kaizen.InputEventJoypadMotion"/> events.</para>
    /// <para><b>Note:</b> This method only considers the event configuration (such as the keyboard key or the joypad axis), not state information like <see cref="Kaizen.InputEvent.IsPressed()"/>, <see cref="Kaizen.InputEvent.IsReleased()"/>, <see cref="Kaizen.InputEvent.IsEcho()"/>, or <see cref="Kaizen.InputEvent.IsCanceled()"/>.</para>
    /// </summary>
    public bool IsMatch(InputEvent @event, bool exactMatch = true)
    {
        bool ret = NativeCalls.kaizen_icall_2_791(MethodBind11, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@event), exactMatch.ToKaizenBool()).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsActionType, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this input event's type is one that can be assigned to an input action: <see cref="Kaizen.InputEventKey"/>, <see cref="Kaizen.InputEventMouseButton"/>, <see cref="Kaizen.InputEventJoypadButton"/>, <see cref="Kaizen.InputEventJoypadMotion"/>, <see cref="Kaizen.InputEventAction"/>. Returns <see langword="false"/> for all other input event types.</para>
    /// </summary>
    public bool IsActionType()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind12, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Accumulate, 1062211774ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given input event and this input event can be added together (only for events of type <see cref="Kaizen.InputEventMouseMotion"/>).</para>
    /// <para>The given input event's position, global position and speed will be copied. The resulting <c>relative</c> is a sum of both events. Both events' modifiers have to be identical.</para>
    /// </summary>
    public bool Accumulate(InputEvent withEvent)
    {
        bool ret = NativeCalls.kaizen_icall_1_202(MethodBind13, KaizenObject.GetPtr(this), KaizenObject.GetPtr(withEvent)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.XformedBy, 1282766827ul);

    /// <summary>
    /// <para>Returns a copy of the given input event which has been offset by <paramref name="localOfs"/> and transformed by <paramref name="xform"/>. Relevant for events of type <see cref="Kaizen.InputEventMouseButton"/>, <see cref="Kaizen.InputEventMouseMotion"/>, <see cref="Kaizen.InputEventScreenTouch"/>, <see cref="Kaizen.InputEventScreenDrag"/>, <see cref="Kaizen.InputEventMagnifyGesture"/> and <see cref="Kaizen.InputEventPanGesture"/>.</para>
    /// </summary>
    /// <param name="localOfs">If the parameter is null, then the default value is <c>new Vector2(0.0f, 0.0f)</c>.</param>
    public unsafe InputEvent XformedBy(Transform2D xform, Nullable<Vector2> localOfs = null)
    {
        Vector2 localOfsOrDefVal = localOfs.HasValue ? localOfs.Value : new Vector2(0.0f, 0.0f);
        InputEvent ret = (InputEvent)NativeCalls.kaizen_icall_2_792(MethodBind14, KaizenObject.GetPtr(this), &xform, &localOfsOrDefVal);
        global::System.GC.KeepAlive(this);
        return ret;
    }

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
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : Resource.PropertyName
    {
        /// <summary>
        /// Cached name for the 'device' property.
        /// </summary>
        public static readonly StringName Device = "device";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_device' method.
        /// </summary>
        public static readonly StringName SetDevice = "set_device";
        /// <summary>
        /// Cached name for the 'get_device' method.
        /// </summary>
        public static readonly StringName GetDevice = "get_device";
        /// <summary>
        /// Cached name for the 'is_action' method.
        /// </summary>
        public static readonly StringName IsAction = "is_action";
        /// <summary>
        /// Cached name for the 'is_action_pressed' method.
        /// </summary>
        public static readonly StringName IsActionPressed = "is_action_pressed";
        /// <summary>
        /// Cached name for the 'is_action_released' method.
        /// </summary>
        public static readonly StringName IsActionReleased = "is_action_released";
        /// <summary>
        /// Cached name for the 'get_action_strength' method.
        /// </summary>
        public static readonly StringName GetActionStrength = "get_action_strength";
        /// <summary>
        /// Cached name for the 'is_canceled' method.
        /// </summary>
        public static readonly StringName IsCanceled = "is_canceled";
        /// <summary>
        /// Cached name for the 'is_pressed' method.
        /// </summary>
        public static readonly StringName IsPressed = "is_pressed";
        /// <summary>
        /// Cached name for the 'is_released' method.
        /// </summary>
        public static readonly StringName IsReleased = "is_released";
        /// <summary>
        /// Cached name for the 'is_echo' method.
        /// </summary>
        public static readonly StringName IsEcho = "is_echo";
        /// <summary>
        /// Cached name for the 'as_text' method.
        /// </summary>
        public static readonly StringName AsText = "as_text";
        /// <summary>
        /// Cached name for the 'is_match' method.
        /// </summary>
        public static readonly StringName IsMatch = "is_match";
        /// <summary>
        /// Cached name for the 'is_action_type' method.
        /// </summary>
        public static readonly StringName IsActionType = "is_action_type";
        /// <summary>
        /// Cached name for the 'accumulate' method.
        /// </summary>
        public static readonly StringName Accumulate = "accumulate";
        /// <summary>
        /// Cached name for the 'xformed_by' method.
        /// </summary>
        public static readonly StringName XformedBy = "xformed_by";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
