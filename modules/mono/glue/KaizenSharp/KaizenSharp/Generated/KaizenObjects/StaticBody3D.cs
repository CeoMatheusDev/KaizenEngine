namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A static 3D physics body. It can't be moved by external forces or contacts, but can be moved manually by other means such as code, <see cref="Kaizen.AnimationMixer"/>s (with <see cref="Kaizen.AnimationMixer.CallbackModeProcess"/> set to <see cref="Kaizen.AnimationMixer.AnimationCallbackModeProcess.Physics"/>), and <see cref="Kaizen.RemoteTransform3D"/>.</para>
/// <para>When <see cref="Kaizen.StaticBody3D"/> is moved, it is teleported to its new position without affecting other physics bodies in its path. If this is not desired, use <see cref="Kaizen.AnimatableBody3D"/> instead.</para>
/// <para><see cref="Kaizen.StaticBody3D"/> is useful for completely static objects like floors and walls, as well as moving surfaces like conveyor belts and circular revolving platforms (by using <see cref="Kaizen.StaticBody3D.ConstantLinearVelocity"/> and <see cref="Kaizen.StaticBody3D.ConstantAngularVelocity"/>).</para>
/// </summary>
public partial class StaticBody3D : PhysicsBody3D
{
    /// <summary>
    /// <para>The physics material override for the body.</para>
    /// <para>If a material is assigned to this property, it will be used instead of any other physics material, such as an inherited one.</para>
    /// </summary>
    public PhysicsMaterial PhysicsMaterialOverride
    {
        get
        {
            return GetPhysicsMaterialOverride();
        }
        set
        {
            SetPhysicsMaterialOverride(value);
        }
    }

    /// <summary>
    /// <para>The body's constant linear velocity. This does not move the body, but affects touching bodies, as if it were moving.</para>
    /// </summary>
    public Vector3 ConstantLinearVelocity
    {
        get
        {
            return GetConstantLinearVelocity();
        }
        set
        {
            SetConstantLinearVelocity(value);
        }
    }

    /// <summary>
    /// <para>The body's constant angular velocity. This does not rotate the body, but affects touching bodies, as if it were rotating.</para>
    /// </summary>
    public Vector3 ConstantAngularVelocity
    {
        get
        {
            return GetConstantAngularVelocity();
        }
        set
        {
            SetConstantAngularVelocity(value);
        }
    }

    private static readonly System.Type CachedType = typeof(StaticBody3D);

    private static readonly StringName NativeName = "StaticBody3D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public StaticBody3D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal StaticBody3D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal StaticBody3D(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetConstantLinearVelocity, 3460891852ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetConstantLinearVelocity(Vector3 vel)
    {
        NativeCalls.kaizen_icall_1_203(MethodBind0, KaizenObject.GetPtr(this), &vel);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetConstantAngularVelocity, 3460891852ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetConstantAngularVelocity(Vector3 vel)
    {
        NativeCalls.kaizen_icall_1_203(MethodBind1, KaizenObject.GetPtr(this), &vel);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetConstantLinearVelocity, 3360562783ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Vector3 GetConstantLinearVelocity()
    {
        Vector3 ret = NativeCalls.kaizen_icall_0_151(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetConstantAngularVelocity, 3360562783ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Vector3 GetConstantAngularVelocity()
    {
        Vector3 ret = NativeCalls.kaizen_icall_0_151(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetPhysicsMaterialOverride, 1784508650ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetPhysicsMaterialOverride(PhysicsMaterial physicsMaterialOverride)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind4, KaizenObject.GetPtr(this), KaizenObject.GetPtr(physicsMaterialOverride));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPhysicsMaterialOverride, 2521850424ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public PhysicsMaterial GetPhysicsMaterialOverride()
    {
        PhysicsMaterial ret = (PhysicsMaterial)NativeCalls.kaizen_icall_0_87(MethodBind5, KaizenObject.GetPtr(this));
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
    public new class PropertyName : PhysicsBody3D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'physics_material_override' property.
        /// </summary>
        public static readonly StringName PhysicsMaterialOverride = "physics_material_override";
        /// <summary>
        /// Cached name for the 'constant_linear_velocity' property.
        /// </summary>
        public static readonly StringName ConstantLinearVelocity = "constant_linear_velocity";
        /// <summary>
        /// Cached name for the 'constant_angular_velocity' property.
        /// </summary>
        public static readonly StringName ConstantAngularVelocity = "constant_angular_velocity";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : PhysicsBody3D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_constant_linear_velocity' method.
        /// </summary>
        public static readonly StringName SetConstantLinearVelocity = "set_constant_linear_velocity";
        /// <summary>
        /// Cached name for the 'set_constant_angular_velocity' method.
        /// </summary>
        public static readonly StringName SetConstantAngularVelocity = "set_constant_angular_velocity";
        /// <summary>
        /// Cached name for the 'get_constant_linear_velocity' method.
        /// </summary>
        public static readonly StringName GetConstantLinearVelocity = "get_constant_linear_velocity";
        /// <summary>
        /// Cached name for the 'get_constant_angular_velocity' method.
        /// </summary>
        public static readonly StringName GetConstantAngularVelocity = "get_constant_angular_velocity";
        /// <summary>
        /// Cached name for the 'set_physics_material_override' method.
        /// </summary>
        public static readonly StringName SetPhysicsMaterialOverride = "set_physics_material_override";
        /// <summary>
        /// Cached name for the 'get_physics_material_override' method.
        /// </summary>
        public static readonly StringName GetPhysicsMaterialOverride = "get_physics_material_override";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : PhysicsBody3D.SignalName
    {
    }
}
