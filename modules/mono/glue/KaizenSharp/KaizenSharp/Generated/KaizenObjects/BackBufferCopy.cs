namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Node for back-buffering the currently-displayed screen. The region defined in the <see cref="Kaizen.BackBufferCopy"/> node is buffered with the content of the screen it covers, or the entire screen according to the <see cref="Kaizen.BackBufferCopy.CopyMode"/>. It can be accessed in shader scripts using the screen texture (i.e. a uniform sampler with <c>hint_screen_texture</c>).</para>
/// <para><b>Note:</b> Since this node inherits from <see cref="Kaizen.Node2D"/> (and not <see cref="Kaizen.Control"/>), anchors and margins won't apply to child <see cref="Kaizen.Control"/>-derived nodes. This can be problematic when resizing the window. To avoid this, add <see cref="Kaizen.Control"/>-derived nodes as <i>siblings</i> to the <see cref="Kaizen.BackBufferCopy"/> node instead of adding them as children.</para>
/// </summary>
public partial class BackBufferCopy : Node2D
{
    public enum CopyModeEnum : long
    {
        /// <summary>
        /// <para>Disables the buffering mode. This means the <see cref="Kaizen.BackBufferCopy"/> node will directly use the portion of screen it covers.</para>
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// <para><see cref="Kaizen.BackBufferCopy"/> buffers a rectangular region.</para>
        /// </summary>
        Rect = 1,
        /// <summary>
        /// <para><see cref="Kaizen.BackBufferCopy"/> buffers the entire screen.</para>
        /// </summary>
        Viewport = 2
    }

    /// <summary>
    /// <para>Buffer mode.</para>
    /// </summary>
    public BackBufferCopy.CopyModeEnum CopyMode
    {
        get
        {
            return GetCopyMode();
        }
        set
        {
            SetCopyMode(value);
        }
    }

    /// <summary>
    /// <para>The area covered by the <see cref="Kaizen.BackBufferCopy"/>. Only used if <see cref="Kaizen.BackBufferCopy.CopyMode"/> is <see cref="Kaizen.BackBufferCopy.CopyModeEnum.Rect"/>.</para>
    /// </summary>
    public Rect2 Rect
    {
        get
        {
            return GetRect();
        }
        set
        {
            SetRect(value);
        }
    }

    private static readonly System.Type CachedType = typeof(BackBufferCopy);

    private static readonly StringName NativeName = "BackBufferCopy";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public BackBufferCopy() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal BackBufferCopy(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal BackBufferCopy(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRect, 2046264180ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public unsafe void SetRect(Rect2 rect)
    {
        NativeCalls.kaizen_icall_1_214(MethodBind0, KaizenObject.GetPtr(this), &rect);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRect, 1639390495ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Rect2 GetRect()
    {
        Rect2 ret = NativeCalls.kaizen_icall_0_215(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCopyMode, 1713538590ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCopyMode(BackBufferCopy.CopyModeEnum copyMode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)copyMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCopyMode, 3271169440ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public BackBufferCopy.CopyModeEnum GetCopyMode()
    {
        BackBufferCopy.CopyModeEnum ret = (BackBufferCopy.CopyModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
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
    public new class PropertyName : Node2D.PropertyName
    {
        /// <summary>
        /// Cached name for the 'copy_mode' property.
        /// </summary>
        public static readonly StringName CopyMode = "copy_mode";
        /// <summary>
        /// Cached name for the 'rect' property.
        /// </summary>
        public static readonly StringName Rect = "rect";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Node2D.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_rect' method.
        /// </summary>
        public static readonly StringName SetRect = "set_rect";
        /// <summary>
        /// Cached name for the 'get_rect' method.
        /// </summary>
        public static readonly StringName GetRect = "get_rect";
        /// <summary>
        /// Cached name for the 'set_copy_mode' method.
        /// </summary>
        public static readonly StringName SetCopyMode = "set_copy_mode";
        /// <summary>
        /// Cached name for the 'get_copy_mode' method.
        /// </summary>
        public static readonly StringName GetCopyMode = "get_copy_mode";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Node2D.SignalName
    {
    }
}
