using System;
using System.Runtime.InteropServices;
using Kaizen.NativeInterop;

namespace Kaizen.Bridge
{
    internal static class CSharpInstanceBridge
    {
        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool Call(IntPtr kaizenObjectGCHandle, kaizen_string_name* method,
            kaizen_variant** args, int argCount, kaizen_variant_call_error* refCallError, kaizen_variant* outRet)
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                {
                    *outRet = default;
                    (*refCallError).Error = kaizen_variant_call_error_error.KAIZEN_CALL_ERROR_CALL_ERROR_INSTANCE_IS_NULL;
                    return kaizen_bool.False;
                }

                bool methodInvoked = kaizenObject.InvokeKaizenClassMethod(CustomUnsafe.AsRef(method),
                    new NativeVariantPtrArgs(args, argCount), out kaizen_variant retValue);

                if (!methodInvoked)
                {
                    *outRet = default;
                    // This is important, as it tells Object::call that no method was called.
                    // Otherwise, it would prevent Object::call from calling native methods.
                    (*refCallError).Error = kaizen_variant_call_error_error.KAIZEN_CALL_ERROR_CALL_ERROR_INVALID_METHOD;
                    return kaizen_bool.False;
                }

                *outRet = retValue;
                return kaizen_bool.True;
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                *outRet = default;
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool Set(IntPtr kaizenObjectGCHandle, kaizen_string_name* name, kaizen_variant* value)
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    throw new InvalidOperationException("Kaizen Object instance is null.");

                if (kaizenObject.SetKaizenClassPropertyValue(CustomUnsafe.AsRef(name), CustomUnsafe.AsRef(value)))
                {
                    return kaizen_bool.True;
                }

                if (!kaizenObject.HasKaizenClassMethod(KaizenObject.MethodName._Set.NativeValue.DangerousSelfRef))
                {
                    return kaizen_bool.False;
                }

                var nameManaged = StringName.CreateTakingOwnershipOfDisposableValue(
                    NativeFuncs.kaizensharp_string_name_new_copy(CustomUnsafe.AsRef(name)));

                Variant valueManaged = Variant.CreateCopyingBorrowed(*value);

                return kaizenObject._Set(nameManaged, valueManaged).ToKaizenBool();
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool Get(IntPtr kaizenObjectGCHandle, kaizen_string_name* name,
            kaizen_variant* outRet)
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    throw new InvalidOperationException("Kaizen Object instance is null.");

                // Properties
                if (kaizenObject.GetKaizenClassPropertyValue(CustomUnsafe.AsRef(name), out kaizen_variant outRetValue))
                {
                    *outRet = outRetValue;
                    return kaizen_bool.True;
                }

                // Signals
                if (kaizenObject.HasKaizenClassSignal(CustomUnsafe.AsRef(name)))
                {
                    kaizen_signal signal = new kaizen_signal(NativeFuncs.kaizensharp_string_name_new_copy(*name),
                        kaizenObject.GetInstanceId());
                    *outRet = VariantUtils.CreateFromSignalTakingOwnershipOfDisposableValue(signal);
                    return kaizen_bool.True;
                }

                // Methods
                if (kaizenObject.HasKaizenClassMethod(CustomUnsafe.AsRef(name)))
                {
                    kaizen_callable method = new kaizen_callable(NativeFuncs.kaizensharp_string_name_new_copy(*name),
                        kaizenObject.GetInstanceId());
                    *outRet = VariantUtils.CreateFromCallableTakingOwnershipOfDisposableValue(method);
                    return kaizen_bool.True;
                }

                if (!kaizenObject.HasKaizenClassMethod(KaizenObject.MethodName._Get.NativeValue.DangerousSelfRef))
                {
                    return kaizen_bool.False;
                }

                var nameManaged = StringName.CreateTakingOwnershipOfDisposableValue(
                    NativeFuncs.kaizensharp_string_name_new_copy(CustomUnsafe.AsRef(name)));

                Variant ret = kaizenObject._Get(nameManaged);

                if (ret.VariantType == Variant.Type.Nil)
                {
                    *outRet = default;
                    return kaizen_bool.False;
                }

