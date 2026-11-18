using Kaizen;
using Kaizen.NativeInterop;

partial class AbstractGenericNode<T>
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::Kaizen.Node.PropertyName {
        /// <summary>
        /// Cached name for the 'MyArray' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyArray = "MyArray";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static kaizen_variant trampoline_get_MyArray(object kaizenObject)
            {
                var ret = ((global::AbstractGenericNode<T>)kaizenObject).@MyArray;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFromArray(ret);
            }
            static void trampoline_set_MyArray(object kaizenObject, in kaizen_variant value)
            {
                ((global::AbstractGenericNode<T>)kaizenObject).@MyArray = global::Kaizen.NativeInterop.VariantUtils.ConvertToArray<T>(value);
            }
            var aux_delegate_get_MyArray = trampoline_get_MyArray;
            var aux_delegate_set_MyArray = trampoline_set_MyArray;
            collector.TryAdd(PropertyName.@MyArray, new(aux_delegate_get_MyArray.Method.MethodHandle.GetFunctionPointer()), new(aux_delegate_set_MyArray.Method.MethodHandle.GetFunctionPointer()));
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
        properties.Add(new(type: (global::Kaizen.Variant.Type)28, name: PropertyName.@MyArray, hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)4102, exported: true));
        return properties;
    }
#pragma warning restore CS0109
}
