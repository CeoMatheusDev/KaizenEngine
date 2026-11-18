using System;
using System.Runtime.InteropServices;
using Kaizen.NativeInterop;

namespace Kaizen
{
    public class SignalAwaiter : IAwaiter<Variant[]>, IAwaitable<Variant[]>
    {
        private bool _completed;
        private Variant[] _result;
        private Action _continuation;

        public SignalAwaiter(KaizenObject source, StringName signal, KaizenObject target)
        {
            var awaiterGcHandle = CustomGCHandle.AllocStrong(this);
            using kaizen_string_name signalSrc = NativeFuncs.kaizensharp_string_name_new_copy(
                (kaizen_string_name)(signal?.NativeValue ?? default));
            NativeFuncs.kaizensharp_internal_signal_awaiter_connect(KaizenObject.GetPtr(source), in signalSrc,
                KaizenObject.GetPtr(target), GCHandle.ToIntPtr(awaiterGcHandle));
        }

        public bool IsCompleted => _completed;

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }

        public Variant[] GetResult() => _result;

        public IAwaiter<Variant[]> GetAwaiter() => this;

        [UnmanagedCallersOnly]
        internal static unsafe void SignalCallback(IntPtr awaiterGCHandlePtr, kaizen_variant** args, int argCount,
            kaizen_bool* outAwaiterIsNull)
        {
            try
            {
                var awaiter = (SignalAwaiter)GCHandle.FromIntPtr(awaiterGCHandlePtr).Target;

                if (awaiter == null)
                {
                    *outAwaiterIsNull = kaizen_bool.True;
                    return;
                }

                *outAwaiterIsNull = kaizen_bool.False;

                awaiter._completed = true;

                if (argCount > 0)
                {
                    Variant[] signalArgs = new Variant[argCount];

                    for (int i = 0; i < argCount; i++)
                        signalArgs[i] = Variant.CreateCopyingBorrowed(*args[i]);

                    awaiter._result = signalArgs;
                }
                else
                {
                    awaiter._result = [];
                }

                awaiter._continuation?.Invoke();
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                *outAwaiterIsNull = kaizen_bool.False;
            }
        }
    }
}
