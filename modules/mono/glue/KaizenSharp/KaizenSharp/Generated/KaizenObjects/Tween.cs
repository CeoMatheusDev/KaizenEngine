namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Tweens are mostly useful for animations requiring a numerical property to be interpolated over a range of values. The name <i>tween</i> comes from <i>in-betweening</i>, an animation technique where you specify <i>keyframes</i> and the computer interpolates the frames that appear between them. Animating something with a <see cref="Kaizen.Tween"/> is called tweening.</para>
/// <para><see cref="Kaizen.Tween"/> is more suited than <see cref="Kaizen.AnimationPlayer"/> for animations where you don't know the final values in advance. For example, interpolating a dynamically-chosen camera zoom value is best done with a <see cref="Kaizen.Tween"/>; it would be difficult to do the same thing with an <see cref="Kaizen.AnimationPlayer"/> node. Tweens are also more light-weight than <see cref="Kaizen.AnimationPlayer"/>, so they are very much suited for simple animations or general tasks that don't require visual tweaking provided by the editor. They can be used in a "fire-and-forget" manner for some logic that normally would be done by code. You can e.g. make something shoot periodically by using a looped <see cref="Kaizen.CallbackTweener"/> with a delay.</para>
/// <para>A <see cref="Kaizen.Tween"/> can be created by using either <see cref="Kaizen.SceneTree.CreateTween()"/> or <see cref="Kaizen.Node.CreateTween()"/>. <see cref="Kaizen.Tween"/>s created manually (i.e. by using <c>Tween.new()</c>) are invalid and can't be used for tweening values.</para>
/// <para>A tween animation is created by adding <see cref="Kaizen.Tweener"/>s to the <see cref="Kaizen.Tween"/> object, using <see cref="Kaizen.Tween.TweenProperty(KaizenObject, NodePath, Variant, double)"/>, <see cref="Kaizen.Tween.TweenInterval(double)"/>, <see cref="Kaizen.Tween.TweenCallback(Callable)"/>, <see cref="Kaizen.Tween.TweenMethod(Callable, Variant, Variant, double)"/>, <see cref="Kaizen.Tween.TweenSubtween(Tween)"/>, or <see cref="Kaizen.Tween.TweenAwait(Signal)"/>:</para>
/// <para><code>
/// Tween tween = GetTree().CreateTween();
/// tween.TweenProperty(GetNode("Sprite"), "modulate", Colors.Red, 1.0f);
/// tween.TweenProperty(GetNode("Sprite"), "scale", Vector2.Zero, 1.0f);
/// tween.TweenCallback(Callable.From(GetNode("Sprite").QueueFree));
/// </code></para>
/// <para>This sequence will make the <c>$Sprite</c> node turn red, then shrink, before finally calling <see cref="Kaizen.Node.QueueFree()"/> to free the sprite. <see cref="Kaizen.Tweener"/>s are executed one after another by default. This behavior can be changed using <see cref="Kaizen.Tween.Parallel()"/> and <see cref="Kaizen.Tween.SetParallel(bool)"/>.</para>
/// <para>When a <see cref="Kaizen.Tweener"/> is created with one of the <c>tween_*</c> methods, a chained method call can be used to tweak the properties of this <see cref="Kaizen.Tweener"/>. For example, if you want to set a different transition type in the above example, you can use <see cref="Kaizen.Tween.SetTrans(Tween.TransitionType)"/>:</para>
/// <para><code>
/// Tween tween = GetTree().CreateTween();
/// tween.TweenProperty(GetNode("Sprite"), "modulate", Colors.Red, 1.0f).SetTrans(Tween.TransitionType.Sine);
/// tween.TweenProperty(GetNode("Sprite"), "scale", Vector2.Zero, 1.0f).SetTrans(Tween.TransitionType.Bounce);
/// tween.TweenCallback(Callable.From(GetNode("Sprite").QueueFree));
/// </code></para>
/// <para>Most of the <see cref="Kaizen.Tween"/> methods can be chained this way too. In the following example the <see cref="Kaizen.Tween"/> is bound to the running script's node and a default transition is set for its <see cref="Kaizen.Tweener"/>s:</para>
/// <para><code>
/// var tween = GetTree().CreateTween().BindNode(this).SetTrans(Tween.TransitionType.Elastic);
/// tween.TweenProperty(GetNode("Sprite"), "modulate", Colors.Red, 1.0f);
/// tween.TweenProperty(GetNode("Sprite"), "scale", Vector2.Zero, 1.0f);
/// tween.TweenCallback(Callable.From(GetNode("Sprite").QueueFree));
/// </code></para>
/// <para>Another interesting use for <see cref="Kaizen.Tween"/>s is animating arbitrary sets of objects:</para>
/// <para><code>
/// Tween tween = CreateTween();
/// foreach (Node sprite in GetChildren())
/// 	tween.TweenProperty(sprite, "position", Vector2.Zero, 1.0f);
/// </code></para>
/// <para>In the example above, all children of a node are moved one after another to position <c>(0, 0)</c>.</para>
/// <para>You should avoid using more than one <see cref="Kaizen.Tween"/> per object's property. If two or more tweens animate one property at the same time, the last one created will take priority and assign the final value. If you want to interrupt and restart an animation, consider assigning the <see cref="Kaizen.Tween"/> to a variable:</para>
/// <para><code>
/// private Tween _tween;
/// 
/// public void Animate()
/// {
/// 	if (_tween != null)
/// 		_tween.Kill(); // Abort the previous animation
/// 	_tween = CreateTween();
/// }
/// </code></para>
/// <para>Some <see cref="Kaizen.Tweener"/>s use transitions and eases. The first accepts a <see cref="Kaizen.Tween.TransitionType"/> constant, and refers to the way the timing of the animation is handled (see <a href="https://easings.net/">easings.net</a> for some examples). The second accepts an <see cref="Kaizen.Tween.EaseType"/> constant, and controls where the <c>trans_type</c> is applied to the interpolation (in the beginning, the end, or both). If you don't know which transition and easing to pick, you can try different <see cref="Kaizen.Tween.TransitionType"/> constants with <see cref="Kaizen.Tween.EaseType.InOut"/>, and use the one that looks best.</para>
/// <para><a href="https://raw.githubusercontent.com/kaizenengine/kaizen-docs/master/img/tween_cheatsheet.webp">Tween easing and transition types cheatsheet</a></para>
/// <para><b>Note:</b> Tweens are not designed to be reused and trying to do so results in an undefined behavior. Create a new Tween for each animation and every time you replay an animation from start. Keep in mind that Tweens start immediately, so only create a Tween when you want to start animating.</para>
/// <para><b>Note:</b> The tween is processed after all of the nodes in the current frame, i.e. node's <see cref="Kaizen.Node._Process(double)"/> method would be called before the tween (or <see cref="Kaizen.Node._PhysicsProcess(double)"/> depending on the value passed to <see cref="Kaizen.Tween.SetProcessMode(Tween.TweenProcessMode)"/>).</para>
/// </summary>
public partial class Tween : RefCounted
{
    public enum TweenProcessMode : long
    {
        /// <summary>
        /// <para>The <see cref="Kaizen.Tween"/> updates after each physics frame (see <see cref="Kaizen.Node._PhysicsProcess(double)"/>).</para>
        /// </summary>
        Physics = 0,
        /// <summary>
        /// <para>The <see cref="Kaizen.Tween"/> updates after each process frame (see <see cref="Kaizen.Node._Process(double)"/>).</para>
        /// </summary>
        Idle = 1
    }

