partial class ExportedProperties
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
#if TOOLS
    /// <summary>
    /// Get the default values for all properties declared in this class.
    /// This method is used by Kaizen to determine the value that will be
    /// used by the inspector when resetting properties.
    /// Do not call this method.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.Dictionary<global::Kaizen.StringName, global::Kaizen.Variant> GetKaizenPropertyDefaultValues()
    {
        var values = new global::System.Collections.Generic.Dictionary<global::Kaizen.StringName, global::Kaizen.Variant>(71);
        string __NotGenerateComplexLamdaProperty_default_value = default;
        values.Add(PropertyName.@NotGenerateComplexLamdaProperty, global::Kaizen.Variant.From<string>(__NotGenerateComplexLamdaProperty_default_value));
        string __NotGenerateLamdaNoFieldProperty_default_value = default;
        values.Add(PropertyName.@NotGenerateLamdaNoFieldProperty, global::Kaizen.Variant.From<string>(__NotGenerateLamdaNoFieldProperty_default_value));
        string __NotGenerateComplexReturnProperty_default_value = default;
        values.Add(PropertyName.@NotGenerateComplexReturnProperty, global::Kaizen.Variant.From<string>(__NotGenerateComplexReturnProperty_default_value));
        string __NotGenerateReturnsProperty_default_value = default;
        values.Add(PropertyName.@NotGenerateReturnsProperty, global::Kaizen.Variant.From<string>(__NotGenerateReturnsProperty_default_value));
        string __FullPropertyString_default_value = "FullPropertyString";
        values.Add(PropertyName.@FullPropertyString, global::Kaizen.Variant.From<string>(__FullPropertyString_default_value));
        string __FullPropertyString_Complex_default_value = new string("FullPropertyString_Complex")   + global::System.Convert.ToInt32("1");
        values.Add(PropertyName.@FullPropertyString_Complex, global::Kaizen.Variant.From<string>(__FullPropertyString_Complex_default_value));
        float __FullPropertyStaticImport_default_value = global::Kaizen.Mathf.Pi;
        values.Add(PropertyName.@FullPropertyStaticImport, global::Kaizen.Variant.From<float>(__FullPropertyStaticImport_default_value));
        string __LamdaPropertyString_default_value = "LamdaPropertyString";
        values.Add(PropertyName.@LamdaPropertyString, global::Kaizen.Variant.From<string>(__LamdaPropertyString_default_value));
        float __LambdaPropertyStaticImport_default_value = global::Kaizen.Mathf.Tau;
        values.Add(PropertyName.@LambdaPropertyStaticImport, global::Kaizen.Variant.From<float>(__LambdaPropertyStaticImport_default_value));
        string __PrimaryCtorParameter_default_value = default;
        values.Add(PropertyName.@PrimaryCtorParameter, global::Kaizen.Variant.From<string>(__PrimaryCtorParameter_default_value));
        float __ConstantMath_default_value = 2  * global::Kaizen.Mathf.Pi;
        values.Add(PropertyName.@ConstantMath, global::Kaizen.Variant.From<float>(__ConstantMath_default_value));
        float __ConstantMathStaticImport_default_value = global::Kaizen.Mathf.RadToDeg(2  * global::Kaizen.Mathf.Pi);
        values.Add(PropertyName.@ConstantMathStaticImport, global::Kaizen.Variant.From<float>(__ConstantMathStaticImport_default_value));
        string __StaticStringAddition_default_value = string.Empty   + string.Empty;
        values.Add(PropertyName.@StaticStringAddition, global::Kaizen.Variant.From<string>(__StaticStringAddition_default_value));
        bool __PropertyBoolean_default_value = true;
        values.Add(PropertyName.@PropertyBoolean, global::Kaizen.Variant.From<bool>(__PropertyBoolean_default_value));
        char __PropertyChar_default_value = 'f';
        values.Add(PropertyName.@PropertyChar, global::Kaizen.Variant.From<char>(__PropertyChar_default_value));
        sbyte __PropertySByte_default_value = 10;
        values.Add(PropertyName.@PropertySByte, global::Kaizen.Variant.From<sbyte>(__PropertySByte_default_value));
        short __PropertyInt16_default_value = 10;
        values.Add(PropertyName.@PropertyInt16, global::Kaizen.Variant.From<short>(__PropertyInt16_default_value));
        int __PropertyInt32_default_value = 10;
        values.Add(PropertyName.@PropertyInt32, global::Kaizen.Variant.From<int>(__PropertyInt32_default_value));
        long __PropertyInt64_default_value = -10_000;
        values.Add(PropertyName.@PropertyInt64, global::Kaizen.Variant.From<long>(__PropertyInt64_default_value));
        byte __PropertyByte_default_value = 10;
        values.Add(PropertyName.@PropertyByte, global::Kaizen.Variant.From<byte>(__PropertyByte_default_value));
        ushort __PropertyUInt16_default_value = 10;
        values.Add(PropertyName.@PropertyUInt16, global::Kaizen.Variant.From<ushort>(__PropertyUInt16_default_value));
        uint __PropertyUInt32_default_value = 10;
        values.Add(PropertyName.@PropertyUInt32, global::Kaizen.Variant.From<uint>(__PropertyUInt32_default_value));
        ulong __PropertyUInt64_default_value = 10;
        values.Add(PropertyName.@PropertyUInt64, global::Kaizen.Variant.From<ulong>(__PropertyUInt64_default_value));
        float __PropertySingle_default_value = 10;
        values.Add(PropertyName.@PropertySingle, global::Kaizen.Variant.From<float>(__PropertySingle_default_value));
        double __PropertyDouble_default_value = 10;
        values.Add(PropertyName.@PropertyDouble, global::Kaizen.Variant.From<double>(__PropertyDouble_default_value));
        string __PropertyString_default_value = "foo";
        values.Add(PropertyName.@PropertyString, global::Kaizen.Variant.From<string>(__PropertyString_default_value));
        global::Kaizen.Vector2 __PropertyVector2_default_value = new(10f, 10f);
        values.Add(PropertyName.@PropertyVector2, global::Kaizen.Variant.From<global::Kaizen.Vector2>(__PropertyVector2_default_value));
        global::Kaizen.Vector2I __PropertyVector2I_default_value = global::Kaizen.Vector2I.Up;
        values.Add(PropertyName.@PropertyVector2I, global::Kaizen.Variant.From<global::Kaizen.Vector2I>(__PropertyVector2I_default_value));
        global::Kaizen.Rect2 __PropertyRect2_default_value = new(new global::Kaizen.Vector2(10f, 10f), new global::Kaizen.Vector2(10f, 10f));
        values.Add(PropertyName.@PropertyRect2, global::Kaizen.Variant.From<global::Kaizen.Rect2>(__PropertyRect2_default_value));
        global::Kaizen.Rect2I __PropertyRect2I_default_value = new(new global::Kaizen.Vector2I(10, 10), new global::Kaizen.Vector2I(10, 10));
        values.Add(PropertyName.@PropertyRect2I, global::Kaizen.Variant.From<global::Kaizen.Rect2I>(__PropertyRect2I_default_value));
        global::Kaizen.Transform2D __PropertyTransform2D_default_value = global::Kaizen.Transform2D.Identity;
        values.Add(PropertyName.@PropertyTransform2D, global::Kaizen.Variant.From<global::Kaizen.Transform2D>(__PropertyTransform2D_default_value));
        global::Kaizen.Vector3 __PropertyVector3_default_value = new(10f, 10f, 10f);
        values.Add(PropertyName.@PropertyVector3, global::Kaizen.Variant.From<global::Kaizen.Vector3>(__PropertyVector3_default_value));
        global::Kaizen.Vector3I __PropertyVector3I_default_value = global::Kaizen.Vector3I.Back;
        values.Add(PropertyName.@PropertyVector3I, global::Kaizen.Variant.From<global::Kaizen.Vector3I>(__PropertyVector3I_default_value));
        global::Kaizen.Basis __PropertyBasis_default_value = new global::Kaizen.Basis(global::Kaizen.Quaternion.Identity);
        values.Add(PropertyName.@PropertyBasis, global::Kaizen.Variant.From<global::Kaizen.Basis>(__PropertyBasis_default_value));
        global::Kaizen.Quaternion __PropertyQuaternion_default_value = new global::Kaizen.Quaternion(global::Kaizen.Basis.Identity);
        values.Add(PropertyName.@PropertyQuaternion, global::Kaizen.Variant.From<global::Kaizen.Quaternion>(__PropertyQuaternion_default_value));
        global::Kaizen.Transform3D __PropertyTransform3D_default_value = global::Kaizen.Transform3D.Identity;
        values.Add(PropertyName.@PropertyTransform3D, global::Kaizen.Variant.From<global::Kaizen.Transform3D>(__PropertyTransform3D_default_value));
        global::Kaizen.Vector4 __PropertyVector4_default_value = new(10f, 10f, 10f, 10f);
        values.Add(PropertyName.@PropertyVector4, global::Kaizen.Variant.From<global::Kaizen.Vector4>(__PropertyVector4_default_value));
        global::Kaizen.Vector4I __PropertyVector4I_default_value = global::Kaizen.Vector4I.One;
        values.Add(PropertyName.@PropertyVector4I, global::Kaizen.Variant.From<global::Kaizen.Vector4I>(__PropertyVector4I_default_value));
        global::Kaizen.Projection __PropertyProjection_default_value = global::Kaizen.Projection.Identity;
        values.Add(PropertyName.@PropertyProjection, global::Kaizen.Variant.From<global::Kaizen.Projection>(__PropertyProjection_default_value));
        global::Kaizen.Aabb __PropertyAabb_default_value = new global::Kaizen.Aabb(10f, 10f, 10f, new global::Kaizen.Vector3(1f, 1f, 1f));
        values.Add(PropertyName.@PropertyAabb, global::Kaizen.Variant.From<global::Kaizen.Aabb>(__PropertyAabb_default_value));
        global::Kaizen.Color __PropertyColor_default_value = global::Kaizen.Colors.Aquamarine;
        values.Add(PropertyName.@PropertyColor, global::Kaizen.Variant.From<global::Kaizen.Color>(__PropertyColor_default_value));
        global::Kaizen.Plane __PropertyPlane_default_value = global::Kaizen.Plane.PlaneXZ;
        values.Add(PropertyName.@PropertyPlane, global::Kaizen.Variant.From<global::Kaizen.Plane>(__PropertyPlane_default_value));
        global::Kaizen.Callable __PropertyCallable_default_value = new global::Kaizen.Callable(global::Kaizen.Engine.GetMainLoop(), "_process");
        values.Add(PropertyName.@PropertyCallable, global::Kaizen.Variant.From<global::Kaizen.Callable>(__PropertyCallable_default_value));
        global::Kaizen.Signal __PropertySignal_default_value = new global::Kaizen.Signal(global::Kaizen.Engine.GetMainLoop(), "Propertylist_changed");
        values.Add(PropertyName.@PropertySignal, global::Kaizen.Variant.From<global::Kaizen.Signal>(__PropertySignal_default_value));
        global::ExportedProperties.MyEnum __PropertyEnum_default_value = global::ExportedProperties.MyEnum.C;
        values.Add(PropertyName.@PropertyEnum, global::Kaizen.Variant.From<global::ExportedProperties.MyEnum>(__PropertyEnum_default_value));
        global::ExportedProperties.MyFlagsEnum __PropertyFlagsEnum_default_value = global::ExportedProperties.MyFlagsEnum.C;
        values.Add(PropertyName.@PropertyFlagsEnum, global::Kaizen.Variant.From<global::ExportedProperties.MyFlagsEnum>(__PropertyFlagsEnum_default_value));
        byte[] __PropertyByteArray_default_value = { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@PropertyByteArray, global::Kaizen.Variant.From<byte[]>(__PropertyByteArray_default_value));
        int[] __PropertyInt32Array_default_value = { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@PropertyInt32Array, global::Kaizen.Variant.From<int[]>(__PropertyInt32Array_default_value));
        long[] __PropertyInt64Array_default_value = { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@PropertyInt64Array, global::Kaizen.Variant.From<long[]>(__PropertyInt64Array_default_value));
        float[] __PropertySingleArray_default_value = { 0f, 1f, 2f, 3f, 4f, 5f, 6f  };
        values.Add(PropertyName.@PropertySingleArray, global::Kaizen.Variant.From<float[]>(__PropertySingleArray_default_value));
        double[] __PropertyDoubleArray_default_value = { 0d, 1d, 2d, 3d, 4d, 5d, 6d  };
        values.Add(PropertyName.@PropertyDoubleArray, global::Kaizen.Variant.From<double[]>(__PropertyDoubleArray_default_value));
        string[] __PropertyStringArray_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@PropertyStringArray, global::Kaizen.Variant.From<string[]>(__PropertyStringArray_default_value));
        string[] __PropertyStringArrayEnum_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@PropertyStringArrayEnum, global::Kaizen.Variant.From<string[]>(__PropertyStringArrayEnum_default_value));
        global::Kaizen.Vector2[] __PropertyVector2Array_default_value = { global::Kaizen.Vector2.Up, global::Kaizen.Vector2.Down, global::Kaizen.Vector2.Left, global::Kaizen.Vector2.Right   };
        values.Add(PropertyName.@PropertyVector2Array, global::Kaizen.Variant.From<global::Kaizen.Vector2[]>(__PropertyVector2Array_default_value));
        global::Kaizen.Vector3[] __PropertyVector3Array_default_value = { global::Kaizen.Vector3.Up, global::Kaizen.Vector3.Down, global::Kaizen.Vector3.Left, global::Kaizen.Vector3.Right   };
        values.Add(PropertyName.@PropertyVector3Array, global::Kaizen.Variant.From<global::Kaizen.Vector3[]>(__PropertyVector3Array_default_value));
        global::Kaizen.Color[] __PropertyColorArray_default_value = { global::Kaizen.Colors.Aqua, global::Kaizen.Colors.Aquamarine, global::Kaizen.Colors.Azure, global::Kaizen.Colors.Beige   };
        values.Add(PropertyName.@PropertyColorArray, global::Kaizen.Variant.From<global::Kaizen.Color[]>(__PropertyColorArray_default_value));
        global::Kaizen.KaizenObject[] __PropertyKaizenObjectOrDerivedArray_default_value = { null  };
        values.Add(PropertyName.@PropertyKaizenObjectOrDerivedArray, global::Kaizen.Variant.CreateFrom(__PropertyKaizenObjectOrDerivedArray_default_value));
        global::Kaizen.StringName[] __field_StringNameArray_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@field_StringNameArray, global::Kaizen.Variant.From<global::Kaizen.StringName[]>(__field_StringNameArray_default_value));
        global::Kaizen.NodePath[] __field_NodePathArray_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@field_NodePathArray, global::Kaizen.Variant.From<global::Kaizen.NodePath[]>(__field_NodePathArray_default_value));
        global::Kaizen.Rid[] __field_RidArray_default_value = { default, default, default  };
        values.Add(PropertyName.@field_RidArray, global::Kaizen.Variant.From<global::Kaizen.Rid[]>(__field_RidArray_default_value));
        global::Kaizen.Variant __PropertyVariant_default_value = "foo";
        values.Add(PropertyName.@PropertyVariant, global::Kaizen.Variant.From<global::Kaizen.Variant>(__PropertyVariant_default_value));
        global::Kaizen.KaizenObject __PropertyKaizenObjectOrDerived_default_value = default;
        values.Add(PropertyName.@PropertyKaizenObjectOrDerived, global::Kaizen.Variant.From<global::Kaizen.KaizenObject>(__PropertyKaizenObjectOrDerived_default_value));
        global::Kaizen.Texture __PropertyKaizenResourceTexture_default_value = default;
        values.Add(PropertyName.@PropertyKaizenResourceTexture, global::Kaizen.Variant.From<global::Kaizen.Texture>(__PropertyKaizenResourceTexture_default_value));
        global::Kaizen.Texture __PropertyKaizenResourceTextureWithInitializer_default_value = new()  { ResourceName  = ""   };
        values.Add(PropertyName.@PropertyKaizenResourceTextureWithInitializer, global::Kaizen.Variant.From<global::Kaizen.Texture>(__PropertyKaizenResourceTextureWithInitializer_default_value));
        global::Kaizen.StringName __PropertyStringName_default_value = new global::Kaizen.StringName("foo");
        values.Add(PropertyName.@PropertyStringName, global::Kaizen.Variant.From<global::Kaizen.StringName>(__PropertyStringName_default_value));
        global::Kaizen.NodePath __PropertyNodePath_default_value = new global::Kaizen.NodePath("foo");
        values.Add(PropertyName.@PropertyNodePath, global::Kaizen.Variant.From<global::Kaizen.NodePath>(__PropertyNodePath_default_value));
        global::Kaizen.Rid __PropertyRid_default_value = default;
        values.Add(PropertyName.@PropertyRid, global::Kaizen.Variant.From<global::Kaizen.Rid>(__PropertyRid_default_value));
        global::Kaizen.Collections.Dictionary __PropertyKaizenDictionary_default_value = new()  { { "foo", 10  }, { global::Kaizen.Vector2.Up, global::Kaizen.Colors.Chocolate   }  };
        values.Add(PropertyName.@PropertyKaizenDictionary, global::Kaizen.Variant.From<global::Kaizen.Collections.Dictionary>(__PropertyKaizenDictionary_default_value));
        global::Kaizen.Collections.Array __PropertyKaizenArray_default_value = new()  { "foo", 10, global::Kaizen.Vector2.Up, global::Kaizen.Colors.Chocolate   };
        values.Add(PropertyName.@PropertyKaizenArray, global::Kaizen.Variant.From<global::Kaizen.Collections.Array>(__PropertyKaizenArray_default_value));
        global::Kaizen.Collections.Dictionary<string, bool> __PropertyKaizenGenericDictionary_default_value = new()  { { "foo", true  }, { "bar", false  }  };
        values.Add(PropertyName.@PropertyKaizenGenericDictionary, global::Kaizen.Variant.CreateFrom(__PropertyKaizenGenericDictionary_default_value));
        global::Kaizen.Collections.Array<int> __PropertyKaizenGenericArray_default_value = new()  { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@PropertyKaizenGenericArray, global::Kaizen.Variant.CreateFrom(__PropertyKaizenGenericArray_default_value));
        return values;
    }
#endif // TOOLS
#pragma warning restore CS0109
}
