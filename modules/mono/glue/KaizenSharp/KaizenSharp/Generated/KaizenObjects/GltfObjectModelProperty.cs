namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>GLTFObjectModelProperty defines a mapping between a property in the glTF object model and a NodePath in the Kaizen scene tree. This can be used to animate properties in a glTF file using the <c>KHR_animation_pointer</c> extension, or to access them through an engine-agnostic script such as a behavior graph as defined by the <c>KHR_interactivity</c> extension.</para>
/// <para>The glTF property is identified by JSON pointer(s) stored in <see cref="Kaizen.GltfObjectModelProperty.JsonPointers"/>, while the Kaizen property it maps to is defined by <see cref="Kaizen.GltfObjectModelProperty.NodePaths"/>. In most cases <see cref="Kaizen.GltfObjectModelProperty.JsonPointers"/> and <see cref="Kaizen.GltfObjectModelProperty.NodePaths"/> will each only have one item, but in some cases a single glTF JSON pointer will map to multiple Kaizen properties, or a single Kaizen property will be mapped to multiple glTF JSON pointers, or it might be a many-to-many relationship.</para>
/// <para><see cref="Kaizen.Expression"/> objects can be used to define conversions between the data, such as when glTF defines an angle in radians and Kaizen uses degrees. The <see cref="Kaizen.GltfObjectModelProperty.ObjectModelType"/> property defines the type of data stored in the glTF file as defined by the object model, see <see cref="Kaizen.GltfObjectModelProperty.GltfObjectModelType"/> for possible values.</para>
/// </summary>
[KaizenClassName("GLTFObjectModelProperty")]
public partial class GltfObjectModelProperty : RefCounted
{
    public enum GltfObjectModelType : long
    {
        /// <summary>
        /// <para>Unknown or not set object model type. If the object model type is set to this value, the real type still needs to be determined.</para>
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// <para>Object model type "bool". Represented in the glTF JSON as a boolean, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "SCALAR". When encoded in an accessor, a value of <c>0</c> is <see langword="false"/>, and any other value is <see langword="true"/>.</para>
        /// </summary>
        Bool = 1,
        /// <summary>
        /// <para>Object model type "float". Represented in the glTF JSON as a number, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "SCALAR".</para>
        /// </summary>
        Float = 2,
        /// <summary>
        /// <para>Object model type "float[lb][rb]". Represented in the glTF JSON as an array of numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "SCALAR".</para>
        /// </summary>
        FloatArray = 3,
        /// <summary>
        /// <para>Object model type "float2". Represented in the glTF JSON as an array of two numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "VEC2".</para>
        /// </summary>
        Float2 = 4,
        /// <summary>
        /// <para>Object model type "float3". Represented in the glTF JSON as an array of three numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "VEC3".</para>
        /// </summary>
        Float3 = 5,
        /// <summary>
        /// <para>Object model type "float4". Represented in the glTF JSON as an array of four numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "VEC4".</para>
        /// </summary>
        Float4 = 6,
        /// <summary>
        /// <para>Object model type "float2x2". Represented in the glTF JSON as an array of four numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "MAT2".</para>
        /// </summary>
        Float2X2 = 7,
        /// <summary>
        /// <para>Object model type "float3x3". Represented in the glTF JSON as an array of nine numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "MAT3".</para>
        /// </summary>
        Float3X3 = 8,
        /// <summary>
        /// <para>Object model type "float4x4". Represented in the glTF JSON as an array of sixteen numbers, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "MAT4".</para>
        /// </summary>
        Float4X4 = 9,
        /// <summary>
        /// <para>Object model type "int". Represented in the glTF JSON as a number, and encoded in a <see cref="Kaizen.GltfAccessor"/> as "SCALAR". The range of values is limited to signed integers. For <c>KHR_interactivity</c>, only 32-bit integers are supported.</para>
        /// </summary>
        Int = 10
    }

    /// <summary>
    /// <para>If set, this <see cref="Kaizen.Expression"/> will be used to convert the property value from the glTF object model to the value expected by the Kaizen property. This is useful when the glTF object model uses a different unit system, or when the data needs to be transformed in some way. If <see langword="null"/>, the value will be copied as-is.</para>
    /// </summary>
    public Expression GltfToKaizenExpression
    {
        get
        {
            return GetGltfToKaizenExpression();
        }
        set
        {
            SetGltfToKaizenExpression(value);
        }
    }

