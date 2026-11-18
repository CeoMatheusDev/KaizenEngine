namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.AccessibilityServer"/> handles screen reader support.</para>
/// <para><b>Note:</b> <see cref="Kaizen.AccessibilityServer"/> is implemented on Android, Linux, macOS, and Windows.</para>
/// </summary>
[KaizenClassName("AccessibilityServer")]
public partial class AccessibilityServerInstance : KaizenObject
{
    private static readonly System.Type CachedType = typeof(AccessibilityServerInstance);

    private static readonly StringName NativeName = "AccessibilityServer";

    internal AccessibilityServerInstance() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal AccessibilityServerInstance(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal AccessibilityServerInstance(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsSupported, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if screen reader is support by this implementation.</para>
    /// </summary>
    public bool IsSupported()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind0, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateElement, 3846965249ul);

    /// <summary>
    /// <para>Creates a new, empty accessibility element resource.</para>
    /// <para><b>Note:</b> An accessibility element is created and freed automatically for each <see cref="Kaizen.Node"/>. In general, this function should not be called manually.</para>
    /// </summary>
    public Rid CreateElement(int windowId, AccessibilityServer.AccessibilityRole role)
    {
        Rid ret = NativeCalls.kaizen_icall_2_59(MethodBind1, KaizenObject.GetPtr(this), windowId, (int)role);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateSubElement, 1151690429ul);

    /// <summary>
    /// <para>Creates a new, empty accessibility sub-element resource. Sub-elements can be used to provide accessibility information for objects which are not <see cref="Kaizen.Node"/>s, such as list items, table cells, or menu items. Sub-elements are freed automatically when the parent element is freed, or can be freed early using the <see cref="Kaizen.AccessibilityServerInstance.FreeElement(Rid)"/> method.</para>
    /// </summary>
    public Rid CreateSubElement(Rid parentRid, AccessibilityServer.AccessibilityRole role, int insertPos = -1)
    {
        Rid ret = NativeCalls.kaizen_icall_3_60(MethodBind2, KaizenObject.GetPtr(this), parentRid, (int)role, insertPos);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateSubTextEditElements, 2702009895ul);

    /// <summary>
    /// <para>Creates a new, empty accessibility sub-element from the shaped text buffer. Sub-elements are freed automatically when the parent element is freed, or can be freed early using the <see cref="Kaizen.AccessibilityServerInstance.FreeElement(Rid)"/> method.</para>
    /// <para>If <paramref name="isLastLine"/> is <see langword="true"/>, no trailing newline is appended to the text content. Set to <see langword="true"/> for the last line in multi-line text fields and for single-line text fields.</para>
    /// </summary>
    public Rid CreateSubTextEditElements(Rid parentRid, Rid shapedText, float minHeight, int insertPos = -1, bool isLastLine = false)
    {
        Rid ret = NativeCalls.kaizen_icall_5_61(MethodBind3, KaizenObject.GetPtr(this), parentRid, shapedText, minHeight, insertPos, isLastLine.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasElement, 4155700596ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <paramref name="id"/> is a valid accessibility element.</para>
    /// </summary>
    public bool HasElement(Rid id)
    {
        bool ret = NativeCalls.kaizen_icall_1_62(MethodBind4, KaizenObject.GetPtr(this), id).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.FreeElement, 2722037293ul);

    /// <summary>
    /// <para>Frees the accessibility element <paramref name="id"/> created by <see cref="Kaizen.AccessibilityServerInstance.CreateElement(int, AccessibilityServer.AccessibilityRole)"/>, <see cref="Kaizen.AccessibilityServerInstance.CreateSubElement(Rid, AccessibilityServer.AccessibilityRole, int)"/>, or <see cref="Kaizen.AccessibilityServerInstance.CreateSubTextEditElements(Rid, Rid, float, int, bool)"/>.</para>
    /// </summary>
    public void FreeElement(Rid id)
    {
        NativeCalls.kaizen_icall_1_63(MethodBind5, KaizenObject.GetPtr(this), id);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ElementSetMeta, 3175752987ul);

    /// <summary>
    /// <para>Sets the metadata of the accessibility element <paramref name="id"/> to <paramref name="meta"/>.</para>
    /// </summary>
    public void ElementSetMeta(Rid id, Variant meta)
    {
        NativeCalls.kaizen_icall_2_64(MethodBind6, KaizenObject.GetPtr(this), id, meta);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ElementGetMeta, 4171304767ul);

    /// <summary>
    /// <para>Returns the metadata of the accessibility element <paramref name="id"/>.</para>
    /// </summary>
    public Variant ElementGetMeta(Rid id)
    {
        Variant ret = NativeCalls.kaizen_icall_1_65(MethodBind7, KaizenObject.GetPtr(this), id);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetWindowRect, 2386961724ul);

    /// <summary>
    /// <para>Sets window outer (with decorations) and inner (without decorations) bounds for assistive apps.</para>
    /// <para><b>Note:</b> This method is implemented on Linux, macOS, and Windows.</para>
    /// <para><b>Note:</b> Advanced users only! <see cref="Kaizen.Window"/> objects call this method automatically.</para>
    /// </summary>
    public unsafe void SetWindowRect(int windowId, Rect2 rectOut, Rect2 rectIn)
    {
        NativeCalls.kaizen_icall_3_66(MethodBind8, KaizenObject.GetPtr(this), windowId, &rectOut, &rectIn);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetWindowFocused, 300928843ul);

    /// <summary>
    /// <para>Sets the window focused state for assistive apps.</para>
    /// <para><b>Note:</b> This method is implemented on Linux, macOS, and Windows.</para>
    /// <para><b>Note:</b> Advanced users only! <see cref="Kaizen.Window"/> objects call this method automatically.</para>
    /// </summary>
    public void SetWindowFocused(int windowId, bool focused)
    {
        NativeCalls.kaizen_icall_2_67(MethodBind9, KaizenObject.GetPtr(this), windowId, focused.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetFocus, 2722037293ul);

    /// <summary>
    /// <para>Sets currently focused element.</para>
    /// </summary>
    public void UpdateSetFocus(Rid id)
    {
        NativeCalls.kaizen_icall_1_63(MethodBind10, KaizenObject.GetPtr(this), id);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetWindowRoot, 495598643ul);

    /// <summary>
    /// <para>Returns the main accessibility element of the OS native window.</para>
    /// </summary>
    public Rid GetWindowRoot(int windowId)
    {
        Rid ret = NativeCalls.kaizen_icall_1_68(MethodBind11, KaizenObject.GetPtr(this), windowId);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetRole, 3747886520ul);

    /// <summary>
    /// <para>Sets element accessibility role.</para>
    /// </summary>
    public void UpdateSetRole(Rid id, AccessibilityServer.AccessibilityRole role)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind12, KaizenObject.GetPtr(this), id, (int)role);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetName, 2726140452ul);

    /// <summary>
    /// <para>Sets element accessibility name.</para>
    /// </summary>
    public void UpdateSetName(Rid id, string name)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind13, KaizenObject.GetPtr(this), id, name);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetBrailleLabel, 2726140452ul);

