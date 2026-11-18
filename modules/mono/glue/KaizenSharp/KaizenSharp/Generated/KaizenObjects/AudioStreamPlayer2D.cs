namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Plays audio that is attenuated with distance to the listener.</para>
/// <para>By default, audio is heard from the screen center. This can be changed by adding an <see cref="Kaizen.AudioListener2D"/> node to the scene and enabling it by calling <see cref="Kaizen.AudioListener2D.MakeCurrent()"/> on it.</para>
/// <para>See also <see cref="Kaizen.AudioStreamPlayer"/> to play a sound non-positionally.</para>
/// <para><b>Note:</b> Hiding an <see cref="Kaizen.AudioStreamPlayer2D"/> node does not disable its audio output. To temporarily disable an <see cref="Kaizen.AudioStreamPlayer2D"/>'s audio output, set <see cref="Kaizen.AudioStreamPlayer2D.VolumeDb"/> to a very low value like <c>-100</c> (which isn't audible to human hearing).</para>
/// </summary>
public partial class AudioStreamPlayer2D : Node2D
{
    /// <summary>
    /// <para>The <see cref="Kaizen.AudioStream"/> object to be played.</para>
    /// </summary>
    public AudioStream Stream
    {
        get
        {
            return GetStream();
        }
        set
        {
            SetStream(value);
        }
    }

    /// <summary>
    /// <para>Base volume before attenuation, in decibels.</para>
    /// </summary>
    public float VolumeDb
    {
        get
        {
            return GetVolumeDb();
        }
        set
        {
            SetVolumeDb(value);
        }
    }

    /// <summary>
    /// <para>Base volume before attenuation, as a linear value.</para>
    /// <para><b>Note:</b> This member modifies <see cref="Kaizen.AudioStreamPlayer2D.VolumeDb"/> for convenience. The returned value is equivalent to the result of <c>@GlobalScope.db_to_linear</c> on <see cref="Kaizen.AudioStreamPlayer2D.VolumeDb"/>. Setting this member is equivalent to setting <see cref="Kaizen.AudioStreamPlayer2D.VolumeDb"/> to the result of <c>@GlobalScope.linear_to_db</c> on a value.</para>
    /// </summary>
    public float VolumeLinear
    {
        get
        {
            return GetVolumeLinear();
        }
        set
        {
            SetVolumeLinear(value);
        }
    }

