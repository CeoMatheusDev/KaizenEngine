using Kaizen;
using Kaizen.NativeInterop;

partial class InheritanceChild
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::InheritanceBase.PropertyName {
        /// <summary>
        /// Cached name for the 'MyString' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyString = "MyString";
        /// <summary>
        /// Cached name for the 'MyInteger' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyInteger = "MyInteger";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static kaizen_variant trampoline_get_MyString(object kaizenObject)
            {
                var ret = ((global::InheritanceChild)kaizenObject).@MyString;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<string>(ret);
            }
            static void trampoline_set_MyString(object kaizenObject, in kaizen_variant value)
            {
                ((global::InheritanceChild)kaizenObject).@MyString = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<string>(value);
            }
            static kaizen_variant trampoline_get_MyInteger(object kaizenObject)
            {
                var ret = ((global::InheritanceChild)kaizenObject).@MyInteger;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<int>(ret);
            }
            static void trampoline_set_MyInteger(object kaizenObject, in kaizen_variant value)
            {
                ((global::InheritanceChild)kaizenObject).@MyInteger = global::Kaizen.NativeInterop.VariantUtils.ConvertTo<int>(value);
            }
            var aux_delegate_get_MyString = trampoline_get_MyString;
            var aux_delegate_set_MyString = trampoline_set_MyString;
            collector.TryAdd(PropertyName.@MyString, new(aux_delegate_get_MyString.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set_MyString.Method.MethodHandle.GetFunctionPointer()));
            var aux_delegate_get_MyInteger = trampoline_get_MyInteger;
            var aux_delegate_set_MyInteger = trampoline_set_MyInteger;
            collector.TryAdd(PropertyName.@MyInteger, new(aux_delegate_get_MyInteger.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set_MyInteger.Method.MethodHandle.GetFunctionPointer()));
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
        properties.Add(new(type: (global::Kaizen.Variant.Type)4, name: PropertyName.@MyString, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4096, exported: false));
        properties.Add(new(type: (global::Kaizen.Variant.Type)2, name: PropertyName.@MyInteger, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4102, exported: true));
        return properties;
    }
#pragma warning restore CS0109
}
