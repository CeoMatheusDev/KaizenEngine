namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A "reverb" effect plays the input audio back continuously, decaying over a period of time. It simulates sounds in different kinds of spaces, ranging from small rooms, to big caverns.</para>
/// <para>See also <see cref="Kaizen.AudioEffectDelay"/> for a non-blurry type of echo.</para>
/// </summary>
public partial class AudioEffectReverb : AudioEffect
{
    /// <summary>
    /// <para>Time between the original audio and the early reflections of the reverb signal, in milliseconds. Value can range from 20 to 500.</para>
    /// </summary>
    public float PredelayMsec
    {
        get
        {
            return GetPredelayMsec();
        }
        set
        {
            SetPredelayMsec(value);
        }
    }

    /// <summary>
    /// <para>Gain of early reflection copies. At higher values, early reflection copies are louder and ring out for longer. Value can range from 0 to 1.</para>
    /// </summary>
    public float PredelayFeedback
    {
        get
        {
            return GetPredelayFeedback();
        }
        set
        {
            SetPredelayFeedback(value);
        }
    }

    /// <summary>
    /// <para>Dimensions of simulated room. Bigger means more echoes. Value can range from 0 to 1.</para>
    /// </summary>
    public float RoomSize
    {
        get
        {
            return GetRoomSize();
        }
        set
        {
            SetRoomSize(value);
        }
    }

    /// <summary>
    /// <para>Defines how reflective the imaginary room's walls are. The more reflective, the more high frequency content the reverb has. Value can range from 0 to 1.</para>
    /// </summary>
    public float Damping
    {
        get
        {
            return GetDamping();
        }
        set
        {
            SetDamping(value);
        }
    }

    /// <summary>
    /// <para>Widens or narrows the stereo image of the reverb tail. At 1, it fully widens. Value can range from 0 to 1.</para>
    /// </summary>
    public float Spread
    {
        get
        {
            return GetSpread();
        }
        set
        {
            SetSpread(value);
        }
    }

    /// <summary>
    /// <para>High-pass filter allows frequencies higher than a certain cutoff threshold and attenuates frequencies lower than the cutoff threshold. Value can range from 0 to 1.</para>
    /// </summary>
    public float Hipass
    {
        get
        {
            return GetHpf();
        }
        set
        {
            SetHpf(value);
        }
    }

    /// <summary>
    /// <para>The volume ratio of the original audio. At 0, only the modified audio is outputted. Value can range from 0 to 1.</para>
    /// </summary>
    public float Dry
    {
        get
        {
            return GetDry();
        }
        set
        {
            SetDry(value);
        }
    }

    /// <summary>
    /// <para>The volume ratio of the modified audio. At 0, only the original audio is outputted. Value can range from 0 to 1.</para>
    /// </summary>
    public float Wet
    {
        get
        {
            return GetWet();
        }
        set
        {
            SetWet(value);
        }
    }

    private static readonly System.Type CachedType = typeof(AudioEffectReverb);

