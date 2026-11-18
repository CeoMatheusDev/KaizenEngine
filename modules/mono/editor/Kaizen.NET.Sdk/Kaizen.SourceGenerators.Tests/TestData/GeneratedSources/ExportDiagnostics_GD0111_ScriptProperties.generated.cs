using Kaizen;
using Kaizen.NativeInterop;

partial class ExportDiagnostics_GD0111
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::Kaizen.Node.PropertyName {
        /// <summary>
        /// Cached name for the 'MyButtonGet' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonGet = "MyButtonGet";
        /// <summary>
        /// Cached name for the 'MyButtonGetSet' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonGetSet = "MyButtonGetSet";
        /// <summary>
        /// Cached name for the 'MyButtonGetWithBackingField' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonGetWithBackingField = "MyButtonGetWithBackingField";
        /// <summary>
        /// Cached name for the 'MyButtonGetSetWithBackingField' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonGetSetWithBackingField = "MyButtonGetSetWithBackingField";
        /// <summary>
        /// Cached name for the 'MyButtonOkWithCallableCreationExpression' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonOkWithCallableCreationExpression = "MyButtonOkWithCallableCreationExpression";
        /// <summary>
        /// Cached name for the 'MyButtonOkWithImplicitCallableCreationExpression' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonOkWithImplicitCallableCreationExpression = "MyButtonOkWithImplicitCallableCreationExpression";
        /// <summary>
        /// Cached name for the 'MyButtonOkWithCallableFromExpression' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButtonOkWithCallableFromExpression = "MyButtonOkWithCallableFromExpression";
        /// <summary>
        /// Cached name for the '_backingField' field.
        /// </summary>
        public new static readonly global::Kaizen.StringName @_backingField = "_backingField";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static kaizen_variant trampoline_get_MyButtonGet(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonGet;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static kaizen_variant trampoline_get_MyButtonGetSet(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonGetSet;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static void trampoline_set_MyButtonGetSet(object kaizenObject, in kaizen_variant value)
            {
                ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonGetSet = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<global::Kaizen.Callable>(value);
            }
            static kaizen_variant trampoline_get_MyButtonGetWithBackingField(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonGetWithBackingField;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static kaizen_variant trampoline_get_MyButtonGetSetWithBackingField(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonGetSetWithBackingField;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static void trampoline_set_MyButtonGetSetWithBackingField(object kaizenObject, in kaizen_variant value)
            {
                ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonGetSetWithBackingField = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<global::Kaizen.Callable>(value);
            }
            static kaizen_variant trampoline_get_MyButtonOkWithCallableCreationExpression(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonOkWithCallableCreationExpression;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static kaizen_variant trampoline_get_MyButtonOkWithImplicitCallableCreationExpression(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonOkWithImplicitCallableCreationExpression;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static kaizen_variant trampoline_get_MyButtonOkWithCallableFromExpression(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@MyButtonOkWithCallableFromExpression;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static kaizen_variant trampoline_get__backingField(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0111)kaizenObject).@_backingField;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static void trampoline_set__backingField(object kaizenObject, in kaizen_variant value)
            {
                ((global::ExportDiagnostics_GD0111)kaizenObject).@_backingField = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<global::Kaizen.Callable>(value);
            }
            var aux_delegate_get_MyButtonGet = trampoline_get_MyButtonGet;
            collector.TryAdd(PropertyName.@MyButtonGet, new(aux_delegate_get_MyButtonGet.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_MyButtonGetSet = trampoline_get_MyButtonGetSet;
            var aux_delegate_set_MyButtonGetSet = trampoline_set_MyButtonGetSet;
            collector.TryAdd(PropertyName.@MyButtonGetSet, new(aux_delegate_get_MyButtonGetSet.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set_MyButtonGetSet.Method.MethodHandle.GetFunctionPointer()));
            var aux_delegate_get_MyButtonGetWithBackingField = trampoline_get_MyButtonGetWithBackingField;
            collector.TryAdd(PropertyName.@MyButtonGetWithBackingField, new(aux_delegate_get_MyButtonGetWithBackingField.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_MyButtonGetSetWithBackingField = trampoline_get_MyButtonGetSetWithBackingField;
            var aux_delegate_set_MyButtonGetSetWithBackingField = trampoline_set_MyButtonGetSetWithBackingField;
            collector.TryAdd(PropertyName.@MyButtonGetSetWithBackingField, new(aux_delegate_get_MyButtonGetSetWithBackingField.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set_MyButtonGetSetWithBackingField.Method.MethodHandle.GetFunctionPointer()));
            var aux_delegate_get_MyButtonOkWithCallableCreationExpression = trampoline_get_MyButtonOkWithCallableCreationExpression;
            collector.TryAdd(PropertyName.@MyButtonOkWithCallableCreationExpression, new(aux_delegate_get_MyButtonOkWithCallableCreationExpression.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_MyButtonOkWithImplicitCallableCreationExpression = trampoline_get_MyButtonOkWithImplicitCallableCreationExpression;
            collector.TryAdd(PropertyName.@MyButtonOkWithImplicitCallableCreationExpression, new(aux_delegate_get_MyButtonOkWithImplicitCallableCreationExpression.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_MyButtonOkWithCallableFromExpression = trampoline_get_MyButtonOkWithCallableFromExpression;
            collector.TryAdd(PropertyName.@MyButtonOkWithCallableFromExpression, new(aux_delegate_get_MyButtonOkWithCallableFromExpression.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get__backingField = trampoline_get__backingField;
            var aux_delegate_set__backingField = trampoline_set__backingField;
            collector.TryAdd(PropertyName.@_backingField, new(aux_delegate_get__backingField.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set__backingField.Method.MethodHandle.GetFunctionPointer()));
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
        properties.Add(new(type: (global::Kaizen.Variant.Type)25, name: PropertyName.@_backingField, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)25, name: PropertyName.@MyButtonOkWithCallableCreationExpression, hint: (global::Kaizen.PropertyHint)39, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4, exported: true));
        properties.Add(new(type: (global::Kaizen.Variant.Type)25, name: PropertyName.@MyButtonOkWithImplicitCallableCreationExpression, hint: (global::Kaizen.PropertyHint)39, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4, exported: true));
        properties.Add(new(type: (global::Kaizen.Variant.Type)25, name: PropertyName.@MyButtonOkWithCallableFromExpression, hint: (global::Kaizen.PropertyHint)39, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4, exported: true));
        return properties;
    }
#pragma warning restore CS0109
}
