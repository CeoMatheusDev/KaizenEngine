#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming rule violation
// ReSharper disable InconsistentNaming

namespace Kaizen.NativeInterop
{
    public static partial class NativeFuncs
    {
        public static kaizen_variant kaizensharp_variant_new_copy(scoped in kaizen_variant src)
        {
            switch (src.Type)
            {
                case Variant.Type.Nil:
                    return default;
                case Variant.Type.Bool:
                    return new kaizen_variant() { Bool = src.Bool, Type = Variant.Type.Bool };
                case Variant.Type.Int:
                    return new kaizen_variant() { Int = src.Int, Type = Variant.Type.Int };
                case Variant.Type.Float:
                    return new kaizen_variant() { Float = src.Float, Type = Variant.Type.Float };
                case Variant.Type.Vector2:
                    return new kaizen_variant() { Vector2 = src.Vector2, Type = Variant.Type.Vector2 };
                case Variant.Type.Vector2I:
                    return new kaizen_variant() { Vector2I = src.Vector2I, Type = Variant.Type.Vector2I };
                case Variant.Type.Rect2:
                    return new kaizen_variant() { Rect2 = src.Rect2, Type = Variant.Type.Rect2 };
                case Variant.Type.Rect2I:
                    return new kaizen_variant() { Rect2I = src.Rect2I, Type = Variant.Type.Rect2I };
                case Variant.Type.Vector3:
                    return new kaizen_variant() { Vector3 = src.Vector3, Type = Variant.Type.Vector3 };
                case Variant.Type.Vector3I:
                    return new kaizen_variant() { Vector3I = src.Vector3I, Type = Variant.Type.Vector3I };
                case Variant.Type.Vector4:
                    return new kaizen_variant() { Vector4 = src.Vector4, Type = Variant.Type.Vector4 };
                case Variant.Type.Vector4I:
                    return new kaizen_variant() { Vector4I = src.Vector4I, Type = Variant.Type.Vector4I };
                case Variant.Type.Plane:
                    return new kaizen_variant() { Plane = src.Plane, Type = Variant.Type.Plane };
                case Variant.Type.Quaternion:
                    return new kaizen_variant() { Quaternion = src.Quaternion, Type = Variant.Type.Quaternion };
                case Variant.Type.Color:
                    return new kaizen_variant() { Color = src.Color, Type = Variant.Type.Color };
                case Variant.Type.Rid:
                    return new kaizen_variant() { Rid = src.Rid, Type = Variant.Type.Rid };
            }

            kaizensharp_variant_new_copy(out kaizen_variant ret, src);
            return ret;
        }

        public static kaizen_string_name kaizensharp_string_name_new_copy(scoped in kaizen_string_name src)
        {
            if (src.IsEmpty)
                return default;
            kaizensharp_string_name_new_copy(out kaizen_string_name ret, src);
            return ret;
        }

        public static kaizen_node_path kaizensharp_node_path_new_copy(scoped in kaizen_node_path src)
        {
            if (src.IsEmpty)
                return default;
            kaizensharp_node_path_new_copy(out kaizen_node_path ret, src);
            return ret;
        }

        public static kaizen_array kaizensharp_array_new()
        {
            kaizensharp_array_new(out kaizen_array ret);
            return ret;
        }

        public static kaizen_array kaizensharp_array_new_copy(scoped in kaizen_array src)
        {
            kaizensharp_array_new_copy(out kaizen_array ret, src);
            return ret;
        }

        public static kaizen_dictionary kaizensharp_dictionary_new()
        {
            kaizensharp_dictionary_new(out kaizen_dictionary ret);
            return ret;
        }

        public static kaizen_dictionary kaizensharp_dictionary_new_copy(scoped in kaizen_dictionary src)
        {
            kaizensharp_dictionary_new_copy(out kaizen_dictionary ret, src);
            return ret;
        }

        public static kaizen_string_name kaizensharp_string_name_new_from_string(string name)
        {
            using kaizen_string src = Marshaling.ConvertStringToNative(name);
            kaizensharp_string_name_new_from_string(out kaizen_string_name ret, src);
            return ret;
        }

        public static kaizen_node_path kaizensharp_node_path_new_from_string(string name)
        {
            using kaizen_string src = Marshaling.ConvertStringToNative(name);
            kaizensharp_node_path_new_from_string(out kaizen_node_path ret, src);
            return ret;
        }
    }
}
