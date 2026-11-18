namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A stream peer that handles TLS connections. This object can be used to connect to a TLS server or accept a single TLS client connection.</para>
/// <para><b>Note:</b> When exporting to Android, make sure to enable the <c>INTERNET</c> permission in the Android export preset before exporting the project or using remote deploy. Otherwise, network communication of any kind will be blocked by Android.</para>
/// </summary>
[KaizenClassName("StreamPeerTLS")]
public partial class StreamPeerTls : StreamPeer
{
    public enum Status : long
    {
        /// <summary>
        /// <para>A status representing a <see cref="Kaizen.StreamPeerTls"/> that is disconnected.</para>
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// <para>A status representing a <see cref="Kaizen.StreamPeerTls"/> during handshaking.</para>
        /// </summary>
        Handshaking = 1,
        /// <summary>
        /// <para>A status representing a <see cref="Kaizen.StreamPeerTls"/> that is connected to a host.</para>
        /// </summary>
        Connected = 2,
        /// <summary>
        /// <para>A status representing a <see cref="Kaizen.StreamPeerTls"/> in error state.</para>
        /// </summary>
        Error = 3,
        /// <summary>
        /// <para>An error status that shows a mismatch in the TLS certificate domain presented by the host and the domain requested for validation.</para>
        /// </summary>
        ErrorHostnameMismatch = 4
    }

    private static readonly System.Type CachedType = typeof(StreamPeerTls);

    private static readonly StringName NativeName = "StreamPeerTLS";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public StreamPeerTls() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal StreamPeerTls(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal StreamPeerTls(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Poll, 3218959716ul);

    /// <summary>
    /// <para>Poll the connection to check for incoming bytes. Call this right before <see cref="Kaizen.StreamPeer.GetAvailableBytes()"/> for it to work properly.</para>
    /// </summary>
    public void Poll()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AcceptStream, 4292689651ul);

    /// <summary>
    /// <para>Accepts a peer connection as a server using the given <paramref name="serverOptions"/>. See <see cref="Kaizen.TlsOptions.Server(CryptoKey, X509Certificate)"/>.</para>
    /// </summary>
    public Error AcceptStream(StreamPeer stream, TlsOptions serverOptions)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_2_1351(MethodBind1, KaizenObject.GetPtr(this), KaizenObject.GetPtr(stream), KaizenObject.GetPtr(serverOptions));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ConnectToStream, 57169517ul);

    /// <summary>
    /// <para>Connects to a peer using an underlying <see cref="Kaizen.StreamPeer"/> <paramref name="stream"/> and verifying the remote certificate is correctly signed for the given <paramref name="commonName"/>. You can pass the optional <paramref name="clientOptions"/> parameter to customize the trusted certification authorities, or disable the common name verification. See <see cref="Kaizen.TlsOptions.Client(X509Certificate, string)"/> and <see cref="Kaizen.TlsOptions.ClientUnsafe(X509Certificate)"/>.</para>
    /// </summary>
    public Error ConnectToStream(StreamPeer stream, string commonName, TlsOptions clientOptions = null)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_3_1017(MethodBind2, KaizenObject.GetPtr(this), KaizenObject.GetPtr(stream), commonName, KaizenObject.GetPtr(clientOptions));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStatus, 1128380576ul);

    /// <summary>
    /// <para>Returns the status of the connection.</para>
    /// </summary>
    public StreamPeerTls.Status GetStatus()
    {
        StreamPeerTls.Status ret = (StreamPeerTls.Status)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetStream, 2741655269ul);

    /// <summary>
    /// <para>Returns the underlying <see cref="Kaizen.StreamPeer"/> connection, used in <see cref="Kaizen.StreamPeerTls.AcceptStream(StreamPeer, TlsOptions)"/> or <see cref="Kaizen.StreamPeerTls.ConnectToStream(StreamPeer, string, TlsOptions)"/>.</para>
    /// </summary>
    public StreamPeer GetStream()
    {
        StreamPeer ret = (StreamPeer)NativeCalls.kaizen_icall_0_87(MethodBind4, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.DisconnectFromStream, 3218959716ul);

    /// <summary>
    /// <para>Disconnects from host.</para>
    /// </summary>
    public void DisconnectFromStream()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind5, KaizenObject.GetPtr(this));
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
    public new class PropertyName : StreamPeer.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : StreamPeer.MethodName
    {
        /// <summary>
        /// Cached name for the 'poll' method.
        /// </summary>
        public static readonly StringName Poll = "poll";
        /// <summary>
        /// Cached name for the 'accept_stream' method.
        /// </summary>
        public static readonly StringName AcceptStream = "accept_stream";
        /// <summary>
        /// Cached name for the 'connect_to_stream' method.
        /// </summary>
        public static readonly StringName ConnectToStream = "connect_to_stream";
        /// <summary>
        /// Cached name for the 'get_status' method.
        /// </summary>
        public static readonly StringName GetStatus = "get_status";
        /// <summary>
        /// Cached name for the 'get_stream' method.
        /// </summary>
        public static readonly StringName GetStream = "get_stream";
        /// <summary>
        /// Cached name for the 'disconnect_from_stream' method.
        /// </summary>
        public static readonly StringName DisconnectFromStream = "disconnect_from_stream";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : StreamPeer.SignalName
    {
    }
}
