namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>An input event for keys on a keyboard. Supports key presses, key releases and <see cref="Kaizen.InputEventKey.Echo"/> events. It can also be received in <see cref="Kaizen.Node._UnhandledKeyInput(InputEvent)"/>.</para>
/// <para><b>Note:</b> Events received from the keyboard usually have all properties set. Event mappings should have only one of the <see cref="Kaizen.InputEventKey.Keycode"/>, <see cref="Kaizen.InputEventKey.PhysicalKeycode"/> or <see cref="Kaizen.InputEventKey.Unicode"/> set.</para>
/// <para>When events are compared, properties are checked in the following priority - <see cref="Kaizen.InputEventKey.Keycode"/>, <see cref="Kaizen.InputEventKey.PhysicalKeycode"/> and <see cref="Kaizen.InputEventKey.Unicode"/>. Events with the first matching value will be considered equal.</para>
/// </summary>
public partial class InputEventKey : InputEventWithModifiers
{
    /// <summary>
    /// <para>If <see langword="true"/>, the key's state is pressed. If <see langword="false"/>, the key's state is released.</para>
    /// </summary>
    public bool Pressed
    {
        get
        {
            return IsPressed();
        }
        set
        {
            SetPressed(value);
        }
    }

    /// <summary>
    /// <para>Latin label printed on the key in the current keyboard layout, which corresponds to one of the <see cref="Kaizen.Key"/> constants. Key codes are meant for shortcuts expressed with a standard Latin keyboard, such as Ctrl + S for a "Save" shortcut.</para>
    /// <para>To get a human-readable representation of the <see cref="Kaizen.InputEventKey"/>, use <c>OS.get_keycode_string(event.keycode)</c> where <c>event</c> is the <see cref="Kaizen.InputEventKey"/>.</para>
    /// <para><code>
    /// ┌─────┐ ┌─────┐
    /// │ Q   │ │ Q   │ - "Q" - keycode
    /// │   Й │ │  ض │ - "Й" and "ض" - key_label
    /// └─────┘ └─────┘
    /// </code></para>
    /// </summary>
    public Key Keycode
    {
        get
        {
            return GetKeycode();
        }
        set
        {
            SetKeycode(value);
        }
    }

    /// <summary>
    /// <para>Represents the physical location of a key on the 101/102-key US QWERTY keyboard, which corresponds to one of the <see cref="Kaizen.Key"/> constants. Physical key codes meant for game input, such as WASD movement, where only the location of the keys is important.</para>
    /// <para>To get a human-readable representation of the <see cref="Kaizen.InputEventKey"/>, use <see cref="Kaizen.OS.GetKeycodeString(Key)"/> in combination with <see cref="Kaizen.DisplayServer.KeyboardGetKeycodeFromPhysical(Key)"/> or <see cref="Kaizen.DisplayServer.KeyboardGetLabelFromPhysical(Key)"/>:</para>
    /// <para><code>
    /// public override void _Input(InputEvent @event)
    /// {
    /// 	if (@event is InputEventKey inputEventKey)
    /// 	{
    /// 		var keycode = DisplayServer.KeyboardGetKeycodeFromPhysical(inputEventKey.PhysicalKeycode);
    /// 		var label = DisplayServer.KeyboardGetLabelFromPhysical(inputEventKey.PhysicalKeycode);
    /// 		GD.Print(OS.GetKeycodeString(keycode));
    /// 		GD.Print(OS.GetKeycodeString(label));
    /// 	}
    /// }
    /// </code></para>
    /// </summary>
    public Key PhysicalKeycode
    {
        get
        {
            return GetPhysicalKeycode();
        }
        set
        {
            SetPhysicalKeycode(value);
        }
    }

