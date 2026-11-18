namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.EditorDebuggerPlugin"/> provides functions related to the editor side of the debugger.</para>
/// <para>To interact with the debugger, an instance of this class must be added to the editor via <see cref="Kaizen.EditorPlugin.AddDebuggerPlugin(EditorDebuggerPlugin)"/>.</para>
/// <para>Once added, the <see cref="Kaizen.EditorDebuggerPlugin._SetupSession(int)"/> callback will be called for every <see cref="Kaizen.EditorDebuggerSession"/> available to the plugin, and when new ones are created (the sessions may be inactive during this stage).</para>
/// <para>You can retrieve the available <see cref="Kaizen.EditorDebuggerSession"/>s via <see cref="Kaizen.EditorDebuggerPlugin.GetSessions()"/> or get a specific one via <see cref="Kaizen.EditorDebuggerPlugin.GetSession(int)"/>.</para>
/// <para></para>
/// <para>To connect on the running game side, use the <see cref="Kaizen.EngineDebugger"/> singleton:</para>
/// <para></para>
/// <para><b>Note:</b> While the game is running, <c>@GlobalScope.print</c> and similar functions <i>called in the editor</i> do not print anything, the Output Log prints only game messages.</para>
/// </summary>
public partial class EditorDebuggerPlugin : RefCounted
{
    private static readonly System.Type CachedType = typeof(EditorDebuggerPlugin);