                *outRet = ret.CopyNativeVariant();
                return kaizen_bool.True;
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                *outRet = default;
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool CallViaTrampoline(
            MethodTrampolineDelegate methodTrampoline,
            IntPtr kaizenObjectGCHandle, kaizen_variant** args, int argCount,
            kaizen_variant_call_error* refCallError, kaizen_variant* outRet)
        {
            try
            {
                object kaizenObject = GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                {
                    *outRet = default;
                    (*refCallError).Error = kaizen_variant_call_error_error.KAIZEN_CALL_ERROR_CALL_ERROR_INSTANCE_IS_NULL;
                    return kaizen_bool.False;
                }

                *outRet = methodTrampoline(kaizenObject, new NativeVariantPtrArgs(args, argCount), ref *refCallError);
                return kaizen_bool.True;
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                *outRet = default;
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool SetViaTrampoline(
            PropertySetterTrampolineDelegate setterTrampoline,
            IntPtr kaizenObjectGCHandle, kaizen_variant* value)
        {
            try
            {
                object kaizenObject = GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    throw new InvalidOperationException("Kaizen Object instance is null.");

                setterTrampoline(kaizenObject, *value);
                return kaizen_bool.True;
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool GetViaTrampoline(
            PropertyGetterTrampolineDelegate getterTrampoline,
            IntPtr kaizenObjectGCHandle, kaizen_variant* outRet)
        {
            try
            {
                object kaizenObject = GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    throw new InvalidOperationException("Kaizen Object instance is null.");

                *outRet = getterTrampoline(kaizenObject);
                return kaizen_bool.True;
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                *outRet = default;
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static void CallDispose(IntPtr kaizenObjectGCHandle, kaizen_bool okIfNull)
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (okIfNull.ToBool())
                    kaizenObject?.Dispose();
                else
                    kaizenObject!.Dispose();
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe void CallToString(IntPtr kaizenObjectGCHandle, kaizen_string* outRes, kaizen_bool* outValid)
        {
            try
            {
                var self = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (self == null)
                {
                    *outRes = default;
                    *outValid = kaizen_bool.False;
                    return;
                }

                var resultStr = self.ToString();

                if (resultStr == null)
                {
                    *outRes = default;
                    *outValid = kaizen_bool.False;
                    return;
                }

                *outRes = Marshaling.ConvertStringToNative(resultStr);
                *outValid = kaizen_bool.True;
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                *outRes = default;
                *outValid = kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe kaizen_bool HasMethodUnknownParams(IntPtr kaizenObjectGCHandle, kaizen_string_name* method)
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    return kaizen_bool.False;

                return kaizenObject.HasKaizenClassMethod(CustomUnsafe.AsRef(method)).ToKaizenBool();
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
                return kaizen_bool.False;
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe void SerializeState(
            IntPtr kaizenObjectGCHandle,
            kaizen_dictionary* propertiesState,
            kaizen_dictionary* signalEventsState
        )
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    return;

                // Call OnBeforeSerialize

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (kaizenObject is ISerializationListener serializationListener)
                    serializationListener.OnBeforeSerialize();

                // Save instance state

                using var info = KaizenSerializationInfo.CreateCopyingBorrowed(
                    *propertiesState, *signalEventsState);

                kaizenObject.SaveKaizenObjectData(info);
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
            }
        }

        [UnmanagedCallersOnly]
        internal static unsafe void DeserializeState(
            IntPtr kaizenObjectGCHandle,
            kaizen_dictionary* propertiesState,
            kaizen_dictionary* signalEventsState
        )
        {
            try
            {
                var kaizenObject = (KaizenObject)GCHandle.FromIntPtr(kaizenObjectGCHandle).Target;

                if (kaizenObject == null)
                    return;

                // Restore instance state

                using var info = KaizenSerializationInfo.CreateCopyingBorrowed(
                    *propertiesState, *signalEventsState);

                kaizenObject.RestoreKaizenObjectData(info);

                // Call OnAfterDeserialize

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (kaizenObject is ISerializationListener serializationListener)
                    serializationListener.OnAfterDeserialize();
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
            }
        }
    }
}
