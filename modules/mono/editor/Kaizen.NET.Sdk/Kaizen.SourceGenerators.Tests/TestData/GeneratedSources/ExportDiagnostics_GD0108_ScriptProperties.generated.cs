using Kaizen;
using Kaizen.NativeInterop;

partial class ExportDiagnostics_GD0108
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : global::Kaizen.Node.PropertyName {
        /// <summary>
        /// Cached name for the 'MyButton' property.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MyButton = "MyButton";
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenPropertyTrampolines(global::Kaizen.Bridge.PropertyTrampolineCollector collector)
        {
            static kaizen_variant trampoline_get_MyButton(object kaizenObject)
            {
                var ret = ((global::ExportDiagnostics_GD0108)kaizenObject).@MyButton;
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Callable>(ret);
            }
            var aux_delegate_get_MyButton = trampoline_get_MyButton;
            collector.TryAdd(PropertyName.@MyButton, new(aux_delegate_get_MyButton.Method.MethodHandle.GetFunctionPointer()), new(global::System.IntPtr.Zero));
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
        return properties;
    }
#pragma warning restore CS0109
}
