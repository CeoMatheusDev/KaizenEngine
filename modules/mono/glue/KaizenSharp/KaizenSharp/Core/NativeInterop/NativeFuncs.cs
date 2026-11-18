#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE1006 // Naming rule violation
// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using Kaizen.SourceGenerators.Internal;


namespace Kaizen.NativeInterop
{
    /*
     * IMPORTANT:
     * The order of the methods defined in NativeFuncs must match the order
     * in the array defined at the bottom of 'glue/runtime_interop.cpp'.
     */

    [GenerateUnmanagedCallbacks(typeof(UnmanagedCallbacks))]
    public static unsafe partial class NativeFuncs
    {
        private static bool initialized;

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        public static void Initialize(IntPtr unmanagedCallbacks, int unmanagedCallbacksSize)
        {
            if (initialized)
                throw new InvalidOperationException("Already initialized.");
            initialized = true;

            if (unmanagedCallbacksSize != sizeof(UnmanagedCallbacks))
                throw new ArgumentException("Unmanaged callbacks size mismatch.", nameof(unmanagedCallbacksSize));

            _unmanagedCallbacks = Unsafe.AsRef<UnmanagedCallbacks>((void*)unmanagedCallbacks);
        }

        private partial struct UnmanagedCallbacks
        {
        }

        // Custom functions

        internal static partial kaizen_bool kaizensharp_dotnet_module_is_initialized();

        internal static partial void kaizensharp_initialize_marshaling_information(out ulong r_ref_count_offset, out ulong r_capacity_offset, out ulong r_size_offset, out ulong r_data_offset);

        public static partial IntPtr kaizensharp_method_bind_get_method(in kaizen_string_name p_classname,
            in kaizen_string_name p_methodname);

        public static partial IntPtr kaizensharp_method_bind_get_method_with_compatibility(
            in kaizen_string_name p_classname, in kaizen_string_name p_methodname, ulong p_hash);

        public static partial delegate* unmanaged<kaizen_bool, IntPtr> kaizensharp_get_class_constructor(
            in kaizen_string_name p_classname);

        public static partial IntPtr kaizensharp_engine_get_singleton(in kaizen_string p_name);


        internal static partial Error kaizensharp_stack_info_vector_resize(
            ref DebuggingUtils.kaizen_stack_info_vector p_stack_info_vector, int p_size);

        internal static partial void kaizensharp_stack_info_vector_destroy(
            ref DebuggingUtils.kaizen_stack_info_vector p_stack_info_vector);

        internal static partial void kaizensharp_internal_editor_file_system_update_files(in kaizen_packed_string_array p_script_paths);

        internal static partial void kaizensharp_internal_script_debugger_send_error(in kaizen_string p_func,
            in kaizen_string p_file, int p_line, in kaizen_string p_err, in kaizen_string p_descr,
            kaizen_error_handler_type p_type, in DebuggingUtils.kaizen_stack_info_vector p_stack_info_vector);

        internal static partial kaizen_bool kaizensharp_internal_script_debugger_is_active();

        internal static partial IntPtr kaizensharp_internal_object_get_associated_gchandle(IntPtr ptr);

        internal static partial void kaizensharp_internal_object_disposed(IntPtr ptr, IntPtr gcHandleToFree);

        internal static partial void kaizensharp_internal_refcounted_disposed(IntPtr ptr, IntPtr gcHandleToFree,
            kaizen_bool isFinalizer);

        internal static partial Error kaizensharp_internal_signal_awaiter_connect(IntPtr source,
            in kaizen_string_name signal,
            IntPtr target, IntPtr awaiterHandlePtr);

        internal static partial void kaizensharp_internal_tie_native_managed_to_unmanaged(IntPtr gcHandleIntPtr,
            IntPtr unmanaged, in kaizen_string_name nativeName, kaizen_bool refCounted);

        internal static partial void kaizensharp_internal_tie_user_managed_to_unmanaged(IntPtr gcHandleIntPtr,
            IntPtr unmanaged, kaizen_ref* scriptPtr, kaizen_bool refCounted);

        internal static partial void kaizensharp_internal_tie_managed_to_unmanaged_with_pre_setup(
            IntPtr gcHandleIntPtr, IntPtr unmanaged);

