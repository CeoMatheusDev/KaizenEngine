namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Once added to the scene tree and enabled using <see cref="Kaizen.AudioListener2D.MakeCurrent()"/>, this node will override the location sounds are heard from. Only one <see cref="Kaizen.AudioListener2D"/> can be current. Using <see cref="Kaizen.AudioListener2D.MakeCurrent()"/> will disable the previous <see cref="Kaizen.AudioListener2D"/>.</para>
/// <para>If there is no active <see cref="Kaizen.AudioListener2D"/> in the current <see cref="Kaizen.Viewport"/>, center of the screen will be used as a hearing point for the audio. <see cref="Kaizen.AudioListener2D"/> needs to be inside <see cref="Kaizen.SceneTree"/> to function.</para>
/// </summary>
public partial class AudioListener2D : Node2D
{
    private static readonly System.Type CachedType = typeof(AudioListener2D);

    private static readonly StringName NativeName = "AudioListener2D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public AudioListener2D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal AudioListener2D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal AudioListener2D(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.MakeCurrent, 3218959716ul);

    /// <summary>
    /// <para>Makes the <see cref="Kaizen.AudioListener2D"/> active, setting it as the hearing point for the sounds. If there is already another active <see cref="Kaizen.AudioListener2D"/>, it will be disabled.</para>
    /// <para>This method will have no effect if the <see cref="Kaizen.AudioListener2D"/> is not added to <see cref="Kaizen.SceneTree"/>.</para>
    /// </summary>
    public void MakeCurrent()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearCurrent, 3218959716ul);

    /// <summary>
    /// <para>Disables the <see cref="Kaizen.AudioListener2D"/>. If it's not set as current, this method will have no effect.</para>
    /// </summary>
    public void ClearCurrent()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCurrent, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this <see cref="Kaizen.AudioListener2D"/> is currently active.</para>
    /// </summary>
    public bool IsCurrent()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind2, KaizenObject.GetPtr(this)).ToBool();
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
    public new class PropertyName : Node2D.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Node2D.MethodName
    {
        /// <summary>
        /// Cached name for the 'make_current' method.
        /// </summary>
        public static readonly StringName MakeCurrent = "make_current";
        /// <summary>
        /// Cached name for the 'clear_current' method.
        /// </summary>
        public static readonly StringName ClearCurrent = "clear_current";
        /// <summary>
        /// Cached name for the 'is_current' method.
        /// </summary>
        public static readonly StringName IsCurrent = "is_current";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Node2D.SignalName
    {
    }
}
