using Kaizen;
using Kaizen.NativeInterop;

partial class AllWriteOnly
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::Kaizen.KaizenObject.PropertyName {
        /// <summary>
        /// Cached name for the 'WriteOnlyProperty' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @WriteOnlyProperty = "WriteOnlyProperty";
        /// <summary>
        /// Cached name for the '_writeOnlyBackingField' field.
        /// </summary>
        public new static readonly global::Kaizen.StringName @_writeOnlyBackingField = "_writeOnlyBackingField";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static void trampoline_set_WriteOnlyProperty(object kaizenObject, in kaizen_variant value)
            {
                ((global::AllWriteOnly)kaizenObject).@WriteOnlyProperty = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<bool>(value);
            }
            static kaizen_variant trampoline_get__writeOnlyBackingField(object kaizenObject)
            {
                var ret = ((global::AllWriteOnly)kaizenObject).@_writeOnlyBackingField;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<bool>(ret);
            }
            static void trampoline_set__writeOnlyBackingField(object kaizenObject, in kaizen_variant value)
            {
                ((global::AllWriteOnly)kaizenObject).@_writeOnlyBackingField = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<bool>(value);
            }
            var aux_delegate_set_WriteOnlyProperty = trampoline_set_WriteOnlyProperty;
            collector.TryAdd(PropertyName.@WriteOnlyProperty, new(global::System.IntPtr.Zero), new(aux_delegate_set_WriteOnlyProperty.Method.MethodHandle.GetFunctionPointer()));
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
        properties.Add(new(type: (global::Kaizen.Variant.Type)1, name: PropertyName.@_writeOnlyBackingField, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)1, name: PropertyName.@WriteOnlyProperty, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        return properties;
    }
#pragma warning restore CS0109
}
