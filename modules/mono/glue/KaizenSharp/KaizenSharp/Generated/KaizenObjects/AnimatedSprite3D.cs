namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.AnimatedSprite3D"/> is similar to the <see cref="Kaizen.Sprite3D"/> node, except it carries multiple textures as animation <see cref="Kaizen.AnimatedSprite3D.SpriteFrames"/>. Animations are created using a <see cref="Kaizen.SpriteFrames"/> resource, which allows you to import image files (or a folder containing said files) to provide the animation frames for the sprite. The <see cref="Kaizen.SpriteFrames"/> resource can be configured in the editor via the SpriteFrames bottom panel.</para>
/// </summary>
public partial class AnimatedSprite3D : SpriteBase3D
{
    /// <summary>
    /// <para>The <see cref="Kaizen.SpriteFrames"/> resource containing the animation(s). Allows you the option to load, edit, clear, make unique and save the states of the <see cref="Kaizen.SpriteFrames"/> resource.</para>
    /// </summary>
    public SpriteFrames SpriteFrames
    {
        get
        {
            return GetSpriteFrames();
        }
        set
        {
            SetSpriteFrames(value);
        }
    }

    /// <summary>
    /// <para>The current animation from the <see cref="Kaizen.AnimatedSprite3D.SpriteFrames"/> resource. If this value is changed, the <see cref="Kaizen.AnimatedSprite3D.Frame"/> counter and the <see cref="Kaizen.AnimatedSprite3D.FrameProgress"/> are reset.</para>
    /// </summary>
    public StringName Animation
    {
        get
        {
            return GetAnimation();
        }
        set
        {
            SetAnimation(value);
        }
    }

    /// <summary>
    /// <para>The key of the animation to play when the scene loads.</para>
    /// </summary>
    public string Autoplay
    {
        get
        {
            return GetAutoplay();
        }
        set
        {
            SetAutoplay(value);
        }
    }

    /// <summary>
    /// <para>The displayed animation frame's index. Setting this property also resets <see cref="Kaizen.AnimatedSprite3D.FrameProgress"/>. If this is not desired, use <see cref="Kaizen.AnimatedSprite3D.SetFrameAndProgress(int, float)"/>.</para>
    /// </summary>
    public int Frame
    {
        get
        {
            return GetFrame();
        }
        set
        {
            SetFrame(value);
        }
    }

    /// <summary>
    /// <para>The progress value between <c>0.0</c> and <c>1.0</c> until the current frame transitions to the next frame. If the animation is playing backwards, the value transitions from <c>1.0</c> to <c>0.0</c>.</para>
    /// </summary>
    public float FrameProgress
    {
        get
        {
            return GetFrameProgress();
        }
        set
        {
            SetFrameProgress(value);
        }
    }

    /// <summary>
    /// <para>The speed scaling ratio. For example, if this value is <c>1</c>, then the animation plays at normal speed. If it's <c>0.5</c>, then it plays at half speed. If it's <c>2</c>, then it plays at double speed.</para>
    /// <para>If set to a negative value, the animation is played in reverse. If set to <c>0</c>, the animation will not advance.</para>
    /// </summary>
    public float SpeedScale
    {
        get
        {
            return GetSpeedScale();
        }
        set
        {
            SetSpeedScale(value);
        }
    }

    private static readonly System.Type CachedType = typeof(AnimatedSprite3D);