        internal static partial IntPtr kaizensharp_internal_unmanaged_get_script_instance_managed(IntPtr p_unmanaged,
            out kaizen_bool r_has_cs_script_instance);

        internal static partial IntPtr kaizensharp_internal_unmanaged_get_instance_binding_managed(IntPtr p_unmanaged);

        internal static partial IntPtr kaizensharp_internal_unmanaged_instance_binding_create_managed(IntPtr p_unmanaged,
            IntPtr oldGCHandlePtr);

        internal static partial void kaizensharp_internal_new_csharp_script(kaizen_ref* r_dest);

        internal static partial kaizen_bool kaizensharp_internal_script_load(in kaizen_string p_path, kaizen_ref* r_dest);

        internal static partial void kaizensharp_internal_reload_registered_script(IntPtr scriptPtr);

        internal static partial void kaizensharp_array_filter_kaizen_objects_by_native(scoped in kaizen_string_name p_native_name,
            scoped in kaizen_array p_input, out kaizen_array r_output);

        internal static partial void kaizensharp_array_filter_kaizen_objects_by_non_native(scoped in kaizen_array p_input,
            out kaizen_array r_output);

        public static partial void kaizensharp_ref_new_from_ref_counted_ptr(out kaizen_ref r_dest,
            IntPtr p_ref_counted_ptr);

        public static partial void kaizensharp_ref_destroy(ref kaizen_ref p_instance);

        public static partial void kaizensharp_string_name_new_from_string(out kaizen_string_name r_dest,
            scoped in kaizen_string p_name);

        public static partial void kaizensharp_node_path_new_from_string(out kaizen_node_path r_dest,
            scoped in kaizen_string p_name);

        public static partial void
            kaizensharp_string_name_as_string(out kaizen_string r_dest, scoped in kaizen_string_name p_name);

        public static partial void kaizensharp_node_path_as_string(out kaizen_string r_dest, scoped in kaizen_node_path p_np);

        public static partial kaizen_packed_byte_array kaizensharp_packed_byte_array_new_mem_copy(byte* p_src,
            int p_length);

        public static partial kaizen_packed_int32_array kaizensharp_packed_int32_array_new_mem_copy(int* p_src,
            int p_length);

        public static partial kaizen_packed_int64_array kaizensharp_packed_int64_array_new_mem_copy(long* p_src,
            int p_length);

        public static partial kaizen_packed_float32_array kaizensharp_packed_float32_array_new_mem_copy(float* p_src,
            int p_length);

        public static partial kaizen_packed_float64_array kaizensharp_packed_float64_array_new_mem_copy(double* p_src,
            int p_length);

        public static partial kaizen_packed_vector2_array kaizensharp_packed_vector2_array_new_mem_copy(Vector2* p_src,
            int p_length);

        public static partial kaizen_packed_vector3_array kaizensharp_packed_vector3_array_new_mem_copy(Vector3* p_src,
            int p_length);

        public static partial kaizen_packed_vector4_array kaizensharp_packed_vector4_array_new_mem_copy(Vector4* p_src,
            int p_length);

        public static partial kaizen_packed_color_array kaizensharp_packed_color_array_new_mem_copy(Color* p_src,
            int p_length);

        public static partial void kaizensharp_packed_string_array_add(ref kaizen_packed_string_array r_dest,
            in kaizen_string p_element);

        public static partial void kaizensharp_callable_new_with_delegate(IntPtr p_delegate_handle, IntPtr p_trampoline,
            IntPtr p_object, out kaizen_callable r_callable);

        internal static partial kaizen_bool kaizensharp_callable_get_data_for_marshalling(scoped in kaizen_callable p_callable,
            out IntPtr r_delegate_handle, out IntPtr r_trampoline, out IntPtr r_object, out kaizen_string_name r_name);

        internal static partial kaizen_variant kaizensharp_callable_call(scoped in kaizen_callable p_callable,
            kaizen_variant** p_args, int p_arg_count, out kaizen_variant_call_error p_call_error);

        internal static partial void kaizensharp_callable_call_deferred(in kaizen_callable p_callable,
            kaizen_variant** p_args, int p_arg_count);

