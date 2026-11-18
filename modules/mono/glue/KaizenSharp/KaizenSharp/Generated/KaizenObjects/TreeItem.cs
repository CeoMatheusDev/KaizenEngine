namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A single item of a <see cref="Kaizen.Tree"/> control. It can contain other <see cref="Kaizen.TreeItem"/>s as children, which allows it to create a hierarchy. It can also contain text and buttons. <see cref="Kaizen.TreeItem"/> is not a <see cref="Kaizen.Node"/>, it is internal to the <see cref="Kaizen.Tree"/>.</para>
/// <para>To create a <see cref="Kaizen.TreeItem"/>, use <see cref="Kaizen.Tree.CreateItem(TreeItem, int)"/> or <see cref="Kaizen.TreeItem.CreateChild(int)"/>. To remove a <see cref="Kaizen.TreeItem"/>, use <see cref="Kaizen.KaizenObject.Free()"/>.</para>
/// <para><b>Note:</b> The ID values used for buttons are 32-bit, unlike <see cref="int"/> which is always 64-bit. They go from <c>-2147483648</c> to <c>2147483647</c>.</para>
/// </summary>
public partial class TreeItem : KaizenObject
{
    public enum TreeCellMode : long
    {
        /// <summary>
        /// <para>Cell shows a string label, optionally with an icon. When editable, the text can be edited using a <see cref="Kaizen.LineEdit"/>, or a <see cref="Kaizen.TextEdit"/> popup if <see cref="Kaizen.TreeItem.SetEditMultiline(int, bool)"/> is used.</para>
        /// </summary>
        String = 0,
        /// <summary>
        /// <para>Cell shows a checkbox, optionally with text and an icon. The checkbox can be pressed, released, or indeterminate (via <see cref="Kaizen.TreeItem.SetIndeterminate(int, bool)"/>). The checkbox can't be clicked unless the cell is editable.</para>
        /// </summary>
        Check = 1,
        /// <summary>
        /// <para>Cell shows a numeric range. When editable, it can be edited using a range slider. Use <see cref="Kaizen.TreeItem.SetRange(int, double)"/> to set the value and <see cref="Kaizen.TreeItem.SetRangeConfig(int, double, double, double, bool)"/> to configure the range.</para>
        /// <para>This cell can also be used in a text dropdown mode when you assign a text with <see cref="Kaizen.TreeItem.SetText(int, string)"/>. Separate options with a comma, e.g. <c>"Option1,Option2,Option3"</c>.</para>
        /// </summary>
        Range = 2,
        /// <summary>
        /// <para>Cell shows an icon. It can't be edited nor display text. The icon is always centered within the cell.</para>
        /// </summary>
        Icon = 3,
        /// <summary>
        /// <para>Cell shows as a clickable button. It will display an arrow similar to <see cref="Kaizen.OptionButton"/>, but doesn't feature a dropdown (for that you can use <see cref="Kaizen.TreeItem.TreeCellMode.Range"/>). Clicking the button emits the <see cref="Kaizen.Tree.ItemEdited"/> signal. The button is flat by default, you can use <see cref="Kaizen.TreeItem.SetCustomAsButton(int, bool)"/> to display it with a <see cref="Kaizen.StyleBox"/>.</para>
        /// <para>This mode also supports custom drawing using <see cref="Kaizen.TreeItem.SetCustomDrawCallback(int, Callable)"/>.</para>
        /// </summary>
        Custom = 4
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the TreeItem is collapsed.</para>
    /// </summary>
    public bool Collapsed
    {
        get
        {
            return IsCollapsed();
        }
        set
        {
            SetCollapsed(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the <see cref="Kaizen.TreeItem"/> is visible (default).</para>
    /// <para>Note that if a <see cref="Kaizen.TreeItem"/> is set to not be visible, none of its children will be visible either.</para>
    /// </summary>
    public bool Visible
    {
        get
        {
            return IsVisible();
        }
        set
        {
            SetVisible(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, folding is disabled for this TreeItem.</para>
    /// </summary>
    public bool DisableFolding
    {
        get
        {
            return IsFoldingDisabled();
        }
        set
        {
            SetDisableFolding(value);
        }
    }

    /// <summary>
    /// <para>The custom minimum height.</para>
    /// </summary>
    public int CustomMinimumHeight
    {
        get
        {
            return GetCustomMinimumHeight();
        }
        set
        {
            SetCustomMinimumHeight(value);
        }
    }

    private static readonly System.Type CachedType = typeof(TreeItem);

    private static readonly StringName NativeName = "TreeItem";

    internal TreeItem() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal TreeItem(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal TreeItem(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCellMode, 289920701ul);

    /// <summary>
    /// <para>Sets the given column's cell mode to <paramref name="mode"/>. This determines how the cell is displayed and edited.</para>
    /// </summary>
    public void SetCellMode(int column, TreeItem.TreeCellMode mode)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind0, KaizenObject.GetPtr(this), column, (int)mode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCellMode, 3406114978ul);

    /// <summary>
    /// <para>Returns the column's cell mode.</para>
    /// </summary>
    public TreeItem.TreeCellMode GetCellMode(int column)
    {
        TreeItem.TreeCellMode ret = (TreeItem.TreeCellMode)NativeCalls.kaizen_icall_1_85(MethodBind1, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAutoTranslateMode, 287402019ul);

    /// <summary>
    /// <para>Sets the given column's auto translate mode to <paramref name="mode"/>.</para>
    /// <para>All columns use <see cref="Kaizen.Node.AutoTranslateModeEnum.Inherit"/> by default, which uses the same auto translate mode as the <see cref="Kaizen.Tree"/> itself.</para>
    /// </summary>
    public void SetAutoTranslateMode(int column, Node.AutoTranslateModeEnum mode)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind2, KaizenObject.GetPtr(this), column, (int)mode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAutoTranslateMode, 906302372ul);

    /// <summary>
    /// <para>Returns the column's auto translate mode.</para>
    /// </summary>
    public Node.AutoTranslateModeEnum GetAutoTranslateMode(int column)
    {
        Node.AutoTranslateModeEnum ret = (Node.AutoTranslateModeEnum)NativeCalls.kaizen_icall_1_85(MethodBind3, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEditMultiline, 300928843ul);

    /// <summary>
    /// <para>If <paramref name="multiline"/> is <see langword="true"/>, the given <paramref name="column"/> is multiline editable.</para>
    /// <para><b>Note:</b> This option only affects the type of control (<see cref="Kaizen.LineEdit"/> or <see cref="Kaizen.TextEdit"/>) that appears when editing the column. You can set multiline values with <see cref="Kaizen.TreeItem.SetText(int, string)"/> even if the column is not multiline editable.</para>
    /// </summary>
    public void SetEditMultiline(int column, bool multiline)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind4, KaizenObject.GetPtr(this), column, multiline.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsEditMultiline, 1116898809ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given <paramref name="column"/> is multiline editable.</para>
    /// </summary>
    public bool IsEditMultiline(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind5, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetChecked, 300928843ul);

    /// <summary>
    /// <para>If <paramref name="checked"/> is <see langword="true"/>, the given <paramref name="column"/> is checked. Clears column's indeterminate status.</para>
    /// </summary>
    public void SetChecked(int column, bool @checked)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind6, KaizenObject.GetPtr(this), column, @checked.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIndeterminate, 300928843ul);

    /// <summary>
    /// <para>If <paramref name="indeterminate"/> is <see langword="true"/>, the given <paramref name="column"/> is marked indeterminate.</para>
    /// <para><b>Note:</b> If set <see langword="true"/> from <see langword="false"/>, then column is cleared of checked status.</para>
    /// </summary>
    public void SetIndeterminate(int column, bool indeterminate)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind7, KaizenObject.GetPtr(this), column, indeterminate.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsChecked, 1116898809ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given <paramref name="column"/> is checked.</para>
    /// </summary>
    public bool IsChecked(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind8, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsIndeterminate, 1116898809ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given <paramref name="column"/> is indeterminate.</para>
    /// </summary>
    public bool IsIndeterminate(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind9, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.PropagateCheck, 972357352ul);

    /// <summary>
    /// <para>Propagates this item's checked status to its children and parents for the given <paramref name="column"/>. It is possible to process the items affected by this method call by connecting to <see cref="Kaizen.Tree.CheckPropagatedToItem"/>. The order that the items affected will be processed is as follows: the item invoking this method, children of that item, and finally parents of that item. If <paramref name="emitSignal"/> is <see langword="false"/>, then <see cref="Kaizen.Tree.CheckPropagatedToItem"/> will not be emitted.</para>
    /// </summary>
    public void PropagateCheck(int column, bool emitSignal = true)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind10, KaizenObject.GetPtr(this), column, emitSignal.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetText, 501894301ul);

    /// <summary>
    /// <para>Sets the given column's text value.</para>
    /// </summary>
    public void SetText(int column, string text)
    {
        NativeCalls.kaizen_icall_2_206(MethodBind11, KaizenObject.GetPtr(this), column, text);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetText, 844755477ul);

    /// <summary>
    /// <para>Returns the given column's text.</para>
    /// </summary>
    public string GetText(int column)
    {
        string ret = NativeCalls.kaizen_icall_1_158(MethodBind12, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDescription, 501894301ul);

    /// <summary>
    /// <para>Sets the given column's description for assistive apps.</para>
    /// </summary>
    public void SetDescription(int column, string description)
    {
        NativeCalls.kaizen_icall_2_206(MethodBind13, KaizenObject.GetPtr(this), column, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDescription, 844755477ul);

    /// <summary>
    /// <para>Returns the given column's description for assistive apps.</para>
    /// </summary>
    public string GetDescription(int column)
    {
        string ret = NativeCalls.kaizen_icall_1_158(MethodBind14, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTextDirection, 1707680378ul);

    /// <summary>
    /// <para>Sets item's text base writing direction.</para>
    /// </summary>
    public void SetTextDirection(int column, Control.TextDirection direction)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind15, KaizenObject.GetPtr(this), column, (int)direction);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextDirection, 4235602388ul);

    /// <summary>
    /// <para>Returns item's text base writing direction.</para>
    /// </summary>
    public Control.TextDirection GetTextDirection(int column)
    {
        Control.TextDirection ret = (Control.TextDirection)NativeCalls.kaizen_icall_1_85(MethodBind16, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAutowrapMode, 3633006561ul);

    /// <summary>
    /// <para>Sets the autowrap mode in the given <paramref name="column"/>. If set to something other than <see cref="Kaizen.TextServer.AutowrapMode.Off"/>, the text gets wrapped inside the cell's bounding rectangle.</para>
    /// </summary>
    public void SetAutowrapMode(int column, TextServer.AutowrapMode autowrapMode)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind17, KaizenObject.GetPtr(this), column, (int)autowrapMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAutowrapMode, 2902757236ul);

    /// <summary>
    /// <para>Returns the text autowrap mode in the given <paramref name="column"/>. By default it is <see cref="Kaizen.TextServer.AutowrapMode.Off"/>.</para>
    /// </summary>
    public TextServer.AutowrapMode GetAutowrapMode(int column)
    {
        TextServer.AutowrapMode ret = (TextServer.AutowrapMode)NativeCalls.kaizen_icall_1_85(MethodBind18, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAutowrapTrimFlags, 2186029660ul);

    /// <summary>
    /// <para>Sets the autowrap trim flags for the given <paramref name="column"/>. These flags control whether leading and trailing spaces are trimmed on wrapped lines. Set to <c>0</c> to disable all trimming.</para>
    /// </summary>
    public void SetAutowrapTrimFlags(int column, TextServer.LineBreakFlag flags)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind19, KaizenObject.GetPtr(this), column, (int)flags);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAutowrapTrimFlags, 3513056523ul);

    /// <summary>
    /// <para>Returns the autowrap trim flags for the given <paramref name="column"/>. By default, both <see cref="Kaizen.TextServer.LineBreakFlag.TrimStartEdgeSpaces"/> and <see cref="Kaizen.TextServer.LineBreakFlag.TrimEndEdgeSpaces"/> are enabled.</para>
    /// </summary>
    public TextServer.LineBreakFlag GetAutowrapTrimFlags(int column)
    {
        TextServer.LineBreakFlag ret = (TextServer.LineBreakFlag)NativeCalls.kaizen_icall_1_85(MethodBind20, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTextOverrunBehavior, 1940772195ul);

    /// <summary>
    /// <para>Sets the clipping behavior when the text exceeds the item's bounding rectangle in the given <paramref name="column"/>.</para>
    /// </summary>
    public void SetTextOverrunBehavior(int column, TextServer.OverrunBehavior overrunBehavior)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind21, KaizenObject.GetPtr(this), column, (int)overrunBehavior);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextOverrunBehavior, 3782727860ul);

    /// <summary>
    /// <para>Returns the clipping behavior when the text exceeds the item's bounding rectangle in the given <paramref name="column"/>. By default it is <see cref="Kaizen.TextServer.OverrunBehavior.TrimEllipsis"/>.</para>
    /// </summary>
    public TextServer.OverrunBehavior GetTextOverrunBehavior(int column)
    {
        TextServer.OverrunBehavior ret = (TextServer.OverrunBehavior)NativeCalls.kaizen_icall_1_85(MethodBind22, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetStructuredTextBidiOverride, 868756907ul);

    /// <summary>
    /// <para>Set BiDi algorithm override for the structured text. Has effect for cells that display text.</para>
    /// </summary>
    public void SetStructuredTextBidiOverride(int column, TextServer.StructuredTextParser parser)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind23, KaizenObject.GetPtr(this), column, (int)parser);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStructuredTextBidiOverride, 3377823772ul);

    /// <summary>
    /// <para>Returns the BiDi algorithm override set for this cell.</para>
    /// </summary>
    public TextServer.StructuredTextParser GetStructuredTextBidiOverride(int column)
    {
        TextServer.StructuredTextParser ret = (TextServer.StructuredTextParser)NativeCalls.kaizen_icall_1_85(MethodBind24, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetStructuredTextBidiOverrideOptions, 537221740ul);

    /// <summary>
    /// <para>Set additional options for BiDi override. Has effect for cells that display text.</para>
    /// </summary>
    public void SetStructuredTextBidiOverrideOptions(int column, Kaizen.Collections.Array args)
    {
        NativeCalls.kaizen_icall_2_841(MethodBind25, KaizenObject.GetPtr(this), column, (kaizen_array)(args ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStructuredTextBidiOverrideOptions, 663333327ul);

    /// <summary>
    /// <para>Returns the additional BiDi options set for this cell.</para>
    /// </summary>
    public Kaizen.Collections.Array GetStructuredTextBidiOverrideOptions(int column)
    {
        Kaizen.Collections.Array ret = NativeCalls.kaizen_icall_1_432(MethodBind26, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLanguage, 501894301ul);

    /// <summary>
    /// <para>Sets the language code of the given <paramref name="column"/>'s text to <paramref name="language"/>. This is used for line-breaking and text shaping algorithms. If <paramref name="language"/> is empty, the current locale is used.</para>
    /// </summary>
    public void SetLanguage(int column, string language)
    {
        NativeCalls.kaizen_icall_2_206(MethodBind27, KaizenObject.GetPtr(this), column, language);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLanguage, 844755477ul);

    /// <summary>
    /// <para>Returns item's text language code.</para>
    /// </summary>
    public string GetLanguage(int column)
    {
        string ret = NativeCalls.kaizen_icall_1_158(MethodBind28, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSuffix, 501894301ul);

    /// <summary>
    /// <para>Sets a string to be shown after a column's value (for example, a unit abbreviation).</para>
    /// </summary>
    public void SetSuffix(int column, string text)
    {
        NativeCalls.kaizen_icall_2_206(MethodBind29, KaizenObject.GetPtr(this), column, text);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSuffix, 844755477ul);

    /// <summary>
    /// <para>Gets the suffix string shown after the column value.</para>
    /// </summary>
    public string GetSuffix(int column)
    {
        string ret = NativeCalls.kaizen_icall_1_158(MethodBind30, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIcon, 666127730ul);

    /// <summary>
    /// <para>Sets the given cell's icon <see cref="Kaizen.Texture2D"/>. If the cell is in <see cref="Kaizen.TreeItem.TreeCellMode.Icon"/> mode, the icon is displayed in the center of the cell. Otherwise, the icon is displayed before the cell's text. <see cref="Kaizen.TreeItem.TreeCellMode.Range"/> does not display an icon.</para>
    /// </summary>
    public void SetIcon(int column, Texture2D texture)
    {
        NativeCalls.kaizen_icall_2_94(MethodBind31, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(texture));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIcon, 3536238170ul);

    /// <summary>
    /// <para>Returns the given column's icon <see cref="Kaizen.Texture2D"/>. Error if no icon is set.</para>
    /// </summary>
    public Texture2D GetIcon(int column)
    {
        Texture2D ret = (Texture2D)NativeCalls.kaizen_icall_1_95(MethodBind32, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIconOverlay, 666127730ul);

    /// <summary>
    /// <para>Sets the given cell's icon overlay <see cref="Kaizen.Texture2D"/>. The cell has to be in <see cref="Kaizen.TreeItem.TreeCellMode.Icon"/> mode, and icon has to be set. Overlay is drawn on top of icon, in the bottom left corner.</para>
    /// </summary>
    public void SetIconOverlay(int column, Texture2D texture)
    {
        NativeCalls.kaizen_icall_2_94(MethodBind33, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(texture));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind34 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIconOverlay, 3536238170ul);

    /// <summary>
    /// <para>Returns the given column's icon overlay <see cref="Kaizen.Texture2D"/>.</para>
    /// </summary>
    public Texture2D GetIconOverlay(int column)
    {
        Texture2D ret = (Texture2D)NativeCalls.kaizen_icall_1_95(MethodBind34, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind35 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIconRegion, 1356297692ul);

    /// <summary>
    /// <para>Sets the given column's icon's texture region.</para>
    /// </summary>
    public unsafe void SetIconRegion(int column, Rect2 region)
    {
        NativeCalls.kaizen_icall_2_801(MethodBind35, KaizenObject.GetPtr(this), column, &region);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind36 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIconRegion, 3327874267ul);

    /// <summary>
    /// <para>Returns the icon <see cref="Kaizen.Texture2D"/> region as <see cref="Kaizen.Rect2"/>.</para>
    /// </summary>
    public Rect2 GetIconRegion(int column)
    {
        Rect2 ret = NativeCalls.kaizen_icall_1_461(MethodBind36, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind37 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIconMaxWidth, 3937882851ul);

    /// <summary>
    /// <para>Sets the maximum allowed width of the icon in the given <paramref name="column"/>. This limit is applied on top of the default size of the icon and on top of <c>Tree.icon_max_width</c>. The height is adjusted according to the icon's ratio.</para>
    /// </summary>
    public void SetIconMaxWidth(int column, int width)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind37, KaizenObject.GetPtr(this), column, width);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind38 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIconMaxWidth, 923996154ul);

    /// <summary>
    /// <para>Returns the maximum allowed width of the icon in the given <paramref name="column"/>.</para>
    /// </summary>
    public int GetIconMaxWidth(int column)
    {
        int ret = NativeCalls.kaizen_icall_1_85(MethodBind38, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind39 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIconModulate, 2878471219ul);

    /// <summary>
    /// <para>Modulates the given column's icon with <paramref name="modulate"/>.</para>
    /// </summary>
    public unsafe void SetIconModulate(int column, Color modulate)
    {
        NativeCalls.kaizen_icall_2_717(MethodBind39, KaizenObject.GetPtr(this), column, &modulate);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind40 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIconModulate, 3457211756ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Color"/> modulating the column's icon.</para>
    /// </summary>
    public Color GetIconModulate(int column)
    {
        Color ret = NativeCalls.kaizen_icall_1_718(MethodBind40, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind41 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRange, 1602489585ul);

    /// <summary>
    /// <para>Sets the value of a <see cref="Kaizen.TreeItem.TreeCellMode.Range"/> column.</para>
    /// </summary>
    public void SetRange(int column, double value)
    {
        NativeCalls.kaizen_icall_2_108(MethodBind41, KaizenObject.GetPtr(this), column, value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind42 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRange, 2339986948ul);

    /// <summary>
    /// <para>Returns the value of a <see cref="Kaizen.TreeItem.TreeCellMode.Range"/> column.</para>
    /// </summary>
    public double GetRange(int column)
    {
        double ret = NativeCalls.kaizen_icall_1_473(MethodBind42, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind43 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRangeConfig, 1547181014ul);

    /// <summary>
    /// <para>Sets the range of accepted values for a column. The column must be in the <see cref="Kaizen.TreeItem.TreeCellMode.Range"/> mode.</para>
    /// <para>If <paramref name="expr"/> is <see langword="true"/>, the edit mode slider will use an exponential scale as with <see cref="Kaizen.Range.ExpEdit"/>.</para>
    /// </summary>
    public void SetRangeConfig(int column, double min, double max, double step, bool expr = false)
    {
        NativeCalls.kaizen_icall_5_1537(MethodBind43, KaizenObject.GetPtr(this), column, min, max, step, expr.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind44 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRangeConfig, 3554694381ul);

    /// <summary>
    /// <para>Returns a dictionary containing the range parameters for a given column. The keys are "min", "max", "step", and "expr".</para>
    /// </summary>
    public Kaizen.Collections.Dictionary GetRangeConfig(int column)
    {
        Kaizen.Collections.Dictionary ret = NativeCalls.kaizen_icall_1_335(MethodBind44, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind45 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMetadata, 2152698145ul);

    /// <summary>
    /// <para>Sets the metadata value for the given column, which can be retrieved later using <see cref="Kaizen.TreeItem.GetMetadata(int)"/>. This can be used, for example, to store a reference to the original data.</para>
    /// </summary>
    public void SetMetadata(int column, Variant meta)
    {
        NativeCalls.kaizen_icall_2_731(MethodBind45, KaizenObject.GetPtr(this), column, meta);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind46 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMetadata, 4227898402ul);

    /// <summary>
    /// <para>Returns the metadata value that was set for the given column using <see cref="Kaizen.TreeItem.SetMetadata(int, Variant)"/>.</para>
    /// </summary>
    public Variant GetMetadata(int column)
    {
        Variant ret = NativeCalls.kaizen_icall_1_732(MethodBind46, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind47 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomDraw, 272420368ul);

    /// <summary>
    /// <para>Sets the given column's custom draw callback to the <paramref name="callback"/> method on <paramref name="object"/>.</para>
    /// <para>The method named <paramref name="callback"/> should accept two arguments: the <see cref="Kaizen.TreeItem"/> that is drawn and its position and size as a <see cref="Kaizen.Rect2"/>.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.TreeItem.SetCustomDrawCallback(int, Callable)' instead.")]
    public void SetCustomDraw(int column, KaizenObject @object, StringName callback)
    {
        NativeCalls.kaizen_icall_3_1538(MethodBind47, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(@object), (kaizen_string_name)(callback?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind48 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomDrawCallback, 957362965ul);

    /// <summary>
    /// <para>Sets the given column's custom draw callback. Use an empty <see cref="Kaizen.Callable"/> (<c>Callable()</c>) to clear the custom callback. The cell has to be in <see cref="Kaizen.TreeItem.TreeCellMode.Custom"/> to use this feature.</para>
    /// <para>The <paramref name="callback"/> should accept two arguments: the <see cref="Kaizen.TreeItem"/> that is drawn and its position and size as a <see cref="Kaizen.Rect2"/>.</para>
    /// <para>To draw custom content over the native style, please use <see cref="Kaizen.Tree.GetCustomDrawingCanvasItem()"/>.</para>
    /// </summary>
    public void SetCustomDrawCallback(int column, Callable callback)
    {
        NativeCalls.kaizen_icall_2_430(MethodBind48, KaizenObject.GetPtr(this), column, callback);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind49 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomDrawCallback, 1317077508ul);

    /// <summary>
    /// <para>Returns the custom callback of column <paramref name="column"/>.</para>
    /// </summary>
    public Callable GetCustomDrawCallback(int column)
    {
        Callable ret = NativeCalls.kaizen_icall_1_1539(MethodBind49, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind50 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomStylebox, 1433009359ul);

    /// <summary>
    /// <para>Sets the given column's custom <see cref="Kaizen.StyleBox"/> used to draw the background.</para>
    /// <para><b>Note:</b> If a custom background color is set, the <see cref="Kaizen.StyleBox"/> will be drawn in front of it.</para>
    /// </summary>
    public void SetCustomStylebox(int column, StyleBox stylebox)
    {
        NativeCalls.kaizen_icall_2_94(MethodBind50, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(stylebox));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind51 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomStylebox, 3362509644ul);

    /// <summary>
    /// <para>Returns the given column's custom <see cref="Kaizen.StyleBox"/> used to draw the background.</para>
    /// </summary>
    public StyleBox GetCustomStylebox(int column)
    {
        StyleBox ret = (StyleBox)NativeCalls.kaizen_icall_1_95(MethodBind51, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind52 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCollapsed, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCollapsed(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind52, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind53 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCollapsed, 2240911060ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsCollapsed()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind53, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind54 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCollapsedRecursive, 2586408642ul);

    /// <summary>
    /// <para>Collapses or uncollapses this <see cref="Kaizen.TreeItem"/> and all the descendants of this item.</para>
    /// </summary>
    public void SetCollapsedRecursive(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind54, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind55 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAnyCollapsed, 2595650253ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this <see cref="Kaizen.TreeItem"/>, or any of its descendants, is collapsed.</para>
    /// <para>If <paramref name="onlyVisible"/> is <see langword="true"/> it ignores non-visible <see cref="Kaizen.TreeItem"/>s.</para>
    /// </summary>
    public bool IsAnyCollapsed(bool onlyVisible = false)
    {
        bool ret = NativeCalls.kaizen_icall_1_364(MethodBind55, KaizenObject.GetPtr(this), onlyVisible.ToKaizenBool()).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind56 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVisible, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVisible(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind56, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind57 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsVisible, 2240911060ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsVisible()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind57, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind58 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsVisibleInTree, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <see cref="Kaizen.TreeItem.Visible"/> is <see langword="true"/> and all its ancestors are also visible.</para>
    /// </summary>
    public bool IsVisibleInTree()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind58, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind59 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UncollapseTree, 3218959716ul);

    /// <summary>
    /// <para>Uncollapses all <see cref="Kaizen.TreeItem"/>s necessary to reveal this <see cref="Kaizen.TreeItem"/>, i.e. all ancestor <see cref="Kaizen.TreeItem"/>s.</para>
    /// </summary>
    public void UncollapseTree()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind59, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind60 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomMinimumHeight, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCustomMinimumHeight(int height)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind60, KaizenObject.GetPtr(this), height);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind61 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomMinimumHeight, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetCustomMinimumHeight()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind61, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind62 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSelectable, 300928843ul);

    /// <summary>
    /// <para>If <paramref name="selectable"/> is <see langword="true"/>, the given <paramref name="column"/> is selectable.</para>
    /// </summary>
    public void SetSelectable(int column, bool selectable)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind62, KaizenObject.GetPtr(this), column, selectable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind63 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsSelectable, 1116898809ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given <paramref name="column"/> is selectable.</para>
    /// </summary>
    public bool IsSelectable(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind63, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind64 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsSelected, 3067735520ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given <paramref name="column"/> is selected.</para>
    /// </summary>
    public bool IsSelected(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind64, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind65 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Select, 972357352ul);

    /// <summary>
    /// <para>Selects the given <paramref name="column"/>. If <paramref name="setAsCursor"/> is <see langword="true"/>, the <see cref="Kaizen.Tree"/>'s cursor will be moved to this item (only matters if <see cref="Kaizen.Tree.SelectMode"/> is set to <see cref="Kaizen.Tree.SelectModeEnum.Multi"/>).</para>
    /// </summary>
    public void Select(int column, bool setAsCursor = true)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind65, KaizenObject.GetPtr(this), column, setAsCursor.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind66 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Deselect, 1286410249ul);

    /// <summary>
    /// <para>Deselects the given column.</para>
    /// </summary>
    public void Deselect(int column)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind66, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind67 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEditable, 300928843ul);

    /// <summary>
    /// <para>If <paramref name="enabled"/> is <see langword="true"/>, the given <paramref name="column"/> is editable.</para>
    /// </summary>
    public void SetEditable(int column, bool enabled)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind67, KaizenObject.GetPtr(this), column, enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind68 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsEditable, 3067735520ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the given <paramref name="column"/> is editable.</para>
    /// </summary>
    public bool IsEditable(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind68, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind69 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomColor, 2878471219ul);

    /// <summary>
    /// <para>Sets the given column's custom color.</para>
    /// </summary>
    public unsafe void SetCustomColor(int column, Color color)
    {
        NativeCalls.kaizen_icall_2_717(MethodBind69, KaizenObject.GetPtr(this), column, &color);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind70 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomColor, 3457211756ul);

    /// <summary>
    /// <para>Returns the custom color of column <paramref name="column"/>.</para>
    /// </summary>
    public Color GetCustomColor(int column)
    {
        Color ret = NativeCalls.kaizen_icall_1_718(MethodBind70, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind71 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearCustomColor, 1286410249ul);

    /// <summary>
    /// <para>Resets the color for the given column to default.</para>
    /// </summary>
    public void ClearCustomColor(int column)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind71, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind72 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomFont, 2637609184ul);

    /// <summary>
    /// <para>Sets custom font used to draw text in the given <paramref name="column"/>.</para>
    /// </summary>
    public void SetCustomFont(int column, Font font)
    {
        NativeCalls.kaizen_icall_2_94(MethodBind72, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(font));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind73 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomFont, 4244553094ul);

    /// <summary>
    /// <para>Returns custom font used to draw text in the column <paramref name="column"/>.</para>
    /// </summary>
    public Font GetCustomFont(int column)
    {
        Font ret = (Font)NativeCalls.kaizen_icall_1_95(MethodBind73, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind74 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomFontSize, 3937882851ul);

    /// <summary>
    /// <para>Sets custom font size used to draw text in the given <paramref name="column"/>.</para>
    /// </summary>
    public void SetCustomFontSize(int column, int fontSize)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind74, KaizenObject.GetPtr(this), column, fontSize);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind75 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomFontSize, 923996154ul);

    /// <summary>
    /// <para>Returns custom font size used to draw text in the column <paramref name="column"/>.</para>
    /// </summary>
    public int GetCustomFontSize(int column)
    {
        int ret = NativeCalls.kaizen_icall_1_85(MethodBind75, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind76 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomBgColor, 894174518ul);

    /// <summary>
    /// <para>Sets the given column's custom background color and whether to just use it as an outline.</para>
    /// <para><b>Note:</b> If a custom <see cref="Kaizen.StyleBox"/> is set, the background color will be drawn behind it.</para>
    /// </summary>
    public unsafe void SetCustomBgColor(int column, Color color, bool justOutline = false)
    {
        NativeCalls.kaizen_icall_3_1540(MethodBind76, KaizenObject.GetPtr(this), column, &color, justOutline.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind77 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearCustomBgColor, 1286410249ul);

    /// <summary>
    /// <para>Resets the background color for the given column to default.</para>
    /// </summary>
    public void ClearCustomBgColor(int column)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind77, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind78 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomBgColor, 3457211756ul);

    /// <summary>
    /// <para>Returns the custom background color of column <paramref name="column"/>.</para>
    /// </summary>
    public Color GetCustomBgColor(int column)
    {
        Color ret = NativeCalls.kaizen_icall_1_718(MethodBind78, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind79 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomAsButton, 300928843ul);

    /// <summary>
    /// <para>Makes a cell with <see cref="Kaizen.TreeItem.TreeCellMode.Custom"/> display as a non-flat button with a <see cref="Kaizen.StyleBox"/>.</para>
    /// </summary>
    public void SetCustomAsButton(int column, bool enable)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind79, KaizenObject.GetPtr(this), column, enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind80 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCustomSetAsButton, 1116898809ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the cell was made into a button with <see cref="Kaizen.TreeItem.SetCustomAsButton(int, bool)"/>.</para>
    /// </summary>
    public bool IsCustomSetAsButton(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind80, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind81 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearButtons, 3218959716ul);

    /// <summary>
    /// <para>Removes all buttons from all columns of this item.</para>
    /// </summary>
    public void ClearButtons()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind81, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind82 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddButton, 973481897ul);

    /// <summary>
    /// <para>Adds a button with <see cref="Kaizen.Texture2D"/> <paramref name="button"/> to the end of the cell at column <paramref name="column"/>. The <paramref name="id"/> is used to identify the button in the according <see cref="Kaizen.Tree.ButtonClicked"/> signal and can be different from the buttons index. If not specified, the next available index is used, which may be retrieved by calling <see cref="Kaizen.TreeItem.GetButtonCount(int)"/> immediately before this method. Optionally, the button can be <paramref name="disabled"/> and have a <paramref name="tooltipText"/>. <paramref name="description"/> is used as the button description for assistive apps.</para>
    /// </summary>
    public void AddButton(int column, Texture2D button, int id = -1, bool disabled = false, string tooltipText = "", string description = "")
    {
        NativeCalls.kaizen_icall_6_1541(MethodBind82, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(button), id, disabled.ToKaizenBool(), tooltipText, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind83 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetButtonCount, 923996154ul);

    /// <summary>
    /// <para>Returns the number of buttons in column <paramref name="column"/>.</para>
    /// </summary>
    public int GetButtonCount(int column)
    {
        int ret = NativeCalls.kaizen_icall_1_85(MethodBind83, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind84 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetButtonTooltipText, 1391810591ul);

    /// <summary>
    /// <para>Returns the tooltip text for the button at index <paramref name="buttonIndex"/> in column <paramref name="column"/>.</para>
    /// </summary>
    public string GetButtonTooltipText(int column, int buttonIndex)
    {
        string ret = NativeCalls.kaizen_icall_2_251(MethodBind84, KaizenObject.GetPtr(this), column, buttonIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind85 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetButtonId, 3175239445ul);

    /// <summary>
    /// <para>Returns the ID for the button at index <paramref name="buttonIndex"/> in column <paramref name="column"/>.</para>
    /// </summary>
    public int GetButtonId(int column, int buttonIndex)
    {
        int ret = NativeCalls.kaizen_icall_2_97(MethodBind85, KaizenObject.GetPtr(this), column, buttonIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind86 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetButtonById, 3175239445ul);

    /// <summary>
    /// <para>Returns the button index if there is a button with ID <paramref name="id"/> in column <paramref name="column"/>, otherwise returns -1.</para>
    /// </summary>
    public int GetButtonById(int column, int id)
    {
        int ret = NativeCalls.kaizen_icall_2_97(MethodBind86, KaizenObject.GetPtr(this), column, id);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind87 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetButtonColor, 2165839948ul);

    /// <summary>
    /// <para>Returns the color of the button with ID <paramref name="id"/> in column <paramref name="column"/>. If the specified button does not exist, returns <c>Color.BLACK</c>.</para>
    /// </summary>
    public Color GetButtonColor(int column, int id)
    {
        Color ret = NativeCalls.kaizen_icall_2_770(MethodBind87, KaizenObject.GetPtr(this), column, id);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind88 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetButton, 2584904275ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Texture2D"/> of the button at index <paramref name="buttonIndex"/> in column <paramref name="column"/>.</para>
    /// </summary>
    public Texture2D GetButton(int column, int buttonIndex)
    {
        Texture2D ret = (Texture2D)NativeCalls.kaizen_icall_2_125(MethodBind88, KaizenObject.GetPtr(this), column, buttonIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind89 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetButtonTooltipText, 2285447957ul);

    /// <summary>
    /// <para>Sets the tooltip text for the button at index <paramref name="buttonIndex"/> in the given <paramref name="column"/>.</para>
    /// </summary>
    public void SetButtonTooltipText(int column, int buttonIndex, string tooltip)
    {
        NativeCalls.kaizen_icall_3_1374(MethodBind89, KaizenObject.GetPtr(this), column, buttonIndex, tooltip);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind90 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetButton, 176101966ul);

    /// <summary>
    /// <para>Sets the given column's button <see cref="Kaizen.Texture2D"/> at index <paramref name="buttonIndex"/> to <paramref name="button"/>.</para>
    /// </summary>
    public void SetButton(int column, int buttonIndex, Texture2D button)
    {
        NativeCalls.kaizen_icall_3_124(MethodBind90, KaizenObject.GetPtr(this), column, buttonIndex, KaizenObject.GetPtr(button));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind91 = ClassDB_get_method_with_compatibility(NativeName, MethodName.EraseButton, 3937882851ul);

    /// <summary>
    /// <para>Removes the button at index <paramref name="buttonIndex"/> in column <paramref name="column"/>.</para>
    /// </summary>
    public void EraseButton(int column, int buttonIndex)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind91, KaizenObject.GetPtr(this), column, buttonIndex);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind92 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetButtonDescription, 2285447957ul);

    /// <summary>
    /// <para>Sets the given column's button description at index <paramref name="buttonIndex"/> for assistive apps.</para>
    /// </summary>
    public void SetButtonDescription(int column, int buttonIndex, string description)
    {
        NativeCalls.kaizen_icall_3_1374(MethodBind92, KaizenObject.GetPtr(this), column, buttonIndex, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind93 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetButtonDisabled, 1383440665ul);

    /// <summary>
    /// <para>If <see langword="true"/>, disables the button at index <paramref name="buttonIndex"/> in the given <paramref name="column"/>.</para>
    /// </summary>
    public void SetButtonDisabled(int column, int buttonIndex, bool disabled)
    {
        NativeCalls.kaizen_icall_3_222(MethodBind93, KaizenObject.GetPtr(this), column, buttonIndex, disabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind94 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetButtonColor, 3733378741ul);

    /// <summary>
    /// <para>Sets the given column's button color at index <paramref name="buttonIndex"/> to <paramref name="color"/>.</para>
    /// </summary>
    public unsafe void SetButtonColor(int column, int buttonIndex, Color color)
    {
        NativeCalls.kaizen_icall_3_772(MethodBind94, KaizenObject.GetPtr(this), column, buttonIndex, &color);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind95 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsButtonDisabled, 2522259332ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the button at index <paramref name="buttonIndex"/> for the given <paramref name="column"/> is disabled.</para>
    /// </summary>
    public bool IsButtonDisabled(int column, int buttonIndex)
    {
        bool ret = NativeCalls.kaizen_icall_2_40(MethodBind95, KaizenObject.GetPtr(this), column, buttonIndex).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind96 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTooltipText, 501894301ul);

    /// <summary>
    /// <para>Sets the given column's tooltip text.</para>
    /// </summary>
    public void SetTooltipText(int column, string tooltip)
    {
        NativeCalls.kaizen_icall_2_206(MethodBind96, KaizenObject.GetPtr(this), column, tooltip);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind97 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTooltipText, 844755477ul);

    /// <summary>
    /// <para>Returns the given column's tooltip text.</para>
    /// </summary>
    public string GetTooltipText(int column)
    {
        string ret = NativeCalls.kaizen_icall_1_158(MethodBind97, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind98 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTextAlignment, 3276431499ul);

    /// <summary>
    /// <para>Sets the given column's text alignment to <paramref name="textAlignment"/>.</para>
    /// </summary>
    public void SetTextAlignment(int column, HorizontalAlignment textAlignment)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind98, KaizenObject.GetPtr(this), column, (int)textAlignment);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind99 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextAlignment, 4171562184ul);

    /// <summary>
    /// <para>Returns the given column's text alignment.</para>
    /// </summary>
    public HorizontalAlignment GetTextAlignment(int column)
    {
        HorizontalAlignment ret = (HorizontalAlignment)NativeCalls.kaizen_icall_1_85(MethodBind99, KaizenObject.GetPtr(this), column);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind100 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetExpandRight, 300928843ul);

    /// <summary>
    /// <para>If <paramref name="enable"/> is <see langword="true"/>, the given <paramref name="column"/> is expanded to the right.</para>
    /// </summary>
    public void SetExpandRight(int column, bool enable)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind100, KaizenObject.GetPtr(this), column, enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind101 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetExpandRight, 1116898809ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <c>expand_right</c> is set.</para>
    /// </summary>
    public bool GetExpandRight(int column)
    {
        bool ret = NativeCalls.kaizen_icall_1_86(MethodBind101, KaizenObject.GetPtr(this), column).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind102 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDisableFolding, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDisableFolding(bool disable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind102, KaizenObject.GetPtr(this), disable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind103 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsFoldingDisabled, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsFoldingDisabled()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind103, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind104 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAcceptChildren, 2586408642ul);

    /// <summary>
    /// <para>Sets <see cref="Kaizen.TreeItem"/>'s ability to accept children.</para>
    /// </summary>
    public void SetAcceptChildren(bool allowed)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind104, KaizenObject.GetPtr(this), allowed.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind105 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAcceptingChildren, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this <see cref="Kaizen.TreeItem"/> is allowed to accept children.</para>
    /// </summary>
    public bool IsAcceptingChildren()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind105, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind106 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateChild, 954243986ul);

    /// <summary>
    /// <para>Creates an item and adds it as a child.</para>
    /// <para>The new item will be inserted as position <paramref name="index"/> (the default value <c>-1</c> means the last position), or it will be the last child if <paramref name="index"/> is higher than the child count.</para>
    /// </summary>
    public TreeItem CreateChild(int index = -1)
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_1_365(MethodBind106, KaizenObject.GetPtr(this), index);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind107 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddChild, 1819951137ul);

    /// <summary>
    /// <para>Adds a previously unparented <see cref="Kaizen.TreeItem"/> as a direct child of this one. The <paramref name="child"/> item must not be a part of any <see cref="Kaizen.Tree"/> or parented to any <see cref="Kaizen.TreeItem"/>. See also <see cref="Kaizen.TreeItem.RemoveChild(TreeItem)"/>.</para>
    /// </summary>
    public void AddChild(TreeItem child)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind107, KaizenObject.GetPtr(this), KaizenObject.GetPtr(child));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind108 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RemoveChild, 1819951137ul);

    /// <summary>
    /// <para>Removes the given child <see cref="Kaizen.TreeItem"/> and all its children from the <see cref="Kaizen.Tree"/>. Note that it doesn't free the item from memory, so it can be reused later (see <see cref="Kaizen.TreeItem.AddChild(TreeItem)"/>). To completely remove a <see cref="Kaizen.TreeItem"/> use <see cref="Kaizen.KaizenObject.Free()"/>.</para>
    /// <para><b>Note:</b> If you want to move a child from one <see cref="Kaizen.Tree"/> to another, then instead of removing and adding it manually you can use <see cref="Kaizen.TreeItem.MoveBefore(TreeItem)"/> or <see cref="Kaizen.TreeItem.MoveAfter(TreeItem)"/>.</para>
    /// </summary>
    public void RemoveChild(TreeItem child)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind108, KaizenObject.GetPtr(this), KaizenObject.GetPtr(child));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind109 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTree, 2243340556ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.Tree"/> that owns this TreeItem.</para>
    /// </summary>
    public Tree GetTree()
    {
        Tree ret = (Tree)NativeCalls.kaizen_icall_0_53(MethodBind109, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind110 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNext, 1514277247ul);

    /// <summary>
    /// <para>Returns the next sibling TreeItem in the tree or a <see langword="null"/> object if there is none.</para>
    /// </summary>
    public TreeItem GetNext()
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_0_53(MethodBind110, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind111 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPrev, 2768121250ul);

    /// <summary>
    /// <para>Returns the previous sibling TreeItem in the tree or a <see langword="null"/> object if there is none.</para>
    /// </summary>
    public TreeItem GetPrev()
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_0_53(MethodBind111, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind112 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetParent, 1514277247ul);

    /// <summary>
    /// <para>Returns the parent TreeItem or a <see langword="null"/> object if there is none.</para>
    /// </summary>
    public TreeItem GetParent()
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_0_53(MethodBind112, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind113 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFirstChild, 1514277247ul);

    /// <summary>
    /// <para>Returns the TreeItem's first child.</para>
    /// </summary>
    public TreeItem GetFirstChild()
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_0_53(MethodBind113, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind114 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNextInTree, 1666920593ul);

    /// <summary>
    /// <para>Returns the next TreeItem in the tree (in the context of a depth-first search) or a <see langword="null"/> object if there is none.</para>
    /// <para>If <paramref name="wrap"/> is enabled, the method will wrap around to the first element in the tree when called on the last element, otherwise it returns <see langword="null"/>.</para>
    /// </summary>
    public TreeItem GetNextInTree(bool wrap = false)
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_1_252(MethodBind114, KaizenObject.GetPtr(this), wrap.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind115 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPrevInTree, 1666920593ul);

    /// <summary>
    /// <para>Returns the previous TreeItem in the tree (in the context of a depth-first search) or a <see langword="null"/> object if there is none.</para>
    /// <para>If <paramref name="wrap"/> is enabled, the method will wrap around to the last element in the tree when called on the first visible element, otherwise it returns <see langword="null"/>.</para>
    /// </summary>
    public TreeItem GetPrevInTree(bool wrap = false)
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_1_252(MethodBind115, KaizenObject.GetPtr(this), wrap.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind116 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNextVisible, 1666920593ul);

    /// <summary>
    /// <para>Returns the next visible TreeItem in the tree (in the context of a depth-first search) or a <see langword="null"/> object if there is none.</para>
    /// <para>If <paramref name="wrap"/> is enabled, the method will wrap around to the first visible element in the tree when called on the last visible element, otherwise it returns <see langword="null"/>.</para>
    /// </summary>
    public TreeItem GetNextVisible(bool wrap = false)
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_1_252(MethodBind116, KaizenObject.GetPtr(this), wrap.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind117 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPrevVisible, 1666920593ul);

    /// <summary>
    /// <para>Returns the previous visible sibling TreeItem in the tree (in the context of a depth-first search) or a <see langword="null"/> object if there is none.</para>
    /// <para>If <paramref name="wrap"/> is enabled, the method will wrap around to the last visible element in the tree when called on the first visible element, otherwise it returns <see langword="null"/>.</para>
    /// </summary>
    public TreeItem GetPrevVisible(bool wrap = false)
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_1_252(MethodBind117, KaizenObject.GetPtr(this), wrap.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind118 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetChild, 306700752ul);

    /// <summary>
    /// <para>Returns a child item by its <paramref name="index"/> (see <see cref="Kaizen.TreeItem.GetChildCount()"/>). This method is often used for iterating all children of an item.</para>
    /// <para>Negative indices access the children from the last one.</para>
    /// </summary>
    public TreeItem GetChild(int index)
    {
        TreeItem ret = (TreeItem)NativeCalls.kaizen_icall_1_365(MethodBind118, KaizenObject.GetPtr(this), index);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind119 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetChildCount, 2455072627ul);

    /// <summary>
    /// <para>Returns the number of child items.</para>
    /// </summary>
    public int GetChildCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind119, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind120 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetChildren, 2915620761ul);

    /// <summary>
    /// <para>Returns an array of references to the item's children.</para>
    /// </summary>
    public Kaizen.Collections.Array<TreeItem> GetChildren()
    {
        Kaizen.Collections.Array<TreeItem> ret = new Kaizen.Collections.Array<TreeItem>(NativeCalls.kaizen_icall_0_146(MethodBind120, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind121 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetIndex, 2455072627ul);

    /// <summary>
    /// <para>Returns the node's order in the tree. For example, if called on the first child item the position is <c>0</c>.</para>
    /// </summary>
    public int GetIndex()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind121, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind122 = ClassDB_get_method_with_compatibility(NativeName, MethodName.MoveBefore, 1819951137ul);

    /// <summary>
    /// <para>Moves this TreeItem right before the given <paramref name="item"/>.</para>
    /// <para><b>Note:</b> You can't move to the root or move the root.</para>
    /// </summary>
    public void MoveBefore(TreeItem item)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind122, KaizenObject.GetPtr(this), KaizenObject.GetPtr(item));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind123 = ClassDB_get_method_with_compatibility(NativeName, MethodName.MoveAfter, 1819951137ul);

    /// <summary>
    /// <para>Moves this TreeItem right after the given <paramref name="item"/>.</para>
    /// <para><b>Note:</b> You can't move to the root or move the root.</para>
    /// </summary>
    public void MoveAfter(TreeItem item)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind123, KaizenObject.GetPtr(this), KaizenObject.GetPtr(item));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind124 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CallRecursive, 2866548813ul);

    /// <summary>
    /// <para>Calls the <paramref name="method"/> on the actual TreeItem and its children recursively. Pass parameters as a comma separated list.</para>
    /// </summary>
    public void CallRecursive(StringName method, params Variant[] @args)
    {
        NativeCalls.kaizen_icall_2_1542(MethodBind124, KaizenObject.GetPtr(this), (kaizen_string_name)(method?.NativeValue ?? default), @args, (kaizen_string_name)MethodName.CallRecursive.NativeValue);
        global::System.GC.KeepAlive(this);
    }

    /// <summary>
    /// <para>Calls the <paramref name="method"/> on the actual TreeItem and its children recursively. Pass parameters as a comma separated list.</para>
    /// </summary>
    public void CallRecursive(StringName method, ReadOnlySpan<Variant> @args)
    {
        NativeCalls.kaizen_icall_2_1542(MethodBind124, KaizenObject.GetPtr(this), (kaizen_string_name)(method?.NativeValue ?? default), @args, (kaizen_string_name)MethodName.CallRecursive.NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind125 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddButton, 1688223362ul);

    /// <summary>
    /// <para>Adds a button with <see cref="Kaizen.Texture2D"/> <paramref name="button"/> to the end of the cell at column <paramref name="column"/>. The <paramref name="id"/> is used to identify the button in the according <see cref="Kaizen.Tree.ButtonClicked"/> signal and can be different from the buttons index. If not specified, the next available index is used, which may be retrieved by calling <see cref="Kaizen.TreeItem.GetButtonCount(int)"/> immediately before this method. Optionally, the button can be <paramref name="disabled"/> and have a <paramref name="tooltipText"/>. <paramref name="description"/> is used as the button description for assistive apps.</para>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddButton(int column, Texture2D button, int id, bool disabled, string tooltipText)
    {
        NativeCalls.kaizen_icall_5_1543(MethodBind125, KaizenObject.GetPtr(this), column, KaizenObject.GetPtr(button), id, disabled.ToKaizenBool(), tooltipText);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind126 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Select, 1286410249ul);

    /// <summary>
    /// <para>Selects the given <paramref name="column"/>. If <paramref name="setAsCursor"/> is <see langword="true"/>, the <see cref="Kaizen.Tree"/>'s cursor will be moved to this item (only matters if <see cref="Kaizen.Tree.SelectMode"/> is set to <see cref="Kaizen.Tree.SelectModeEnum.Multi"/>).</para>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Select(int column)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind126, KaizenObject.GetPtr(this), column);
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
    public new class PropertyName : KaizenObject.PropertyName
    {
        /// <summary>
        /// Cached name for the 'collapsed' property.
        /// </summary>
        public static readonly StringName Collapsed = "collapsed";
        /// <summary>
        /// Cached name for the 'visible' property.
        /// </summary>
        public static readonly StringName Visible = "visible";
        /// <summary>
        /// Cached name for the 'disable_folding' property.
        /// </summary>
        public static readonly StringName DisableFolding = "disable_folding";
        /// <summary>
        /// Cached name for the 'custom_minimum_height' property.
        /// </summary>
        public static readonly StringName CustomMinimumHeight = "custom_minimum_height";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_cell_mode' method.
        /// </summary>
        public static readonly StringName SetCellMode = "set_cell_mode";
        /// <summary>
        /// Cached name for the 'get_cell_mode' method.
        /// </summary>
        public static readonly StringName GetCellMode = "get_cell_mode";
        /// <summary>
        /// Cached name for the 'set_auto_translate_mode' method.
        /// </summary>
        public static readonly StringName SetAutoTranslateMode = "set_auto_translate_mode";
        /// <summary>
        /// Cached name for the 'get_auto_translate_mode' method.
        /// </summary>
        public static readonly StringName GetAutoTranslateMode = "get_auto_translate_mode";
        /// <summary>
        /// Cached name for the 'set_edit_multiline' method.
        /// </summary>
        public static readonly StringName SetEditMultiline = "set_edit_multiline";
        /// <summary>
        /// Cached name for the 'is_edit_multiline' method.
        /// </summary>
        public static readonly StringName IsEditMultiline = "is_edit_multiline";
        /// <summary>
        /// Cached name for the 'set_checked' method.
        /// </summary>
        public static readonly StringName SetChecked = "set_checked";
        /// <summary>
        /// Cached name for the 'set_indeterminate' method.
        /// </summary>
        public static readonly StringName SetIndeterminate = "set_indeterminate";
        /// <summary>
        /// Cached name for the 'is_checked' method.
        /// </summary>
        public static readonly StringName IsChecked = "is_checked";
        /// <summary>
        /// Cached name for the 'is_indeterminate' method.
        /// </summary>
        public static readonly StringName IsIndeterminate = "is_indeterminate";
        /// <summary>
        /// Cached name for the 'propagate_check' method.
        /// </summary>
        public static readonly StringName PropagateCheck = "propagate_check";
        /// <summary>
        /// Cached name for the 'set_text' method.
        /// </summary>
        public static readonly StringName SetText = "set_text";
        /// <summary>
        /// Cached name for the 'get_text' method.
        /// </summary>
        public static readonly StringName GetText = "get_text";
        /// <summary>
        /// Cached name for the 'set_description' method.
        /// </summary>
        public static readonly StringName SetDescription = "set_description";
        /// <summary>
        /// Cached name for the 'get_description' method.
        /// </summary>
        public static readonly StringName GetDescription = "get_description";
        /// <summary>
        /// Cached name for the 'set_text_direction' method.
        /// </summary>
        public static readonly StringName SetTextDirection = "set_text_direction";
        /// <summary>
        /// Cached name for the 'get_text_direction' method.
        /// </summary>
        public static readonly StringName GetTextDirection = "get_text_direction";
        /// <summary>
        /// Cached name for the 'set_autowrap_mode' method.
        /// </summary>
        public static readonly StringName SetAutowrapMode = "set_autowrap_mode";
        /// <summary>
        /// Cached name for the 'get_autowrap_mode' method.
        /// </summary>
        public static readonly StringName GetAutowrapMode = "get_autowrap_mode";
        /// <summary>
        /// Cached name for the 'set_autowrap_trim_flags' method.
        /// </summary>
        public static readonly StringName SetAutowrapTrimFlags = "set_autowrap_trim_flags";
        /// <summary>
        /// Cached name for the 'get_autowrap_trim_flags' method.
        /// </summary>
        public static readonly StringName GetAutowrapTrimFlags = "get_autowrap_trim_flags";
        /// <summary>
        /// Cached name for the 'set_text_overrun_behavior' method.
        /// </summary>
        public static readonly StringName SetTextOverrunBehavior = "set_text_overrun_behavior";
        /// <summary>
        /// Cached name for the 'get_text_overrun_behavior' method.
        /// </summary>
        public static readonly StringName GetTextOverrunBehavior = "get_text_overrun_behavior";
        /// <summary>
        /// Cached name for the 'set_structured_text_bidi_override' method.
        /// </summary>
        public static readonly StringName SetStructuredTextBidiOverride = "set_structured_text_bidi_override";
        /// <summary>
        /// Cached name for the 'get_structured_text_bidi_override' method.
        /// </summary>
        public static readonly StringName GetStructuredTextBidiOverride = "get_structured_text_bidi_override";
        /// <summary>
        /// Cached name for the 'set_structured_text_bidi_override_options' method.
        /// </summary>
        public static readonly StringName SetStructuredTextBidiOverrideOptions = "set_structured_text_bidi_override_options";
        /// <summary>
        /// Cached name for the 'get_structured_text_bidi_override_options' method.
        /// </summary>
        public static readonly StringName GetStructuredTextBidiOverrideOptions = "get_structured_text_bidi_override_options";
        /// <summary>
        /// Cached name for the 'set_language' method.
        /// </summary>
        public static readonly StringName SetLanguage = "set_language";
        /// <summary>
        /// Cached name for the 'get_language' method.
        /// </summary>
        public static readonly StringName GetLanguage = "get_language";
        /// <summary>
        /// Cached name for the 'set_suffix' method.
        /// </summary>
        public static readonly StringName SetSuffix = "set_suffix";
        /// <summary>
        /// Cached name for the 'get_suffix' method.
        /// </summary>
        public static readonly StringName GetSuffix = "get_suffix";
        /// <summary>
        /// Cached name for the 'set_icon' method.
        /// </summary>
        public static readonly StringName SetIcon = "set_icon";
        /// <summary>
        /// Cached name for the 'get_icon' method.
        /// </summary>
        public static readonly StringName GetIcon = "get_icon";
        /// <summary>
        /// Cached name for the 'set_icon_overlay' method.
        /// </summary>
        public static readonly StringName SetIconOverlay = "set_icon_overlay";
        /// <summary>
        /// Cached name for the 'get_icon_overlay' method.
        /// </summary>
        public static readonly StringName GetIconOverlay = "get_icon_overlay";
        /// <summary>
        /// Cached name for the 'set_icon_region' method.
        /// </summary>
        public static readonly StringName SetIconRegion = "set_icon_region";
        /// <summary>
        /// Cached name for the 'get_icon_region' method.
        /// </summary>
        public static readonly StringName GetIconRegion = "get_icon_region";
        /// <summary>
        /// Cached name for the 'set_icon_max_width' method.
        /// </summary>
        public static readonly StringName SetIconMaxWidth = "set_icon_max_width";
        /// <summary>
        /// Cached name for the 'get_icon_max_width' method.
        /// </summary>
        public static readonly StringName GetIconMaxWidth = "get_icon_max_width";
        /// <summary>
        /// Cached name for the 'set_icon_modulate' method.
        /// </summary>
        public static readonly StringName SetIconModulate = "set_icon_modulate";
        /// <summary>
        /// Cached name for the 'get_icon_modulate' method.
        /// </summary>
        public static readonly StringName GetIconModulate = "get_icon_modulate";
        /// <summary>
        /// Cached name for the 'set_range' method.
        /// </summary>
        public static readonly StringName SetRange = "set_range";
        /// <summary>
        /// Cached name for the 'get_range' method.
        /// </summary>
        public static readonly StringName GetRange = "get_range";
        /// <summary>
        /// Cached name for the 'set_range_config' method.
        /// </summary>
        public static readonly StringName SetRangeConfig = "set_range_config";
        /// <summary>
        /// Cached name for the 'get_range_config' method.
        /// </summary>
        public static readonly StringName GetRangeConfig = "get_range_config";
        /// <summary>
        /// Cached name for the 'set_metadata' method.
        /// </summary>
        public static readonly StringName SetMetadata = "set_metadata";
        /// <summary>
        /// Cached name for the 'get_metadata' method.
        /// </summary>
        public static readonly StringName GetMetadata = "get_metadata";
        /// <summary>
        /// Cached name for the 'set_custom_draw' method.
        /// </summary>
        public static readonly StringName SetCustomDraw = "set_custom_draw";
        /// <summary>
        /// Cached name for the 'set_custom_draw_callback' method.
        /// </summary>
        public static readonly StringName SetCustomDrawCallback = "set_custom_draw_callback";
        /// <summary>
        /// Cached name for the 'get_custom_draw_callback' method.
        /// </summary>
        public static readonly StringName GetCustomDrawCallback = "get_custom_draw_callback";
        /// <summary>
        /// Cached name for the 'set_custom_stylebox' method.
        /// </summary>
        public static readonly StringName SetCustomStylebox = "set_custom_stylebox";
        /// <summary>
        /// Cached name for the 'get_custom_stylebox' method.
        /// </summary>
        public static readonly StringName GetCustomStylebox = "get_custom_stylebox";
        /// <summary>
        /// Cached name for the 'set_collapsed' method.
        /// </summary>
        public static readonly StringName SetCollapsed = "set_collapsed";
        /// <summary>
        /// Cached name for the 'is_collapsed' method.
        /// </summary>
        public static readonly StringName IsCollapsed = "is_collapsed";
        /// <summary>
        /// Cached name for the 'set_collapsed_recursive' method.
        /// </summary>
        public static readonly StringName SetCollapsedRecursive = "set_collapsed_recursive";
        /// <summary>
        /// Cached name for the 'is_any_collapsed' method.
        /// </summary>
        public static readonly StringName IsAnyCollapsed = "is_any_collapsed";
        /// <summary>
        /// Cached name for the 'set_visible' method.
        /// </summary>
        public static readonly StringName SetVisible = "set_visible";
        /// <summary>
        /// Cached name for the 'is_visible' method.
        /// </summary>
        public static readonly StringName IsVisible = "is_visible";
        /// <summary>
        /// Cached name for the 'is_visible_in_tree' method.
        /// </summary>
        public static readonly StringName IsVisibleInTree = "is_visible_in_tree";
        /// <summary>
        /// Cached name for the 'uncollapse_tree' method.
        /// </summary>
        public static readonly StringName UncollapseTree = "uncollapse_tree";
        /// <summary>
        /// Cached name for the 'set_custom_minimum_height' method.
        /// </summary>
        public static readonly StringName SetCustomMinimumHeight = "set_custom_minimum_height";
        /// <summary>
        /// Cached name for the 'get_custom_minimum_height' method.
        /// </summary>
        public static readonly StringName GetCustomMinimumHeight = "get_custom_minimum_height";
        /// <summary>
        /// Cached name for the 'set_selectable' method.
        /// </summary>
        public static readonly StringName SetSelectable = "set_selectable";
        /// <summary>
        /// Cached name for the 'is_selectable' method.
        /// </summary>
        public static readonly StringName IsSelectable = "is_selectable";
        /// <summary>
        /// Cached name for the 'is_selected' method.
        /// </summary>
        public static readonly StringName IsSelected = "is_selected";
        /// <summary>
        /// Cached name for the 'select' method.
        /// </summary>
        public static readonly StringName Select = "select";
        /// <summary>
        /// Cached name for the 'deselect' method.
        /// </summary>
        public static readonly StringName Deselect = "deselect";
        /// <summary>
        /// Cached name for the 'set_editable' method.
        /// </summary>
        public static readonly StringName SetEditable = "set_editable";
        /// <summary>
        /// Cached name for the 'is_editable' method.
        /// </summary>
        public static readonly StringName IsEditable = "is_editable";
        /// <summary>
        /// Cached name for the 'set_custom_color' method.
        /// </summary>
        public static readonly StringName SetCustomColor = "set_custom_color";
        /// <summary>
        /// Cached name for the 'get_custom_color' method.
        /// </summary>
        public static readonly StringName GetCustomColor = "get_custom_color";
        /// <summary>
        /// Cached name for the 'clear_custom_color' method.
        /// </summary>
        public static readonly StringName ClearCustomColor = "clear_custom_color";
        /// <summary>
        /// Cached name for the 'set_custom_font' method.
        /// </summary>
        public static readonly StringName SetCustomFont = "set_custom_font";
        /// <summary>
        /// Cached name for the 'get_custom_font' method.
        /// </summary>
        public static readonly StringName GetCustomFont = "get_custom_font";
        /// <summary>
        /// Cached name for the 'set_custom_font_size' method.
        /// </summary>
        public static readonly StringName SetCustomFontSize = "set_custom_font_size";
        /// <summary>
        /// Cached name for the 'get_custom_font_size' method.
        /// </summary>
        public static readonly StringName GetCustomFontSize = "get_custom_font_size";
        /// <summary>
        /// Cached name for the 'set_custom_bg_color' method.
        /// </summary>
        public static readonly StringName SetCustomBgColor = "set_custom_bg_color";
        /// <summary>
        /// Cached name for the 'clear_custom_bg_color' method.
        /// </summary>
        public static readonly StringName ClearCustomBgColor = "clear_custom_bg_color";
        /// <summary>
        /// Cached name for the 'get_custom_bg_color' method.
        /// </summary>
        public static readonly StringName GetCustomBgColor = "get_custom_bg_color";
        /// <summary>
        /// Cached name for the 'set_custom_as_button' method.
        /// </summary>
        public static readonly StringName SetCustomAsButton = "set_custom_as_button";
        /// <summary>
        /// Cached name for the 'is_custom_set_as_button' method.
        /// </summary>
        public static readonly StringName IsCustomSetAsButton = "is_custom_set_as_button";
        /// <summary>
        /// Cached name for the 'clear_buttons' method.
        /// </summary>
        public static readonly StringName ClearButtons = "clear_buttons";
        /// <summary>
        /// Cached name for the 'add_button' method.
        /// </summary>
        public static readonly StringName AddButton = "add_button";
        /// <summary>
        /// Cached name for the 'get_button_count' method.
        /// </summary>
        public static readonly StringName GetButtonCount = "get_button_count";
        /// <summary>
        /// Cached name for the 'get_button_tooltip_text' method.
        /// </summary>
        public static readonly StringName GetButtonTooltipText = "get_button_tooltip_text";
        /// <summary>
        /// Cached name for the 'get_button_id' method.
        /// </summary>
        public static readonly StringName GetButtonId = "get_button_id";
        /// <summary>
        /// Cached name for the 'get_button_by_id' method.
        /// </summary>
        public static readonly StringName GetButtonById = "get_button_by_id";
        /// <summary>
        /// Cached name for the 'get_button_color' method.
        /// </summary>
        public static readonly StringName GetButtonColor = "get_button_color";
        /// <summary>
        /// Cached name for the 'get_button' method.
        /// </summary>
        public static readonly StringName GetButton = "get_button";
        /// <summary>
        /// Cached name for the 'set_button_tooltip_text' method.
        /// </summary>
        public static readonly StringName SetButtonTooltipText = "set_button_tooltip_text";
        /// <summary>
        /// Cached name for the 'set_button' method.
        /// </summary>
        public static readonly StringName SetButton = "set_button";
        /// <summary>
        /// Cached name for the 'erase_button' method.
        /// </summary>
        public static readonly StringName EraseButton = "erase_button";
        /// <summary>
        /// Cached name for the 'set_button_description' method.
        /// </summary>
        public static readonly StringName SetButtonDescription = "set_button_description";
        /// <summary>
        /// Cached name for the 'set_button_disabled' method.
        /// </summary>
        public static readonly StringName SetButtonDisabled = "set_button_disabled";
        /// <summary>
        /// Cached name for the 'set_button_color' method.
        /// </summary>
        public static readonly StringName SetButtonColor = "set_button_color";
        /// <summary>
        /// Cached name for the 'is_button_disabled' method.
        /// </summary>
        public static readonly StringName IsButtonDisabled = "is_button_disabled";
        /// <summary>
        /// Cached name for the 'set_tooltip_text' method.
        /// </summary>
        public static readonly StringName SetTooltipText = "set_tooltip_text";
        /// <summary>
        /// Cached name for the 'get_tooltip_text' method.
        /// </summary>
        public static readonly StringName GetTooltipText = "get_tooltip_text";
        /// <summary>
        /// Cached name for the 'set_text_alignment' method.
        /// </summary>
        public static readonly StringName SetTextAlignment = "set_text_alignment";
        /// <summary>
        /// Cached name for the 'get_text_alignment' method.
        /// </summary>
        public static readonly StringName GetTextAlignment = "get_text_alignment";
        /// <summary>
        /// Cached name for the 'set_expand_right' method.
        /// </summary>
        public static readonly StringName SetExpandRight = "set_expand_right";
        /// <summary>
        /// Cached name for the 'get_expand_right' method.
        /// </summary>
        public static readonly StringName GetExpandRight = "get_expand_right";
        /// <summary>
        /// Cached name for the 'set_disable_folding' method.
        /// </summary>
        public static readonly StringName SetDisableFolding = "set_disable_folding";
        /// <summary>
        /// Cached name for the 'is_folding_disabled' method.
        /// </summary>
        public static readonly StringName IsFoldingDisabled = "is_folding_disabled";
        /// <summary>
        /// Cached name for the 'set_accept_children' method.
        /// </summary>
        public static readonly StringName SetAcceptChildren = "set_accept_children";
        /// <summary>
        /// Cached name for the 'is_accepting_children' method.
        /// </summary>
        public static readonly StringName IsAcceptingChildren = "is_accepting_children";
        /// <summary>
        /// Cached name for the 'create_child' method.
        /// </summary>
        public static readonly StringName CreateChild = "create_child";
        /// <summary>
        /// Cached name for the 'add_child' method.
        /// </summary>
        public static readonly StringName AddChild = "add_child";
        /// <summary>
        /// Cached name for the 'remove_child' method.
        /// </summary>
        public static readonly StringName RemoveChild = "remove_child";
        /// <summary>
        /// Cached name for the 'get_tree' method.
        /// </summary>
        public static readonly StringName GetTree = "get_tree";
        /// <summary>
        /// Cached name for the 'get_next' method.
        /// </summary>
        public static readonly StringName GetNext = "get_next";
        /// <summary>
        /// Cached name for the 'get_prev' method.
        /// </summary>
        public static readonly StringName GetPrev = "get_prev";
        /// <summary>
        /// Cached name for the 'get_parent' method.
        /// </summary>
        public static readonly StringName GetParent = "get_parent";
        /// <summary>
        /// Cached name for the 'get_first_child' method.
        /// </summary>
        public static readonly StringName GetFirstChild = "get_first_child";
        /// <summary>
        /// Cached name for the 'get_next_in_tree' method.
        /// </summary>
        public static readonly StringName GetNextInTree = "get_next_in_tree";
        /// <summary>
        /// Cached name for the 'get_prev_in_tree' method.
        /// </summary>
        public static readonly StringName GetPrevInTree = "get_prev_in_tree";
        /// <summary>
        /// Cached name for the 'get_next_visible' method.
        /// </summary>
        public static readonly StringName GetNextVisible = "get_next_visible";
        /// <summary>
        /// Cached name for the 'get_prev_visible' method.
        /// </summary>
        public static readonly StringName GetPrevVisible = "get_prev_visible";
        /// <summary>
        /// Cached name for the 'get_child' method.
        /// </summary>
        public static readonly StringName GetChild = "get_child";
        /// <summary>
        /// Cached name for the 'get_child_count' method.
        /// </summary>
        public static readonly StringName GetChildCount = "get_child_count";
        /// <summary>
        /// Cached name for the 'get_children' method.
        /// </summary>
        public static readonly StringName GetChildren = "get_children";
        /// <summary>
        /// Cached name for the 'get_index' method.
        /// </summary>
        public static readonly StringName GetIndex = "get_index";
        /// <summary>
        /// Cached name for the 'move_before' method.
        /// </summary>
        public static readonly StringName MoveBefore = "move_before";
        /// <summary>
        /// Cached name for the 'move_after' method.
        /// </summary>
        public static readonly StringName MoveAfter = "move_after";
        /// <summary>
        /// Cached name for the 'call_recursive' method.
        /// </summary>
        public static readonly StringName CallRecursive = "call_recursive";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
