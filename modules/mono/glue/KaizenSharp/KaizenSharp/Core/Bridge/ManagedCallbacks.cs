using System;
using System.Runtime.InteropServices;
using Kaizen.NativeInterop;

namespace Kaizen.Bridge
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ManagedCallbacks
    {
        // @formatter:off
        public delegate* unmanaged<IntPtr, kaizen_variant**, int, kaizen_bool*, void> SignalAwaiter_SignalCallback;
        public delegate* unmanaged<IntPtr, void*, kaizen_variant**, int, kaizen_variant*, void> DelegateUtils_InvokeWithVariantArgs;
        public delegate* unmanaged<IntPtr, IntPtr, kaizen_bool> DelegateUtils_DelegateEquals;
        public delegate* unmanaged<IntPtr, int> DelegateUtils_DelegateHash;
        public delegate* unmanaged<IntPtr, kaizen_bool*, int> DelegateUtils_GetArgumentCount;
        public delegate* unmanaged<IntPtr, kaizen_array*, kaizen_bool> DelegateUtils_TrySerializeDelegateWithGCHandle;
        public delegate* unmanaged<kaizen_array*, IntPtr*, kaizen_bool> DelegateUtils_TryDeserializeDelegateWithGCHandle;
        public delegate* unmanaged<void> ScriptManagerBridge_FrameCallback;
        public delegate* unmanaged<kaizen_string_name*, IntPtr, IntPtr> ScriptManagerBridge_CreateManagedForKaizenObjectBinding;
        public delegate* unmanaged<IntPtr, IntPtr, kaizen_variant**, int, kaizen_bool> ScriptManagerBridge_CreateManagedForKaizenObjectScriptInstance;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, void> ScriptManagerBridge_GetScriptNativeName;
        public delegate* unmanaged<kaizen_string*, kaizen_string*, kaizen_string*, kaizen_bool*, kaizen_bool*, kaizen_string*, void> ScriptManagerBridge_GetGlobalClassName;
        public delegate* unmanaged<IntPtr, IntPtr, void> ScriptManagerBridge_SetKaizenObjectPtr;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, kaizen_variant**, int, kaizen_bool*, void> ScriptManagerBridge_RaiseEventSignal;
        public delegate* unmanaged<RaiseSignalTrampolineDelegate, IntPtr, kaizen_variant**, int, kaizen_variant_call_error*, kaizen_bool*, void> ScriptManagerBridge_RaiseEventSignalViaTrampoline;
        public delegate* unmanaged<IntPtr, IntPtr, kaizen_bool> ScriptManagerBridge_ScriptIsOrInherits;
        public delegate* unmanaged<IntPtr, kaizen_string*, kaizen_bool> ScriptManagerBridge_AddScriptBridge;
        public delegate* unmanaged<kaizen_string*, kaizen_ref*, void> ScriptManagerBridge_GetOrCreateScriptBridgeForPath;
        public delegate* unmanaged<IntPtr, void> ScriptManagerBridge_RemoveScriptBridge;
        public delegate* unmanaged<IntPtr, kaizen_bool> ScriptManagerBridge_TryReloadRegisteredScriptWithClass;
        public delegate* unmanaged<IntPtr, kaizen_bool*,
            delegate* unmanaged<IntPtr, kaizen_string_name*, int, void*, kaizen_bool, void>,
            delegate* unmanaged<IntPtr, kaizen_string_name*, void*, void*, void>,
            delegate* unmanaged<IntPtr, kaizen_string_name*, int, void*, void>,
            void> ScriptManagerBridge_UpdateScriptTrampolines;
        public delegate* unmanaged<IntPtr, kaizen_csharp_type_info*, kaizen_array*, kaizen_dictionary*, kaizen_dictionary*, kaizen_ref*, void> ScriptManagerBridge_UpdateScriptClassInfo;
        public delegate* unmanaged<IntPtr, IntPtr*, kaizen_bool, kaizen_bool> ScriptManagerBridge_SwapGCHandleForType;
        public delegate* unmanaged<IntPtr, delegate* unmanaged<IntPtr, kaizen_string*, void*, int, void>, void> ScriptManagerBridge_GetPropertyInfoList;
        public delegate* unmanaged<IntPtr, delegate* unmanaged<IntPtr, void*, int, void>, void> ScriptManagerBridge_GetPropertyDefaultValues;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, kaizen_variant**, int, kaizen_variant_call_error*, kaizen_variant*, kaizen_bool> ScriptManagerBridge_CallStatic;
        public delegate* unmanaged<MethodTrampolineDelegate, kaizen_variant**, int, kaizen_variant_call_error*, kaizen_variant*, kaizen_bool> ScriptManagerBridge_CallStaticWithTrampoline;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, kaizen_variant**, int, kaizen_variant_call_error*, kaizen_variant*, kaizen_bool> CSharpInstanceBridge_Call;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, kaizen_variant*, kaizen_bool> CSharpInstanceBridge_Set;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, kaizen_variant*, kaizen_bool> CSharpInstanceBridge_Get;
        public delegate* unmanaged<MethodTrampolineDelegate, IntPtr, kaizen_variant**, int, kaizen_variant_call_error*, kaizen_variant*, kaizen_bool> CSharpInstanceBridge_CallViaTrampoline;
        public delegate* unmanaged<PropertySetterTrampolineDelegate, IntPtr, kaizen_variant*, kaizen_bool> CSharpInstanceBridge_SetViaTrampoline;
        public delegate* unmanaged<PropertyGetterTrampolineDelegate, IntPtr, kaizen_variant*, kaizen_bool> CSharpInstanceBridge_GetViaTrampoline;
        public delegate* unmanaged<IntPtr, kaizen_bool, void> CSharpInstanceBridge_CallDispose;
        public delegate* unmanaged<IntPtr, kaizen_string*, kaizen_bool*, void> CSharpInstanceBridge_CallToString;
        public delegate* unmanaged<IntPtr, kaizen_string_name*, kaizen_bool> CSharpInstanceBridge_HasMethodUnknownParams;
        public delegate* unmanaged<IntPtr, kaizen_dictionary*, kaizen_dictionary*, void> CSharpInstanceBridge_SerializeState;
        public delegate* unmanaged<IntPtr, kaizen_dictionary*, kaizen_dictionary*, void> CSharpInstanceBridge_DeserializeState;
        public delegate* unmanaged<IntPtr, void> GCHandleBridge_FreeGCHandle;
        public delegate* unmanaged<IntPtr, kaizen_bool> GCHandleBridge_GCHandleIsTargetCollectible;
        public delegate* unmanaged<void*, void> DebuggingUtils_GetCurrentStackInfo;
        public delegate* unmanaged<void> DisposablesTracker_OnKaizenShuttingDown;
        public delegate* unmanaged<kaizen_bool, void> GD_OnCoreApiAssemblyLoaded;
        // @formatter:on

        public static ManagedCallbacks Create()
        {
            return new()
            {
                // @formatter:off
                SignalAwaiter_SignalCallback = &SignalAwaiter.SignalCallback,
                DelegateUtils_InvokeWithVariantArgs = &DelegateUtils.InvokeWithVariantArgs,
                DelegateUtils_DelegateEquals = &DelegateUtils.DelegateEquals,
                DelegateUtils_DelegateHash = &DelegateUtils.DelegateHash,
                DelegateUtils_GetArgumentCount = &DelegateUtils.GetArgumentCount,
                DelegateUtils_TrySerializeDelegateWithGCHandle = &DelegateUtils.TrySerializeDelegateWithGCHandle,
                DelegateUtils_TryDeserializeDelegateWithGCHandle = &DelegateUtils.TryDeserializeDelegateWithGCHandle,
                ScriptManagerBridge_FrameCallback = &ScriptManagerBridge.FrameCallback,
                ScriptManagerBridge_CreateManagedForKaizenObjectBinding = &ScriptManagerBridge.CreateManagedForKaizenObjectBinding,
                ScriptManagerBridge_CreateManagedForKaizenObjectScriptInstance = &ScriptManagerBridge.CreateManagedForKaizenObjectScriptInstance,
                ScriptManagerBridge_GetScriptNativeName = &ScriptManagerBridge.GetScriptNativeName,
                ScriptManagerBridge_GetGlobalClassName = &ScriptManagerBridge.GetGlobalClassName,
                ScriptManagerBridge_SetKaizenObjectPtr = &ScriptManagerBridge.SetKaizenObjectPtr,
                ScriptManagerBridge_RaiseEventSignal = &ScriptManagerBridge.RaiseEventSignal,
                ScriptManagerBridge_RaiseEventSignalViaTrampoline = &ScriptManagerBridge.RaiseEventSignalViaTrampoline,
                ScriptManagerBridge_ScriptIsOrInherits = &ScriptManagerBridge.ScriptIsOrInherits,
                ScriptManagerBridge_AddScriptBridge = &ScriptManagerBridge.AddScriptBridge,
                ScriptManagerBridge_GetOrCreateScriptBridgeForPath = &ScriptManagerBridge.GetOrCreateScriptBridgeForPath,
                ScriptManagerBridge_RemoveScriptBridge = &ScriptManagerBridge.RemoveScriptBridge,
                ScriptManagerBridge_TryReloadRegisteredScriptWithClass = &ScriptManagerBridge.TryReloadRegisteredScriptWithClass,
                ScriptManagerBridge_UpdateScriptTrampolines = &ScriptManagerBridge.UpdateScriptTrampolines,
                ScriptManagerBridge_UpdateScriptClassInfo = &ScriptManagerBridge.UpdateScriptClassInfo,
                ScriptManagerBridge_SwapGCHandleForType = &ScriptManagerBridge.SwapGCHandleForType,
                ScriptManagerBridge_GetPropertyInfoList = &ScriptManagerBridge.GetPropertyInfoList,
                ScriptManagerBridge_GetPropertyDefaultValues = &ScriptManagerBridge.GetPropertyDefaultValues,
                ScriptManagerBridge_CallStatic = &ScriptManagerBridge.CallStatic,
                ScriptManagerBridge_CallStaticWithTrampoline = &ScriptManagerBridge.CallStaticWithTrampoline,
                CSharpInstanceBridge_Call = &CSharpInstanceBridge.Call,
                CSharpInstanceBridge_Set = &CSharpInstanceBridge.Set,
                CSharpInstanceBridge_Get = &CSharpInstanceBridge.Get,
                CSharpInstanceBridge_CallViaTrampoline = &CSharpInstanceBridge.CallViaTrampoline,
                CSharpInstanceBridge_SetViaTrampoline = &CSharpInstanceBridge.SetViaTrampoline,
                CSharpInstanceBridge_GetViaTrampoline = &CSharpInstanceBridge.GetViaTrampoline,
                CSharpInstanceBridge_CallDispose = &CSharpInstanceBridge.CallDispose,
                CSharpInstanceBridge_CallToString = &CSharpInstanceBridge.CallToString,
                CSharpInstanceBridge_HasMethodUnknownParams = &CSharpInstanceBridge.HasMethodUnknownParams,
                CSharpInstanceBridge_SerializeState = &CSharpInstanceBridge.SerializeState,
                CSharpInstanceBridge_DeserializeState = &CSharpInstanceBridge.DeserializeState,
                GCHandleBridge_FreeGCHandle = &GCHandleBridge.FreeGCHandle,
                GCHandleBridge_GCHandleIsTargetCollectible = &GCHandleBridge.GCHandleIsTargetCollectible,
                DebuggingUtils_GetCurrentStackInfo = &DebuggingUtils.GetCurrentStackInfo,
                DisposablesTracker_OnKaizenShuttingDown = &DisposablesTracker.OnKaizenShuttingDown,
                GD_OnCoreApiAssemblyLoaded = &GD.OnCoreApiAssemblyLoaded,
                // @formatter:on
            };
        }

        public static void Create(IntPtr outManagedCallbacks)
            => *(ManagedCallbacks*)outManagedCallbacks = Create();
    }
}
