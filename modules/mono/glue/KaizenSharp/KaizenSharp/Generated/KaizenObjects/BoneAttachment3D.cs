namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This node selects a bone in a <see cref="Kaizen.Skeleton3D"/> and attaches to it. This means that the <see cref="Kaizen.BoneAttachment3D"/> node will either dynamically copy or override the 3D transform of the selected bone.</para>
/// </summary>
public partial class BoneAttachment3D : Node3D
{
    /// <summary>
    /// <para>The name of the attached bone.</para>
    /// </summary>
    public string BoneName
    {
        get
        {
            return GetBoneName();
        }
        set
        {
            SetBoneName(value);
        }
    }

    /// <summary>
    /// <para>The index of the attached bone.</para>
    /// </summary>
    public int BoneIdx
    {
        get
        {
            return GetBoneIdx();
        }
        set
        {
            SetBoneIdx(value);
        }
    }

    /// <summary>
    /// <para>Whether the <see cref="Kaizen.BoneAttachment3D"/> node will override the bone pose of the bone it is attached to. When set to <see langword="true"/>, the <see cref="Kaizen.BoneAttachment3D"/> node can change the pose of the bone. When set to <see langword="false"/>, the <see cref="Kaizen.BoneAttachment3D"/> will always be set to the bone's transform.</para>
    /// <para><b>Note:</b> This override performs interruptively in the skeleton update process using signals due to the old design. It may cause unintended behavior when used at the same time with <see cref="Kaizen.SkeletonModifier3D"/>.</para>
    /// </summary>
    public bool OverridePose
    {
        get
        {
            return GetOverridePose();
        }
        set
        {
            SetOverridePose(value);
        }
    }

    /// <summary>
    /// <para>Whether the <see cref="Kaizen.BoneAttachment3D"/> node will use an external <see cref="Kaizen.Skeleton3D"/> node rather than attempting to use its parent node as the <see cref="Kaizen.Skeleton3D"/>. When set to <see langword="true"/>, the <see cref="Kaizen.BoneAttachment3D"/> node will use the external <see cref="Kaizen.Skeleton3D"/> node set in <see cref="Kaizen.BoneAttachment3D.ExternalSkeleton"/>.</para>
    /// </summary>
    public bool UseExternalSkeleton
    {
        get
        {
            return GetUseExternalSkeleton();
        }
        set
        {
            SetUseExternalSkeleton(value);
        }
    }

    /// <summary>
    /// <para>The <see cref="Kaizen.NodePath"/> to the external <see cref="Kaizen.Skeleton3D"/> node.</para>
    /// </summary>
    public NodePath ExternalSkeleton
    {
        get
        {
            return GetExternalSkeleton();
        }
        set
        {
            SetExternalSkeleton(value);
        }
    }

    private static readonly System.Type CachedType = typeof(BoneAttachment3D);

    private static readonly StringName NativeName = "BoneAttachment3D";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public BoneAttachment3D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal BoneAttachment3D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal BoneAttachment3D(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSkeleton, 1814733083ul);

