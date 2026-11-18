namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This is the default binding modifier editor used in the OpenXR action map.</para>
/// </summary>
public partial class OpenXRBindingModifierEditor : PanelContainer
{
    private static readonly System.Type CachedType = typeof(OpenXRBindingModifierEditor);

    private static readonly StringName NativeName = "OpenXRBindingModifierEditor";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public OpenXRBindingModifierEditor() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: false);
        }
    }

    internal OpenXRBindingModifierEditor(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal OpenXRBindingModifierEditor(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBindingModifier, 2930765082ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.OpenXRBindingModifier"/> currently being edited.</para>
    /// </summary>
    public OpenXRBindingModifier GetBindingModifier()
    {
        OpenXRBindingModifier ret = (OpenXRBindingModifier)NativeCalls.kaizen_icall_0_87(MethodBind0, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.Setup, 1284787389ul);

    /// <summary>
    /// <para>Setup this editor for the provided <paramref name="actionMap"/> and <paramref name="bindingModifier"/>.</para>
    /// </summary>
    public void Setup(OpenXRActionMap actionMap, OpenXRBindingModifier bindingModifier)
    {
        NativeCalls.kaizen_icall_2_268(MethodBind1, KaizenObject.GetPtr(this), KaizenObject.GetPtr(actionMap), KaizenObject.GetPtr(bindingModifier));
        global::System.GC.KeepAlive(this);
    }

    /// <summary>
    /// Represents the method that handles the <see cref="Kaizen.OpenXRBindingModifierEditor.BindingModifierRemoved"/> event of a <see cref="Kaizen.OpenXRBindingModifierEditor"/> class.
    /// </summary>
    public delegate void BindingModifierRemovedEventHandler(KaizenObject bindingModifierEditor);

    private static void BindingModifierRemovedTrampoline(object delegateObj, NativeVariantPtrArgs args, out kaizen_variant ret)
    {
        Callable.ThrowIfArgCountMismatch(args, 1);
        ((BindingModifierRemovedEventHandler)delegateObj)(VariantUtils.ConvertTo<KaizenObject>(args[0]));
        ret = default;
    }

    /// <summary>
    /// <para>Signal emitted when the user presses the delete binding modifier button for this modifier.</para>
    /// </summary>
    public unsafe event BindingModifierRemovedEventHandler BindingModifierRemoved
    {
        add => Connect(SignalName.BindingModifierRemoved, Callable.CreateWithUnsafeTrampoline(value, &BindingModifierRemovedTrampoline));
        remove => Disconnect(SignalName.BindingModifierRemoved, Callable.CreateWithUnsafeTrampoline(value, &BindingModifierRemovedTrampoline));
    }

    protected void EmitSignalBindingModifierRemoved(KaizenObject bindingModifierEditor)
    {
        EmitSignal(SignalName.BindingModifierRemoved, bindingModifierEditor);
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName SignalProxyName_binding_modifier_removed = "BindingModifierRemoved";

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
        if (signal == SignalName.BindingModifierRemoved)
        {
            if (HasKaizenClassSignal(SignalProxyName_binding_modifier_removed.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        return base.HasKaizenClassSignal(signal);
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public new class PropertyName : PanelContainer.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : PanelContainer.MethodName
    {
        /// <summary>
        /// Cached name for the 'get_binding_modifier' method.
        /// </summary>
        public static readonly StringName GetBindingModifier = "get_binding_modifier";
        /// <summary>
        /// Cached name for the 'setup' method.
        /// </summary>
        public static readonly StringName Setup = "setup";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : PanelContainer.SignalName
    {
        /// <summary>
        /// Cached name for the 'binding_modifier_removed' signal.
        /// </summary>
        public static readonly StringName BindingModifierRemoved = "binding_modifier_removed";
    }
}
