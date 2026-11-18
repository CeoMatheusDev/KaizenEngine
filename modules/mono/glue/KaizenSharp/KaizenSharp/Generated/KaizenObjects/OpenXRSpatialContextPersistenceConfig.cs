namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Configuration header for spatial persistence. Pass this to <see cref="Kaizen.OpenXRSpatialEntityExtension.CreateSpatialContext(Kaizen.Collections.Array{OpenXRSpatialCapabilityConfigurationBaseHeader}, OpenXRStructureBase, Callable, Callable)"/> as the next parameter to create a spatial context with spatial persistence capabilities.</para>
/// </summary>
public partial class OpenXRSpatialContextPersistenceConfig : OpenXRStructureBase
{
    private static readonly System.Type CachedType = typeof(OpenXRSpatialContextPersistenceConfig);

    private static readonly StringName NativeName = "OpenXRSpatialContextPersistenceConfig";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public OpenXRSpatialContextPersistenceConfig() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal OpenXRSpatialContextPersistenceConfig(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal OpenXRSpatialContextPersistenceConfig(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddPersistenceContext, 2722037293ul);

    /// <summary>
    /// <para>Adds a persistence context to this configuration. You must add at least one persistence context to create a valid configuration. You can create a persistence context by calling <see cref="Kaizen.OpenXRSpatialAnchorCapability.CreatePersistenceContext(OpenXRSpatialAnchorCapability.PersistenceScope, Callable)"/>.</para>
    /// </summary>
    public void AddPersistenceContext(Rid persistenceContext)
    {
        NativeCalls.kaizen_icall_1_63(MethodBind0, KaizenObject.GetPtr(this), persistenceContext);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RemovePersistenceContext, 2722037293ul);

    /// <summary>
    /// <para>Removes a persistence context.</para>
    /// </summary>
    public void RemovePersistenceContext(Rid persistenceContext)
    {
        NativeCalls.kaizen_icall_1_63(MethodBind1, KaizenObject.GetPtr(this), persistenceContext);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPersistenceContexts, 3995934104ul);

    /// <summary>
    /// <para>Gets the persistence context(s) (as <see cref="Kaizen.Rid"/>s) received by <see cref="Kaizen.OpenXRSpatialContextPersistenceConfig.AddPersistenceContext(Rid)"/>.</para>
    /// </summary>
    public Kaizen.Collections.Array GetPersistenceContexts()
    {
        Kaizen.Collections.Array ret = NativeCalls.kaizen_icall_0_146(MethodBind2, KaizenObject.GetPtr(this));
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
    public new class PropertyName : OpenXRStructureBase.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : OpenXRStructureBase.MethodName
    {
        /// <summary>
        /// Cached name for the 'add_persistence_context' method.
        /// </summary>
        public static readonly StringName AddPersistenceContext = "add_persistence_context";
        /// <summary>
        /// Cached name for the 'remove_persistence_context' method.
        /// </summary>
        public static readonly StringName RemovePersistenceContext = "remove_persistence_context";
        /// <summary>
        /// Cached name for the 'get_persistence_contexts' method.
        /// </summary>
        public static readonly StringName GetPersistenceContexts = "get_persistence_contexts";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : OpenXRStructureBase.SignalName
    {
    }
}