    /// <summary>
    /// <para>Returns the parent or external <see cref="Kaizen.Skeleton3D"/> node if it exists, otherwise returns <see langword="null"/>.</para>
    /// </summary>
    public Skeleton3D GetSkeleton()
    {
        Skeleton3D ret = (Skeleton3D)NativeCalls.kaizen_icall_0_53(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBoneName, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBoneName(string boneName)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind1, KaizenObject.GetPtr(this), boneName);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBoneName, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetBoneName()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBoneIdx, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBoneIdx(int boneIdx)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind3, KaizenObject.GetPtr(this), boneIdx);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBoneIdx, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetBoneIdx()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.OnSkeletonUpdate, 3218959716ul);

    /// <summary>
    /// <para>A function that is called automatically when the <see cref="Kaizen.Skeleton3D"/> is updated. This function is where the <see cref="Kaizen.BoneAttachment3D"/> node updates its position so it is correctly bound when it is <i>not</i> set to override the bone pose.</para>
    /// </summary>
    public void OnSkeletonUpdate()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetOverridePose, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetOverridePose(bool overridePose)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind6, KaizenObject.GetPtr(this), overridePose.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetOverridePose, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetOverridePose()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind7, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUseExternalSkeleton, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUseExternalSkeleton(bool useExternalSkeleton)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind8, KaizenObject.GetPtr(this), useExternalSkeleton.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUseExternalSkeleton, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetUseExternalSkeleton()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind9, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetExternalSkeleton, 1348162250ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetExternalSkeleton(NodePath externalSkeleton)
    {
        NativeCalls.kaizen_icall_1_149(MethodBind10, KaizenObject.GetPtr(this), (kaizen_node_path)(externalSkeleton?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetExternalSkeleton, 4075236667ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public NodePath GetExternalSkeleton()
    {
        NodePath ret = NativeCalls.kaizen_icall_0_150(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.OnBonePoseUpdate, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void OnBonePoseUpdate(int boneIndex)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind12, KaizenObject.GetPtr(this), boneIndex);
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
    public new class PropertyName : Node3D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'bone_name' property.
        /// </summary>
        public static readonly StringName BoneName = "bone_name";
        /// <summary>
        /// Cached name for the 'bone_idx' property.
        /// </summary>
        public static readonly StringName BoneIdx = "bone_idx";
        /// <summary>
        /// Cached name for the 'override_pose' property.
        /// </summary>
        public static readonly StringName OverridePose = "override_pose";
        /// <summary>
        /// Cached name for the 'use_external_skeleton' property.
        /// </summary>
        public static readonly StringName UseExternalSkeleton = "use_external_skeleton";
        /// <summary>
        /// Cached name for the 'external_skeleton' property.
        /// </summary>
        public static readonly StringName ExternalSkeleton = "external_skeleton";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Node3D.MethodName
    {
        /// <summary>
        /// Cached name for the 'get_skeleton' method.
        /// </summary>
        public static readonly StringName GetSkeleton = "get_skeleton";
        /// <summary>
        /// Cached name for the 'set_bone_name' method.
        /// </summary>
        public static readonly StringName SetBoneName = "set_bone_name";
        /// <summary>
        /// Cached name for the 'get_bone_name' method.
        /// </summary>
        public static readonly StringName GetBoneName = "get_bone_name";
        /// <summary>
        /// Cached name for the 'set_bone_idx' method.
        /// </summary>
        public static readonly StringName SetBoneIdx = "set_bone_idx";
        /// <summary>
        /// Cached name for the 'get_bone_idx' method.
        /// </summary>
        public static readonly StringName GetBoneIdx = "get_bone_idx";
        /// <summary>
        /// Cached name for the 'on_skeleton_update' method.
        /// </summary>
        public static readonly StringName OnSkeletonUpdate = "on_skeleton_update";
        /// <summary>
        /// Cached name for the 'set_override_pose' method.
        /// </summary>
        public static readonly StringName SetOverridePose = "set_override_pose";
        /// <summary>
        /// Cached name for the 'get_override_pose' method.
        /// </summary>
        public static readonly StringName GetOverridePose = "get_override_pose";
        /// <summary>
        /// Cached name for the 'set_use_external_skeleton' method.
        /// </summary>
        public static readonly StringName SetUseExternalSkeleton = "set_use_external_skeleton";
        /// <summary>
        /// Cached name for the 'get_use_external_skeleton' method.
        /// </summary>
        public static readonly StringName GetUseExternalSkeleton = "get_use_external_skeleton";
        /// <summary>
        /// Cached name for the 'set_external_skeleton' method.
        /// </summary>
        public static readonly StringName SetExternalSkeleton = "set_external_skeleton";
        /// <summary>
        /// Cached name for the 'get_external_skeleton' method.
        /// </summary>
        public static readonly StringName GetExternalSkeleton = "get_external_skeleton";
        /// <summary>
        /// Cached name for the 'on_bone_pose_update' method.
        /// </summary>
        public static readonly StringName OnBonePoseUpdate = "on_bone_pose_update";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Node3D.SignalName
    {
    }
}
