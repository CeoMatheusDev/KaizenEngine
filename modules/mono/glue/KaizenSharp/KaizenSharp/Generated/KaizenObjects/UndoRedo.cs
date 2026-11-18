namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>UndoRedo works by registering methods and property changes inside "actions". You can create an action, then provide ways to do and undo this action using function calls and property changes, then commit the action.</para>
/// <para>When an action is committed, all of the <c>do_*</c> methods will run. If the <see cref="Kaizen.UndoRedo.Undo()"/> method is used, the <c>undo_*</c> methods will run. If the <see cref="Kaizen.UndoRedo.Redo()"/> method is used, once again, all of the <c>do_*</c> methods will run.</para>
/// <para>Here's an example on how to add an action:</para>
/// <para><code>
/// private UndoRedo _undoRedo;
/// 
/// public override void _Ready()
/// {
/// 	_undoRedo = new UndoRedo();
/// }
/// 
/// public void DoSomething()
/// {
/// 	// Put your code here.
/// }
/// 
/// public void UndoSomething()
/// {
/// 	// Put here the code that reverts what's done by "DoSomething()".
/// }
/// 
/// private void OnMyButtonPressed()
/// {
/// 	var node = GetNode&lt;Node2D&gt;("MyNode2D");
/// 	_undoRedo.CreateAction("Move the node");
/// 	_undoRedo.AddDoMethod(new Callable(this, MethodName.DoSomething));
/// 	_undoRedo.AddUndoMethod(new Callable(this, MethodName.UndoSomething));
/// 	_undoRedo.AddDoProperty(node, "position", new Vector2(100, 100));
/// 	_undoRedo.AddUndoProperty(node, "position", node.Position);
/// 	_undoRedo.CommitAction();
/// }
/// </code></para>
/// <para>Before calling any of the <c>add_(un)do_*</c> methods, you need to first call <see cref="Kaizen.UndoRedo.CreateAction(string, UndoRedo.MergeMode, bool)"/>. Afterwards you need to call <see cref="Kaizen.UndoRedo.CommitAction(bool)"/>.</para>
/// <para>If you don't need to register a method, you can leave <see cref="Kaizen.UndoRedo.AddDoMethod(Callable)"/> and <see cref="Kaizen.UndoRedo.AddUndoMethod(Callable)"/> out; the same goes for properties. You can also register more than one method/property.</para>
/// <para>If you are making an <c>EditorPlugin</c> and want to integrate into the editor's undo history, use <c>EditorUndoRedoManager</c> instead.</para>
/// <para>If you are registering multiple properties/method which depend on one another, be aware that by default undo operation are called in the same order they have been added. Therefore instead of grouping do operation with their undo operations it is better to group do on one side and undo on the other as shown below.</para>
/// <para><code>
/// _undo_redo.CreateAction("Add object");
/// 
/// // DO
/// _undo_redo.AddDoMethod(new Callable(this, MethodName.CreateObject));
/// _undo_redo.AddDoMethod(new Callable(this, MethodName.AddObjectToSingleton));
/// 
/// // UNDO
/// _undo_redo.AddUndoMethod(new Callable(this, MethodName.RemoveObjectFromSingleton));
/// _undo_redo.AddUndoMethod(new Callable(this, MethodName.DestroyThatObject));
/// 
/// _undo_redo.CommitAction();
/// </code></para>
/// </summary>
public partial class UndoRedo : KaizenObject
{
    public enum MergeMode : long
    {
        /// <summary>
        /// <para>Makes "do"/"undo" operations stay in separate actions.</para>
        /// </summary>
        Disable = 0,
        /// <summary>
        /// <para>Merges this action with the previous one if they have the same name. Keeps only the first action's "undo" operations and the last action's "do" operations. Useful for sequential changes to a single value.</para>
        /// </summary>
        Ends = 1,
        /// <summary>
        /// <para>Merges this action with the previous one if they have the same name.</para>
        /// </summary>
        All = 2
    }

    /// <summary>
    /// <para>The maximum number of steps that can be stored in the undo/redo history. If the number of stored steps exceeds this limit, older steps are removed from history and can no longer be reached by calling <see cref="Kaizen.UndoRedo.Undo()"/>. A value of <c>0</c> or lower means no limit.</para>
    /// </summary>
    public int MaxSteps
    {
        get
        {
            return GetMaxSteps();
        }
        set
        {
            SetMaxSteps(value);
        }
    }

