namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A capsule shape collision that interacts with <see cref="Kaizen.SpringBoneSimulator3D"/>.</para>
/// </summary>
public partial class SpringBoneCollisionCapsule3D : SpringBoneCollision3D
{
    /// <summary>
    /// <para>The capsule's radius.</para>
    /// <para><b>Note:</b> The <see cref="Kaizen.SpringBoneCollisionCapsule3D.Radius"/> of a capsule cannot be greater than half of its <see cref="Kaizen.SpringBoneCollisionCapsule3D.Height"/>. Otherwise, the capsule becomes a sphere. If the <see cref="Kaizen.SpringBoneCollisionCapsule3D.Radius"/> is greater than half of the <see cref="Kaizen.SpringBoneCollisionCapsule3D.Height"/>, the properties adjust to a valid value.</para>
    /// </summary>
    public float Radius
    {
        get
        {
            return GetRadius();
        }
        set
        {
            SetRadius(value);
        }
    }

    /// <summary>
    /// <para>The capsule's full height, including the hemispheres.</para>
    /// <para><b>Note:</b> The <see cref="Kaizen.SpringBoneCollisionCapsule3D.Height"/> of a capsule must be at least twice its <see cref="Kaizen.SpringBoneCollisionCapsule3D.Radius"/>. Otherwise, the capsule becomes a sphere. If the <see cref="Kaizen.SpringBoneCollisionCapsule3D.Height"/> is less than twice the <see cref="Kaizen.SpringBoneCollisionCapsule3D.Radius"/>, the properties adjust to a valid value.</para>
    /// </summary>
    public float Height
    {
        get
        {
            return GetHeight();
        }
        set
        {
            SetHeight(value);
        }
    }

    /// <summary>
    /// <para>The capsule's height, excluding the hemispheres. This is the height of the central cylindrical part in the middle of the capsule, and is the distance between the centers of the two hemispheres. This is a wrapper for <see cref="Kaizen.SpringBoneCollisionCapsule3D.Height"/>.</para>
    /// </summary>
    public float MidHeight
    {
        get
        {
            return GetMidHeight();
        }
        set
        {
            SetMidHeight(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the collision acts to trap the joint within the collision.</para>
    /// </summary>
    public bool Inside
    {
        get
        {
            return IsInside();
        }
        set
        {
            SetInside(value);
        }
    }

    private static readonly System.Type CachedType = typeof(SpringBoneCollisionCapsule3D);

    private static readonly StringName NativeName = "SpringBoneCollisionCapsule3D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public SpringBoneCollisionCapsule3D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal SpringBoneCollisionCapsule3D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal SpringBoneCollisionCapsule3D(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRadius, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRadius(float radius)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind0, KaizenObject.GetPtr(this), radius);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRadius, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetRadius()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetHeight, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetHeight(float height)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind2, KaizenObject.GetPtr(this), height);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHeight, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetHeight()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMidHeight, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMidHeight(float midHeight)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind4, KaizenObject.GetPtr(this), midHeight);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMidHeight, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetMidHeight()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetInside, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetInside(bool enabled)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind6, KaizenObject.GetPtr(this), enabled.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsInside, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsInside()
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
    public new class PropertyName : SpringBoneCollision3D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'radius' property.
        /// </summary>
        public static readonly StringName Radius = "radius";
        /// <summary>
        /// Cached name for the 'height' property.
        /// </summary>
        public static readonly StringName Height = "height";
        /// <summary>
        /// Cached name for the 'mid_height' property.
        /// </summary>
        public static readonly StringName MidHeight = "mid_height";
        /// <summary>
        /// Cached name for the 'inside' property.
        /// </summary>
        public static readonly StringName Inside = "inside";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : SpringBoneCollision3D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_radius' method.
        /// </summary>
        public static readonly StringName SetRadius = "set_radius";
        /// <summary>
        /// Cached name for the 'get_radius' method.
        /// </summary>
        public static readonly StringName GetRadius = "get_radius";
        /// <summary>
        /// Cached name for the 'set_height' method.
        /// </summary>
        public static readonly StringName SetHeight = "set_height";
        /// <summary>
        /// Cached name for the 'get_height' method.
        /// </summary>
        public static readonly StringName GetHeight = "get_height";
        /// <summary>
        /// Cached name for the 'set_mid_height' method.
        /// </summary>
        public static readonly StringName SetMidHeight = "set_mid_height";
        /// <summary>
        /// Cached name for the 'get_mid_height' method.
        /// </summary>
        public static readonly StringName GetMidHeight = "get_mid_height";
        /// <summary>
        /// Cached name for the 'set_inside' method.
        /// </summary>
        public static readonly StringName SetInside = "set_inside";
        /// <summary>
        /// Cached name for the 'is_inside' method.
        /// </summary>
        public static readonly StringName IsInside = "is_inside";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : SpringBoneCollision3D.SignalName
    {
    }
}
