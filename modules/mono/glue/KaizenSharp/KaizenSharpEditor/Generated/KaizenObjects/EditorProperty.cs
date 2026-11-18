namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A custom control for editing properties that can be added to the <see cref="Kaizen.EditorInspector"/>. It is added via <see cref="Kaizen.EditorInspectorPlugin"/>.</para>
/// </summary>
public partial class EditorProperty : Container
{
    /// <summary>
    /// <para>Set this property to change the label (if you want to show one).</para>
    /// </summary>
    public string Label
    {
        get
        {
            return GetLabel();
        }
        set
        {
            SetLabel(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property is read-only.</para>
    /// </summary>
    public bool ReadOnly
    {
        get
        {
            return IsReadOnly();
        }
        set
        {
            SetReadOnly(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property label is drawn.</para>
    /// </summary>
    public bool DrawLabel
    {
        get
        {
            return IsDrawLabel();
        }
        set
        {
            SetDrawLabel(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property background is drawn.</para>
    /// </summary>
    public bool DrawBackground
    {
        get
        {
            return IsDrawBackground();
        }
        set
        {
            SetDrawBackground(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property is checkable.</para>
    /// </summary>
    public bool Checkable
    {
        get
        {
            return IsCheckable();
        }
        set
        {
            SetCheckable(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property is checked.</para>
    /// </summary>
    public bool Checked
    {
        get
        {
            return IsChecked();
        }
        set
        {
            SetChecked(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property is drawn with the editor theme's warning color. This is used for editable children's properties.</para>
    /// </summary>
    public bool DrawWarning
    {
        get
        {
            return IsDrawWarning();
        }
        set
        {
            SetDrawWarning(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property can add keys for animation.</para>
    /// </summary>
    public bool Keying
    {
        get
        {
            return IsKeying();
        }
        set
        {
            SetKeying(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property can be deleted by the user.</para>
    /// </summary>
    public bool Deletable
    {
        get
        {
            return IsDeletable();
        }
        set
        {
            SetDeletable(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property is selectable.</para>
    /// </summary>
    public bool Selectable
    {
        get
        {
            return IsSelectable();
        }
        set
        {
            SetSelectable(value);
        }
    }

    /// <summary>
    /// <para>Used by the inspector, set to <see langword="true"/> when the property is using folding.</para>
    /// </summary>
    public bool UseFolding
    {
        get
        {
            return IsUsingFolding();
        }
        set
        {
            SetUseFolding(value);
        }
    }

    /// <summary>
    /// <para>Space distribution ratio between the label and the editing field.</para>
    /// </summary>
    public float NameSplitRatio
    {
        get
        {
            return GetNameSplitRatio();
        }
        set
        {
            SetNameSplitRatio(value);
        }
    }

    private static readonly System.Type CachedType = typeof(EditorProperty);

    private static readonly StringName NativeName = "EditorProperty";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public EditorProperty() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal EditorProperty(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal EditorProperty(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Called when the read-only status of the property is changed. It may be used to change custom controls into a read-only or modifiable state.</para>
    /// </summary>
    public virtual void _SetReadOnly(bool readOnly)
    {
    }

    /// <summary>
    /// <para>When this virtual function is called, you must update your editor.</para>
    /// </summary>
    public virtual void _UpdateProperty()
    {
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLabel, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetLabel(string text)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind0, KaizenObject.GetPtr(this), text);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLabel, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetLabel()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetReadOnly, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetReadOnly(bool readOnly)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind2, KaizenObject.GetPtr(this), readOnly.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsReadOnly, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsReadOnly()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind3, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDrawLabel, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDrawLabel(bool drawLabel)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind4, KaizenObject.GetPtr(this), drawLabel.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDrawLabel, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDrawLabel()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind5, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDrawBackground, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDrawBackground(bool drawBackground)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind6, KaizenObject.GetPtr(this), drawBackground.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDrawBackground, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDrawBackground()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind7, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCheckable, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCheckable(bool checkable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind8, KaizenObject.GetPtr(this), checkable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCheckable, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsCheckable()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind9, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetChecked, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetChecked(bool @checked)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind10, KaizenObject.GetPtr(this), @checked.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsChecked, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsChecked()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind11, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDrawWarning, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDrawWarning(bool drawWarning)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind12, KaizenObject.GetPtr(this), drawWarning.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDrawWarning, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDrawWarning()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind13, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetKeying, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetKeying(bool keying)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind14, KaizenObject.GetPtr(this), keying.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsKeying, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsKeying()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind15, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDeletable, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDeletable(bool deletable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind16, KaizenObject.GetPtr(this), deletable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDeletable, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDeletable()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind17, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetEditedProperty, 2002593661ul);

    /// <summary>
    /// <para>Returns the edited property. If your editor is for a single property (added via <see cref="Kaizen.EditorInspectorPlugin._ParseProperty(KaizenObject, Variant.Type, string, PropertyHint, string, PropertyUsageFlags, bool)"/>), then this will return the property.</para>
    /// <para><b>Note:</b> This method could return <see langword="null"/> if the editor has not yet been associated with a property. However, in <see cref="Kaizen.EditorProperty._UpdateProperty()"/> and <see cref="Kaizen.EditorProperty._SetReadOnly(bool)"/>, this value is <i>guaranteed</i> to be non-<see langword="null"/>.</para>
    /// </summary>
    public StringName GetEditedProperty()
    {
        StringName ret = NativeCalls.kaizen_icall_0_89(MethodBind18, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetEditedObject, 2050059866ul);

    /// <summary>
    /// <para>Returns the edited object.</para>
    /// <para><b>Note:</b> This method could return <see langword="null"/> if the editor has not yet been associated with a property. However, in <see cref="Kaizen.EditorProperty._UpdateProperty()"/> and <see cref="Kaizen.EditorProperty._SetReadOnly(bool)"/>, this value is <i>guaranteed</i> to be non-<see langword="null"/>.</para>
    /// </summary>
    public KaizenObject GetEditedObject()
    {
        KaizenObject ret = (KaizenObject)NativeCalls.kaizen_icall_0_53(MethodBind19, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateProperty, 3218959716ul);

    /// <summary>
    /// <para>Forces a refresh of the property display.</para>
    /// </summary>
    public void UpdateProperty()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind20, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddFocusable, 1496901182ul);

    /// <summary>
    /// <para>If any of the controls added can gain keyboard focus, add it here. This ensures that focus will be restored if the inspector is refreshed.</para>
    /// </summary>
    public void AddFocusable(Control control)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind21, KaizenObject.GetPtr(this), KaizenObject.GetPtr(control));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBottomEditor, 1496901182ul);

    /// <summary>
    /// <para>Puts the <paramref name="editor"/> control below the property label. The control must be previously added using <see cref="Kaizen.Node.AddChild(Node, bool, Node.InternalMode)"/>.</para>
    /// </summary>
    public void SetBottomEditor(Control editor)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind22, KaizenObject.GetPtr(this), KaizenObject.GetPtr(editor));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSelectable, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSelectable(bool selectable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind23, KaizenObject.GetPtr(this), selectable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsSelectable, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsSelectable()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind24, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUseFolding, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUseFolding(bool useFolding)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind25, KaizenObject.GetPtr(this), useFolding.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsUsingFolding, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsUsingFolding()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind26, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetNameSplitRatio, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetNameSplitRatio(float ratio)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind27, KaizenObject.GetPtr(this), ratio);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNameSplitRatio, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetNameSplitRatio()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind28, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Deselect, 3218959716ul);

    /// <summary>
    /// <para>Draw property as not selected. Used by the inspector.</para>
    /// </summary>
    public void Deselect()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind29, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsSelected, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if property is drawn as selected. Used by the inspector.</para>
    /// </summary>
    public bool IsSelected()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind30, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Select, 1025054187ul);

    /// <summary>
    /// <para>Draw property as selected. Used by the inspector.</para>
    /// </summary>
    public void Select(int focusable = -1)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind31, KaizenObject.GetPtr(this), focusable);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetObjectAndProperty, 4157606280ul);

    /// <summary>
    /// <para>Assigns object and property to edit.</para>
    /// </summary>
    public void SetObjectAndProperty(KaizenObject @object, StringName property)
    {
        NativeCalls.kaizen_icall_2_542(MethodBind32, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@object), (kaizen_string_name)(property?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLabelReference, 1496901182ul);

    /// <summary>
    /// <para>Used by the inspector, set to a control that will be used as a reference to calculate the size of the label.</para>
    /// </summary>
    public void SetLabelReference(Control control)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind33, KaizenObject.GetPtr(this), KaizenObject.GetPtr(control));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind34 = ClassDB_get_method_with_compatibility(NativeName, MethodName.EmitChanged, 1822500399ul);

    /// <summary>
    /// <para>If one or several properties have changed, this must be called. <paramref name="field"/> is used in case your editor can modify fields separately (as an example, Vector3.x). The <paramref name="changing"/> argument avoids the editor requesting this property to be refreshed (leave as <see langword="false"/> if unsure).</para>
    /// </summary>
    public void EmitChanged(StringName property, Variant value, StringName field = null, bool changing = false)
    {
        EditorNativeCalls.kaizen_icall_4_543(MethodBind34, KaizenObject.GetPtr(this), (kaizen_string_name)(property?.NativeValue ?? default), value, (kaizen_string_name)(field?.NativeValue ?? default), changing.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyChanged"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyChangedEventHandler(StringName property, Variant value, StringName field, bool changing);

    private static void PropertyChangedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 4);
        ((PropertyChangedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<Variant>(args[1]), VariantUtils.ConvertTo<StringName>(args[2]), VariantUtils.ConvertTo<bool>(args[3]));
        ret = default;
    }

    /// <summary>
    /// <para>Do not emit this manually, use the <see cref="Kaizen.EditorProperty.EmitChanged(StringName, Variant, StringName, bool)"/> method instead.</para>
    /// </summary>
    public unsafe event PropertyChangedEventHandler PropertyChanged
    {
        add => Connect(SignalName.PropertyChanged, Callable.CreateWithUnsafeTrampoline(value, &PropertyChangedTrampoline));
        remove => Disconnect(SignalName.PropertyChanged, Callable.CreateWithUnsafeTrampoline(value, &PropertyChangedTrampoline));
    }

    protected void EmitSignalPropertyChanged(StringName property, Variant value, StringName field, bool changing)
    {
        EmitSignal(SignalName.PropertyChanged, property, value, field, changing);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.MultiplePropertiesChanged"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void MultiplePropertiesChangedEventHandler(string[] properties, Kaizen.Collections.Array value);

    private static void MultiplePropertiesChangedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((MultiplePropertiesChangedEventHandler)delegateObj)(VariantUtils.ConvertTo<string[]>(args[0]), VariantUtils.ConvertTo<Kaizen.Collections.Array>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emit it if you want multiple properties modified at the same time. Do not use if added via <see cref="Kaizen.EditorInspectorPlugin._ParseProperty(KaizenObject, Variant.Type, string, PropertyHint, string, PropertyUsageFlags, bool)"/>.</para>
    /// </summary>
    public unsafe event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged
    {
        add => Connect(SignalName.MultiplePropertiesChanged, Callable.CreateWithUnsafeTrampoline(value, &MultiplePropertiesChangedTrampoline));
        remove => Disconnect(SignalName.MultiplePropertiesChanged, Callable.CreateWithUnsafeTrampoline(value, &MultiplePropertiesChangedTrampoline));
    }

    protected void EmitSignalMultiplePropertiesChanged(string[] properties, Kaizen.Collections.Array value)
    {
        EmitSignal(SignalName.MultiplePropertiesChanged, properties, value);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyKeyed"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyKeyedEventHandler(StringName property);

    private static void PropertyKeyedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((PropertyKeyedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emit it if you want to add this value as an animation key (check for keying being enabled first).</para>
    /// </summary>
    public unsafe event PropertyKeyedEventHandler PropertyKeyed
    {
        add => Connect(SignalName.PropertyKeyed, Callable.CreateWithUnsafeTrampoline(value, &PropertyKeyedTrampoline));
        remove => Disconnect(SignalName.PropertyKeyed, Callable.CreateWithUnsafeTrampoline(value, &PropertyKeyedTrampoline));
    }

    protected void EmitSignalPropertyKeyed(StringName property)
    {
        EmitSignal(SignalName.PropertyKeyed, property);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyDeleted"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyDeletedEventHandler(StringName property);

    private static void PropertyDeletedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((PropertyDeletedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when a property was deleted. Used internally.</para>
    /// </summary>
    public unsafe event PropertyDeletedEventHandler PropertyDeleted
    {
        add => Connect(SignalName.PropertyDeleted, Callable.CreateWithUnsafeTrampoline(value, &PropertyDeletedTrampoline));
        remove => Disconnect(SignalName.PropertyDeleted, Callable.CreateWithUnsafeTrampoline(value, &PropertyDeletedTrampoline));
    }

    protected void EmitSignalPropertyDeleted(StringName property)
    {
        EmitSignal(SignalName.PropertyDeleted, property);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyKeyedWithValue"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyKeyedWithValueEventHandler(StringName property, Variant value);

    private static void PropertyKeyedWithValueTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((PropertyKeyedWithValueEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<Variant>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emit it if you want to key a property with a single value.</para>
    /// </summary>
    public unsafe event PropertyKeyedWithValueEventHandler PropertyKeyedWithValue
    {
        add => Connect(SignalName.PropertyKeyedWithValue, Callable.CreateWithUnsafeTrampoline(value, &PropertyKeyedWithValueTrampoline));
        remove => Disconnect(SignalName.PropertyKeyedWithValue, Callable.CreateWithUnsafeTrampoline(value, &PropertyKeyedWithValueTrampoline));
    }

    protected void EmitSignalPropertyKeyedWithValue(StringName property, Variant value)
    {
        EmitSignal(SignalName.PropertyKeyedWithValue, property, value);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyChecked"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyCheckedEventHandler(StringName property, bool @checked);

    private static void PropertyCheckedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((PropertyCheckedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<bool>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when a property was checked. Used internally.</para>
    /// </summary>
    public unsafe event PropertyCheckedEventHandler PropertyChecked
    {
        add => Connect(SignalName.PropertyChecked, Callable.CreateWithUnsafeTrampoline(value, &PropertyCheckedTrampoline));
        remove => Disconnect(SignalName.PropertyChecked, Callable.CreateWithUnsafeTrampoline(value, &PropertyCheckedTrampoline));
    }

    protected void EmitSignalPropertyChecked(StringName property, bool @checked)
    {
        EmitSignal(SignalName.PropertyChecked, property, @checked);
    }

    /// <summary>
    /// <para>Emitted when a setting override for the current project is requested.</para>
    /// </summary>
    public event Action PropertyOverridden
    {
        add => Connect(SignalName.PropertyOverridden, Callable.From(value));
        remove => Disconnect(SignalName.PropertyOverridden, Callable.From(value));
    }

    protected void EmitSignalPropertyOverridden()
    {
        EmitSignal(SignalName.PropertyOverridden);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyFavorited"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyFavoritedEventHandler(StringName property, bool favorited);

    private static void PropertyFavoritedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((PropertyFavoritedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<bool>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emit it if you want to mark a property as favorited, making it appear at the top of the inspector.</para>
    /// </summary>
    public unsafe event PropertyFavoritedEventHandler PropertyFavorited
    {
        add => Connect(SignalName.PropertyFavorited, Callable.CreateWithUnsafeTrampoline(value, &PropertyFavoritedTrampoline));
        remove => Disconnect(SignalName.PropertyFavorited, Callable.CreateWithUnsafeTrampoline(value, &PropertyFavoritedTrampoline));
    }

    protected void EmitSignalPropertyFavorited(StringName property, bool favorited)
    {
        EmitSignal(SignalName.PropertyFavorited, property, favorited);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyPinned"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyPinnedEventHandler(StringName property, bool pinned);

    private static void PropertyPinnedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((PropertyPinnedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<bool>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emit it if you want to mark (or unmark) the value of a property for being saved regardless of being equal to the default value.</para>
    /// <para>The default value is the one the property will get when the node is just instantiated and can come from an ancestor scene in the inheritance/instantiation chain, a script or a builtin class.</para>
    /// </summary>
    public unsafe event PropertyPinnedEventHandler PropertyPinned
    {
        add => Connect(SignalName.PropertyPinned, Callable.CreateWithUnsafeTrampoline(value, &PropertyPinnedTrampoline));
        remove => Disconnect(SignalName.PropertyPinned, Callable.CreateWithUnsafeTrampoline(value, &PropertyPinnedTrampoline));
    }

    protected void EmitSignalPropertyPinned(StringName property, bool pinned)
    {
        EmitSignal(SignalName.PropertyPinned, property, pinned);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.PropertyCanRevertChanged"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void PropertyCanRevertChangedEventHandler(StringName property, bool canRevert);

    private static void PropertyCanRevertChangedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((PropertyCanRevertChangedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<bool>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when the revertability (i.e., whether it has a non-default value and thus is displayed with a revert icon) of a property has changed.</para>
    /// </summary>
    public unsafe event PropertyCanRevertChangedEventHandler PropertyCanRevertChanged
    {
        add => Connect(SignalName.PropertyCanRevertChanged, Callable.CreateWithUnsafeTrampoline(value, &PropertyCanRevertChangedTrampoline));
        remove => Disconnect(SignalName.PropertyCanRevertChanged, Callable.CreateWithUnsafeTrampoline(value, &PropertyCanRevertChangedTrampoline));
    }

    protected void EmitSignalPropertyCanRevertChanged(StringName property, bool canRevert)
    {
        EmitSignal(SignalName.PropertyCanRevertChanged, property, canRevert);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.ResourceSelected"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void ResourceSelectedEventHandler(string path, Resource resource);

    private static void ResourceSelectedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((ResourceSelectedEventHandler)delegateObj)(VariantUtils.ConvertTo<string>(args[0]), VariantUtils.ConvertTo<Resource>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>If you want a sub-resource to be edited, emit this signal with the resource.</para>
    /// </summary>
    public unsafe event ResourceSelectedEventHandler ResourceSelected
    {
        add => Connect(SignalName.ResourceSelected, Callable.CreateWithUnsafeTrampoline(value, &ResourceSelectedTrampoline));
        remove => Disconnect(SignalName.ResourceSelected, Callable.CreateWithUnsafeTrampoline(value, &ResourceSelectedTrampoline));
    }

    protected void EmitSignalResourceSelected(string path, Resource resource)
    {
        EmitSignal(SignalName.ResourceSelected, path, resource);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.ObjectIdSelected"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void ObjectIdSelectedEventHandler(StringName property, long id);

    private static void ObjectIdSelectedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((ObjectIdSelectedEventHandler)delegateObj)(VariantUtils.ConvertTo<StringName>(args[0]), VariantUtils.ConvertTo<long>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Used by sub-inspectors. Emit it if what was selected was an Object ID.</para>
    /// </summary>
    public unsafe event ObjectIdSelectedEventHandler ObjectIdSelected
    {
        add => Connect(SignalName.ObjectIdSelected, Callable.CreateWithUnsafeTrampoline(value, &ObjectIdSelectedTrampoline));
        remove => Disconnect(SignalName.ObjectIdSelected, Callable.CreateWithUnsafeTrampoline(value, &ObjectIdSelectedTrampoline));
    }

    protected void EmitSignalObjectIdSelected(StringName property, long id)
    {
        EmitSignal(SignalName.ObjectIdSelected, property, id);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.EditorProperty.Selected"/> event of a <see cref="Kaizen.EditorProperty"/> class.
    /// </summary>
    public delegate void SelectedEventHandler(string path, long focusableIdx);

    private static void SelectedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 2);
        ((SelectedEventHandler)delegateObj)(VariantUtils.ConvertTo<string>(args[0]), VariantUtils.ConvertTo<long>(args[1]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when selected. Used internally.</para>
    /// </summary>
    public unsafe event SelectedEventHandler Selected
    {
        add => Connect(SignalName.Selected, Callable.CreateWithUnsafeTrampoline(value, &SelectedTrampoline));
        remove => Disconnect(SignalName.Selected, Callable.CreateWithUnsafeTrampoline(value, &SelectedTrampoline));
    }

    protected void EmitSignalSelected(string path, long focusableIdx)
    {
        EmitSignal(SignalName.Selected, path, focusableIdx);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__set_read_only = "_SetReadOnly";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__update_property = "_UpdateProperty";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_changed = "PropertyChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_multiple_properties_changed = "MultiplePropertiesChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_keyed = "PropertyKeyed";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_deleted = "PropertyDeleted";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_keyed_with_value = "PropertyKeyedWithValue";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_checked = "PropertyChecked";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_overridden = "PropertyOverridden";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_favorited = "PropertyFavorited";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_pinned = "PropertyPinned";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_property_can_revert_changed = "PropertyCanRevertChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_resource_selected = "ResourceSelected";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_object_id_selected = "ObjectIdSelected";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_selected = "Selected";

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
        if ((method == MethodProxyName__set_read_only || method == MethodName._SetReadOnly) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__set_read_only.NativeValue))
        {
            _SetReadOnly(VariantUtils.ConvertTo<bool>(args[0]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__update_property || method == MethodName._UpdateProperty) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__update_property.NativeValue))
        {
            _UpdateProperty();
            ret = default;
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
        if (method == MethodName._SetReadOnly)
        {
            if (HasKaizenClassMethod(MethodProxyName__set_read_only.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._UpdateProperty)
        {
            if (HasKaizenClassMethod(MethodProxyName__update_property.NativeValue.DangerousSelfRef))
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
        if (signal == SignalName.PropertyChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.MultiplePropertiesChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_multiple_properties_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyKeyed)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_keyed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyDeleted)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_deleted.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyKeyedWithValue)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_keyed_with_value.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyChecked)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_checked.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyOverridden)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_overridden.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyFavorited)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_favorited.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyPinned)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_pinned.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PropertyCanRevertChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_property_can_revert_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.ResourceSelected)
        {
            if (HasKaizenClassSignal(SignalProxyName_resource_selected.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.ObjectIdSelected)
        {
            if (HasKaizenClassSignal(SignalProxyName_object_id_selected.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.Selected)
        {
            if (HasKaizenClassSignal(SignalProxyName_selected.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : Container.PropertyName
    {
        /// <summary>
        /// Cached name for the 'label' property.
        /// </summary>
        public static readonly StringName Label = "label";
        /// <summary>
        /// Cached name for the 'read_only' property.
        /// </summary>
        public static readonly StringName ReadOnly = "read_only";
        /// <summary>
        /// Cached name for the 'draw_label' property.
        /// </summary>
        public static readonly StringName DrawLabel = "draw_label";
        /// <summary>
        /// Cached name for the 'draw_background' property.
        /// </summary>
        public static readonly StringName DrawBackground = "draw_background";
        /// <summary>
        /// Cached name for the 'checkable' property.
        /// </summary>
        public static readonly StringName Checkable = "checkable";
        /// <summary>
        /// Cached name for the 'checked' property.
        /// </summary>
        public static readonly StringName Checked = "checked";
        /// <summary>
        /// Cached name for the 'draw_warning' property.
        /// </summary>
        public static readonly StringName DrawWarning = "draw_warning";
        /// <summary>
        /// Cached name for the 'keying' property.
        /// </summary>
        public static readonly StringName Keying = "keying";
        /// <summary>
        /// Cached name for the 'deletable' property.
        /// </summary>
        public static readonly StringName Deletable = "deletable";
        /// <summary>
        /// Cached name for the 'selectable' property.
        /// </summary>
        public static readonly StringName Selectable = "selectable";
        /// <summary>
        /// Cached name for the 'use_folding' property.
        /// </summary>
        public static readonly StringName UseFolding = "use_folding";
        /// <summary>
        /// Cached name for the 'name_split_ratio' property.
        /// </summary>
        public static readonly StringName NameSplitRatio = "name_split_ratio";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Container.MethodName
    {
        /// <summary>
        /// Cached name for the '_set_read_only' method.
        /// </summary>
        public static readonly StringName _SetReadOnly = "_set_read_only";
        /// <summary>
        /// Cached name for the '_update_property' method.
        /// </summary>
        public static readonly StringName _UpdateProperty = "_update_property";
        /// <summary>
        /// Cached name for the 'set_label' method.
        /// </summary>
        public static readonly StringName SetLabel = "set_label";
        /// <summary>
        /// Cached name for the 'get_label' method.
        /// </summary>
        public static readonly StringName GetLabel = "get_label";
        /// <summary>
        /// Cached name for the 'set_read_only' method.
        /// </summary>
        public static readonly StringName SetReadOnly = "set_read_only";
        /// <summary>
        /// Cached name for the 'is_read_only' method.
        /// </summary>
        public static readonly StringName IsReadOnly = "is_read_only";
        /// <summary>
        /// Cached name for the 'set_draw_label' method.
        /// </summary>
        public static readonly StringName SetDrawLabel = "set_draw_label";
        /// <summary>
        /// Cached name for the 'is_draw_label' method.
        /// </summary>
        public static readonly StringName IsDrawLabel = "is_draw_label";
        /// <summary>
        /// Cached name for the 'set_draw_background' method.
        /// </summary>
        public static readonly StringName SetDrawBackground = "set_draw_background";
        /// <summary>
        /// Cached name for the 'is_draw_background' method.
        /// </summary>
        public static readonly StringName IsDrawBackground = "is_draw_background";
        /// <summary>
        /// Cached name for the 'set_checkable' method.
        /// </summary>
        public static readonly StringName SetCheckable = "set_checkable";
        /// <summary>
        /// Cached name for the 'is_checkable' method.
        /// </summary>
        public static readonly StringName IsCheckable = "is_checkable";
        /// <summary>
        /// Cached name for the 'set_checked' method.
        /// </summary>
        public static readonly StringName SetChecked = "set_checked";
        /// <summary>
        /// Cached name for the 'is_checked' method.
        /// </summary>
        public static readonly StringName IsChecked = "is_checked";
        /// <summary>
        /// Cached name for the 'set_draw_warning' method.
        /// </summary>
        public static readonly StringName SetDrawWarning = "set_draw_warning";
        /// <summary>
        /// Cached name for the 'is_draw_warning' method.
        /// </summary>
        public static readonly StringName IsDrawWarning = "is_draw_warning";
        /// <summary>
        /// Cached name for the 'set_keying' method.
        /// </summary>
        public static readonly StringName SetKeying = "set_keying";
        /// <summary>
        /// Cached name for the 'is_keying' method.
        /// </summary>
        public static readonly StringName IsKeying = "is_keying";
        /// <summary>
        /// Cached name for the 'set_deletable' method.
        /// </summary>
        public static readonly StringName SetDeletable = "set_deletable";
        /// <summary>
        /// Cached name for the 'is_deletable' method.
        /// </summary>
        public static readonly StringName IsDeletable = "is_deletable";
        /// <summary>
        /// Cached name for the 'get_edited_property' method.
        /// </summary>
        public static readonly StringName GetEditedProperty = "get_edited_property";
        /// <summary>
        /// Cached name for the 'get_edited_object' method.
        /// </summary>
        public static readonly StringName GetEditedObject = "get_edited_object";
        /// <summary>
        /// Cached name for the 'update_property' method.
        /// </summary>
        public static readonly StringName UpdateProperty = "update_property";
        /// <summary>
        /// Cached name for the 'add_focusable' method.
        /// </summary>
        public static readonly StringName AddFocusable = "add_focusable";
        /// <summary>
        /// Cached name for the 'set_bottom_editor' method.
        /// </summary>
        public static readonly StringName SetBottomEditor = "set_bottom_editor";
        /// <summary>
        /// Cached name for the 'set_selectable' method.
        /// </summary>
        public static readonly StringName SetSelectable = "set_selectable";
        /// <summary>
        /// Cached name for the 'is_selectable' method.
        /// </summary>
        public static readonly StringName IsSelectable = "is_selectable";
        /// <summary>
        /// Cached name for the 'set_use_folding' method.
        /// </summary>
        public static readonly StringName SetUseFolding = "set_use_folding";
        /// <summary>
        /// Cached name for the 'is_using_folding' method.
        /// </summary>
        public static readonly StringName IsUsingFolding = "is_using_folding";
        /// <summary>
        /// Cached name for the 'set_name_split_ratio' method.
        /// </summary>
        public static readonly StringName SetNameSplitRatio = "set_name_split_ratio";
        /// <summary>
        /// Cached name for the 'get_name_split_ratio' method.
        /// </summary>
        public static readonly StringName GetNameSplitRatio = "get_name_split_ratio";
        /// <summary>
        /// Cached name for the 'deselect' method.
        /// </summary>
        public static readonly StringName Deselect = "deselect";
        /// <summary>
        /// Cached name for the 'is_selected' method.
        /// </summary>
        public static readonly StringName IsSelected = "is_selected";
        /// <summary>
        /// Cached name for the 'select' method.
        /// </summary>
        public static readonly StringName Select = "select";
        /// <summary>
        /// Cached name for the 'set_object_and_property' method.
        /// </summary>
        public static readonly StringName SetObjectAndProperty = "set_object_and_property";
        /// <summary>
        /// Cached name for the 'set_label_reference' method.
        /// </summary>
        public static readonly StringName SetLabelReference = "set_label_reference";
        /// <summary>
        /// Cached name for the 'emit_changed' method.
        /// </summary>
        public static readonly StringName EmitChanged = "emit_changed";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Container.SignalName
    {
        /// <summary>
        /// Cached name for the 'property_changed' signal.
        /// </summary>
        public static readonly StringName PropertyChanged = "property_changed";
        /// <summary>
        /// Cached name for the 'multiple_properties_changed' signal.
        /// </summary>
        public static readonly StringName MultiplePropertiesChanged = "multiple_properties_changed";
        /// <summary>
        /// Cached name for the 'property_keyed' signal.
        /// </summary>
        public static readonly StringName PropertyKeyed = "property_keyed";
        /// <summary>
        /// Cached name for the 'property_deleted' signal.
        /// </summary>
        public static readonly StringName PropertyDeleted = "property_deleted";
        /// <summary>
        /// Cached name for the 'property_keyed_with_value' signal.
        /// </summary>
        public static readonly StringName PropertyKeyedWithValue = "property_keyed_with_value";
        /// <summary>
        /// Cached name for the 'property_checked' signal.
        /// </summary>
        public static readonly StringName PropertyChecked = "property_checked";
        /// <summary>
        /// Cached name for the 'property_overridden' signal.
        /// </summary>
        public static readonly StringName PropertyOverridden = "property_overridden";
        /// <summary>
        /// Cached name for the 'property_favorited' signal.
        /// </summary>
        public static readonly StringName PropertyFavorited = "property_favorited";
        /// <summary>
        /// Cached name for the 'property_pinned' signal.
        /// </summary>
        public static readonly StringName PropertyPinned = "property_pinned";
        /// <summary>
        /// Cached name for the 'property_can_revert_changed' signal.
        /// </summary>
        public static readonly StringName PropertyCanRevertChanged = "property_can_revert_changed";
        /// <summary>
        /// Cached name for the 'resource_selected' signal.
        /// </summary>
        public static readonly StringName ResourceSelected = "resource_selected";
        /// <summary>
        /// Cached name for the 'object_id_selected' signal.
        /// </summary>
        public static readonly StringName ObjectIdSelected = "object_id_selected";
        /// <summary>
        /// Cached name for the 'selected' signal.
        /// </summary>
        public static readonly StringName Selected = "selected";
    }
}
