namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A texture works by registering an image in the video hardware, which then can be used in 3D models or 2D <see cref="Kaizen.Sprite2D"/> or GUI <see cref="Kaizen.Control"/>.</para>
/// <para>Textures are often created by loading them from a file. See <c>@GDScript.load</c>.</para>
/// <para><see cref="Kaizen.Texture2D"/> is a base for other resources. It cannot be used directly.</para>
/// <para><b>Note:</b> The maximum texture size is 16384×16384 pixels due to graphics hardware limitations. Larger textures may fail to import.</para>
/// </summary>
public partial class Texture2D : Texture
{
    private static readonly System.Type CachedType = typeof(Texture2D);

    private static readonly StringName NativeName = "Texture2D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public Texture2D() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal Texture2D(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal Texture2D(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Called when the entire <see cref="Kaizen.Texture2D"/> is requested to be drawn over a <see cref="Kaizen.CanvasItem"/>, with the top-left offset specified in <paramref name="pos"/>. <paramref name="modulate"/> specifies a multiplier for the colors being drawn, while <paramref name="transpose"/> specifies whether drawing should be performed in column-major order instead of row-major order (resulting in 90-degree clockwise rotation).</para>
    /// <para><b>Note:</b> This is only used in 2D rendering, not 3D.</para>
    /// </summary>
    public virtual unsafe void _Draw(Rid toCanvasItem, Vector2 pos, Color modulate, bool transpose)
    {
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture2D"/> is requested to be drawn onto <see cref="Kaizen.CanvasItem"/>'s specified <paramref name="rect"/>. <paramref name="modulate"/> specifies a multiplier for the colors being drawn, while <paramref name="transpose"/> specifies whether drawing should be performed in column-major order instead of row-major order (resulting in 90-degree clockwise rotation).</para>
    /// <para><b>Note:</b> This is only used in 2D rendering, not 3D.</para>
    /// </summary>
    public virtual unsafe void _DrawRect(Rid toCanvasItem, Rect2 rect, bool tile, Color modulate, bool transpose)
    {
    }

    /// <summary>
    /// <para>Called when a part of the <see cref="Kaizen.Texture2D"/> specified by <paramref name="srcRect"/>'s coordinates is requested to be drawn onto <see cref="Kaizen.CanvasItem"/>'s specified <paramref name="rect"/>. <paramref name="modulate"/> specifies a multiplier for the colors being drawn, while <paramref name="transpose"/> specifies whether drawing should be performed in column-major order instead of row-major order (resulting in 90-degree clockwise rotation).</para>
    /// <para><b>Note:</b> This is only used in 2D rendering, not 3D.</para>
    /// </summary>
    public virtual unsafe void _DrawRectRegion(Rid toCanvasItem, Rect2 rect, Rect2 srcRect, Color modulate, bool transpose, bool clipUV)
    {
    }

    /// <summary>
    /// <para>Called when <see cref="Kaizen.Texture2D.GetFormat()"/> is called.</para>
    /// </summary>
    public virtual Image.Format _GetFormat()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture2D"/>'s height is queried.</para>
    /// </summary>
    public virtual int _GetHeight()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when <see cref="Kaizen.Texture2D.GetImage()"/> is called.</para>
    /// </summary>
    public virtual Image _GetImage()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when <see cref="Kaizen.Texture2D.GetMipmapCount()"/> is called.</para>
    /// </summary>
    public virtual int _GetMipmapCount()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.Texture2D"/>'s width is queried.</para>
    /// </summary>
    public virtual int _GetWidth()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the presence of an alpha channel in the <see cref="Kaizen.Texture2D"/> is queried.</para>
    /// </summary>
    public virtual bool _HasAlpha()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when <see cref="Kaizen.Texture2D.HasMipmaps()"/> is called.</para>
    /// </summary>
    public virtual bool _HasMipmaps()
    {
        return default;
    }

    /// <summary>
    /// <para>Called when a pixel's opaque state in the <see cref="Kaizen.Texture2D"/> is queried at the specified <c>(x, y)</c> position.</para>
    /// </summary>
    public virtual bool _IsPixelOpaque(int x, int y)
    {
        return default;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFormat, 3847873762ul);

    /// <summary>
    /// <para>Returns the image format of the texture.</para>
    /// </summary>
    public Image.Format GetFormat()
    {
        Image.Format ret = (Image.Format)NativeCalls.kaizen_icall_0_39(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMipmapCount, 3905245786ul);

    /// <summary>
    /// <para>Returns the number of mipmaps of the texture.</para>
    /// </summary>
    public int GetMipmapCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetWidth, 3905245786ul);

    /// <summary>
    /// <para>Returns the texture width in pixels.</para>
    /// </summary>
    public int GetWidth()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHeight, 3905245786ul);

