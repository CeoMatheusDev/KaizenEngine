namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.VoxelGI"/>s are used to provide high-quality real-time indirect light and reflections to scenes. They precompute the effect of objects that emit light and the effect of static geometry to simulate the behavior of complex light in real-time. <see cref="Kaizen.VoxelGI"/>s need to be baked before having a visible effect. However, once baked, dynamic objects will receive light from them. Furthermore, lights can be fully dynamic or baked.</para>
/// <para><b>Note:</b> <see cref="Kaizen.VoxelGI"/> is only supported in the Forward+ rendering method, not Mobile or Compatibility.</para>
/// <para><b>Procedural generation:</b> <see cref="Kaizen.VoxelGI"/> can be baked in an exported project, which makes it suitable for procedurally generated or user-built levels as long as all the geometry is generated in advance. For games where geometry is generated at any time during gameplay, SDFGI is more suitable (see <see cref="Kaizen.Environment.SdfgiEnabled"/>).</para>
/// <para><b>Performance:</b> <see cref="Kaizen.VoxelGI"/> is relatively demanding on the GPU and is not suited to low-end hardware such as integrated graphics (consider <see cref="Kaizen.LightmapGI"/> instead). To improve performance, adjust <c>ProjectSettings.rendering/global_illumination/voxel_gi/quality</c> and enable <c>ProjectSettings.rendering/global_illumination/gi/use_half_resolution</c> in the Project Settings. To provide a fallback for low-end hardware, consider adding an option to disable <see cref="Kaizen.VoxelGI"/> in your project's options menus. A <see cref="Kaizen.VoxelGI"/> node can be disabled by hiding it.</para>
/// <para><b>Note:</b> Meshes should have sufficiently thick walls to avoid light leaks (avoid one-sided walls). For interior levels, enclose your level geometry in a sufficiently large box and bridge the loops to close the mesh. To further prevent light leaks, you can also strategically place temporary <see cref="Kaizen.MeshInstance3D"/> nodes with their <see cref="Kaizen.GeometryInstance3D.GIMode"/> set to <see cref="Kaizen.GeometryInstance3D.GIModeEnum.Static"/>. These temporary nodes can then be hidden after baking the <see cref="Kaizen.VoxelGI"/> node.</para>
/// </summary>
public partial class VoxelGI : VisualInstance3D
{
    public enum SubdivEnum : long
    {
        /// <summary>
        /// <para>Use 64 subdivisions. This is the lowest quality setting, but the fastest. Use it if you can, but especially use it on lower-end hardware.</para>
        /// </summary>
        Subdiv64 = 0,
        /// <summary>
        /// <para>Use 128 subdivisions. This is the default quality setting.</para>
        /// </summary>
        Subdiv128 = 1,
        /// <summary>
        /// <para>Use 256 subdivisions.</para>
        /// </summary>
        Subdiv256 = 2,
        /// <summary>
        /// <para>Use 512 subdivisions. This is the highest quality setting, but the slowest. On lower-end hardware, this could cause the GPU to stall.</para>
        /// </summary>
        Subdiv512 = 3,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.VoxelGI.SubdivEnum"/> enum.</para>
        /// </summary>
        Max = 4
    }

    /// <summary>
    /// <para>Number of times to subdivide the grid that the <see cref="Kaizen.VoxelGI"/> operates on. A higher number results in finer detail and thus higher visual quality, while lower numbers result in better performance.</para>
    /// </summary>
    public VoxelGI.SubdivEnum Subdiv
    {
        get
        {
            return GetSubdiv();
        }
        set
        {
            SetSubdiv(value);
        }
    }

    /// <summary>
    /// <para>The size of the area covered by the <see cref="Kaizen.VoxelGI"/>. This must be <c>1.0</c> or greater on each axis.</para>
    /// <para><b>Note:</b> If you make the size larger without increasing the number of subdivisions with <see cref="Kaizen.VoxelGI.Subdiv"/>, the size of each cell will increase and result in less detailed lighting.</para>
    /// </summary>
    public Vector3 Size
    {
        get
        {
            return GetSize();
        }
        set
        {
            SetSize(value);
        }
    }

    /// <summary>
    /// <para>The <see cref="Kaizen.CameraAttributes"/> resource that specifies exposure levels to bake at. Auto-exposure and non exposure properties will be ignored. Exposure settings should be used to reduce the dynamic range present when baking. If exposure is too high, the <see cref="Kaizen.VoxelGI"/> will have banding artifacts or may have over-exposure artifacts.</para>
    /// </summary>
    public CameraAttributes CameraAttributes
    {
        get
        {
            return GetCameraAttributes();
        }
        set
        {
            SetCameraAttributes(value);
        }
    }

    /// <summary>
    /// <para>The <see cref="Kaizen.VoxelGIData"/> resource that holds the data for this <see cref="Kaizen.VoxelGI"/>.</para>
    /// </summary>
    public VoxelGIData Data
    {
        get
        {
            return GetProbeData();
        }
        set
        {
            SetProbeData(value);
        }
    }

    private static readonly System.Type CachedType = typeof(VoxelGI);

