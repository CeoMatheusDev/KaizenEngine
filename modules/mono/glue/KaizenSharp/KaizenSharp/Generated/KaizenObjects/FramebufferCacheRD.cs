namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Framebuffer cache manager for <see cref="Kaizen.RenderingDevice"/>-based renderers. Provides a way to create a framebuffer and reuse it in subsequent calls for as long as the used textures exists. Framebuffers will automatically be cleaned up when dependent objects are freed.</para>
/// </summary>
public partial class FramebufferCacheRD : KaizenObject
{
    private static readonly System.Type CachedType = typeof(FramebufferCacheRD);

    private static readonly StringName NativeName = "FramebufferCacheRD";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public FramebufferCacheRD() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal FramebufferCacheRD(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal FramebufferCacheRD(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCacheMultipass, 3437881813ul);

    /// <summary>
    /// <para>Creates, or obtains a cached, framebuffer. <paramref name="textures"/> lists textures accessed. <paramref name="passes"/> defines the subpasses and texture allocation, if left empty a single pass is created and textures are allocated depending on their usage flags. <paramref name="views"/> defines the number of views used when rendering.</para>
    /// </summary>
    public static Rid GetCacheMultipass(Kaizen.Collections.Array<Rid> textures, Kaizen.Collections.Array<RDFramebufferPass> passes, uint views)
    {
        Rid ret = NativeCalls.kaizen_icall_3_654(MethodBind0, (kaizen_array)(textures ?? new()).NativeValue, (kaizen_array)(passes ?? new()).NativeValue, views);
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
        /// Cached name for the 'get_cache_multipass' method.
        /// </summary>
        public static readonly StringName GetCacheMultipass = "get_cache_multipass";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
