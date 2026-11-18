namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.SpinBox"/> is a numerical input text field. It allows entering integers and floating-point numbers. The <see cref="Kaizen.SpinBox"/> also has up and down buttons that can be clicked to increase or decrease the value. The value can also be changed by dragging the mouse up or down over the <see cref="Kaizen.SpinBox"/>'s arrows.</para>
/// <para>Additionally, mathematical expressions can be entered. These are evaluated when the user presses Enter while editing the <see cref="Kaizen.SpinBox"/>'s text field. This uses the <see cref="Kaizen.Expression"/> class to parse and evaluate the expression. The result of the expression is then set as the value of the <see cref="Kaizen.SpinBox"/>. Some examples of valid expressions are <c>5 + 2 * 3</c>, <c>pow(2, 4)</c>, and <c>PI + sin(0.5)</c>. Expressions are case-sensitive.</para>
/// <para><b>Example:</b> Create a <see cref="Kaizen.SpinBox"/>, disable its context menu and set its text alignment to right.</para>
/// <para><code>
/// var spinBox = new SpinBox();
/// AddChild(spinBox);
/// var lineEdit = spinBox.GetLineEdit();
/// lineEdit.ContextMenuEnabled = false;
/// spinBox.AlignHorizontal = LineEdit.HorizontalAlignEnum.Right;
/// </code></para>
/// <para>See <see cref="Kaizen.Range"/> class for more options over the <see cref="Kaizen.SpinBox"/>.</para>
/// <para><b>Note:</b> With the <see cref="Kaizen.SpinBox"/>'s context menu disabled, you can right-click the bottom half of the spinbox to set the value to its minimum, while right-clicking the top half sets the value to its maximum.</para>
/// <para><b>Note:</b> <see cref="Kaizen.SpinBox"/> relies on an underlying <see cref="Kaizen.LineEdit"/> node. To theme a <see cref="Kaizen.SpinBox"/>'s background and audio feedback on its text field, add theme items for <see cref="Kaizen.LineEdit"/> and customize them. The <see cref="Kaizen.LineEdit"/> has the <c>SpinBoxInnerLineEdit</c> theme variation, so that you can give it a distinct appearance from regular <see cref="Kaizen.LineEdit"/>s.</para>
/// <para><b>Note:</b> If you want to implement drag and drop for the underlying <see cref="Kaizen.LineEdit"/>, you can use <see cref="Kaizen.Control.SetDragForwarding(Callable, Callable, Callable)"/> on the node returned by <see cref="Kaizen.SpinBox.GetLineEdit()"/>.</para>
/// </summary>
public partial class SpinBox : Range
{
    /// <summary>
    /// <para>Changes the alignment of the underlying <see cref="Kaizen.LineEdit"/>.</para>
    /// </summary>
    public HorizontalAlignment Alignment
    {
        get
        {
            return GetHorizontalAlignment();
        }
        set
        {
            SetHorizontalAlignment(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the <see cref="Kaizen.SpinBox"/> will be editable. Otherwise, it will be read only.</para>
    /// </summary>
    public bool Editable
    {
        get
        {
            return IsEditable();
        }
        set
        {
            SetEditable(value);
        }
    }

    /// <summary>
    /// <para>Sets the value of the <see cref="Kaizen.Range"/> for this <see cref="Kaizen.SpinBox"/> when the <see cref="Kaizen.LineEdit"/> text is <i>changed</i> instead of <i>submitted</i>. See <see cref="Kaizen.LineEdit.TextChanged"/> and <see cref="Kaizen.LineEdit.TextSubmitted"/>.</para>
    /// <para><b>Note:</b> If set to <see langword="true"/>, this will interfere with entering mathematical expressions in the <see cref="Kaizen.SpinBox"/>. The <see cref="Kaizen.SpinBox"/> will try to evaluate the expression as you type, which means symbols like a trailing <c>+</c> are removed immediately by the expression being evaluated.</para>
    /// </summary>
    public bool UpdateOnTextChanged
    {
        get
        {
            return GetUpdateOnTextChanged();
        }
        set
        {
            SetUpdateOnTextChanged(value);
        }
    }

    /// <summary>
    /// <para>If not <c>0</c>, sets the step when interacting with the arrow buttons of the <see cref="Kaizen.SpinBox"/>.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.Range.Value"/> will still be rounded to a multiple of <see cref="Kaizen.Range.Step"/>.</para>
    /// </summary>
    public double CustomArrowStep
    {
        get
        {
            return GetCustomArrowStep();
        }
        set
        {
            SetCustomArrowStep(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the value will be rounded to a multiple of <see cref="Kaizen.SpinBox.CustomArrowStep"/> when interacting with the arrow buttons. Otherwise, increments the value by <see cref="Kaizen.SpinBox.CustomArrowStep"/> and then rounds it according to <see cref="Kaizen.Range.Step"/>.</para>
    /// </summary>
    public bool CustomArrowRound
    {
        get
        {
            return IsCustomArrowRounding();
        }
        set
        {
            SetCustomArrowRound(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the <see cref="Kaizen.SpinBox"/> will select the whole text when the <see cref="Kaizen.LineEdit"/> gains focus. Clicking the up and down arrows won't trigger this behavior.</para>
    /// </summary>
    public bool SelectAllOnFocus
    {
        get
        {
            return IsSelectAllOnFocus();
        }
        set
        {
            SetSelectAllOnFocus(value);
        }
    }

    /// <summary>
    /// <para>The formatting of the displayed numeric value, using the same rules as <see cref="string"/>'s <c>%</c> operator (see <a href="$DOCS_URL/tutorials/scripting/gdscript/gdscript_format_string.html">GDScript format strings</a>). If empty, the value is displayed with no special formatting. If the format string is invalid, the <see cref="Kaizen.SpinBox"/> will display <c>"&lt;Invalid&gt;"</c>, with the error message available as a configuration warning.</para>
    /// <para>The following format strings can be used as examples:</para>
    /// <para>- <c>"(%s)"</c> displays as a number in parentheses (e.g. <c>(6.9)</c>).</para>
    /// <para>- <c>"%d ms"</c> displays as an integer with milliseconds unit (e.g. <c>420 ms</c>).</para>
    /// <para>- <c>"%0.2f%%"</c> displays as a percentage, with 2 decimal places of precision (e.g. <c>21.37%</c>).</para>
    /// </summary>
    public string Format
    {
        get
        {
            return GetFormat();
        }
        set
        {
            SetFormat(value);
        }
    }

    /// <summary>
    /// <para>Plural version of <see cref="Kaizen.SpinBox.Format"/>. Used only if <see cref="Kaizen.SpinBox.Format"/> is not empty and <see cref="Kaizen.SpinBox.FormatAutoTranslateMode"/> is not disabled. If either <see cref="Kaizen.SpinBox.Format"/> or this contain <c>%s</c> the other must also use it. See also <see cref="Kaizen.KaizenObject.TrN(StringName, StringName, int, StringName)"/>.</para>
    /// </summary>
    public string PluralFormat
    {
        get
        {
            return GetPluralFormat();
        }
        set
        {
            SetPluralFormat(value);
        }
    }

    /// <summary>
    /// <para>Auto-translation mode for the <see cref="Kaizen.SpinBox.Format"/>. Useful when the format has a word that can be translated.</para>
    /// </summary>
    public Node.AutoTranslateModeEnum FormatAutoTranslateMode
    {
        get
        {
            return GetFormatAutoTranslateMode();
        }
        set
        {
            SetFormatAutoTranslateMode(value);
        }
    }

    /// <summary>
    /// <para>Adds the specified prefix string before the numerical value of the <see cref="Kaizen.SpinBox"/>. This is used only if <see cref="Kaizen.SpinBox.Format"/> is empty.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.SpinBox.Format' instead.")]
    public string Prefix
    {
        get
        {
            return GetPrefix();
        }
        set
        {
            SetPrefix(value);
        }
    }

    /// <summary>
    /// <para>Adds the specified suffix string after the numerical value of the <see cref="Kaizen.SpinBox"/>. This is used only if <see cref="Kaizen.SpinBox.Format"/> is empty.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.SpinBox.Format' instead.")]
    public string Suffix
    {
        get
        {
            return GetSuffix();
        }
        set
        {
            SetSuffix(value);
        }
    }

    private static readonly System.Type CachedType = typeof(SpinBox);

    private static readonly StringName NativeName = "SpinBox";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public SpinBox() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal SpinBox(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal SpinBox(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetHorizontalAlignment, 2312603777ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetHorizontalAlignment(HorizontalAlignment alignment)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), (int)alignment);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHorizontalAlignment, 341400642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public HorizontalAlignment GetHorizontalAlignment()
    {
        HorizontalAlignment ret = (HorizontalAlignment)NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFormat, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFormat(string format)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind2, KaizenObject.GetPtr(this), format);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFormat, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetFormat()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPluralFormat, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPluralFormat(string format)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind4, KaizenObject.GetPtr(this), format);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPluralFormat, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetPluralFormat()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFormatAutoTranslateMode, 776149714ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFormatAutoTranslateMode(Node.AutoTranslateModeEnum mode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind6, KaizenObject.GetPtr(this), (int)mode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFormatAutoTranslateMode, 2498906432ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Node.AutoTranslateModeEnum GetFormatAutoTranslateMode()
    {
        Node.AutoTranslateModeEnum ret = (Node.AutoTranslateModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSuffix, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSuffix(string suffix)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind8, KaizenObject.GetPtr(this), suffix);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSuffix, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetSuffix()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPrefix, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPrefix(string prefix)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind10, KaizenObject.GetPtr(this), prefix);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPrefix, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetPrefix()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEditable, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetEditable(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind12, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomArrowStep, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCustomArrowStep(double arrowStep)
    {
        NativeCalls.kaizen_icall_1_135(MethodBind13, KaizenObject.GetPtr(this), arrowStep);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCustomArrowStep, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public double GetCustomArrowStep()
    {
        double ret = NativeCalls.kaizen_icall_0_136(MethodBind14, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCustomArrowRound, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCustomArrowRound(bool round)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind15, KaizenObject.GetPtr(this), round.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCustomArrowRounding, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsCustomArrowRounding()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind16, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsEditable, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsEditable()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind17, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUpdateOnTextChanged, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUpdateOnTextChanged(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind18, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUpdateOnTextChanged, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetUpdateOnTextChanged()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind19, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSelectAllOnFocus, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSelectAllOnFocus(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind20, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsSelectAllOnFocus, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsSelectAllOnFocus()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind21, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Apply, 3218959716ul);

    /// <summary>
    /// <para>Applies the current value of this <see cref="Kaizen.SpinBox"/>. This is equivalent to pressing Enter while editing the <see cref="Kaizen.LineEdit"/> used by the <see cref="Kaizen.SpinBox"/>. This will cause <see cref="Kaizen.LineEdit.TextSubmitted"/> to be emitted and its currently contained expression to be evaluated.</para>
    /// </summary>
    public void Apply()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind22, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLineEdit, 4071694264ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.LineEdit"/> instance from this <see cref="Kaizen.SpinBox"/>. You can use it to access properties and methods of <see cref="Kaizen.LineEdit"/>.</para>
    /// <para><b>Warning:</b> This is a required internal node, removing and freeing it may cause a crash. If you wish to hide it or any of its children, use their <see cref="Kaizen.CanvasItem.Visible"/> property.</para>
    /// </summary>
    public LineEdit GetLineEdit()
    {
        LineEdit ret = (LineEdit)NativeCalls.kaizen_icall_0_53(MethodBind23, KaizenObject.GetPtr(this));
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
    public new class PropertyName : Range.PropertyName
    {
        /// <summary>
        /// Cached name for the 'alignment' property.
        /// </summary>
        public static readonly StringName Alignment = "alignment";
        /// <summary>
        /// Cached name for the 'editable' property.
        /// </summary>
        public static readonly StringName Editable = "editable";
        /// <summary>
        /// Cached name for the 'update_on_text_changed' property.
        /// </summary>
        public static readonly StringName UpdateOnTextChanged = "update_on_text_changed";
        /// <summary>
        /// Cached name for the 'custom_arrow_step' property.
        /// </summary>
        public static readonly StringName CustomArrowStep = "custom_arrow_step";
        /// <summary>
        /// Cached name for the 'custom_arrow_round' property.
        /// </summary>
        public static readonly StringName CustomArrowRound = "custom_arrow_round";
        /// <summary>
        /// Cached name for the 'select_all_on_focus' property.
        /// </summary>
        public static readonly StringName SelectAllOnFocus = "select_all_on_focus";
        /// <summary>
        /// Cached name for the 'format' property.
        /// </summary>
        public static readonly StringName Format = "format";
        /// <summary>
        /// Cached name for the 'plural_format' property.
        /// </summary>
        public static readonly StringName PluralFormat = "plural_format";
        /// <summary>
        /// Cached name for the 'format_auto_translate_mode' property.
        /// </summary>
        public static readonly StringName FormatAutoTranslateMode = "format_auto_translate_mode";
        /// <summary>
        /// Cached name for the 'prefix' property.
        /// </summary>
        public static readonly StringName Prefix = "prefix";
        /// <summary>
        /// Cached name for the 'suffix' property.
        /// </summary>
        public static readonly StringName Suffix = "suffix";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Range.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_horizontal_alignment' method.
        /// </summary>
        public static readonly StringName SetHorizontalAlignment = "set_horizontal_alignment";
        /// <summary>
        /// Cached name for the 'get_horizontal_alignment' method.
        /// </summary>
        public static readonly StringName GetHorizontalAlignment = "get_horizontal_alignment";
        /// <summary>
        /// Cached name for the 'set_format' method.
        /// </summary>
        public static readonly StringName SetFormat = "set_format";
        /// <summary>
        /// Cached name for the 'get_format' method.
        /// </summary>
        public static readonly StringName GetFormat = "get_format";
        /// <summary>
        /// Cached name for the 'set_plural_format' method.
        /// </summary>
        public static readonly StringName SetPluralFormat = "set_plural_format";
        /// <summary>
        /// Cached name for the 'get_plural_format' method.
        /// </summary>
        public static readonly StringName GetPluralFormat = "get_plural_format";
        /// <summary>
        /// Cached name for the 'set_format_auto_translate_mode' method.
        /// </summary>
        public static readonly StringName SetFormatAutoTranslateMode = "set_format_auto_translate_mode";
        /// <summary>
        /// Cached name for the 'get_format_auto_translate_mode' method.
        /// </summary>
        public static readonly StringName GetFormatAutoTranslateMode = "get_format_auto_translate_mode";
        /// <summary>
        /// Cached name for the 'set_suffix' method.
        /// </summary>
        public static readonly StringName SetSuffix = "set_suffix";
        /// <summary>
        /// Cached name for the 'get_suffix' method.
        /// </summary>
        public static readonly StringName GetSuffix = "get_suffix";
        /// <summary>
        /// Cached name for the 'set_prefix' method.
        /// </summary>
        public static readonly StringName SetPrefix = "set_prefix";
        /// <summary>
        /// Cached name for the 'get_prefix' method.
        /// </summary>
        public static readonly StringName GetPrefix = "get_prefix";
        /// <summary>
        /// Cached name for the 'set_editable' method.
        /// </summary>
        public static readonly StringName SetEditable = "set_editable";
        /// <summary>
        /// Cached name for the 'set_custom_arrow_step' method.
        /// </summary>
        public static readonly StringName SetCustomArrowStep = "set_custom_arrow_step";
        /// <summary>
        /// Cached name for the 'get_custom_arrow_step' method.
        /// </summary>
        public static readonly StringName GetCustomArrowStep = "get_custom_arrow_step";
        /// <summary>
        /// Cached name for the 'set_custom_arrow_round' method.
        /// </summary>
        public static readonly StringName SetCustomArrowRound = "set_custom_arrow_round";
        /// <summary>
        /// Cached name for the 'is_custom_arrow_rounding' method.
        /// </summary>
        public static readonly StringName IsCustomArrowRounding = "is_custom_arrow_rounding";
        /// <summary>
        /// Cached name for the 'is_editable' method.
        /// </summary>
        public static readonly StringName IsEditable = "is_editable";
        /// <summary>
        /// Cached name for the 'set_update_on_text_changed' method.
        /// </summary>
        public static readonly StringName SetUpdateOnTextChanged = "set_update_on_text_changed";
        /// <summary>
        /// Cached name for the 'get_update_on_text_changed' method.
        /// </summary>
        public static readonly StringName GetUpdateOnTextChanged = "get_update_on_text_changed";
        /// <summary>
        /// Cached name for the 'set_select_all_on_focus' method.
        /// </summary>
        public static readonly StringName SetSelectAllOnFocus = "set_select_all_on_focus";
        /// <summary>
        /// Cached name for the 'is_select_all_on_focus' method.
        /// </summary>
        public static readonly StringName IsSelectAllOnFocus = "is_select_all_on_focus";
        /// <summary>
        /// Cached name for the 'apply' method.
        /// </summary>
        public static readonly StringName Apply = "apply";
        /// <summary>
        /// Cached name for the 'get_line_edit' method.
        /// </summary>
        public static readonly StringName GetLineEdit = "get_line_edit";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Range.SignalName
    {
    }
}
