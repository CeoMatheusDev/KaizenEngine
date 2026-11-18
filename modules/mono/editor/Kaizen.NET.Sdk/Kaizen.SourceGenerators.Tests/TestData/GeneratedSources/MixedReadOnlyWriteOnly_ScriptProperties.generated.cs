using Kaizen;
using Kaizen.NativeInterop;

partial class MixedReadOnlyWriteOnly
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::Kaizen.KaizenObject.PropertyName {
        /// <summary>
        /// Cached name for the 'ReadOnlyAutoProperty' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @ReadOnlyAutoProperty = "ReadOnlyAutoProperty";
        /// <summary>
        /// Cached name for the 'ReadOnlyProperty' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @ReadOnlyProperty = "ReadOnlyProperty";
        /// <summary>
        /// Cached name for the 'InitOnlyAutoProperty' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @InitOnlyAutoProperty = "InitOnlyAutoProperty";
        /// <summary>
        /// Cached name for the 'WriteOnlyProperty' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @WriteOnlyProperty = "WriteOnlyProperty";
        /// <summary>
        /// Cached name for the 'ReadOnlyField' field.
        /// </summary>
        public new static readonly global::Kaizen.StringName @ReadOnlyField = "ReadOnlyField";
        /// <summary>
        /// Cached name for the '_writeOnlyBackingField' field.
        /// </summary>
        public new static readonly global::Kaizen.StringName @_writeOnlyBackingField = "_writeOnlyBackingField";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static kaizen_variant trampoline_get_ReadOnlyAutoProperty(object kaizenObject)
            {
                var ret = ((global::MixedReadOnlyWriteOnly)kaizenObject).@ReadOnlyAutoProperty;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<string>(ret);
            }
            static kaizen_variant trampoline_get_ReadOnlyProperty(object kaizenObject)
            {
                var ret = ((global::MixedReadOnlyWriteOnly)kaizenObject).@ReadOnlyProperty;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<string>(ret);
            }
            static kaizen_variant trampoline_get_InitOnlyAutoProperty(object kaizenObject)
            {
                var ret = ((global::MixedReadOnlyWriteOnly)kaizenObject).@InitOnlyAutoProperty;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<string>(ret);
            }
            static void trampoline_set_WriteOnlyProperty(object kaizenObject, in kaizen_variant value)
            {
                ((global::MixedReadOnlyWriteOnly)kaizenObject).@WriteOnlyProperty = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<bool>(value);
            }
            static kaizen_variant trampoline_get_ReadOnlyField(object kaizenObject)
            {
                var ret = ((global::MixedReadOnlyWriteOnly)kaizenObject).@ReadOnlyField;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<string>(ret);
            }
            static kaizen_variant trampoline_get__writeOnlyBackingField(object kaizenObject)
            {
                var ret = ((global::MixedReadOnlyWriteOnly)kaizenObject).@_writeOnlyBackingField;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<bool>(ret);
            }
            static void trampoline_set__writeOnlyBackingField(object kaizenObject, in kaizen_variant value)
            {
                ((global::MixedReadOnlyWriteOnly)kaizenObject).@_writeOnlyBackingField = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<bool>(value);
            }
            var aux_delegate_get_ReadOnlyAutoProperty = trampoline_get_ReadOnlyAutoProperty;
            collector.TryAdd(PropertyName.@ReadOnlyAutoProperty, new(aux_delegate_get_ReadOnlyAutoProperty.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_ReadOnlyProperty = trampoline_get_ReadOnlyProperty;
            collector.TryAdd(PropertyName.@ReadOnlyProperty, new(aux_delegate_get_ReadOnlyProperty.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_InitOnlyAutoProperty = trampoline_get_InitOnlyAutoProperty;
            collector.TryAdd(PropertyName.@InitOnlyAutoProperty, new(aux_delegate_get_InitOnlyAutoProperty.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_set_WriteOnlyProperty = trampoline_set_WriteOnlyProperty;
            collector.TryAdd(PropertyName.@WriteOnlyProperty, new(global::System.IntPtr.Zero), new(aux_delegate_set_WriteOnlyProperty.Method.MethodHandle.GetFunctionPointer()));
            var aux_delegate_get_ReadOnlyField = trampoline_get_ReadOnlyField;
            collector.TryAdd(PropertyName.@ReadOnlyField, new(aux_delegate_get_ReadOnlyField.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get__writeOnlyBackingField = trampoline_get__writeOnlyBackingField;
            var aux_delegate_set__writeOnlyBackingField = trampoline_set__writeOnlyBackingField;
            collector.TryAdd(PropertyName.@_writeOnlyBackingField, new(aux_delegate_get__writeOnlyBackingField.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set__writeOnlyBackingField.Method.MethodHandle.GetFunctionPointer()));
        }
    }
    /// <summary>
    /// Get the property information for all the properties declared in this class.
    /// This method is used by Kaizen to register the available properties in the editor.
    /// Do not call this method.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.List<global::Kaizen.Bridge.PropertyInfo> GetKaizenPropertyList()
    {
        var properties = new global::System.Collections.Generic.List<global::Kaizen.Bridge.PropertyInfo>();
        properties.Add(new(type: (global::Kaizen.Variant.Type)4, name: PropertyName.@ReadOnlyField, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)4, name: PropertyName.@ReadOnlyAutoProperty, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)4, name: PropertyName.@ReadOnlyProperty, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)4, name: PropertyName.@InitOnlyAutoProperty, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)1, name: PropertyName.@_writeOnlyBackingField, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)1, name: PropertyName.@WriteOnlyProperty, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        return properties;
    }
#pragma warning restore CS0109
}