    private static readonly StringName NativeName = "EditorDebuggerPlugin";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public EditorDebuggerPlugin() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal EditorDebuggerPlugin(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal EditorDebuggerPlugin(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Override this method to be notified when a breakpoint is set in the editor.</para>
    /// </summary>
    public virtual void _BreakpointSetInTree(Script script, int line, bool enabled)
    {
    }

    /// <summary>
    /// <para>Override this method to be notified when all breakpoints are cleared in the editor.</para>
    /// </summary>
    public virtual void _BreakpointsClearedInTree()
    {
    }

    /// <summary>
    /// <para>Override this method to process incoming messages. The <paramref name="sessionId"/> is the ID of the <see cref="Kaizen.EditorDebuggerSession"/> that received the <paramref name="message"/>. Use <see cref="Kaizen.EditorDebuggerPlugin.GetSession(int)"/> to retrieve the session. This method should return <see langword="true"/> if the message is recognized.</para>
    /// </summary>
    public virtual bool _Capture(string message, Kaizen.Collections.Array data, int sessionId)
    {
        return default;
    }

    /// <summary>
    /// <para>Override this method to be notified when a breakpoint line has been clicked in the debugger breakpoint panel.</para>
    /// </summary>
    public virtual void _GotoScriptLine(Script script, int line)
    {
    }

    /// <summary>
    /// <para>Override this method to enable receiving messages from the debugger. If <paramref name="capture"/> is "my_message" then messages starting with "my_message:" will be passed to the <see cref="Kaizen.EditorDebuggerPlugin._Capture(string, Kaizen.Collections.Array, int)"/> method.</para>
    /// </summary>
    public virtual bool _HasCapture(string capture)
    {
        return default;
    }

    /// <summary>
    /// <para>Override this method to be notified whenever a new <see cref="Kaizen.EditorDebuggerSession"/> is created. Note that the session may be inactive during this stage.</para>
    /// </summary>
    public virtual void _SetupSession(int sessionId)
    {
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSession, 3061968499ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.EditorDebuggerSession"/> with the given <paramref name="id"/>.</para>
    /// </summary>
    public EditorDebuggerSession GetSession(int id)
    {
        EditorDebuggerSession ret = (EditorDebuggerSession)NativeCalls.kaizen_icall_1_95(MethodBind0, KaizenObject.GetPtr(this), id);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSessions, 2915620761ul);

    /// <summary>
    /// <para>Returns an array of <see cref="Kaizen.EditorDebuggerSession"/> currently available to this debugger plugin.</para>
    /// <para><b>Note:</b> Sessions in the array may be inactive, check their state via <see cref="Kaizen.EditorDebuggerSession.IsActive()"/>.</para>
    /// </summary>
    public Kaizen.Collections.Array GetSessions()
    {
        Kaizen.Collections.Array ret = NativeCalls.kaizen_icall_0_146(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__breakpoint_set_in_tree = "_BreakpointSetInTree";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__breakpoints_cleared_in_tree = "_BreakpointsClearedInTree";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__capture = "_Capture";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__goto_script_line = "_GotoScriptLine";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__has_capture = "_HasCapture";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__setup_session = "_SetupSession";

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
        if ((method == MethodProxyName__breakpoint_set_in_tree || method == MethodName._BreakpointSetInTree) && args.Count == 3 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__breakpoint_set_in_tree.NativeValue))
        {
            _BreakpointSetInTree(VariantUtils.ConvertTo<Script>(args[0]), VariantUtils.ConvertTo<int>(args[1]), VariantUtils.ConvertTo<bool>(args[2]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__breakpoints_cleared_in_tree || method == MethodName._BreakpointsClearedInTree) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__breakpoints_cleared_in_tree.NativeValue))
        {
            _BreakpointsClearedInTree();
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__capture || method == MethodName._Capture) && args.Count == 3 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__capture.NativeValue))
        {
            var callRet = _Capture(VariantUtils.ConvertTo<string>(args[0]), VariantUtils.ConvertTo<Kaizen.Collections.Array>(args[1]), VariantUtils.ConvertTo<int>(args[2]));
            ret = VariantUtils.CreateFrom<bool>(callRet);
            return true;
        }
        if ((method == MethodProxyName__goto_script_line || method == MethodName._GotoScriptLine) && args.Count == 2 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__goto_script_line.NativeValue))
        {
            _GotoScriptLine(VariantUtils.ConvertTo<Script>(args[0]), VariantUtils.ConvertTo<int>(args[1]));
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__has_capture || method == MethodName._HasCapture) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__has_capture.NativeValue))
        {
            var callRet = _HasCapture(VariantUtils.ConvertTo<string>(args[0]));
            ret = VariantUtils.CreateFrom<bool>(callRet);
            return true;
        }
        if ((method == MethodProxyName__setup_session || method == MethodName._SetupSession) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__setup_session.NativeValue))
        {
            _SetupSession(VariantUtils.ConvertTo<int>(args[0]));
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
        if (method == MethodName._BreakpointSetInTree)
        {
            if (HasKaizenClassMethod(MethodProxyName__breakpoint_set_in_tree.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._BreakpointsClearedInTree)
        {
            if (HasKaizenClassMethod(MethodProxyName__breakpoints_cleared_in_tree.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._Capture)
        {
            if (HasKaizenClassMethod(MethodProxyName__capture.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GotoScriptLine)
        {
            if (HasKaizenClassMethod(MethodProxyName__goto_script_line.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._HasCapture)
        {
            if (HasKaizenClassMethod(MethodProxyName__has_capture.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._SetupSession)
        {
            if (HasKaizenClassMethod(MethodProxyName__setup_session.NativeValue.DangerousSelfRef))
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
    public new class PropertyName : RefCounted.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the '_breakpoint_set_in_tree' method.
        /// </summary>
        public static readonly StringName _BreakpointSetInTree = "_breakpoint_set_in_tree";
        /// <summary>
        /// Cached name for the '_breakpoints_cleared_in_tree' method.
        /// </summary>
        public static readonly StringName _BreakpointsClearedInTree = "_breakpoints_cleared_in_tree";
        /// <summary>
        /// Cached name for the '_capture' method.
        /// </summary>
        public static readonly StringName _Capture = "_capture";
        /// <summary>
        /// Cached name for the '_goto_script_line' method.
        /// </summary>
        public static readonly StringName _GotoScriptLine = "_goto_script_line";
        /// <summary>
        /// Cached name for the '_has_capture' method.
        /// </summary>
        public static readonly StringName _HasCapture = "_has_capture";
        /// <summary>
        /// Cached name for the '_setup_session' method.
        /// </summary>
        public static readonly StringName _SetupSession = "_setup_session";
        /// <summary>
        /// Cached name for the 'get_session' method.
        /// </summary>
        public static readonly StringName GetSession = "get_session";
        /// <summary>
        /// Cached name for the 'get_sessions' method.
        /// </summary>
        public static readonly StringName GetSessions = "get_sessions";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
