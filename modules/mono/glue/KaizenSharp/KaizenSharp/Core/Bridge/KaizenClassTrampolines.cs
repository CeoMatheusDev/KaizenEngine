using System;

namespace Kaizen.Bridge;

#nullable enable

/// <summary>
/// Struct representing a method trampoline, containing a delegate to the trampoline function.
/// The trampoline is a <c>delegate* managed</c> with a signature equivalent to the following C# delegate:
/// <code>
/// delegate kaizen_variant MethodTrampoline(object kaizenObject, NativeVariantPtrArgs args, ref kaizen_variant_call_error callError);
/// </code>
/// </summary>
public readonly struct MethodTrampoline
{
    public unsafe MethodTrampoline(MethodTrampolineDelegate trampolineDelegate, bool isStatic)
    {
        TrampolineDelegate = trampolineDelegate;
        IsStatic = isStatic;
    }

    public unsafe MethodTrampoline(IntPtr trampolineDelegate, bool isStatic)
    {
        TrampolineDelegate = (MethodTrampolineDelegate)trampolineDelegate;
        IsStatic = isStatic;
    }

    public unsafe MethodTrampolineDelegate TrampolineDelegate { get; }

    public bool IsStatic { get; }
}

/// <summary>
/// Struct representing a property getter trampoline, containing a delegate to the trampoline function.
/// The trampoline is a <c>delegate* managed</c> with a signature equivalent to the following C# delegate:
/// <code>
/// delegate kaizen_variant PropertyGetterTrampoline(object kaizenObject);
/// </code>
/// </summary>
public readonly struct PropertyGetterTrampoline
{
    public unsafe PropertyGetterTrampoline(PropertyGetterTrampolineDelegate trampolineDelegate)
        => TrampolineDelegate = trampolineDelegate;

    public unsafe PropertyGetterTrampoline(IntPtr trampolineDelegate)
        => TrampolineDelegate = (PropertyGetterTrampolineDelegate)trampolineDelegate;

    public unsafe PropertyGetterTrampolineDelegate TrampolineDelegate { get; }
}

/// <summary>
/// Struct representing a property setter trampoline, containing a delegate to the trampoline function.
/// The trampoline is a <c>delegate* managed</c> with a signature equivalent to the following C# delegate:
/// <code>
/// delegate void PropertySetterTrampoline(object kaizenObject, in kaizen_variant value);
/// </code>
/// </summary>
public readonly struct PropertySetterTrampoline
{
    public unsafe PropertySetterTrampoline(PropertySetterTrampolineDelegate trampolineDelegate)
        => TrampolineDelegate = trampolineDelegate;

    public unsafe PropertySetterTrampoline(IntPtr trampolineDelegate)
        => TrampolineDelegate = (PropertySetterTrampolineDelegate)trampolineDelegate;

    public unsafe PropertySetterTrampolineDelegate TrampolineDelegate { get; }
}

/// <summary>
/// Struct representing a raise signal trampoline, containing a delegate to the trampoline function.
/// The trampoline is a <c>delegate* managed</c> with a signature equivalent to the following C# delegate:
/// <code>
/// delegate void RaiseSignalTrampoline(object kaizenObject, NativeVariantPtrArgs args);
/// </code>
/// </summary>
public readonly struct RaiseSignalTrampoline
{
    public unsafe RaiseSignalTrampoline(RaiseSignalTrampolineDelegate trampolineDelegate)
        => TrampolineDelegate = trampolineDelegate;

    public unsafe RaiseSignalTrampoline(IntPtr trampolineDelegate)
        => TrampolineDelegate = (RaiseSignalTrampolineDelegate)trampolineDelegate;

    public unsafe RaiseSignalTrampolineDelegate TrampolineDelegate { get; }
}