    /// <summary>
    /// <para>Sets element accessibility label for Braille display.</para>
    /// </summary>
    public void UpdateSetBrailleLabel(Rid id, string name)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind14, KaizenObject.GetPtr(this), id, name);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetBrailleRoleDescription, 2726140452ul);

    /// <summary>
    /// <para>Sets element accessibility role description for Braille display.</para>
    /// </summary>
    public void UpdateSetBrailleRoleDescription(Rid id, string description)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind15, KaizenObject.GetPtr(this), id, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetExtraInfo, 2726140452ul);

    /// <summary>
    /// <para>Sets element accessibility extra information added to the element name.</para>
    /// </summary>
    public void UpdateSetExtraInfo(Rid id, string name)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind16, KaizenObject.GetPtr(this), id, name);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetDescription, 2726140452ul);

    /// <summary>
    /// <para>Sets element accessibility description.</para>
    /// </summary>
    public void UpdateSetDescription(Rid id, string description)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind17, KaizenObject.GetPtr(this), id, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetValue, 2726140452ul);

    /// <summary>
    /// <para>Sets element text value.</para>
    /// </summary>
    public void UpdateSetValue(Rid id, string value)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind18, KaizenObject.GetPtr(this), id, value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTooltip, 2726140452ul);

    /// <summary>
    /// <para>Sets tooltip text.</para>
    /// </summary>
    public void UpdateSetTooltip(Rid id, string tooltip)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind19, KaizenObject.GetPtr(this), id, tooltip);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetBounds, 1378122625ul);

    /// <summary>
    /// <para>Sets element bounding box, relative to the node position.</para>
    /// </summary>
    public unsafe void UpdateSetBounds(Rid id, Rect2 rect)
    {
        NativeCalls.kaizen_icall_2_71(MethodBind20, KaizenObject.GetPtr(this), id, &rect);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTransform, 1246044741ul);

    /// <summary>
    /// <para>Sets element 2D transform.</para>
    /// </summary>
    public unsafe void UpdateSetTransform(Rid id, Transform2D transform)
    {
        NativeCalls.kaizen_icall_2_72(MethodBind21, KaizenObject.GetPtr(this), id, &transform);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddChild, 395945892ul);

    /// <summary>
    /// <para>Adds a child accessibility element.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.Node"/> children and sub-elements are added to the child list automatically.</para>
    /// </summary>
    public void UpdateAddChild(Rid id, Rid childId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind22, KaizenObject.GetPtr(this), id, childId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddRelatedControls, 395945892ul);

    /// <summary>
    /// <para>Adds an element that is controlled by this element.</para>
    /// </summary>
    public void UpdateAddRelatedControls(Rid id, Rid relatedId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind23, KaizenObject.GetPtr(this), id, relatedId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddRelatedDetails, 395945892ul);

    /// <summary>
    /// <para>Adds an element that details this element.</para>
    /// </summary>
    public void UpdateAddRelatedDetails(Rid id, Rid relatedId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind24, KaizenObject.GetPtr(this), id, relatedId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddRelatedDescribedBy, 395945892ul);

    /// <summary>
    /// <para>Adds an element that describes this element.</para>
    /// </summary>
    public void UpdateAddRelatedDescribedBy(Rid id, Rid relatedId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind25, KaizenObject.GetPtr(this), id, relatedId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddRelatedFlowTo, 395945892ul);

    /// <summary>
    /// <para>Adds an element that this element flow into.</para>
    /// </summary>
    public void UpdateAddRelatedFlowTo(Rid id, Rid relatedId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind26, KaizenObject.GetPtr(this), id, relatedId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddRelatedLabeledBy, 395945892ul);

    /// <summary>
    /// <para>Adds an element that labels this element.</para>
    /// </summary>
    public void UpdateAddRelatedLabeledBy(Rid id, Rid relatedId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind27, KaizenObject.GetPtr(this), id, relatedId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddRelatedRadioGroup, 395945892ul);

    /// <summary>
    /// <para>Adds an element that is part of the same radio group.</para>
    /// <para><b>Note:</b> This method should be called on each element of the group, using all other elements as <paramref name="relatedId"/>.</para>
    /// </summary>
    public void UpdateAddRelatedRadioGroup(Rid id, Rid relatedId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind28, KaizenObject.GetPtr(this), id, relatedId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetActiveDescendant, 395945892ul);

    /// <summary>
    /// <para>Adds an element that is an active descendant of this element.</para>
    /// </summary>
    public void UpdateSetActiveDescendant(Rid id, Rid otherId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind29, KaizenObject.GetPtr(this), id, otherId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetNextOnLine, 395945892ul);

    /// <summary>
    /// <para>Sets next element on the line.</para>
    /// </summary>
    public void UpdateSetNextOnLine(Rid id, Rid otherId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind30, KaizenObject.GetPtr(this), id, otherId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetPreviousOnLine, 395945892ul);

    /// <summary>
    /// <para>Sets previous element on the line.</para>
    /// </summary>
    public void UpdateSetPreviousOnLine(Rid id, Rid otherId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind31, KaizenObject.GetPtr(this), id, otherId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetMemberOf, 395945892ul);

    /// <summary>
    /// <para>Sets the element to be a member of the group.</para>
    /// </summary>
    public void UpdateSetMemberOf(Rid id, Rid groupId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind32, KaizenObject.GetPtr(this), id, groupId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetInPageLinkTarget, 395945892ul);

    /// <summary>
    /// <para>Sets target element for the link.</para>
    /// </summary>
    public void UpdateSetInPageLinkTarget(Rid id, Rid otherId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind33, KaizenObject.GetPtr(this), id, otherId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind34 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetErrorMessage, 395945892ul);

    /// <summary>
    /// <para>Sets an element which contains an error message for this element.</para>
    /// </summary>
    public void UpdateSetErrorMessage(Rid id, Rid otherId)
    {
        NativeCalls.kaizen_icall_2_73(MethodBind34, KaizenObject.GetPtr(this), id, otherId);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind35 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetLive, 2993365237ul);

    /// <summary>
    /// <para>Sets the priority of the live region updates.</para>
    /// </summary>
    public void UpdateSetLive(Rid id, AccessibilityServer.AccessibilityLiveMode live)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind35, KaizenObject.GetPtr(this), id, (int)live);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind36 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddAction, 3960092835ul);

    /// <summary>
    /// <para>Adds a callback for the accessibility action (action which can be performed by using a special screen reader command or buttons on the Braille display), and marks this action as supported. The action callback receives one <see cref="Kaizen.Variant"/> argument, which value depends on action type.</para>
    /// </summary>
    public void UpdateAddAction(Rid id, AccessibilityServer.AccessibilityAction action, Callable callable)
    {
        NativeCalls.kaizen_icall_3_74(MethodBind36, KaizenObject.GetPtr(this), id, (int)action, callable);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind37 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateAddCustomAction, 4153150897ul);

    /// <summary>
    /// <para>Adds support for a custom accessibility action. <paramref name="actionId"/> is passed as an argument to the callback of <see cref="Kaizen.AccessibilityServer.AccessibilityAction.Custom"/> action.</para>
    /// </summary>
    public void UpdateAddCustomAction(Rid id, int actionId, string actionDescription)
    {
        NativeCalls.kaizen_icall_3_75(MethodBind37, KaizenObject.GetPtr(this), id, actionId, actionDescription);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind38 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTableRowCount, 3411492887ul);

    /// <summary>
    /// <para>Sets number of rows in the table.</para>
    /// </summary>
    public void UpdateSetTableRowCount(Rid id, int count)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind38, KaizenObject.GetPtr(this), id, count);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind39 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTableColumnCount, 3411492887ul);

    /// <summary>
    /// <para>Sets number of columns in the table.</para>
    /// </summary>
    public void UpdateSetTableColumnCount(Rid id, int count)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind39, KaizenObject.GetPtr(this), id, count);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind40 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTableRowIndex, 3411492887ul);

    /// <summary>
    /// <para>Sets position of the row in the table.</para>
    /// </summary>
    public void UpdateSetTableRowIndex(Rid id, int index)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind40, KaizenObject.GetPtr(this), id, index);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind41 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTableColumnIndex, 3411492887ul);

    /// <summary>
    /// <para>Sets position of the column.</para>
    /// </summary>
    public void UpdateSetTableColumnIndex(Rid id, int index)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind41, KaizenObject.GetPtr(this), id, index);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind42 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTableCellPosition, 4288446313ul);

    /// <summary>
    /// <para>Sets cell position in the table.</para>
    /// </summary>
    public void UpdateSetTableCellPosition(Rid id, int rowIndex, int columnIndex)
    {
        NativeCalls.kaizen_icall_3_76(MethodBind42, KaizenObject.GetPtr(this), id, rowIndex, columnIndex);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind43 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTableCellSpan, 4288446313ul);

    /// <summary>
    /// <para>Sets cell row/column span.</para>
    /// </summary>
    public void UpdateSetTableCellSpan(Rid id, int rowSpan, int columnSpan)
    {
        NativeCalls.kaizen_icall_3_76(MethodBind43, KaizenObject.GetPtr(this), id, rowSpan, columnSpan);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind44 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetListItemCount, 3411492887ul);

    /// <summary>
    /// <para>Sets number of items in the list.</para>
    /// </summary>
    public void UpdateSetListItemCount(Rid id, int size)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind44, KaizenObject.GetPtr(this), id, size);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind45 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetListItemIndex, 3411492887ul);

    /// <summary>
    /// <para>Sets the position of the element in the list.</para>
    /// </summary>
    public void UpdateSetListItemIndex(Rid id, int index)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind45, KaizenObject.GetPtr(this), id, index);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind46 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetListItemLevel, 3411492887ul);

    /// <summary>
    /// <para>Sets the hierarchical level of the element in the list.</para>
    /// </summary>
    public void UpdateSetListItemLevel(Rid id, int level)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind46, KaizenObject.GetPtr(this), id, level);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind47 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetListItemSelected, 1265174801ul);

    /// <summary>
    /// <para>Sets list/tree item selected status.</para>
    /// </summary>
    public void UpdateSetListItemSelected(Rid id, bool selected)
    {
        NativeCalls.kaizen_icall_2_77(MethodBind47, KaizenObject.GetPtr(this), id, selected.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind48 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetListItemExpanded, 1265174801ul);

    /// <summary>
    /// <para>Sets list/tree item expanded status.</para>
    /// </summary>
    public void UpdateSetListItemExpanded(Rid id, bool expanded)
    {
        NativeCalls.kaizen_icall_2_77(MethodBind48, KaizenObject.GetPtr(this), id, expanded.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind49 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetPopupType, 690307634ul);

    /// <summary>
    /// <para>Sets popup type for popup buttons.</para>
    /// </summary>
    public void UpdateSetPopupType(Rid id, AccessibilityServer.AccessibilityPopupType popup)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind49, KaizenObject.GetPtr(this), id, (int)popup);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind50 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetChecked, 1265174801ul);

    /// <summary>
    /// <para>Sets element checked state.</para>
    /// </summary>
    public void UpdateSetChecked(Rid id, bool checekd)
    {
        NativeCalls.kaizen_icall_2_77(MethodBind50, KaizenObject.GetPtr(this), id, checekd.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind51 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetNumValue, 1794382983ul);

    /// <summary>
    /// <para>Sets numeric value.</para>
    /// </summary>
    public void UpdateSetNumValue(Rid id, double position)
    {
        NativeCalls.kaizen_icall_2_78(MethodBind51, KaizenObject.GetPtr(this), id, position);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind52 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetNumRange, 2513314492ul);

    /// <summary>
    /// <para>Sets numeric value range.</para>
    /// </summary>
    public void UpdateSetNumRange(Rid id, double min, double max)
    {
        NativeCalls.kaizen_icall_3_79(MethodBind52, KaizenObject.GetPtr(this), id, min, max);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind53 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetNumStep, 1794382983ul);

    /// <summary>
    /// <para>Sets numeric value step.</para>
    /// </summary>
    public void UpdateSetNumStep(Rid id, double step)
    {
        NativeCalls.kaizen_icall_2_78(MethodBind53, KaizenObject.GetPtr(this), id, step);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind54 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetNumJump, 1794382983ul);

    /// <summary>
    /// <para>Sets numeric value jump.</para>
    /// </summary>
    public void UpdateSetNumJump(Rid id, double jump)
    {
        NativeCalls.kaizen_icall_2_78(MethodBind54, KaizenObject.GetPtr(this), id, jump);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind55 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetScrollX, 1794382983ul);

    /// <summary>
    /// <para>Sets scroll bar x position.</para>
    /// </summary>
    public void UpdateSetScrollX(Rid id, double position)
    {
        NativeCalls.kaizen_icall_2_78(MethodBind55, KaizenObject.GetPtr(this), id, position);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind56 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetScrollXRange, 2513314492ul);

    /// <summary>
    /// <para>Sets scroll bar x range.</para>
    /// </summary>
    public void UpdateSetScrollXRange(Rid id, double min, double max)
    {
        NativeCalls.kaizen_icall_3_79(MethodBind56, KaizenObject.GetPtr(this), id, min, max);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind57 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetScrollY, 1794382983ul);

    /// <summary>
    /// <para>Sets scroll bar y position.</para>
    /// </summary>
    public void UpdateSetScrollY(Rid id, double position)
    {
        NativeCalls.kaizen_icall_2_78(MethodBind57, KaizenObject.GetPtr(this), id, position);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind58 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetScrollYRange, 2513314492ul);

    /// <summary>
    /// <para>Sets scroll bar y range.</para>
    /// </summary>
    public void UpdateSetScrollYRange(Rid id, double min, double max)
    {
        NativeCalls.kaizen_icall_3_79(MethodBind58, KaizenObject.GetPtr(this), id, min, max);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind59 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTextDecorations, 457503484ul);

    /// <summary>
    /// <para>Sets text underline/overline/strikethrough.</para>
    /// </summary>
    /// <param name="color">If the parameter is null, then the default value is <c>new Color(0.0f, 0.0f, 0.0f, 1.0f)</c>.</param>
    public unsafe void UpdateSetTextDecorations(Rid id, bool underline, bool strikethrough, bool overline, Nullable<Color> color = null)
    {
        Color colorOrDefVal = color.HasValue ? color.Value : new Color(0.0f, 0.0f, 0.0f, 1.0f);
        NativeCalls.kaizen_icall_5_80(MethodBind59, KaizenObject.GetPtr(this), id, underline.ToKaizenBool(), strikethrough.ToKaizenBool(), overline.ToKaizenBool(), &colorOrDefVal);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind60 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTextAlign, 3725995085ul);

    /// <summary>
    /// <para>Sets element text alignment.</para>
    /// </summary>
    public void UpdateSetTextAlign(Rid id, HorizontalAlignment align)
    {
        NativeCalls.kaizen_icall_2_69(MethodBind60, KaizenObject.GetPtr(this), id, (int)align);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind61 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTextSelection, 3119144029ul);

    /// <summary>
    /// <para>Sets text selection to the text field. <paramref name="textStartId"/> and <paramref name="textEndId"/> should be elements created by <see cref="Kaizen.AccessibilityServerInstance.CreateSubTextEditElements(Rid, Rid, float, int, bool)"/>. Character offsets are relative to the corresponding element.</para>
    /// </summary>
    public void UpdateSetTextSelection(Rid id, Rid textStartId, int startChar, Rid textEndId, int endChar)
    {
        NativeCalls.kaizen_icall_5_81(MethodBind61, KaizenObject.GetPtr(this), id, textStartId, startChar, textEndId, endChar);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind62 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetFlag, 1473043386ul);

    /// <summary>
    /// <para>Sets element flag.</para>
    /// </summary>
    public void UpdateSetFlag(Rid id, AccessibilityServer.AccessibilityFlags flag, bool value)
    {
        NativeCalls.kaizen_icall_3_82(MethodBind62, KaizenObject.GetPtr(this), id, (int)flag, value.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind63 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetClassname, 2726140452ul);

    /// <summary>
    /// <para>Sets element class name.</para>
    /// </summary>
    public void UpdateSetClassname(Rid id, string classname)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind63, KaizenObject.GetPtr(this), id, classname);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind64 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetPlaceholder, 2726140452ul);

    /// <summary>
    /// <para>Sets placeholder text.</para>
    /// </summary>
    public void UpdateSetPlaceholder(Rid id, string placeholder)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind64, KaizenObject.GetPtr(this), id, placeholder);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind65 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetLanguage, 2726140452ul);

    /// <summary>
    /// <para>Sets element text language.</para>
    /// </summary>
    public void UpdateSetLanguage(Rid id, string language)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind65, KaizenObject.GetPtr(this), id, language);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind66 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetTextOrientation, 1265174801ul);

    /// <summary>
    /// <para>Sets text orientation.</para>
    /// </summary>
    public void UpdateSetTextOrientation(Rid id, bool vertical)
    {
        NativeCalls.kaizen_icall_2_77(MethodBind66, KaizenObject.GetPtr(this), id, vertical.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind67 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetListOrientation, 1265174801ul);

    /// <summary>
    /// <para>Sets the orientation of the list elements.</para>
    /// </summary>
    public void UpdateSetListOrientation(Rid id, bool vertical)
    {
        NativeCalls.kaizen_icall_2_77(MethodBind67, KaizenObject.GetPtr(this), id, vertical.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind68 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetShortcut, 2726140452ul);

    /// <summary>
    /// <para>Sets the list of keyboard shortcuts used by element.</para>
    /// </summary>
    public void UpdateSetShortcut(Rid id, string shortcut)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind68, KaizenObject.GetPtr(this), id, shortcut);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind69 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetUrl, 2726140452ul);

    /// <summary>
    /// <para>Sets link URL.</para>
    /// </summary>
    public void UpdateSetUrl(Rid id, string url)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind69, KaizenObject.GetPtr(this), id, url);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind70 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetRoleDescription, 2726140452ul);

    /// <summary>
    /// <para>Sets element accessibility role description text.</para>
    /// </summary>
    public void UpdateSetRoleDescription(Rid id, string description)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind70, KaizenObject.GetPtr(this), id, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind71 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetStateDescription, 2726140452ul);

    /// <summary>
    /// <para>Sets human-readable description of the current checked state.</para>
    /// </summary>
    public void UpdateSetStateDescription(Rid id, string description)
    {
        NativeCalls.kaizen_icall_2_70(MethodBind71, KaizenObject.GetPtr(this), id, description);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind72 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetColorValue, 2948539648ul);

    /// <summary>
    /// <para>Sets element color value.</para>
    /// </summary>
    public unsafe void UpdateSetColorValue(Rid id, Color color)
    {
        NativeCalls.kaizen_icall_2_83(MethodBind72, KaizenObject.GetPtr(this), id, &color);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind73 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetBackgroundColor, 2948539648ul);

    /// <summary>
    /// <para>Sets element background color.</para>
    /// </summary>
    public unsafe void UpdateSetBackgroundColor(Rid id, Color color)
    {
        NativeCalls.kaizen_icall_2_83(MethodBind73, KaizenObject.GetPtr(this), id, &color);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind74 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateSetForegroundColor, 2948539648ul);

    /// <summary>
    /// <para>Sets element foreground color.</para>
    /// </summary>
    public unsafe void UpdateSetForegroundColor(Rid id, Color color)
    {
        NativeCalls.kaizen_icall_2_83(MethodBind74, KaizenObject.GetPtr(this), id, &color);
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
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
        /// <summary>
        /// Cached name for the 'is_supported' method.
        /// </summary>
        public static readonly StringName IsSupported = "is_supported";
        /// <summary>
        /// Cached name for the 'create_element' method.
        /// </summary>
        public static readonly StringName CreateElement = "create_element";
        /// <summary>
        /// Cached name for the 'create_sub_element' method.
        /// </summary>
        public static readonly StringName CreateSubElement = "create_sub_element";
        /// <summary>
        /// Cached name for the 'create_sub_text_edit_elements' method.
        /// </summary>
        public static readonly StringName CreateSubTextEditElements = "create_sub_text_edit_elements";
        /// <summary>
        /// Cached name for the 'has_element' method.
        /// </summary>
        public static readonly StringName HasElement = "has_element";
        /// <summary>
        /// Cached name for the 'free_element' method.
        /// </summary>
        public static readonly StringName FreeElement = "free_element";
        /// <summary>
        /// Cached name for the 'element_set_meta' method.
        /// </summary>
        public static readonly StringName ElementSetMeta = "element_set_meta";
        /// <summary>
        /// Cached name for the 'element_get_meta' method.
        /// </summary>
        public static readonly StringName ElementGetMeta = "element_get_meta";
        /// <summary>
        /// Cached name for the 'set_window_rect' method.
        /// </summary>
        public static readonly StringName SetWindowRect = "set_window_rect";
        /// <summary>
        /// Cached name for the 'set_window_focused' method.
        /// </summary>
        public static readonly StringName SetWindowFocused = "set_window_focused";
        /// <summary>
        /// Cached name for the 'update_set_focus' method.
        /// </summary>
        public static readonly StringName UpdateSetFocus = "update_set_focus";
        /// <summary>
        /// Cached name for the 'get_window_root' method.
        /// </summary>
        public static readonly StringName GetWindowRoot = "get_window_root";
        /// <summary>
        /// Cached name for the 'update_set_role' method.
        /// </summary>
        public static readonly StringName UpdateSetRole = "update_set_role";
        /// <summary>
        /// Cached name for the 'update_set_name' method.
        /// </summary>
        public static readonly StringName UpdateSetName = "update_set_name";
        /// <summary>
        /// Cached name for the 'update_set_braille_label' method.
        /// </summary>
        public static readonly StringName UpdateSetBrailleLabel = "update_set_braille_label";
        /// <summary>
        /// Cached name for the 'update_set_braille_role_description' method.
        /// </summary>
        public static readonly StringName UpdateSetBrailleRoleDescription = "update_set_braille_role_description";
        /// <summary>
        /// Cached name for the 'update_set_extra_info' method.
        /// </summary>
        public static readonly StringName UpdateSetExtraInfo = "update_set_extra_info";
        /// <summary>
        /// Cached name for the 'update_set_description' method.
        /// </summary>
        public static readonly StringName UpdateSetDescription = "update_set_description";
        /// <summary>
        /// Cached name for the 'update_set_value' method.
        /// </summary>
        public static readonly StringName UpdateSetValue = "update_set_value";
        /// <summary>
        /// Cached name for the 'update_set_tooltip' method.
        /// </summary>
        public static readonly StringName UpdateSetTooltip = "update_set_tooltip";
        /// <summary>
        /// Cached name for the 'update_set_bounds' method.
        /// </summary>
        public static readonly StringName UpdateSetBounds = "update_set_bounds";
        /// <summary>
        /// Cached name for the 'update_set_transform' method.
        /// </summary>
        public static readonly StringName UpdateSetTransform = "update_set_transform";
        /// <summary>
        /// Cached name for the 'update_add_child' method.
        /// </summary>
        public static readonly StringName UpdateAddChild = "update_add_child";
        /// <summary>
        /// Cached name for the 'update_add_related_controls' method.
        /// </summary>
        public static readonly StringName UpdateAddRelatedControls = "update_add_related_controls";
        /// <summary>
        /// Cached name for the 'update_add_related_details' method.
        /// </summary>
        public static readonly StringName UpdateAddRelatedDetails = "update_add_related_details";
        /// <summary>
        /// Cached name for the 'update_add_related_described_by' method.
        /// </summary>
        public static readonly StringName UpdateAddRelatedDescribedBy = "update_add_related_described_by";
        /// <summary>
        /// Cached name for the 'update_add_related_flow_to' method.
        /// </summary>
        public static readonly StringName UpdateAddRelatedFlowTo = "update_add_related_flow_to";
        /// <summary>
        /// Cached name for the 'update_add_related_labeled_by' method.
        /// </summary>
        public static readonly StringName UpdateAddRelatedLabeledBy = "update_add_related_labeled_by";
        /// <summary>
        /// Cached name for the 'update_add_related_radio_group' method.
        /// </summary>
        public static readonly StringName UpdateAddRelatedRadioGroup = "update_add_related_radio_group";
        /// <summary>
        /// Cached name for the 'update_set_active_descendant' method.
        /// </summary>
        public static readonly StringName UpdateSetActiveDescendant = "update_set_active_descendant";
        /// <summary>
        /// Cached name for the 'update_set_next_on_line' method.
        /// </summary>
        public static readonly StringName UpdateSetNextOnLine = "update_set_next_on_line";
        /// <summary>
        /// Cached name for the 'update_set_previous_on_line' method.
        /// </summary>
        public static readonly StringName UpdateSetPreviousOnLine = "update_set_previous_on_line";
        /// <summary>
        /// Cached name for the 'update_set_member_of' method.
        /// </summary>
        public static readonly StringName UpdateSetMemberOf = "update_set_member_of";
        /// <summary>
        /// Cached name for the 'update_set_in_page_link_target' method.
        /// </summary>
        public static readonly StringName UpdateSetInPageLinkTarget = "update_set_in_page_link_target";
        /// <summary>
        /// Cached name for the 'update_set_error_message' method.
        /// </summary>
        public static readonly StringName UpdateSetErrorMessage = "update_set_error_message";
        /// <summary>
        /// Cached name for the 'update_set_live' method.
        /// </summary>
        public static readonly StringName UpdateSetLive = "update_set_live";
        /// <summary>
        /// Cached name for the 'update_add_action' method.
        /// </summary>
        public static readonly StringName UpdateAddAction = "update_add_action";
        /// <summary>
        /// Cached name for the 'update_add_custom_action' method.
        /// </summary>
        public static readonly StringName UpdateAddCustomAction = "update_add_custom_action";
        /// <summary>
        /// Cached name for the 'update_set_table_row_count' method.
        /// </summary>
        public static readonly StringName UpdateSetTableRowCount = "update_set_table_row_count";
        /// <summary>
        /// Cached name for the 'update_set_table_column_count' method.
        /// </summary>
        public static readonly StringName UpdateSetTableColumnCount = "update_set_table_column_count";
        /// <summary>
        /// Cached name for the 'update_set_table_row_index' method.
        /// </summary>
        public static readonly StringName UpdateSetTableRowIndex = "update_set_table_row_index";
        /// <summary>
        /// Cached name for the 'update_set_table_column_index' method.
        /// </summary>
        public static readonly StringName UpdateSetTableColumnIndex = "update_set_table_column_index";
        /// <summary>
        /// Cached name for the 'update_set_table_cell_position' method.
        /// </summary>
        public static readonly StringName UpdateSetTableCellPosition = "update_set_table_cell_position";
        /// <summary>
        /// Cached name for the 'update_set_table_cell_span' method.
        /// </summary>
        public static readonly StringName UpdateSetTableCellSpan = "update_set_table_cell_span";
        /// <summary>
        /// Cached name for the 'update_set_list_item_count' method.
        /// </summary>
        public static readonly StringName UpdateSetListItemCount = "update_set_list_item_count";
        /// <summary>
        /// Cached name for the 'update_set_list_item_index' method.
        /// </summary>
        public static readonly StringName UpdateSetListItemIndex = "update_set_list_item_index";
        /// <summary>
        /// Cached name for the 'update_set_list_item_level' method.
        /// </summary>
        public static readonly StringName UpdateSetListItemLevel = "update_set_list_item_level";
        /// <summary>
        /// Cached name for the 'update_set_list_item_selected' method.
        /// </summary>
        public static readonly StringName UpdateSetListItemSelected = "update_set_list_item_selected";
        /// <summary>
        /// Cached name for the 'update_set_list_item_expanded' method.
        /// </summary>
        public static readonly StringName UpdateSetListItemExpanded = "update_set_list_item_expanded";
        /// <summary>
        /// Cached name for the 'update_set_popup_type' method.
        /// </summary>
        public static readonly StringName UpdateSetPopupType = "update_set_popup_type";
        /// <summary>
        /// Cached name for the 'update_set_checked' method.
        /// </summary>
        public static readonly StringName UpdateSetChecked = "update_set_checked";
        /// <summary>
        /// Cached name for the 'update_set_num_value' method.
        /// </summary>
        public static readonly StringName UpdateSetNumValue = "update_set_num_value";
        /// <summary>
        /// Cached name for the 'update_set_num_range' method.
        /// </summary>
        public static readonly StringName UpdateSetNumRange = "update_set_num_range";
        /// <summary>
        /// Cached name for the 'update_set_num_step' method.
        /// </summary>
        public static readonly StringName UpdateSetNumStep = "update_set_num_step";
        /// <summary>
        /// Cached name for the 'update_set_num_jump' method.
        /// </summary>
        public static readonly StringName UpdateSetNumJump = "update_set_num_jump";
        /// <summary>
        /// Cached name for the 'update_set_scroll_x' method.
        /// </summary>
        public static readonly StringName UpdateSetScrollX = "update_set_scroll_x";
        /// <summary>
        /// Cached name for the 'update_set_scroll_x_range' method.
        /// </summary>
        public static readonly StringName UpdateSetScrollXRange = "update_set_scroll_x_range";
        /// <summary>
        /// Cached name for the 'update_set_scroll_y' method.
        /// </summary>
        public static readonly StringName UpdateSetScrollY = "update_set_scroll_y";
        /// <summary>
        /// Cached name for the 'update_set_scroll_y_range' method.
        /// </summary>
        public static readonly StringName UpdateSetScrollYRange = "update_set_scroll_y_range";
        /// <summary>
        /// Cached name for the 'update_set_text_decorations' method.
        /// </summary>
        public static readonly StringName UpdateSetTextDecorations = "update_set_text_decorations";
        /// <summary>
        /// Cached name for the 'update_set_text_align' method.
        /// </summary>
        public static readonly StringName UpdateSetTextAlign = "update_set_text_align";
        /// <summary>
        /// Cached name for the 'update_set_text_selection' method.
        /// </summary>
        public static readonly StringName UpdateSetTextSelection = "update_set_text_selection";
        /// <summary>
        /// Cached name for the 'update_set_flag' method.
        /// </summary>
        public static readonly StringName UpdateSetFlag = "update_set_flag";
        /// <summary>
        /// Cached name for the 'update_set_classname' method.
        /// </summary>
        public static readonly StringName UpdateSetClassname = "update_set_classname";
        /// <summary>
        /// Cached name for the 'update_set_placeholder' method.
        /// </summary>
        public static readonly StringName UpdateSetPlaceholder = "update_set_placeholder";
        /// <summary>
        /// Cached name for the 'update_set_language' method.
        /// </summary>
        public static readonly StringName UpdateSetLanguage = "update_set_language";
        /// <summary>
        /// Cached name for the 'update_set_text_orientation' method.
        /// </summary>
        public static readonly StringName UpdateSetTextOrientation = "update_set_text_orientation";
        /// <summary>
        /// Cached name for the 'update_set_list_orientation' method.
        /// </summary>
        public static readonly StringName UpdateSetListOrientation = "update_set_list_orientation";
        /// <summary>
        /// Cached name for the 'update_set_shortcut' method.
        /// </summary>
        public static readonly StringName UpdateSetShortcut = "update_set_shortcut";
        /// <summary>
        /// Cached name for the 'update_set_url' method.
        /// </summary>
        public static readonly StringName UpdateSetUrl = "update_set_url";
        /// <summary>
        /// Cached name for the 'update_set_role_description' method.
        /// </summary>
        public static readonly StringName UpdateSetRoleDescription = "update_set_role_description";
        /// <summary>
        /// Cached name for the 'update_set_state_description' method.
        /// </summary>
        public static readonly StringName UpdateSetStateDescription = "update_set_state_description";
        /// <summary>
        /// Cached name for the 'update_set_color_value' method.
        /// </summary>
        public static readonly StringName UpdateSetColorValue = "update_set_color_value";
        /// <summary>
        /// Cached name for the 'update_set_background_color' method.
        /// </summary>
        public static readonly StringName UpdateSetBackgroundColor = "update_set_background_color";
        /// <summary>
        /// Cached name for the 'update_set_foreground_color' method.
        /// </summary>
        public static readonly StringName UpdateSetForegroundColor = "update_set_foreground_color";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