    private static readonly StringName NativeName = "VoxelGI";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public VoxelGI() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal VoxelGI(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal VoxelGI(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetProbeData, 1637849675ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetProbeData(VoxelGIData data)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(data));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetProbeData, 1730645405ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public VoxelGIData GetProbeData()
    {
        VoxelGIData ret = (VoxelGIData)NativeCalls.kaizen_icall_0_87(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSubdiv, 2240898472ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSubdiv(VoxelGI.SubdivEnum subdiv)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)subdiv);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSubdiv, 4261647950ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public VoxelGI.SubdivEnum GetSubdiv()
    {
        VoxelGI.SubdivEnum ret = (VoxelGI.SubdivEnum)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSize, 3460891852ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetSize(Vector3 size)
    {
        NativeCalls.kaizen_icall_1_203(MethodBind4, KaizenObject.GetPtr(this), &size);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSize, 3360562783ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Vector3 GetSize()
    {
        Vector3 ret = NativeCalls.kaizen_icall_0_151(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCameraAttributes, 2817810567ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCameraAttributes(CameraAttributes cameraAttributes)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind6, KaizenObject.GetPtr(this), KaizenObject.GetPtr(cameraAttributes));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCameraAttributes, 3921283215ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public CameraAttributes GetCameraAttributes()
    {
        CameraAttributes ret = (CameraAttributes)NativeCalls.kaizen_icall_0_87(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Bake, 2781551026ul);

    /// <summary>
    /// <para>Bakes the effect from all <see cref="Kaizen.GeometryInstance3D"/>s marked with <see cref="Kaizen.GeometryInstance3D.GIModeEnum.Static"/> and <see cref="Kaizen.Light3D"/>s marked with either <see cref="Kaizen.Light3D.BakeMode.Static"/> or <see cref="Kaizen.Light3D.BakeMode.Dynamic"/>. If <paramref name="createVisualDebug"/> is <see langword="true"/>, after baking the light, this will generate a <see cref="Kaizen.MultiMesh"/> that has a cube representing each solid cell with each cube colored to the cell's albedo color. This can be used to visualize the <see cref="Kaizen.VoxelGI"/>'s data and debug any issues that may be occurring.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.VoxelGI.Bake(Node, bool)"/> works from the editor and in exported projects. This makes it suitable for procedurally generated or user-built levels. Baking a <see cref="Kaizen.VoxelGI"/> node generally takes from 5 to 20 seconds in most scenes. Reducing <see cref="Kaizen.VoxelGI.Subdiv"/> can speed up baking.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.GeometryInstance3D"/>s and <see cref="Kaizen.Light3D"/>s must be fully ready before <see cref="Kaizen.VoxelGI.Bake(Node, bool)"/> is called. If you are procedurally creating those and some meshes or lights are missing from your baked <see cref="Kaizen.VoxelGI"/>, use <c>call_deferred("bake")</c> instead of calling <see cref="Kaizen.VoxelGI.Bake(Node, bool)"/> directly.</para>
    /// </summary>
    public void Bake(Node fromNode = default, bool createVisualDebug = false)
    {
        NativeCalls.kaizen_icall_2_526(MethodBind8, KaizenObject.GetPtr(this), KaizenObject.GetPtr(fromNode), createVisualDebug.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.DebugBake, 3218959716ul);

    /// <summary>
    /// <para>Calls <see cref="Kaizen.VoxelGI.Bake(Node, bool)"/> with <c>create_visual_debug</c> enabled.</para>
    /// </summary>
    public void DebugBake()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind9, KaizenObject.GetPtr(this));
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
    public new class PropertyName : VisualInstance3D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'subdiv' property.
        /// </summary>
        public static readonly StringName Subdiv = "subdiv";
        /// <summary>
        /// Cached name for the 'size' property.
        /// </summary>
        public static readonly StringName Size = "size";
        /// <summary>
        /// Cached name for the 'camera_attributes' property.
        /// </summary>
        public static readonly StringName CameraAttributes = "camera_attributes";
        /// <summary>
        /// Cached name for the 'data' property.
        /// </summary>
        public static readonly StringName Data = "data";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : VisualInstance3D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_probe_data' method.
        /// </summary>
        public static readonly StringName SetProbeData = "set_probe_data";
        /// <summary>
        /// Cached name for the 'get_probe_data' method.
        /// </summary>
        public static readonly StringName GetProbeData = "get_probe_data";
        /// <summary>
        /// Cached name for the 'set_subdiv' method.
        /// </summary>
        public static readonly StringName SetSubdiv = "set_subdiv";
        /// <summary>
        /// Cached name for the 'get_subdiv' method.
        /// </summary>
        public static readonly StringName GetSubdiv = "get_subdiv";
        /// <summary>
        /// Cached name for the 'set_size' method.
        /// </summary>
        public static readonly StringName SetSize = "set_size";
        /// <summary>
        /// Cached name for the 'get_size' method.
        /// </summary>
        public static readonly StringName GetSize = "get_size";
        /// <summary>
        /// Cached name for the 'set_camera_attributes' method.
        /// </summary>
        public static readonly StringName SetCameraAttributes = "set_camera_attributes";
        /// <summary>
        /// Cached name for the 'get_camera_attributes' method.
        /// </summary>
        public static readonly StringName GetCameraAttributes = "get_camera_attributes";
        /// <summary>
        /// Cached name for the 'bake' method.
        /// </summary>
        public static readonly StringName Bake = "bake";
        /// <summary>
        /// Cached name for the 'debug_bake' method.
        /// </summary>
        public static readonly StringName DebugBake = "debug_bake";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : VisualInstance3D.SignalName
    {
    }
}
