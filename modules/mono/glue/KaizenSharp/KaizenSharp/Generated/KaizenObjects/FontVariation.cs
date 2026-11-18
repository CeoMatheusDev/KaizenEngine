namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Provides OpenType variations, simulated bold / slant, and additional font settings like OpenType features and extra spacing.</para>
/// <para>To use simulated bold font variant:</para>
/// <para><code>
/// var fv = new FontVariation();
/// fv.SetBaseFont(ResourceLoader.Load&lt;FontFile&gt;("res://BarlowCondensed-Regular.ttf"));
/// fv.SetVariationEmbolden(1.2);
/// GetNode("Label").AddThemeFontOverride("font", fv);
/// GetNode("Label").AddThemeFontSizeOverride("font_size", 64);
/// </code></para>
/// <para>To set the coordinate of multiple variation axes:</para>
/// <para><code>
/// var fv = FontVariation.new();
/// var ts = TextServerManager.get_primary_interface()
/// fv.base_font = load("res://BarlowCondensed-Regular.ttf")
/// fv.variation_opentype = { ts.name_to_tag("wght"): 900, ts.name_to_tag("custom_hght"): 900 }
/// </code></para>
/// </summary>
public partial class FontVariation : Font
{
    /// <summary>
    /// <para>Base font used to create a variation. If not set, default <see cref="Kaizen.Theme"/> font is used.</para>
    /// </summary>
    public Font BaseFont
    {
        get
        {
            return GetBaseFont();
        }
        set
        {
            SetBaseFont(value);
        }
    }

    /// <summary>
    /// <para>Font OpenType variation coordinates. More info: <a href="https://docs.microsoft.com/en-us/typography/opentype/spec/dvaraxisreg">OpenType variation tags</a>.</para>
    /// <para><b>Note:</b> This <see cref="Kaizen.Collections.Dictionary"/> uses OpenType tags as keys. Variation axes can be identified both by tags (<see cref="int"/>, e.g. <c>0x77678674</c>) and names (<see cref="string"/>, e.g. <c>wght</c>). Some axes might be accessible by multiple names. For example, <c>wght</c> refers to the same axis as <c>weight</c>. Tags on the other hand are unique. To convert between names and tags, use <see cref="Kaizen.TextServer.NameToTag(string)"/> and <see cref="Kaizen.TextServer.TagToName(long)"/>.</para>
    /// <para><b>Note:</b> To get available variation axes of a font, use <see cref="Kaizen.Font.GetSupportedVariationList()"/>.</para>
    /// </summary>
    public Kaizen.Collections.Dictionary VariationOpentype
    {
        get
        {
            return GetVariationOpentype();
        }
        set
        {
            SetVariationOpentype(value);
        }
    }

    /// <summary>
    /// <para>Active face index in the TrueType / OpenType collection file.</para>
    /// </summary>
    public int VariationFaceIndex
    {
        get
        {
            return GetVariationFaceIndex();
        }
        set
        {
            SetVariationFaceIndex(value);
        }
    }

    /// <summary>
    /// <para>If is not equal to zero, emboldens the font outlines. Negative values reduce the outline thickness.</para>
    /// <para><b>Note:</b> Emboldened fonts might have self-intersecting outlines, which will prevent MSDF fonts and <see cref="Kaizen.TextMesh"/> from working correctly.</para>
    /// </summary>
    public float VariationEmbolden
    {
        get
        {
            return GetVariationEmbolden();
        }
        set
        {
            SetVariationEmbolden(value);
        }
    }

    /// <summary>
    /// <para>2D transform, applied to the font outlines, can be used for slanting, flipping and rotating glyphs.</para>
    /// <para>For example, to simulate italic typeface by slanting, apply the following transform <c>Transform2D(1.0, slant, 0.0, 1.0, 0.0, 0.0)</c>.</para>
    /// </summary>
    public Transform2D VariationTransform
    {
        get
        {
            return GetVariationTransform();
        }
        set
        {
            SetVariationTransform(value);
        }
    }

    /// <summary>
    /// <para>A set of OpenType feature tags. More info: <a href="https://docs.microsoft.com/en-us/typography/opentype/spec/featuretags">OpenType feature tags</a>.</para>
    /// </summary>
    public Kaizen.Collections.Dictionary OpentypeFeatures
    {
        get
        {
            return GetOpentypeFeatures();
        }
        set
        {
            SetOpentypeFeatures(value);
        }
    }

    /// <summary>
    /// <para>Extra spacing between graphical glyphs.</para>
    /// </summary>
    public int SpacingGlyph
    {
        get
        {
            return GetSpacing((TextServer.SpacingType)(0));
        }
        set
        {
            SetSpacing((TextServer.SpacingType)(0), value);
        }
    }

    /// <summary>
    /// <para>Extra width of the space glyphs.</para>
    /// </summary>
    public int SpacingSpace
    {
        get
        {
            return GetSpacing((TextServer.SpacingType)(1));
        }
        set
        {
            SetSpacing((TextServer.SpacingType)(1), value);
        }
    }

