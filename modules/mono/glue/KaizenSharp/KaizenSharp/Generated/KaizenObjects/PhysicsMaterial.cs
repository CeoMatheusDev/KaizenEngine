namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Holds physics-related properties of a surface, namely its roughness and bounciness. This class is used to apply these properties to a physics body.</para>
/// </summary>
public partial class PhysicsMaterial : Resource
{
    /// <summary>
    /// <para>The body's friction. Values range from <c>0</c> (frictionless) to <c>1</c> (maximum friction).</para>
    /// </summary>
    public float Friction
    {
        get
        {
            return GetFriction();
        }
        set
        {
            SetFriction(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the physics engine will use the friction of the object marked as "rough" when two objects collide. If <see langword="false"/>, the physics engine will use the lowest friction of all colliding objects instead. If <see langword="true"/> for both colliding objects, the physics engine will use the highest friction.</para>
    /// </summary>
    public bool Rough
    {
        get
        {
            return IsRough();
        }
        set
        {
            SetRough(value);
        }
    }

    /// <summary>
    /// <para>The body's bounciness. Values range from <c>0</c> (no bounce) to <c>1</c> (full bounciness).</para>
    /// <para><b>Note:</b> Even with <see cref="Kaizen.PhysicsMaterial.Bounce"/> set to <c>1.0</c>, some energy will be lost over time due to linear and angular damping. To have a physics body that preserves all its energy over time, set <see cref="Kaizen.PhysicsMaterial.Bounce"/> to <c>1.0</c>, the body's linear damp mode to <b>Replace</b> (if applicable), its linear damp to <c>0.0</c>, its angular damp mode to <b>Replace</b> (if applicable), and its angular damp to <c>0.0</c>.</para>
    /// </summary>
    public float Bounce
    {
        get
        {
            return GetBounce();
        }
        set
        {
            SetBounce(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, subtracts the bounciness from the colliding object's bounciness instead of adding it.</para>
    /// </summary>
    public bool Absorbent
    {
        get
        {
            return IsAbsorbent();
        }
        set
        {
            SetAbsorbent(value);
        }
    }

    private static readonly System.Type CachedType = typeof(PhysicsMaterial);

    private static readonly StringName NativeName = "PhysicsMaterial";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public PhysicsMaterial() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal PhysicsMaterial(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal PhysicsMaterial(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFriction, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFriction(float friction)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind0, KaizenObject.GetPtr(this), friction);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFriction, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetFriction()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRough, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRough(bool rough)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind2, KaizenObject.GetPtr(this), rough.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsRough, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsRough()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind3, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBounce, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBounce(float bounce)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind4, KaizenObject.GetPtr(this), bounce);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBounce, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetBounce()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAbsorbent, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAbsorbent(bool absorbent)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind6, KaizenObject.GetPtr(this), absorbent.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsAbsorbent, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsAbsorbent()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind7, KaizenObject.GetPtr(this)).ToBool();
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
    public new class PropertyName : Resource.PropertyName
    {
        /// <summary>
        /// Cached name for the 'friction' property.
        /// </summary>
        public static readonly StringName Friction = "friction";
        /// <summary>
        /// Cached name for the 'rough' property.
        /// </summary>
        public static readonly StringName Rough = "rough";
        /// <summary>
        /// Cached name for the 'bounce' property.
        /// </summary>
        public static readonly StringName Bounce = "bounce";
        /// <summary>
        /// Cached name for the 'absorbent' property.
        /// </summary>
        public static readonly StringName Absorbent = "absorbent";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_friction' method.
        /// </summary>
        public static readonly StringName SetFriction = "set_friction";
        /// <summary>
        /// Cached name for the 'get_friction' method.
        /// </summary>
        public static readonly StringName GetFriction = "get_friction";
        /// <summary>
        /// Cached name for the 'set_rough' method.
        /// </summary>
        public static readonly StringName SetRough = "set_rough";
        /// <summary>
        /// Cached name for the 'is_rough' method.
        /// </summary>
        public static readonly StringName IsRough = "is_rough";
        /// <summary>
        /// Cached name for the 'set_bounce' method.
        /// </summary>
        public static readonly StringName SetBounce = "set_bounce";
        /// <summary>
        /// Cached name for the 'get_bounce' method.
        /// </summary>
        public static readonly StringName GetBounce = "get_bounce";
        /// <summary>
        /// Cached name for the 'set_absorbent' method.
        /// </summary>
        public static readonly StringName SetAbsorbent = "set_absorbent";
        /// <summary>
        /// Cached name for the 'is_absorbent' method.
        /// </summary>
        public static readonly StringName IsAbsorbent = "is_absorbent";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
