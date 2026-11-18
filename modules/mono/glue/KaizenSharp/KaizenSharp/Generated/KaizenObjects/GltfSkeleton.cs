namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
[KaizenClassName("GLTFSkeleton")]
public partial class GltfSkeleton : Resource
{
    public int[] Joints
    {
        get
        {
            return GetJoints();
        }
        set
        {
            SetJoints(value);
        }
    }

    public int[] Roots
    {
        get
        {
            return GetRoots();
        }
        set
        {
            SetRoots(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<string> UniqueNames
    {
        get
        {
            return GetUniqueNames();
        }
        set
        {
            SetUniqueNames(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Dictionary KaizenBoneNode
    {
        get
        {
            return GetKaizenBoneNode();
        }
        set
        {
            SetKaizenBoneNode(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    private static readonly System.Type CachedType = typeof(GltfSkeleton);

    private static readonly StringName NativeName = "GLTFSkeleton";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public GltfSkeleton() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfSkeleton(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfSkeleton(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetJoints, 969006518ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int[] GetJoints()
    {
        int[] ret = NativeCalls.kaizen_icall_0_227(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetJoints, 3614634198ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetJoints(int[] joints)
    {
        NativeCalls.kaizen_icall_1_213(MethodBind1, KaizenObject.GetPtr(this), joints);
        global::System.GC.KeepAlive(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetJoints(ReadOnlySpan<int> joints)
    {
        NativeCalls.kaizen_icall_1_213(MethodBind1, KaizenObject.GetPtr(this), joints);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRoots, 969006518ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int[] GetRoots()
    {
        int[] ret = NativeCalls.kaizen_icall_0_227(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRoots, 3614634198ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRoots(int[] roots)
    {
        NativeCalls.kaizen_icall_1_213(MethodBind3, KaizenObject.GetPtr(this), roots);
        global::System.GC.KeepAlive(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRoots(ReadOnlySpan<int> roots)
    {
        NativeCalls.kaizen_icall_1_213(MethodBind3, KaizenObject.GetPtr(this), roots);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKaizenSkeleton, 1814733083ul);

    public Skeleton3D GetKaizenSkeleton()
    {
        Skeleton3D ret = (Skeleton3D)NativeCalls.kaizen_icall_0_53(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUniqueNames, 2915620761ul);

    public Kaizen.Collections.Array<string> GetUniqueNames()
    {
        Kaizen.Collections.Array<string> ret = new Kaizen.Collections.Array<string>(NativeCalls.kaizen_icall_0_146(MethodBind5, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUniqueNames, 381264803ul);

    public void SetUniqueNames(Kaizen.Collections.Array<string> uniqueNames)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind6, KaizenObject.GetPtr(this), (kaizen_array)(uniqueNames ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKaizenBoneNode, 2382534195ul);

    /// <summary>
    /// <para>Returns a <see cref="Kaizen.Collections.Dictionary"/> that maps skeleton bone indices to the indices of glTF nodes. This property is unused during import, and only set during export. In a glTF file, a bone is a node, so Kaizen converts skeleton bones to glTF nodes.</para>
    /// </summary>
    public Kaizen.Collections.Dictionary GetKaizenBoneNode()
    {
        Kaizen.Collections.Dictionary ret = NativeCalls.kaizen_icall_0_148(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetKaizenBoneNode, 4155329257ul);

    /// <summary>
    /// <para>Sets a <see cref="Kaizen.Collections.Dictionary"/> that maps skeleton bone indices to the indices of glTF nodes. This property is unused during import, and only set during export. In a glTF file, a bone is a node, so Kaizen converts skeleton bones to glTF nodes.</para>
    /// </summary>
    public void SetKaizenBoneNode(Kaizen.Collections.Dictionary kaizenBoneNode)
    {
        NativeCalls.kaizen_icall_1_147(MethodBind8, KaizenObject.GetPtr(this), (kaizen_dictionary)(kaizenBoneNode ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBoneAttachmentCount, 2455072627ul);

    public int GetBoneAttachmentCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBoneAttachment, 945440495ul);

    public BoneAttachment3D GetBoneAttachment(int idx)
    {
        BoneAttachment3D ret = (BoneAttachment3D)NativeCalls.kaizen_icall_1_365(MethodBind10, KaizenObject.GetPtr(this), idx);
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
        /// Cached name for the 'joints' property.
        /// </summary>
        public static readonly StringName Joints = "joints";
        /// <summary>
        /// Cached name for the 'roots' property.
        /// </summary>
        public static readonly StringName Roots = "roots";
        /// <summary>
        /// Cached name for the 'unique_names' property.
        /// </summary>
        public static readonly StringName UniqueNames = "unique_names";
        /// <summary>
        /// Cached name for the 'kaizen_bone_node' property.
        /// </summary>
        public static readonly StringName KaizenBoneNode = "kaizen_bone_node";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'get_joints' method.
        /// </summary>
        public static readonly StringName GetJoints = "get_joints";
        /// <summary>
        /// Cached name for the 'set_joints' method.
        /// </summary>
        public static readonly StringName SetJoints = "set_joints";
        /// <summary>
        /// Cached name for the 'get_roots' method.
        /// </summary>
        public static readonly StringName GetRoots = "get_roots";
        /// <summary>
        /// Cached name for the 'set_roots' method.
        /// </summary>
        public static readonly StringName SetRoots = "set_roots";
        /// <summary>
        /// Cached name for the 'get_kaizen_skeleton' method.
        /// </summary>
        public static readonly StringName GetKaizenSkeleton = "get_kaizen_skeleton";
        /// <summary>
        /// Cached name for the 'get_unique_names' method.
        /// </summary>
        public static readonly StringName GetUniqueNames = "get_unique_names";
        /// <summary>
        /// Cached name for the 'set_unique_names' method.
        /// </summary>
        public static readonly StringName SetUniqueNames = "set_unique_names";
        /// <summary>
        /// Cached name for the 'get_kaizen_bone_node' method.
        /// </summary>
        public static readonly StringName GetKaizenBoneNode = "get_kaizen_bone_node";
        /// <summary>
        /// Cached name for the 'set_kaizen_bone_node' method.
        /// </summary>
        public static readonly StringName SetKaizenBoneNode = "set_kaizen_bone_node";
        /// <summary>
        /// Cached name for the 'get_bone_attachment_count' method.
        /// </summary>
        public static readonly StringName GetBoneAttachmentCount = "get_bone_attachment_count";
        /// <summary>
        /// Cached name for the 'get_bone_attachment' method.
        /// </summary>
        public static readonly StringName GetBoneAttachment = "get_bone_attachment";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
