namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
public partial class PhysicsServer3DRenderingServerHandler : KaizenObject
{
    private static readonly System.Type CachedType = typeof(PhysicsServer3DRenderingServerHandler);

    private static readonly StringName NativeName = "PhysicsServer3DRenderingServerHandler";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public PhysicsServer3DRenderingServerHandler() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal PhysicsServer3DRenderingServerHandler(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal PhysicsServer3DRenderingServerHandler(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Called by the <see cref="Kaizen.PhysicsServer3D"/> to set the bounding box for the <see cref="Kaizen.SoftBody3D"/>.</para>
    /// </summary>
    public virtual unsafe void _SetAabb(Aabb aabb)
    {
    }

    /// <summary>
    /// <para>Called by the <see cref="Kaizen.PhysicsServer3D"/> to set the normal for the <see cref="Kaizen.SoftBody3D"/> vertex at the index specified by <paramref name="vertexId"/>.</para>
    /// <para><b>Note:</b> The <paramref name="normal"/> parameter used to be of type <c>const void*</c> prior to Kaizen 4.2.</para>
    /// </summary>
    public virtual unsafe void _SetNormal(int vertexId, Vector3 normal)
    {
    }

    /// <summary>
    /// <para>Called by the <see cref="Kaizen.PhysicsServer3D"/> to set the position for the <see cref="Kaizen.SoftBody3D"/> vertex at the index specified by <paramref name="vertexId"/>.</para>
    /// <para><b>Note:</b> The <paramref name="vertex"/> parameter used to be of type <c>const void*</c> prior to Kaizen 4.2.</para>
    /// </summary>
    public virtual unsafe void _SetVertex(int vertexId, Vector3 vertex)
    {
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVertex, 1530502735ul);

    /// <summary>
    /// <para>Sets the position for the <see cref="Kaizen.SoftBody3D"/> vertex at the index specified by <paramref name="vertexId"/>.</para>
    /// </summary>
    public unsafe void SetVertex(int vertexId, Vector3 vertex)
    {
        NativeCalls.kaizen_icall_2_247(MethodBind0, KaizenObject.GetPtr(this), vertexId, &vertex);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetNormal, 1530502735ul);

    /// <summary>
    /// <para>Sets the normal for the <see cref="Kaizen.SoftBody3D"/> vertex at the index specified by <paramref name="vertexId"/>.</para>
    /// </summary>
    public unsafe void SetNormal(int vertexId, Vector3 normal)
    {
        NativeCalls.kaizen_icall_2_247(MethodBind1, KaizenObject.GetPtr(this), vertexId, &normal);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAabb, 259215842ul);

    /// <summary>
    /// <para>Sets the bounding box for the <see cref="Kaizen.SoftBody3D"/>.</para>
    /// </summary>
    public unsafe void SetAabb(Aabb aabb)
    {
        NativeCalls.kaizen_icall_1_208(MethodBind2, KaizenObject.GetPtr(this), &aabb);
        global::System.GC.KeepAlive(this);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__set_aabb = "_SetAabb";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__set_normal = "_SetNormal";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__set_vertex = "_SetVertex";

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
        if ((method == MethodProxyName__set_aabb || method == MethodName._SetAabb) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__set_aabb.NativeValue))
        {
            _SetAabb(VariantUtils.ConvertTo<Aabb>(args[0]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__set_normal || method == MethodName._SetNormal) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__set_normal.NativeValue))
        {
            _SetNormal(VariantUtils.ConvertTo<int>(args[0]), VariantUtils.ConvertTo<Vector3>(args[1]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__set_vertex || method == MethodName._SetVertex) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__set_vertex.NativeValue))
        {
            _SetVertex(VariantUtils.ConvertTo<int>(args[0]), VariantUtils.ConvertTo<Vector3>(args[1]));
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
        if (method == MethodName._SetAabb)
        {
            if (HasKaizenClassMethod(MethodProxyName__set_aabb.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._SetNormal)
        {
            if (HasKaizenClassMethod(MethodProxyName__set_normal.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._SetVertex)
        {
            if (HasKaizenClassMethod(MethodProxyName__set_vertex.NativeValue.DangerousSelfRef))
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
    public new class PropertyName : KaizenObject.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
        /// <summary>
        /// Cached name for the '_set_aabb' method.
        /// </summary>
        public static readonly StringName _SetAabb = "_set_aabb";
        /// <summary>
        /// Cached name for the '_set_normal' method.
        /// </summary>
        public static readonly StringName _SetNormal = "_set_normal";
        /// <summary>
        /// Cached name for the '_set_vertex' method.
        /// </summary>
        public static readonly StringName _SetVertex = "_set_vertex";
        /// <summary>
        /// Cached name for the 'set_vertex' method.
        /// </summary>
        public static readonly StringName SetVertex = "set_vertex";
        /// <summary>
        /// Cached name for the 'set_normal' method.
        /// </summary>
        public static readonly StringName SetNormal = "set_normal";
        /// <summary>
        /// Cached name for the 'set_aabb' method.
        /// </summary>
        public static readonly StringName SetAabb = "set_aabb";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
