namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
public partial class ScriptLanguage : KaizenObject
{
    public enum ScriptNameCasing : long
    {
        /// <summary>
        /// <para>Used by <c>ProjectSettings.editor/naming/script_name_casing</c> to set the default script name casing to the script language's preferred casing.</para>
        /// </summary>
        Auto = 0,
        /// <summary>
        /// <para>Used by <c>ProjectSettings.editor/naming/script_name_casing</c> to set the default script name casing to <c>PascalCase</c>.</para>
        /// </summary>
        PascalCase = 1,
        /// <summary>
        /// <para>Used by <c>ProjectSettings.editor/naming/script_name_casing</c> to set the default script name casing to <c>snake_case</c>.</para>
        /// </summary>
        SnakeCase = 2,
        /// <summary>
        /// <para>Used by <c>ProjectSettings.editor/naming/script_name_casing</c> to set the default script name casing to <c>kebab-case</c>.</para>
        /// </summary>
        KebabCase = 3,
        /// <summary>
        /// <para>Used by <c>ProjectSettings.editor/naming/script_name_casing</c> to set the default script name casing to <c>camelCase</c>.</para>
        /// </summary>
        CamelCase = 4
    }

    private static readonly System.Type CachedType = typeof(ScriptLanguage);

    private static readonly StringName NativeName = "ScriptLanguage";

    internal ScriptLanguage() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal ScriptLanguage(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal ScriptLanguage(bool memoryOwn) : base(memoryOwn) { }

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
    public new class PropertyName : KaizenObject.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : KaizenObject.MethodName
    {
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : KaizenObject.SignalName
    {
    }
}