    /// <summary>
    /// <para>If set, this <see cref="Kaizen.Expression"/> will be used to convert the property value from the Kaizen property to the value expected by the glTF object model. This is useful when the glTF object model uses a different unit system, or when the data needs to be transformed in some way. If <see langword="null"/>, the value will be copied as-is.</para>
    /// </summary>
    public Expression KaizenToGltfExpression
    {
        get
        {
            return GetKaizenToGltfExpression();
        }
        set
        {
            SetKaizenToGltfExpression(value);
        }
    }

    /// <summary>
    /// <para>An array of <see cref="Kaizen.NodePath"/>s that point to a property, or multiple properties, in the Kaizen scene tree. On import, this will either be set by <see cref="Kaizen.GltfDocument"/>, or by a <see cref="Kaizen.GltfDocumentExtension"/> class. For simple cases, use <see cref="Kaizen.GltfObjectModelProperty.AppendPathToProperty(NodePath, StringName)"/> to add properties to this array.</para>
    /// <para>In most cases <see cref="Kaizen.GltfObjectModelProperty.NodePaths"/> will only have one item, but in some cases a single glTF JSON pointer will map to multiple Kaizen properties. For example, a <see cref="Kaizen.GltfCamera"/> or <see cref="Kaizen.GltfLight"/> used on multiple glTF nodes will be represented by multiple Kaizen nodes.</para>
    /// </summary>
    public Kaizen.Collections.Array<NodePath> NodePaths
    {
        get
        {
            return GetNodePaths();
        }
        set
        {
            SetNodePaths(value);
        }
    }

    /// <summary>
    /// <para>The type of data stored in the glTF file as defined by the object model. This is a superset of the available accessor types, and determines the accessor type.</para>
    /// </summary>
    public GltfObjectModelProperty.GltfObjectModelType ObjectModelType
    {
        get
        {
            return GetObjectModelType();
        }
        set
        {
            SetObjectModelType(value);
        }
    }

    /// <summary>
    /// <para>The glTF object model JSON pointers used to identify the property in the glTF object model. In most cases, there will be only one item in this array, but specific cases may require multiple pointers. The items are themselves arrays which represent the JSON pointer split into its components.</para>
    /// </summary>
    public Kaizen.Collections.Array<string[]> JsonPointers
    {
        get
        {
            return GetJsonPointers();
        }
        set
        {
            SetJsonPointers(value);
        }
    }

    /// <summary>
    /// <para>The type of data stored in the Kaizen property. This is the type of the property that the <see cref="Kaizen.GltfObjectModelProperty.NodePaths"/> point to.</para>
    /// </summary>
    public Variant.Type VariantType
    {
        get
        {
            return GetVariantType();
        }
        set
        {
            SetVariantType(value);
        }
    }

    private static readonly System.Type CachedType = typeof(GltfObjectModelProperty);