    /// <summary>
    /// <para>Extra spacing at the top of the line in pixels.</para>
    /// </summary>
    public int SpacingTop
    {
        get
        {
            return GetSpacing((TextServer.SpacingType)(2));
        }
        set
        {
            SetSpacing((TextServer.SpacingType)(2), value);
        }
    }

    /// <summary>
    /// <para>Extra spacing at the bottom of the line in pixels.</para>
    /// </summary>
    public int SpacingBottom
    {
        get
        {
            return GetSpacing((TextServer.SpacingType)(3));
        }
        set
        {
            SetSpacing((TextServer.SpacingType)(3), value);
        }
    }

    /// <summary>
    /// <para>Extra baseline offset (as a fraction of font height).</para>
    /// </summary>
    public float BaselineOffset
    {
        get
        {
            return GetBaselineOffset();
        }
        set
        {
            SetBaselineOffset(value);
        }
    }

    /// <summary>
    /// <para>A palette index.</para>
    /// </summary>
    public long PaletteIndex
    {
        get
        {
            return GetPaletteIndex();
        }
        set
        {
            SetPaletteIndex(value);
        }
    }

    /// <summary>
    /// <para>An array of colors to override predefined palette. Use <c>Color(0, 0, 0, 0)</c>, to keep predefined palette color at specific position.</para>
    /// </summary>
    public Color[] PaletteCustomColors
    {
        get
        {
            return GetPaletteCustomColors();
        }
        set
        {
            SetPaletteCustomColors(value);
        }
    }

    private static readonly System.Type CachedType = typeof(FontVariation);