    /// <summary>
    /// <para>Represents the localized label printed on the key in the current keyboard layout, which corresponds to one of the <see cref="Kaizen.Key"/> constants or any valid Unicode character. Key labels are meant for key prompts.</para>
    /// <para>For keyboard layouts with a single label on the key, it is equivalent to <see cref="Kaizen.InputEventKey.Keycode"/>.</para>
    /// <para>To get a human-readable representation of the <see cref="Kaizen.InputEventKey"/>, use <c>OS.get_keycode_string(event.key_label)</c> where <c>event</c> is the <see cref="Kaizen.InputEventKey"/>.</para>
    /// <para><code>
    /// ┌─────┐ ┌─────┐
    /// │ Q   │ │ Q   │ - "Q" - keycode
    /// │   Й │ │  ض │ - "Й" and "ض" - key_label
    /// └─────┘ └─────┘
    /// </code></para>
    /// </summary>
    public Key KeyLabel
    {
        get
        {
            return GetKeyLabel();
        }
        set
        {
            SetKeyLabel(value);
        }
    }

    /// <summary>
    /// <para>The key Unicode character code (when relevant), shifted by modifier keys. Unicode character codes for composite characters and complex scripts may not be available unless IME input mode is active. See <see cref="Kaizen.Window.SetImeActive(bool)"/> for more information. Unicode character codes are meant for text input.</para>
    /// <para><b>Note:</b> This property is set by the engine only for a pressed event. If the event is sent by an IME or a virtual keyboard, no corresponding key released event is sent.</para>
    /// </summary>
    public long Unicode
    {
        get
        {
            return GetUnicode();
        }
        set
        {
            SetUnicode(value);
        }
    }