    /// <summary>
    /// <para>Returns the texture height in pixels.</para>
    /// </summary>
    public int GetHeight()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSize, 3341600327ul);

    /// <summary>
    /// <para>Returns the texture size in pixels.</para>
    /// </summary>
    public Vector2 GetSize()
    {
        Vector2 ret = NativeCalls.kaizen_icall_0_37(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasAlpha, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this <see cref="Kaizen.Texture2D"/> has an alpha channel.</para>
    /// </summary>
    public bool HasAlpha()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind5, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasMipmaps, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the texture has mipmaps.</para>
    /// </summary>
    public bool HasMipmaps()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind6, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Draw, 2729649137ul);

    /// <summary>
    /// <para>Draws the texture using a <see cref="Kaizen.CanvasItem"/> with the <see cref="Kaizen.RenderingServer"/> API at the specified <paramref name="position"/>.</para>
    /// </summary>
    /// <param name="modulate">If the parameter is null, then the default value is <c>new Color(1.0f, 1.0f, 1.0f, 1.0f)</c>.</param>
    public unsafe void Draw(Rid canvasItem, Vector2 position, Nullable<Color> modulate = null, bool transpose = false)
    {
        Color modulateOrDefVal = modulate.HasValue ? modulate.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f);
        NativeCalls.kaizen_icall_4_1470(MethodBind7, KaizenObject.GetPtr(this), canvasItem, &position, &modulateOrDefVal, transpose.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.DrawRect, 3499451691ul);

    /// <summary>
    /// <para>Draws the texture using a <see cref="Kaizen.CanvasItem"/> with the <see cref="Kaizen.RenderingServer"/> API.</para>
    /// </summary>
    /// <param name="modulate">If the parameter is null, then the default value is <c>new Color(1.0f, 1.0f, 1.0f, 1.0f)</c>.</param>
    public unsafe void DrawRect(Rid canvasItem, Rect2 rect, bool tile, Nullable<Color> modulate = null, bool transpose = false)
    {
        Color modulateOrDefVal = modulate.HasValue ? modulate.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f);
        NativeCalls.kaizen_icall_5_1471(MethodBind8, KaizenObject.GetPtr(this), canvasItem, &rect, tile.ToKaizenBool(), &modulateOrDefVal, transpose.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.DrawRectRegion, 2963678660ul);

    /// <summary>
    /// <para>Draws a part of the texture using a <see cref="Kaizen.CanvasItem"/> with the <see cref="Kaizen.RenderingServer"/> API.</para>
    /// </summary>
    /// <param name="modulate">If the parameter is null, then the default value is <c>new Color(1.0f, 1.0f, 1.0f, 1.0f)</c>.</param>
    public unsafe void DrawRectRegion(Rid canvasItem, Rect2 rect, Rect2 srcRect, Nullable<Color> modulate = null, bool transpose = false, bool clipUV = true)
    {
        Color modulateOrDefVal = modulate.HasValue ? modulate.Value : new Color(1.0f, 1.0f, 1.0f, 1.0f);
        NativeCalls.kaizen_icall_6_1472(MethodBind9, KaizenObject.GetPtr(this), canvasItem, &rect, &srcRect, &modulateOrDefVal, transpose.ToKaizenBool(), clipUV.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetImage, 4190603485ul);

    /// <summary>
    /// <para>Returns an <see cref="Kaizen.Image"/> that is a copy of data from this <see cref="Kaizen.Texture2D"/> (a new <see cref="Kaizen.Image"/> is created each time). <see cref="Kaizen.Image"/>s can be accessed and manipulated directly.</para>
    /// <para><b>Note:</b> This will return <see langword="null"/> if this <see cref="Kaizen.Texture2D"/> is invalid.</para>
    /// <para><b>Note:</b> This will fetch the texture data from the GPU, which might cause performance problems when overused. Avoid calling <see cref="Kaizen.Texture2D.GetImage()"/> every frame, especially on large textures.</para>
    /// </summary>
    public Image GetImage()
    {
        Image ret = (Image)NativeCalls.kaizen_icall_0_87(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreatePlaceholder, 121922552ul);

    /// <summary>
    /// <para>Creates a placeholder version of this resource (<see cref="Kaizen.PlaceholderTexture2D"/>).</para>
    /// </summary>
    public Resource CreatePlaceholder()
    {
        Resource ret = (Resource)NativeCalls.kaizen_icall_0_87(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__draw = "_Draw";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__draw_rect = "_DrawRect";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__draw_rect_region = "_DrawRectRegion";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_format = "_GetFormat";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_height = "_GetHeight";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_image = "_GetImage";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_mipmap_count = "_GetMipmapCount";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_width = "_GetWidth";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__has_alpha = "_HasAlpha";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__has_mipmaps = "_HasMipmaps";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__is_pixel_opaque = "_IsPixelOpaque";

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
        if ((method == MethodProxyName__draw || method == MethodName._Draw) && args.Count == 4 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__draw.NativeValue))
        {
            _Draw(VariantUtils.ConvertTo<Rid>(args[0]), VariantUtils.ConvertTo<Vector2>(args[1]), VariantUtils.ConvertTo<Color>(args[2]), VariantUtils.ConvertTo<bool>(args[3]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__draw_rect || method == MethodName._DrawRect) && args.Count == 5 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__draw_rect.NativeValue))
        {
            _DrawRect(VariantUtils.ConvertTo<Rid>(args[0]), VariantUtils.ConvertTo<Rect2>(args[1]), VariantUtils.ConvertTo<bool>(args[2]), VariantUtils.ConvertTo<Color>(args[3]), VariantUtils.ConvertTo<bool>(args[4]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__draw_rect_region || method == MethodName._DrawRectRegion) && args.Count == 6 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__draw_rect_region.NativeValue))
        {
            _DrawRectRegion(VariantUtils.ConvertTo<Rid>(args[0]), VariantUtils.ConvertTo<Rect2>(args[1]), VariantUtils.ConvertTo<Rect2>(args[2]), VariantUtils.ConvertTo<Color>(args[3]), VariantUtils.ConvertTo<bool>(args[4]), VariantUtils.ConvertTo<bool>(args[5]));
            ret = default;
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
        if ((method == MethodProxyName__get_image || method == MethodName._GetImage) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_image.NativeValue))
        {
            var callRet = _GetImage();
            ret = VariantUtils.CreateFrom<Image>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_mipmap_count || method == MethodName._GetMipmapCount) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_mipmap_count.NativeValue))
        {
            var callRet = _GetMipmapCount();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_width || method == MethodName._GetWidth) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_width.NativeValue))
        {
            var callRet = _GetWidth();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__has_alpha || method == MethodName._HasAlpha) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__has_alpha.NativeValue))
        {
            var callRet = _HasAlpha();
            ret = VariantUtils.CreateFrom<bool>(callRet);
            return true;
        }
        if ((method == MethodProxyName__has_mipmaps || method == MethodName._HasMipmaps) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__has_mipmaps.NativeValue))
        {
            var callRet = _HasMipmaps();
            ret = VariantUtils.CreateFrom<bool>(callRet);
            return true;
        }
        if ((method == MethodProxyName__is_pixel_opaque || method == MethodName._IsPixelOpaque) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__is_pixel_opaque.NativeValue))
        {
            var callRet = _IsPixelOpaque(VariantUtils.ConvertTo<int>(args[0]), VariantUtils.ConvertTo<int>(args[1]));
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
        if (method == MethodName._Draw)
        {
            if (HasKaizenClassMethod(MethodProxyName__draw.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._DrawRect)
        {
            if (HasKaizenClassMethod(MethodProxyName__draw_rect.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._DrawRectRegion)
        {
            if (HasKaizenClassMethod(MethodProxyName__draw_rect_region.NativeValue.DangerousSelfRef))
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
        if (method == MethodName._GetImage)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_image.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetMipmapCount)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_mipmap_count.NativeValue.DangerousSelfRef))
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
        if (method == MethodName._HasAlpha)
        {
            if (HasKaizenClassMethod(MethodProxyName__has_alpha.NativeValue.DangerousSelfRef))
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
        if (method == MethodName._IsPixelOpaque)
        {
            if (HasKaizenClassMethod(MethodProxyName__is_pixel_opaque.NativeValue.DangerousSelfRef))
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
        /// Cached name for the '_draw' method.
        /// </summary>
        public static readonly StringName _Draw = "_draw";
        /// <summary>
        /// Cached name for the '_draw_rect' method.
        /// </summary>
        public static readonly StringName _DrawRect = "_draw_rect";
        /// <summary>
        /// Cached name for the '_draw_rect_region' method.
        /// </summary>
        public static readonly StringName _DrawRectRegion = "_draw_rect_region";
        /// <summary>
        /// Cached name for the '_get_format' method.
        /// </summary>
        public static readonly StringName _GetFormat = "_get_format";
        /// <summary>
        /// Cached name for the '_get_height' method.
        /// </summary>
        public static readonly StringName _GetHeight = "_get_height";
        /// <summary>
        /// Cached name for the '_get_image' method.
        /// </summary>
        public static readonly StringName _GetImage = "_get_image";
        /// <summary>
        /// Cached name for the '_get_mipmap_count' method.
        /// </summary>
        public static readonly StringName _GetMipmapCount = "_get_mipmap_count";
        /// <summary>
        /// Cached name for the '_get_width' method.
        /// </summary>
        public static readonly StringName _GetWidth = "_get_width";
        /// <summary>
        /// Cached name for the '_has_alpha' method.
        /// </summary>
        public static readonly StringName _HasAlpha = "_has_alpha";
        /// <summary>
        /// Cached name for the '_has_mipmaps' method.
        /// </summary>
        public static readonly StringName _HasMipmaps = "_has_mipmaps";
        /// <summary>
        /// Cached name for the '_is_pixel_opaque' method.
        /// </summary>
        public static readonly StringName _IsPixelOpaque = "_is_pixel_opaque";
        /// <summary>
        /// Cached name for the 'get_format' method.
        /// </summary>
        public static readonly StringName GetFormat = "get_format";
        /// <summary>
        /// Cached name for the 'get_mipmap_count' method.
        /// </summary>
        public static readonly StringName GetMipmapCount = "get_mipmap_count";
        /// <summary>
        /// Cached name for the 'get_width' method.
        /// </summary>
        public static readonly StringName GetWidth = "get_width";
        /// <summary>
        /// Cached name for the 'get_height' method.
        /// </summary>
        public static readonly StringName GetHeight = "get_height";
        /// <summary>
        /// Cached name for the 'get_size' method.
        /// </summary>
        public static readonly StringName GetSize = "get_size";
        /// <summary>
        /// Cached name for the 'has_alpha' method.
        /// </summary>
        public static readonly StringName HasAlpha = "has_alpha";
        /// <summary>
        /// Cached name for the 'has_mipmaps' method.
        /// </summary>
        public static readonly StringName HasMipmaps = "has_mipmaps";
        /// <summary>
        /// Cached name for the 'draw' method.
        /// </summary>
        public static readonly StringName Draw = "draw";
        /// <summary>
        /// Cached name for the 'draw_rect' method.
        /// </summary>
        public static readonly StringName DrawRect = "draw_rect";
        /// <summary>
        /// Cached name for the 'draw_rect_region' method.
        /// </summary>
        public static readonly StringName DrawRectRegion = "draw_rect_region";
        /// <summary>
        /// Cached name for the 'get_image' method.
        /// </summary>
        public static readonly StringName GetImage = "get_image";
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