        internal static partial Color kaizensharp_color_from_ok_hsl(float p_h, float p_s, float p_l, float p_alpha);

        internal static partial float kaizensharp_color_get_ok_hsl_h(in Color p_self);

        internal static partial float kaizensharp_color_get_ok_hsl_s(in Color p_self);

        internal static partial float kaizensharp_color_get_ok_hsl_l(in Color p_self);

        // GDNative functions

        // gdnative.h

        public static partial void kaizensharp_method_bind_ptrcall(IntPtr p_method_bind, IntPtr p_instance, void** p_args,
            void* p_ret);

        public static partial kaizen_variant kaizensharp_method_bind_call(IntPtr p_method_bind, IntPtr p_instance,
            kaizen_variant** p_args, int p_arg_count, out kaizen_variant_call_error p_call_error);

        // variant.h

        public static partial void
            kaizensharp_variant_new_string_name(out kaizen_variant r_dest, scoped in kaizen_string_name p_s);

        public static partial void kaizensharp_variant_new_copy(out kaizen_variant r_dest, scoped in kaizen_variant p_src);

        public static partial void kaizensharp_variant_new_node_path(out kaizen_variant r_dest, scoped in kaizen_node_path p_np);

        public static partial void kaizensharp_variant_new_object(out kaizen_variant r_dest, IntPtr p_obj);

        public static partial void kaizensharp_variant_new_transform2d(out kaizen_variant r_dest, scoped in Transform2D p_t2d);

        public static partial void kaizensharp_variant_new_basis(out kaizen_variant r_dest, scoped in Basis p_basis);

        public static partial void kaizensharp_variant_new_transform3d(out kaizen_variant r_dest, scoped in Transform3D p_trans);

        public static partial void kaizensharp_variant_new_projection(out kaizen_variant r_dest, scoped in Projection p_proj);

        public static partial void kaizensharp_variant_new_aabb(out kaizen_variant r_dest, scoped in Aabb p_aabb);

        public static partial void kaizensharp_variant_new_dictionary(out kaizen_variant r_dest,
            scoped in kaizen_dictionary p_dict);

        public static partial void kaizensharp_variant_new_array(out kaizen_variant r_dest, scoped in kaizen_array p_arr);

        public static partial void kaizensharp_variant_new_packed_byte_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_byte_array p_pba);

