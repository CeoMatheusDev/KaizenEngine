namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>PacketPeer is an abstraction and base class for packet-based protocols (such as UDP). It provides an API for sending and receiving packets both as raw data or variables. This makes it easy to transfer data over a protocol, without having to encode data as low-level bytes or having to worry about network ordering.</para>
/// <para><b>Note:</b> When exporting to Android, make sure to enable the <c>INTERNET</c> permission in the Android export preset before exporting the project or using remote deploy. Otherwise, network communication of any kind will be blocked by Android.</para>
/// </summary>
public partial class PacketPeer : RefCounted
{
    /// <summary>
    /// <para>Maximum buffer size allowed when encoding <see cref="Kaizen.Variant"/>s. Raise this value to support heavier memory allocations.</para>
    /// <para>The <see cref="Kaizen.PacketPeer.PutVar(Variant, bool)"/> method allocates memory on the stack, and the buffer used will grow automatically to the closest power of two to match the size of the <see cref="Kaizen.Variant"/>. If the <see cref="Kaizen.Variant"/> is bigger than <see cref="Kaizen.PacketPeer.EncodeBufferMaxSize"/>, the method will error out with <see cref="Kaizen.Error.OutOfMemory"/>.</para>
    /// </summary>
    public int EncodeBufferMaxSize
    {
        get
        {
            return GetEncodeBufferMaxSize();
        }
        set
        {
            SetEncodeBufferMaxSize(value);
        }
    }

    private static readonly System.Type CachedType = typeof(PacketPeer);

    private static readonly StringName NativeName = "PacketPeer";

    internal PacketPeer() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal PacketPeer(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal PacketPeer(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVar, 3442865206ul);

    /// <summary>
    /// <para>Gets a Variant. If <paramref name="allowObjects"/> is <see langword="true"/>, decoding objects is allowed.</para>
    /// <para>Internally, this uses the same decoding mechanism as the <c>@GlobalScope.bytes_to_var</c> method.</para>
    /// <para><b>Warning:</b> Deserialized objects can contain code which gets executed. Do not use this option if the serialized object comes from untrusted sources to avoid potential security threats such as remote code execution.</para>
    /// </summary>
    public Variant GetVar(bool allowObjects = false)
    {
        Variant ret = NativeCalls.kaizen_icall_1_582(MethodBind0, KaizenObject.GetPtr(this), allowObjects.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.PutVar, 2436251611ul);

    /// <summary>
    /// <para>Sends a <see cref="Kaizen.Variant"/> as a packet. If <paramref name="fullObjects"/> is <see langword="true"/>, encoding objects is allowed (and can potentially include code).</para>
    /// <para>Internally, this uses the same encoding mechanism as the <c>@GlobalScope.var_to_bytes</c> method.</para>
    /// </summary>
    public Error PutVar(Variant var, bool fullObjects = false)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_2_1016(MethodBind1, KaizenObject.GetPtr(this), var, fullObjects.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPacket, 2115431945ul);

    /// <summary>
    /// <para>Gets a raw packet.</para>
    /// </summary>
    public byte[] GetPacket()
    {
        byte[] ret = NativeCalls.kaizen_icall_0_2(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.PutPacket, 680677267ul);

    /// <summary>
    /// <para>Sends a raw packet.</para>
    /// </summary>
    public Error PutPacket(byte[] buffer)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_747(MethodBind3, KaizenObject.GetPtr(this), buffer);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Sends a raw packet.</para>
    /// </summary>
    public Error PutPacket(ReadOnlySpan<byte> buffer)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_1_747(MethodBind3, KaizenObject.GetPtr(this), buffer);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetPacketError, 3185525595ul);

    /// <summary>
    /// <para>Returns the error state of the last packet received (via <see cref="Kaizen.PacketPeer.GetPacket()"/> and <see cref="Kaizen.PacketPeer.GetVar(bool)"/>).</para>
    /// </summary>
    public Error GetPacketError()
    {
        Error ret = (Error)NativeCalls.kaizen_icall_0_39(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAvailablePacketCount, 3905245786ul);

    /// <summary>
    /// <para>Returns the number of packets currently available in the ring-buffer.</para>
    /// </summary>
    public int GetAvailablePacketCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetEncodeBufferMaxSize, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetEncodeBufferMaxSize()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind6, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEncodeBufferMaxSize, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetEncodeBufferMaxSize(int maxSize)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind7, KaizenObject.GetPtr(this), maxSize);
        global::System.GC.KeepAlive(this);
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
        /// <summary>
        /// Cached name for the 'encode_buffer_max_size' property.
        /// </summary>
        public static readonly StringName EncodeBufferMaxSize = "encode_buffer_max_size";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the 'get_var' method.
        /// </summary>
        public static readonly StringName GetVar = "get_var";
        /// <summary>
        /// Cached name for the 'put_var' method.
        /// </summary>
        public static readonly StringName PutVar = "put_var";
        /// <summary>
        /// Cached name for the 'get_packet' method.
        /// </summary>
        public static readonly StringName GetPacket = "get_packet";
        /// <summary>
        /// Cached name for the 'put_packet' method.
        /// </summary>
        public static readonly StringName PutPacket = "put_packet";
        /// <summary>
        /// Cached name for the 'get_packet_error' method.
        /// </summary>
        public static readonly StringName GetPacketError = "get_packet_error";
        /// <summary>
        /// Cached name for the 'get_available_packet_count' method.
        /// </summary>
        public static readonly StringName GetAvailablePacketCount = "get_available_packet_count";
        /// <summary>
        /// Cached name for the 'get_encode_buffer_max_size' method.
        /// </summary>
        public static readonly StringName GetEncodeBufferMaxSize = "get_encode_buffer_max_size";
        /// <summary>
        /// Cached name for the 'set_encode_buffer_max_size' method.
        /// </summary>
        public static readonly StringName SetEncodeBufferMaxSize = "set_encode_buffer_max_size";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