    public enum TweenPauseMode : long
    {
        /// <summary>
        /// <para>If the <see cref="Kaizen.Tween"/> has a bound node, it will process when that node can process (see <see cref="Kaizen.Node.ProcessMode"/>). Otherwise it's the same as <see cref="Kaizen.Tween.TweenPauseMode.Stop"/>.</para>
        /// </summary>
        Bound = 0,
        /// <summary>
        /// <para>If <see cref="Kaizen.SceneTree"/> is paused, the <see cref="Kaizen.Tween"/> will also pause.</para>
        /// </summary>
        Stop = 1,
        /// <summary>
        /// <para>The <see cref="Kaizen.Tween"/> will process regardless of whether <see cref="Kaizen.SceneTree"/> is paused.</para>
        /// </summary>
        Process = 2
    }

    public enum TransitionType : long
    {
        /// <summary>
        /// <para>The animation is interpolated linearly.</para>
        /// </summary>
        Linear = 0,
        /// <summary>
        /// <para>The animation is interpolated using a sine function.</para>
        /// </summary>
        Sine = 1,
        /// <summary>
        /// <para>The animation is interpolated with a quintic (to the power of 5) function.</para>
        /// </summary>
        Quint = 2,
        /// <summary>
        /// <para>The animation is interpolated with a quartic (to the power of 4) function.</para>
        /// </summary>
        Quart = 3,
        /// <summary>
        /// <para>The animation is interpolated with a quadratic (to the power of 2) function.</para>
        /// </summary>
        Quad = 4,
        /// <summary>
        /// <para>The animation is interpolated with an exponential (to the power of x) function.</para>
        /// </summary>
        Expo = 5,
        /// <summary>
        /// <para>The animation is interpolated with elasticity, wiggling around the edges.</para>
        /// </summary>
        Elastic = 6,
        /// <summary>
        /// <para>The animation is interpolated with a cubic (to the power of 3) function.</para>
        /// </summary>
        Cubic = 7,
        /// <summary>
        /// <para>The animation is interpolated with a function using square roots.</para>
        /// </summary>
        Circ = 8,
        /// <summary>
        /// <para>The animation is interpolated by bouncing at the end.</para>
        /// </summary>
        Bounce = 9,
        /// <summary>
        /// <para>The animation is interpolated backing out at ends.</para>
        /// </summary>
        Back = 10,
        /// <summary>
        /// <para>The animation is interpolated like a spring towards the end.</para>
        /// </summary>
        Spring = 11
    }

    public enum EaseType : long
    {
        /// <summary>
        /// <para>The interpolation starts slowly and speeds up towards the end.</para>
        /// </summary>
        In = 0,
        /// <summary>
        /// <para>The interpolation starts quickly and slows down towards the end.</para>
        /// </summary>
        Out = 1,
        /// <summary>
        /// <para>A combination of <see cref="Kaizen.Tween.EaseType.In"/> and <see cref="Kaizen.Tween.EaseType.Out"/>. The interpolation is slowest at both ends.</para>
        /// </summary>
        InOut = 2,
        /// <summary>
        /// <para>A combination of <see cref="Kaizen.Tween.EaseType.In"/> and <see cref="Kaizen.Tween.EaseType.Out"/>. The interpolation is fastest at both ends.</para>
        /// </summary>
        OutIn = 3
    }

    private static readonly System.Type CachedType = typeof(Tween);

