namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>KaizenInstance represents a running Kaizen instance that is controlled from an outside codebase, without a perpetual main loop. It is created by the C API <c>libkaizen_create_kaizen_instance</c>. Only one may be created per process.</para>
/// </summary>
public partial class KaizenInstance : KaizenObject
{
    private static readonly System.Type CachedType = typeof(KaizenInstance);

    private static readonly StringName NativeName = "KaizenInstance";

    internal KaizenInstance() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal KaizenInstance(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal KaizenInstance(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Start, 2240911060ul);

    /// <summary>
    /// <para>Finishes this instance's startup sequence. Returns <see langword="true"/> on success.</para>
    /// </summary>
    public bool Start()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind0, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsStarted, 2240911060ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if this instance has been fully started.</para>
    /// </summary>
    public bool IsStarted()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind1, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Iteration, 2240911060ul);

    /// <summary>
    /// <para>Runs a single iteration of the main loop. Returns <see langword="true"/> if the engine is attempting to quit.</para>
    /// </summary>
    public bool Iteration()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind2, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.FocusIn, 3218959716ul);

    /// <summary>
    /// <para>Notifies the instance that it is now in focus.</para>
    /// </summary>
    public void FocusIn()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.FocusOut, 3218959716ul);

    /// <summary>
    /// <para>Notifies the instance that it is now not in focus.</para>
    /// </summary>
    public void FocusOut()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Pause, 3218959716ul);

    /// <summary>
    /// <para>Notifies the instance that it is going to be paused.</para>
    /// </summary>
    public void Pause()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Resume, 3218959716ul);

    /// <summary>
    /// <para>Notifies the instance that it is being resumed.</para>
    /// </summary>
    public void Resume()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind6, KaizenObject.GetPtr(this));
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
        /// Cached name for the 'start' method.
        /// </summary>
        public static readonly StringName Start = "start";
        /// <summary>
        /// Cached name for the 'is_started' method.
        /// </summary>
        public static readonly StringName IsStarted = "is_started";
        /// <summary>
        /// Cached name for the 'iteration' method.
        /// </summary>
        public static readonly StringName Iteration = "iteration";
        /// <summary>
        /// Cached name for the 'focus_in' method.
        /// </summary>
        public static readonly StringName FocusIn = "focus_in";
        /// <summary>
        /// Cached name for the 'focus_out' method.
        /// </summary>
        public static readonly StringName FocusOut = "focus_out";
        /// <summary>
        /// Cached name for the 'pause' method.
        /// </summary>
        public static readonly StringName Pause = "pause";
        /// <summary>
        /// Cached name for the 'resume' method.
        /// </summary>
        public static readonly StringName Resume = "resume";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
