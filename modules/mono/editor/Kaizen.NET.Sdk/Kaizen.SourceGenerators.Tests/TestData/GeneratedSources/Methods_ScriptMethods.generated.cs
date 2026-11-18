using Kaizen;
using Kaizen.NativeInterop;

partial class Methods
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : global::Kaizen.KaizenObject.MethodName {
        /// <summary>
        /// Cached name for the 'MethodWithOverload' method.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MethodWithOverload = "MethodWithOverload";
    }
    /// <summary>
    /// Get the method information for all the methods declared in this class.
    /// This method is used by Kaizen to register the available methods in the editor.
    /// Do not call this method.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo> GetKaizenMethodList()
    {
        var methods = new global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo>(3);
        methods.Add(new(name: MethodName.@MethodWithOverload, returnVal: new(type: (global::Kaizen.Variant.Type)0, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: null, defaultArguments: null));
        methods.Add(new(name: MethodName.@MethodWithOverload, returnVal: new(type: (global::Kaizen.Variant.Type)0, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: new() { new(type: (global::Kaizen.Variant.Type)2, name: "a", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false),  }, defaultArguments: null));
        methods.Add(new(name: MethodName.@MethodWithOverload, returnVal: new(type: (global::Kaizen.Variant.Type)0, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: new() { new(type: (global::Kaizen.Variant.Type)2, name: "a", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), new(type: (global::Kaizen.Variant.Type)2, name: "b", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false),  }, defaultArguments: null));
        return methods;
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenMethodTrampolines(global::Kaizen.Bridge.MethodTrampolineCollector collector)
        {
            static kaizen_variant trampoline_0_MethodWithOverload(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 0) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 0, provided: args.Count);
                    return default;
                }
                ((global::Methods)kaizenObject).@MethodWithOverload();
                return default;
            }
            var aux_delegate_0_MethodWithOverload = trampoline_0_MethodWithOverload;
            collector.TryAdd(new(MethodName.@MethodWithOverload, 0), new(aux_delegate_0_MethodWithOverload.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
            static kaizen_variant trampoline_1_MethodWithOverload(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 1) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 1, provided: args.Count);
                    return default;
                }
                ((global::Methods)kaizenObject).@MethodWithOverload(global::Kaizen.NativeInterop.VariantUtils.ConvertTo<int>(args[0]));
                return default;
            }
            var aux_delegate_1_MethodWithOverload = trampoline_1_MethodWithOverload;
            collector.TryAdd(new(MethodName.@MethodWithOverload, 1), new(aux_delegate_1_MethodWithOverload.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
            static kaizen_variant trampoline_2_MethodWithOverload(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 2) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 2, provided: args.Count);
                    return default;
                }
                ((global::Methods)kaizenObject).@MethodWithOverload(global::Kaizen.NativeInterop.VariantUtils.ConvertTo<int>(args[0]), global::Kaizen.NativeInterop.VariantUtils.ConvertTo<int>(args[1]));
                return default;
            }
            var aux_delegate_2_MethodWithOverload = trampoline_2_MethodWithOverload;
            collector.TryAdd(new(MethodName.@MethodWithOverload, 2), new(aux_delegate_2_MethodWithOverload.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
        }
    }
#pragma warning restore CS0109
}