    private static readonly StringName NativeName = "Tween";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public Tween() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal Tween(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal Tween(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.TweenProperty, 4049770449ul);

    /// <summary>
    /// <para>Creates and appends a <see cref="Kaizen.PropertyTweener"/>. This method tweens a <paramref name="property"/> of an <paramref name="object"/> between an initial value and <paramref name="finalVal"/> in a span of time equal to <paramref name="duration"/>, in seconds. The initial value by default is the property's value at the time the tweening of the <see cref="Kaizen.PropertyTweener"/> starts.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenProperty(GetNode("Sprite"), "position", new Vector2(100.0f, 200.0f), 1.0f);
    /// tween.TweenProperty(GetNode("Sprite"), "position", new Vector2(200.0f, 300.0f), 1.0f);
    /// </code></para>
    /// <para>will move the sprite to position (100, 200) and then to (200, 300). If you use <see cref="Kaizen.PropertyTweener.From(Variant)"/> or <see cref="Kaizen.PropertyTweener.FromCurrent()"/>, the starting position will be overwritten by the given value instead. See other methods in <see cref="Kaizen.PropertyTweener"/> to see how the tweening can be tweaked further.</para>
    /// <para><b>Note:</b> You can find the correct property name by hovering over the property in the Inspector. You can also provide the components of a property directly by using <c>"property:component"</c> (eg. <c>position:x</c>), where it would only apply to that particular component.</para>
    /// <para><b>Example:</b> Moving an object twice from the same position, with different transition types:</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenProperty(GetNode("Sprite"), "position", Vector2.Right * 300.0f, 1.0f).AsRelative().SetTrans(Tween.TransitionType.Sine);
    /// tween.TweenProperty(GetNode("Sprite"), "position", Vector2.Right * 300.0f, 1.0f).AsRelative().FromCurrent().SetTrans(Tween.TransitionType.Expo);
    /// </code></para>
    /// </summary>
    public PropertyTweener TweenProperty(KaizenObject @object, NodePath property, Variant finalVal, double duration)
    {
        PropertyTweener ret = (PropertyTweener)NativeCalls.kaizen_icall_4_1546(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(@object), (kaizen_node_path)(property?.NativeValue ?? default), finalVal, duration);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.TweenInterval, 413360199ul);

    /// <summary>
    /// <para>Creates and appends an <see cref="Kaizen.IntervalTweener"/>. This method can be used to create delays in the tween animation, as an alternative to using the delay in other <see cref="Kaizen.Tweener"/>s, or when there's no animation (in which case the <see cref="Kaizen.Tween"/> acts as a timer). <paramref name="time"/> is the length of the interval, in seconds.</para>
    /// <para><b>Example:</b> Creating an interval in code execution:</para>
    /// <para><code>
    /// // ... some code
    /// await ToSignal(CreateTween().TweenInterval(2.0f), Tween.SignalName.Finished);
    /// // ... more code
    /// </code></para>
    /// <para><b>Example:</b> Creating an object that moves back and forth and jumps every few seconds:</para>
    /// <para><code>
    /// Tween tween = CreateTween().SetLoops();
    /// tween.TweenProperty(GetNode("Sprite"), "position:x", 200.0f, 1.0f).AsRelative();
    /// tween.TweenCallback(Callable.From(Jump));
    /// tween.TweenInterval(2.0f);
    /// tween.TweenProperty(GetNode("Sprite"), "position:x", -200.0f, 1.0f).AsRelative();
    /// tween.TweenCallback(Callable.From(Jump));
    /// tween.TweenInterval(2.0f);
    /// </code></para>
    /// </summary>
    public IntervalTweener TweenInterval(double time)
    {
        IntervalTweener ret = (IntervalTweener)NativeCalls.kaizen_icall_1_239(MethodBind1, KaizenObject.GetPtr(this), time);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.TweenCallback, 1540176488ul);

    /// <summary>
    /// <para>Creates and appends a <see cref="Kaizen.CallbackTweener"/>. This method can be used to call an arbitrary method in any object. Use <c>Callable.bind</c> to bind additional arguments for the call.</para>
    /// <para><b>Example:</b> Object that keeps shooting every 1 second:</para>
    /// <para><code>
    /// Tween tween = GetTree().CreateTween().SetLoops();
    /// tween.TweenCallback(Callable.From(Shoot)).SetDelay(1.0f);
    /// </code></para>
    /// <para><b>Example:</b> Turning a sprite red and then blue, with 2 second delay:</para>
    /// <para><code>
    /// Tween tween = GetTree().CreateTween();
    /// Sprite2D sprite = GetNode&lt;Sprite2D&gt;("Sprite");
    /// tween.TweenCallback(Callable.From(() =&gt; sprite.Modulate = Colors.Red)).SetDelay(2.0f);
    /// tween.TweenCallback(Callable.From(() =&gt; sprite.Modulate = Colors.Blue)).SetDelay(2.0f);
    /// </code></para>
    /// </summary>
    public CallbackTweener TweenCallback(Callable callback)
    {
        CallbackTweener ret = (CallbackTweener)NativeCalls.kaizen_icall_1_820(MethodBind2, KaizenObject.GetPtr(this), callback);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.TweenMethod, 2337877153ul);

    /// <summary>
    /// <para>Creates and appends a <see cref="Kaizen.MethodTweener"/>. This method is similar to a combination of <see cref="Kaizen.Tween.TweenCallback(Callable)"/> and <see cref="Kaizen.Tween.TweenProperty(KaizenObject, NodePath, Variant, double)"/>. It calls a method over time with a tweened value provided as an argument. The value is tweened between <paramref name="from"/> and <paramref name="to"/> over the time specified by <paramref name="duration"/>, in seconds. Use <c>Callable.bind</c> to bind additional arguments for the call. You can use <see cref="Kaizen.MethodTweener.SetEase(Tween.EaseType)"/> and <see cref="Kaizen.MethodTweener.SetTrans(Tween.TransitionType)"/> to tweak the easing and transition of the value or <see cref="Kaizen.MethodTweener.SetDelay(double)"/> to delay the tweening.</para>
    /// <para><b>Example:</b> Making a 3D object look from one point to another point:</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenMethod(Callable.From((Vector3 target) =&gt; LookAt(target, Vector3.Up)), new Vector3(-1.0f, 0.0f, -1.0f), new Vector3(1.0f, 0.0f, -1.0f), 1.0f); // Use lambdas to bind additional arguments for the call.
    /// </code></para>
    /// <para><b>Example:</b> Setting the text of a <see cref="Kaizen.Label"/>, using an intermediate method and after a delay:</para>
    /// <para><code>
    /// public override void _Ready()
    /// {
    /// 	base._Ready();
    /// 
    /// 	Tween tween = CreateTween();
    /// 	tween.TweenMethod(Callable.From&lt;int&gt;(SetLabelText), 0.0f, 10.0f, 1.0f).SetDelay(1.0f);
    /// }
    /// 
    /// private void SetLabelText(int value)
    /// {
    /// 	GetNode&lt;Label&gt;("Label").Text = $"Counting {value}";
    /// }
    /// </code></para>
    /// </summary>
    public MethodTweener TweenMethod(Callable method, Variant from, Variant to, double duration)
    {
        MethodTweener ret = (MethodTweener)NativeCalls.kaizen_icall_4_1547(MethodBind3, KaizenObject.GetPtr(this), method, from, to, duration);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.TweenSubtween, 1567358477ul);

    /// <summary>
    /// <para>Creates and appends a <see cref="Kaizen.SubtweenTweener"/>. This method can be used to nest <paramref name="subtween"/> within this <see cref="Kaizen.Tween"/>, allowing for the creation of more complex and composable sequences.</para>
    /// <para><code>
    /// // Subtween will rotate the object.
    /// Tween subtween = CreateTween();
    /// subtween.TweenProperty(this, "rotation_degrees", 45.0f, 1.0f);
    /// subtween.TweenProperty(this, "rotation_degrees", 0.0f, 1.0f);
    /// 
    /// // Parent tween will execute the subtween as one of its steps.
    /// Tween tween = CreateTween();
    /// tween.TweenProperty(this, "position:x", 500.0f, 3.0f);
    /// tween.TweenSubtween(subtween);
    /// tween.TweenProperty(this, "position:x", 300.0f, 2.0f);
    /// </code></para>
    /// <para><b>Note:</b> The methods <see cref="Kaizen.Tween.Pause()"/>, <see cref="Kaizen.Tween.Stop()"/>, and <see cref="Kaizen.Tween.SetLoops(int)"/> can cause the parent <see cref="Kaizen.Tween"/> to get stuck on the subtween step; see the documentation for those methods for more information.</para>
    /// <para><b>Note:</b> The pause and process modes set by <see cref="Kaizen.Tween.SetPauseMode(Tween.TweenPauseMode)"/> and <see cref="Kaizen.Tween.SetProcessMode(Tween.TweenProcessMode)"/> on <paramref name="subtween"/> will be overridden by the parent <see cref="Kaizen.Tween"/>'s settings.</para>
    /// </summary>
    public SubtweenTweener TweenSubtween(Tween subtween)
    {
        SubtweenTweener ret = (SubtweenTweener)NativeCalls.kaizen_icall_1_297(MethodBind4, KaizenObject.GetPtr(this), KaizenObject.GetPtr(subtween));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.TweenAwait, 2242837462ul);

    /// <summary>
    /// <para>Creates and appends an <see cref="Kaizen.AwaitTweener"/>. This method can be used to await a signal to be emitted and create asynchronous animations or cutscenes.</para>
    /// <para>The animation will not progress to the next step until the awaited signal is emitted or the connection becomes invalid (e.g. as a result of freeing the target object). If you know that the emission may not happen, use <see cref="Kaizen.AwaitTweener.SetTimeout(double)"/>.</para>
    /// <para><b>Note:</b> The awaited signal should be emitted during the step when <see cref="Kaizen.AwaitTweener"/> is active.</para>
    /// <para><b>Example:</b> An object launches itself and explodes upon collision or after 4 seconds.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenCallback(Callable.From(Launch));
    /// tween.TweenAwait(new Signal(this, SignalName.Collided)).SetTimeout(4.0f);
    /// tween.TweenCallback(Callable.From(Explode));
    /// </code></para>
    /// <para><b>Example:</b> A character walks to a specific point, says some lines and walks back when the player closes the message box.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenCallback(Callable.From(() =&gt; WalkTo(600.0f)));
    /// tween.TweenAwait(new Signal(this, SignalName.DestinationReached));
    /// tween.TweenCallback(Callable.From(() =&gt; SayDialogue("Good day, sir!")));
    /// tween.TweenAwait(new Signal(this, SignalName.DialogueClosed));
    /// tween.TweenCallback(Callable.From(() =&gt; WalkTo(0.0f)));
    /// </code></para>
    /// <para><b>Note:</b> If you are awaiting a signal from a callback called in the same <see cref="Kaizen.Tween"/>, make sure the signal is emitted <i>after</i> the await starts. If it can't be reasonably guaranteed, you can await and emit in the same step:</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenAwait(new Signal(this, SignalName.Signal));
    /// tween.Parallel().TweenCallback(Callable.From(MethodThatEmitsSignal));
    /// </code></para>
    /// </summary>
    public AwaitTweener TweenAwait(Signal signal)
    {
        AwaitTweener ret = (AwaitTweener)NativeCalls.kaizen_icall_1_1548(MethodBind5, KaizenObject.GetPtr(this), signal);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CustomStep, 330693286ul);

    /// <summary>
    /// <para>Processes the <see cref="Kaizen.Tween"/> by the given <paramref name="delta"/> value, in seconds. This is mostly useful for manual control when the <see cref="Kaizen.Tween"/> is paused. It can also be used to end the <see cref="Kaizen.Tween"/> animation immediately, by setting <paramref name="delta"/> longer than the whole duration of the <see cref="Kaizen.Tween"/> animation.</para>
    /// <para>Returns <see langword="true"/> if the <see cref="Kaizen.Tween"/> still has <see cref="Kaizen.Tweener"/>s that haven't finished.</para>
    /// </summary>
    public bool CustomStep(double delta)
    {
        bool ret = NativeCalls.kaizen_icall_1_587(MethodBind6, KaizenObject.GetPtr(this), delta).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Stop, 3218959716ul);

    /// <summary>
    /// <para>Stops the tweening and resets the <see cref="Kaizen.Tween"/> to its initial state. This will not remove any appended <see cref="Kaizen.Tweener"/>s.</para>
    /// <para><b>Note:</b> This does <i>not</i> reset targets of <see cref="Kaizen.PropertyTweener"/>s to their values when the <see cref="Kaizen.Tween"/> first started.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// 
    /// // Will move from 0 to 500 over 1 second.
    /// Position = Position with { X = 0.0f };
    /// tween.TweenProperty(this, "position:x", 500.0f, 1.0f);
    /// 
    /// // Will be at (about) 250 when the timer finishes.
    /// await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);
    /// 
    /// // Will now move from (about) 250 to 500 over 1 second,
    /// // thus at half the speed as before.
    /// tween.Stop();
    /// tween.Play();
    /// </code></para>
    /// <para><b>Note:</b> If a Tween is stopped and not bound to any node, it will exist indefinitely until manually started or invalidated. If you lose a reference to such Tween, you can retrieve it using <see cref="Kaizen.SceneTree.GetProcessedTweens()"/>.</para>
    /// </summary>
    public void Stop()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Pause, 3218959716ul);