    private static readonly StringName NativeName = "AudioEffectReverb";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public AudioEffectReverb() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal AudioEffectReverb(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal AudioEffectReverb(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPredelayMsec, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPredelayMsec(float msec)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind0, KaizenObject.GetPtr(this), msec);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPredelayMsec, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetPredelayMsec()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPredelayFeedback, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPredelayFeedback(float feedback)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind2, KaizenObject.GetPtr(this), feedback);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPredelayFeedback, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetPredelayFeedback()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRoomSize, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRoomSize(float size)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind4, KaizenObject.GetPtr(this), size);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRoomSize, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetRoomSize()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDamping, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDamping(float amount)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind6, KaizenObject.GetPtr(this), amount);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDamping, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetDamping()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSpread, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSpread(float amount)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind8, KaizenObject.GetPtr(this), amount);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSpread, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetSpread()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDry, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDry(float amount)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind10, KaizenObject.GetPtr(this), amount);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDry, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetDry()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetWet, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetWet(float amount)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind12, KaizenObject.GetPtr(this), amount);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetWet, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetWet()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetHpf, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetHpf(float amount)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind14, KaizenObject.GetPtr(this), amount);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHpf, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetHpf()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind15, KaizenObject.GetPtr(this));
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
    public new class PropertyName : AudioEffect.PropertyName
    {
        /// <summary>
        /// Cached name for the 'predelay_msec' property.
        /// </summary>
        public static readonly StringName PredelayMsec = "predelay_msec";
        /// <summary>
        /// Cached name for the 'predelay_feedback' property.
        /// </summary>
        public static readonly StringName PredelayFeedback = "predelay_feedback";
        /// <summary>
        /// Cached name for the 'room_size' property.
        /// </summary>
        public static readonly StringName RoomSize = "room_size";
        /// <summary>
        /// Cached name for the 'damping' property.
        /// </summary>
        public static readonly StringName Damping = "damping";
        /// <summary>
        /// Cached name for the 'spread' property.
        /// </summary>
        public static readonly StringName Spread = "spread";
        /// <summary>
        /// Cached name for the 'hipass' property.
        /// </summary>
        public static readonly StringName Hipass = "hipass";
        /// <summary>
        /// Cached name for the 'dry' property.
        /// </summary>
        public static readonly StringName Dry = "dry";
        /// <summary>
        /// Cached name for the 'wet' property.
        /// </summary>
        public static readonly StringName Wet = "wet";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : AudioEffect.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_predelay_msec' method.
        /// </summary>
        public static readonly StringName SetPredelayMsec = "set_predelay_msec";
        /// <summary>
        /// Cached name for the 'get_predelay_msec' method.
        /// </summary>
        public static readonly StringName GetPredelayMsec = "get_predelay_msec";
        /// <summary>
        /// Cached name for the 'set_predelay_feedback' method.
        /// </summary>
        public static readonly StringName SetPredelayFeedback = "set_predelay_feedback";
        /// <summary>
        /// Cached name for the 'get_predelay_feedback' method.
        /// </summary>
        public static readonly StringName GetPredelayFeedback = "get_predelay_feedback";
        /// <summary>
        /// Cached name for the 'set_room_size' method.
        /// </summary>
        public static readonly StringName SetRoomSize = "set_room_size";
        /// <summary>
        /// Cached name for the 'get_room_size' method.
        /// </summary>
        public static readonly StringName GetRoomSize = "get_room_size";
        /// <summary>
        /// Cached name for the 'set_damping' method.
        /// </summary>
        public static readonly StringName SetDamping = "set_damping";
        /// <summary>
        /// Cached name for the 'get_damping' method.
        /// </summary>
        public static readonly StringName GetDamping = "get_damping";
        /// <summary>
        /// Cached name for the 'set_spread' method.
        /// </summary>
        public static readonly StringName SetSpread = "set_spread";
        /// <summary>
        /// Cached name for the 'get_spread' method.
        /// </summary>
        public static readonly StringName GetSpread = "get_spread";
        /// <summary>
        /// Cached name for the 'set_dry' method.
        /// </summary>
        public static readonly StringName SetDry = "set_dry";
        /// <summary>
        /// Cached name for the 'get_dry' method.
        /// </summary>
        public static readonly StringName GetDry = "get_dry";
        /// <summary>
        /// Cached name for the 'set_wet' method.
        /// </summary>
        public static readonly StringName SetWet = "set_wet";
        /// <summary>
        /// Cached name for the 'get_wet' method.
        /// </summary>
        public static readonly StringName GetWet = "get_wet";
        /// <summary>
        /// Cached name for the 'set_hpf' method.
        /// </summary>
        public static readonly StringName SetHpf = "set_hpf";
        /// <summary>
        /// Cached name for the 'get_hpf' method.
        /// </summary>
        public static readonly StringName GetHpf = "get_hpf";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : AudioEffect.SignalName
    {
    }
}
