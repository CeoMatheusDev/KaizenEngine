namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This class can be used to implement custom profilers that are able to interact with the engine and editor debugger.</para>
/// <para>See <see cref="Kaizen.EngineDebugger"/> and <c>EditorDebuggerPlugin</c> for more information.</para>
/// </summary>
public partial class EngineProfiler : RefCounted
{
    private static readonly System.Type CachedType = typeof(EngineProfiler);

    private static readonly StringName NativeName = "EngineProfiler";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public EngineProfiler() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal EngineProfiler(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal EngineProfiler(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Called when data is added to profiler using <see cref="Kaizen.EngineDebugger.ProfilerAddFrameData(StringName, Kaizen.Collections.Array)"/>.</para>
    /// </summary>
    public virtual void _AddFrame(Kaizen.Collections.Array data)
    {
    }

    /// <summary>
    /// <para>Called once every engine iteration when the profiler is active with information about the current frame. All time values are in seconds. Lower values represent faster processing times and are therefore considered better.</para>
    /// </summary>
    public virtual void _Tick(double frameTime, double processTime, double physicsTime, double physicsFrameTime)
    {
    }

    /// <summary>
    /// <para>Called when the profiler is enabled/disabled, along with a set of <paramref name="options"/>.</para>
    /// </summary>
    public virtual void _Toggle(bool enable, Kaizen.Collections.Array options)
    {
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__add_frame = "_AddFrame";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__tick = "_Tick";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__toggle = "_Toggle";

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
        if ((method == MethodProxyName__add_frame || method == MethodName._AddFrame) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__add_frame.NativeValue))
        {
            _AddFrame(VariantUtils.ConvertTo<Kaizen.Collections.Array>(args[0]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__tick || method == MethodName._Tick) && args.Count == 4 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__tick.NativeValue))
        {
            _Tick(VariantUtils.ConvertTo<double>(args[0]), VariantUtils.ConvertTo<double>(args[1]), VariantUtils.ConvertTo<double>(args[2]), VariantUtils.ConvertTo<double>(args[3]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__toggle || method == MethodName._Toggle) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__toggle.NativeValue))
        {
            _Toggle(VariantUtils.ConvertTo<bool>(args[0]), VariantUtils.ConvertTo<Kaizen.Collections.Array>(args[1]));
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
        if (method == MethodName._AddFrame)
        {
            if (HasKaizenClassMethod(MethodProxyName__add_frame.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._Tick)
        {
            if (HasKaizenClassMethod(MethodProxyName__tick.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._Toggle)
        {
            if (HasKaizenClassMethod(MethodProxyName__toggle.NativeValue.DangerousSelfRef))
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
        /// Cached name for the '_add_frame' method.
        /// </summary>
        public static readonly StringName _AddFrame = "_add_frame";
        /// <summary>
        /// Cached name for the '_tick' method.
        /// </summary>
        public static readonly StringName _Tick = "_tick";
        /// <summary>
        /// Cached name for the '_toggle' method.
        /// </summary>
        public static readonly StringName _Toggle = "_toggle";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
