#if REAL_T_IS_DOUBLE
global using real_t = System.Double;
#else
global using real_t = System.Single;
#endif

global using unsafe MethodTrampolineDelegate = delegate* managed<
    /* kaizenObject: */ object,
    /* args: */ Kaizen.NativeInterop.NativeVariantPtrArgs,
    /* callError: */ ref Kaizen.NativeInterop.kaizen_variant_call_error,
    /* return (value): */ Kaizen.NativeInterop.kaizen_variant>;
global using unsafe PropertyGetterTrampolineDelegate = delegate* managed<
    /* kaizenObject: */ object,
    /* return (value): */ Kaizen.NativeInterop.kaizen_variant>;
global using unsafe PropertySetterTrampolineDelegate = delegate* managed<
    /* kaizenObject: */ object,
    /* value: */ in Kaizen.NativeInterop.kaizen_variant,
    /* return: */ void>;
global using unsafe RaiseSignalTrampolineDelegate = delegate* managed<
    /* kaizenObject: */ object,
    /* args: */ Kaizen.NativeInterop.NativeVariantPtrArgs,
    /* callError: */ ref Kaizen.NativeInterop.kaizen_variant_call_error,
    /* return */ void>;

global using unsafe TryAddMethodTrampolineDelegate = delegate* unmanaged<
    /* scriptPtr: */ System.IntPtr,
    /* name: */ Kaizen.NativeInterop.kaizen_string_name*,
    /* argCount: */ int,
    /* trampolineDelegate: */ void*,
    /* isStatic: */ Kaizen.NativeInterop.kaizen_bool,
    /* return */ void>;
global using unsafe TryAddPropertyTrampolineDelegate = delegate* unmanaged<
    /* scriptPtr: */ System.IntPtr,
    /* name: */ Kaizen.NativeInterop.kaizen_string_name*,
    /* getterTrampolineDelegate: */ void*,
    /* setterTrampolineDelegate: */ void*,
    /* return */ void>;
global using unsafe TryAddRaiseSignalTrampolineDelegate = delegate* unmanaged<
    /* scriptPtr: */ System.IntPtr,
    /* name: */ Kaizen.NativeInterop.kaizen_string_name*,
    /* argCount: */ int,
    /* trampolineDelegate: */ void*,
    /* return */ void>;