    private static readonly StringName NativeName = "AnimatedSprite3D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public AnimatedSprite3D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal AnimatedSprite3D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal AnimatedSprite3D(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSpriteFrames, 905781144ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSpriteFrames(SpriteFrames spriteFrames)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(spriteFrames));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSpriteFrames, 3804851214ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public SpriteFrames GetSpriteFrames()
    {
        SpriteFrames ret = (SpriteFrames)NativeCalls.kaizen_icall_0_87(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAnimation, 3304788590ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAnimation(StringName name)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind2, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAnimation, 2002593661ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringName GetAnimation()
    {
        StringName ret = NativeCalls.kaizen_icall_0_89(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAutoplay, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAutoplay(string name)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind4, KaizenObject.GetPtr(this), name);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAutoplay, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetAutoplay()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsPlaying, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if an animation is currently playing (even if <see cref="Kaizen.AnimatedSprite3D.SpeedScale"/> and/or <c>custom_speed</c> are <c>0</c>).</para>
    /// </summary>
    public bool IsPlaying()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind6, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Play, 3269405555ul);

    /// <summary>
    /// <para>Plays the animation with key <paramref name="name"/>. If <paramref name="customSpeed"/> is negative and <paramref name="fromEnd"/> is <see langword="true"/>, the animation will play backwards (which is equivalent to calling <see cref="Kaizen.AnimatedSprite3D.PlayBackwards(StringName)"/>).</para>
    /// <para>If this method is called with that same animation <paramref name="name"/>, or with no <paramref name="name"/> parameter, the assigned animation will resume playing if it was paused.</para>
    /// </summary>
    public void Play(StringName name = null, float customSpeed = 1.0f, bool fromEnd = false)
    {
        NativeCalls.kaizen_icall_3_90(MethodBind7, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default), customSpeed, fromEnd.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.PlayBackwards, 3323268493ul);

    /// <summary>
    /// <para>Plays the animation with key <paramref name="name"/> in reverse.</para>
    /// <para>This method is a shorthand for <see cref="Kaizen.AnimatedSprite3D.Play(StringName, float, bool)"/> with <c>custom_speed = -1.0</c> and <c>from_end = true</c>, so see its description for more information.</para>
    /// </summary>
    public void PlayBackwards(StringName name = null)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind8, KaizenObject.GetPtr(this), (kaizen_string_name)(name?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Pause, 3218959716ul);

    /// <summary>
    /// <para>Pauses the currently playing animation. The <see cref="Kaizen.AnimatedSprite3D.Frame"/> and <see cref="Kaizen.AnimatedSprite3D.FrameProgress"/> will be kept and calling <see cref="Kaizen.AnimatedSprite3D.Play(StringName, float, bool)"/> or <see cref="Kaizen.AnimatedSprite3D.PlayBackwards(StringName)"/> without arguments will resume the animation from the current playback position.</para>
    /// <para>See also <see cref="Kaizen.AnimatedSprite3D.Stop()"/>.</para>
    /// </summary>
    public void Pause()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Stop, 3218959716ul);

    /// <summary>
    /// <para>Stops the currently playing animation. The animation position is reset to <c>0</c> and the <c>custom_speed</c> is reset to <c>1.0</c>. See also <see cref="Kaizen.AnimatedSprite3D.Pause()"/>.</para>
    /// </summary>
    public void Stop()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFrame, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFrame(int frame)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind11, KaizenObject.GetPtr(this), frame);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFrame, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetFrame()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind12, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFrameProgress, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFrameProgress(float progress)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind13, KaizenObject.GetPtr(this), progress);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFrameProgress, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetFrameProgress()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind14, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFrameAndProgress, 1602489585ul);

    /// <summary>
    /// <para>Sets <see cref="Kaizen.AnimatedSprite3D.Frame"/> and <see cref="Kaizen.AnimatedSprite3D.FrameProgress"/> to the given values. Unlike setting <see cref="Kaizen.AnimatedSprite3D.Frame"/>, this method does not reset the <see cref="Kaizen.AnimatedSprite3D.FrameProgress"/> to <c>0.0</c> implicitly.</para>
    /// <para><b>Example:</b> Change the animation while keeping the same <see cref="Kaizen.AnimatedSprite3D.Frame"/> and <see cref="Kaizen.AnimatedSprite3D.FrameProgress"/>:</para>
    /// <para></para>
    /// </summary>
    public void SetFrameAndProgress(int frame, float progress)
    {
        NativeCalls.kaizen_icall_2_93(MethodBind15, KaizenObject.GetPtr(this), frame, progress);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSpeedScale, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSpeedScale(float speedScale)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind16, KaizenObject.GetPtr(this), speedScale);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSpeedScale, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetSpeedScale()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind17, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPlayingSpeed, 1740695150ul);

    /// <summary>
    /// <para>Returns the actual playing speed of current animation or <c>0</c> if not playing. This speed is the <see cref="Kaizen.AnimatedSprite3D.SpeedScale"/> property multiplied by <c>custom_speed</c> argument specified when calling the <see cref="Kaizen.AnimatedSprite3D.Play(StringName, float, bool)"/> method.</para>
    /// <para>Returns a negative value if the current animation is playing backwards.</para>
    /// </summary>
    public float GetPlayingSpeed()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind18, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Emitted when <see cref="Kaizen.AnimatedSprite3D.SpriteFrames"/> changes.</para>
    /// </summary>
    public event Action SpriteFramesChanged
    {
        add => Connect(SignalName.SpriteFramesChanged, Callable.From(value));
        remove => Disconnect(SignalName.SpriteFramesChanged, Callable.From(value));
    }

    protected void EmitSignalSpriteFramesChanged()
    {
        EmitSignal(SignalName.SpriteFramesChanged);
    }

    /// <summary>
    /// <para>Emitted when <see cref="Kaizen.AnimatedSprite3D.Animation"/> changes.</para>
    /// </summary>
    public event Action AnimationChanged
    {
        add => Connect(SignalName.AnimationChanged, Callable.From(value));
        remove => Disconnect(SignalName.AnimationChanged, Callable.From(value));
    }

    protected void EmitSignalAnimationChanged()
    {
        EmitSignal(SignalName.AnimationChanged);
    }

    /// <summary>
    /// <para>Emitted when <see cref="Kaizen.AnimatedSprite3D.Frame"/> changes.</para>
    /// </summary>
    public event Action FrameChanged
    {
        add => Connect(SignalName.FrameChanged, Callable.From(value));
        remove => Disconnect(SignalName.FrameChanged, Callable.From(value));
    }

    protected void EmitSignalFrameChanged()
    {
        EmitSignal(SignalName.FrameChanged);
    }

    /// <summary>
    /// <para>Emitted when the animation loops.</para>
    /// </summary>
    public event Action AnimationLooped
    {
        add => Connect(SignalName.AnimationLooped, Callable.From(value));
        remove => Disconnect(SignalName.AnimationLooped, Callable.From(value));
    }

    protected void EmitSignalAnimationLooped()
    {
        EmitSignal(SignalName.AnimationLooped);
    }

    /// <summary>
    /// <para>Emitted when the animation reaches the end, or the start if it is played in reverse. When the animation finishes, it pauses the playback.</para>
    /// <para><b>Note:</b> This signal is not emitted if an animation is looping.</para>
    /// </summary>
    public event Action AnimationFinished
    {
        add => Connect(SignalName.AnimationFinished, Callable.From(value));
        remove => Disconnect(SignalName.AnimationFinished, Callable.From(value));
    }

    protected void EmitSignalAnimationFinished()
    {
        EmitSignal(SignalName.AnimationFinished);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_sprite_frames_changed = "SpriteFramesChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_animation_changed = "AnimationChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_frame_changed = "FrameChanged";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_animation_looped = "AnimationLooped";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_animation_finished = "AnimationFinished";

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
        if (signal == SignalName.SpriteFramesChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_sprite_frames_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.AnimationChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_animation_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.FrameChanged)
        {
            if (HasKaizenClassSignal(SignalProxyName_frame_changed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.AnimationLooped)
        {
            if (HasKaizenClassSignal(SignalProxyName_animation_looped.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.AnimationFinished)
        {
            if (HasKaizenClassSignal(SignalProxyName_animation_finished.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : SpriteBase3D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'sprite_frames' property.
        /// </summary>
        public static readonly StringName SpriteFrames = "sprite_frames";
        /// <summary>
        /// Cached name for the 'animation' property.
        /// </summary>
        public static readonly StringName Animation = "animation";
        /// <summary>
        /// Cached name for the 'autoplay' property.
        /// </summary>
        public static readonly StringName Autoplay = "autoplay";
        /// <summary>
        /// Cached name for the 'frame' property.
        /// </summary>
        public static readonly StringName Frame = "frame";
        /// <summary>
        /// Cached name for the 'frame_progress' property.
        /// </summary>
        public static readonly StringName FrameProgress = "frame_progress";
        /// <summary>
        /// Cached name for the 'speed_scale' property.
        /// </summary>
        public static readonly StringName SpeedScale = "speed_scale";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : SpriteBase3D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_sprite_frames' method.
        /// </summary>
        public static readonly StringName SetSpriteFrames = "set_sprite_frames";
        /// <summary>
        /// Cached name for the 'get_sprite_frames' method.
        /// </summary>
        public static readonly StringName GetSpriteFrames = "get_sprite_frames";
        /// <summary>
        /// Cached name for the 'set_animation' method.
        /// </summary>
        public static readonly StringName SetAnimation = "set_animation";
        /// <summary>
        /// Cached name for the 'get_animation' method.
        /// </summary>
        public static readonly StringName GetAnimation = "get_animation";
        /// <summary>
        /// Cached name for the 'set_autoplay' method.
        /// </summary>
        public static readonly StringName SetAutoplay = "set_autoplay";
        /// <summary>
        /// Cached name for the 'get_autoplay' method.
        /// </summary>
        public static readonly StringName GetAutoplay = "get_autoplay";
        /// <summary>
        /// Cached name for the 'is_playing' method.
        /// </summary>
        public static readonly StringName IsPlaying = "is_playing";
        /// <summary>
        /// Cached name for the 'play' method.
        /// </summary>
        public static readonly StringName Play = "play";
        /// <summary>
        /// Cached name for the 'play_backwards' method.
        /// </summary>
        public static readonly StringName PlayBackwards = "play_backwards";
        /// <summary>
        /// Cached name for the 'pause' method.
        /// </summary>
        public static readonly StringName Pause = "pause";
        /// <summary>
        /// Cached name for the 'stop' method.
        /// </summary>
        public static readonly StringName Stop = "stop";
        /// <summary>
        /// Cached name for the 'set_frame' method.
        /// </summary>
        public static readonly StringName SetFrame = "set_frame";
        /// <summary>
        /// Cached name for the 'get_frame' method.
        /// </summary>
        public static readonly StringName GetFrame = "get_frame";
        /// <summary>
        /// Cached name for the 'set_frame_progress' method.
        /// </summary>
        public static readonly StringName SetFrameProgress = "set_frame_progress";
        /// <summary>
        /// Cached name for the 'get_frame_progress' method.
        /// </summary>
        public static readonly StringName GetFrameProgress = "get_frame_progress";
        /// <summary>
        /// Cached name for the 'set_frame_and_progress' method.
        /// </summary>
        public static readonly StringName SetFrameAndProgress = "set_frame_and_progress";
        /// <summary>
        /// Cached name for the 'set_speed_scale' method.
        /// </summary>
        public static readonly StringName SetSpeedScale = "set_speed_scale";
        /// <summary>
        /// Cached name for the 'get_speed_scale' method.
        /// </summary>
        public static readonly StringName GetSpeedScale = "get_speed_scale";
        /// <summary>
        /// Cached name for the 'get_playing_speed' method.
        /// </summary>
        public static readonly StringName GetPlayingSpeed = "get_playing_speed";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : SpriteBase3D.SignalName
    {
        /// <summary>
        /// Cached name for the 'sprite_frames_changed' signal.
        /// </summary>
        public static readonly StringName SpriteFramesChanged = "sprite_frames_changed";
        /// <summary>
        /// Cached name for the 'animation_changed' signal.
        /// </summary>
        public static readonly StringName AnimationChanged = "animation_changed";
        /// <summary>
        /// Cached name for the 'frame_changed' signal.
        /// </summary>
        public static readonly StringName FrameChanged = "frame_changed";
        /// <summary>
        /// Cached name for the 'animation_looped' signal.
        /// </summary>
        public static readonly StringName AnimationLooped = "animation_looped";
        /// <summary>
        /// Cached name for the 'animation_finished' signal.
        /// </summary>
        public static readonly StringName AnimationFinished = "animation_finished";
    }
}
