namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>The GDExtensionManager loads, initializes, and keeps track of all available <see cref="Kaizen.GDExtension"/> libraries in the project.</para>
/// <para><b>Note:</b> Do not worry about GDExtension unless you know what you are doing.</para>
/// </summary>
[KaizenClassName("GDExtensionManager")]
public partial class GDExtensionManagerInstance : KaizenObject
{
    private static readonly System.Type CachedType = typeof(GDExtensionManagerInstance);

    private static readonly StringName NativeName = "GDExtensionManager";

    internal GDExtensionManagerInstance() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal GDExtensionManagerInstance(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal GDExtensionManagerInstance(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.LoadExtension, 4024158731ul);

    /// <summary>
    /// <para>Loads an extension by absolute file path. The <paramref name="path"/> needs to point to a valid <see cref="Kaizen.GDExtension"/>. Returns <see cref="Kaizen.GDExtensionManager.LoadStatus.Ok"/> if successful.</para>
    /// </summary>
    public GDExtensionManager.LoadStatus LoadExtension(string path)
    {
        GDExtensionManager.LoadStatus ret = (GDExtensionManager.LoadStatus)NativeCalls.kaizen_icall_1_159(MethodBind0, KaizenObject.GetPtr(this), path);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ReloadExtension, 4024158731ul);

    /// <summary>
    /// <para>Reloads the extension at the given file path. The <paramref name="path"/> needs to point to a valid <see cref="Kaizen.GDExtension"/>, otherwise this method may return either <see cref="Kaizen.GDExtensionManager.LoadStatus.NotLoaded"/> or <see cref="Kaizen.GDExtensionManager.LoadStatus.Failed"/>.</para>
    /// <para><b>Note:</b> You can only reload extensions in the editor. In release builds, this method always fails and returns <see cref="Kaizen.GDExtensionManager.LoadStatus.Failed"/>.</para>
    /// </summary>
    public GDExtensionManager.LoadStatus ReloadExtension(string path)
    {
        GDExtensionManager.LoadStatus ret = (GDExtensionManager.LoadStatus)NativeCalls.kaizen_icall_1_159(MethodBind1, KaizenObject.GetPtr(this), path);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UnloadExtension, 4024158731ul);

    /// <summary>
    /// <para>Unloads an extension by file path. The <paramref name="path"/> needs to point to an already loaded <see cref="Kaizen.GDExtension"/>, otherwise this method returns <see cref="Kaizen.GDExtensionManager.LoadStatus.NotLoaded"/>.</para>
    /// </summary>
    public GDExtensionManager.LoadStatus UnloadExtension(string path)
    {
        GDExtensionManager.LoadStatus ret = (GDExtensionManager.LoadStatus)NativeCalls.kaizen_icall_1_159(MethodBind2, KaizenObject.GetPtr(this), path);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsExtensionLoaded, 3927539163ul);

    /// <summary>
    /// <para>Returns <see langword="true"/> if the extension at the given file <paramref name="path"/> has already been loaded successfully. See also <see cref="Kaizen.GDExtensionManagerInstance.GetLoadedExtensions()"/>.</para>
    /// </summary>
    public bool IsExtensionLoaded(string path)
    {
        bool ret = NativeCalls.kaizen_icall_1_156(MethodBind3, KaizenObject.GetPtr(this), path).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLoadedExtensions, 1139954409ul);

    /// <summary>
    /// <para>Returns the file paths of all currently loaded extensions.</para>
    /// </summary>
    public string[] GetLoadedExtensions()
    {
        string[] ret = NativeCalls.kaizen_icall_0_132(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetExtension, 49743343ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.GDExtension"/> at the given file <paramref name="path"/>, or <see langword="null"/> if it has not been loaded or does not exist.</para>
    /// </summary>
    public GDExtension GetExtension(string path)
    {
        GDExtension ret = (GDExtension)NativeCalls.kaizen_icall_1_551(MethodBind5, KaizenObject.GetPtr(this), path);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Emitted after the editor has finished reloading one or more extensions.</para>
    /// </summary>
    public event Action ExtensionsReloaded
    {
        add => Connect(SignalName.ExtensionsReloaded, Callable.From(value));
        remove => Disconnect(SignalName.ExtensionsReloaded, Callable.From(value));
    }

    protected void EmitSignalExtensionsReloaded()
    {
        EmitSignal(SignalName.ExtensionsReloaded);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.GDExtensionManagerInstance.ExtensionLoaded"/> event of a <see cref="Kaizen.GDExtensionManagerInstance"/> class.
    /// </summary>
    public delegate void ExtensionLoadedEventHandler(GDExtension extension);

    private static void ExtensionLoadedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((ExtensionLoadedEventHandler)delegateObj)(VariantUtils.ConvertTo<GDExtension>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted after the editor has finished loading a new extension.</para>
    /// <para><b>Note:</b> This signal is only emitted in editor builds.</para>
    /// </summary>
    public unsafe event ExtensionLoadedEventHandler ExtensionLoaded
    {
        add => Connect(SignalName.ExtensionLoaded, Callable.CreateWithUnsafeTrampoline(value, &ExtensionLoadedTrampoline));
        remove => Disconnect(SignalName.ExtensionLoaded, Callable.CreateWithUnsafeTrampoline(value, &ExtensionLoadedTrampoline));
    }

    protected void EmitSignalExtensionLoaded(GDExtension extension)
    {
        EmitSignal(SignalName.ExtensionLoaded, extension);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.GDExtensionManagerInstance.ExtensionUnloading"/> event of a <see cref="Kaizen.GDExtensionManagerInstance"/> class.
    /// </summary>
    public delegate void ExtensionUnloadingEventHandler(GDExtension extension);

    private static void ExtensionUnloadingTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((ExtensionUnloadingEventHandler)delegateObj)(VariantUtils.ConvertTo<GDExtension>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Emitted before the editor starts unloading an extension.</para>
    /// <para><b>Note:</b> This signal is only emitted in editor builds.</para>
    /// </summary>
    public unsafe event ExtensionUnloadingEventHandler ExtensionUnloading
    {
        add => Connect(SignalName.ExtensionUnloading, Callable.CreateWithUnsafeTrampoline(value, &ExtensionUnloadingTrampoline));
        remove => Disconnect(SignalName.ExtensionUnloading, Callable.CreateWithUnsafeTrampoline(value, &ExtensionUnloadingTrampoline));
    }

    protected void EmitSignalExtensionUnloading(GDExtension extension)
    {
        EmitSignal(SignalName.ExtensionUnloading, extension);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_extensions_reloaded = "ExtensionsReloaded";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_extension_loaded = "ExtensionLoaded";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_extension_unloading = "ExtensionUnloading";

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
        if (signal == SignalName.ExtensionsReloaded)
        {
            if (HasKaizenClassSignal(SignalProxyName_extensions_reloaded.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.ExtensionLoaded)
        {
            if (HasKaizenClassSignal(SignalProxyName_extension_loaded.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (signal == SignalName.ExtensionUnloading)
        {
            if (HasKaizenClassSignal(SignalProxyName_extension_unloading.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
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
        /// Cached name for the 'load_extension' method.
        /// </summary>
        public static readonly StringName LoadExtension = "load_extension";
        /// <summary>
        /// Cached name for the 'reload_extension' method.
        /// </summary>
        public static readonly StringName ReloadExtension = "reload_extension";
        /// <summary>
        /// Cached name for the 'unload_extension' method.
        /// </summary>
        public static readonly StringName UnloadExtension = "unload_extension";
        /// <summary>
        /// Cached name for the 'is_extension_loaded' method.
        /// </summary>
        public static readonly StringName IsExtensionLoaded = "is_extension_loaded";
        /// <summary>
        /// Cached name for the 'get_loaded_extensions' method.
        /// </summary>
        public static readonly StringName GetLoadedExtensions = "get_loaded_extensions";
        /// <summary>
        /// Cached name for the 'get_extension' method.
        /// </summary>
        public static readonly StringName GetExtension = "get_extension";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
        /// <summary>
        /// Cached name for the 'extensions_reloaded' signal.
        /// </summary>
        public static readonly StringName ExtensionsReloaded = "extensions_reloaded";
        /// <summary>
        /// Cached name for the 'extension_loaded' signal.
        /// </summary>
        public static readonly StringName ExtensionLoaded = "extension_loaded";
        /// <summary>
        /// Cached name for the 'extension_unloading' signal.
        /// </summary>
        public static readonly StringName ExtensionUnloading = "extension_unloading";
    }
}
