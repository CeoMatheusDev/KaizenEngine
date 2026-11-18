namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.SystemFont"/> loads a font from a system font with the first matching name from <see cref="Kaizen.SystemFont.FontNames"/>.</para>
/// <para>It will attempt to match font style, but it's not guaranteed.</para>
/// <para>The returned font might be part of a font collection or be a variable font with OpenType "weight", "width" and/or "italic" features set.</para>
/// <para>You can create <see cref="Kaizen.FontVariation"/> of the system font for precise control over its features.</para>
/// <para><b>Note:</b> This class is implemented on iOS, Linux, macOS and Windows, on other platforms it will fallback to default theme font.</para>
/// </summary>
public partial class SystemFont : Font
{
    /// <summary>
    /// <para>Array of font family names to search, first matching font found is used.</para>
    /// </summary>
    public string[] FontNames
    {
        get
        {
            return GetFontNames();
        }
        set
        {
            SetFontNames(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, italic or oblique font is preferred.</para>
    /// </summary>
    public bool FontItalic
    {
        get
        {
            return GetFontItalic();
        }
        set
        {
            SetFontItalic(value);
        }
    }

    /// <summary>
    /// <para>Preferred weight (boldness) of the font. A value in the <c>100...999</c> range, normal font weight is <c>400</c>, bold font weight is <c>700</c>.</para>
    /// </summary>
    public int FontWeight
    {
        get
        {
            return GetFontWeight();
        }
        set
        {
            SetFontWeight(value);
        }
    }

    /// <summary>
    /// <para>Preferred font stretch amount, compared to a normal width. A percentage value between <c>50%</c> and <c>200%</c>.</para>
    /// </summary>
    public int FontStretch
    {
        get
        {
            return GetFontStretch();
        }
        set
        {
            SetFontStretch(value);
        }
    }

    /// <summary>
    /// <para>Font anti-aliasing mode.</para>
    /// </summary>
    public TextServer.FontAntialiasing Antialiasing
    {
        get
        {
            return GetAntialiasing();
        }
        set
        {
            SetAntialiasing(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, generate mipmaps for the font textures.</para>
    /// </summary>
    public bool GenerateMipmaps
    {
        get
        {
            return GetGenerateMipmaps();
        }
        set
        {
            SetGenerateMipmaps(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, embedded font bitmap loading is disabled (bitmap-only and color fonts ignore this property).</para>
    /// </summary>
    public bool DisableEmbeddedBitmaps
    {
        get
        {
            return GetDisableEmbeddedBitmaps();
        }
        set
        {
            SetDisableEmbeddedBitmaps(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, system fonts can be automatically used as fallbacks.</para>
    /// </summary>
    public bool AllowSystemFallback
    {
        get
        {
            return IsAllowSystemFallback();
        }
        set
        {
            SetAllowSystemFallback(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, auto-hinting is supported and preferred over font built-in hinting.</para>
    /// </summary>
    public bool ForceAutohinter
    {
        get
        {
            return IsForceAutohinter();
        }
        set
        {
            SetForceAutohinter(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, color modulation is applied when drawing colored glyphs, otherwise it's applied to the monochrome glyphs only.</para>
    /// </summary>
    public bool ModulateColorGlyphs
    {
        get
        {
            return IsModulateColorGlyphs();
        }
        set
        {
            SetModulateColorGlyphs(value);
        }
    }

    /// <summary>
    /// <para>Font hinting mode.</para>
    /// </summary>
    public TextServer.Hinting Hinting
    {
        get
        {
            return GetHinting();
        }
        set
        {
            SetHinting(value);
        }
    }

    /// <summary>
    /// <para>Font glyph subpixel positioning mode. Subpixel positioning provides shaper text and better kerning for smaller font sizes, at the cost of memory usage and font rasterization speed. Use <see cref="Kaizen.TextServer.SubpixelPositioning.Auto"/> to automatically enable it based on the font size.</para>
    /// </summary>
    public TextServer.SubpixelPositioning SubpixelPositioning
    {
        get
        {
            return GetSubpixelPositioning();
        }
        set
        {
            SetSubpixelPositioning(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, when aligning glyphs to the pixel boundaries rounding remainders are accumulated to ensure more uniform glyph distribution. This setting has no effect if subpixel positioning is enabled.</para>
    /// </summary>
    public bool KeepRoundingRemainders
    {
        get
        {
            return GetKeepRoundingRemainders();
        }
        set
        {
            SetKeepRoundingRemainders(value);
        }
    }

    /// <summary>
    /// <para>If set to <see langword="true"/>, glyphs of all sizes are rendered using single multichannel signed distance field generated from the dynamic font vector data.</para>
    /// </summary>
    public bool MultichannelSignedDistanceField
    {
        get
        {
            return IsMultichannelSignedDistanceField();
        }
        set
        {
            SetMultichannelSignedDistanceField(value);
        }
    }

    /// <summary>
    /// <para>The width of the range around the shape between the minimum and maximum representable signed distance. If using font outlines, <see cref="Kaizen.SystemFont.MsdfPixelRange"/> must be set to at least <i>twice</i> the size of the largest font outline. The default <see cref="Kaizen.SystemFont.MsdfPixelRange"/> value of <c>16</c> allows outline sizes up to <c>8</c> to look correct.</para>
    /// </summary>
    public int MsdfPixelRange
    {
        get
        {
            return GetMsdfPixelRange();
        }
        set
        {
            SetMsdfPixelRange(value);
        }
    }

    /// <summary>
    /// <para>Source font size used to generate MSDF textures. Higher values allow for more precision, but are slower to render and require more memory. Only increase this value if you notice a visible lack of precision in glyph rendering.</para>
    /// </summary>
    public int MsdfSize
    {
        get
        {
            return GetMsdfSize();
        }
        set
        {
            SetMsdfSize(value);
        }
    }

    /// <summary>
    /// <para>If set to a positive value, overrides the oversampling factor of the viewport this font is used in. See <see cref="Kaizen.Viewport.Oversampling"/>. This value doesn't override the <c>oversampling</c> parameter of <c>draw_*</c> methods.</para>
    /// </summary>
    public float Oversampling
    {
        get
        {
            return GetOversampling();
        }
        set
        {
            SetOversampling(value);
        }
    }

    private static readonly System.Type CachedType = typeof(SystemFont);

    private static readonly StringName NativeName = "SystemFont";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public SystemFont() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal SystemFont(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal SystemFont(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAntialiasing, 1669900ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAntialiasing(TextServer.FontAntialiasing antialiasing)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), (int)antialiasing);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAntialiasing, 4262718649ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextServer.FontAntialiasing GetAntialiasing()
    {
        TextServer.FontAntialiasing ret = (TextServer.FontAntialiasing)NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDisableEmbeddedBitmaps, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDisableEmbeddedBitmaps(bool disableEmbeddedBitmaps)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind2, KaizenObject.GetPtr(this), disableEmbeddedBitmaps.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDisableEmbeddedBitmaps, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetDisableEmbeddedBitmaps()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind3, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetGenerateMipmaps, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetGenerateMipmaps(bool generateMipmaps)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind4, KaizenObject.GetPtr(this), generateMipmaps.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetGenerateMipmaps, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetGenerateMipmaps()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind5, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAllowSystemFallback, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAllowSystemFallback(bool allowSystemFallback)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind6, KaizenObject.GetPtr(this), allowSystemFallback.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAllowSystemFallback, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsAllowSystemFallback()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind7, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetForceAutohinter, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetForceAutohinter(bool forceAutohinter)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind8, KaizenObject.GetPtr(this), forceAutohinter.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsForceAutohinter, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsForceAutohinter()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind9, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetModulateColorGlyphs, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetModulateColorGlyphs(bool modulate)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind10, KaizenObject.GetPtr(this), modulate.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsModulateColorGlyphs, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsModulateColorGlyphs()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind11, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetHinting, 1827459492ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetHinting(TextServer.Hinting hinting)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind12, KaizenObject.GetPtr(this), (int)hinting);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHinting, 3683214614ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextServer.Hinting GetHinting()
    {
        TextServer.Hinting ret = (TextServer.Hinting)NativeCalls.kaizen_icall_0_39(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSubpixelPositioning, 4225742182ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSubpixelPositioning(TextServer.SubpixelPositioning subpixelPositioning)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind14, KaizenObject.GetPtr(this), (int)subpixelPositioning);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSubpixelPositioning, 1069238588ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public TextServer.SubpixelPositioning GetSubpixelPositioning()
    {
        TextServer.SubpixelPositioning ret = (TextServer.SubpixelPositioning)NativeCalls.kaizen_icall_0_39(MethodBind15, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetKeepRoundingRemainders, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetKeepRoundingRemainders(bool keepRoundingRemainders)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind16, KaizenObject.GetPtr(this), keepRoundingRemainders.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKeepRoundingRemainders, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetKeepRoundingRemainders()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind17, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMultichannelSignedDistanceField, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMultichannelSignedDistanceField(bool msdf)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind18, KaizenObject.GetPtr(this), msdf.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsMultichannelSignedDistanceField, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsMultichannelSignedDistanceField()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind19, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMsdfPixelRange, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMsdfPixelRange(int msdfPixelRange)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind20, KaizenObject.GetPtr(this), msdfPixelRange);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMsdfPixelRange, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetMsdfPixelRange()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind21, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMsdfSize, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMsdfSize(int msdfSize)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind22, KaizenObject.GetPtr(this), msdfSize);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMsdfSize, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetMsdfSize()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind23, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetOversampling, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetOversampling(float oversampling)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind24, KaizenObject.GetPtr(this), oversampling);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetOversampling, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetOversampling()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind25, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontNames, 1139954409ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string[] GetFontNames()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind26, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFontNames, 4015028928ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFontNames(string[] names)
    {
        NativeCalls.kaizen_icall_1_210(MethodBind27, KaizenObject.GetPtr(this), names);
        global::System.GC.KeepAlive(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFontNames(ReadOnlySpan<string> names)
    {
        NativeCalls.kaizen_icall_1_210(MethodBind27, KaizenObject.GetPtr(this), names);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontItalic, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetFontItalic()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind28, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFontItalic, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFontItalic(bool italic)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind29, KaizenObject.GetPtr(this), italic.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFontWeight, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFontWeight(int weight)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind30, KaizenObject.GetPtr(this), weight);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFontStretch, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFontStretch(int stretch)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind31, KaizenObject.GetPtr(this), stretch);
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
        /// Cached name for the 'font_names' property.
        /// </summary>
        public static readonly StringName FontNames = "font_names";
        /// <summary>
        /// Cached name for the 'font_italic' property.
        /// </summary>
        public static readonly StringName FontItalic = "font_italic";
        /// <summary>
        /// Cached name for the 'font_weight' property.
        /// </summary>
        public static readonly StringName FontWeight = "font_weight";
        /// <summary>
        /// Cached name for the 'font_stretch' property.
        /// </summary>
        public static readonly StringName FontStretch = "font_stretch";
        /// <summary>
        /// Cached name for the 'antialiasing' property.
        /// </summary>
        public static readonly StringName Antialiasing = "antialiasing";
        /// <summary>
        /// Cached name for the 'generate_mipmaps' property.
        /// </summary>
        public static readonly StringName GenerateMipmaps = "generate_mipmaps";
        /// <summary>
        /// Cached name for the 'disable_embedded_bitmaps' property.
        /// </summary>
        public static readonly StringName DisableEmbeddedBitmaps = "disable_embedded_bitmaps";
        /// <summary>
        /// Cached name for the 'allow_system_fallback' property.
        /// </summary>
        public static readonly StringName AllowSystemFallback = "allow_system_fallback";
        /// <summary>
        /// Cached name for the 'force_autohinter' property.
        /// </summary>
        public static readonly StringName ForceAutohinter = "force_autohinter";
        /// <summary>
        /// Cached name for the 'modulate_color_glyphs' property.
        /// </summary>
        public static readonly StringName ModulateColorGlyphs = "modulate_color_glyphs";
        /// <summary>
        /// Cached name for the 'hinting' property.
        /// </summary>
        public static readonly StringName Hinting = "hinting";
        /// <summary>
        /// Cached name for the 'subpixel_positioning' property.
        /// </summary>
        public static readonly StringName SubpixelPositioning = "subpixel_positioning";
        /// <summary>
        /// Cached name for the 'keep_rounding_remainders' property.
        /// </summary>
        public static readonly StringName KeepRoundingRemainders = "keep_rounding_remainders";
        /// <summary>
        /// Cached name for the 'multichannel_signed_distance_field' property.
        /// </summary>
        public static readonly StringName MultichannelSignedDistanceField = "multichannel_signed_distance_field";
        /// <summary>
        /// Cached name for the 'msdf_pixel_range' property.
        /// </summary>
        public static readonly StringName MsdfPixelRange = "msdf_pixel_range";
        /// <summary>
        /// Cached name for the 'msdf_size' property.
        /// </summary>
        public static readonly StringName MsdfSize = "msdf_size";
        /// <summary>
        /// Cached name for the 'oversampling' property.
        /// </summary>
        public static readonly StringName Oversampling = "oversampling";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Font.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_antialiasing' method.
        /// </summary>
        public static readonly StringName SetAntialiasing = "set_antialiasing";
        /// <summary>
        /// Cached name for the 'get_antialiasing' method.
        /// </summary>
        public static readonly StringName GetAntialiasing = "get_antialiasing";
        /// <summary>
        /// Cached name for the 'set_disable_embedded_bitmaps' method.
        /// </summary>
        public static readonly StringName SetDisableEmbeddedBitmaps = "set_disable_embedded_bitmaps";
        /// <summary>
        /// Cached name for the 'get_disable_embedded_bitmaps' method.
        /// </summary>
        public static readonly StringName GetDisableEmbeddedBitmaps = "get_disable_embedded_bitmaps";
        /// <summary>
        /// Cached name for the 'set_generate_mipmaps' method.
        /// </summary>
        public static readonly StringName SetGenerateMipmaps = "set_generate_mipmaps";
        /// <summary>
        /// Cached name for the 'get_generate_mipmaps' method.
        /// </summary>
        public static readonly StringName GetGenerateMipmaps = "get_generate_mipmaps";
        /// <summary>
        /// Cached name for the 'set_allow_system_fallback' method.
        /// </summary>
        public static readonly StringName SetAllowSystemFallback = "set_allow_system_fallback";
        /// <summary>
        /// Cached name for the 'is_allow_system_fallback' method.
        /// </summary>
        public static readonly StringName IsAllowSystemFallback = "is_allow_system_fallback";
        /// <summary>
        /// Cached name for the 'set_force_autohinter' method.
        /// </summary>
        public static readonly StringName SetForceAutohinter = "set_force_autohinter";
        /// <summary>
        /// Cached name for the 'is_force_autohinter' method.
        /// </summary>
        public static readonly StringName IsForceAutohinter = "is_force_autohinter";
        /// <summary>
        /// Cached name for the 'set_modulate_color_glyphs' method.
        /// </summary>
        public static readonly StringName SetModulateColorGlyphs = "set_modulate_color_glyphs";
        /// <summary>
        /// Cached name for the 'is_modulate_color_glyphs' method.
        /// </summary>
        public static readonly StringName IsModulateColorGlyphs = "is_modulate_color_glyphs";
        /// <summary>
        /// Cached name for the 'set_hinting' method.
        /// </summary>
        public static readonly StringName SetHinting = "set_hinting";
        /// <summary>
        /// Cached name for the 'get_hinting' method.
        /// </summary>
        public static readonly StringName GetHinting = "get_hinting";
        /// <summary>
        /// Cached name for the 'set_subpixel_positioning' method.
        /// </summary>
        public static readonly StringName SetSubpixelPositioning = "set_subpixel_positioning";
        /// <summary>
        /// Cached name for the 'get_subpixel_positioning' method.
        /// </summary>
        public static readonly StringName GetSubpixelPositioning = "get_subpixel_positioning";
        /// <summary>
        /// Cached name for the 'set_keep_rounding_remainders' method.
        /// </summary>
        public static readonly StringName SetKeepRoundingRemainders = "set_keep_rounding_remainders";
        /// <summary>
        /// Cached name for the 'get_keep_rounding_remainders' method.
        /// </summary>
        public static readonly StringName GetKeepRoundingRemainders = "get_keep_rounding_remainders";
        /// <summary>
        /// Cached name for the 'set_multichannel_signed_distance_field' method.
        /// </summary>
        public static readonly StringName SetMultichannelSignedDistanceField = "set_multichannel_signed_distance_field";
        /// <summary>
        /// Cached name for the 'is_multichannel_signed_distance_field' method.
        /// </summary>
        public static readonly StringName IsMultichannelSignedDistanceField = "is_multichannel_signed_distance_field";
        /// <summary>
        /// Cached name for the 'set_msdf_pixel_range' method.
        /// </summary>
        public static readonly StringName SetMsdfPixelRange = "set_msdf_pixel_range";
        /// <summary>
        /// Cached name for the 'get_msdf_pixel_range' method.
        /// </summary>
        public static readonly StringName GetMsdfPixelRange = "get_msdf_pixel_range";
        /// <summary>
        /// Cached name for the 'set_msdf_size' method.
        /// </summary>
        public static readonly StringName SetMsdfSize = "set_msdf_size";
        /// <summary>
        /// Cached name for the 'get_msdf_size' method.
        /// </summary>
        public static readonly StringName GetMsdfSize = "get_msdf_size";
        /// <summary>
        /// Cached name for the 'set_oversampling' method.
        /// </summary>
        public static readonly StringName SetOversampling = "set_oversampling";
        /// <summary>
        /// Cached name for the 'get_oversampling' method.
        /// </summary>
        public static readonly StringName GetOversampling = "get_oversampling";
        /// <summary>
        /// Cached name for the 'get_font_names' method.
        /// </summary>
        public static readonly StringName GetFontNames = "get_font_names";
        /// <summary>
        /// Cached name for the 'set_font_names' method.
        /// </summary>
        public static readonly StringName SetFontNames = "set_font_names";
        /// <summary>
        /// Cached name for the 'get_font_italic' method.
        /// </summary>
        public static readonly StringName GetFontItalic = "get_font_italic";
        /// <summary>
        /// Cached name for the 'set_font_italic' method.
        /// </summary>
        public static readonly StringName SetFontItalic = "set_font_italic";
        /// <summary>
        /// Cached name for the 'set_font_weight' method.
        /// </summary>
        public static readonly StringName SetFontWeight = "set_font_weight";
        /// <summary>
        /// Cached name for the 'set_font_stretch' method.
        /// </summary>
        public static readonly StringName SetFontStretch = "set_font_stretch";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Font.SignalName
    {
    }
}
