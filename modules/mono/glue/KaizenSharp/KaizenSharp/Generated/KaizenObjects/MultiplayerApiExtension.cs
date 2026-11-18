namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This class can be used to extend or replace the default <see cref="Kaizen.MultiplayerApi"/> implementation via script or extensions.</para>
/// <para>The following example extend the default implementation (<see cref="Kaizen.SceneMultiplayer"/>) by logging every RPC being made, and every object being configured for replication.</para>
/// <para></para>
/// <para>Then in your main scene or in an autoload call <see cref="Kaizen.SceneTree.SetMultiplayer(MultiplayerApi, NodePath)"/> to start using your custom <see cref="Kaizen.MultiplayerApi"/>:</para>
/// <para></para>
/// <para>Native extensions can alternatively use the <see cref="Kaizen.MultiplayerApi.SetDefaultInterface(StringName)"/> method during initialization to configure themselves as the default implementation.</para>
/// </summary>
[KaizenClassName("MultiplayerAPIExtension")]
public partial class MultiplayerApiExtension : MultiplayerApi
{
    private static readonly System.Type CachedType = typeof(MultiplayerApiExtension);

    private static readonly StringName NativeName = "MultiplayerAPIExtension";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public MultiplayerApiExtension() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal MultiplayerApiExtension(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal MultiplayerApiExtension(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.MultiplayerApi.MultiplayerPeer"/> is retrieved.</para>
    /// </summary>
    public virtual MultiplayerPeer _GetMultiplayerPeer()
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.GetPeers()"/>.</para>
    /// </summary>
    public virtual int[] _GetPeerIds()
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.GetRemoteSenderId()"/>.</para>
    /// </summary>
    public virtual int _GetRemoteSenderId()
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.GetUniqueId()"/>.</para>
    /// </summary>
    public virtual int _GetUniqueId()
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.ObjectConfigurationAdd(KaizenObject, Variant)"/>.</para>
    /// </summary>
    public virtual Error _ObjectConfigurationAdd(KaizenObject @object, Variant configuration)
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.ObjectConfigurationRemove(KaizenObject, Variant)"/>.</para>
    /// </summary>
    public virtual Error _ObjectConfigurationRemove(KaizenObject @object, Variant configuration)
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.Poll()"/>.</para>
    /// </summary>
    public virtual Error _Poll()
    {
        return default;
    }

    /// <summary>
    /// <para>Callback for <see cref="Kaizen.MultiplayerApi.Rpc(int, KaizenObject, StringName, Kaizen.Collections.Array)"/>.</para>
    /// </summary>
    public virtual Error _Rpc(int peer, KaizenObject @object, StringName method, Kaizen.Collections.Array args)
    {
        return default;
    }

    /// <summary>
    /// <para>Called when the <see cref="Kaizen.MultiplayerApi.MultiplayerPeer"/> is set.</para>
    /// </summary>
    public virtual void _SetMultiplayerPeer(MultiplayerPeer multiplayerPeer)
    {
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_multiplayer_peer = "_GetMultiplayerPeer";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_peer_ids = "_GetPeerIds";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_remote_sender_id = "_GetRemoteSenderId";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_unique_id = "_GetUniqueId";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__object_configuration_add = "_ObjectConfigurationAdd";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__object_configuration_remove = "_ObjectConfigurationRemove";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__poll = "_Poll";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__rpc = "_Rpc";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__set_multiplayer_peer = "_SetMultiplayerPeer";

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
        if ((method == MethodProxyName__get_multiplayer_peer || method == MethodName._GetMultiplayerPeer) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_multiplayer_peer.NativeValue))
        {
            var callRet = _GetMultiplayerPeer();
            ret = VariantUtils.CreateFrom<MultiplayerPeer>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_peer_ids || method == MethodName._GetPeerIds) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_peer_ids.NativeValue))
        {
            var callRet = _GetPeerIds();
            ret = VariantUtils.CreateFrom<int[]>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_remote_sender_id || method == MethodName._GetRemoteSenderId) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_remote_sender_id.NativeValue))
        {
            var callRet = _GetRemoteSenderId();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__get_unique_id || method == MethodName._GetUniqueId) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_unique_id.NativeValue))
        {
            var callRet = _GetUniqueId();
            ret = VariantUtils.CreateFrom<int>(callRet);
            return true;
        }
        if ((method == MethodProxyName__object_configuration_add || method == MethodName._ObjectConfigurationAdd) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__object_configuration_add.NativeValue))
        {
            var callRet = _ObjectConfigurationAdd(VariantUtils.ConvertTo<KaizenObject>(args[0]), VariantUtils.ConvertTo<Variant>(args[1]));
            ret = VariantUtils.CreateFrom<Error>(callRet);
            return true;
        }
        if ((method == MethodProxyName__object_configuration_remove || method == MethodName._ObjectConfigurationRemove) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__object_configuration_remove.NativeValue))
        {
            var callRet = _ObjectConfigurationRemove(VariantUtils.ConvertTo<KaizenObject>(args[0]), VariantUtils.ConvertTo<Variant>(args[1]));
            ret = VariantUtils.CreateFrom<Error>(callRet);
            return true;
        }
        if ((method == MethodProxyName__poll || method == MethodName._Poll) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__poll.NativeValue))
        {
            var callRet = _Poll();
            ret = VariantUtils.CreateFrom<Error>(callRet);
            return true;
        }
        if ((method == MethodProxyName__rpc || method == MethodName._Rpc) && args.Count == 4 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__rpc.NativeValue))
        {
            var callRet = _Rpc(VariantUtils.ConvertTo<int>(args[0]), VariantUtils.ConvertTo<KaizenObject>(args[1]), VariantUtils.ConvertTo<StringName>(args[2]), VariantUtils.ConvertTo<Kaizen.Collections.Array>(args[3]));
            ret = VariantUtils.CreateFrom<Error>(callRet);
            return true;
        }
        if ((method == MethodProxyName__set_multiplayer_peer || method == MethodName._SetMultiplayerPeer) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__set_multiplayer_peer.NativeValue))
        {
            _SetMultiplayerPeer(VariantUtils.ConvertTo<MultiplayerPeer>(args[0]));
            ret = default;
            return true;
        }
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
        if (method == MethodName._GetMultiplayerPeer)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_multiplayer_peer.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetPeerIds)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_peer_ids.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetRemoteSenderId)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_remote_sender_id.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetUniqueId)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_unique_id.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._ObjectConfigurationAdd)
        {
            if (HasKaizenClassMethod(MethodProxyName__object_configuration_add.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._ObjectConfigurationRemove)
        {
            if (HasKaizenClassMethod(MethodProxyName__object_configuration_remove.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._Poll)
        {
            if (HasKaizenClassMethod(MethodProxyName__poll.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._Rpc)
        {
            if (HasKaizenClassMethod(MethodProxyName__rpc.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._SetMultiplayerPeer)
        {
            if (HasKaizenClassMethod(MethodProxyName__set_multiplayer_peer.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
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
    public new class PropertyName : MultiplayerApi.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : MultiplayerApi.MethodName
    {
        /// <summary>
        /// Cached name for the '_get_multiplayer_peer' method.
        /// </summary>
        public static readonly StringName _GetMultiplayerPeer = "_get_multiplayer_peer";
        /// <summary>
        /// Cached name for the '_get_peer_ids' method.
        /// </summary>
        public static readonly StringName _GetPeerIds = "_get_peer_ids";
        /// <summary>
        /// Cached name for the '_get_remote_sender_id' method.
        /// </summary>
        public static readonly StringName _GetRemoteSenderId = "_get_remote_sender_id";
        /// <summary>
        /// Cached name for the '_get_unique_id' method.
        /// </summary>
        public static readonly StringName _GetUniqueId = "_get_unique_id";
        /// <summary>
        /// Cached name for the '_object_configuration_add' method.
        /// </summary>
        public static readonly StringName _ObjectConfigurationAdd = "_object_configuration_add";
        /// <summary>
        /// Cached name for the '_object_configuration_remove' method.
        /// </summary>
        public static readonly StringName _ObjectConfigurationRemove = "_object_configuration_remove";
        /// <summary>
        /// Cached name for the '_poll' method.
        /// </summary>
        public static readonly StringName _Poll = "_poll";
        /// <summary>
        /// Cached name for the '_rpc' method.
        /// </summary>
        public static readonly StringName _Rpc = "_rpc";
        /// <summary>
        /// Cached name for the '_set_multiplayer_peer' method.
        /// </summary>
        public static readonly StringName _SetMultiplayerPeer = "_set_multiplayer_peer";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : MultiplayerApi.SignalName
    {
    }
}
