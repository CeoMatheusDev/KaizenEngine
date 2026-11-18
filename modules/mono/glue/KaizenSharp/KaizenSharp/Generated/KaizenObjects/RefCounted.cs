namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Base class for any object that keeps a reference count. <see cref="Kaizen.Resource"/> and many other helper objects inherit this class.</para>
/// <para>Unlike other <see cref="Kaizen.KaizenObject"/> types, <see cref="Kaizen.RefCounted"/>s keep an internal reference counter so that they are automatically released when no longer in use, and only then. <see cref="Kaizen.RefCounted"/>s therefore do not need to be freed manually with <see cref="Kaizen.KaizenObject.Free()"/>.</para>
/// <para><see cref="Kaizen.RefCounted"/> instances caught in a cyclic reference will <b>not</b> be freed automatically. For example, if a node holds a reference to instance <c>A</c>, which directly or indirectly holds a reference back to <c>A</c>, <c>A</c>'s reference count will be 2. Destruction of the node will leave <c>A</c> dangling with a reference count of 1, and there will be a memory leak. To prevent this, one of the references in the cycle can be made weak with <c>@GlobalScope.weakref</c>.</para>
/// <para>In the vast majority of use cases, instantiating and using <see cref="Kaizen.RefCounted"/>-derived types is all you need to do. The methods provided in this class are only for advanced users, and can cause issues if misused.</para>
/// <para><b>Note:</b> In C#, reference-counted objects will not be freed instantly after they are no longer in use. Instead, garbage collection will run periodically and will free reference-counted objects that are no longer in use. This means that unused ones will remain in memory for a while before being removed.</para>
/// </summary>
public partial class RefCounted : KaizenObject
{
    private static readonly System.Type CachedType = typeof(RefCounted);

    private static readonly StringName NativeName = "RefCounted";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public RefCounted() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal RefCounted(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal RefCounted(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.InitRef, 2240911060ul);

    /// <summary>
    /// <para>Initializes the internal reference counter. Use this only if you really know what you are doing.</para>
    /// <para>Returns whether the initialization was successful.</para>
    /// </summary>
    public bool InitRef()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind0, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Reference, 2240911060ul);

    /// <summary>
    /// <para>Increments the internal reference counter. Use this only if you really know what you are doing.</para>
    /// <para>Returns <see langword="true"/> if the increment was successful, <see langword="false"/> otherwise.</para>
    /// </summary>
    public bool Reference()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind1, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Unreference, 2240911060ul);

    /// <summary>
    /// <para>Decrements the internal reference counter. Use this only if you really know what you are doing.</para>
    /// <para>Returns <see langword="true"/> if the object should be freed after the decrement, <see langword="false"/> otherwise.</para>
    /// </summary>
    public bool Unreference()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind2, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetReferenceCount, 3905245786ul);

    /// <summary>
    /// <para>Returns the current reference count.</para>
    /// </summary>
    public int GetReferenceCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// Invokes the method with the given name, using the given arguments.
    /// This method is used by Kaizen to invoke methods from the engine side.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="method">Name of the method to invoke.</param>
    /// <param name="args">Arguments to use with the invoked method.</param>
    /// <param name="ret">Value returned by the invoked method.</param>
#pragma warning disable CS0618 // Member is obsolete
    protected internal override bool InvokeKaizenClassMethod(in kaizen_string_name method, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        return base.InvokeKaizenClassMethod(method, args, out ret);
    }
#pragma warning restore CS0618

    /// <summary>
    /// Check if the type contains a method with the given name.
    /// This method is used by Kaizen to check if a method exists before invoking it.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="method">Name of the method to check for.</param>

    protected internal override bool HasKaizenClassMethod(in kaizen_string_name method)
    {
        return base.HasKaizenClassMethod(method);
    }

    /// <summary>
    /// Check if the type contains a signal with the given name.
    /// This method is used by Kaizen to check if a signal exists before raising it.
    /// Do not call or override this method.
    /// </summary>
    /// <param name="signal">Name of the signal to check for.</param>

    protected internal override bool HasKaizenClassSignal(in kaizen_string_name signal)
    {
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : KaizenObject.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
        /// <summary>
        /// Cached name for the 'init_ref' method.
        /// </summary>
        public static readonly StringName InitRef = "init_ref";
        /// <summary>
        /// Cached name for the 'reference' method.
        /// </summary>
        public static readonly StringName Reference = "reference";
        /// <summary>
        /// Cached name for the 'unreference' method.
        /// </summary>
        public static readonly StringName Unreference = "unreference";
        /// <summary>
        /// Cached name for the 'get_reference_count' method.
        /// </summary>
        public static readonly StringName GetReferenceCount = "get_reference_count";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
