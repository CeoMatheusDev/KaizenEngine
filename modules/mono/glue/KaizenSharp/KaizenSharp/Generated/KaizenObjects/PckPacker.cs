namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>The <see cref="Kaizen.PckPacker"/> is used to create packages that can be loaded into a running project using <see cref="Kaizen.ProjectSettings.LoadResourcePack(string, bool, int)"/>.</para>
/// <para><code>
/// var packer = new PckPacker();
/// packer.PckStart("test.pck");
/// packer.AddFile("res://text.txt", "text.txt");
/// packer.Flush();
/// </code></para>
/// <para>The above <see cref="Kaizen.PckPacker"/> creates package <c>test.pck</c>, then adds a file named <c>text.txt</c> at the root of the package.</para>
/// <para><b>Note:</b> PCK is Kaizen's own pack file format. To create ZIP archives that can be read by any program, use <see cref="Kaizen.ZipPacker"/> instead.</para>
/// </summary>
[KaizenClassName("PCKPacker")]
public partial class PckPacker : RefCounted
{
    private static readonly System.Type CachedType = typeof(PckPacker);

    private static readonly StringName NativeName = "PCKPacker";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public PckPacker() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal PckPacker(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal PckPacker(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.PckStart, 508410629ul);

    /// <summary>
    /// <para>Creates a new PCK file at the file path <paramref name="pckPath"/>. The <c>.pck</c> file extension isn't added automatically, so it should be part of <paramref name="pckPath"/> (even though it's not required).</para>
    /// </summary>
    public Error PckStart(string pckPath, int alignment = 32, string key = "0000000000000000000000000000000000000000000000000000000000000000", bool encryptDirectory = false)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_4_1012(MethodBind0, KaizenObject.GetPtr(this), pckPath, alignment, key, encryptDirectory.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddFile, 2215643711ul);

    /// <summary>
    /// <para>Adds the <paramref name="sourcePath"/> file to the current PCK package at the <paramref name="targetPath"/> internal path. The <c>res://</c> prefix for <paramref name="targetPath"/> is optional and stripped internally. File content is immediately written to the PCK.</para>
    /// </summary>
    public Error AddFile(string targetPath, string sourcePath, bool encrypt = false)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_3_1013(MethodBind1, KaizenObject.GetPtr(this), targetPath, sourcePath, encrypt.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddFileFromBuffer, 1131482346ul);

    /// <summary>
    /// <para>Adds the <paramref name="data"/> to the current PCK package at the <paramref name="targetPath"/> internal path. The <c>res://</c> prefix for <paramref name="targetPath"/> is optional and stripped internally. File content is immediately written to the PCK.</para>
    /// </summary>
    public Error AddFileFromBuffer(string targetPath, byte[] data, bool encrypt = false)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_3_1014(MethodBind2, KaizenObject.GetPtr(this), targetPath, data, encrypt.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Adds the <paramref name="data"/> to the current PCK package at the <paramref name="targetPath"/> internal path. The <c>res://</c> prefix for <paramref name="targetPath"/> is optional and stripped internally. File content is immediately written to the PCK.</para>
    /// </summary>
    public Error AddFileFromBuffer(string targetPath, ReadOnlySpan<byte> data, bool encrypt)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_3_1014(MethodBind2, KaizenObject.GetPtr(this), targetPath, data, encrypt.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddFileRemoval, 166001499ul);

    /// <summary>
    /// <para>Registers a file removal of the <paramref name="targetPath"/> internal path to the PCK. This is mainly used for patches. If the file at this path has been loaded from a previous PCK, it will be removed. The <c>res://</c> prefix for <paramref name="targetPath"/> is optional and stripped internally.</para>
    /// </summary>
    public Error AddFileRemoval(string targetPath)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_159(MethodBind3, KaizenObject.GetPtr(this), targetPath);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Flush, 1633102583ul);

    /// <summary>
    /// <para>Writes the file directory and closes the PCK. If <paramref name="verbose"/> is <see langword="true"/>, a list of files added will be printed to the console for easier debugging.</para>
    /// <para><b>Note:</b> <see cref="Kaizen.PckPacker"/> will automatically flush when it's freed, which happens when it goes out of scope or when it gets assigned with <see langword="null"/>. In C# the reference must be disposed after use, either with the <c>using</c> statement or by calling the <c>Dispose</c> method directly.</para>
    /// </summary>
    public Error Flush(bool verbose = false)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_223(MethodBind4, KaizenObject.GetPtr(this), verbose.ToKaizenBool());
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
    public new class PropertyName : RefCounted.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the 'pck_start' method.
        /// </summary>
        public static readonly StringName PckStart = "pck_start";
        /// <summary>
        /// Cached name for the 'add_file' method.
        /// </summary>
        public static readonly StringName AddFile = "add_file";
        /// <summary>
        /// Cached name for the 'add_file_from_buffer' method.
        /// </summary>
        public static readonly StringName AddFileFromBuffer = "add_file_from_buffer";
        /// <summary>
        /// Cached name for the 'add_file_removal' method.
        /// </summary>
        public static readonly StringName AddFileRemoval = "add_file_removal";
        /// <summary>
        /// Cached name for the 'flush' method.
        /// </summary>
        public static readonly StringName Flush = "flush";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