    /// <summary>
    /// <para>The pitch and the tempo of the audio, as a multiplier of the audio sample's sample rate.</para>
    /// </summary>
    public float PitchScale
    {
        get
        {
            return GetPitchScale();
        }
        set
        {
            SetPitchScale(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, audio is playing or is queued to be played (see <see cref="Kaizen.AudioStreamPlayer2D.Play(float)"/>).</para>
    /// </summary>
    public bool Playing
    {
        get
        {
            return IsPlaying();
        }
        set
        {
            SetPlaying(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, audio plays when added to scene tree.</para>
    /// </summary>
    public bool Autoplay
    {
        get
        {
            return IsAutoplayEnabled();
        }
        set
        {
            SetAutoplay(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the playback is paused. You can resume it by setting <see cref="Kaizen.AudioStreamPlayer2D.StreamPaused"/> to <see langword="false"/>.</para>
    /// </summary>
    public bool StreamPaused
    {
        get
        {
            return GetStreamPaused();
        }
        set
        {
            SetStreamPaused(value);
        }
    }

    /// <summary>
    /// <para>Maximum distance from which audio is still hearable.</para>
    /// </summary>
    public float MaxDistance
    {
        get
        {
            return GetMaxDistance();
        }
        set
        {
            SetMaxDistance(value);
        }
    }

    /// <summary>
    /// <para>The volume is attenuated over distance with this as an exponent.</para>
    /// </summary>
    public float Attenuation
    {
        get
        {
            return GetAttenuation();
        }
        set
        {
            SetAttenuation(value);
        }
    }

    /// <summary>
    /// <para>The maximum number of sounds this node can play at the same time. Playing additional sounds after this value is reached will cut off the oldest sounds.</para>
    /// </summary>
    public int MaxPolyphony
    {
        get
        {
            return GetMaxPolyphony();
        }
        set
        {
            SetMaxPolyphony(value);
        }
    }

    /// <summary>
    /// <para>Scales the panning strength for this node by multiplying the base <c>ProjectSettings.audio/general/2d_panning_strength</c> with this factor. Higher values will pan audio from left to right more dramatically than lower values.</para>
    /// </summary>
    public float PanningStrength
    {
        get
        {
            return GetPanningStrength();
        }
        set
        {
            SetPanningStrength(value);
        }
    }

    /// <summary>
    /// <para>Bus on which this audio is playing.</para>
    /// <para><b>Note:</b> When setting this property, keep in mind that no validation is performed to see if the given name matches an existing bus. This is because audio bus layouts might be loaded after this property is set. If this given name can't be resolved at runtime, it will fall back to <c>"Master"</c>.</para>
    /// </summary>
    public StringName Bus
    {
        get
        {
            return GetBus();
        }
        set
        {
            SetBus(value);
        }
    }

    /// <summary>
    /// <para>Determines which <see cref="Kaizen.Area2D"/> layers affect the sound for reverb and audio bus effects. Areas can be used to redirect <see cref="Kaizen.AudioStream"/>s so that they play in a certain audio bus. An example of how you might use this is making a "water" area so that sounds played in the water are redirected through an audio bus to make them sound like they are being played underwater.</para>
    /// </summary>
    public uint AreaMask
    {
        get
        {
            return GetAreaMask();
        }
        set
        {
            SetAreaMask(value);
        }
    }

    /// <summary>
    /// <para>The playback type of the stream player. If set other than to the default value, it will force that playback type.</para>
    /// </summary>
    public AudioServer.PlaybackType PlaybackType
    {
        get
        {
            return GetPlaybackType();
        }
        set
        {
            SetPlaybackType(value);
        }
    }

    private static readonly System.Type CachedType = typeof(AudioStreamPlayer2D);

    private static readonly StringName NativeName = "AudioStreamPlayer2D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public AudioStreamPlayer2D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal AudioStreamPlayer2D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal AudioStreamPlayer2D(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetStream, 2210767741ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetStream(AudioStream stream)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(stream));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStream, 160907539ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public AudioStream GetStream()
    {
        AudioStream ret = (AudioStream)NativeCalls.kaizen_icall_0_87(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVolumeDb, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVolumeDb(float volumeDb)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind2, KaizenObject.GetPtr(this), volumeDb);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVolumeDb, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetVolumeDb()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVolumeLinear, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVolumeLinear(float volumeLinear)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind4, KaizenObject.GetPtr(this), volumeLinear);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVolumeLinear, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetVolumeLinear()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPitchScale, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPitchScale(float pitchScale)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind6, KaizenObject.GetPtr(this), pitchScale);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPitchScale, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetPitchScale()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Play, 1958160172ul);

    /// <summary>
    /// <para>Queues the audio to play on the next physics frame, from the given position <paramref name="fromPosition"/>, in seconds.</para>
    /// </summary>
    public void Play(float fromPosition = 0.0f)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind8, KaizenObject.GetPtr(this), fromPosition);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Seek, 373806689ul);

    /// <summary>
    /// <para>Sets the position from which audio will be played, in seconds.</para>
    /// </summary>
    public void Seek(float toPosition)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind9, KaizenObject.GetPtr(this), toPosition);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Stop, 3218959716ul);

    /// <summary>
    /// <para>Stops the audio.</para>
    /// </summary>
    public void Stop()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsPlaying, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsPlaying()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind11, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPlaybackPosition, 191475506ul);

    /// <summary>
    /// <para>Returns the position in the <see cref="Kaizen.AudioStream"/>.</para>
    /// </summary>
    public float GetPlaybackPosition()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind12, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBus, 3304788590ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBus(StringName bus)
    {
        NativeCalls.kaizen_icall_1_88(MethodBind13, KaizenObject.GetPtr(this), (kaizen_string_name)(bus?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBus, 2002593661ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringName GetBus()
    {
        StringName ret = NativeCalls.kaizen_icall_0_89(MethodBind14, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAutoplay, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAutoplay(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind15, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAutoplayEnabled, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsAutoplayEnabled()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind16, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPlaying, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPlaying(bool enable)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind17, KaizenObject.GetPtr(this), enable.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMaxDistance, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMaxDistance(float pixels)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind18, KaizenObject.GetPtr(this), pixels);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMaxDistance, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetMaxDistance()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind19, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAttenuation, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAttenuation(float curve)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind20, KaizenObject.GetPtr(this), curve);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAttenuation, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetAttenuation()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind21, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAreaMask, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAreaMask(uint mask)
    {
        NativeCalls.kaizen_icall_1_234(MethodBind22, KaizenObject.GetPtr(this), mask);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAreaMask, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public uint GetAreaMask()
    {
        uint ret = NativeCalls.kaizen_icall_0_235(MethodBind23, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetStreamPaused, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetStreamPaused(bool pause)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind24, KaizenObject.GetPtr(this), pause.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStreamPaused, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetStreamPaused()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind25, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMaxPolyphony, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMaxPolyphony(int maxPolyphony)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind26, KaizenObject.GetPtr(this), maxPolyphony);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMaxPolyphony, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetMaxPolyphony()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind27, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPanningStrength, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPanningStrength(float panningStrength)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind28, KaizenObject.GetPtr(this), panningStrength);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPanningStrength, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetPanningStrength()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind29, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasStreamPlayback, 2240911060ul);

    /// <summary>
    /// <para>Returns whether the <see cref="Kaizen.AudioStreamPlayer"/> can return the <see cref="Kaizen.AudioStreamPlayback"/> object or not.</para>
    /// </summary>
    public bool HasStreamPlayback()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind30, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStreamPlayback, 210135309ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.AudioStreamPlayback"/> object associated with this <see cref="Kaizen.AudioStreamPlayer2D"/>.</para>
    /// </summary>
    public AudioStreamPlayback GetStreamPlayback()
    {
        AudioStreamPlayback ret = (AudioStreamPlayback)NativeCalls.kaizen_icall_0_87(MethodBind31, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPlaybackType, 725473817ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPlaybackType(AudioServer.PlaybackType playbackType)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind32, KaizenObject.GetPtr(this), (int)playbackType);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPlaybackType, 4011264623ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public AudioServer.PlaybackType GetPlaybackType()
    {
        AudioServer.PlaybackType ret = (AudioServer.PlaybackType)NativeCalls.kaizen_icall_0_39(MethodBind33, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Emitted when the audio stops playing.</para>
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
    public new class PropertyName : Node2D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'stream' property.
        /// </summary>
        public static readonly StringName Stream = "stream";
        /// <summary>
        /// Cached name for the 'volume_db' property.
        /// </summary>
        public static readonly StringName VolumeDb = "volume_db";
        /// <summary>
        /// Cached name for the 'volume_linear' property.
        /// </summary>
        public static readonly StringName VolumeLinear = "volume_linear";
        /// <summary>
        /// Cached name for the 'pitch_scale' property.
        /// </summary>
        public static readonly StringName PitchScale = "pitch_scale";
        /// <summary>
        /// Cached name for the 'playing' property.
        /// </summary>
        public static readonly StringName Playing = "playing";
        /// <summary>
        /// Cached name for the 'autoplay' property.
        /// </summary>
        public static readonly StringName Autoplay = "autoplay";
        /// <summary>
        /// Cached name for the 'stream_paused' property.
        /// </summary>
        public static readonly StringName StreamPaused = "stream_paused";
        /// <summary>
        /// Cached name for the 'max_distance' property.
        /// </summary>
        public static readonly StringName MaxDistance = "max_distance";
        /// <summary>
        /// Cached name for the 'attenuation' property.
        /// </summary>
        public static readonly StringName Attenuation = "attenuation";
        /// <summary>
        /// Cached name for the 'max_polyphony' property.
        /// </summary>
        public static readonly StringName MaxPolyphony = "max_polyphony";
        /// <summary>
        /// Cached name for the 'panning_strength' property.
        /// </summary>
        public static readonly StringName PanningStrength = "panning_strength";
        /// <summary>
        /// Cached name for the 'bus' property.
        /// </summary>
        public static readonly StringName Bus = "bus";
        /// <summary>
        /// Cached name for the 'area_mask' property.
        /// </summary>
        public static readonly StringName AreaMask = "area_mask";
        /// <summary>
        /// Cached name for the 'playback_type' property.
        /// </summary>
        public static readonly StringName PlaybackType = "playback_type";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Node2D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_stream' method.
        /// </summary>
        public static readonly StringName SetStream = "set_stream";
        /// <summary>
        /// Cached name for the 'get_stream' method.
        /// </summary>
        public static readonly StringName GetStream = "get_stream";
        /// <summary>
        /// Cached name for the 'set_volume_db' method.
        /// </summary>
        public static readonly StringName SetVolumeDb = "set_volume_db";
        /// <summary>
        /// Cached name for the 'get_volume_db' method.
        /// </summary>
        public static readonly StringName GetVolumeDb = "get_volume_db";
        /// <summary>
        /// Cached name for the 'set_volume_linear' method.
        /// </summary>
        public static readonly StringName SetVolumeLinear = "set_volume_linear";
        /// <summary>
        /// Cached name for the 'get_volume_linear' method.
        /// </summary>
        public static readonly StringName GetVolumeLinear = "get_volume_linear";
        /// <summary>
        /// Cached name for the 'set_pitch_scale' method.
        /// </summary>
        public static readonly StringName SetPitchScale = "set_pitch_scale";
        /// <summary>
        /// Cached name for the 'get_pitch_scale' method.
        /// </summary>
        public static readonly StringName GetPitchScale = "get_pitch_scale";
        /// <summary>
        /// Cached name for the 'play' method.
        /// </summary>
        public static readonly StringName Play = "play";
        /// <summary>
        /// Cached name for the 'seek' method.
        /// </summary>
        public static readonly StringName Seek = "seek";
        /// <summary>
        /// Cached name for the 'stop' method.
        /// </summary>
        public static readonly StringName Stop = "stop";
        /// <summary>
        /// Cached name for the 'is_playing' method.
        /// </summary>
        public static readonly StringName IsPlaying = "is_playing";
        /// <summary>
        /// Cached name for the 'get_playback_position' method.
        /// </summary>
        public static readonly StringName GetPlaybackPosition = "get_playback_position";
        /// <summary>
        /// Cached name for the 'set_bus' method.
        /// </summary>
        public static readonly StringName SetBus = "set_bus";
        /// <summary>
        /// Cached name for the 'get_bus' method.
        /// </summary>
        public static readonly StringName GetBus = "get_bus";
        /// <summary>
        /// Cached name for the 'set_autoplay' method.
        /// </summary>
        public static readonly StringName SetAutoplay = "set_autoplay";
        /// <summary>
        /// Cached name for the 'is_autoplay_enabled' method.
        /// </summary>
        public static readonly StringName IsAutoplayEnabled = "is_autoplay_enabled";
        /// <summary>
        /// Cached name for the 'set_playing' method.
        /// </summary>
        public static readonly StringName SetPlaying = "set_playing";
        /// <summary>
        /// Cached name for the 'set_max_distance' method.
        /// </summary>
        public static readonly StringName SetMaxDistance = "set_max_distance";
        /// <summary>
        /// Cached name for the 'get_max_distance' method.
        /// </summary>
        public static readonly StringName GetMaxDistance = "get_max_distance";
        /// <summary>
        /// Cached name for the 'set_attenuation' method.
        /// </summary>
        public static readonly StringName SetAttenuation = "set_attenuation";
        /// <summary>
        /// Cached name for the 'get_attenuation' method.
        /// </summary>
        public static readonly StringName GetAttenuation = "get_attenuation";
        /// <summary>
        /// Cached name for the 'set_area_mask' method.
        /// </summary>
        public static readonly StringName SetAreaMask = "set_area_mask";
        /// <summary>
        /// Cached name for the 'get_area_mask' method.
        /// </summary>
        public static readonly StringName GetAreaMask = "get_area_mask";
        /// <summary>
        /// Cached name for the 'set_stream_paused' method.
        /// </summary>
        public static readonly StringName SetStreamPaused = "set_stream_paused";
        /// <summary>
        /// Cached name for the 'get_stream_paused' method.
        /// </summary>
        public static readonly StringName GetStreamPaused = "get_stream_paused";
        /// <summary>
        /// Cached name for the 'set_max_polyphony' method.
        /// </summary>
        public static readonly StringName SetMaxPolyphony = "set_max_polyphony";
        /// <summary>
        /// Cached name for the 'get_max_polyphony' method.
        /// </summary>
        public static readonly StringName GetMaxPolyphony = "get_max_polyphony";
        /// <summary>
        /// Cached name for the 'set_panning_strength' method.
        /// </summary>
        public static readonly StringName SetPanningStrength = "set_panning_strength";
        /// <summary>
        /// Cached name for the 'get_panning_strength' method.
        /// </summary>
        public static readonly StringName GetPanningStrength = "get_panning_strength";
        /// <summary>
        /// Cached name for the 'has_stream_playback' method.
        /// </summary>
        public static readonly StringName HasStreamPlayback = "has_stream_playback";
        /// <summary>
        /// Cached name for the 'get_stream_playback' method.
        /// </summary>
        public static readonly StringName GetStreamPlayback = "get_stream_playback";
        /// <summary>
        /// Cached name for the 'set_playback_type' method.
        /// </summary>
        public static readonly StringName SetPlaybackType = "set_playback_type";
        /// <summary>
        /// Cached name for the 'get_playback_type' method.
        /// </summary>
        public static readonly StringName GetPlaybackType = "get_playback_type";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Node2D.SignalName
    {
        /// <summary>
        /// Cached name for the 'finished' signal.
        /// </summary>
        public static readonly StringName Finished = "finished";
    }
}
