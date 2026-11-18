namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>OpenXR uses an action system similar to Kaizens Input map system to bind inputs and outputs on various types of XR controllers to named actions. OpenXR specifies more detail on these inputs and outputs than Kaizen supports.</para>
/// <para>Another important distinction is that OpenXR offers no control over these bindings. The bindings we register are suggestions, it is up to the XR runtime to offer users the ability to change these bindings. This allows the XR runtime to fill in the gaps if new hardware becomes available.</para>
/// <para>The action map therefore needs to be loaded at startup and can't be changed afterwards. This resource is a container for the entire action map.</para>
/// </summary>
public partial class OpenXRActionMap : Resource
{
    /// <summary>
    /// <para>Collection of <see cref="Kaizen.OpenXRActionSet"/>s that are part of this action map.</para>
    /// </summary>
    public Kaizen.Collections.Array ActionSets
    {
        get
        {
            return GetActionSets();
        }
        set
        {
            SetActionSets(value);
        }
    }

    /// <summary>
    /// <para>Collection of <see cref="Kaizen.OpenXRInteractionProfile"/>s that are part of this action map.</para>
    /// </summary>
    public Kaizen.Collections.Array InteractionProfiles
    {
        get
        {
            return GetInteractionProfiles();
        }
        set
        {
            SetInteractionProfiles(value);
        }
    }

    private static readonly System.Type CachedType = typeof(OpenXRActionMap);

