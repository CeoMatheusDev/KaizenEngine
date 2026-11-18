using Kaizen;
using Kaizen.NativeInterop;

partial struct OuterClass
{
partial class NestedClass
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : global::Kaizen.RefCounted.MethodName {
        /// <summary>
        /// Cached name for the '_Get' method.
        /// </summary>
        public new static readonly global::Kaizen.StringName @_Get = "_Get";
    }
    /// <summary>
    /// Get the method information for all the methods declared in this class.
    /// This method is used by Kaizen to register the available methods in the editor.
    /// Do not call this method.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo> GetKaizenMethodList()
    {
        var methods = new global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo>(1);
        methods.Add(new(name: MethodName.@_Get, returnVal: new(type: (global::Kaizen.Variant.Type)0, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)131078, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: new() { new(type: (global::Kaizen.Variant.Type)21, name: "property", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false),  }, defaultArguments: null));
        return methods;
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenMethodTrampolines(global::Kaizen.Bridge.MethodTrampolineCollector collector)
        {
            static kaizen_variant trampoline_1__Get(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 1) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 1, provided: args.Count);
                    return default;
                }
                var callRet = ((global::Kaizen.KaizenObject)kaizenObject).@_Get(global::Kaizen.NativeInterop.VariantUtils.ConvertTo<global::Kaizen.StringName>(args[0]));
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<global::Kaizen.Variant>(callRet);
            }
            var aux_delegate_1__Get = trampoline_1__Get;
            collector.TryAdd(new(MethodName.@_Get, 1), new(aux_delegate_1__Get.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
            collector.TryAdd(new(global::Kaizen.KaizenObject.MethodName.@_Get, 1), new(aux_delegate_1__Get.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
        }
    }
#pragma warning restore CS0109
}
}
