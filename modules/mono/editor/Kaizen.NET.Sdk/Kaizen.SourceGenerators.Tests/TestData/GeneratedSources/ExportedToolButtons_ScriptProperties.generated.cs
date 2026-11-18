using Kaizen;
using Kaizen.NativeInterop;

partial class ExportedToolButtons
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::Kaizen.KaizenObject.PropertyName {
        /// <summary>
        /// Cached name for the 'MyButton1' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButton1 = "MyButton1";
        /// <summary>
        /// Cached name for the 'MyButton2' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButton2 = "MyButton2";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static kaizen_variant trampoline_get_MyButton1(object kaizenObject)
            {
                var ret = ((global::ExportedToolButtons)kaizenObject).@MyButton1;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            static kaizen_variant trampoline_get_MyButton2(object kaizenObject)
            {
                var ret = ((global::ExportedToolButtons)kaizenObject).@MyButton2;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            var aux_delegate_get_MyButton1 = trampoline_get_MyButton1;
            collector.TryAdd(PropertyName.@MyButton1, new(aux_delegate_get_MyButton1.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
            var aux_delegate_get_MyButton2 = trampoline_get_MyButton2;
            collector.TryAdd(PropertyName.@MyButton2, new(aux_delegate_get_MyButton2.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
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
        properties.Add(new(type: (global::Kaizen.Variant.Type)25, name: PropertyName.@MyButton1, hint: (global::Kaizen.PropertyHint)39, hintString: "Click me!", usage: (global::Kaizen.PropertyUsageFlags)4, exported: true));
        properties.Add(new(type: (global::Kaizen.Variant.Type)25, name: PropertyName.@MyButton2, hint: (global::Kaizen.PropertyHint)39, hintString: "Click me!,ColorRect", usage: (global::Kaizen.PropertyUsageFlags)4, exported: true));
        return properties;
    }
#pragma warning restore CS0109
}