    private static readonly StringName NativeName = "OpenXRActionMap";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public OpenXRActionMap() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal OpenXRActionMap(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal OpenXRActionMap(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetActionSets, 381264803ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetActionSets(Kaizen.Collections.Array actionSets)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind0, KaizenObject.GetPtr(this), (kaizen_array)(actionSets ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActionSets, 3995934104ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Array GetActionSets()
    {
        Kaizen.Collections.Array ret = NativeCalls.kaizen_icall_0_146(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActionSetCount, 3905245786ul);

    /// <summary>
    /// <para>Retrieve the number of actions sets in our action map.</para>
    /// </summary>
    public int GetActionSetCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.FindActionSet, 1888809267ul);

    /// <summary>
    /// <para>Retrieve an action set by name.</para>
    /// </summary>
    public OpenXRActionSet FindActionSet(string name)
    {
        OpenXRActionSet ret = (OpenXRActionSet)NativeCalls.kaizen_icall_1_551(MethodBind3, KaizenObject.GetPtr(this), name);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetActionSet, 1789580336ul);

    /// <summary>
    /// <para>Retrieve the action set at this index.</para>
    /// </summary>
    public OpenXRActionSet GetActionSet(int idx)
    {
        OpenXRActionSet ret = (OpenXRActionSet)NativeCalls.kaizen_icall_1_95(MethodBind4, KaizenObject.GetPtr(this), idx);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddActionSet, 2093310581ul);

    /// <summary>
    /// <para>Add an action set.</para>
    /// </summary>
    public void AddActionSet(OpenXRActionSet actionSet)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind5, KaizenObject.GetPtr(this), KaizenObject.GetPtr(actionSet));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RemoveActionSet, 2093310581ul);

    /// <summary>
    /// <para>Remove an action set.</para>
    /// </summary>
    public void RemoveActionSet(OpenXRActionSet actionSet)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind6, KaizenObject.GetPtr(this), KaizenObject.GetPtr(actionSet));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetInteractionProfiles, 381264803ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetInteractionProfiles(Kaizen.Collections.Array interactionProfiles)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind7, KaizenObject.GetPtr(this), (kaizen_array)(interactionProfiles ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetInteractionProfiles, 3995934104ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Array GetInteractionProfiles()
    {
        Kaizen.Collections.Array ret = NativeCalls.kaizen_icall_0_146(MethodBind8, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetInteractionProfileCount, 3905245786ul);

    /// <summary>
    /// <para>Retrieve the number of interaction profiles in our action map.</para>
    /// </summary>
    public int GetInteractionProfileCount()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.FindInteractionProfile, 3095875538ul);

    /// <summary>
    /// <para>Find an interaction profile by its name (path).</para>
    /// </summary>
    public OpenXRInteractionProfile FindInteractionProfile(string name)
    {
        OpenXRInteractionProfile ret = (OpenXRInteractionProfile)NativeCalls.kaizen_icall_1_551(MethodBind10, KaizenObject.GetPtr(this), name);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetInteractionProfile, 2546151210ul);

    /// <summary>
    /// <para>Get the interaction profile at this index.</para>
    /// </summary>
    public OpenXRInteractionProfile GetInteractionProfile(int idx)
    {
        OpenXRInteractionProfile ret = (OpenXRInteractionProfile)NativeCalls.kaizen_icall_1_95(MethodBind11, KaizenObject.GetPtr(this), idx);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddInteractionProfile, 2697953512ul);

    /// <summary>
    /// <para>Add an interaction profile.</para>
    /// </summary>
    public void AddInteractionProfile(OpenXRInteractionProfile interactionProfile)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind12, KaizenObject.GetPtr(this), KaizenObject.GetPtr(interactionProfile));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RemoveInteractionProfile, 2697953512ul);

    /// <summary>
    /// <para>Remove an interaction profile.</para>
    /// </summary>
    public void RemoveInteractionProfile(OpenXRInteractionProfile interactionProfile)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind13, KaizenObject.GetPtr(this), KaizenObject.GetPtr(interactionProfile));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.CreateDefaultActionSets, 3218959716ul);

    /// <summary>
    /// <para>Setup this action set with our default actions.</para>
    /// </summary>
    public void CreateDefaultActionSets()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind14, KaizenObject.GetPtr(this));
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
    public new class PropertyName : Resource.PropertyName
    {
        /// <summary>
        /// Cached name for the 'action_sets' property.
        /// </summary>
        public static readonly StringName ActionSets = "action_sets";
        /// <summary>
        /// Cached name for the 'interaction_profiles' property.
        /// </summary>
        public static readonly StringName InteractionProfiles = "interaction_profiles";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_action_sets' method.
        /// </summary>
        public static readonly StringName SetActionSets = "set_action_sets";
        /// <summary>
        /// Cached name for the 'get_action_sets' method.
        /// </summary>
        public static readonly StringName GetActionSets = "get_action_sets";
        /// <summary>
        /// Cached name for the 'get_action_set_count' method.
        /// </summary>
        public static readonly StringName GetActionSetCount = "get_action_set_count";
        /// <summary>
        /// Cached name for the 'find_action_set' method.
        /// </summary>
        public static readonly StringName FindActionSet = "find_action_set";
        /// <summary>
        /// Cached name for the 'get_action_set' method.
        /// </summary>
        public static readonly StringName GetActionSet = "get_action_set";
        /// <summary>
        /// Cached name for the 'add_action_set' method.
        /// </summary>
        public static readonly StringName AddActionSet = "add_action_set";
        /// <summary>
        /// Cached name for the 'remove_action_set' method.
        /// </summary>
        public static readonly StringName RemoveActionSet = "remove_action_set";
        /// <summary>
        /// Cached name for the 'set_interaction_profiles' method.
        /// </summary>
        public static readonly StringName SetInteractionProfiles = "set_interaction_profiles";
        /// <summary>
        /// Cached name for the 'get_interaction_profiles' method.
        /// </summary>
        public static readonly StringName GetInteractionProfiles = "get_interaction_profiles";
        /// <summary>
        /// Cached name for the 'get_interaction_profile_count' method.
        /// </summary>
        public static readonly StringName GetInteractionProfileCount = "get_interaction_profile_count";
        /// <summary>
        /// Cached name for the 'find_interaction_profile' method.
        /// </summary>
        public static readonly StringName FindInteractionProfile = "find_interaction_profile";
        /// <summary>
        /// Cached name for the 'get_interaction_profile' method.
        /// </summary>
        public static readonly StringName GetInteractionProfile = "get_interaction_profile";
        /// <summary>
        /// Cached name for the 'add_interaction_profile' method.
        /// </summary>
        public static readonly StringName AddInteractionProfile = "add_interaction_profile";
        /// <summary>
        /// Cached name for the 'remove_interaction_profile' method.
        /// </summary>
        public static readonly StringName RemoveInteractionProfile = "remove_interaction_profile";
        /// <summary>
        /// Cached name for the 'create_default_action_sets' method.
        /// </summary>
        public static readonly StringName CreateDefaultActionSets = "create_default_action_sets";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