    /// <summary>
    /// <para>Represents the location of a key which has both left and right versions, such as Shift or Alt.</para>
    /// </summary>
    public KeyLocation Location
    {
        get
        {
            return GetLocation();
        }
        set
        {
            SetLocation(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the key was already pressed before this event. An echo event is a repeated key event sent when the user is holding down the key.</para>
    /// <para><b>Note:</b> The rate at which echo events are sent is typically around 20 events per second (after holding down the key for roughly half a second). However, the key repeat delay/speed can be changed by the user or disabled entirely in the operating system settings. To ensure your project works correctly on all configurations, do not assume the user has a specific key repeat configuration in your project's behavior.</para>
    /// </summary>
    public bool Echo
    {
        get
        {
            return IsEcho();
        }
        set
        {
            SetEcho(value);
        }
    }

    private static readonly System.Type CachedType = typeof(InputEventKey);

    private static readonly StringName NativeName = "InputEventKey";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public InputEventKey() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal InputEventKey(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal InputEventKey(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPressed, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPressed(bool pressed)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind0, KaizenObject.GetPtr(this), pressed.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetKeycode, 888074362ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetKeycode(Key keycode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind1, KaizenObject.GetPtr(this), (int)keycode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKeycode, 1585896689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Key GetKeycode()
    {
        Key ret = (Key)NativeCalls.kaizen_icall_0_39(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPhysicalKeycode, 888074362ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPhysicalKeycode(Key physicalKeycode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind3, KaizenObject.GetPtr(this), (int)physicalKeycode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPhysicalKeycode, 1585896689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Key GetPhysicalKeycode()
    {
        Key ret = (Key)NativeCalls.kaizen_icall_0_39(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetKeyLabel, 888074362ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetKeyLabel(Key keyLabel)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind5, KaizenObject.GetPtr(this), (int)keyLabel);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKeyLabel, 1585896689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Key GetKeyLabel()
    {
        Key ret = (Key)NativeCalls.kaizen_icall_0_39(MethodBind6, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUnicode, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUnicode(long unicode)
    {
        NativeCalls.kaizen_icall_1_10(MethodBind7, KaizenObject.GetPtr(this), unicode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUnicode, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public long GetUnicode()
    {
        long ret = NativeCalls.kaizen_icall_0_4(MethodBind8, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLocation, 634453155ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetLocation(KeyLocation location)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind9, KaizenObject.GetPtr(this), (int)location);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLocation, 211810873ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public KeyLocation GetLocation()
    {
        KeyLocation ret = (KeyLocation)NativeCalls.kaizen_icall_0_39(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEcho, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetEcho(bool echo)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind11, KaizenObject.GetPtr(this), echo.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKeycodeWithModifiers, 1585896689ul);

    /// <summary>
    /// <para>Returns the Latin keycode combined with modifier keys such as Shift or Alt. See also <see cref="Kaizen.InputEventWithModifiers"/>.</para>
    /// <para>To get a human-readable representation of the <see cref="Kaizen.InputEventKey"/> with modifiers, use <c>OS.get_keycode_string(event.get_keycode_with_modifiers())</c> where <c>event</c> is the <see cref="Kaizen.InputEventKey"/>.</para>
    /// </summary>
    public Key GetKeycodeWithModifiers()
    {
        Key ret = (Key)NativeCalls.kaizen_icall_0_39(MethodBind12, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPhysicalKeycodeWithModifiers, 1585896689ul);

    /// <summary>
    /// <para>Returns the physical keycode combined with modifier keys such as Shift or Alt. See also <see cref="Kaizen.InputEventWithModifiers"/>.</para>
    /// <para>To get a human-readable representation of the <see cref="Kaizen.InputEventKey"/> with modifiers, use <c>OS.get_keycode_string(event.get_physical_keycode_with_modifiers())</c> where <c>event</c> is the <see cref="Kaizen.InputEventKey"/>.</para>
    /// </summary>
    public Key GetPhysicalKeycodeWithModifiers()
    {
        Key ret = (Key)NativeCalls.kaizen_icall_0_39(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKeyLabelWithModifiers, 1585896689ul);

    /// <summary>
    /// <para>Returns the localized key label combined with modifier keys such as Shift or Alt. See also <see cref="Kaizen.InputEventWithModifiers"/>.</para>
    /// <para>To get a human-readable representation of the <see cref="Kaizen.InputEventKey"/> with modifiers, use <c>OS.get_keycode_string(event.get_key_label_with_modifiers())</c> where <c>event</c> is the <see cref="Kaizen.InputEventKey"/>.</para>
    /// </summary>
    public Key GetKeyLabelWithModifiers()
    {
        Key ret = (Key)NativeCalls.kaizen_icall_0_39(MethodBind14, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AsTextKeycode, 201670096ul);

    /// <summary>
    /// <para>Returns a <see cref="string"/> representation of the event's <see cref="Kaizen.InputEventKey.Keycode"/> and modifiers.</para>
    /// </summary>
    public string AsTextKeycode()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind15, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AsTextPhysicalKeycode, 201670096ul);

    /// <summary>
    /// <para>Returns a <see cref="string"/> representation of the event's <see cref="Kaizen.InputEventKey.PhysicalKeycode"/> and modifiers.</para>
    /// </summary>
    public string AsTextPhysicalKeycode()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind16, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AsTextKeyLabel, 201670096ul);

    /// <summary>
    /// <para>Returns a <see cref="string"/> representation of the event's <see cref="Kaizen.InputEventKey.KeyLabel"/> and modifiers.</para>
    /// </summary>
    public string AsTextKeyLabel()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind17, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AsTextLocation, 201670096ul);

    /// <summary>
    /// <para>Returns a <see cref="string"/> representation of the event's <see cref="Kaizen.InputEventKey.Location"/>. This will be a blank string if the event is not specific to a location.</para>
    /// </summary>
    public string AsTextLocation()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind18, KaizenObject.GetPtr(this));
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
    public new class PropertyName : InputEventWithModifiers.PropertyName
    {
        /// <summary>
        /// Cached name for the 'pressed' property.
        /// </summary>
        public static readonly StringName Pressed = "pressed";
        /// <summary>
        /// Cached name for the 'keycode' property.
        /// </summary>
        public static readonly StringName Keycode = "keycode";
        /// <summary>
        /// Cached name for the 'physical_keycode' property.
        /// </summary>
        public static readonly StringName PhysicalKeycode = "physical_keycode";
        /// <summary>
        /// Cached name for the 'key_label' property.
        /// </summary>
        public static readonly StringName KeyLabel = "key_label";
        /// <summary>
        /// Cached name for the 'unicode' property.
        /// </summary>
        public static readonly StringName Unicode = "unicode";
        /// <summary>
        /// Cached name for the 'location' property.
        /// </summary>
        public static readonly StringName Location = "location";
        /// <summary>
        /// Cached name for the 'echo' property.
        /// </summary>
        public static readonly StringName Echo = "echo";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : InputEventWithModifiers.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_pressed' method.
        /// </summary>
        public static readonly StringName SetPressed = "set_pressed";
        /// <summary>
        /// Cached name for the 'set_keycode' method.
        /// </summary>
        public static readonly StringName SetKeycode = "set_keycode";
        /// <summary>
        /// Cached name for the 'get_keycode' method.
        /// </summary>
        public static readonly StringName GetKeycode = "get_keycode";
        /// <summary>
        /// Cached name for the 'set_physical_keycode' method.
        /// </summary>
        public static readonly StringName SetPhysicalKeycode = "set_physical_keycode";
        /// <summary>
        /// Cached name for the 'get_physical_keycode' method.
        /// </summary>
        public static readonly StringName GetPhysicalKeycode = "get_physical_keycode";
        /// <summary>
        /// Cached name for the 'set_key_label' method.
        /// </summary>
        public static readonly StringName SetKeyLabel = "set_key_label";
        /// <summary>
        /// Cached name for the 'get_key_label' method.
        /// </summary>
        public static readonly StringName GetKeyLabel = "get_key_label";
        /// <summary>
        /// Cached name for the 'set_unicode' method.
        /// </summary>
        public static readonly StringName SetUnicode = "set_unicode";
        /// <summary>
        /// Cached name for the 'get_unicode' method.
        /// </summary>
        public static readonly StringName GetUnicode = "get_unicode";
        /// <summary>
        /// Cached name for the 'set_location' method.
        /// </summary>
        public static readonly StringName SetLocation = "set_location";
        /// <summary>
        /// Cached name for the 'get_location' method.
        /// </summary>
        public static readonly StringName GetLocation = "get_location";
        /// <summary>
        /// Cached name for the 'set_echo' method.
        /// </summary>
        public static readonly StringName SetEcho = "set_echo";
        /// <summary>
        /// Cached name for the 'get_keycode_with_modifiers' method.
        /// </summary>
        public static readonly StringName GetKeycodeWithModifiers = "get_keycode_with_modifiers";
        /// <summary>
        /// Cached name for the 'get_physical_keycode_with_modifiers' method.
        /// </summary>
        public static readonly StringName GetPhysicalKeycodeWithModifiers = "get_physical_keycode_with_modifiers";
        /// <summary>
        /// Cached name for the 'get_key_label_with_modifiers' method.
        /// </summary>
        public static readonly StringName GetKeyLabelWithModifiers = "get_key_label_with_modifiers";
        /// <summary>
        /// Cached name for the 'as_text_keycode' method.
        /// </summary>
        public static readonly StringName AsTextKeycode = "as_text_keycode";
        /// <summary>
        /// Cached name for the 'as_text_physical_keycode' method.
        /// </summary>
        public static readonly StringName AsTextPhysicalKeycode = "as_text_physical_keycode";
        /// <summary>
        /// Cached name for the 'as_text_key_label' method.
        /// </summary>
        public static readonly StringName AsTextKeyLabel = "as_text_key_label";
        /// <summary>
        /// Cached name for the 'as_text_location' method.
        /// </summary>
        public static readonly StringName AsTextLocation = "as_text_location";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : InputEventWithModifiers.SignalName
    {
    }
}
