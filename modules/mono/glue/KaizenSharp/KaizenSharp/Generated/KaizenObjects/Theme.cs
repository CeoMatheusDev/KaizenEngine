namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A resource used for styling/skinning <see cref="Kaizen.Control"/> and <see cref="Kaizen.Window"/> nodes. While individual controls can be styled using their local theme overrides (see <see cref="Kaizen.Control.AddThemeColorOverride(StringName, Color)"/>), theme resources allow you to store and apply the same settings across all controls sharing the same type (e.g. style all <see cref="Kaizen.Button"/>s the same). One theme resource can be used for the entire project, but you can also set a separate theme resource to a branch of control nodes. A theme resource assigned to a control applies to the control itself, as well as all of its direct and indirect children (as long as a chain of controls is uninterrupted).</para>
/// <para>Use <c>ProjectSettings.gui/theme/custom</c> to set up a project-scope theme that will be available to every control in your project.</para>
/// <para>Use <see cref="Kaizen.Control.Theme"/> of any control node to set up a theme that will be available to that control and all of its direct and indirect children.</para>
/// </summary>
public partial class Theme : Resource
{
    public enum DataType : long
    {
        /// <summary>
        /// <para>Theme's <see cref="Kaizen.Color"/> item type.</para>
        /// </summary>
        Color = 0,
        /// <summary>
        /// <para>Theme's constant item type.</para>
        /// </summary>
        Constant = 1,
        /// <summary>
        /// <para>Theme's <see cref="Kaizen.Font"/> item type.</para>
        /// </summary>
        Font = 2,
        /// <summary>
        /// <para>Theme's font size item type.</para>
        /// </summary>
        FontSize = 3,
        /// <summary>
        /// <para>Theme's icon <see cref="Kaizen.Texture2D"/> item type.</para>
        /// </summary>
        Icon = 4,
        /// <summary>
        /// <para>Theme's <see cref="Kaizen.StyleBox"/> item type.</para>
        /// </summary>
        Stylebox = 5,
        /// <summary>
        /// <para>Theme's <see cref="Kaizen.AudioStream"/> item type.</para>
        /// </summary>
        Sound = 6,
        /// <summary>
        /// <para>Maximum value for the DataType enum.</para>
        /// </summary>
        Max = 7
    }

    /// <summary>
    /// <para>The default base scale factor of this theme resource. Used by some controls to scale their visual properties based on the global scale factor. If this value is set to <c>0.0</c>, the global scale factor is used (see <see cref="Kaizen.ThemeDB.FallbackBaseScale"/>).</para>
    /// <para>Use <see cref="Kaizen.Theme.HasDefaultBaseScale()"/> to check if this value is valid.</para>
    /// </summary>
    public float DefaultBaseScale
    {
        get
        {
            return GetDefaultBaseScale();
        }
        set
        {
            SetDefaultBaseScale(value);
        }
    }

    /// <summary>
    /// <para>The default font of this theme resource. Used as the default value when trying to fetch a font resource that doesn't exist in this theme or is in invalid state. If the default font is also missing or invalid, the engine fallback value is used (see <see cref="Kaizen.ThemeDB.FallbackFont"/>).</para>
    /// <para>Use <see cref="Kaizen.Theme.HasDefaultFont()"/> to check if this value is valid.</para>
    /// </summary>
    public Font DefaultFont
    {
        get
        {
            return GetDefaultFont();
        }
        set
        {
            SetDefaultFont(value);
        }
    }

    /// <summary>
    /// <para>The default font size of this theme resource. Used as the default value when trying to fetch a font size value that doesn't exist in this theme or is in invalid state. If the default font size is also missing or invalid, the engine fallback value is used (see <see cref="Kaizen.ThemeDB.FallbackFontSize"/>).</para>
    /// <para>Values below <c>1</c> are invalid and can be used to unset the property. Use <see cref="Kaizen.Theme.HasDefaultFontSize()"/> to check if this value is valid.</para>
    /// </summary>
    public int DefaultFontSize
    {
        get
        {
            return GetDefaultFontSize();
        }
        set
        {
            SetDefaultFontSize(value);
        }
    }

    private static readonly System.Type CachedType = typeof(Theme);

    private static readonly StringName NativeName = "Theme";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public Theme() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal Theme(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal Theme(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIcon, 2188371082ul);

    /// <summary>
    /// <para>Creates or changes the value of the icon property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearIcon(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public void SetIcon(StringName name, StringName themeType, Texture2D texture)
    {
        NativeCalls.kaizen_icall_3_188(MethodBind0, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), KaizenObject.GetPtr(texture));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIcon, 934555193ul);