    private static readonly System.Type CachedType = typeof(UndoRedo);

    private static readonly StringName NativeName = "UndoRedo";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public UndoRedo() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal UndoRedo(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal UndoRedo(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateAction, 3171901514ul);

    /// <summary>
    /// <para>Create a new action. After this is called, do all your calls to <see cref="Kaizen.UndoRedo.AddDoMethod(Callable)"/>, <see cref="Kaizen.UndoRedo.AddUndoMethod(Callable)"/>, <see cref="Kaizen.UndoRedo.AddDoProperty(KaizenObject, StringName, Variant)"/>, and <see cref="Kaizen.UndoRedo.AddUndoProperty(KaizenObject, StringName, Variant)"/>, then commit the action with <see cref="Kaizen.UndoRedo.CommitAction(bool)"/>.</para>
    /// <para>The way actions are merged is dictated by <paramref name="mergeMode"/>.</para>
    /// <para>The way undo operation are ordered in actions is dictated by <paramref name="backwardUndoOps"/>. When <paramref name="backwardUndoOps"/> is <see langword="false"/> undo option are ordered in the same order they were added. Which means the first operation to be added will be the first to be undone.</para>
    /// </summary>
    public void CreateAction(string name, UndoRedo.MergeMode mergeMode = (UndoRedo.MergeMode)(0), bool backwardUndoOps = false)
    {
        NativeCalls.kaizen_icall_3_422(MethodBind0, KaizenObject.GetPtr(this), name, (int)mergeMode, backwardUndoOps.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CommitAction, 3216645846ul);

    /// <summary>
    /// <para>Commit the action. If <paramref name="execute"/> is <see langword="true"/> (which it is by default), all "do" methods/properties are called/set when this function is called.</para>
    /// </summary>
    public void CommitAction(bool execute = true)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind1, KaizenObject.GetPtr(this), execute.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCommittingAction, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.UndoRedo"/> is currently committing the action, i.e. running its "do" method or property change (see <see cref="Kaizen.UndoRedo.CommitAction(bool)"/>).</para>
    /// </summary>
    public bool IsCommittingAction()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind2, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddDoMethod, 1611583062ul);

    /// <summary>
    /// <para>Register a <see cref="Kaizen.Callable"/> that will be called when the action is committed.</para>
    /// </summary>
    public void AddDoMethod(Callable callable)
    {
        NativeCalls.kaizen_icall_1_431(MethodBind3, KaizenObject.GetPtr(this), callable);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddUndoMethod, 1611583062ul);

    /// <summary>
    /// <para>Register a <see cref="Kaizen.Callable"/> that will be called when the action is undone.</para>
    /// </summary>
    public void AddUndoMethod(Callable callable)
    {
        NativeCalls.kaizen_icall_1_431(MethodBind4, KaizenObject.GetPtr(this), callable);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddDoProperty, 1017172818ul);

    /// <summary>
    /// <para>Register a <paramref name="property"/> that would change its value to <paramref name="value"/> when the action is committed.</para>
    /// </summary>
    public void AddDoProperty(KaizenObject @object, StringName property, Variant value)
    {
        NativeCalls.kaizen_icall_3_554(MethodBind5, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@object), (kaizen_string_name)(property?.NativeValue ?? default), value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddUndoProperty, 1017172818ul);

    /// <summary>
    /// <para>Register a <paramref name="property"/> that would change its value to <paramref name="value"/> when the action is undone.</para>
    /// </summary>
    public void AddUndoProperty(KaizenObject @object, StringName property, Variant value)
    {
        NativeCalls.kaizen_icall_3_554(MethodBind6, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@object), (kaizen_string_name)(property?.NativeValue ?? default), value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddDoReference, 3975164845ul);

    /// <summary>
    /// <para>Register a reference to an object that will be erased if the "do" history is deleted. This is useful for objects added by the "do" action and removed by the "undo" action.</para>
    /// <para>When the "do" history is deleted, if the object is a <see cref="Kaizen.RefCounted"/>, it will be unreferenced. Otherwise, it will be freed. Do not use for resources.</para>
    /// <para><code>
    /// var node = Node2D.new()
    /// undo_redo.create_action("Add node")
    /// undo_redo.add_do_method(add_child.bind(node))
    /// undo_redo.add_do_reference(node)
    /// undo_redo.add_undo_method(remove_child.bind(node))
    /// undo_redo.commit_action()
    /// </code></para>
    /// </summary>
    public void AddDoReference(KaizenObject @object)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind7, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@object));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddUndoReference, 3975164845ul);

    /// <summary>
    /// <para>Register a reference to an object that will be erased if the "undo" history is deleted. This is useful for objects added by the "undo" action and removed by the "do" action.</para>
    /// <para>When the "undo" history is deleted, if the object is a <see cref="Kaizen.RefCounted"/>, it will be unreferenced. Otherwise, it will be freed. Do not use for resources.</para>
    /// <para><code>
    /// var node = $Node2D
    /// undo_redo.create_action("Remove node")
    /// undo_redo.add_do_method(remove_child.bind(node))
    /// undo_redo.add_undo_method(add_child.bind(node))
    /// undo_redo.add_undo_reference(node)
    /// undo_redo.commit_action()
    /// </code></para>
    /// </summary>
    public void AddUndoReference(KaizenObject @object)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind8, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@object));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.StartForceKeepInMergeEnds, 3218959716ul);

    /// <summary>
    /// <para>Marks the next "do" and "undo" operations to be processed even if the action gets merged with another in the <see cref="Kaizen.UndoRedo.MergeMode.Ends"/> mode. Return to normal operation using <see cref="Kaizen.UndoRedo.EndForceKeepInMergeEnds()"/>.</para>
    /// </summary>
    public void StartForceKeepInMergeEnds()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.EndForceKeepInMergeEnds, 3218959716ul);

    /// <summary>
    /// <para>Stops marking operations as to be processed even if the action gets merged with another in the <see cref="Kaizen.UndoRedo.MergeMode.Ends"/> mode. See <see cref="Kaizen.UndoRedo.StartForceKeepInMergeEnds()"/>.</para>
    /// </summary>
    public void EndForceKeepInMergeEnds()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHistoryCount, 2455072627ul);

    /// <summary>
    /// <para>Returns how many elements are in the history.</para>
    /// </summary>
    public int GetHistoryCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCurrentAction, 2455072627ul);

    /// <summary>
    /// <para>Gets the index of the current action.</para>
    /// </summary>
    public int GetCurrentAction()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind12, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActionName, 990163283ul);

    /// <summary>
    /// <para>Gets the action name from its index.</para>
    /// </summary>
    public string GetActionName(int id)
    {
        string ret = NativeCalls.kaizen_icall_1_158(MethodBind13, KaizenObject.GetPtr(this), id);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearHistory, 3216645846ul);

    /// <summary>
    /// <para>Clear the undo/redo history and associated references.</para>
    /// <para>Passing <see langword="false"/> to <paramref name="increaseVersion"/> will prevent the version number from increasing when the history is cleared.</para>
    /// </summary>
    public void ClearHistory(bool increaseVersion = true)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind14, KaizenObject.GetPtr(this), increaseVersion.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCurrentActionName, 201670096ul);

    /// <summary>
    /// <para>Gets the name of the current action, equivalent to <c>get_action_name(get_current_action())</c>.</para>
    /// </summary>
    public string GetCurrentActionName()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind15, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasUndo, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if an "undo" action is available.</para>
    /// </summary>
    public bool HasUndo()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind16, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasRedo, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if a "redo" action is available.</para>
    /// </summary>
    public bool HasRedo()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind17, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVersion, 3905245786ul);

    /// <summary>
    /// <para>Gets the version. Every time a new action is committed, the <see cref="Kaizen.UndoRedo"/>'s version number is increased automatically.</para>
    /// <para>This is useful mostly to check if something changed from a saved version. The initial version is <c>1</c>.</para>
    /// </summary>
    public ulong GetVersion()
    {
        ulong ret = NativeCalls.kaizen_icall_0_162(MethodBind18, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMaxSteps, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMaxSteps(int maxSteps)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind19, KaizenObject.GetPtr(this), maxSteps);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMaxSteps, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetMaxSteps()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind20, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Redo, 2240911060ul);

    /// <summary>
    /// <para>Redo the last action. Returns <see langword="false"/> if there was no action to redo.</para>
    /// </summary>
    public bool Redo()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind21, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Undo, 2240911060ul);

    /// <summary>
    /// <para>Undo the last action. Returns <see langword="false"/> if there was no action to undo.</para>
    /// </summary>
    public bool Undo()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind22, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Called when <see cref="Kaizen.UndoRedo.Undo()"/> or <see cref="Kaizen.UndoRedo.Redo()"/> was called.</para>
    /// </summary>
    public event Action VersionChanged
    {
        add => Connect(SignalName.VersionChanged, Callable.From(value));
        remove => Disconnect(SignalName.VersionChanged, Callable.From(value));
    }

    protected void EmitSignalVersionChanged()
    {
        EmitSignal(SignalName.VersionChanged);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_version_changed = "VersionChanged";

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
        if (signal == SignalName.VersionChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_version_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : KaizenObject.PropertyName
    {
        /// <summary>
        /// Cached name for the 'max_steps' property.
        /// </summary>
        public static readonly StringName MaxSteps = "max_steps";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
        /// <summary>
        /// Cached name for the 'create_action' method.
        /// </summary>
        public static readonly StringName CreateAction = "create_action";
        /// <summary>
        /// Cached name for the 'commit_action' method.
        /// </summary>
        public static readonly StringName CommitAction = "commit_action";
        /// <summary>
        /// Cached name for the 'is_committing_action' method.
        /// </summary>
        public static readonly StringName IsCommittingAction = "is_committing_action";
        /// <summary>
        /// Cached name for the 'add_do_method' method.
        /// </summary>
        public static readonly StringName AddDoMethod = "add_do_method";
        /// <summary>
        /// Cached name for the 'add_undo_method' method.
        /// </summary>
        public static readonly StringName AddUndoMethod = "add_undo_method";
        /// <summary>
        /// Cached name for the 'add_do_property' method.
        /// </summary>
        public static readonly StringName AddDoProperty = "add_do_property";
        /// <summary>
        /// Cached name for the 'add_undo_property' method.
        /// </summary>
        public static readonly StringName AddUndoProperty = "add_undo_property";
        /// <summary>
        /// Cached name for the 'add_do_reference' method.
        /// </summary>
        public static readonly StringName AddDoReference = "add_do_reference";
        /// <summary>
        /// Cached name for the 'add_undo_reference' method.
        /// </summary>
        public static readonly StringName AddUndoReference = "add_undo_reference";
        /// <summary>
        /// Cached name for the 'start_force_keep_in_merge_ends' method.
        /// </summary>
        public static readonly StringName StartForceKeepInMergeEnds = "start_force_keep_in_merge_ends";
        /// <summary>
        /// Cached name for the 'end_force_keep_in_merge_ends' method.
        /// </summary>
        public static readonly StringName EndForceKeepInMergeEnds = "end_force_keep_in_merge_ends";
        /// <summary>
        /// Cached name for the 'get_history_count' method.
        /// </summary>
        public static readonly StringName GetHistoryCount = "get_history_count";
        /// <summary>
        /// Cached name for the 'get_current_action' method.
        /// </summary>
        public static readonly StringName GetCurrentAction = "get_current_action";
        /// <summary>
        /// Cached name for the 'get_action_name' method.
        /// </summary>
        public static readonly StringName GetActionName = "get_action_name";
        /// <summary>
        /// Cached name for the 'clear_history' method.
        /// </summary>
        public static readonly StringName ClearHistory = "clear_history";
        /// <summary>
        /// Cached name for the 'get_current_action_name' method.
        /// </summary>
        public static readonly StringName GetCurrentActionName = "get_current_action_name";
        /// <summary>
        /// Cached name for the 'has_undo' method.
        /// </summary>
        public static readonly StringName HasUndo = "has_undo";
        /// <summary>
        /// Cached name for the 'has_redo' method.
        /// </summary>
        public static readonly StringName HasRedo = "has_redo";
        /// <summary>
        /// Cached name for the 'get_version' method.
        /// </summary>
        public static readonly StringName GetVersion = "get_version";
        /// <summary>
        /// Cached name for the 'set_max_steps' method.
        /// </summary>
        public static readonly StringName SetMaxSteps = "set_max_steps";
        /// <summary>
        /// Cached name for the 'get_max_steps' method.
        /// </summary>
        public static readonly StringName GetMaxSteps = "get_max_steps";
        /// <summary>
        /// Cached name for the 'redo' method.
        /// </summary>
        public static readonly StringName Redo = "redo";
        /// <summary>
        /// Cached name for the 'undo' method.
        /// </summary>
        public static readonly StringName Undo = "undo";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
        /// <summary>
        /// Cached name for the 'version_changed' signal.
        /// </summary>
        public static readonly StringName VersionChanged = "version_changed";
    }
}
