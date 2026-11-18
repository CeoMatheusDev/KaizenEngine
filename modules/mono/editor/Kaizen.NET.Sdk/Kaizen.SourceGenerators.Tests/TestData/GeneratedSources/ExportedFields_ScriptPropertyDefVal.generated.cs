partial class ExportedFields
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
        var values = new global::System.Collections.Generic.Dictionary<global::Kaizen.StringName, global::Kaizen.Variant>(62);
        bool ___fieldBoolean_default_value = true;
        values.Add(PropertyName.@_fieldBoolean, global::Kaizen.Variant.From<bool>(___fieldBoolean_default_value));
        char ___fieldChar_default_value = 'f';
        values.Add(PropertyName.@_fieldChar, global::Kaizen.Variant.From<char>(___fieldChar_default_value));
        sbyte ___fieldSByte_default_value = 10;
        values.Add(PropertyName.@_fieldSByte, global::Kaizen.Variant.From<sbyte>(___fieldSByte_default_value));
        short ___fieldInt16_default_value = 10;
        values.Add(PropertyName.@_fieldInt16, global::Kaizen.Variant.From<short>(___fieldInt16_default_value));
        int ___fieldInt32_default_value = 10;
        values.Add(PropertyName.@_fieldInt32, global::Kaizen.Variant.From<int>(___fieldInt32_default_value));
        long ___fieldInt64_default_value = -10_000;
        values.Add(PropertyName.@_fieldInt64, global::Kaizen.Variant.From<long>(___fieldInt64_default_value));
        byte ___fieldByte_default_value = 10;
        values.Add(PropertyName.@_fieldByte, global::Kaizen.Variant.From<byte>(___fieldByte_default_value));
        ushort ___fieldUInt16_default_value = 10;
        values.Add(PropertyName.@_fieldUInt16, global::Kaizen.Variant.From<ushort>(___fieldUInt16_default_value));
        uint ___fieldUInt32_default_value = 10;
        values.Add(PropertyName.@_fieldUInt32, global::Kaizen.Variant.From<uint>(___fieldUInt32_default_value));
        ulong ___fieldUInt64_default_value = 10;
        values.Add(PropertyName.@_fieldUInt64, global::Kaizen.Variant.From<ulong>(___fieldUInt64_default_value));
        float ___fieldSingle_default_value = 10;
        values.Add(PropertyName.@_fieldSingle, global::Kaizen.Variant.From<float>(___fieldSingle_default_value));
        double ___fieldDouble_default_value = 10;
        values.Add(PropertyName.@_fieldDouble, global::Kaizen.Variant.From<double>(___fieldDouble_default_value));
        string ___fieldString_default_value = "foo";
        values.Add(PropertyName.@_fieldString, global::Kaizen.Variant.From<string>(___fieldString_default_value));
        float ___fieldStaticImport_default_value = global::Kaizen.Mathf.RadToDeg(2  * global::Kaizen.Mathf.Pi);
        values.Add(PropertyName.@_fieldStaticImport, global::Kaizen.Variant.From<float>(___fieldStaticImport_default_value));
        global::Kaizen.Vector2 ___fieldVector2_default_value = new(10f, 10f);
        values.Add(PropertyName.@_fieldVector2, global::Kaizen.Variant.From<global::Kaizen.Vector2>(___fieldVector2_default_value));
        global::Kaizen.Vector2I ___fieldVector2I_default_value = global::Kaizen.Vector2I.Up;
        values.Add(PropertyName.@_fieldVector2I, global::Kaizen.Variant.From<global::Kaizen.Vector2I>(___fieldVector2I_default_value));
        global::Kaizen.Rect2 ___fieldRect2_default_value = new(new global::Kaizen.Vector2(10f, 10f), new global::Kaizen.Vector2(10f, 10f));
        values.Add(PropertyName.@_fieldRect2, global::Kaizen.Variant.From<global::Kaizen.Rect2>(___fieldRect2_default_value));
        global::Kaizen.Rect2I ___fieldRect2I_default_value = new(new global::Kaizen.Vector2I(10, 10), new global::Kaizen.Vector2I(10, 10));
        values.Add(PropertyName.@_fieldRect2I, global::Kaizen.Variant.From<global::Kaizen.Rect2I>(___fieldRect2I_default_value));
        global::Kaizen.Transform2D ___fieldTransform2D_default_value = global::Kaizen.Transform2D.Identity;
        values.Add(PropertyName.@_fieldTransform2D, global::Kaizen.Variant.From<global::Kaizen.Transform2D>(___fieldTransform2D_default_value));
        global::Kaizen.Vector3 ___fieldVector3_default_value = new(10f, 10f, 10f);
        values.Add(PropertyName.@_fieldVector3, global::Kaizen.Variant.From<global::Kaizen.Vector3>(___fieldVector3_default_value));
        global::Kaizen.Vector3I ___fieldVector3I_default_value = global::Kaizen.Vector3I.Back;
        values.Add(PropertyName.@_fieldVector3I, global::Kaizen.Variant.From<global::Kaizen.Vector3I>(___fieldVector3I_default_value));
        global::Kaizen.Basis ___fieldBasis_default_value = new global::Kaizen.Basis(global::Kaizen.Quaternion.Identity);
        values.Add(PropertyName.@_fieldBasis, global::Kaizen.Variant.From<global::Kaizen.Basis>(___fieldBasis_default_value));
        global::Kaizen.Quaternion ___fieldQuaternion_default_value = new global::Kaizen.Quaternion(global::Kaizen.Basis.Identity);
        values.Add(PropertyName.@_fieldQuaternion, global::Kaizen.Variant.From<global::Kaizen.Quaternion>(___fieldQuaternion_default_value));
        global::Kaizen.Transform3D ___fieldTransform3D_default_value = global::Kaizen.Transform3D.Identity;
        values.Add(PropertyName.@_fieldTransform3D, global::Kaizen.Variant.From<global::Kaizen.Transform3D>(___fieldTransform3D_default_value));
        global::Kaizen.Vector4 ___fieldVector4_default_value = new(10f, 10f, 10f, 10f);
        values.Add(PropertyName.@_fieldVector4, global::Kaizen.Variant.From<global::Kaizen.Vector4>(___fieldVector4_default_value));
        global::Kaizen.Vector4I ___fieldVector4I_default_value = global::Kaizen.Vector4I.One;
        values.Add(PropertyName.@_fieldVector4I, global::Kaizen.Variant.From<global::Kaizen.Vector4I>(___fieldVector4I_default_value));
        global::Kaizen.Projection ___fieldProjection_default_value = global::Kaizen.Projection.Identity;
        values.Add(PropertyName.@_fieldProjection, global::Kaizen.Variant.From<global::Kaizen.Projection>(___fieldProjection_default_value));
        global::Kaizen.Aabb ___fieldAabb_default_value = new global::Kaizen.Aabb(10f, 10f, 10f, new global::Kaizen.Vector3(1f, 1f, 1f));
        values.Add(PropertyName.@_fieldAabb, global::Kaizen.Variant.From<global::Kaizen.Aabb>(___fieldAabb_default_value));
        global::Kaizen.Color ___fieldColor_default_value = global::Kaizen.Colors.Aquamarine;
        values.Add(PropertyName.@_fieldColor, global::Kaizen.Variant.From<global::Kaizen.Color>(___fieldColor_default_value));
        global::Kaizen.Plane ___fieldPlane_default_value = global::Kaizen.Plane.PlaneXZ;
        values.Add(PropertyName.@_fieldPlane, global::Kaizen.Variant.From<global::Kaizen.Plane>(___fieldPlane_default_value));
        global::Kaizen.Callable ___fieldCallable_default_value = new global::Kaizen.Callable(global::Kaizen.Engine.GetMainLoop(), "_process");
        values.Add(PropertyName.@_fieldCallable, global::Kaizen.Variant.From<global::Kaizen.Callable>(___fieldCallable_default_value));
        global::Kaizen.Signal ___fieldSignal_default_value = new global::Kaizen.Signal(global::Kaizen.Engine.GetMainLoop(), "property_list_changed");
        values.Add(PropertyName.@_fieldSignal, global::Kaizen.Variant.From<global::Kaizen.Signal>(___fieldSignal_default_value));
        global::ExportedFields.MyEnum ___fieldEnum_default_value = global::ExportedFields.MyEnum.C;
        values.Add(PropertyName.@_fieldEnum, global::Kaizen.Variant.From<global::ExportedFields.MyEnum>(___fieldEnum_default_value));
        global::ExportedFields.MyFlagsEnum ___fieldFlagsEnum_default_value = global::ExportedFields.MyFlagsEnum.C;
        values.Add(PropertyName.@_fieldFlagsEnum, global::Kaizen.Variant.From<global::ExportedFields.MyFlagsEnum>(___fieldFlagsEnum_default_value));
        byte[] ___fieldByteArray_default_value = { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@_fieldByteArray, global::Kaizen.Variant.From<byte[]>(___fieldByteArray_default_value));
        int[] ___fieldInt32Array_default_value = { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@_fieldInt32Array, global::Kaizen.Variant.From<int[]>(___fieldInt32Array_default_value));
        long[] ___fieldInt64Array_default_value = { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@_fieldInt64Array, global::Kaizen.Variant.From<long[]>(___fieldInt64Array_default_value));
        float[] ___fieldSingleArray_default_value = { 0f, 1f, 2f, 3f, 4f, 5f, 6f  };
        values.Add(PropertyName.@_fieldSingleArray, global::Kaizen.Variant.From<float[]>(___fieldSingleArray_default_value));
        double[] ___fieldDoubleArray_default_value = { 0d, 1d, 2d, 3d, 4d, 5d, 6d  };
        values.Add(PropertyName.@_fieldDoubleArray, global::Kaizen.Variant.From<double[]>(___fieldDoubleArray_default_value));
        string[] ___fieldStringArray_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@_fieldStringArray, global::Kaizen.Variant.From<string[]>(___fieldStringArray_default_value));
        string[] ___fieldStringArrayEnum_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@_fieldStringArrayEnum, global::Kaizen.Variant.From<string[]>(___fieldStringArrayEnum_default_value));
        global::Kaizen.Vector2[] ___fieldVector2Array_default_value = { global::Kaizen.Vector2.Up, global::Kaizen.Vector2.Down, global::Kaizen.Vector2.Left, global::Kaizen.Vector2.Right   };
        values.Add(PropertyName.@_fieldVector2Array, global::Kaizen.Variant.From<global::Kaizen.Vector2[]>(___fieldVector2Array_default_value));
        global::Kaizen.Vector3[] ___fieldVector3Array_default_value = { global::Kaizen.Vector3.Up, global::Kaizen.Vector3.Down, global::Kaizen.Vector3.Left, global::Kaizen.Vector3.Right   };
        values.Add(PropertyName.@_fieldVector3Array, global::Kaizen.Variant.From<global::Kaizen.Vector3[]>(___fieldVector3Array_default_value));
        global::Kaizen.Color[] ___fieldColorArray_default_value = { global::Kaizen.Colors.Aqua, global::Kaizen.Colors.Aquamarine, global::Kaizen.Colors.Azure, global::Kaizen.Colors.Beige   };
        values.Add(PropertyName.@_fieldColorArray, global::Kaizen.Variant.From<global::Kaizen.Color[]>(___fieldColorArray_default_value));
        global::Kaizen.KaizenObject[] ___fieldKaizenObjectOrDerivedArray_default_value = { null  };
        values.Add(PropertyName.@_fieldKaizenObjectOrDerivedArray, global::Kaizen.Variant.CreateFrom(___fieldKaizenObjectOrDerivedArray_default_value));
        global::Kaizen.StringName[] ___fieldStringNameArray_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@_fieldStringNameArray, global::Kaizen.Variant.From<global::Kaizen.StringName[]>(___fieldStringNameArray_default_value));
        global::Kaizen.NodePath[] ___fieldNodePathArray_default_value = { "foo", "bar"  };
        values.Add(PropertyName.@_fieldNodePathArray, global::Kaizen.Variant.From<global::Kaizen.NodePath[]>(___fieldNodePathArray_default_value));
        global::Kaizen.Rid[] ___fieldRidArray_default_value = { default, default, default  };
        values.Add(PropertyName.@_fieldRidArray, global::Kaizen.Variant.From<global::Kaizen.Rid[]>(___fieldRidArray_default_value));
        int[] ___fieldEmptyInt32Array_default_value = global::System.Array.Empty<int>();
        values.Add(PropertyName.@_fieldEmptyInt32Array, global::Kaizen.Variant.From<int[]>(___fieldEmptyInt32Array_default_value));
        int[] ___fieldArrayFromList_default_value = new global::System.Collections.Generic.List<int>(global::System.Array.Empty<int>()).ToArray();
        values.Add(PropertyName.@_fieldArrayFromList, global::Kaizen.Variant.From<int[]>(___fieldArrayFromList_default_value));
        global::Kaizen.Variant ___fieldVariant_default_value = "foo";
        values.Add(PropertyName.@_fieldVariant, global::Kaizen.Variant.From<global::Kaizen.Variant>(___fieldVariant_default_value));
        global::Kaizen.KaizenObject ___fieldKaizenObjectOrDerived_default_value = default;
        values.Add(PropertyName.@_fieldKaizenObjectOrDerived, global::Kaizen.Variant.From<global::Kaizen.KaizenObject>(___fieldKaizenObjectOrDerived_default_value));
        global::Kaizen.Texture ___fieldKaizenResourceTexture_default_value = default;
        values.Add(PropertyName.@_fieldKaizenResourceTexture, global::Kaizen.Variant.From<global::Kaizen.Texture>(___fieldKaizenResourceTexture_default_value));
        global::Kaizen.Texture ___fieldKaizenResourceTextureWithInitializer_default_value = new()  { ResourceName  = ""   };
        values.Add(PropertyName.@_fieldKaizenResourceTextureWithInitializer, global::Kaizen.Variant.From<global::Kaizen.Texture>(___fieldKaizenResourceTextureWithInitializer_default_value));
        global::Kaizen.StringName ___fieldStringName_default_value = new global::Kaizen.StringName("foo");
        values.Add(PropertyName.@_fieldStringName, global::Kaizen.Variant.From<global::Kaizen.StringName>(___fieldStringName_default_value));
        global::Kaizen.NodePath ___fieldNodePath_default_value = new global::Kaizen.NodePath("foo");
        values.Add(PropertyName.@_fieldNodePath, global::Kaizen.Variant.From<global::Kaizen.NodePath>(___fieldNodePath_default_value));
        global::Kaizen.Rid ___fieldRid_default_value = default;
        values.Add(PropertyName.@_fieldRid, global::Kaizen.Variant.From<global::Kaizen.Rid>(___fieldRid_default_value));
        global::Kaizen.Collections.Dictionary ___fieldKaizenDictionary_default_value = new()  { { "foo", 10  }, { global::Kaizen.Vector2.Up, global::Kaizen.Colors.Chocolate   }  };
        values.Add(PropertyName.@_fieldKaizenDictionary, global::Kaizen.Variant.From<global::Kaizen.Collections.Dictionary>(___fieldKaizenDictionary_default_value));
        global::Kaizen.Collections.Array ___fieldKaizenArray_default_value = new()  { "foo", 10, global::Kaizen.Vector2.Up, global::Kaizen.Colors.Chocolate   };
        values.Add(PropertyName.@_fieldKaizenArray, global::Kaizen.Variant.From<global::Kaizen.Collections.Array>(___fieldKaizenArray_default_value));
        global::Kaizen.Collections.Dictionary<string, bool> ___fieldKaizenGenericDictionary_default_value = new()  { { "foo", true  }, { "bar", false  }  };
        values.Add(PropertyName.@_fieldKaizenGenericDictionary, global::Kaizen.Variant.CreateFrom(___fieldKaizenGenericDictionary_default_value));
        global::Kaizen.Collections.Array<int> ___fieldKaizenGenericArray_default_value = new()  { 0, 1, 2, 3, 4, 5, 6  };
        values.Add(PropertyName.@_fieldKaizenGenericArray, global::Kaizen.Variant.CreateFrom(___fieldKaizenGenericArray_default_value));
        long[] ___fieldEmptyInt64Array_default_value = global::System.Array.Empty<long>();
        values.Add(PropertyName.@_fieldEmptyInt64Array, global::Kaizen.Variant.From<long[]>(___fieldEmptyInt64Array_default_value));
        return values;
    }
#endif // TOOLS
#pragma warning restore CS0109
}
