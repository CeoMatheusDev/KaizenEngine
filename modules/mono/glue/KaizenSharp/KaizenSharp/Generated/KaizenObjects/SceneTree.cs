namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>As one of the most important classes, the <see cref="Kaizen.SceneTree"/> manages the hierarchy of nodes in a scene, as well as scenes themselves. Nodes can be added, fetched and removed. The whole scene tree (and thus the current scene) can be paused. Scenes can be loaded, switched and reloaded.</para>
/// <para>You can also use the <see cref="Kaizen.SceneTree"/> to organize your nodes into <b>groups</b>: every node can be added to as many groups as you want to create, e.g. an "enemy" group. You can then iterate these groups or even call methods and set properties on all the nodes belonging to any given group.</para>
/// <para><see cref="Kaizen.SceneTree"/> is the default <see cref="Kaizen.MainLoop"/> implementation used by the engine, and is thus in charge of the game loop.</para>
/// </summary>
public partial class SceneTree : MainLoop
{
    public enum GroupCallFlags : long
    {
        /// <summary>
        /// <para>Call nodes within a group with no special behavior (default).</para>
        /// </summary>
        Default = 0,
        /// <summary>
        /// <para>Call nodes within a group in reverse tree hierarchy order (all nested children are called before their respective parent nodes).</para>
        /// </summary>
        Reverse = 1,
        /// <summary>
        /// <para>Call nodes within a group at the end of the current frame (can be either process or physics frame), similar to <see cref="Kaizen.KaizenObject.CallDeferred(StringName, Variant[])"/>.</para>
        /// </summary>
        Deferred = 2,
        /// <summary>
        /// <para>Call nodes within a group only once, even if the call is executed many times in the same frame. Must be combined with <see cref="Kaizen.SceneTree.GroupCallFlags.Deferred"/> to work.</para>
        /// <para><b>Note:</b> Different arguments are not taken into account. Therefore, when the same call is executed with different arguments, only the first call will be performed.</para>
        /// </summary>
        Unique = 4
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the application automatically accepts quitting requests.</para>
    /// <para>For mobile platforms, see <see cref="Kaizen.SceneTree.QuitOnGoBack"/>.</para>
    /// </summary>
    public bool AutoAcceptQuit
    {
        get
        {
            return IsAutoAcceptQuit();
        }
        set
        {
            SetAutoAcceptQuit(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the application quits automatically when navigating back (e.g. using the system "Back" button on Android).</para>
    /// <para>To handle 'Go Back' button when this option is disabled, use <see cref="Kaizen.DisplayServer.WindowEvent.GoBackRequest"/>.</para>
    /// </summary>
    public bool QuitOnGoBack
    {
        get
        {
            return IsQuitOnGoBack();
        }
        set
        {
            SetQuitOnGoBack(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, collision shapes will be visible when running the game from the editor for debugging purposes.</para>
    /// </summary>
    public bool DebugCollisionsHint
    {
        get
        {
            return IsDebuggingCollisionsHint();
        }
        set
        {
            SetDebugCollisionsHint(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, curves from <see cref="Kaizen.Path2D"/> and <see cref="Kaizen.Path3D"/> nodes will be visible when running the game from the editor for debugging purposes.</para>
    /// <para><b>Note:</b> This property is not designed to be changed at run-time. Changing the value of <see cref="Kaizen.SceneTree.DebugPathsHint"/> while the project is running will not have the desired effect.</para>
    /// </summary>
    public bool DebugPathsHint
    {
        get
        {
            return IsDebuggingPathsHint();
        }
        set
        {
            SetDebugPathsHint(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, navigation polygons will be visible when running the game from the editor for debugging purposes.</para>
    /// <para><b>Note:</b> This property is not designed to be changed at run-time. Changing the value of <see cref="Kaizen.SceneTree.DebugNavigationHint"/> while the project is running will not have the desired effect.</para>
    /// </summary>
    public bool DebugNavigationHint
    {
        get
        {
            return IsDebuggingNavigationHint();
        }
        set
        {
            SetDebugNavigationHint(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the scene tree is considered paused. This causes the following behavior:</para>
    /// <para>- 2D and 3D physics will be stopped, as well as collision detection and related signals.</para>
    /// <para>- Depending on each node's <see cref="Kaizen.Node.ProcessMode"/>, their <see cref="Kaizen.Node._Process(double)"/>, <see cref="Kaizen.Node._PhysicsProcess(double)"/> and <see cref="Kaizen.Node._Input(InputEvent)"/> callback methods may not called anymore.</para>
    /// </summary>
    public bool Paused
    {
        get
        {
            return IsPaused();
        }
        set
        {
            SetPause(value);
        }
    }

    /// <summary>
    /// <para>The root of the scene currently being edited in the editor. This is usually a direct child of <see cref="Kaizen.SceneTree.Root"/>.</para>
    /// <para><b>Note:</b> This property does nothing in release builds.</para>
    /// </summary>
    public Node EditedSceneRoot
    {
        get
        {
            return GetEditedSceneRoot();
        }
        set
        {
            SetEditedSceneRoot(value);
        }
    }

    /// <summary>
    /// <para>The root node of the currently loaded main scene, usually as a direct child of <see cref="Kaizen.SceneTree.Root"/>. See also <see cref="Kaizen.SceneTree.ChangeSceneToFile(string)"/>, <see cref="Kaizen.SceneTree.ChangeSceneToPacked(PackedScene)"/>, and <see cref="Kaizen.SceneTree.ReloadCurrentScene()"/>.</para>
    /// <para><b>Warning:</b> Setting this property directly may not work as expected, as it does <i>not</i> add or remove any nodes from this tree.</para>
    /// </summary>
    public Node CurrentScene
    {
        get
        {
            return GetCurrentScene();
        }
        set
        {
            SetCurrentScene(value);
        }
    }

    /// <summary>
    /// <para>The tree's root <see cref="Kaizen.Window"/>. This is top-most <see cref="Kaizen.Node"/> of the scene tree, and is always present. An absolute <see cref="Kaizen.NodePath"/> always starts from this node. Children of the root node may include the loaded <see cref="Kaizen.SceneTree.CurrentScene"/>, as well as any <a href="$DOCS_URL/tutorials/scripting/singletons_autoload.html">AutoLoad</a> configured in the Project Settings.</para>
    /// <para><b>Warning:</b> Do not delete this node. This will result in unstable behavior, followed by a crash.</para>
    /// </summary>
    public Window Root
    {
        get
        {
            return GetRoot();
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/> (default value), enables automatic polling of the <see cref="Kaizen.MultiplayerApi"/> for this SceneTree during <see cref="Kaizen.SceneTree.ProcessFrame"/>.</para>
    /// <para>If <see langword="false"/>, you need to manually call <see cref="Kaizen.MultiplayerApi.Poll()"/> to process network packets and deliver RPCs. This allows running RPCs in a different loop (e.g. physics, thread, specific time step) and for manual <see cref="Kaizen.Mutex"/> protection when accessing the <see cref="Kaizen.MultiplayerApi"/> from threads.</para>
    /// </summary>
    public bool MultiplayerPoll
    {
        get
        {
            return IsMultiplayerPollEnabled();
        }
        set
        {
            SetMultiplayerPollEnabled(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the renderer will interpolate the transforms of objects (both physics and non-physics) between the last two transforms, so that smooth motion is seen even when physics ticks do not coincide with rendered frames.</para>
    /// <para>The default value of this property is controlled by <c>ProjectSettings.physics/common/physics_interpolation</c>.</para>
    /// <para><b>Note:</b> Although this is a global setting, finer control of individual branches of the <see cref="Kaizen.SceneTree"/> is possible using <see cref="Kaizen.Node.PhysicsInterpolationMode"/>.</para>
    /// </summary>
    public bool PhysicsInterpolation
    {
        get
        {
            return IsPhysicsInterpolationEnabled();
        }
        set
        {
            SetPhysicsInterpolationEnabled(value);
        }
    }

    private static readonly System.Type CachedType = typeof(SceneTree);

    private static readonly StringName NativeName = "SceneTree";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public SceneTree() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal SceneTree(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal SceneTree(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRoot, 1757182445ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Window GetRoot()
    {
        Window ret = (Window)NativeCalls.kaizen_icall_0_53(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasGroup, 2619796661ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if a node added to the given group <paramref name="name"/> exists in the tree.</para>
    /// </summary>
    public bool HasGroup(StringName name)
    {
        bool ret = NativeCalls.kaizen_icall_1_129(MethodBind1, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAccessibilityEnabled, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if accessibility features are enabled, and accessibility information updates are actively processed.</para>
    /// </summary>
    public bool IsAccessibilityEnabled()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind2, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAccessibilitySupported, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if accessibility features are supported by the OS and enabled in project settings.</para>
    /// </summary>
    public bool IsAccessibilitySupported()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind3, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAutoAcceptQuit, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsAutoAcceptQuit()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind4, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAutoAcceptQuit, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAutoAcceptQuit(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind5, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsQuitOnGoBack, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsQuitOnGoBack()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind6, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetQuitOnGoBack, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetQuitOnGoBack(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind7, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDebugCollisionsHint, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDebugCollisionsHint(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind8, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDebuggingCollisionsHint, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDebuggingCollisionsHint()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind9, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDebugPathsHint, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDebugPathsHint(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind10, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDebuggingPathsHint, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDebuggingPathsHint()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind11, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDebugNavigationHint, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetDebugNavigationHint(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind12, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsDebuggingNavigationHint, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsDebuggingNavigationHint()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind13, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEditedSceneRoot, 1078189570ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetEditedSceneRoot(Node scene)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind14, KaizenObject.GetPtr(this), KaizenObject.GetPtr(scene));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetEditedSceneRoot, 3160264692ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Node GetEditedSceneRoot()
    {
        Node ret = (Node)NativeCalls.kaizen_icall_0_53(MethodBind15, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPause, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPause(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind16, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsPaused, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsPaused()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind17, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateTimer, 2709170273ul);

    /// <summary>
    /// <para>Returns a new <see cref="Kaizen.SceneTreeTimer"/>. After <paramref name="timeSec"/> in seconds have passed, the timer will emit <see cref="Kaizen.SceneTreeTimer.Timeout"/> and will be automatically freed.</para>
    /// <para>If <paramref name="processAlways"/> is <see langword="false"/>, the timer will be paused when setting <see cref="Kaizen.SceneTree.Paused"/> to <see langword="true"/>.</para>
    /// <para>If <paramref name="processInPhysics"/> is <see langword="true"/>, the timer will update at the end of the physics frame, instead of the process frame.</para>
    /// <para>If <paramref name="ignoreTimeScale"/> is <see langword="true"/>, the timer will ignore <see cref="Kaizen.Engine.TimeScale"/> and update with the real, elapsed time.</para>
    /// <para>This method is commonly used to create a one-shot delay timer, as in the following example:</para>
    /// <para><code>
    /// public async Task SomeFunction()
    /// {
    /// 	GD.Print("start");
    /// 	await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
    /// 	GD.Print("end");
    /// }
    /// </code></para>
    /// <para><b>Note:</b> The timer is always updated <i>after</i> all of the nodes in the tree. A node's <see cref="Kaizen.Node._Process(double)"/> method would be called before the timer updates (or <see cref="Kaizen.Node._PhysicsProcess(double)"/> if <paramref name="processInPhysics"/> is set to <see langword="true"/>).</para>
    /// </summary>
    public SceneTreeTimer CreateTimer(double timeSec, bool processAlways = true, bool processInPhysics = false, bool ignoreTimeScale = false)
    {
        SceneTreeTimer ret = (SceneTreeTimer)NativeCalls.kaizen_icall_4_1315(MethodBind18, KaizenObject.GetPtr(this), timeSec, processAlways.ToKaizenBool(), processInPhysics.ToKaizenBool(), ignoreTimeScale.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateTween, 3426978995ul);

    /// <summary>
    /// <para>Creates and returns a new <see cref="Kaizen.Tween"/> processed in this tree. The Tween will start automatically on the next process frame or physics frame (depending on its <see cref="Kaizen.Tween.TweenProcessMode"/>).</para>
    /// <para><b>Note:</b> A <see cref="Kaizen.Tween"/> created using this method is not bound to any <see cref="Kaizen.Node"/>. It may keep working until there is nothing left to animate. If you want the <see cref="Kaizen.Tween"/> to be automatically killed when the <see cref="Kaizen.Node"/> is freed, use <see cref="Kaizen.Node.CreateTween()"/> or <see cref="Kaizen.Tween.BindNode(Node)"/>.</para>
    /// </summary>
    public Tween CreateTween()
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_0_87(MethodBind19, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetProcessedTweens, 2915620761ul);

    /// <summary>
    /// <para>Returns an <see cref="Kaizen.Collections.Array"/> of currently existing <see cref="Kaizen.Tween"/>s in the tree, including paused tweens.</para>
    /// </summary>
    public Kaizen.Collections.Array<Tween> GetProcessedTweens()
    {
        Kaizen.Collections.Array<Tween> ret = new Kaizen.Collections.Array<Tween>(NativeCalls.kaizen_icall_0_146(MethodBind20, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNodeCount, 3905245786ul);

    /// <summary>
    /// <para>Returns the number of nodes inside this tree.</para>
    /// </summary>
    public int GetNodeCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind21, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFrame, 3905245786ul);

    /// <summary>
    /// <para>Returns how many physics process steps have been processed, since the application started. This is <i>not</i> a measurement of elapsed time. See also <see cref="Kaizen.SceneTree.PhysicsFrame"/>. For the number of frames rendered, see <see cref="Kaizen.Engine.GetProcessFrames()"/>.</para>
    /// </summary>
    public long GetFrame()
    {
        long ret = NativeCalls.kaizen_icall_0_4(MethodBind22, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Quit, 1995695955ul);

    /// <summary>
    /// <para>Quits the application at the end of the current iteration, with the given <paramref name="exitCode"/>.</para>
    /// <para>By convention, an exit code of <c>0</c> indicates success, whereas any other exit code indicates an error. For portability reasons, it should be between <c>0</c> and <c>125</c> (inclusive).</para>
    /// <para><b>Note:</b> On iOS this method doesn't work. Instead, as recommended by the <a href="https://developer.apple.com/library/archive/qa/qa1561/_index.html">iOS Human Interface Guidelines</a>, the user is expected to close apps via the Home button.</para>
    /// </summary>
    public void Quit(int exitCode = 0)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind23, KaizenObject.GetPtr(this), exitCode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPhysicsInterpolationEnabled, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPhysicsInterpolationEnabled(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind24, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsPhysicsInterpolationEnabled, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsPhysicsInterpolationEnabled()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind25, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.QueueDelete, 3975164845ul);

    /// <summary>
    /// <para>Queues the given <paramref name="obj"/> to be deleted, calling its <see cref="Kaizen.KaizenObject.Free()"/> at the end of the current frame. This method is similar to <see cref="Kaizen.Node.QueueFree()"/>.</para>
    /// </summary>
    public void QueueDelete(KaizenObject obj)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind26, KaizenObject.GetPtr(this), KaizenObject.GetPtr(obj));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CallGroupFlags, 1527739229ul);

    /// <summary>
    /// <para>Calls the given <paramref name="method"/> on each node inside this tree added to the given <paramref name="group"/>. Use <paramref name="flags"/> to customize this method's behavior (see <see cref="Kaizen.SceneTree.GroupCallFlags"/>). Additional arguments for <paramref name="method"/> can be passed at the end of this method. Nodes that cannot call <paramref name="method"/> (either because the method doesn't exist or the arguments do not match) are ignored.</para>
    /// <para><code>
    /// # Calls "hide" to all nodes of the "enemies" group, at the end of the frame and in reverse tree order.
    /// get_tree().call_group_flags(
    /// 		SceneTree.GROUP_CALL_DEFERRED | SceneTree.GROUP_CALL_REVERSE,
    /// 		"enemies", "hide")
    /// </code></para>
    /// <para><b>Note:</b> In C#, <paramref name="method"/> must be in snake_case when referring to built-in Kaizen methods. Prefer using the names exposed in the <c>MethodName</c> class to avoid allocating a new <see cref="Kaizen.StringName"/> on each call.</para>
    /// </summary>
    public void CallGroupFlags(long flags, StringName group, StringName method, params Variant[] @args)
    {
        NativeCalls.kaizen_icall_4_1316(MethodBind27, KaizenObject.GetPtr(this), flags, (kaizen_string_name)(group?.NativeValue ?? default), (kaizen_string_name)(method?.NativeValue ?? default), @args, (kaizen_string_name)MethodName.CallGroupFlags.NativeValue);
        global::System.GC.KeepAlive(this);
    }

    /// <summary>
    /// <para>Calls the given <paramref name="method"/> on each node inside this tree added to the given <paramref name="group"/>. Use <paramref name="flags"/> to customize this method's behavior (see <see cref="Kaizen.SceneTree.GroupCallFlags"/>). Additional arguments for <paramref name="method"/> can be passed at the end of this method. Nodes that cannot call <paramref name="method"/> (either because the method doesn't exist or the arguments do not match) are ignored.</para>
    /// <para><code>
    /// # Calls "hide" to all nodes of the "enemies" group, at the end of the frame and in reverse tree order.
    /// get_tree().call_group_flags(
    /// 		SceneTree.GROUP_CALL_DEFERRED | SceneTree.GROUP_CALL_REVERSE,
    /// 		"enemies", "hide")
    /// </code></para>
    /// <para><b>Note:</b> In C#, <paramref name="method"/> must be in snake_case when referring to built-in Kaizen methods. Prefer using the names exposed in the <c>MethodName</c> class to avoid allocating a new <see cref="Kaizen.StringName"/> on each call.</para>
    /// </summary>
    public void CallGroupFlags(long flags, StringName group, StringName method, ReadOnlySpan<Variant> @args)
    {
        NativeCalls.kaizen_icall_4_1316(MethodBind27, KaizenObject.GetPtr(this), flags, (kaizen_string_name)(group?.NativeValue ?? default), (kaizen_string_name)(method?.NativeValue ?? default), @args, (kaizen_string_name)MethodName.CallGroupFlags.NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.NotifyGroupFlags, 1245489420ul);

    /// <summary>
    /// <para>Calls <see cref="Kaizen.KaizenObject.Notification(int, bool)"/> with the given <paramref name="notification"/> to all nodes inside this tree added to the <paramref name="group"/>. Use <paramref name="callFlags"/> to customize this method's behavior (see <see cref="Kaizen.SceneTree.GroupCallFlags"/>).</para>
    /// </summary>
    public void NotifyGroupFlags(uint callFlags, StringName group, int notification)
    {
        NativeCalls.kaizen_icall_3_1317(MethodBind28, KaizenObject.GetPtr(this), callFlags, (kaizen_string_name)(group?.NativeValue ?? default), notification);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetGroupFlags, 3497599527ul);

    /// <summary>
    /// <para>Sets the given <paramref name="property"/> to <paramref name="value"/> on all nodes inside this tree added to the given <paramref name="group"/>. Nodes that do not have the <paramref name="property"/> are ignored. Use <paramref name="callFlags"/> to customize this method's behavior (see <see cref="Kaizen.SceneTree.GroupCallFlags"/>).</para>
    /// <para><b>Note:</b> In C#, <paramref name="property"/> must be in snake_case when referring to built-in Kaizen properties. Prefer using the names exposed in the <c>PropertyName</c> class to avoid allocating a new <see cref="Kaizen.StringName"/> on each call.</para>
    /// </summary>
    public void SetGroupFlags(uint callFlags, StringName group, string property, Variant value)
    {
        NativeCalls.kaizen_icall_4_1318(MethodBind29, KaizenObject.GetPtr(this), callFlags, (kaizen_string_name)(group?.NativeValue ?? default), property, value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CallGroup, 1257962832ul);

    /// <summary>
    /// <para>Calls <paramref name="method"/> on each node inside this tree added to the given <paramref name="group"/>. You can pass arguments to <paramref name="method"/> by specifying them at the end of this method call. Nodes that cannot call <paramref name="method"/> (either because the method doesn't exist or the arguments do not match) are ignored. See also <see cref="Kaizen.SceneTree.SetGroup(StringName, string, Variant)"/> and <see cref="Kaizen.SceneTree.NotifyGroup(StringName, int)"/>.</para>
    /// <para><b>Note:</b> This method acts immediately on all selected nodes at once, which may cause stuttering in some performance-intensive situations.</para>
    /// <para><b>Note:</b> In C#, <paramref name="method"/> must be in snake_case when referring to built-in Kaizen methods. Prefer using the names exposed in the <c>MethodName</c> class to avoid allocating a new <see cref="Kaizen.StringName"/> on each call.</para>
    /// </summary>
    public void CallGroup(StringName group, StringName method, params Variant[] @args)
    {
        NativeCalls.kaizen_icall_3_1319(MethodBind30, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default), (kaizen_string_name)(method?.NativeValue ?? default), @args, (kaizen_string_name)MethodName.CallGroup.NativeValue);
        global::System.GC.KeepAlive(this);
    }

    /// <summary>
    /// <para>Calls <paramref name="method"/> on each node inside this tree added to the given <paramref name="group"/>. You can pass arguments to <paramref name="method"/> by specifying them at the end of this method call. Nodes that cannot call <paramref name="method"/> (either because the method doesn't exist or the arguments do not match) are ignored. See also <see cref="Kaizen.SceneTree.SetGroup(StringName, string, Variant)"/> and <see cref="Kaizen.SceneTree.NotifyGroup(StringName, int)"/>.</para>
    /// <para><b>Note:</b> This method acts immediately on all selected nodes at once, which may cause stuttering in some performance-intensive situations.</para>
    /// <para><b>Note:</b> In C#, <paramref name="method"/> must be in snake_case when referring to built-in Kaizen methods. Prefer using the names exposed in the <c>MethodName</c> class to avoid allocating a new <see cref="Kaizen.StringName"/> on each call.</para>
    /// </summary>
    public void CallGroup(StringName group, StringName method, ReadOnlySpan<Variant> @args)
    {
        NativeCalls.kaizen_icall_3_1319(MethodBind30, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default), (kaizen_string_name)(method?.NativeValue ?? default), @args, (kaizen_string_name)MethodName.CallGroup.NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.NotifyGroup, 2415702435ul);

    /// <summary>
    /// <para>Calls <see cref="Kaizen.KaizenObject.Notification(int, bool)"/> with the given <paramref name="notification"/> to all nodes inside this tree added to the <paramref name="group"/>. See also <a href="$DOCS_URL/tutorials/best_practices/kaizen_notifications.html">Kaizen notifications</a> and <see cref="Kaizen.SceneTree.CallGroup(StringName, StringName, Variant[])"/> and <see cref="Kaizen.SceneTree.SetGroup(StringName, string, Variant)"/>.</para>
    /// <para><b>Note:</b> This method acts immediately on all selected nodes at once, which may cause stuttering in some performance-intensive situations.</para>
    /// </summary>
    public void NotifyGroup(StringName group, int notification)
    {
        NativeCalls.kaizen_icall_2_181(MethodBind31, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default), notification);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetGroup, 1279312029ul);

    /// <summary>
    /// <para>Sets the given <paramref name="property"/> to <paramref name="value"/> on all nodes inside this tree added to the given <paramref name="group"/>. Nodes that do not have the <paramref name="property"/> are ignored. See also <see cref="Kaizen.SceneTree.CallGroup(StringName, StringName, Variant[])"/> and <see cref="Kaizen.SceneTree.NotifyGroup(StringName, int)"/>.</para>
    /// <para><b>Note:</b> This method acts immediately on all selected nodes at once, which may cause stuttering in some performance-intensive situations.</para>
    /// <para><b>Note:</b> In C#, <paramref name="property"/> must be in snake_case when referring to built-in Kaizen properties. Prefer using the names exposed in the <c>PropertyName</c> class to avoid allocating a new <see cref="Kaizen.StringName"/> on each call.</para>
    /// </summary>
    public void SetGroup(StringName group, string property, Variant value)
    {
        NativeCalls.kaizen_icall_3_1320(MethodBind32, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default), property, value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNodesInGroup, 689397652ul);

    /// <summary>
    /// <para>Returns an <see cref="Kaizen.Collections.Array"/> containing all nodes inside this tree, that have been added to the given <paramref name="group"/>, in scene hierarchy order.</para>
    /// </summary>
    public Kaizen.Collections.Array<Node> GetNodesInGroup(StringName group)
    {
        Kaizen.Collections.Array<Node> ret = new Kaizen.Collections.Array<Node>(NativeCalls.kaizen_icall_1_726(MethodBind33, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind34 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFirstNodeInGroup, 4071044623ul);

    /// <summary>
    /// <para>Returns the first <see cref="Kaizen.Node"/> found inside the tree, that has been added to the given <paramref name="group"/>, in scene hierarchy order. Returns <see langword="null"/> if no match is found. See also <see cref="Kaizen.SceneTree.GetNodesInGroup(StringName)"/>.</para>
    /// </summary>
    public Node GetFirstNodeInGroup(StringName group)
    {
        Node ret = (Node)NativeCalls.kaizen_icall_1_563(MethodBind34, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind35 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNodeCountInGroup, 2458036349ul);

    /// <summary>
    /// <para>Returns the number of nodes assigned to the given group.</para>
    /// </summary>
    public int GetNodeCountInGroup(StringName group)
    {
        int ret = NativeCalls.kaizen_icall_1_172(MethodBind35, KaizenObject.GetPtr(this), (kaizen_string_name)(group?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind36 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCurrentScene, 1078189570ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCurrentScene(Node childNode)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind36, KaizenObject.GetPtr(this), KaizenObject.GetPtr(childNode));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind37 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCurrentScene, 3160264692ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Node GetCurrentScene()
    {
        Node ret = (Node)NativeCalls.kaizen_icall_0_53(MethodBind37, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind38 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ChangeSceneToFile, 166001499ul);

    /// <summary>
    /// <para>Changes the running scene to the one at the given <paramref name="path"/>, after loading it into a <see cref="Kaizen.PackedScene"/> and creating a new instance.</para>
    /// <para>Returns <see cref="Kaizen.Error.Ok"/> on success, <see cref="Kaizen.Error.CantOpen"/> if the <paramref name="path"/> cannot be loaded into a <see cref="Kaizen.PackedScene"/>, or <see cref="Kaizen.Error.CantCreate"/> if that scene cannot be instantiated.</para>
    /// <para><b>Note:</b> See <see cref="Kaizen.SceneTree.ChangeSceneToNode(Node)"/> for details on the order of operations.</para>
    /// </summary>
    public Error ChangeSceneToFile(string path)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_159(MethodBind38, KaizenObject.GetPtr(this), path);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind39 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ChangeSceneToPacked, 107349098ul);

    /// <summary>
    /// <para>Changes the running scene to a new instance of the given <see cref="Kaizen.PackedScene"/> (which must be valid).</para>
    /// <para>Returns <see cref="Kaizen.Error.Ok"/> on success, <see cref="Kaizen.Error.CantCreate"/> if the scene cannot be instantiated, or <see cref="Kaizen.Error.InvalidParameter"/> if the scene is invalid.</para>
    /// <para><b>Note:</b> See <see cref="Kaizen.SceneTree.ChangeSceneToNode(Node)"/> for details on the order of operations.</para>
    /// </summary>
    public Error ChangeSceneToPacked(PackedScene packedScene)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_399(MethodBind39, KaizenObject.GetPtr(this), KaizenObject.GetPtr(packedScene));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind40 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ChangeSceneToNode, 2584678054ul);

    /// <summary>
    /// <para>Changes the running scene to the provided <see cref="Kaizen.Node"/>. Useful when you want to set up the new scene before changing.</para>
    /// <para>Returns <see cref="Kaizen.Error.Ok"/> on success, <see cref="Kaizen.Error.InvalidParameter"/> if the <paramref name="node"/> is <see langword="null"/>, or <see cref="Kaizen.Error.Unconfigured"/> if the <paramref name="node"/> is already inside the scene tree.</para>
    /// <para><b>Note:</b> Operations happen in the following order when <see cref="Kaizen.SceneTree.ChangeSceneToNode(Node)"/> is called:</para>
    /// <para>1. The current scene node is immediately removed from the tree. From that point, <see cref="Kaizen.Node.GetTree()"/> called on the current (outgoing) scene will return <see langword="null"/>. <see cref="Kaizen.SceneTree.CurrentScene"/> will be <see langword="null"/> too, because the new scene is not available yet.</para>
    /// <para>2. At the end of the frame, the formerly current scene, already removed from the tree, will be deleted (freed from memory) and then the new scene node will be added to the tree. <see cref="Kaizen.Node.GetTree()"/> and <see cref="Kaizen.SceneTree.CurrentScene"/> will be back to working as usual.</para>
    /// <para>This ensures that both scenes aren't running at the same time, while still freeing the previous scene in a safe way similar to <see cref="Kaizen.Node.QueueFree()"/>.</para>
    /// <para>If you want to reliably access the new scene, await the <see cref="Kaizen.SceneTree.SceneChanged"/> signal.</para>
    /// <para><b>Warning:</b> After using this method, the <see cref="Kaizen.SceneTree"/> will take ownership of the node and will free it automatically when changing scene again. Any references you had to that node will become invalid.</para>
    /// </summary>
    public Error ChangeSceneToNode(Node node)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_399(MethodBind40, KaizenObject.GetPtr(this), KaizenObject.GetPtr(node));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind41 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ReloadCurrentScene, 166280745ul);

    /// <summary>
    /// <para>Reloads the currently active scene, replacing <see cref="Kaizen.SceneTree.CurrentScene"/> with a new instance of its original <see cref="Kaizen.PackedScene"/>.</para>
    /// <para>Returns <see cref="Kaizen.Error.Ok"/> on success, <see cref="Kaizen.Error.Unconfigured"/> if no <see cref="Kaizen.SceneTree.CurrentScene"/> is defined, <see cref="Kaizen.Error.CantOpen"/> if <see cref="Kaizen.SceneTree.CurrentScene"/> cannot be loaded into a <see cref="Kaizen.PackedScene"/>, or <see cref="Kaizen.Error.CantCreate"/> if the scene cannot be instantiated.</para>
    /// </summary>
    public Error ReloadCurrentScene()
    {
        Error ret = (Error)NativeCalls.kaizen_icall_0_39(MethodBind41, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind42 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UnloadCurrentScene, 3218959716ul);

    /// <summary>
    /// <para>If a current scene is loaded, calling this method will unload it.</para>
    /// </summary>
    public void UnloadCurrentScene()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind42, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind43 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMultiplayer, 2385607013ul);

    /// <summary>
    /// <para>Sets a custom <see cref="Kaizen.MultiplayerApi"/> with the given <paramref name="rootPath"/> (controlling also the relative subpaths), or override the default one if <paramref name="rootPath"/> is empty.</para>
    /// <para><b>Note:</b> No <see cref="Kaizen.MultiplayerApi"/> must be configured for the subpath containing <paramref name="rootPath"/>, nested custom multiplayers are not allowed. I.e. if one is configured for <c>"/root/Foo"</c> setting one for <c>"/root/Foo/Bar"</c> will cause an error.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.SceneTree.SetMultiplayer(MultiplayerApi, NodePath)"/> should be called <i>before</i> the child nodes are ready at the given <paramref name="rootPath"/>. If multiplayer nodes like <see cref="Kaizen.MultiplayerSpawner"/> or <see cref="Kaizen.MultiplayerSynchronizer"/> are added to the tree before the custom multiplayer API is set, they will not work.</para>
    /// </summary>
    public void SetMultiplayer(MultiplayerApi multiplayer, NodePath rootPath = null)
    {
        NativeCalls.kaizen_icall_2_1321(MethodBind43, KaizenObject.GetPtr(this), KaizenObject.GetPtr(multiplayer), (kaizen_node_path)(rootPath?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind44 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMultiplayer, 3453401404ul);

    /// <summary>
    /// <para>Searches for the <see cref="Kaizen.MultiplayerApi"/> configured for the given path, if one does not exist it searches the parent paths until one is found. If the path is empty, or none is found, the default one is returned. See <see cref="Kaizen.SceneTree.SetMultiplayer(MultiplayerApi, NodePath)"/>.</para>
    /// </summary>
    public MultiplayerApi GetMultiplayer(NodePath forPath = null)
    {
        MultiplayerApi ret = (MultiplayerApi)NativeCalls.kaizen_icall_1_1322(MethodBind44, KaizenObject.GetPtr(this), (kaizen_node_path)(forPath?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind45 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMultiplayerPollEnabled, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMultiplayerPollEnabled(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind45, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind46 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsMultiplayerPollEnabled, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsMultiplayerPollEnabled()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind46, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Emitted any time the tree's hierarchy changes (nodes being moved, renamed, etc.).</para>
    /// </summary>
    public event Action TreeChanged
    {
        add => Connect(SignalName.TreeChanged, Callable.From(value));
        remove => Disconnect(SignalName.TreeChanged, Callable.From(value));
    }

    protected void EmitSignalTreeChanged()
    {
        EmitSignal(SignalName.TreeChanged);
    }

    /// <summary>
    /// <para>Emitted after the new scene is added to scene tree and initialized. Can be used to reliably access <see cref="Kaizen.SceneTree.CurrentScene"/> when changing scenes.</para>
    /// <para><code>
    /// # This code should be inside an autoload.
    /// get_tree().change_scene_to_file(other_scene_path)
    /// await get_tree().scene_changed
    /// print(get_tree().current_scene) # Prints the new scene.
    /// </code></para>
    /// </summary>
    public event Action SceneChanged
    {
        add => Connect(SignalName.SceneChanged, Callable.From(value));
        remove => Disconnect(SignalName.SceneChanged, Callable.From(value));
    }

    protected void EmitSignalSceneChanged()
    {
        EmitSignal(SignalName.SceneChanged);
    }

    /// <summary>
    /// <para>Emitted when the <see cref="Kaizen.Node.ProcessMode"/> of any node inside the tree is changed. Only emitted in the editor, to update the visibility of disabled nodes.</para>
    /// </summary>
    public event Action TreeProcessModeChanged
    {
        add => Connect(SignalName.TreeProcessModeChanged, Callable.From(value));
        remove => Disconnect(SignalName.TreeProcessModeChanged, Callable.From(value));
    }

    protected void EmitSignalTreeProcessModeChanged()
    {
        EmitSignal(SignalName.TreeProcessModeChanged);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.SceneTree.NodeAdded"/> event of a <see cref="Kaizen.SceneTree"/> class.
    /// </summary>
    public delegate void NodeAddedEventHandler(Node node);

    private static void NodeAddedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((NodeAddedEventHandler)delegateObj)(VariantUtils.ConvertTo<Node>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when the <c>node</c> enters this tree.</para>
    /// </summary>
    public unsafe event NodeAddedEventHandler NodeAdded
    {
        add => Connect(SignalName.NodeAdded, Callable.CreateWithUnsafeTrampoline(value, &NodeAddedTrampoline));
        remove => Disconnect(SignalName.NodeAdded, Callable.CreateWithUnsafeTrampoline(value, &NodeAddedTrampoline));
    }

    protected void EmitSignalNodeAdded(Node node)
    {
        EmitSignal(SignalName.NodeAdded, node);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.SceneTree.NodeRemoved"/> event of a <see cref="Kaizen.SceneTree"/> class.
    /// </summary>
    public delegate void NodeRemovedEventHandler(Node node);

    private static void NodeRemovedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((NodeRemovedEventHandler)delegateObj)(VariantUtils.ConvertTo<Node>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when the <c>node</c> exits this tree.</para>
    /// </summary>
    public unsafe event NodeRemovedEventHandler NodeRemoved
    {
        add => Connect(SignalName.NodeRemoved, Callable.CreateWithUnsafeTrampoline(value, &NodeRemovedTrampoline));
        remove => Disconnect(SignalName.NodeRemoved, Callable.CreateWithUnsafeTrampoline(value, &NodeRemovedTrampoline));
    }

    protected void EmitSignalNodeRemoved(Node node)
    {
        EmitSignal(SignalName.NodeRemoved, node);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.SceneTree.NodeRenamed"/> event of a <see cref="Kaizen.SceneTree"/> class.
    /// </summary>
    public delegate void NodeRenamedEventHandler(Node node);

    private static void NodeRenamedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((NodeRenamedEventHandler)delegateObj)(VariantUtils.ConvertTo<Node>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when the <c>node</c>'s <see cref="Kaizen.Node.Name"/> is changed.</para>
    /// </summary>
    public unsafe event NodeRenamedEventHandler NodeRenamed
    {
        add => Connect(SignalName.NodeRenamed, Callable.CreateWithUnsafeTrampoline(value, &NodeRenamedTrampoline));
        remove => Disconnect(SignalName.NodeRenamed, Callable.CreateWithUnsafeTrampoline(value, &NodeRenamedTrampoline));
    }

    protected void EmitSignalNodeRenamed(Node node)
    {
        EmitSignal(SignalName.NodeRenamed, node);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.SceneTree.NodeConfigurationWarningChanged"/> event of a <see cref="Kaizen.SceneTree"/> class.
    /// </summary>
    public delegate void NodeConfigurationWarningChangedEventHandler(Node node);

    private static void NodeConfigurationWarningChangedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((NodeConfigurationWarningChangedEventHandler)delegateObj)(VariantUtils.ConvertTo<Node>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when the <c>node</c>'s <see cref="Kaizen.Node.UpdateConfigurationWarnings()"/> is called. Only emitted in the editor.</para>
    /// </summary>
    public unsafe event NodeConfigurationWarningChangedEventHandler NodeConfigurationWarningChanged
    {
        add => Connect(SignalName.NodeConfigurationWarningChanged, Callable.CreateWithUnsafeTrampoline(value, &NodeConfigurationWarningChangedTrampoline));
        remove => Disconnect(SignalName.NodeConfigurationWarningChanged, Callable.CreateWithUnsafeTrampoline(value, &NodeConfigurationWarningChangedTrampoline));
    }

    protected void EmitSignalNodeConfigurationWarningChanged(Node node)
    {
        EmitSignal(SignalName.NodeConfigurationWarningChanged, node);
    }

    /// <summary>
    /// <para>Emitted immediately before <see cref="Kaizen.Node._Process(double)"/> is called on every node in this tree.</para>
    /// </summary>
    public event Action ProcessFrame
    {
        add => Connect(SignalName.ProcessFrame, Callable.From(value));
        remove => Disconnect(SignalName.ProcessFrame, Callable.From(value));
    }

    protected void EmitSignalProcessFrame()
    {
        EmitSignal(SignalName.ProcessFrame);
    }

    /// <summary>
    /// <para>Emitted immediately before <see cref="Kaizen.Node._PhysicsProcess(double)"/> is called on every node in this tree.</para>
    /// </summary>
    public event Action PhysicsFrame
    {
        add => Connect(SignalName.PhysicsFrame, Callable.From(value));
        remove => Disconnect(SignalName.PhysicsFrame, Callable.From(value));
    }

    protected void EmitSignalPhysicsFrame()
    {
        EmitSignal(SignalName.PhysicsFrame);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_tree_changed = "TreeChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_scene_changed = "SceneChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_tree_process_mode_changed = "TreeProcessModeChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_node_added = "NodeAdded";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_node_removed = "NodeRemoved";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_node_renamed = "NodeRenamed";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_node_configuration_warning_changed = "NodeConfigurationWarningChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_process_frame = "ProcessFrame";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_physics_frame = "PhysicsFrame";

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
        if (signal == SignalName.TreeChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_tree_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.SceneChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_scene_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.TreeProcessModeChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_tree_process_mode_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.NodeAdded)
        {
            if (HasKaizenClassSignal(SignalProxyName_node_added.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.NodeRemoved)
        {
            if (HasKaizenClassSignal(SignalProxyName_node_removed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.NodeRenamed)
        {
            if (HasKaizenClassSignal(SignalProxyName_node_renamed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.NodeConfigurationWarningChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_node_configuration_warning_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.ProcessFrame)
        {
            if (HasKaizenClassSignal(SignalProxyName_process_frame.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.PhysicsFrame)
        {
            if (HasKaizenClassSignal(SignalProxyName_physics_frame.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : MainLoop.PropertyName
    {
        /// <summary>
        /// Cached name for the 'auto_accept_quit' property.
        /// </summary>
        public static readonly StringName AutoAcceptQuit = "auto_accept_quit";
        /// <summary>
        /// Cached name for the 'quit_on_go_back' property.
        /// </summary>
        public static readonly StringName QuitOnGoBack = "quit_on_go_back";
        /// <summary>
        /// Cached name for the 'debug_collisions_hint' property.
        /// </summary>
        public static readonly StringName DebugCollisionsHint = "debug_collisions_hint";
        /// <summary>
        /// Cached name for the 'debug_paths_hint' property.
        /// </summary>
        public static readonly StringName DebugPathsHint = "debug_paths_hint";
        /// <summary>
        /// Cached name for the 'debug_navigation_hint' property.
        /// </summary>
        public static readonly StringName DebugNavigationHint = "debug_navigation_hint";
        /// <summary>
        /// Cached name for the 'paused' property.
        /// </summary>
        public static readonly StringName Paused = "paused";
        /// <summary>
        /// Cached name for the 'edited_scene_root' property.
        /// </summary>
        public static readonly StringName EditedSceneRoot = "edited_scene_root";
        /// <summary>
        /// Cached name for the 'current_scene' property.
        /// </summary>
        public static readonly StringName CurrentScene = "current_scene";
        /// <summary>
        /// Cached name for the 'root' property.
        /// </summary>
        public static readonly StringName Root = "root";
        /// <summary>
        /// Cached name for the 'multiplayer_poll' property.
        /// </summary>
        public static readonly StringName MultiplayerPoll = "multiplayer_poll";
        /// <summary>
        /// Cached name for the 'physics_interpolation' property.
        /// </summary>
        public static readonly StringName PhysicsInterpolation = "physics_interpolation";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : MainLoop.MethodName
    {
        /// <summary>
        /// Cached name for the 'get_root' method.
        /// </summary>
        public static readonly StringName GetRoot = "get_root";
        /// <summary>
        /// Cached name for the 'has_group' method.
        /// </summary>
        public static readonly StringName HasGroup = "has_group";
        /// <summary>
        /// Cached name for the 'is_accessibility_enabled' method.
        /// </summary>
        public static readonly StringName IsAccessibilityEnabled = "is_accessibility_enabled";
        /// <summary>
        /// Cached name for the 'is_accessibility_supported' method.
        /// </summary>
        public static readonly StringName IsAccessibilitySupported = "is_accessibility_supported";
        /// <summary>
        /// Cached name for the 'is_auto_accept_quit' method.
        /// </summary>
        public static readonly StringName IsAutoAcceptQuit = "is_auto_accept_quit";
        /// <summary>
        /// Cached name for the 'set_auto_accept_quit' method.
        /// </summary>
        public static readonly StringName SetAutoAcceptQuit = "set_auto_accept_quit";
        /// <summary>
        /// Cached name for the 'is_quit_on_go_back' method.
        /// </summary>
        public static readonly StringName IsQuitOnGoBack = "is_quit_on_go_back";
        /// <summary>
        /// Cached name for the 'set_quit_on_go_back' method.
        /// </summary>
        public static readonly StringName SetQuitOnGoBack = "set_quit_on_go_back";
        /// <summary>
        /// Cached name for the 'set_debug_collisions_hint' method.
        /// </summary>
        public static readonly StringName SetDebugCollisionsHint = "set_debug_collisions_hint";
        /// <summary>
        /// Cached name for the 'is_debugging_collisions_hint' method.
        /// </summary>
        public static readonly StringName IsDebuggingCollisionsHint = "is_debugging_collisions_hint";
        /// <summary>
        /// Cached name for the 'set_debug_paths_hint' method.
        /// </summary>
        public static readonly StringName SetDebugPathsHint = "set_debug_paths_hint";
        /// <summary>
        /// Cached name for the 'is_debugging_paths_hint' method.
        /// </summary>
        public static readonly StringName IsDebuggingPathsHint = "is_debugging_paths_hint";
        /// <summary>
        /// Cached name for the 'set_debug_navigation_hint' method.
        /// </summary>
        public static readonly StringName SetDebugNavigationHint = "set_debug_navigation_hint";
        /// <summary>
        /// Cached name for the 'is_debugging_navigation_hint' method.
        /// </summary>
        public static readonly StringName IsDebuggingNavigationHint = "is_debugging_navigation_hint";
        /// <summary>
        /// Cached name for the 'set_edited_scene_root' method.
        /// </summary>
        public static readonly StringName SetEditedSceneRoot = "set_edited_scene_root";
        /// <summary>
        /// Cached name for the 'get_edited_scene_root' method.
        /// </summary>
        public static readonly StringName GetEditedSceneRoot = "get_edited_scene_root";
        /// <summary>
        /// Cached name for the 'set_pause' method.
        /// </summary>
        public static readonly StringName SetPause = "set_pause";
        /// <summary>
        /// Cached name for the 'is_paused' method.
        /// </summary>
        public static readonly StringName IsPaused = "is_paused";
        /// <summary>
        /// Cached name for the 'create_timer' method.
        /// </summary>
        public static readonly StringName CreateTimer = "create_timer";
        /// <summary>
        /// Cached name for the 'create_tween' method.
        /// </summary>
        public static readonly StringName CreateTween = "create_tween";
        /// <summary>
        /// Cached name for the 'get_processed_tweens' method.
        /// </summary>
        public static readonly StringName GetProcessedTweens = "get_processed_tweens";
        /// <summary>
        /// Cached name for the 'get_node_count' method.
        /// </summary>
        public static readonly StringName GetNodeCount = "get_node_count";
        /// <summary>
        /// Cached name for the 'get_frame' method.
        /// </summary>
        public static readonly StringName GetFrame = "get_frame";
        /// <summary>
        /// Cached name for the 'quit' method.
        /// </summary>
        public static readonly StringName Quit = "quit";
        /// <summary>
        /// Cached name for the 'set_physics_interpolation_enabled' method.
        /// </summary>
        public static readonly StringName SetPhysicsInterpolationEnabled = "set_physics_interpolation_enabled";
        /// <summary>
        /// Cached name for the 'is_physics_interpolation_enabled' method.
        /// </summary>
        public static readonly StringName IsPhysicsInterpolationEnabled = "is_physics_interpolation_enabled";
        /// <summary>
        /// Cached name for the 'queue_delete' method.
        /// </summary>
        public static readonly StringName QueueDelete = "queue_delete";
        /// <summary>
        /// Cached name for the 'call_group_flags' method.
        /// </summary>
        public static readonly StringName CallGroupFlags = "call_group_flags";
        /// <summary>
        /// Cached name for the 'notify_group_flags' method.
        /// </summary>
        public static readonly StringName NotifyGroupFlags = "notify_group_flags";
        /// <summary>
        /// Cached name for the 'set_group_flags' method.
        /// </summary>
        public static readonly StringName SetGroupFlags = "set_group_flags";
        /// <summary>
        /// Cached name for the 'call_group' method.
        /// </summary>
        public static readonly StringName CallGroup = "call_group";
        /// <summary>
        /// Cached name for the 'notify_group' method.
        /// </summary>
        public static readonly StringName NotifyGroup = "notify_group";
        /// <summary>
        /// Cached name for the 'set_group' method.
        /// </summary>
        public static readonly StringName SetGroup = "set_group";
        /// <summary>
        /// Cached name for the 'get_nodes_in_group' method.
        /// </summary>
        public static readonly StringName GetNodesInGroup = "get_nodes_in_group";
        /// <summary>
        /// Cached name for the 'get_first_node_in_group' method.
        /// </summary>
        public static readonly StringName GetFirstNodeInGroup = "get_first_node_in_group";
        /// <summary>
        /// Cached name for the 'get_node_count_in_group' method.
        /// </summary>
        public static readonly StringName GetNodeCountInGroup = "get_node_count_in_group";
        /// <summary>
        /// Cached name for the 'set_current_scene' method.
        /// </summary>
        public static readonly StringName SetCurrentScene = "set_current_scene";
        /// <summary>
        /// Cached name for the 'get_current_scene' method.
        /// </summary>
        public static readonly StringName GetCurrentScene = "get_current_scene";
        /// <summary>
        /// Cached name for the 'change_scene_to_file' method.
        /// </summary>
        public static readonly StringName ChangeSceneToFile = "change_scene_to_file";
        /// <summary>
        /// Cached name for the 'change_scene_to_packed' method.
        /// </summary>
        public static readonly StringName ChangeSceneToPacked = "change_scene_to_packed";
        /// <summary>
        /// Cached name for the 'change_scene_to_node' method.
        /// </summary>
        public static readonly StringName ChangeSceneToNode = "change_scene_to_node";
        /// <summary>
        /// Cached name for the 'reload_current_scene' method.
        /// </summary>
        public static readonly StringName ReloadCurrentScene = "reload_current_scene";
        /// <summary>
        /// Cached name for the 'unload_current_scene' method.
        /// </summary>
        public static readonly StringName UnloadCurrentScene = "unload_current_scene";
        /// <summary>
        /// Cached name for the 'set_multiplayer' method.
        /// </summary>
        public static readonly StringName SetMultiplayer = "set_multiplayer";
        /// <summary>
        /// Cached name for the 'get_multiplayer' method.
        /// </summary>
        public static readonly StringName GetMultiplayer = "get_multiplayer";
        /// <summary>
        /// Cached name for the 'set_multiplayer_poll_enabled' method.
        /// </summary>
        public static readonly StringName SetMultiplayerPollEnabled = "set_multiplayer_poll_enabled";
        /// <summary>
        /// Cached name for the 'is_multiplayer_poll_enabled' method.
        /// </summary>
        public static readonly StringName IsMultiplayerPollEnabled = "is_multiplayer_poll_enabled";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : MainLoop.SignalName
    {
        /// <summary>
        /// Cached name for the 'tree_changed' signal.
        /// </summary>
        public static readonly StringName TreeChanged = "tree_changed";
        /// <summary>
        /// Cached name for the 'scene_changed' signal.
        /// </summary>
        public static readonly StringName SceneChanged = "scene_changed";
        /// <summary>
        /// Cached name for the 'tree_process_mode_changed' signal.
        /// </summary>
        public static readonly StringName TreeProcessModeChanged = "tree_process_mode_changed";
        /// <summary>
        /// Cached name for the 'node_added' signal.
        /// </summary>
        public static readonly StringName NodeAdded = "node_added";
        /// <summary>
        /// Cached name for the 'node_removed' signal.
        /// </summary>
        public static readonly StringName NodeRemoved = "node_removed";
        /// <summary>
        /// Cached name for the 'node_renamed' signal.
        /// </summary>
        public static readonly StringName NodeRenamed = "node_renamed";
        /// <summary>
        /// Cached name for the 'node_configuration_warning_changed' signal.
        /// </summary>
        public static readonly StringName NodeConfigurationWarningChanged = "node_configuration_warning_changed";
        /// <summary>
        /// Cached name for the 'process_frame' signal.
        /// </summary>
        public static readonly StringName ProcessFrame = "process_frame";
        /// <summary>
        /// Cached name for the 'physics_frame' signal.
        /// </summary>
        public static readonly StringName PhysicsFrame = "physics_frame";
    }
}
