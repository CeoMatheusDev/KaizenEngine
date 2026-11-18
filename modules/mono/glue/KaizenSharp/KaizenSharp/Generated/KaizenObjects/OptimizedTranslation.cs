namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>An optimized translation. Uses real-time compressed translations, which results in very small dictionaries.</para>
/// <para>This class does not store the untranslated strings for optimization purposes. Therefore, <see cref="Kaizen.Translation.GetMessageList()"/> always returns an empty array, and <see cref="Kaizen.Translation.GetMessageCount()"/> always returns <c>0</c>.</para>
/// </summary>
public partial class OptimizedTranslation : Translation
{
    private static readonly System.Type CachedType = typeof(OptimizedTranslation);

    private static readonly StringName NativeName = "OptimizedTranslation";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public OptimizedTranslation() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal OptimizedTranslation(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal OptimizedTranslation(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Generate, 2141509306ul);

    /// <summary>
    /// <para>Generates and sets an optimized translation from the given <see cref="Kaizen.Translation"/> resource. Returns <see langword="true"/> if successful.</para>
    /// <para><b>Note:</b> Messages in <paramref name="from"/> should not use context or plural forms.</para>
    /// <para><b>Note:</b> This method is intended to be used in the editor. It does nothing when called from an exported project.</para>
    /// </summary>
    public bool Generate(Translation from)
    {
        bool ret = NativeCalls.kaizen_icall_1_202(MethodBind0, KaizenObject.GetPtr(this), KaizenObject.GetPtr(from)).ToBool();
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
    public new class PropertyName : Translation.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Translation.MethodName
    {
        /// <summary>
        /// Cached name for the 'generate' method.
        /// </summary>
        public static readonly StringName Generate = "generate";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Translation.SignalName
    {
    }
}