    /// <summary>
    /// <para>Returns the icon property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the engine fallback icon value if the property doesn't exist (see <see cref="Kaizen.ThemeDB.FallbackIcon"/>). Use <see cref="Kaizen.Theme.HasIcon(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public Texture2D GetIcon(StringName name, StringName themeType)
    {
        Texture2D ret = (Texture2D)NativeCalls.kaizen_icall_2_366(MethodBind1, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasIcon, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the icon property defined by <paramref name="name"/> and <paramref name="themeType"/> exists.</para>
    /// <para>Returns <see langword="false"/> if it doesn't exist. Use <see cref="Kaizen.Theme.SetIcon(StringName, StringName, Texture2D)"/> to define it.</para>
    /// </summary>
    public bool HasIcon(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind2, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameIcon, 642128662ul);

    /// <summary>
    /// <para>Renames the icon property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasIcon(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearIcon(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameIcon(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind3, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearIcon, 3740211285ul);

    /// <summary>
    /// <para>Removes the icon property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasIcon(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearIcon(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind4, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIconList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for icon properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetIconTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetIconList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind5, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIconTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for icon properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetIconTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind6, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetStylebox, 2075907568ul);

    /// <summary>
    /// <para>Creates or changes the value of the <see cref="Kaizen.StyleBox"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearStylebox(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public void SetStylebox(StringName name, StringName themeType, StyleBox texture)
    {
        NativeCalls.kaizen_icall_3_188(MethodBind7, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), KaizenObject.GetPtr(texture));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStylebox, 3405608165ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.StyleBox"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the engine fallback stylebox value if the property doesn't exist (see <see cref="Kaizen.ThemeDB.FallbackStylebox"/>). Use <see cref="Kaizen.Theme.HasStylebox(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public StyleBox GetStylebox(StringName name, StringName themeType)
    {
        StyleBox ret = (StyleBox)NativeCalls.kaizen_icall_2_366(MethodBind8, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasStylebox, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.StyleBox"/> property defined by <paramref name="name"/> and <paramref name="themeType"/> exists.</para>
    /// <para>Returns <see langword="false"/> if it doesn't exist. Use <see cref="Kaizen.Theme.SetStylebox(StringName, StringName, StyleBox)"/> to define it.</para>
    /// </summary>
    public bool HasStylebox(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind9, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameStylebox, 642128662ul);

    /// <summary>
    /// <para>Renames the <see cref="Kaizen.StyleBox"/> property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasStylebox(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearStylebox(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameStylebox(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind10, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearStylebox, 3740211285ul);

    /// <summary>
    /// <para>Removes the <see cref="Kaizen.StyleBox"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasStylebox(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearStylebox(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind11, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStyleboxList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for <see cref="Kaizen.StyleBox"/> properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetStyleboxTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetStyleboxList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind12, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStyleboxTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for <see cref="Kaizen.StyleBox"/> properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetStyleboxTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFont, 177292320ul);

    /// <summary>
    /// <para>Creates or changes the value of the <see cref="Kaizen.Font"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearFont(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public void SetFont(StringName name, StringName themeType, Font font)
    {
        NativeCalls.kaizen_icall_3_188(MethodBind14, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), KaizenObject.GetPtr(font));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFont, 3445063586ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Font"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the default theme font if the property doesn't exist and the default theme font is set up (see <see cref="Kaizen.Theme.DefaultFont"/>). Use <see cref="Kaizen.Theme.HasFont(StringName, StringName)"/> to check for existence of the property and <see cref="Kaizen.Theme.HasDefaultFont()"/> to check for existence of the default theme font.</para>
    /// <para>Returns the engine fallback font value, if neither exist (see <see cref="Kaizen.ThemeDB.FallbackFont"/>).</para>
    /// </summary>
    public Font GetFont(StringName name, StringName themeType)
    {
        Font ret = (Font)NativeCalls.kaizen_icall_2_366(MethodBind15, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasFont, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.Font"/> property defined by <paramref name="name"/> and <paramref name="themeType"/> exists, or if the default theme font is set up (see <see cref="Kaizen.Theme.HasDefaultFont()"/>).</para>
    /// <para>Returns <see langword="false"/> if neither exist. Use <see cref="Kaizen.Theme.SetFont(StringName, StringName, Font)"/> to define the property.</para>
    /// </summary>
    public bool HasFont(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind16, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameFont, 642128662ul);

    /// <summary>
    /// <para>Renames the <see cref="Kaizen.Font"/> property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasFont(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearFont(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameFont(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind17, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearFont, 3740211285ul);

    /// <summary>
    /// <para>Removes the <see cref="Kaizen.Font"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasFont(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearFont(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind18, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for <see cref="Kaizen.Font"/> properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetFontTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetFontList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind19, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for <see cref="Kaizen.Font"/> properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetFontTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind20, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFontSize, 281601298ul);

    /// <summary>
    /// <para>Creates or changes the value of the font size property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearFontSize(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public void SetFontSize(StringName name, StringName themeType, int fontSize)
    {
        NativeCalls.kaizen_icall_3_1474(MethodBind21, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), fontSize);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontSize, 2419549490ul);

    /// <summary>
    /// <para>Returns the font size property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the default theme font size if the property doesn't exist and the default theme font size is set up (see <see cref="Kaizen.Theme.DefaultFontSize"/>). Use <see cref="Kaizen.Theme.HasFontSize(StringName, StringName)"/> to check for existence of the property and <see cref="Kaizen.Theme.HasDefaultFontSize()"/> to check for existence of the default theme font.</para>
    /// <para>Returns the engine fallback font size value, if neither exist (see <see cref="Kaizen.ThemeDB.FallbackFontSize"/>).</para>
    /// </summary>
    public int GetFontSize(StringName name, StringName themeType)
    {
        int ret = NativeCalls.kaizen_icall_2_367(MethodBind22, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasFontSize, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the font size property defined by <paramref name="name"/> and <paramref name="themeType"/> exists, or if the default theme font size is set up (see <see cref="Kaizen.Theme.HasDefaultFontSize()"/>).</para>
    /// <para>Returns <see langword="false"/> if neither exist. Use <see cref="Kaizen.Theme.SetFontSize(StringName, StringName, int)"/> to define the property.</para>
    /// </summary>
    public bool HasFontSize(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind23, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameFontSize, 642128662ul);

    /// <summary>
    /// <para>Renames the font size property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasFontSize(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearFontSize(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameFontSize(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind24, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearFontSize, 3740211285ul);

    /// <summary>
    /// <para>Removes the font size property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasFontSize(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearFontSize(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind25, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontSizeList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for font size properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetFontSizeTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetFontSizeList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind26, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFontSizeTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for font size properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetFontSizeTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind27, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetColor, 4111215154ul);

    /// <summary>
    /// <para>Creates or changes the value of the <see cref="Kaizen.Color"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearColor(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public unsafe void SetColor(StringName name, StringName themeType, Color color)
    {
        NativeCalls.kaizen_icall_3_1475(MethodBind28, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), &color);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetColor, 2015923404ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Color"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the default color value if the property doesn't exist. Use <see cref="Kaizen.Theme.HasColor(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public Color GetColor(StringName name, StringName themeType)
    {
        Color ret = NativeCalls.kaizen_icall_2_368(MethodBind29, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasColor, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.Color"/> property defined by <paramref name="name"/> and <paramref name="themeType"/> exists.</para>
    /// <para>Returns <see langword="false"/> if it doesn't exist. Use <see cref="Kaizen.Theme.SetColor(StringName, StringName, Color)"/> to define it.</para>
    /// </summary>
    public bool HasColor(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind30, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameColor, 642128662ul);

    /// <summary>
    /// <para>Renames the <see cref="Kaizen.Color"/> property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasColor(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearColor(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameColor(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind31, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearColor, 3740211285ul);

    /// <summary>
    /// <para>Removes the <see cref="Kaizen.Color"/> property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasColor(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearColor(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind32, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetColorList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for <see cref="Kaizen.Color"/> properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetColorTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetColorList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind33, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind34 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetColorTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for <see cref="Kaizen.Color"/> properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetColorTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind34, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind35 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetConstant, 281601298ul);

    /// <summary>
    /// <para>Creates or changes the value of the constant property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearConstant(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public void SetConstant(StringName name, StringName themeType, int constant)
    {
        NativeCalls.kaizen_icall_3_1474(MethodBind35, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), constant);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind36 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetConstant, 2419549490ul);

    /// <summary>
    /// <para>Returns the constant property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns <c>0</c> if the property doesn't exist. Use <see cref="Kaizen.Theme.HasConstant(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public int GetConstant(StringName name, StringName themeType)
    {
        int ret = NativeCalls.kaizen_icall_2_367(MethodBind36, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind37 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasConstant, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the constant property defined by <paramref name="name"/> and <paramref name="themeType"/> exists.</para>
    /// <para>Returns <see langword="false"/> if it doesn't exist. Use <see cref="Kaizen.Theme.SetConstant(StringName, StringName, int)"/> to define it.</para>
    /// </summary>
    public bool HasConstant(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind37, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind38 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameConstant, 642128662ul);

    /// <summary>
    /// <para>Renames the constant property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasConstant(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearConstant(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameConstant(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind38, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind39 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearConstant, 3740211285ul);

    /// <summary>
    /// <para>Removes the constant property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasConstant(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearConstant(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind39, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind40 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetConstantList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for constant properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetConstantTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetConstantList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind40, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind41 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetConstantTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for constant properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetConstantTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind41, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind42 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSound, 2577918927ul);

    /// <summary>
    /// <para>Creates or changes the value of the sound property defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearSound(StringName, StringName)"/> to remove the property.</para>
    /// </summary>
    public void SetSound(StringName name, StringName themeType, AudioStream sound)
    {
        NativeCalls.kaizen_icall_3_188(MethodBind42, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), KaizenObject.GetPtr(sound));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind43 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSound, 1175136407ul);

    /// <summary>
    /// <para>Returns the sound property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the engine fallback sound value if the property doesn't exist (see <see cref="Kaizen.ThemeDB.FallbackSound"/>). Use <see cref="Kaizen.Theme.HasSound(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public AudioStream GetSound(StringName name, StringName themeType)
    {
        AudioStream ret = (AudioStream)NativeCalls.kaizen_icall_2_366(MethodBind43, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind44 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasSound, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the sound property defined by <paramref name="name"/> and <paramref name="themeType"/> exists.</para>
    /// <para>Returns <see langword="false"/> if it doesn't exist. Use <see cref="Kaizen.Theme.SetSound(StringName, StringName, AudioStream)"/> to define it.</para>
    /// </summary>
    public bool HasSound(StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind44, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind45 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameSound, 642128662ul);

    /// <summary>
    /// <para>Renames the sound property defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasSound(StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearSound(StringName, StringName)"/> to remove the existing property.</para>
    /// </summary>
    public void RenameSound(StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1473(MethodBind45, KaizenObject.GetPtr(this), (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind46 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearSound, 3740211285ul);

    /// <summary>
    /// <para>Removes the sound property defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasSound(StringName, StringName)"/> to check for existence.</para>
    /// </summary>
    public void ClearSound(StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind46, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind47 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSoundList, 4291131558ul);

    /// <summary>
    /// <para>Returns a list of names for sound properties defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetSoundTypeList()"/> to get a list of possible theme type names.</para>
    /// </summary>
    public string[] GetSoundList(string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_358(MethodBind47, KaizenObject.GetPtr(this), themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind48 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSoundTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for sound properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// </summary>
    public string[] GetSoundTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind48, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind49 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDefaultBaseScale, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDefaultBaseScale(float baseScale)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind49, KaizenObject.GetPtr(this), baseScale);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind50 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDefaultBaseScale, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetDefaultBaseScale()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind50, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind51 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasDefaultBaseScale, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <see cref="Kaizen.Theme.DefaultBaseScale"/> has a valid value.</para>
    /// <para>Returns <see langword="false"/> if it doesn't. The value must be greater than <c>0.0</c> to be considered valid.</para>
    /// </summary>
    public bool HasDefaultBaseScale()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind51, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind52 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDefaultFont, 1262170328ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDefaultFont(Font font)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind52, KaizenObject.GetPtr(this), KaizenObject.GetPtr(font));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind53 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDefaultFont, 3229501585ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Font GetDefaultFont()
    {
        Font ret = (Font)NativeCalls.kaizen_icall_0_87(MethodBind53, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind54 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasDefaultFont, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <see cref="Kaizen.Theme.DefaultFont"/> has a valid value.</para>
    /// <para>Returns <see langword="false"/> if it doesn't.</para>
    /// </summary>
    public bool HasDefaultFont()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind54, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind55 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDefaultFontSize, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDefaultFontSize(int fontSize)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind55, KaizenObject.GetPtr(this), fontSize);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind56 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDefaultFontSize, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetDefaultFontSize()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind56, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind57 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasDefaultFontSize, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <see cref="Kaizen.Theme.DefaultFontSize"/> has a valid value.</para>
    /// <para>Returns <see langword="false"/> if it doesn't. The value must be greater than <c>0</c> to be considered valid.</para>
    /// </summary>
    public bool HasDefaultFontSize()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind57, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind58 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetThemeItem, 2492983623ul);

    /// <summary>
    /// <para>Creates or changes the value of the theme property of <paramref name="dataType"/> defined by <paramref name="name"/> and <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.ClearThemeItem(Theme.DataType, StringName, StringName)"/> to remove the property.</para>
    /// <para>Fails if the <paramref name="value"/> type is not accepted by <paramref name="dataType"/>.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public void SetThemeItem(Theme.DataType dataType, StringName name, StringName themeType, Variant value)
    {
        NativeCalls.kaizen_icall_4_1476(MethodBind58, KaizenObject.GetPtr(this), (int)dataType, (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default), value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind59 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetThemeItem, 2191024021ul);

    /// <summary>
    /// <para>Returns the theme property of <paramref name="dataType"/> defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Returns the engine fallback value if the property doesn't exist (see <see cref="Kaizen.ThemeDB"/>). Use <see cref="Kaizen.Theme.HasThemeItem(Theme.DataType, StringName, StringName)"/> to check for existence.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public Variant GetThemeItem(Theme.DataType dataType, StringName name, StringName themeType)
    {
        Variant ret = NativeCalls.kaizen_icall_3_1477(MethodBind59, KaizenObject.GetPtr(this), (int)dataType, (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind60 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasThemeItem, 1739311056ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the theme property of <paramref name="dataType"/> defined by <paramref name="name"/> and <paramref name="themeType"/> exists.</para>
    /// <para>Returns <see langword="false"/> if it doesn't exist. Use <see cref="Kaizen.Theme.SetThemeItem(Theme.DataType, StringName, StringName, Variant)"/> to define it.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public bool HasThemeItem(Theme.DataType dataType, StringName name, StringName themeType)
    {
        bool ret = NativeCalls.kaizen_icall_3_1478(MethodBind60, KaizenObject.GetPtr(this), (int)dataType, (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind61 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameThemeItem, 3900867553ul);

    /// <summary>
    /// <para>Renames the theme property of <paramref name="dataType"/> defined by <paramref name="oldName"/> and <paramref name="themeType"/> to <paramref name="name"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist, or if a similar property with the new name already exists. Use <see cref="Kaizen.Theme.HasThemeItem(Theme.DataType, StringName, StringName)"/> to check for existence, and <see cref="Kaizen.Theme.ClearThemeItem(Theme.DataType, StringName, StringName)"/> to remove the existing property.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public void RenameThemeItem(Theme.DataType dataType, StringName oldName, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_4_1479(MethodBind61, KaizenObject.GetPtr(this), (int)dataType, (kaizen_string_name)(oldName?.NativeValue ?? default), (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind62 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearThemeItem, 2965505587ul);

    /// <summary>
    /// <para>Removes the theme property of <paramref name="dataType"/> defined by <paramref name="name"/> and <paramref name="themeType"/>, if it exists.</para>
    /// <para>Fails if it doesn't exist. Use <see cref="Kaizen.Theme.HasThemeItem(Theme.DataType, StringName, StringName)"/> to check for existence.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public void ClearThemeItem(Theme.DataType dataType, StringName name, StringName themeType)
    {
        NativeCalls.kaizen_icall_3_1480(MethodBind62, KaizenObject.GetPtr(this), (int)dataType, (kaizen_string_name)(name?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind63 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetThemeItemList, 3726716710ul);

    /// <summary>
    /// <para>Returns a list of names for properties of <paramref name="dataType"/> defined with <paramref name="themeType"/>. Use <see cref="Kaizen.Theme.GetThemeItemTypeList(Theme.DataType)"/> to get a list of possible theme type names.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public string[] GetThemeItemList(Theme.DataType dataType, string themeType)
    {
        string[] ret = NativeCalls.kaizen_icall_2_1481(MethodBind63, KaizenObject.GetPtr(this), (int)dataType, themeType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind64 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetThemeItemTypeList, 1316004935ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names for <paramref name="dataType"/> properties. Use <see cref="Kaizen.Theme.GetTypeList()"/> to get a list of all unique theme types.</para>
    /// <para><b>Note:</b> This method is analogous to calling the corresponding data type specific method, but can be used for more generalized logic.</para>
    /// </summary>
    public string[] GetThemeItemTypeList(Theme.DataType dataType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_487(MethodBind64, KaizenObject.GetPtr(this), (int)dataType);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind65 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTypeVariation, 3740211285ul);

    /// <summary>
    /// <para>Marks <paramref name="themeType"/> as a variation of <paramref name="baseType"/>.</para>
    /// <para>This adds <paramref name="themeType"/> as a suggested option for <see cref="Kaizen.Control.ThemeTypeVariation"/> on a <see cref="Kaizen.Control"/> that is of the <paramref name="baseType"/> class.</para>
    /// <para>Variations can also be nested, i.e. <paramref name="baseType"/> can be another variation. If a chain of variations ends with a <paramref name="baseType"/> matching the class of the <see cref="Kaizen.Control"/>, the whole chain is going to be suggested as options.</para>
    /// </summary>
    public void SetTypeVariation(StringName themeType, StringName baseType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind65, KaizenObject.GetPtr(this), (kaizen_string_name)(themeType?.NativeValue ?? default), (kaizen_string_name)(baseType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind66 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsTypeVariation, 471820014ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <paramref name="themeType"/> is marked as a variation of <paramref name="baseType"/>.</para>
    /// </summary>
    public bool IsTypeVariation(StringName themeType, StringName baseType)
    {
        bool ret = NativeCalls.kaizen_icall_2_187(MethodBind66, KaizenObject.GetPtr(this), (kaizen_string_name)(themeType?.NativeValue ?? default), (kaizen_string_name)(baseType?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind67 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearTypeVariation, 3304788590ul);

    /// <summary>
    /// <para>Unmarks <paramref name="themeType"/> as being a variation of another theme type. See <see cref="Kaizen.Theme.SetTypeVariation(StringName, StringName)"/>.</para>
    /// </summary>
    public void ClearTypeVariation(StringName themeType)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind67, KaizenObject.GetPtr(this), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind68 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTypeVariationBase, 1965194235ul);

    /// <summary>
    /// <para>Returns the name of the base theme type if <paramref name="themeType"/> is a valid variation type. Returns an empty string otherwise.</para>
    /// </summary>
    public StringName GetTypeVariationBase(StringName themeType)
    {
        StringName ret = NativeCalls.kaizen_icall_1_190(MethodBind68, KaizenObject.GetPtr(this), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind69 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTypeVariationList, 1761182771ul);

    /// <summary>
    /// <para>Returns a list of all type variations for the given <paramref name="baseType"/>.</para>
    /// </summary>
    public string[] GetTypeVariationList(StringName baseType)
    {
        string[] ret = NativeCalls.kaizen_icall_1_317(MethodBind69, KaizenObject.GetPtr(this), (kaizen_string_name)(baseType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind70 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddType, 3304788590ul);

    /// <summary>
    /// <para>Adds an empty theme type for every valid data type.</para>
    /// <para><b>Note:</b> Empty types are not saved with the theme. This method only exists to perform in-memory changes to the resource. Use available <c>set_*</c> methods to add theme items.</para>
    /// </summary>
    public void AddType(StringName themeType)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind70, KaizenObject.GetPtr(this), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind71 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RemoveType, 3304788590ul);

    /// <summary>
    /// <para>Removes the theme type, gracefully discarding defined theme items. If the type is a variation, this information is also erased. If the type is a base for type variations, those variations lose their base.</para>
    /// </summary>
    public void RemoveType(StringName themeType)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind71, KaizenObject.GetPtr(this), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind72 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RenameType, 3740211285ul);

    /// <summary>
    /// <para>Renames the theme type <paramref name="oldThemeType"/> to <paramref name="themeType"/>, if the old type exists and the new one doesn't exist.</para>
    /// <para><b>Note:</b> Renaming a theme type to an empty name or a variation to a type associated with a built-in class removes type variation connections in a way that cannot be undone by reversing the rename alone.</para>
    /// </summary>
    public void RenameType(StringName oldThemeType, StringName themeType)
    {
        NativeCalls.kaizen_icall_2_144(MethodBind72, KaizenObject.GetPtr(this), (kaizen_string_name)(oldThemeType?.NativeValue ?? default), (kaizen_string_name)(themeType?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind73 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTypeList, 1139954409ul);

    /// <summary>
    /// <para>Returns a list of all unique theme type names. Use the appropriate <c>get_*_type_list</c> method to get a list of unique theme types for a single data type.</para>
    /// </summary>
    public string[] GetTypeList()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind73, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind74 = ClassDB_get_method_with_compatibility(NativeName, MethodName.MergeWith, 2326690814ul);

    /// <summary>
    /// <para>Adds missing and overrides existing definitions with values from the <paramref name="other"/> theme resource.</para>
    /// <para><b>Note:</b> This modifies the current theme. If you want to merge two themes together without modifying either one, create a new empty theme and merge the other two into it one after another.</para>
    /// </summary>
    public void MergeWith(Theme other)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind74, KaizenObject.GetPtr(this), KaizenObject.GetPtr(other));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind75 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Clear, 3218959716ul);

    /// <summary>
    /// <para>Removes all the theme properties defined on the theme resource.</para>
    /// </summary>
    public void Clear()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind75, KaizenObject.GetPtr(this));
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
    public new class PropertyName : Resource.PropertyName
    {
        /// <summary>
        /// Cached name for the 'default_base_scale' property.
        /// </summary>
        public static readonly StringName DefaultBaseScale = "default_base_scale";
        /// <summary>
        /// Cached name for the 'default_font' property.
        /// </summary>
        public static readonly StringName DefaultFont = "default_font";
        /// <summary>
        /// Cached name for the 'default_font_size' property.
        /// </summary>
        public static readonly StringName DefaultFontSize = "default_font_size";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_icon' method.
        /// </summary>
        public static readonly StringName SetIcon = "set_icon";
        /// <summary>
        /// Cached name for the 'get_icon' method.
        /// </summary>
        public static readonly StringName GetIcon = "get_icon";
        /// <summary>
        /// Cached name for the 'has_icon' method.
        /// </summary>
        public static readonly StringName HasIcon = "has_icon";
        /// <summary>
        /// Cached name for the 'rename_icon' method.
        /// </summary>
        public static readonly StringName RenameIcon = "rename_icon";
        /// <summary>
        /// Cached name for the 'clear_icon' method.
        /// </summary>
        public static readonly StringName ClearIcon = "clear_icon";
        /// <summary>
        /// Cached name for the 'get_icon_list' method.
        /// </summary>
        public static readonly StringName GetIconList = "get_icon_list";
        /// <summary>
        /// Cached name for the 'get_icon_type_list' method.
        /// </summary>
        public static readonly StringName GetIconTypeList = "get_icon_type_list";
        /// <summary>
        /// Cached name for the 'set_stylebox' method.
        /// </summary>
        public static readonly StringName SetStylebox = "set_stylebox";
        /// <summary>
        /// Cached name for the 'get_stylebox' method.
        /// </summary>
        public static readonly StringName GetStylebox = "get_stylebox";
        /// <summary>
        /// Cached name for the 'has_stylebox' method.
        /// </summary>
        public static readonly StringName HasStylebox = "has_stylebox";
        /// <summary>
        /// Cached name for the 'rename_stylebox' method.
        /// </summary>
        public static readonly StringName RenameStylebox = "rename_stylebox";
        /// <summary>
        /// Cached name for the 'clear_stylebox' method.
        /// </summary>
        public static readonly StringName ClearStylebox = "clear_stylebox";
        /// <summary>
        /// Cached name for the 'get_stylebox_list' method.
        /// </summary>
        public static readonly StringName GetStyleboxList = "get_stylebox_list";
        /// <summary>
        /// Cached name for the 'get_stylebox_type_list' method.
        /// </summary>
        public static readonly StringName GetStyleboxTypeList = "get_stylebox_type_list";
        /// <summary>
        /// Cached name for the 'set_font' method.
        /// </summary>
        public static readonly StringName SetFont = "set_font";
        /// <summary>
        /// Cached name for the 'get_font' method.
        /// </summary>
        public static readonly StringName GetFont = "get_font";
        /// <summary>
        /// Cached name for the 'has_font' method.
        /// </summary>
        public static readonly StringName HasFont = "has_font";
        /// <summary>
        /// Cached name for the 'rename_font' method.
        /// </summary>
        public static readonly StringName RenameFont = "rename_font";
        /// <summary>
        /// Cached name for the 'clear_font' method.
        /// </summary>
        public static readonly StringName ClearFont = "clear_font";
        /// <summary>
        /// Cached name for the 'get_font_list' method.
        /// </summary>
        public static readonly StringName GetFontList = "get_font_list";
        /// <summary>
        /// Cached name for the 'get_font_type_list' method.
        /// </summary>
        public static readonly StringName GetFontTypeList = "get_font_type_list";
        /// <summary>
        /// Cached name for the 'set_font_size' method.
        /// </summary>
        public static readonly StringName SetFontSize = "set_font_size";
        /// <summary>
        /// Cached name for the 'get_font_size' method.
        /// </summary>
        public static readonly StringName GetFontSize = "get_font_size";
        /// <summary>
        /// Cached name for the 'has_font_size' method.
        /// </summary>
        public static readonly StringName HasFontSize = "has_font_size";
        /// <summary>
        /// Cached name for the 'rename_font_size' method.
        /// </summary>
        public static readonly StringName RenameFontSize = "rename_font_size";
        /// <summary>
        /// Cached name for the 'clear_font_size' method.
        /// </summary>
        public static readonly StringName ClearFontSize = "clear_font_size";
        /// <summary>
        /// Cached name for the 'get_font_size_list' method.
        /// </summary>
        public static readonly StringName GetFontSizeList = "get_font_size_list";
        /// <summary>
        /// Cached name for the 'get_font_size_type_list' method.
        /// </summary>
        public static readonly StringName GetFontSizeTypeList = "get_font_size_type_list";
        /// <summary>
        /// Cached name for the 'set_color' method.
        /// </summary>
        public static readonly StringName SetColor = "set_color";
        /// <summary>
        /// Cached name for the 'get_color' method.
        /// </summary>
        public static readonly StringName GetColor = "get_color";
        /// <summary>
        /// Cached name for the 'has_color' method.
        /// </summary>
        public static readonly StringName HasColor = "has_color";
        /// <summary>
        /// Cached name for the 'rename_color' method.
        /// </summary>
        public static readonly StringName RenameColor = "rename_color";
        /// <summary>
        /// Cached name for the 'clear_color' method.
        /// </summary>
        public static readonly StringName ClearColor = "clear_color";
        /// <summary>
        /// Cached name for the 'get_color_list' method.
        /// </summary>
        public static readonly StringName GetColorList = "get_color_list";
        /// <summary>
        /// Cached name for the 'get_color_type_list' method.
        /// </summary>
        public static readonly StringName GetColorTypeList = "get_color_type_list";
        /// <summary>
        /// Cached name for the 'set_constant' method.
        /// </summary>
        public static readonly StringName SetConstant = "set_constant";
        /// <summary>
        /// Cached name for the 'get_constant' method.
        /// </summary>
        public static readonly StringName GetConstant = "get_constant";
        /// <summary>
        /// Cached name for the 'has_constant' method.
        /// </summary>
        public static readonly StringName HasConstant = "has_constant";
        /// <summary>
        /// Cached name for the 'rename_constant' method.
        /// </summary>
        public static readonly StringName RenameConstant = "rename_constant";
        /// <summary>
        /// Cached name for the 'clear_constant' method.
        /// </summary>
        public static readonly StringName ClearConstant = "clear_constant";
        /// <summary>
        /// Cached name for the 'get_constant_list' method.
        /// </summary>
        public static readonly StringName GetConstantList = "get_constant_list";
        /// <summary>
        /// Cached name for the 'get_constant_type_list' method.
        /// </summary>
        public static readonly StringName GetConstantTypeList = "get_constant_type_list";
        /// <summary>
        /// Cached name for the 'set_sound' method.
        /// </summary>
        public static readonly StringName SetSound = "set_sound";
        /// <summary>
        /// Cached name for the 'get_sound' method.
        /// </summary>
        public static readonly StringName GetSound = "get_sound";
        /// <summary>
        /// Cached name for the 'has_sound' method.
        /// </summary>
        public static readonly StringName HasSound = "has_sound";
        /// <summary>
        /// Cached name for the 'rename_sound' method.
        /// </summary>
        public static readonly StringName RenameSound = "rename_sound";
        /// <summary>
        /// Cached name for the 'clear_sound' method.
        /// </summary>
        public static readonly StringName ClearSound = "clear_sound";
        /// <summary>
        /// Cached name for the 'get_sound_list' method.
        /// </summary>
        public static readonly StringName GetSoundList = "get_sound_list";
        /// <summary>
        /// Cached name for the 'get_sound_type_list' method.
        /// </summary>
        public static readonly StringName GetSoundTypeList = "get_sound_type_list";
        /// <summary>
        /// Cached name for the 'set_default_base_scale' method.
        /// </summary>
        public static readonly StringName SetDefaultBaseScale = "set_default_base_scale";
        /// <summary>
        /// Cached name for the 'get_default_base_scale' method.
        /// </summary>
        public static readonly StringName GetDefaultBaseScale = "get_default_base_scale";
        /// <summary>
        /// Cached name for the 'has_default_base_scale' method.
        /// </summary>
        public static readonly StringName HasDefaultBaseScale = "has_default_base_scale";
        /// <summary>
        /// Cached name for the 'set_default_font' method.
        /// </summary>
        public static readonly StringName SetDefaultFont = "set_default_font";
        /// <summary>
        /// Cached name for the 'get_default_font' method.
        /// </summary>
        public static readonly StringName GetDefaultFont = "get_default_font";
        /// <summary>
        /// Cached name for the 'has_default_font' method.
        /// </summary>
        public static readonly StringName HasDefaultFont = "has_default_font";
        /// <summary>
        /// Cached name for the 'set_default_font_size' method.
        /// </summary>
        public static readonly StringName SetDefaultFontSize = "set_default_font_size";
        /// <summary>
        /// Cached name for the 'get_default_font_size' method.
        /// </summary>
        public static readonly StringName GetDefaultFontSize = "get_default_font_size";
        /// <summary>
        /// Cached name for the 'has_default_font_size' method.
        /// </summary>
        public static readonly StringName HasDefaultFontSize = "has_default_font_size";
        /// <summary>
        /// Cached name for the 'set_theme_item' method.
        /// </summary>
        public static readonly StringName SetThemeItem = "set_theme_item";
        /// <summary>
        /// Cached name for the 'get_theme_item' method.
        /// </summary>
        public static readonly StringName GetThemeItem = "get_theme_item";
        /// <summary>
        /// Cached name for the 'has_theme_item' method.
        /// </summary>
        public static readonly StringName HasThemeItem = "has_theme_item";
        /// <summary>
        /// Cached name for the 'rename_theme_item' method.
        /// </summary>
        public static readonly StringName RenameThemeItem = "rename_theme_item";
        /// <summary>
        /// Cached name for the 'clear_theme_item' method.
        /// </summary>
        public static readonly StringName ClearThemeItem = "clear_theme_item";
        /// <summary>
        /// Cached name for the 'get_theme_item_list' method.
        /// </summary>
        public static readonly StringName GetThemeItemList = "get_theme_item_list";
        /// <summary>
        /// Cached name for the 'get_theme_item_type_list' method.
        /// </summary>
        public static readonly StringName GetThemeItemTypeList = "get_theme_item_type_list";
        /// <summary>
        /// Cached name for the 'set_type_variation' method.
        /// </summary>
        public static readonly StringName SetTypeVariation = "set_type_variation";
        /// <summary>
        /// Cached name for the 'is_type_variation' method.
        /// </summary>
        public static readonly StringName IsTypeVariation = "is_type_variation";
        /// <summary>
        /// Cached name for the 'clear_type_variation' method.
        /// </summary>
        public static readonly StringName ClearTypeVariation = "clear_type_variation";
        /// <summary>
        /// Cached name for the 'get_type_variation_base' method.
        /// </summary>
        public static readonly StringName GetTypeVariationBase = "get_type_variation_base";
        /// <summary>
        /// Cached name for the 'get_type_variation_list' method.
        /// </summary>
        public static readonly StringName GetTypeVariationList = "get_type_variation_list";
        /// <summary>
        /// Cached name for the 'add_type' method.
        /// </summary>
        public static readonly StringName AddType = "add_type";
        /// <summary>
        /// Cached name for the 'remove_type' method.
        /// </summary>
        public static readonly StringName RemoveType = "remove_type";
        /// <summary>
        /// Cached name for the 'rename_type' method.
        /// </summary>
        public static readonly StringName RenameType = "rename_type";
        /// <summary>
        /// Cached name for the 'get_type_list' method.
        /// </summary>
        public static readonly StringName GetTypeList = "get_type_list";
        /// <summary>
        /// Cached name for the 'merge_with' method.
        /// </summary>
        public static readonly StringName MergeWith = "merge_with";
        /// <summary>
        /// Cached name for the 'clear' method.
        /// </summary>
        public static readonly StringName Clear = "clear";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
