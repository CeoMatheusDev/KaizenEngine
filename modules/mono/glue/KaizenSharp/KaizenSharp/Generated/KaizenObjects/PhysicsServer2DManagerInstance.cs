namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.PhysicsServer2DManager"/> is the API for registering <see cref="Kaizen.PhysicsServer2D"/> implementations and for setting the default implementation.</para>
/// <para><b>Note:</b> It is not possible to switch physics servers at runtime. This class is only used on startup at the server initialization level, by Kaizen itself and possibly by GDExtensions.</para>
/// </summary>
[KaizenClassName("PhysicsServer2DManager")]
public partial class PhysicsServer2DManagerInstance : KaizenObject
{
    private static readonly System.Type CachedType = typeof(PhysicsServer2DManagerInstance);

    private static readonly StringName NativeName = "PhysicsServer2DManager";

    internal PhysicsServer2DManagerInstance() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal PhysicsServer2DManagerInstance(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal PhysicsServer2DManagerInstance(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RegisterServer, 2137474292ul);

    /// <summary>
    /// <para>Register a <see cref="Kaizen.PhysicsServer2D"/> implementation by passing a <paramref name="name"/> and a <see cref="Kaizen.Callable"/> that returns a <see cref="Kaizen.PhysicsServer2D"/> object.</para>
    /// </summary>
    public void RegisterServer(string name, Callable createCallback)
    {
        NativeCalls.kaizen_icall_2_537(MethodBind0, KaizenObject.GetPtr(this), name, createCallback);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetDefaultServer, 2956805083ul);

    /// <summary>
    /// <para>Set the default <see cref="Kaizen.PhysicsServer2D"/> implementation to the one identified by <paramref name="name"/>, if <paramref name="priority"/> is greater than the priority of the current default implementation.</para>
    /// </summary>
    public void SetDefaultServer(string name, int priority)
    {
        NativeCalls.kaizen_icall_2_428(MethodBind1, KaizenObject.GetPtr(this), name, priority);
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
        /// Cached name for the 'register_server' method.
        /// </summary>
        public static readonly StringName RegisterServer = "register_server";
        /// <summary>
        /// Cached name for the 'set_default_server' method.
        /// </summary>
        public static readonly StringName SetDefaultServer = "set_default_server";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