    private static readonly StringName NativeName = "FontVariation";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public FontVariation() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal FontVariation(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal FontVariation(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBaseFont, 1262170328ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBaseFont(Font font)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(font));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBaseFont, 3229501585ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Font GetBaseFont()
    {
        Font ret = (Font)NativeCalls.kaizen_icall_0_87(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVariationOpentype, 4155329257ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVariationOpentype(Kaizen.Collections.Dictionary coords)
    {
        NativeCalls.kaizen_icall_1_147(MethodBind2, KaizenObject.GetPtr(this), (kaizen_dictionary)(coords ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVariationOpentype, 3102165223ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Dictionary GetVariationOpentype()
    {
        Kaizen.Collections.Dictionary ret = NativeCalls.kaizen_icall_0_148(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVariationEmbolden, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVariationEmbolden(float strength)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind4, KaizenObject.GetPtr(this), strength);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVariationEmbolden, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetVariationEmbolden()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVariationFaceIndex, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVariationFaceIndex(int faceIndex)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind6, KaizenObject.GetPtr(this), faceIndex);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVariationFaceIndex, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetVariationFaceIndex()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVariationTransform, 2761652528ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetVariationTransform(Transform2D transform)
    {
        NativeCalls.kaizen_icall_1_245(MethodBind8, KaizenObject.GetPtr(this), &transform);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVariationTransform, 3814499831ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Transform2D GetVariationTransform()
    {
        Transform2D ret = NativeCalls.kaizen_icall_0_246(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetOpentypeFeatures, 4155329257ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetOpentypeFeatures(Kaizen.Collections.Dictionary features)
    {
        NativeCalls.kaizen_icall_1_147(MethodBind10, KaizenObject.GetPtr(this), (kaizen_dictionary)(features ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSpacing, 3122339690ul);

    /// <summary>
    /// <para>Sets the spacing for <paramref name="spacing"/> to <paramref name="value"/> in pixels (not relative to the font size).</para>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSpacing(TextServer.SpacingType spacing, int value)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind11, KaizenObject.GetPtr(this), (int)spacing, value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBaselineOffset, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBaselineOffset(float baselineOffset)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind12, KaizenObject.GetPtr(this), baselineOffset);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBaselineOffset, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetBaselineOffset()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPaletteIndex, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public long GetPaletteIndex()
    {
        long ret = NativeCalls.kaizen_icall_0_4(MethodBind14, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPaletteIndex, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPaletteIndex(long paletteIndex)
    {
        NativeCalls.kaizen_icall_1_10(MethodBind15, KaizenObject.GetPtr(this), paletteIndex);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPaletteCustomColors, 1392750486ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Color[] GetPaletteCustomColors()
    {
        Color[] ret = NativeCalls.kaizen_icall_0_257(MethodBind16, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPaletteCustomColors, 3546319833ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPaletteCustomColors(Color[] colors)
    {
        NativeCalls.kaizen_icall_1_256(MethodBind17, KaizenObject.GetPtr(this), colors);
        global::System.GC.KeepAlive(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPaletteCustomColors(ReadOnlySpan<Color> colors)
    {
        NativeCalls.kaizen_icall_1_256(MethodBind17, KaizenObject.GetPtr(this), colors);
        global::System.GC.KeepAlive(this);
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
    public new class PropertyName : Font.PropertyName
    {
        /// <summary>
        /// Cached name for the 'base_font' property.
        /// </summary>
        public static readonly StringName BaseFont = "base_font";
        /// <summary>
        /// Cached name for the 'variation_opentype' property.
        /// </summary>
        public static readonly StringName VariationOpentype = "variation_opentype";
        /// <summary>
        /// Cached name for the 'variation_face_index' property.
        /// </summary>
        public static readonly StringName VariationFaceIndex = "variation_face_index";
        /// <summary>
        /// Cached name for the 'variation_embolden' property.
        /// </summary>
        public static readonly StringName VariationEmbolden = "variation_embolden";
        /// <summary>
        /// Cached name for the 'variation_transform' property.
        /// </summary>
        public static readonly StringName VariationTransform = "variation_transform";
        /// <summary>
        /// Cached name for the 'opentype_features' property.
        /// </summary>
        public static readonly StringName OpentypeFeatures = "opentype_features";
        /// <summary>
        /// Cached name for the 'spacing_glyph' property.
        /// </summary>
        public static readonly StringName SpacingGlyph = "spacing_glyph";
        /// <summary>
        /// Cached name for the 'spacing_space' property.
        /// </summary>
        public static readonly StringName SpacingSpace = "spacing_space";
        /// <summary>
        /// Cached name for the 'spacing_top' property.
        /// </summary>
        public static readonly StringName SpacingTop = "spacing_top";
        /// <summary>
        /// Cached name for the 'spacing_bottom' property.
        /// </summary>
        public static readonly StringName SpacingBottom = "spacing_bottom";
        /// <summary>
        /// Cached name for the 'baseline_offset' property.
        /// </summary>
        public static readonly StringName BaselineOffset = "baseline_offset";
        /// <summary>
        /// Cached name for the 'palette_index' property.
        /// </summary>
        public static readonly StringName PaletteIndex = "palette_index";
        /// <summary>
        /// Cached name for the 'palette_custom_colors' property.
        /// </summary>
        public static readonly StringName PaletteCustomColors = "palette_custom_colors";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Font.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_base_font' method.
        /// </summary>
        public static readonly StringName SetBaseFont = "set_base_font";
        /// <summary>
        /// Cached name for the 'get_base_font' method.
        /// </summary>
        public static readonly StringName GetBaseFont = "get_base_font";
        /// <summary>
        /// Cached name for the 'set_variation_opentype' method.
        /// </summary>
        public static readonly StringName SetVariationOpentype = "set_variation_opentype";
        /// <summary>
        /// Cached name for the 'get_variation_opentype' method.
        /// </summary>
        public static readonly StringName GetVariationOpentype = "get_variation_opentype";
        /// <summary>
        /// Cached name for the 'set_variation_embolden' method.
        /// </summary>
        public static readonly StringName SetVariationEmbolden = "set_variation_embolden";
        /// <summary>
        /// Cached name for the 'get_variation_embolden' method.
        /// </summary>
        public static readonly StringName GetVariationEmbolden = "get_variation_embolden";
        /// <summary>
        /// Cached name for the 'set_variation_face_index' method.
        /// </summary>
        public static readonly StringName SetVariationFaceIndex = "set_variation_face_index";
        /// <summary>
        /// Cached name for the 'get_variation_face_index' method.
        /// </summary>
        public static readonly StringName GetVariationFaceIndex = "get_variation_face_index";
        /// <summary>
        /// Cached name for the 'set_variation_transform' method.
        /// </summary>
        public static readonly StringName SetVariationTransform = "set_variation_transform";
        /// <summary>
        /// Cached name for the 'get_variation_transform' method.
        /// </summary>
        public static readonly StringName GetVariationTransform = "get_variation_transform";
        /// <summary>
        /// Cached name for the 'set_opentype_features' method.
        /// </summary>
        public static readonly StringName SetOpentypeFeatures = "set_opentype_features";
        /// <summary>
        /// Cached name for the 'set_spacing' method.
        /// </summary>
        public static readonly StringName SetSpacing = "set_spacing";
        /// <summary>
        /// Cached name for the 'set_baseline_offset' method.
        /// </summary>
        public static readonly StringName SetBaselineOffset = "set_baseline_offset";
        /// <summary>
        /// Cached name for the 'get_baseline_offset' method.
        /// </summary>
        public static readonly StringName GetBaselineOffset = "get_baseline_offset";
        /// <summary>
        /// Cached name for the 'get_palette_index' method.
        /// </summary>
        public static readonly StringName GetPaletteIndex = "get_palette_index";
        /// <summary>
        /// Cached name for the 'set_palette_index' method.
        /// </summary>
        public static readonly StringName SetPaletteIndex = "set_palette_index";
        /// <summary>
        /// Cached name for the 'get_palette_custom_colors' method.
        /// </summary>
        public static readonly StringName GetPaletteCustomColors = "get_palette_custom_colors";
        /// <summary>
        /// Cached name for the 'set_palette_custom_colors' method.
        /// </summary>
        public static readonly StringName SetPaletteCustomColors = "set_palette_custom_colors";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Font.SignalName
    {
    }
}
