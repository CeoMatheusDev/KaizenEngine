namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.FogVolume"/>s are used to add localized fog into the global volumetric fog effect. <see cref="Kaizen.FogVolume"/>s can also remove volumetric fog from specific areas if using a <see cref="Kaizen.FogMaterial"/> with a negative <see cref="Kaizen.FogMaterial.Density"/>.</para>
/// <para>Performance of <see cref="Kaizen.FogVolume"/>s is directly related to their relative size on the screen and the complexity of their attached <see cref="Kaizen.FogMaterial"/>. It is best to keep <see cref="Kaizen.FogVolume"/>s relatively small and simple where possible.</para>
/// <para><b>Note:</b> <see cref="Kaizen.FogVolume"/>s only have a visible effect if <see cref="Kaizen.Environment.VolumetricFogEnabled"/> is <see langword="true"/>. If you don't want fog to be globally visible (but only within <see cref="Kaizen.FogVolume"/> nodes), set <see cref="Kaizen.Environment.VolumetricFogDensity"/> to <c>0.0</c>.</para>
/// </summary>
public partial class FogVolume : VisualInstance3D
{
    /// <summary>
    /// <para>The size of the <see cref="Kaizen.FogVolume"/> when <see cref="Kaizen.FogVolume.Shape"/> is <see cref="Kaizen.RenderingServer.FogVolumeShape.Ellipsoid"/>, <see cref="Kaizen.RenderingServer.FogVolumeShape.Cone"/>, <see cref="Kaizen.RenderingServer.FogVolumeShape.Cylinder"/> or <see cref="Kaizen.RenderingServer.FogVolumeShape.Box"/>.</para>
    /// <para><b>Note:</b> Thin fog volumes may appear to flicker when the camera moves or rotates. This can be alleviated by increasing <c>ProjectSettings.rendering/environment/volumetric_fog/volume_depth</c> (at a performance cost) or by decreasing <see cref="Kaizen.Environment.VolumetricFogLength"/> (at no performance cost, but at the cost of lower fog range). Alternatively, the <see cref="Kaizen.FogVolume"/> can be made thicker and use a lower density in the <see cref="Kaizen.FogVolume.Material"/>.</para>
    /// <para><b>Note:</b> If <see cref="Kaizen.FogVolume.Shape"/> is <see cref="Kaizen.RenderingServer.FogVolumeShape.Cone"/> or <see cref="Kaizen.RenderingServer.FogVolumeShape.Cylinder"/>, the cone/cylinder will be adjusted to fit within the size. Non-uniform scaling of cone/cylinder shapes via the <see cref="Kaizen.FogVolume.Size"/> property is not supported, but you can scale the <see cref="Kaizen.FogVolume"/> node instead.</para>
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
    /// <para>The shape of the <see cref="Kaizen.FogVolume"/>. This can be set to either <see cref="Kaizen.RenderingServer.FogVolumeShape.Ellipsoid"/>, <see cref="Kaizen.RenderingServer.FogVolumeShape.Cone"/>, <see cref="Kaizen.RenderingServer.FogVolumeShape.Cylinder"/>, <see cref="Kaizen.RenderingServer.FogVolumeShape.Box"/> or <see cref="Kaizen.RenderingServer.FogVolumeShape.World"/>.</para>
    /// </summary>
    public RenderingServer.FogVolumeShape Shape
    {
        get
        {
            return GetShape();
        }
        set
        {
            SetShape(value);
        }
    }

    /// <summary>
    /// <para>The <see cref="Kaizen.Material"/> used by the <see cref="Kaizen.FogVolume"/>. Can be either a built-in <see cref="Kaizen.FogMaterial"/> or a custom <see cref="Kaizen.ShaderMaterial"/>.</para>
    /// </summary>
    public Material Material
    {
        get
        {
            return GetMaterial();
        }
        set
        {
            SetMaterial(value);
        }
    }

    private static readonly System.Type CachedType = typeof(FogVolume);

    private static readonly StringName NativeName = "FogVolume";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public FogVolume() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal FogVolume(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal FogVolume(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSize, 3460891852ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetSize(Vector3 size)
    {
        NativeCalls.kaizen_icall_1_203(MethodBind0, KaizenObject.GetPtr(this), &size);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSize, 3360562783ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Vector3 GetSize()
    {
        Vector3 ret = NativeCalls.kaizen_icall_0_151(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetShape, 1416323362ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetShape(RenderingServer.FogVolumeShape shape)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)shape);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetShape, 3920334604ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingServer.FogVolumeShape GetShape()
    {
        RenderingServer.FogVolumeShape ret = (RenderingServer.FogVolumeShape)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMaterial, 2757459619ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMaterial(Material material)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind4, KaizenObject.GetPtr(this), KaizenObject.GetPtr(material));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMaterial, 5934680ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Material GetMaterial()
    {
        Material ret = (Material)NativeCalls.kaizen_icall_0_87(MethodBind5, KaizenObject.GetPtr(this));
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
    public new class PropertyName : VisualInstance3D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'size' property.
        /// </summary>
        public static readonly StringName Size = "size";
        /// <summary>
        /// Cached name for the 'shape' property.
        /// </summary>
        public static readonly StringName Shape = "shape";
        /// <summary>
        /// Cached name for the 'material' property.
        /// </summary>
        public static readonly StringName Material = "material";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : VisualInstance3D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_size' method.
        /// </summary>
        public static readonly StringName SetSize = "set_size";
        /// <summary>
        /// Cached name for the 'get_size' method.
        /// </summary>
        public static readonly StringName GetSize = "get_size";
        /// <summary>
        /// Cached name for the 'set_shape' method.
        /// </summary>
        public static readonly StringName SetShape = "set_shape";
        /// <summary>
        /// Cached name for the 'get_shape' method.
        /// </summary>
        public static readonly StringName GetShape = "get_shape";
        /// <summary>
        /// Cached name for the 'set_material' method.
        /// </summary>
        public static readonly StringName SetMaterial = "set_material";
        /// <summary>
        /// Cached name for the 'get_material' method.
        /// </summary>
        public static readonly StringName GetMaterial = "get_material";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : VisualInstance3D.SignalName
    {
    }
}
