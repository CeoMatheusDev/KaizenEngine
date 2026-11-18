namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.CubemapArray"/>s are made of an array of <see cref="Kaizen.Cubemap"/>s. Like <see cref="Kaizen.Cubemap"/>s, they are made of multiple textures, the amount of which must be divisible by 6 (one for each face of the cube).</para>
/// <para>The primary benefit of <see cref="Kaizen.CubemapArray"/>s is that they can be accessed in shader code using a single texture reference. In other words, you can pass multiple <see cref="Kaizen.Cubemap"/>s into a shader using a single <see cref="Kaizen.CubemapArray"/>. <see cref="Kaizen.Cubemap"/>s are allocated in adjacent cache regions on the GPU, which makes <see cref="Kaizen.CubemapArray"/>s the most efficient way to store multiple <see cref="Kaizen.Cubemap"/>s.</para>
/// <para>Kaizen uses <see cref="Kaizen.CubemapArray"/>s internally for many effects, including the <see cref="Kaizen.Sky"/> if you set <c>ProjectSettings.rendering/reflections/sky_reflections/texture_array_reflections</c> to <see langword="true"/>.</para>
/// <para>To create such a texture file yourself, reimport your image files using the Kaizen Editor import presets. To create a CubemapArray from code, use <see cref="Kaizen.ImageTextureLayered.CreateFromImages(Kaizen.Collections.Array{Image})"/> on an instance of the CubemapArray class.</para>
/// <para>The expected image order is X+, X-, Y+, Y-, Z+, Z- (in Kaizen's coordinate system, so Y+ is "up" and Z- is "forward"). You can use one of the following templates as a base:</para>
/// <para>- <a href="https://raw.githubusercontent.com/kaizenengine/kaizen-docs/master/tutorials/assets_pipeline/img/cubemap_template_2x3.webp">2×3 cubemap template (default layout option)</a></para>
/// <para>- <a href="https://raw.githubusercontent.com/kaizenengine/kaizen-docs/master/tutorials/assets_pipeline/img/cubemap_template_3x2.webp">3×2 cubemap template</a></para>
/// <para>- <a href="https://raw.githubusercontent.com/kaizenengine/kaizen-docs/master/tutorials/assets_pipeline/img/cubemap_template_1x6.webp">1×6 cubemap template</a></para>
/// <para>- <a href="https://raw.githubusercontent.com/kaizenengine/kaizen-docs/master/tutorials/assets_pipeline/img/cubemap_template_6x1.webp">6×1 cubemap template</a></para>
/// <para>Multiple layers are stacked on top of each other when using the default vertical import option (with the first layer at the top). Alternatively, you can choose a horizontal layout in the import options (with the first layer at the left).</para>
/// <para><b>Note:</b> <see cref="Kaizen.CubemapArray"/> is not supported in the Compatibility renderer due to graphics API limitations.</para>
/// </summary>
public partial class CubemapArray : ImageTextureLayered
{
    private static readonly System.Type CachedType = typeof(CubemapArray);

    private static readonly StringName NativeName = "CubemapArray";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public CubemapArray() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal CubemapArray(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal CubemapArray(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreatePlaceholder, 121922552ul);

    /// <summary>
    /// <para>Creates a placeholder version of this resource (<see cref="Kaizen.PlaceholderCubemapArray"/>).</para>
    /// </summary>
    public Resource CreatePlaceholder()
    {
        Resource ret = (Resource)NativeCalls.kaizen_icall_0_87(MethodBind0, KaizenObject.GetPtr(this));
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
    public new class PropertyName : ImageTextureLayered.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : ImageTextureLayered.MethodName
    {
        /// <summary>
        /// Cached name for the 'create_placeholder' method.
        /// </summary>
        public static readonly StringName CreatePlaceholder = "create_placeholder";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : ImageTextureLayered.SignalName
    {
    }
}
