namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Base class for <see cref="Kaizen.ImageTexture3D"/> and <see cref="Kaizen.CompressedTexture3D"/>. Cannot be used directly, but contains all the functions necessary for accessing the derived resource types. <see cref="Kaizen.Texture3D"/> is the base class for all 3-dimensional texture types. See also <see cref="Kaizen.TextureLayered"/>.</para>
/// <para>All images need to have the same width, height and number of mipmap levels.</para>
/// <para>To create such a texture file yourself, reimport your image files using the Kaizen Editor import presets.</para>
/// </summary>
public partial class Texture3D : Texture
{
    private static readonly System.Type CachedType = typeof(Texture3D);

    private static readonly StringName NativeName = "Texture3D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public Texture3D() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal Texture3D(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal Texture3D(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture3D"/>'s data is queried.</para>
    /// </summary>
    public virtual Kaizen.Collections.Array<Image> _GetData()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture3D"/>'s depth is queried.</para>
    /// </summary>
    public virtual int _GetDepth()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture3D"/>'s format is queried.</para>
    /// </summary>
    public virtual Image.Format _GetFormat()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture3D"/>'s height is queried.</para>
    /// </summary>
    public virtual int _GetHeight()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture3D"/>'s width is queried.</para>
    /// </summary>
    public virtual int _GetWidth()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the presence of mipmaps in the <see cref="Kaizen.Texture3D"/> is queried.</para>
    /// </summary>
    public virtual bool _HasMipmaps()
    {
        return default;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFormat, 3847873762ul);

    /// <summary>
    /// <para>Returns the current format being used by this texture.</para>
    /// </summary>
    public Image.Format GetFormat()
    {
        Image.Format ret = (Image.Format)NativeCalls.kaizen_icall_0_39(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetWidth, 3905245786ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Texture3D"/>'s width in pixels. Width is typically represented by the X axis.</para>
    /// </summary>
    public int GetWidth()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHeight, 3905245786ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Texture3D"/>'s height in pixels. Width is typically represented by the Y axis.</para>
    /// </summary>
    public int GetHeight()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDepth, 3905245786ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Texture3D"/>'s depth in pixels. Depth is typically represented by the Z axis (a dimension not present in <see cref="Kaizen.Texture2D"/>).</para>
    /// </summary>
    public int GetDepth()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasMipmaps, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.Texture3D"/> has generated mipmaps.</para>
    /// </summary>
    public bool HasMipmaps()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind4, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetData, 3995934104ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Texture3D"/>'s data as an array of <see cref="Kaizen.Image"/>s. Each <see cref="Kaizen.Image"/> represents a <i>slice</i> of the <see cref="Kaizen.Texture3D"/>, with different slices mapping to different depth (Z axis) levels.</para>
    /// </summary>
    public Kaizen.Collections.Array<Image> GetData()
    {
        Kaizen.Collections.Array<Image> ret = new Kaizen.Collections.Array<Image>(NativeCalls.kaizen_icall_0_146(MethodBind5, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreatePlaceholder, 121922552ul);

    /// <summary>
    /// <para>Creates a placeholder version of this resource (<see cref="Kaizen.PlaceholderTexture3D"/>).</para>
    /// </summary>
    public Resource CreatePlaceholder()
    {
        Resource ret = (Resource)NativeCalls.kaizen_icall_0_87(MethodBind6, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_data = "_GetData";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_depth = "_GetDepth";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_format = "_GetFormat";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_height = "_GetHeight";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_width = "_GetWidth";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__has_mipmaps = "_HasMipmaps";

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
        if ((method == MethodProxyName__get_data || method == MethodName._GetData) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_data.NativeValue))
        {
            var callRet = _GetData();
            ret = VariantUtils.CreateFromArray(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_depth || method == MethodName._GetDepth) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_depth.NativeValue))
        {
            var callRet = _GetDepth();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_format || method == MethodName._GetFormat) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_format.NativeValue))
        {
            var callRet = _GetFormat();
            ret = VariantUtils.CreateFrom<Image.Format>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_height || method == MethodName._GetHeight) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_height.NativeValue))
        {
            var callRet = _GetHeight();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_width || method == MethodName._GetWidth) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_width.NativeValue))
        {
            var callRet = _GetWidth();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__has_mipmaps || method == MethodName._HasMipmaps) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__has_mipmaps.NativeValue))
        {
            var callRet = _HasMipmaps();
            ret = VariantUtils.CreateFrom<bool>(callRet);
            return true;
        }
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
        if (method == MethodName._GetData)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_data.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetDepth)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_depth.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetFormat)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_format.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetHeight)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_height.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetWidth)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_width.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._HasMipmaps)
        {
            if (HasKaizenClassMethod(MethodProxyName__has_mipmaps.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
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
    public new class PropertyName : Texture.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Texture.MethodName
    {
        /// <summary>
        /// Cached name for the '_get_data' method.
        /// </summary>
        public static readonly StringName _GetData = "_get_data";
        /// <summary>
        /// Cached name for the '_get_depth' method.
        /// </summary>
        public static readonly StringName _GetDepth = "_get_depth";
        /// <summary>
        /// Cached name for the '_get_format' method.
        /// </summary>
        public static readonly StringName _GetFormat = "_get_format";
        /// <summary>
        /// Cached name for the '_get_height' method.
        /// </summary>
        public static readonly StringName _GetHeight = "_get_height";
        /// <summary>
        /// Cached name for the '_get_width' method.
        /// </summary>
        public static readonly StringName _GetWidth = "_get_width";
        /// <summary>
        /// Cached name for the '_has_mipmaps' method.
        /// </summary>
        public static readonly StringName _HasMipmaps = "_has_mipmaps";
        /// <summary>
        /// Cached name for the 'get_format' method.
        /// </summary>
        public static readonly StringName GetFormat = "get_format";
        /// <summary>
        /// Cached name for the 'get_width' method.
        /// </summary>
        public static readonly StringName GetWidth = "get_width";
        /// <summary>
        /// Cached name for the 'get_height' method.
        /// </summary>
        public static readonly StringName GetHeight = "get_height";
        /// <summary>
        /// Cached name for the 'get_depth' method.
        /// </summary>
        public static readonly StringName GetDepth = "get_depth";
        /// <summary>
        /// Cached name for the 'has_mipmaps' method.
        /// </summary>
        public static readonly StringName HasMipmaps = "has_mipmaps";
        /// <summary>
        /// Cached name for the 'get_data' method.
        /// </summary>
        public static readonly StringName GetData = "get_data";
        /// <summary>
        /// Cached name for the 'create_placeholder' method.
        /// </summary>
        public static readonly StringName CreatePlaceholder = "create_placeholder";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Texture.SignalName
    {
    }
}