    private static readonly StringName NativeName = "GLTFObjectModelProperty";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public GltfObjectModelProperty() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfObjectModelProperty(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfObjectModelProperty(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendNodePath, 1348162250ul);

    /// <summary>
    /// <para>Appends a <see cref="Kaizen.NodePath"/> to <see cref="Kaizen.GltfObjectModelProperty.NodePaths"/>. This can be used by <see cref="Kaizen.GltfDocumentExtension"/> classes to define how a glTF object model property maps to a Kaizen property, or multiple Kaizen properties. Prefer using <see cref="Kaizen.GltfObjectModelProperty.AppendPathToProperty(NodePath, StringName)"/> for simple cases. Be sure to also call <see cref="Kaizen.GltfObjectModelProperty.SetTypes(Variant.Type, GltfObjectModelProperty.GltfObjectModelType)"/> once (the order does not matter).</para>
    /// </summary>
    public void AppendNodePath(NodePath nodePath)
    {
        NativeCalls.kaizen_icall_1_149(MethodBind0, KaizenObject.GetPtr(this), (kaizen_node_path)(nodePath?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendPathToProperty, 1331931644ul);

    /// <summary>
    /// <para>High-level wrapper over <see cref="Kaizen.GltfObjectModelProperty.AppendNodePath(NodePath)"/> that handles the most common cases. It constructs a new <see cref="Kaizen.NodePath"/> using <paramref name="nodePath"/> as a base and appends <paramref name="propName"/> to the subpath. Be sure to also call <see cref="Kaizen.GltfObjectModelProperty.SetTypes(Variant.Type, GltfObjectModelProperty.GltfObjectModelType)"/> once (the order does not matter).</para>
    /// </summary>
    public void AppendPathToProperty(NodePath nodePath, StringName propName)
    {
        NativeCalls.kaizen_icall_2_681(MethodBind1, KaizenObject.GetPtr(this), (kaizen_node_path)(nodePath?.NativeValue ?? default), (kaizen_string_name)(propName?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAccessorType, 1998183368ul);

    /// <summary>
    /// <para>The GLTF accessor type associated with this property's <see cref="Kaizen.GltfObjectModelProperty.ObjectModelType"/>. See <see cref="Kaizen.GltfAccessor.AccessorType"/> for possible values, and see <see cref="Kaizen.GltfObjectModelProperty.GltfObjectModelType"/> for how the object model type maps to accessor types.</para>
    /// </summary>
    public GltfAccessor.GltfAccessorType GetAccessorType()
    {
        GltfAccessor.GltfAccessorType ret = (GltfAccessor.GltfAccessorType)NativeCalls.kaizen_icall_0_39(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetGltfToKaizenExpression, 2240072449ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Expression GetGltfToKaizenExpression()
    {
        Expression ret = (Expression)NativeCalls.kaizen_icall_0_87(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetGltfToKaizenExpression, 1815845073ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetGltfToKaizenExpression(Expression gltfToKaizenExpr)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind4, KaizenObject.GetPtr(this), KaizenObject.GetPtr(gltfToKaizenExpr));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetKaizenToGltfExpression, 2240072449ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Expression GetKaizenToGltfExpression()
    {
        Expression ret = (Expression)NativeCalls.kaizen_icall_0_87(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetKaizenToGltfExpression, 1815845073ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetKaizenToGltfExpression(Expression kaizenToGltfExpr)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind6, KaizenObject.GetPtr(this), KaizenObject.GetPtr(kaizenToGltfExpr));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNodePaths, 3995934104ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Array<NodePath> GetNodePaths()
    {
        Kaizen.Collections.Array<NodePath> ret = new Kaizen.Collections.Array<NodePath>(NativeCalls.kaizen_icall_0_146(MethodBind7, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasNodePaths, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <see cref="Kaizen.GltfObjectModelProperty.NodePaths"/> is not empty. This is used during import to determine if a <see cref="Kaizen.GltfObjectModelProperty"/> can handle converting a glTF object model property to a Kaizen property.</para>
    /// </summary>
    public bool HasNodePaths()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind8, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetNodePaths, 381264803ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetNodePaths(Kaizen.Collections.Array<NodePath> nodePaths)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind9, KaizenObject.GetPtr(this), (kaizen_array)(nodePaths ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetObjectModelType, 1094778507ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public GltfObjectModelProperty.GltfObjectModelType GetObjectModelType()
    {
        GltfObjectModelProperty.GltfObjectModelType ret = (GltfObjectModelProperty.GltfObjectModelType)NativeCalls.kaizen_icall_0_39(MethodBind10, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetObjectModelType, 4108684086ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetObjectModelType(GltfObjectModelProperty.GltfObjectModelType type)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind11, KaizenObject.GetPtr(this), (int)type);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetJsonPointers, 3995934104ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Array<string[]> GetJsonPointers()
    {
        Kaizen.Collections.Array<string[]> ret = new Kaizen.Collections.Array<string[]>(NativeCalls.kaizen_icall_0_146(MethodBind12, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.HasJsonPointers, 36873697ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if <see cref="Kaizen.GltfObjectModelProperty.JsonPointers"/> is not empty. This is used during export to determine if a <see cref="Kaizen.GltfObjectModelProperty"/> can handle converting a Kaizen property to a glTF object model property.</para>
    /// </summary>
    public bool HasJsonPointers()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind13, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetJsonPointers, 381264803ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetJsonPointers(Kaizen.Collections.Array<string[]> jsonPointers)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind14, KaizenObject.GetPtr(this), (kaizen_array)(jsonPointers ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVariantType, 3416842102ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Variant.Type GetVariantType()
    {
        Variant.Type ret = (Variant.Type)NativeCalls.kaizen_icall_0_39(MethodBind15, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVariantType, 2887708385ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVariantType(Variant.Type variantType)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind16, KaizenObject.GetPtr(this), (int)variantType);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTypes, 4150728237ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfObjectModelProperty.VariantType"/> and <see cref="Kaizen.GltfObjectModelProperty.ObjectModelType"/> properties. This is a convenience method to set both properties at once, since they are almost always known at the same time. This method should be called once. Calling it again with the same values will have no effect.</para>
    /// </summary>
    public void SetTypes(Variant.Type variantType, GltfObjectModelProperty.GltfObjectModelType objModelType)
    {
        NativeCalls.kaizen_icall_2_84(MethodBind17, KaizenObject.GetPtr(this), (int)variantType, (int)objModelType);
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
    public new class PropertyName : RefCounted.PropertyName
    {
        /// <summary>
        /// Cached name for the 'gltf_to_kaizen_expression' property.
        /// </summary>
        public static readonly StringName GltfToKaizenExpression = "gltf_to_kaizen_expression";
        /// <summary>
        /// Cached name for the 'kaizen_to_gltf_expression' property.
        /// </summary>
        public static readonly StringName KaizenToGltfExpression = "kaizen_to_gltf_expression";
        /// <summary>
        /// Cached name for the 'node_paths' property.
        /// </summary>
        public static readonly StringName NodePaths = "node_paths";
        /// <summary>
        /// Cached name for the 'object_model_type' property.
        /// </summary>
        public static readonly StringName ObjectModelType = "object_model_type";
        /// <summary>
        /// Cached name for the 'json_pointers' property.
        /// </summary>
        public static readonly StringName JsonPointers = "json_pointers";
        /// <summary>
        /// Cached name for the 'variant_type' property.
        /// </summary>
        public static readonly StringName VariantType = "variant_type";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the 'append_node_path' method.
        /// </summary>
        public static readonly StringName AppendNodePath = "append_node_path";
        /// <summary>
        /// Cached name for the 'append_path_to_property' method.
        /// </summary>
        public static readonly StringName AppendPathToProperty = "append_path_to_property";
        /// <summary>
        /// Cached name for the 'get_accessor_type' method.
        /// </summary>
        public static readonly StringName GetAccessorType = "get_accessor_type";
        /// <summary>
        /// Cached name for the 'get_gltf_to_kaizen_expression' method.
        /// </summary>
        public static readonly StringName GetGltfToKaizenExpression = "get_gltf_to_kaizen_expression";
        /// <summary>
        /// Cached name for the 'set_gltf_to_kaizen_expression' method.
        /// </summary>
        public static readonly StringName SetGltfToKaizenExpression = "set_gltf_to_kaizen_expression";
        /// <summary>
        /// Cached name for the 'get_kaizen_to_gltf_expression' method.
        /// </summary>
        public static readonly StringName GetKaizenToGltfExpression = "get_kaizen_to_gltf_expression";
        /// <summary>
        /// Cached name for the 'set_kaizen_to_gltf_expression' method.
        /// </summary>
        public static readonly StringName SetKaizenToGltfExpression = "set_kaizen_to_gltf_expression";
        /// <summary>
        /// Cached name for the 'get_node_paths' method.
        /// </summary>
        public static readonly StringName GetNodePaths = "get_node_paths";
        /// <summary>
        /// Cached name for the 'has_node_paths' method.
        /// </summary>
        public static readonly StringName HasNodePaths = "has_node_paths";
        /// <summary>
        /// Cached name for the 'set_node_paths' method.
        /// </summary>
        public static readonly StringName SetNodePaths = "set_node_paths";
        /// <summary>
        /// Cached name for the 'get_object_model_type' method.
        /// </summary>
        public static readonly StringName GetObjectModelType = "get_object_model_type";
        /// <summary>
        /// Cached name for the 'set_object_model_type' method.
        /// </summary>
        public static readonly StringName SetObjectModelType = "set_object_model_type";
        /// <summary>
        /// Cached name for the 'get_json_pointers' method.
        /// </summary>
        public static readonly StringName GetJsonPointers = "get_json_pointers";
        /// <summary>
        /// Cached name for the 'has_json_pointers' method.
        /// </summary>
        public static readonly StringName HasJsonPointers = "has_json_pointers";
        /// <summary>
        /// Cached name for the 'set_json_pointers' method.
        /// </summary>
        public static readonly StringName SetJsonPointers = "set_json_pointers";
        /// <summary>
        /// Cached name for the 'get_variant_type' method.
        /// </summary>
        public static readonly StringName GetVariantType = "get_variant_type";
        /// <summary>
        /// Cached name for the 'set_variant_type' method.
        /// </summary>
        public static readonly StringName SetVariantType = "set_variant_type";
        /// <summary>
        /// Cached name for the 'set_types' method.
        /// </summary>
        public static readonly StringName SetTypes = "set_types";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
