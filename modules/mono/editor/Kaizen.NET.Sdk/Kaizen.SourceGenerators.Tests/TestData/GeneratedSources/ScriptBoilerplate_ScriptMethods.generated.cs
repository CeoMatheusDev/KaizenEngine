using Kaizen;
using Kaizen.NativeInterop;

partial class ScriptBoilerplate
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : global::Kaizen.Node.MethodName {
        /// <summary>
        /// Cached name for the '_Process' method.
        /// </summary>
        public new static readonly global::Kaizen.StringName @_Process = "_Process";
        /// <summary>
        /// Cached name for the 'Bazz' method.
        /// </summary>
        public new static readonly global::Kaizen.StringName @Bazz = "Bazz";
    }
    /// <summary>
    /// Get the method information for all the methods declared in this class.
    /// This method is used by Kaizen to register the available methods in the editor.
    /// Do not call this method.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo> GetKaizenMethodList()
    {
        var methods = new global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo>(2);
        methods.Add(new(name: MethodName.@_Process, returnVal: new(type: (global::Kaizen.Variant.Type)0, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: new() { new(type: (global::Kaizen.Variant.Type)3, name: "delta", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false),  }, defaultArguments: null));
        methods.Add(new(name: MethodName.@Bazz, returnVal: new(type: (global::Kaizen.Variant.Type)2, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: new() { new(type: (global::Kaizen.Variant.Type)21, name: "name", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false),  }, defaultArguments: null));
        return methods;
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenMethodTrampolines(global::Kaizen.Bridge.MethodTrampolineCollector collector)
        {
            static kaizen_variant trampoline_1__Process(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 1) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 1, provided: args.Count);
                    return default;
                }
                ((global::Kaizen.Node)kaizenObject).@_Process(global::Kaizen.NativeInterop.VariantUtils.ConvertTo<double>(args[0]));
                return default;
            }
            var aux_delegate_1__Process = trampoline_1__Process;
            collector.TryAdd(new(MethodName.@_Process, 1), new(aux_delegate_1__Process.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
            collector.TryAdd(new(global::Kaizen.Node.MethodName.@_Process, 1), new(aux_delegate_1__Process.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
            static kaizen_variant trampoline_1_Bazz(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 1) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 1, provided: args.Count);
                    return default;
                }
                var callRet = ((global::ScriptBoilerplate)kaizenObject).@Bazz(global::Kaizen.NativeInterop.VariantUtils.ConvertTo<global::Kaizen.StringName>(args[0]));
                return global::Kaizen.NativeInterop.VariantUtils.CreateFrom<int>(callRet);
            }
            var aux_delegate_1_Bazz = trampoline_1_Bazz;
            collector.TryAdd(new(MethodName.@Bazz, 1), new(aux_delegate_1_Bazz.Method.MethodHandle.GetFunctionPointer(), isStatic: false));
        }
    }
#pragma warning restore CS0109
}
