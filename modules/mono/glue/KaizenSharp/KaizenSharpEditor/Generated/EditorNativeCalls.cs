namespace Kaizen;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Kaizen.NativeInterop;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantUnsafeContext")]
[SuppressMessage("ReSharper", "RedundantNameQualifier")]
[System.Runtime.CompilerServices.SkipLocalsInit]
internal static class EditorNativeCalls
{
    internal static ulong kaizen_api_hash = 1448119531;

    private const int VarArgsSpanThreshold = 10;


    internal static unsafe void kaizen_icall_4_478(IntPtr method, IntPtr ptr, string arg1, string arg2, in Callable arg3, string arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_callable arg3_in = Marshaling.ConvertCallableToNative(in arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2_in, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_2_479(IntPtr method, IntPtr ptr, IntPtr arg1, in Callable arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg2_in = Marshaling.ConvertCallableToNative(in arg2);
        void** call_args = stackalloc void*[2] { &arg1, &arg2_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_480(IntPtr method, IntPtr ptr, string arg1, in Callable arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_callable arg2_in = Marshaling.ConvertCallableToNative(in arg2);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_481(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_483(IntPtr method, IntPtr ptr, string arg1, kaizen_bool arg2, kaizen_array arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_1_484(IntPtr method, IntPtr ptr, string arg1)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[1] { &arg1_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_4_485(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_bool arg2, string arg3, kaizen_bool arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        void** call_args = stackalloc void*[4] { &arg1, &arg2, &arg3_in, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_3_486(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_bool arg2, string arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        void** call_args = stackalloc void*[3] { &arg1, &arg2, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe int kaizen_icall_4_488(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_bool arg2, in Callable arg3, in Callable arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_callable arg3_in = Marshaling.ConvertCallableToNative(in arg3);
        using kaizen_callable arg4_in = Marshaling.ConvertCallableToNative(in arg4);
        void** call_args = stackalloc void*[4] { &arg1, &arg2, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe int kaizen_icall_5_489(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_bool arg2, string arg3, int arg4, kaizen_bool arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        long arg4_in = arg4;
        void** call_args = stackalloc void*[5] { &arg1, &arg2, &arg3_in, &arg4_in, &arg5 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe int kaizen_icall_4_490(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_bool arg2, string arg3, int arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        long arg4_in = arg4;
        void** call_args = stackalloc void*[4] { &arg1, &arg2, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe int kaizen_icall_5_491(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_bool arg2, string arg3, ReadOnlySpan<string> arg4, int arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        using kaizen_packed_string_array arg4_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg4);
        long arg5_in = arg5;
        void** call_args = stackalloc void*[5] { &arg1, &arg2, &arg3_in, &arg4_in, &arg5_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe void kaizen_icall_3_492(IntPtr method, IntPtr ptr, int arg1, string arg2, string arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long arg1_in = arg1;
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe int kaizen_icall_6_493(IntPtr method, IntPtr ptr, string arg1, string arg2, ReadOnlySpan<string> arg3, string arg4, kaizen_array arg5, int arg6)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_packed_string_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        long arg6_in = arg6;
        void** call_args = stackalloc void*[6] { &arg1_in, &arg2_in, &arg3_in, &arg4_in, &arg5, &arg6_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe long kaizen_icall_5_494(IntPtr method, IntPtr ptr, string arg1, string arg2, ReadOnlySpan<string> arg3, string arg4, int arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_packed_string_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        long arg5_in = arg5;
        void** call_args = stackalloc void*[5] { &arg1_in, &arg2_in, &arg3_in, &arg4_in, &arg5_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return ret;
    }

    internal static unsafe int kaizen_icall_5_495(IntPtr method, IntPtr ptr, string arg1, string arg2, ReadOnlySpan<string> arg3, string arg4, string arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_packed_string_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        using kaizen_string arg5_in = Marshaling.ConvertStringToNative(arg5);
        void** call_args = stackalloc void*[5] { &arg1_in, &arg2_in, &arg3_in, &arg4_in, &arg5_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe string[] kaizen_icall_1_497(IntPtr method, IntPtr arg1)
    {
        using kaizen_packed_string_array ret = default;
        void** call_args = stackalloc void*[1] { &arg1 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, IntPtr.Zero, call_args, &ret);
        return Marshaling.ConvertNativePackedStringArrayToSystemArray(ret);
    }

    internal static unsafe void kaizen_icall_3_499(IntPtr method, IntPtr ptr, string arg1, ReadOnlySpan<string> arg2, string arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_packed_string_array arg2_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg2);
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_500(IntPtr method, IntPtr ptr, string arg1, ReadOnlySpan<byte> arg2, kaizen_bool arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_packed_byte_array arg2_in = Marshaling.ConvertSystemArrayToNativePackedByteArray(arg2);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_501(IntPtr method, IntPtr ptr, string arg1, string arg2, ReadOnlySpan<string> arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_packed_string_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg3);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe Variant kaizen_icall_2_502(IntPtr method, IntPtr ptr, kaizen_string_name arg1, string arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_variant ret = default;
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[2] { &arg1, &arg2_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Variant.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe string kaizen_icall_2_503(IntPtr method, IntPtr ptr, kaizen_string_name arg1, kaizen_bool arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string ret = default;
        void** call_args = stackalloc void*[2] { &arg1, &arg2 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Marshaling.ConvertStringToManaged(ret);
    }

    internal static unsafe void kaizen_icall_3_504(IntPtr method, IntPtr ptr, kaizen_string_name arg1, kaizen_string_name arg2, kaizen_bool arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        void** call_args = stackalloc void*[3] { &arg1, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_2_505(IntPtr method, IntPtr ptr, IntPtr arg1, string arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[2] { &arg1, &arg2_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe int kaizen_icall_4_506(IntPtr method, IntPtr ptr, string arg1, kaizen_dictionary arg2, string arg3, Variant arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long ret;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        kaizen_variant arg4_in = (kaizen_variant)arg4.NativeVar;
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return (int)(ret);
    }

    internal static unsafe KaizenObject kaizen_icall_7_507(IntPtr method, IntPtr arg1, int arg2, string arg3, int arg4, string arg5, uint arg6, kaizen_bool arg7)
    {
        IntPtr ret = default;
        long arg2_in = arg2;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        long arg4_in = arg4;
        using kaizen_string arg5_in = Marshaling.ConvertStringToNative(arg5);
        long arg6_in = arg6;
        void** call_args = stackalloc void*[7] { &arg1, &arg2_in, &arg3_in, &arg4_in, &arg5_in, &arg6_in, &arg7 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, IntPtr.Zero, call_args, &ret);
        return InteropUtils.UnmanagedGetManaged(ret);
    }

    internal static unsafe KaizenObject kaizen_icall_1_508(IntPtr method, IntPtr arg1)
    {
        IntPtr ret = default;
        void** call_args = stackalloc void*[1] { &arg1 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, IntPtr.Zero, call_args, &ret);
        return InteropUtils.UnmanagedGetManaged(ret);
    }

    internal static unsafe void kaizen_icall_1_509(IntPtr method, Variant arg1)
    {
        kaizen_variant arg1_in = (kaizen_variant)arg1.NativeVar;
        void** call_args = stackalloc void*[1] { &arg1_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, IntPtr.Zero, call_args, null);
    }

    internal static unsafe Variant kaizen_icall_0_510(IntPtr method)
    {
        kaizen_variant ret = default;
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, IntPtr.Zero, null, &ret);
        return Variant.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe void kaizen_icall_4_511(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2, kaizen_bool arg3, string arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2, &arg3, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_512(IntPtr method, IntPtr ptr, string arg1, ReadOnlySpan<string> arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_packed_string_array arg2_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg2);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_513(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2, kaizen_bool arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe Kaizen.Collections.Array kaizen_icall_2_514(IntPtr method, IntPtr ptr, kaizen_array arg1, int arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_array ret = default;
        long arg2_in = arg2;
        void** call_args = stackalloc void*[2] { &arg1, &arg2_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Array.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe void kaizen_icall_3_519(IntPtr method, IntPtr ptr, in Callable arg1, kaizen_array arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg1_in = Marshaling.ConvertCallableToNative(in arg1);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_520(IntPtr method, IntPtr ptr, IntPtr arg1, in Callable arg2, ReadOnlySpan<int> arg3, string arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg2_in = Marshaling.ConvertCallableToNative(in arg2);
        using kaizen_packed_int32_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedInt32Array(arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        void** call_args = stackalloc void*[4] { &arg1, &arg2_in, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_521(IntPtr method, IntPtr ptr, IntPtr arg1, in Callable arg2, string arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg2_in = Marshaling.ConvertCallableToNative(in arg2);
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_2_522(IntPtr method, IntPtr ptr, in Callable arg1, kaizen_array arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg1_in = Marshaling.ConvertCallableToNative(in arg1);
        void** call_args = stackalloc void*[2] { &arg1_in, &arg2 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_5_523(IntPtr method, IntPtr ptr, in Callable arg1, kaizen_string_name arg2, string arg3, string arg4, kaizen_array arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg1_in = Marshaling.ConvertCallableToNative(in arg1);
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        void** call_args = stackalloc void*[5] { &arg1_in, &arg2, &arg3_in, &arg4_in, &arg5 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_524(IntPtr method, IntPtr ptr, IntPtr arg1, string arg2, kaizen_bool arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_525(IntPtr method, IntPtr ptr, IntPtr arg1, int arg2, int arg3, kaizen_bool arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long arg2_in = arg2;
        long arg3_in = arg3;
        void** call_args = stackalloc void*[4] { &arg1, &arg2_in, &arg3_in, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_527(IntPtr method, IntPtr ptr, IntPtr arg1, in Callable arg2, ReadOnlySpan<int> arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_callable arg2_in = Marshaling.ConvertCallableToNative(in arg2);
        using kaizen_packed_int32_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedInt32Array(arg3);
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_528(IntPtr method, IntPtr ptr, ReadOnlySpan<Vector3> arg1, IntPtr arg2, kaizen_bool arg3, Color* arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_packed_vector3_array arg1_in = Marshaling.ConvertSystemArrayToNativePackedVector3Array(arg1);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2, &arg3, arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_529(IntPtr method, IntPtr ptr, IntPtr arg1, IntPtr arg2, Transform3D* arg3, IntPtr arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        void** call_args = stackalloc void*[4] { &arg1, &arg2, arg3, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_530(IntPtr method, IntPtr ptr, IntPtr arg1, float arg2, Color* arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        double arg2_in = arg2;
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_5_531(IntPtr method, IntPtr ptr, ReadOnlySpan<Vector3> arg1, IntPtr arg2, ReadOnlySpan<int> arg3, kaizen_bool arg4, kaizen_bool arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_packed_vector3_array arg1_in = Marshaling.ConvertSystemArrayToNativePackedVector3Array(arg1);
        using kaizen_packed_int32_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedInt32Array(arg3);
        void** call_args = stackalloc void*[5] { &arg1_in, &arg2, &arg3_in, &arg4, &arg5 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_5_532(IntPtr method, IntPtr ptr, string arg1, Color* arg2, kaizen_bool arg3, kaizen_bool arg4, kaizen_bool arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[5] { &arg1_in, arg2, &arg3, &arg4, &arg5 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_533(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2, kaizen_bool arg3, Color* arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2, &arg3, arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_534(IntPtr method, IntPtr ptr, string arg1, kaizen_bool arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe KaizenObject kaizen_icall_2_536(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_ref ret = default;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[2] { &arg1_in, &arg2 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return InteropUtils.UnmanagedGetManaged(ret.Reference);
    }

    internal static unsafe void kaizen_icall_4_538(IntPtr method, IntPtr ptr, string arg1, string arg2, IntPtr arg3, IntPtr arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2_in, &arg3, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_539(IntPtr method, IntPtr ptr, int arg1, IntPtr arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long arg1_in = arg1;
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe KaizenObject kaizen_icall_3_540(IntPtr method, IntPtr ptr, IntPtr arg1, string arg2, IntPtr arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        IntPtr ret = default;
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return InteropUtils.UnmanagedGetManaged(ret);
    }

    internal static unsafe KaizenObject kaizen_icall_2_541(IntPtr method, IntPtr ptr, IntPtr arg1, string arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        IntPtr ret = default;
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[2] { &arg1, &arg2_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return InteropUtils.UnmanagedGetManaged(ret);
    }

    internal static unsafe void kaizen_icall_4_543(IntPtr method, IntPtr ptr, kaizen_string_name arg1, Variant arg2, kaizen_string_name arg3, kaizen_bool arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_variant arg2_in = (kaizen_variant)arg2.NativeVar;
        void** call_args = stackalloc void*[4] { &arg1, &arg2_in, &arg3, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_544(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2, kaizen_string_name arg3, Variant arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        kaizen_variant arg4_in = (kaizen_variant)arg4.NativeVar;
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2, &arg3, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_545(IntPtr method, IntPtr ptr, IntPtr arg1, IntPtr arg2, kaizen_string_name arg3, Variant arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_variant arg4_in = (kaizen_variant)arg4.NativeVar;
        void** call_args = stackalloc void*[4] { &arg1, &arg2, &arg3, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_6_547(IntPtr method, IntPtr ptr, int arg1, string arg2, Variant arg3, int arg4, string arg5, int arg6)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        long arg1_in = arg1;
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        kaizen_variant arg3_in = (kaizen_variant)arg3.NativeVar;
        long arg4_in = arg4;
        using kaizen_string arg5_in = Marshaling.ConvertStringToNative(arg5);
        long arg6_in = arg6;
        void** call_args = stackalloc void*[6] { &arg1_in, &arg2_in, &arg3_in, &arg4_in, &arg5_in, &arg6_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_549(IntPtr method, IntPtr ptr, kaizen_string_name arg1, Variant arg2, kaizen_bool arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_variant arg2_in = (kaizen_variant)arg2.NativeVar;
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, &arg3 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe kaizen_bool kaizen_icall_2_550(IntPtr method, IntPtr ptr, string arg1, IntPtr arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_bool ret;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        void** call_args = stackalloc void*[2] { &arg1_in, &arg2 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return ret;
    }

    internal static unsafe void kaizen_icall_5_552(IntPtr method, IntPtr ptr, string arg1, int arg2, IntPtr arg3, kaizen_bool arg4, kaizen_bool arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        long arg2_in = arg2;
        void** call_args = stackalloc void*[5] { &arg1_in, &arg2_in, &arg3, &arg4, &arg5 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_3_553(IntPtr method, IntPtr ptr, IntPtr arg1, kaizen_string_name arg2, ReadOnlySpan<Variant> arg3, kaizen_string_name caller)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        int vararg_length = arg3.Length;
        int total_length = 2 + vararg_length;
        Span<kaizen_variant.movable> varargs_span = vararg_length <= VarArgsSpanThreshold ?
            stackalloc kaizen_variant.movable[VarArgsSpanThreshold] :
            new kaizen_variant.movable[vararg_length];
        Span<IntPtr> call_args_span = total_length <= VarArgsSpanThreshold ?
            stackalloc IntPtr[VarArgsSpanThreshold] :
            new IntPtr[total_length];
        fixed (kaizen_variant.movable* varargs = &MemoryMarshal.GetReference(varargs_span))
        fixed (IntPtr* call_args = &MemoryMarshal.GetReference(call_args_span))
        {
            using kaizen_variant arg1_in = VariantUtils.CreateFromKaizenObjectPtr(arg1);
            call_args[0] = new IntPtr(&arg1_in);
            using kaizen_variant arg2_in = VariantUtils.CreateFromStringName(arg2);
            call_args[1] = new IntPtr(&arg2_in);
            for (int i = 0; i < vararg_length; i++)
            {
                varargs[i] = arg3[i].NativeVar;
                call_args[2 + i] = new IntPtr(&varargs[i]);
            }
            NativeFuncs.kaizensharp_method_bind_call(method, ptr, (kaizen_variant**)call_args, total_length, out kaizen_variant_call_error vcall_error);
            ExceptionUtils.DebugCheckCallError(caller, ptr, (kaizen_variant**)call_args, total_length, vcall_error);
        }
    }

    internal static unsafe void kaizen_icall_4_555(IntPtr method, IntPtr ptr, string arg1, int arg2, IntPtr arg3, kaizen_bool arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        long arg2_in = arg2;
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2_in, &arg3, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_4_556(IntPtr method, IntPtr ptr, int arg1, int arg2, string arg3, string arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        long arg1_in = arg1;
        long arg2_in = arg2;
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        using kaizen_string arg4_in = Marshaling.ConvertStringToNative(arg4);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2_in, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_4_557(IntPtr method, IntPtr ptr, int arg1, int arg2, int arg3, int arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        long arg1_in = arg1;
        long arg2_in = arg2;
        long arg3_in = arg3;
        long arg4_in = arg4;
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2_in, &arg3_in, &arg4_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_2_558(IntPtr method, IntPtr ptr, string arg1, string arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[2] { &arg1_in, &arg2_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_5_559(IntPtr method, IntPtr ptr, string arg1, string arg2, string arg3, long arg4, long arg5)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_string arg3_in = Marshaling.ConvertStringToNative(arg3);
        void** call_args = stackalloc void*[5] { &arg1_in, &arg2_in, &arg3_in, &arg4, &arg5 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_3_560(IntPtr method, IntPtr ptr, string arg1, int arg2, int arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        long arg2_in = arg2;
        long arg3_in = arg3;
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_2_561(IntPtr method, IntPtr ptr, kaizen_dictionary arg1, kaizen_array arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        void** call_args = stackalloc void*[2] { &arg1, &arg2 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Variant kaizen_icall_1_657(IntPtr method, IntPtr ptr, kaizen_dictionary arg1)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_variant ret = default;
        void** call_args = stackalloc void*[1] { &arg1 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Variant.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe void kaizen_icall_3_659(IntPtr method, IntPtr ptr, string arg1, Variant arg2, int arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        kaizen_variant arg2_in = (kaizen_variant)arg2.NativeVar;
        long arg3_in = arg3;
        void** call_args = stackalloc void*[3] { &arg1_in, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe Kaizen.Collections.Array kaizen_icall_1_660(IntPtr method, IntPtr ptr, kaizen_dictionary arg1)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_array ret = default;
        void** call_args = stackalloc void*[1] { &arg1 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Array.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe Kaizen.Collections.Dictionary kaizen_icall_1_661(IntPtr method, IntPtr ptr, kaizen_dictionary arg1)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        kaizen_dictionary ret = default;
        void** call_args = stackalloc void*[1] { &arg1 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, &ret);
        return Kaizen.Collections.Dictionary.CreateTakingOwnershipOfDisposableValue(ret);
    }

    internal static unsafe void kaizen_icall_3_662(IntPtr method, IntPtr ptr, IntPtr arg1, string arg2, ReadOnlySpan<string> arg3)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        using kaizen_packed_string_array arg3_in = Marshaling.ConvertSystemArrayToNativePackedStringArray(arg3);
        void** call_args = stackalloc void*[3] { &arg1, &arg2_in, &arg3_in };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_2_745(IntPtr method, IntPtr ptr, Vector3I* arg1, Vector3I* arg2)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        void** call_args = stackalloc void*[2] { arg1, arg2 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }

    internal static unsafe void kaizen_icall_4_1323(IntPtr method, IntPtr ptr, string arg1, string arg2, kaizen_bool arg3, kaizen_bool arg4)
    {
        ExceptionUtils.ThrowIfNullPtr(ptr);
        using kaizen_string arg1_in = Marshaling.ConvertStringToNative(arg1);
        using kaizen_string arg2_in = Marshaling.ConvertStringToNative(arg2);
        void** call_args = stackalloc void*[4] { &arg1_in, &arg2_in, &arg3, &arg4 };
        NativeFuncs.kaizensharp_method_bind_ptrcall(method, ptr, call_args, null);
    }
}
