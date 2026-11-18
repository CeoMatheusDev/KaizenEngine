namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>The compositor resource stores attributes used to customize how a <see cref="Kaizen.Viewport"/> is rendered.</para>
/// </summary>
public partial class Compositor : Resource
{
    /// <summary>
    /// <para>The custom <see cref="Kaizen.CompositorEffect"/>s that are applied during rendering of viewports using this compositor.</para>
    /// </summary>
    public Kaizen.Collections.Array<CompositorEffect> CompositorEffects
    {
        get
        {
            return GetCompositorEffects();
        }
        set
        {
            SetCompositorEffects(value);
        }
    }

    private static readonly System.Type CachedType = typeof(Compositor);

    private static readonly StringName NativeName = "Compositor";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public Compositor() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal Compositor(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal Compositor(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCompositorEffects, 381264803ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCompositorEffects(Kaizen.Collections.Array<CompositorEffect> compositorEffects)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind0, KaizenObject.GetPtr(this), (kaizen_array)(compositorEffects ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCompositorEffects, 3995934104ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Array<CompositorEffect> GetCompositorEffects()
    {
        Kaizen.Collections.Array<CompositorEffect> ret = new Kaizen.Collections.Array<CompositorEffect>(NativeCalls.kaizen_icall_0_146(MethodBind1, KaizenObject.GetPtr(this)));
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
    public new class PropertyName : Resource.PropertyName
    {
        /// <summary>
        /// Cached name for the 'compositor_effects' property.
        /// </summary>
        public static readonly StringName CompositorEffects = "compositor_effects";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_compositor_effects' method.
        /// </summary>
        public static readonly StringName SetCompositorEffects = "set_compositor_effects";
        /// <summary>
        /// Cached name for the 'get_compositor_effects' method.
        /// </summary>
        public static readonly StringName GetCompositorEffects = "get_compositor_effects";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