    /// <summary>
    /// <para>Pauses the tweening. The animation can be resumed by using <see cref="Kaizen.Tween.Play()"/>.</para>
    /// <para><b>Note:</b> If a Tween is paused and not bound to any node, it will exist indefinitely until manually started or invalidated. If you lose a reference to such Tween, you can retrieve it using <see cref="Kaizen.SceneTree.GetProcessedTweens()"/>.</para>
    /// </summary>
    public void Pause()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind8, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Play, 3218959716ul);

    /// <summary>
    /// <para>Resumes a paused or stopped <see cref="Kaizen.Tween"/>.</para>
    /// </summary>
    public void Play()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Kill, 3218959716ul);

    /// <summary>
    /// <para>Aborts all tweening operations and invalidates the <see cref="Kaizen.Tween"/>.</para>
    /// </summary>
    public void Kill()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTotalElapsedTime, 1740695150ul);

    /// <summary>
    /// <para>Returns the total time in seconds the <see cref="Kaizen.Tween"/> has been animating (i.e. the time since it started, not counting pauses etc.). The time is affected by <see cref="Kaizen.Tween.SetSpeedScale(float)"/>, and <see cref="Kaizen.Tween.Stop()"/> will reset it to <c>0</c>.</para>
    /// <para><b>Note:</b> As it results from accumulating frame deltas, the time returned after the <see cref="Kaizen.Tween"/> has finished animating will be slightly greater than the actual <see cref="Kaizen.Tween"/> duration.</para>
    /// </summary>
    public double GetTotalElapsedTime()
    {
        double ret = NativeCalls.kaizen_icall_0_136(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasTweeners, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if any <see cref="Kaizen.Tweener"/> has been added to the <see cref="Kaizen.Tween"/> and the <see cref="Kaizen.Tween"/> is valid. Useful when tweeners are added dynamically and the tween can end up empty. Killing an empty tween before it starts will prevent errors.</para>
    /// </summary>
    public bool HasTweeners()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind12, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsRunning, 2240911060ul);

    /// <summary>
    /// <para>Returns whether the <see cref="Kaizen.Tween"/> is currently running, i.e. it wasn't paused and it's not finished.</para>
    /// </summary>
    public bool IsRunning()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind13, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsValid, 2240911060ul);

    /// <summary>
    /// <para>Returns whether the <see cref="Kaizen.Tween"/> is valid. A valid <see cref="Kaizen.Tween"/> is a <see cref="Kaizen.Tween"/> contained by the scene tree (i.e. the array from <see cref="Kaizen.SceneTree.GetProcessedTweens()"/> will contain this <see cref="Kaizen.Tween"/>). A <see cref="Kaizen.Tween"/> might become invalid when it has finished tweening, is killed, or when created with <c>Tween.new()</c>. Invalid <see cref="Kaizen.Tween"/>s can't have <see cref="Kaizen.Tweener"/>s appended.</para>
    /// </summary>
    public bool IsValid()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind14, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.BindNode, 2946786331ul);

    /// <summary>
    /// <para>Binds this <see cref="Kaizen.Tween"/> with the given <paramref name="node"/>. <see cref="Kaizen.Tween"/>s are processed directly by the <see cref="Kaizen.SceneTree"/>, so they run independently of the animated nodes. When you bind a <see cref="Kaizen.Node"/> with the <see cref="Kaizen.Tween"/>, the <see cref="Kaizen.Tween"/> will halt the animation when the object is not inside tree and the <see cref="Kaizen.Tween"/> will be automatically killed when the bound object is freed. Also <see cref="Kaizen.Tween.TweenPauseMode.Bound"/> will make the pausing behavior dependent on the bound node.</para>
    /// <para>For a shorter way to create and bind a <see cref="Kaizen.Tween"/>, you can use <see cref="Kaizen.Node.CreateTween()"/>.</para>
    /// </summary>
    public Tween BindNode(Node node)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_297(MethodBind15, KaizenObject.GetPtr(this), KaizenObject.GetPtr(node));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetProcessMode, 855258840ul);

    /// <summary>
    /// <para>Determines whether the <see cref="Kaizen.Tween"/> should run after process frames (see <see cref="Kaizen.Node._Process(double)"/>) or physics frames (see <see cref="Kaizen.Node._PhysicsProcess(double)"/>).</para>
    /// <para>Default value is <see cref="Kaizen.Tween.TweenProcessMode.Idle"/>.</para>
    /// </summary>
    public Tween SetProcessMode(Tween.TweenProcessMode mode)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_95(MethodBind16, KaizenObject.GetPtr(this), (int)mode);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPauseMode, 3363368837ul);

    /// <summary>
    /// <para>Determines the behavior of the <see cref="Kaizen.Tween"/> when the <see cref="Kaizen.SceneTree"/> is paused.</para>
    /// <para>Default value is <see cref="Kaizen.Tween.TweenPauseMode.Bound"/>.</para>
    /// </summary>
    public Tween SetPauseMode(Tween.TweenPauseMode mode)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_95(MethodBind17, KaizenObject.GetPtr(this), (int)mode);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetIgnoreTimeScale, 1942052223ul);

    /// <summary>
    /// <para>If <paramref name="ignore"/> is <see langword="true"/>, the tween will ignore <see cref="Kaizen.Engine.TimeScale"/> and update with the real, elapsed time. This affects all <see cref="Kaizen.Tweener"/>s and their delays. Default value is <see langword="false"/>.</para>
    /// </summary>
    public Tween SetIgnoreTimeScale(bool ignore = true)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_684(MethodBind18, KaizenObject.GetPtr(this), ignore.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetParallel, 1942052223ul);

    /// <summary>
    /// <para>If <paramref name="parallel"/> is <see langword="true"/>, the <see cref="Kaizen.Tweener"/>s appended after this method will by default run simultaneously, as opposed to sequentially.</para>
    /// <para><b>Note:</b> Just like with <see cref="Kaizen.Tween.Parallel()"/>, the tweener added right before this method will also be part of the parallel step.</para>
    /// <para><code>
    /// tween.TweenProperty(this, "position", new Vector2(300, 0), 0.5f);
    /// tween.SetParallel();
    /// tween.TweenProperty(this, "modulate", Colors.Green, 0.5f); // Runs together with the position tweener.
    /// </code></para>
    /// </summary>
    public Tween SetParallel(bool parallel = true)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_684(MethodBind19, KaizenObject.GetPtr(this), parallel.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLoops, 2670836414ul);

    /// <summary>
    /// <para>Sets the number of times the tweening sequence will be repeated, i.e. <c>set_loops(2)</c> will run the animation twice.</para>
    /// <para>Calling this method without arguments will make the <see cref="Kaizen.Tween"/> run infinitely, until either it is killed with <see cref="Kaizen.Tween.Kill()"/>, the <see cref="Kaizen.Tween"/>'s bound node is freed, or all the animated objects have been freed (which makes further animation impossible).</para>
    /// <para><b>Warning:</b> Make sure to always add some duration/delay when using infinite loops. To prevent the game freezing, 0-duration looped animations (e.g. a single <see cref="Kaizen.CallbackTweener"/> with no delay) are stopped after a small number of loops, which may produce unexpected results. If a <see cref="Kaizen.Tween"/>'s lifetime depends on some node, always use <see cref="Kaizen.Tween.BindNode(Node)"/>.</para>
    /// </summary>
    public Tween SetLoops(int loops = 0)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_95(MethodBind20, KaizenObject.GetPtr(this), loops);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLoopsLeft, 3905245786ul);

    /// <summary>
    /// <para>Returns the number of remaining loops for this <see cref="Kaizen.Tween"/> (see <see cref="Kaizen.Tween.SetLoops(int)"/>). A return value of <c>-1</c> indicates an infinitely looping <see cref="Kaizen.Tween"/>, and a return value of <c>0</c> indicates that the <see cref="Kaizen.Tween"/> has already finished.</para>
    /// </summary>
    public int GetLoopsLeft()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind21, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSpeedScale, 3961971106ul);

    /// <summary>
    /// <para>Scales the speed of tweening. This affects all <see cref="Kaizen.Tweener"/>s and their delays.</para>
    /// </summary>
    public Tween SetSpeedScale(float speed)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_829(MethodBind22, KaizenObject.GetPtr(this), speed);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTrans, 3965963875ul);

    /// <summary>
    /// <para>Sets the default transition type for <see cref="Kaizen.PropertyTweener"/>s and <see cref="Kaizen.MethodTweener"/>s appended after this method.</para>
    /// <para>Before this method is called, the default transition type is <see cref="Kaizen.Tween.TransitionType.Linear"/>.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenProperty(this, "position", new Vector2(300, 0), 0.5f); // Uses TransitionType.Linear.
    /// tween.SetTrans(Tween.TransitionType.Sine);
    /// tween.TweenProperty(this, "rotation_degrees", 45.0f, 0.5f); // Uses TransitionType.Sine.
    /// </code></para>
    /// </summary>
    public Tween SetTrans(Tween.TransitionType trans)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_95(MethodBind23, KaizenObject.GetPtr(this), (int)trans);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEase, 1208117252ul);

    /// <summary>
    /// <para>Sets the default ease type for <see cref="Kaizen.PropertyTweener"/>s and <see cref="Kaizen.MethodTweener"/>s appended after this method.</para>
    /// <para>Before this method is called, the default ease type is <see cref="Kaizen.Tween.EaseType.InOut"/>.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenProperty(this, "position", new Vector2(300, 0), 0.5); // Uses EaseType.InOut.
    /// tween.SetEase(Tween.EaseType.In);
    /// tween.TweenProperty(this, "rotation_degrees", 45.0, 0.5); // Uses EaseType.In.
    /// </code></para>
    /// </summary>
    public Tween SetEase(Tween.EaseType ease)
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_1_95(MethodBind24, KaizenObject.GetPtr(this), (int)ease);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Parallel, 3426978995ul);

    /// <summary>
    /// <para>Makes the next <see cref="Kaizen.Tweener"/> run parallelly to the previous one.</para>
    /// <para><code>
    /// Tween tween = CreateTween();
    /// tween.TweenProperty(...);
    /// tween.Parallel().TweenProperty(...);
    /// tween.Parallel().TweenProperty(...);
    /// </code></para>
    /// <para>All <see cref="Kaizen.Tweener"/>s in the example will run at the same time.</para>
    /// <para>You can make the <see cref="Kaizen.Tween"/> parallel by default by using <see cref="Kaizen.Tween.SetParallel(bool)"/>.</para>
    /// </summary>
    public Tween Parallel()
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_0_87(MethodBind25, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Chain, 3426978995ul);

    /// <summary>
    /// <para>Used to chain two <see cref="Kaizen.Tweener"/>s after <see cref="Kaizen.Tween.SetParallel(bool)"/> is called with <see langword="true"/>.</para>
    /// <para><code>
    /// Tween tween = CreateTween().SetParallel(true);
    /// tween.TweenProperty(...);
    /// tween.TweenProperty(...); // Will run parallelly with above.
    /// tween.Chain().TweenProperty(...); // Will run after two above are finished.
    /// </code></para>
    /// </summary>
    public Tween Chain()
    {
        Tween ret = (Tween)NativeCalls.kaizen_icall_0_87(MethodBind26, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.InterpolateValue, 3452526450ul);

    /// <summary>
    /// <para>This method can be used for manual interpolation of a value, when you don't want <see cref="Kaizen.Tween"/> to do animating for you. It's similar to <c>@GlobalScope.lerp</c>, but with support for custom transition and easing.</para>
    /// <para><paramref name="initialValue"/> is the starting value of the interpolation.</para>
    /// <para><paramref name="deltaValue"/> is the change of the value in the interpolation, i.e. it's equal to <c>final_value - initial_value</c>.</para>
    /// <para><paramref name="elapsedTime"/> is the time in seconds that passed after the interpolation started and it's used to control the position of the interpolation. E.g. when it's equal to half of the <paramref name="duration"/>, the interpolated value will be halfway between initial and final values. This value can also be greater than <paramref name="duration"/> or lower than 0, which will extrapolate the value.</para>
    /// <para><paramref name="duration"/> is the total time of the interpolation.</para>
    /// <para><b>Note:</b> If <paramref name="duration"/> is equal to <c>0</c>, the method will always return the final value, regardless of <paramref name="elapsedTime"/> provided.</para>
    /// </summary>
    public static Variant InterpolateValue(Variant initialValue, Variant deltaValue, double elapsedTime, double duration, Tween.TransitionType transType, Tween.EaseType easeType)
    {
        Variant ret = NativeCalls.kaizen_icall_6_1549(MethodBind27, initialValue, deltaValue, elapsedTime, duration, (int)transType, (int)easeType);
        return ret;
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.Tween.StepFinished"/> event of a <see cref="Kaizen.Tween"/> class.
    /// </summary>
    public delegate void StepFinishedEventHandler(long idx);

    private static void StepFinishedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((StepFinishedEventHandler)delegateObj)(VariantUtils.ConvertTo<long>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when one step of the <see cref="Kaizen.Tween"/> is complete, providing the step index. One step is either a single <see cref="Kaizen.Tweener"/> or a group of <see cref="Kaizen.Tweener"/>s running in parallel.</para>
    /// </summary>
    public unsafe event StepFinishedEventHandler StepFinished
    {
        add => Connect(SignalName.StepFinished, Callable.CreateWithUnsafeTrampoline(value, &StepFinishedTrampoline));
        remove => Disconnect(SignalName.StepFinished, Callable.CreateWithUnsafeTrampoline(value, &StepFinishedTrampoline));
    }

    protected void EmitSignalStepFinished(long idx)
    {
        EmitSignal(SignalName.StepFinished, idx);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.Tween.LoopFinished"/> event of a <see cref="Kaizen.Tween"/> class.
    /// </summary>
    public delegate void LoopFinishedEventHandler(long loopCount);

    private static void LoopFinishedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((LoopFinishedEventHandler)delegateObj)(VariantUtils.ConvertTo<long>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted when a full loop is complete (see <see cref="Kaizen.Tween.SetLoops(int)"/>), providing the loop index. This signal is not emitted after the final loop, use <see cref="Kaizen.Tween.Finished"/> instead for this case.</para>
    /// </summary>
    public unsafe event LoopFinishedEventHandler LoopFinished
    {
        add => Connect(SignalName.LoopFinished, Callable.CreateWithUnsafeTrampoline(value, &LoopFinishedTrampoline));
        remove => Disconnect(SignalName.LoopFinished, Callable.CreateWithUnsafeTrampoline(value, &LoopFinishedTrampoline));
    }

    protected void EmitSignalLoopFinished(long loopCount)
    {
        EmitSignal(SignalName.LoopFinished, loopCount);
    }

    /// <summary>
    /// <para>Emitted when the <see cref="Kaizen.Tween"/> has finished all tweening. Never emitted when the <see cref="Kaizen.Tween"/> is set to infinite looping (see <see cref="Kaizen.Tween.SetLoops(int)"/>).</para>
    /// </summary>
    public event Action Finished
    {
        add => Connect(SignalName.Finished, Callable.From(value));
        remove => Disconnect(SignalName.Finished, Callable.From(value));
    }

    protected void EmitSignalFinished()
    {
        EmitSignal(SignalName.Finished);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_step_finished = "StepFinished";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_loop_finished = "LoopFinished";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_finished = "Finished";

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
        if (signal == SignalName.StepFinished)
        {
            if (HasKaizenClassSignal(SignalProxyName_step_finished.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.LoopFinished)
        {
            if (HasKaizenClassSignal(SignalProxyName_loop_finished.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.Finished)
        {
            if (HasKaizenClassSignal(SignalProxyName_finished.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : RefCounted.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the 'tween_property' method.
        /// </summary>
        public static readonly StringName TweenProperty = "tween_property";
        /// <summary>
        /// Cached name for the 'tween_interval' method.
        /// </summary>
        public static readonly StringName TweenInterval = "tween_interval";
        /// <summary>
        /// Cached name for the 'tween_callback' method.
        /// </summary>
        public static readonly StringName TweenCallback = "tween_callback";
        /// <summary>
        /// Cached name for the 'tween_method' method.
        /// </summary>
        public static readonly StringName TweenMethod = "tween_method";
        /// <summary>
        /// Cached name for the 'tween_subtween' method.
        /// </summary>
        public static readonly StringName TweenSubtween = "tween_subtween";
        /// <summary>
        /// Cached name for the 'tween_await' method.
        /// </summary>
        public static readonly StringName TweenAwait = "tween_await";
        /// <summary>
        /// Cached name for the 'custom_step' method.
        /// </summary>
        public static readonly StringName CustomStep = "custom_step";
        /// <summary>
        /// Cached name for the 'stop' method.
        /// </summary>
        public static readonly StringName Stop = "stop";
        /// <summary>
        /// Cached name for the 'pause' method.
        /// </summary>
        public static readonly StringName Pause = "pause";
        /// <summary>
        /// Cached name for the 'play' method.
        /// </summary>
        public static readonly StringName Play = "play";
        /// <summary>
        /// Cached name for the 'kill' method.
        /// </summary>
        public static readonly StringName Kill = "kill";
        /// <summary>
        /// Cached name for the 'get_total_elapsed_time' method.
        /// </summary>
        public static readonly StringName GetTotalElapsedTime = "get_total_elapsed_time";
        /// <summary>
        /// Cached name for the 'has_tweeners' method.
        /// </summary>
        public static readonly StringName HasTweeners = "has_tweeners";
        /// <summary>
        /// Cached name for the 'is_running' method.
        /// </summary>
        public static readonly StringName IsRunning = "is_running";
        /// <summary>
        /// Cached name for the 'is_valid' method.
        /// </summary>
        public static readonly StringName IsValid = "is_valid";
        /// <summary>
        /// Cached name for the 'bind_node' method.
        /// </summary>
        public static readonly StringName BindNode = "bind_node";
        /// <summary>
        /// Cached name for the 'set_process_mode' method.
        /// </summary>
        public static readonly StringName SetProcessMode = "set_process_mode";
        /// <summary>
        /// Cached name for the 'set_pause_mode' method.
        /// </summary>
        public static readonly StringName SetPauseMode = "set_pause_mode";
        /// <summary>
        /// Cached name for the 'set_ignore_time_scale' method.
        /// </summary>
        public static readonly StringName SetIgnoreTimeScale = "set_ignore_time_scale";
        /// <summary>
        /// Cached name for the 'set_parallel' method.
        /// </summary>
        public static readonly StringName SetParallel = "set_parallel";
        /// <summary>
        /// Cached name for the 'set_loops' method.
        /// </summary>
        public static readonly StringName SetLoops = "set_loops";
        /// <summary>
        /// Cached name for the 'get_loops_left' method.
        /// </summary>
        public static readonly StringName GetLoopsLeft = "get_loops_left";
        /// <summary>
        /// Cached name for the 'set_speed_scale' method.
        /// </summary>
        public static readonly StringName SetSpeedScale = "set_speed_scale";
        /// <summary>
        /// Cached name for the 'set_trans' method.
        /// </summary>
        public static readonly StringName SetTrans = "set_trans";
        /// <summary>
        /// Cached name for the 'set_ease' method.
        /// </summary>
        public static readonly StringName SetEase = "set_ease";
        /// <summary>
        /// Cached name for the 'parallel' method.
        /// </summary>
        public static readonly StringName Parallel = "parallel";
        /// <summary>
        /// Cached name for the 'chain' method.
        /// </summary>
        public static readonly StringName Chain = "chain";
        /// <summary>
        /// Cached name for the 'interpolate_value' method.
        /// </summary>
        public static readonly StringName InterpolateValue = "interpolate_value";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
        /// <summary>
        /// Cached name for the 'step_finished' signal.
        /// </summary>
        public static readonly StringName StepFinished = "step_finished";
        /// <summary>
        /// Cached name for the 'loop_finished' signal.
        /// </summary>
        public static readonly StringName LoopFinished = "loop_finished";
        /// <summary>
        /// Cached name for the 'finished' signal.
        /// </summary>
        public static readonly StringName Finished = "finished";
    }
}
