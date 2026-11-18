namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A <see cref="Kaizen.ViewportTexture"/> provides the content of a <see cref="Kaizen.Viewport"/> as a dynamic <see cref="Kaizen.Texture2D"/>. This can be used to combine the rendering of <see cref="Kaizen.Control"/>, <see cref="Kaizen.Node2D"/> and <see cref="Kaizen.Node3D"/> nodes. For example, you can use this texture to display a 3D scene inside a <see cref="Kaizen.TextureRect"/>, or a 2D overlay in a <see cref="Kaizen.Sprite3D"/>.</para>
/// <para>To get a <see cref="Kaizen.ViewportTexture"/> in code, use the <see cref="Kaizen.Viewport.GetTexture()"/> method on the target viewport.</para>
/// <para><b>Note:</b> A <see cref="Kaizen.ViewportTexture"/> is always local to its scene (see <see cref="Kaizen.Resource.ResourceLocalToScene"/>). If the scene root is not ready, it may return incorrect data (see <see cref="Kaizen.Node.Ready"/>).</para>
/// <para><b>Note:</b> Instantiating scenes containing a high-resolution <see cref="Kaizen.ViewportTexture"/> may cause noticeable stutter.</para>
/// <para><b>Note:</b> When using a <see cref="Kaizen.Viewport"/> with <see cref="Kaizen.Viewport.UseHdr2D"/> set to <see langword="true"/>, the returned texture will be an HDR image that uses linear encoding. This may look darker than normal when displayed directly on screen. To convert to nonlinear sRGB encoding, use <see cref="Kaizen.Image.LinearToSrgb()"/>.</para>
/// <para><b>Note:</b> Some nodes such as <see cref="Kaizen.Decal"/>, <see cref="Kaizen.Light3D"/>, and <see cref="Kaizen.PointLight2D"/> do not support using <see cref="Kaizen.ViewportTexture"/> directly. To use texture data from a <see cref="Kaizen.ViewportTexture"/> in these nodes, you need to create an <see cref="Kaizen.ImageTexture"/> by calling <see cref="Kaizen.Texture2D.GetImage()"/> on the <see cref="Kaizen.ViewportTexture"/> and passing the result to <see cref="Kaizen.ImageTexture.CreateFromImage(Image)"/>. This conversion is a slow operation, so it should not be performed every frame.</para>
/// </summary>
public partial class ViewportTexture : Texture2D
{
    /// <summary>
    /// <para>The path to the <see cref="Kaizen.Viewport"/> node to display. This is relative to the local scene root (see <see cref="Kaizen.Resource.GetLocalScene()"/>), <b>not</b> to the nodes that use this texture.</para>
    /// <para><b>Note:</b> In the editor, this path is automatically updated when the target viewport or one of its ancestors is renamed or moved. At runtime, this path may not automatically update if the scene root cannot be found.</para>
    /// </summary>
    public NodePath ViewportPath
    {
        get
        {
            return GetViewportPathInScene();
        }
        set
        {
            SetViewportPathInScene(value);
        }
    }

    private static readonly System.Type CachedType = typeof(ViewportTexture);

    private static readonly StringName NativeName = "ViewportTexture";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public ViewportTexture() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal ViewportTexture(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal ViewportTexture(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetViewportPathInScene, 1348162250ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetViewportPathInScene(NodePath path)
    {
        NativeCalls.kaizen_icall_1_149(MethodBind0, KaizenObject.GetPtr(this), (kaizen_node_path)(path?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetViewportPathInScene, 4075236667ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public NodePath GetViewportPathInScene()
    {
        NodePath ret = NativeCalls.kaizen_icall_0_150(MethodBind1, KaizenObject.GetPtr(this));
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
    public new class PropertyName : Texture2D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'viewport_path' property.
        /// </summary>
        public static readonly StringName ViewportPath = "viewport_path";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Texture2D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_viewport_path_in_scene' method.
        /// </summary>
        public static readonly StringName SetViewportPathInScene = "set_viewport_path_in_scene";
        /// <summary>
        /// Cached name for the 'get_viewport_path_in_scene' method.
        /// </summary>
        public static readonly StringName GetViewportPathInScene = "get_viewport_path_in_scene";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Texture2D.SignalName
    {
    }
}
