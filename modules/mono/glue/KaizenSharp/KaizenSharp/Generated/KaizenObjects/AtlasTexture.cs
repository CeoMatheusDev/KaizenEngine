namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.Texture2D"/> resource that draws only part of its <see cref="Kaizen.AtlasTexture.Atlas"/> texture, as defined by the <see cref="Kaizen.AtlasTexture.Region"/>. An additional <see cref="Kaizen.AtlasTexture.Margin"/> can also be set, which is useful for small adjustments.</para>
/// <para>Multiple <see cref="Kaizen.AtlasTexture"/> resources can be cropped from the same <see cref="Kaizen.AtlasTexture.Atlas"/>. Packing many smaller textures into a singular large texture helps to optimize video memory costs and render calls.</para>
/// <para><b>Note:</b> <see cref="Kaizen.AtlasTexture"/> cannot be used in an <see cref="Kaizen.AnimatedTexture"/>, and will not tile properly in nodes such as <see cref="Kaizen.TextureRect"/> or <see cref="Kaizen.Sprite2D"/>. To tile an <see cref="Kaizen.AtlasTexture"/>, modify its <see cref="Kaizen.AtlasTexture.Region"/> instead.</para>
/// </summary>
public partial class AtlasTexture : Texture2D
{
    /// <summary>
    /// <para>The texture that contains the atlas. Can be any type inheriting from <see cref="Kaizen.Texture2D"/>, including another <see cref="Kaizen.AtlasTexture"/>.</para>
    /// </summary>
    public Texture2D Atlas
    {
        get
        {
            return GetAtlas();
        }
        set
        {
            SetAtlas(value);
        }
    }

    /// <summary>
    /// <para>The region used to draw the <see cref="Kaizen.AtlasTexture.Atlas"/>. If either dimension of the region's size is <c>0</c>, the value from <see cref="Kaizen.AtlasTexture.Atlas"/> size will be used for that axis instead.</para>
    /// <para><b>Note:</b> The image size is always an integer, so the actual region size is rounded down.</para>
    /// </summary>
    public Rect2 Region
    {
        get
        {
            return GetRegion();
        }
        set
        {
            SetRegion(value);
        }
    }

    /// <summary>
    /// <para>The margin around the <see cref="Kaizen.AtlasTexture.Region"/>. Useful for small adjustments. If the <c>Rect2.size</c> of this property ("w" and "h" in the editor) is set, the drawn texture is resized to fit within the margin.</para>
    /// </summary>
    public Rect2 Margin
    {
        get
        {
            return GetMargin();
        }
        set
        {
            SetMargin(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the area outside of the <see cref="Kaizen.AtlasTexture.Region"/> is clipped to avoid bleeding of the surrounding texture pixels.</para>
    /// </summary>
    public bool FilterClip
    {
        get
        {
            return HasFilterClip();
        }
        set
        {
            SetFilterClip(value);
        }
    }

    private static readonly System.Type CachedType = typeof(AtlasTexture);

    private static readonly StringName NativeName = "AtlasTexture";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public AtlasTexture() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal AtlasTexture(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal AtlasTexture(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAtlas, 4051416890ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAtlas(Texture2D atlas)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(atlas));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAtlas, 3635182373ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Texture2D GetAtlas()
    {
        Texture2D ret = (Texture2D)NativeCalls.kaizen_icall_0_87(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRegion, 2046264180ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetRegion(Rect2 region)
    {
        NativeCalls.kaizen_icall_1_214(MethodBind2, KaizenObject.GetPtr(this), &region);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRegion, 1639390495ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Rect2 GetRegion()
    {
        Rect2 ret = NativeCalls.kaizen_icall_0_215(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMargin, 2046264180ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetMargin(Rect2 margin)
    {
        NativeCalls.kaizen_icall_1_214(MethodBind4, KaizenObject.GetPtr(this), &margin);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMargin, 1639390495ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Rect2 GetMargin()
    {
        Rect2 ret = NativeCalls.kaizen_icall_0_215(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFilterClip, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFilterClip(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind6, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasFilterClip, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool HasFilterClip()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind7, KaizenObject.GetPtr(this)).ToBool();
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
    public new class PropertyName : Texture2D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'atlas' property.
        /// </summary>
        public static readonly StringName Atlas = "atlas";
        /// <summary>
        /// Cached name for the 'region' property.
        /// </summary>
        public static readonly StringName Region = "region";
        /// <summary>
        /// Cached name for the 'margin' property.
        /// </summary>
        public static readonly StringName Margin = "margin";
        /// <summary>
        /// Cached name for the 'filter_clip' property.
        /// </summary>
        public static readonly StringName FilterClip = "filter_clip";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Texture2D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_atlas' method.
        /// </summary>
        public static readonly StringName SetAtlas = "set_atlas";
        /// <summary>
        /// Cached name for the 'get_atlas' method.
        /// </summary>
        public static readonly StringName GetAtlas = "get_atlas";
        /// <summary>
        /// Cached name for the 'set_region' method.
        /// </summary>
        public static readonly StringName SetRegion = "set_region";
        /// <summary>
        /// Cached name for the 'get_region' method.
        /// </summary>
        public static readonly StringName GetRegion = "get_region";
        /// <summary>
        /// Cached name for the 'set_margin' method.
        /// </summary>
        public static readonly StringName SetMargin = "set_margin";
        /// <summary>
        /// Cached name for the 'get_margin' method.
        /// </summary>
        public static readonly StringName GetMargin = "get_margin";
        /// <summary>
        /// Cached name for the 'set_filter_clip' method.
        /// </summary>
        public static readonly StringName SetFilterClip = "set_filter_clip";
        /// <summary>
        /// Cached name for the 'has_filter_clip' method.
        /// </summary>
        public static readonly StringName HasFilterClip = "has_filter_clip";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Texture2D.SignalName
    {
    }
}