        public static partial void kaizensharp_variant_new_packed_int32_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_int32_array p_pia);

        public static partial void kaizensharp_variant_new_packed_int64_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_int64_array p_pia);

        public static partial void kaizensharp_variant_new_packed_float32_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_float32_array p_pra);

        public static partial void kaizensharp_variant_new_packed_float64_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_float64_array p_pra);

        public static partial void kaizensharp_variant_new_packed_string_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_string_array p_psa);

        public static partial void kaizensharp_variant_new_packed_vector2_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_vector2_array p_pv2a);

        public static partial void kaizensharp_variant_new_packed_vector3_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_vector3_array p_pv3a);

        public static partial void kaizensharp_variant_new_packed_vector4_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_vector4_array p_pv4a);

        public static partial void kaizensharp_variant_new_packed_color_array(out kaizen_variant r_dest,
            scoped in kaizen_packed_color_array p_pca);

        public static partial kaizen_bool kaizensharp_variant_as_bool(scoped in kaizen_variant p_self);

        public static partial Int64 kaizensharp_variant_as_int(scoped in kaizen_variant p_self);

        public static partial double kaizensharp_variant_as_float(scoped in kaizen_variant p_self);

        public static partial kaizen_string kaizensharp_variant_as_string(scoped in kaizen_variant p_self);

        public static partial Vector2 kaizensharp_variant_as_vector2(scoped in kaizen_variant p_self);

        public static partial Vector2I kaizensharp_variant_as_vector2i(scoped in kaizen_variant p_self);

        public static partial Rect2 kaizensharp_variant_as_rect2(scoped in kaizen_variant p_self);

        public static partial Rect2I kaizensharp_variant_as_rect2i(scoped in kaizen_variant p_self);

        public static partial Vector3 kaizensharp_variant_as_vector3(scoped in kaizen_variant p_self);

        public static partial Vector3I kaizensharp_variant_as_vector3i(scoped in kaizen_variant p_self);

        public static partial Transform2D kaizensharp_variant_as_transform2d(scoped in kaizen_variant p_self);

        public static partial Vector4 kaizensharp_variant_as_vector4(scoped in kaizen_variant p_self);

        public static partial Vector4I kaizensharp_variant_as_vector4i(scoped in kaizen_variant p_self);

        public static partial Plane kaizensharp_variant_as_plane(scoped in kaizen_variant p_self);

        public static partial Quaternion kaizensharp_variant_as_quaternion(scoped in kaizen_variant p_self);

        public static partial Aabb kaizensharp_variant_as_aabb(scoped in kaizen_variant p_self);

        public static partial Basis kaizensharp_variant_as_basis(scoped in kaizen_variant p_self);

        public static partial Transform3D kaizensharp_variant_as_transform3d(scoped in kaizen_variant p_self);

        public static partial Projection kaizensharp_variant_as_projection(scoped in kaizen_variant p_self);

        public static partial Color kaizensharp_variant_as_color(scoped in kaizen_variant p_self);

        public static partial kaizen_string_name kaizensharp_variant_as_string_name(scoped in kaizen_variant p_self);

        public static partial kaizen_node_path kaizensharp_variant_as_node_path(scoped in kaizen_variant p_self);

        public static partial Rid kaizensharp_variant_as_rid(scoped in kaizen_variant p_self);

        public static partial kaizen_callable kaizensharp_variant_as_callable(scoped in kaizen_variant p_self);

        public static partial kaizen_signal kaizensharp_variant_as_signal(scoped in kaizen_variant p_self);

        public static partial kaizen_dictionary kaizensharp_variant_as_dictionary(scoped in kaizen_variant p_self);

        public static partial kaizen_array kaizensharp_variant_as_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_byte_array kaizensharp_variant_as_packed_byte_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_int32_array kaizensharp_variant_as_packed_int32_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_int64_array kaizensharp_variant_as_packed_int64_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_float32_array kaizensharp_variant_as_packed_float32_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_float64_array kaizensharp_variant_as_packed_float64_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_string_array kaizensharp_variant_as_packed_string_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_vector2_array kaizensharp_variant_as_packed_vector2_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_vector3_array kaizensharp_variant_as_packed_vector3_array(scoped in kaizen_variant p_self);

        public static partial kaizen_packed_vector4_array kaizensharp_variant_as_packed_vector4_array(
            in kaizen_variant p_self);

        public static partial kaizen_packed_color_array kaizensharp_variant_as_packed_color_array(scoped in kaizen_variant p_self);

        public static partial kaizen_bool kaizensharp_variant_equals(scoped in kaizen_variant p_a, scoped in kaizen_variant p_b);

        // string.h

        public static partial void kaizensharp_string_new_with_utf16_chars(out kaizen_string r_dest, char* p_contents);

        // string_name.h

        public static partial void kaizensharp_string_name_new_copy(out kaizen_string_name r_dest,
            scoped in kaizen_string_name p_src);

        // node_path.h

        public static partial void kaizensharp_node_path_new_copy(out kaizen_node_path r_dest, scoped in kaizen_node_path p_src);

        // array.h

        public static partial void kaizensharp_array_new(out kaizen_array r_dest);

        public static partial void kaizensharp_array_new_copy(out kaizen_array r_dest, scoped in kaizen_array p_src);

        public static partial kaizen_variant* kaizensharp_array_ptrw(ref kaizen_array p_self);

        // dictionary.h

        public static partial void kaizensharp_dictionary_new(out kaizen_dictionary r_dest);

        public static partial void kaizensharp_dictionary_new_copy(out kaizen_dictionary r_dest,
            scoped in kaizen_dictionary p_src);

        // destroy functions

        public static partial void kaizensharp_packed_byte_array_destroy(ref kaizen_packed_byte_array p_self);

        public static partial void kaizensharp_packed_int32_array_destroy(ref kaizen_packed_int32_array p_self);

        public static partial void kaizensharp_packed_int64_array_destroy(ref kaizen_packed_int64_array p_self);

        public static partial void kaizensharp_packed_float32_array_destroy(ref kaizen_packed_float32_array p_self);

        public static partial void kaizensharp_packed_float64_array_destroy(ref kaizen_packed_float64_array p_self);

        public static partial void kaizensharp_packed_string_array_destroy(ref kaizen_packed_string_array p_self);

        public static partial void kaizensharp_packed_vector2_array_destroy(ref kaizen_packed_vector2_array p_self);

        public static partial void kaizensharp_packed_vector3_array_destroy(ref kaizen_packed_vector3_array p_self);

        public static partial void kaizensharp_packed_vector4_array_destroy(ref kaizen_packed_vector4_array p_self);

        public static partial void kaizensharp_packed_color_array_destroy(ref kaizen_packed_color_array p_self);

        public static partial void kaizensharp_variant_destroy(ref kaizen_variant p_self);

        public static partial void kaizensharp_string_destroy(ref kaizen_string p_self);

        public static partial void kaizensharp_string_name_destroy(ref kaizen_string_name p_self);

        public static partial void kaizensharp_node_path_destroy(ref kaizen_node_path p_self);

        public static partial void kaizensharp_signal_destroy(ref kaizen_signal p_self);

        public static partial void kaizensharp_callable_destroy(ref kaizen_callable p_self);

        public static partial void kaizensharp_array_destroy(ref kaizen_array p_self);

        public static partial void kaizensharp_dictionary_destroy(ref kaizen_dictionary p_self);

        // Array

        public static partial int kaizensharp_array_add(ref kaizen_array p_self, in kaizen_variant p_item);

        public static partial int kaizensharp_array_add_range(ref kaizen_array p_self, in kaizen_array p_collection);

        public static partial int kaizensharp_array_binary_search(ref kaizen_array p_self, int p_index, int p_count, in kaizen_variant p_value);

        public static partial void kaizensharp_array_duplicate(scoped ref kaizen_array p_self, kaizen_bool p_deep, out kaizen_array r_dest);

        public static partial void kaizensharp_array_fill(ref kaizen_array p_self, in kaizen_variant p_value);

        public static partial int kaizensharp_array_index_of(ref kaizen_array p_self, in kaizen_variant p_item, int p_index = 0);

        public static partial void kaizensharp_array_insert(ref kaizen_array p_self, int p_index, in kaizen_variant p_item);

        public static partial int kaizensharp_array_last_index_of(ref kaizen_array p_self, in kaizen_variant p_item, int p_index);

        public static partial void kaizensharp_array_make_read_only(ref kaizen_array p_self);

        public static partial void kaizensharp_array_set_typed(
            ref kaizen_array p_self,
            uint p_elem_type,
            in kaizen_string_name p_elem_class_name,
            in kaizen_ref p_elem_script);

        public static partial kaizen_bool kaizensharp_array_is_typed(ref kaizen_array p_self);

        public static partial void kaizensharp_array_max(scoped ref kaizen_array p_self, out kaizen_variant r_value);

        public static partial void kaizensharp_array_min(scoped ref kaizen_array p_self, out kaizen_variant r_value);

        public static partial void kaizensharp_array_pick_random(scoped ref kaizen_array p_self, out kaizen_variant r_value);

        public static partial kaizen_bool kaizensharp_array_recursive_equal(ref kaizen_array p_self, in kaizen_array p_other);

        public static partial void kaizensharp_array_remove_at(ref kaizen_array p_self, int p_index);

        public static partial Error kaizensharp_array_resize(ref kaizen_array p_self, int p_new_size);

        public static partial void kaizensharp_array_reverse(ref kaizen_array p_self);

        public static partial void kaizensharp_array_shuffle(ref kaizen_array p_self);

        public static partial void kaizensharp_array_slice(scoped ref kaizen_array p_self, int p_start, int p_end,
            int p_step, kaizen_bool p_deep, out kaizen_array r_dest);

        public static partial void kaizensharp_array_sort(ref kaizen_array p_self);

        public static partial void kaizensharp_array_to_string(ref kaizen_array p_self, out kaizen_string r_str);

        public static partial void kaizensharp_packed_byte_array_compress(scoped in kaizen_packed_byte_array p_src, int p_mode, out kaizen_packed_byte_array r_dst);

        public static partial void kaizensharp_packed_byte_array_decompress(scoped in kaizen_packed_byte_array p_src, long p_buffer_size, int p_mode, out kaizen_packed_byte_array r_dst);

        public static partial void kaizensharp_packed_byte_array_decompress_dynamic(scoped in kaizen_packed_byte_array p_src, long p_buffer_size, int p_mode, out kaizen_packed_byte_array r_dst);

        // Dictionary

        public static partial kaizen_bool kaizensharp_dictionary_try_get_value(scoped ref kaizen_dictionary p_self,
            scoped in kaizen_variant p_key,
            out kaizen_variant r_value);

        public static partial void kaizensharp_dictionary_set_value(ref kaizen_dictionary p_self, in kaizen_variant p_key,
            in kaizen_variant p_value);

        public static partial void kaizensharp_dictionary_keys(scoped ref kaizen_dictionary p_self, out kaizen_array r_dest);

        public static partial void kaizensharp_dictionary_values(scoped ref kaizen_dictionary p_self, out kaizen_array r_dest);

        public static partial int kaizensharp_dictionary_count(ref kaizen_dictionary p_self);

        public static partial void kaizensharp_dictionary_key_value_pair_at(scoped ref kaizen_dictionary p_self, int p_index,
            out kaizen_variant r_key, out kaizen_variant r_value);

        public static partial void kaizensharp_dictionary_add(ref kaizen_dictionary p_self, in kaizen_variant p_key,
            in kaizen_variant p_value);

        public static partial void kaizensharp_dictionary_clear(ref kaizen_dictionary p_self);

        public static partial kaizen_bool kaizensharp_dictionary_contains_key(ref kaizen_dictionary p_self,
            in kaizen_variant p_key);

        public static partial void kaizensharp_dictionary_duplicate(scoped ref kaizen_dictionary p_self, kaizen_bool p_deep,
            out kaizen_dictionary r_dest);

        public static partial void kaizensharp_dictionary_merge(ref kaizen_dictionary p_self, in kaizen_dictionary p_dictionary, kaizen_bool p_overwrite);

        public static partial kaizen_bool kaizensharp_dictionary_recursive_equal(ref kaizen_dictionary p_self, in kaizen_dictionary p_other);

        public static partial kaizen_bool kaizensharp_dictionary_remove_key(ref kaizen_dictionary p_self,
            in kaizen_variant p_key);

        public static partial void kaizensharp_dictionary_make_read_only(ref kaizen_dictionary p_self);

        public static partial void kaizensharp_dictionary_set_typed(
            ref kaizen_dictionary p_self,
            uint p_key_type,
            in kaizen_string_name p_key_class_name,
            in kaizen_ref p_key_script,
            uint p_value_type,
            in kaizen_string_name p_value_class_name,
            in kaizen_ref p_value_script);

        public static partial kaizen_bool kaizensharp_dictionary_is_typed_key(ref kaizen_dictionary p_self);

        public static partial kaizen_bool kaizensharp_dictionary_is_typed_value(ref kaizen_dictionary p_self);

        public static partial uint kaizensharp_dictionary_get_typed_key_builtin(ref kaizen_dictionary p_self);

        public static partial uint kaizensharp_dictionary_get_typed_value_builtin(ref kaizen_dictionary p_self);

        public static partial void kaizensharp_dictionary_get_typed_key_class_name(ref kaizen_dictionary p_self, out kaizen_string_name r_dest);

        public static partial void kaizensharp_dictionary_get_typed_value_class_name(ref kaizen_dictionary p_self, out kaizen_string_name r_dest);

        public static partial void kaizensharp_dictionary_get_typed_key_script(ref kaizen_dictionary p_self, out kaizen_variant r_dest);

        public static partial void kaizensharp_dictionary_get_typed_value_script(ref kaizen_dictionary p_self, out kaizen_variant r_dest);

        public static partial void kaizensharp_dictionary_to_string(scoped ref kaizen_dictionary p_self, out kaizen_string r_str);

        // StringExtensions

        public static partial void kaizensharp_string_simplify_path(scoped in kaizen_string p_self,
            out kaizen_string r_simplified_path);

        public static partial void kaizensharp_string_capitalize(scoped in kaizen_string p_self,
            out kaizen_string r_capitalized);

        public static partial void kaizensharp_string_to_camel_case(scoped in kaizen_string p_self,
            out kaizen_string r_camel_case);

        public static partial void kaizensharp_string_to_pascal_case(scoped in kaizen_string p_self,
            out kaizen_string r_pascal_case);

        public static partial void kaizensharp_string_to_snake_case(scoped in kaizen_string p_self,
            out kaizen_string r_snake_case);

        public static partial void kaizensharp_string_to_kebab_case(scoped in kaizen_string p_self,
            out kaizen_string r_kebab_case);

        // NodePath

        public static partial void kaizensharp_node_path_get_as_property_path(in kaizen_node_path p_self,
            ref kaizen_node_path r_dest);

        public static partial void kaizensharp_node_path_get_concatenated_names(scoped in kaizen_node_path p_self,
            out kaizen_string r_names);

        public static partial void kaizensharp_node_path_get_concatenated_subnames(scoped in kaizen_node_path p_self,
            out kaizen_string r_subnames);

        public static partial void kaizensharp_node_path_get_name(scoped in kaizen_node_path p_self, int p_idx,
            out kaizen_string r_name);

        public static partial int kaizensharp_node_path_get_name_count(in kaizen_node_path p_self);

        public static partial void kaizensharp_node_path_get_subname(scoped in kaizen_node_path p_self, int p_idx,
            out kaizen_string r_subname);

        public static partial int kaizensharp_node_path_get_subname_count(in kaizen_node_path p_self);

        public static partial kaizen_bool kaizensharp_node_path_is_absolute(in kaizen_node_path p_self);

        public static partial kaizen_bool kaizensharp_node_path_equals(in kaizen_node_path p_self, in kaizen_node_path p_other);

        public static partial int kaizensharp_node_path_hash(in kaizen_node_path p_self);

        // GD, etc

        internal static partial void kaizensharp_bytes_to_var(scoped in kaizen_packed_byte_array p_bytes,
            kaizen_bool p_allow_objects,
            out kaizen_variant r_ret);

        internal static partial void kaizensharp_convert(scoped in kaizen_variant p_what, int p_type,
            out kaizen_variant r_ret);

        internal static partial int kaizensharp_hash(in kaizen_variant p_var);

        internal static partial IntPtr kaizensharp_instance_from_id(ulong p_instance_id);

        internal static partial void kaizensharp_print(in kaizen_string p_what);

        public static partial void kaizensharp_print_rich(in kaizen_string p_what);

        internal static partial void kaizensharp_printerr(in kaizen_string p_what);

        internal static partial void kaizensharp_printraw(in kaizen_string p_what);

        internal static partial void kaizensharp_prints(in kaizen_string p_what);

        internal static partial void kaizensharp_printt(in kaizen_string p_what);

        internal static partial float kaizensharp_randf();

        internal static partial uint kaizensharp_randi();

        internal static partial void kaizensharp_randomize();

        internal static partial double kaizensharp_randf_range(double from, double to);

        internal static partial double kaizensharp_randfn(double mean, double deviation);

        internal static partial int kaizensharp_randi_range(int from, int to);

        internal static partial uint kaizensharp_rand_from_seed(ulong seed, out ulong newSeed);

        internal static partial void kaizensharp_seed(ulong seed);

        internal static partial void kaizensharp_weakref(IntPtr p_obj, out kaizen_ref r_weak_ref);

        internal static partial void kaizensharp_str_to_var(scoped in kaizen_string p_str, out kaizen_variant r_ret);

        internal static partial void kaizensharp_var_to_bytes(scoped in kaizen_variant p_what, kaizen_bool p_full_objects,
            out kaizen_packed_byte_array r_bytes);

        internal static partial void kaizensharp_var_to_str(scoped in kaizen_variant p_var, out kaizen_string r_ret);

        internal static partial void kaizensharp_err_print_error(in kaizen_string p_function, in kaizen_string p_file, int p_line, in kaizen_string p_error, in kaizen_string p_message = default, kaizen_bool p_editor_notify = kaizen_bool.False, kaizen_error_handler_type p_type = kaizen_error_handler_type.ERR_HANDLER_ERROR);

        // Object

        public static partial void kaizensharp_object_to_string(IntPtr ptr, out kaizen_string r_str);
    }
}
