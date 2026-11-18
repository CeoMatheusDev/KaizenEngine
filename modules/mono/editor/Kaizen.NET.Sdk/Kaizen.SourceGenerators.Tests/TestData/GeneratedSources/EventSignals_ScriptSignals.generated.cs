using Kaizen;
using Kaizen.NativeInterop;

partial class EventSignals
{
#pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : global::Kaizen.KaizenObject.SignalName {
        /// <summary>
        /// Cached name for the 'MySignal' signal.
        /// </summary>
        public new static readonly global::Kaizen.StringName @MySignal = "MySignal";
    }
    /// <summary>
    /// Get the signal information for all the signals declared in this class.
    /// This method is used by Kaizen to register the available signals in the editor.
    /// Do not call this method.
    /// </summary>
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal new static global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo> GetKaizenSignalList()
    {
        var signals = new global::System.Collections.Generic.List<global::Kaizen.Bridge.MethodInfo>(1);
        signals.Add(new(name: SignalName.@MySignal, returnVal: new(type: (global::Kaizen.Variant.Type)0, name: "", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), flags: (global::Kaizen.MethodFlags)1, arguments: new() { new(type: (global::Kaizen.Variant.Type)4, name: "str", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false), new(type: (global::Kaizen.Variant.Type)2, name: "num", hint: (global::Kaizen.PropertyHint)0, hintString: "", usage: (global::Kaizen.PropertyUsageFlags)6, exported: false),  }, defaultArguments: null));
        return signals;
    }
    protected internal new static partial class KaizenInternal
    {
        public static void GetKaizenRaiseSignalTrampolines(global::Kaizen.Bridge.RaiseSignalTrampolineCollector collector)
        {
            static void trampoline_2_MySignal(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError)
            {
                if (args.Count != 2) {
                    callError = kaizen_variant_call_error.CreateInvalidArgumentCountError(expected: 2, provided: args.Count);
                    return;
                }
                ((global::EventSignals)kaizenObject).backing_MySignal?.Invoke(global::Kaizen.NativeInterop.VariantUtils.ConvertTo<string>(args[0]), global::Kaizen.NativeInterop.VariantUtils.ConvertTo<int>(args[1]));
            }
            var aux_delegate_2_MySignal = trampoline_2_MySignal;
            collector.TryAdd(new(SignalName.@MySignal, 2), new(aux_delegate_2_MySignal.Method.MethodHandle.GetFunctionPointer()));
        }
    }
#pragma warning restore CS0109
    private global::EventSignals.MySignalEventHandler backing_MySignal;
    /// <inheritdoc cref="global::EventSignals.MySignalEventHandler"/>
    public event global::EventSignals.MySignalEventHandler @MySignal {
        add => backing_MySignal += value;
        remove => backing_MySignal -= value;
    }
    protected void EmitSignalMySignal(string @str, int @num)
    {
        EmitSignal(SignalName.MySignal, [@str, @num]);
    }
}
