namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This object is used by <see cref="Kaizen.RenderingDevice"/>.</para>
/// </summary>
public partial class RDTextureView : RefCounted
{
    /// <summary>
    /// <para>Optional override for the data format to return sampled values in. The corresponding <see cref="Kaizen.RDTextureFormat"/> must have had this added as a shareable format. The default value of <see cref="Kaizen.RenderingDevice.DataFormat.Max"/> does not override the format.</para>
    /// </summary>
    public RenderingDevice.DataFormat FormatOverride
    {
        get
        {
            return GetFormatOverride();
        }
        set
        {
            SetFormatOverride(value);
        }
    }

    /// <summary>
    /// <para>The channel to sample when sampling the red color channel.</para>
    /// </summary>
    public RenderingDevice.TextureSwizzle SwizzleR
    {
        get
        {
            return GetSwizzleR();
        }
        set
        {
            SetSwizzleR(value);
        }
    }

    /// <summary>
    /// <para>The channel to sample when sampling the green color channel.</para>
    /// </summary>
    public RenderingDevice.TextureSwizzle SwizzleG
    {
        get
        {
            return GetSwizzleG();
        }
        set
        {
            SetSwizzleG(value);
        }
    }

    /// <summary>
    /// <para>The channel to sample when sampling the blue color channel.</para>
    /// </summary>
    public RenderingDevice.TextureSwizzle SwizzleB
    {
        get
        {
            return GetSwizzleB();
        }
        set
        {
            SetSwizzleB(value);
        }
    }

    /// <summary>
    /// <para>The channel to sample when sampling the alpha channel.</para>
    /// </summary>
    public RenderingDevice.TextureSwizzle SwizzleA
    {
        get
        {
            return GetSwizzleA();
        }
        set
        {
            SetSwizzleA(value);
        }
    }

    private static readonly System.Type CachedType = typeof(RDTextureView);

    private static readonly StringName NativeName = "RDTextureView";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public RDTextureView() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal RDTextureView(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal RDTextureView(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFormatOverride, 565531219ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFormatOverride(RenderingDevice.DataFormat pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFormatOverride, 2235804183ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.DataFormat GetFormatOverride()
    {
        RenderingDevice.DataFormat ret = (RenderingDevice.DataFormat)NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSwizzleR, 3833362581ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSwizzleR(RenderingDevice.TextureSwizzle pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSwizzleR, 4150792614ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.TextureSwizzle GetSwizzleR()
    {
        RenderingDevice.TextureSwizzle ret = (RenderingDevice.TextureSwizzle)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSwizzleG, 3833362581ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSwizzleG(RenderingDevice.TextureSwizzle pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind4, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSwizzleG, 4150792614ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.TextureSwizzle GetSwizzleG()
    {
        RenderingDevice.TextureSwizzle ret = (RenderingDevice.TextureSwizzle)NativeCalls.kaizen_icall_0_39(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSwizzleB, 3833362581ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSwizzleB(RenderingDevice.TextureSwizzle pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind6, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSwizzleB, 4150792614ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.TextureSwizzle GetSwizzleB()
    {
        RenderingDevice.TextureSwizzle ret = (RenderingDevice.TextureSwizzle)NativeCalls.kaizen_icall_0_39(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSwizzleA, 3833362581ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSwizzleA(RenderingDevice.TextureSwizzle pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind8, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSwizzleA, 4150792614ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.TextureSwizzle GetSwizzleA()
    {
        RenderingDevice.TextureSwizzle ret = (RenderingDevice.TextureSwizzle)NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
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
    public new class PropertyName : RefCounted.PropertyName
    {
        /// <summary>
        /// Cached name for the 'format_override' property.
        /// </summary>
        public static readonly StringName FormatOverride = "format_override";
        /// <summary>
        /// Cached name for the 'swizzle_r' property.
        /// </summary>
        public static readonly StringName SwizzleR = "swizzle_r";
        /// <summary>
        /// Cached name for the 'swizzle_g' property.
        /// </summary>
        public static readonly StringName SwizzleG = "swizzle_g";
        /// <summary>
        /// Cached name for the 'swizzle_b' property.
        /// </summary>
        public static readonly StringName SwizzleB = "swizzle_b";
        /// <summary>
        /// Cached name for the 'swizzle_a' property.
        /// </summary>
        public static readonly StringName SwizzleA = "swizzle_a";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_format_override' method.
        /// </summary>
        public static readonly StringName SetFormatOverride = "set_format_override";
        /// <summary>
        /// Cached name for the 'get_format_override' method.
        /// </summary>
        public static readonly StringName GetFormatOverride = "get_format_override";
        /// <summary>
        /// Cached name for the 'set_swizzle_r' method.
        /// </summary>
        public static readonly StringName SetSwizzleR = "set_swizzle_r";
        /// <summary>
        /// Cached name for the 'get_swizzle_r' method.
        /// </summary>
        public static readonly StringName GetSwizzleR = "get_swizzle_r";
        /// <summary>
        /// Cached name for the 'set_swizzle_g' method.
        /// </summary>
        public static readonly StringName SetSwizzleG = "set_swizzle_g";
        /// <summary>
        /// Cached name for the 'get_swizzle_g' method.
        /// </summary>
        public static readonly StringName GetSwizzleG = "get_swizzle_g";
        /// <summary>
        /// Cached name for the 'set_swizzle_b' method.
        /// </summary>
        public static readonly StringName SetSwizzleB = "set_swizzle_b";
        /// <summary>
        /// Cached name for the 'get_swizzle_b' method.
        /// </summary>
        public static readonly StringName GetSwizzleB = "get_swizzle_b";
        /// <summary>
        /// Cached name for the 'set_swizzle_a' method.
        /// </summary>
        public static readonly StringName SetSwizzleA = "set_swizzle_a";
        /// <summary>
        /// Cached name for the 'get_swizzle_a' method.
        /// </summary>
        public static readonly StringName GetSwizzleA = "get_swizzle_a";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
