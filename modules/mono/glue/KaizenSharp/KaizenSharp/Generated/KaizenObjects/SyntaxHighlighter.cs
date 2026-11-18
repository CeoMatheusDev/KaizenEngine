namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Base class for syntax highlighters. Provides syntax highlighting data to a <see cref="Kaizen.TextEdit"/>. The associated <see cref="Kaizen.TextEdit"/> will call into the <see cref="Kaizen.SyntaxHighlighter"/> on an as-needed basis.</para>
/// <para><b>Note:</b> A <see cref="Kaizen.SyntaxHighlighter"/> instance should not be used across multiple <see cref="Kaizen.TextEdit"/> nodes.</para>
/// </summary>
public partial class SyntaxHighlighter : Resource
{
    private static readonly System.Type CachedType = typeof(SyntaxHighlighter);

    private static readonly StringName NativeName = "SyntaxHighlighter";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public SyntaxHighlighter() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal SyntaxHighlighter(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal SyntaxHighlighter(bool memoryOwn) : base(memoryOwn) { }

    /// <summary>
    /// <para>Virtual method which can be overridden to clear any local caches.</para>
    /// </summary>
    public virtual void _ClearHighlightingCache()
    {
    }

    /// <summary>
    /// <para>Virtual method which can be overridden to return syntax highlighting data.</para>
    /// <para>See <see cref="Kaizen.SyntaxHighlighter.GetLineSyntaxHighlighting(int)"/> for more details.</para>
    /// </summary>
    public virtual Kaizen.Collections.Dictionary _GetLineSyntaxHighlighting(int line)
    {
        return default;
    }

    /// <summary>
    /// <para>Virtual method which can be overridden to update any local caches.</para>
    /// </summary>
    public virtual void _UpdateCache()
    {
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLineSyntaxHighlighting, 3554694381ul);

    /// <summary>
    /// <para>Returns the syntax highlighting data for the line at index <paramref name="line"/>. If the line is not cached, calls <see cref="Kaizen.SyntaxHighlighter._GetLineSyntaxHighlighting(int)"/> first to calculate the data.</para>
    /// <para>Each entry is a column number containing a nested <see cref="Kaizen.Collections.Dictionary"/>. The column number denotes the start of a region, the region will end if another region is found, or at the end of the line. The nested <see cref="Kaizen.Collections.Dictionary"/> contains the data for that region. Currently only the key <c>"color"</c> is supported.</para>
    /// <para><b>Example:</b> Possible return value. This means columns <c>0</c> to <c>4</c> should be red, and columns <c>5</c> to the end of the line should be green:</para>
    /// <para><code>
    /// {
    /// 	0: {
    /// 		"color": Color(1, 0, 0)
    /// 	},
    /// 	5: {
    /// 		"color": Color(0, 1, 0)
    /// 	}
    /// }
    /// </code></para>
    /// <para><b>Example:</b> Sample implementation. Colors the word <c>Kaizen</c> blue.</para>
    /// <para><code>
    /// public override Dictionary _GetLineSyntaxHighlighting(int line)
    /// {
    /// 	string patternText = "Kaizen";
    /// 	Dictionary result = new Dictionary();
    /// 	TextEdit textEdit = GetTextEdit();
    /// 	string lineText = textEdit.GetLine(line);
    /// 
    /// 	var pos = lineText.Find(patternText);
    /// 	while (pos != -1)
    /// 	{
    /// 		result[pos] = new Dictionary()
    /// 		{
    /// 			{ "color", new Color(0.0f, 0.0f, 1.0f) }
    /// 		};
    /// 		var endPos = pos + patternText.Length;
    /// 		pos = lineText.Find(patternText, endPos);
    /// 		if (pos != endPos)
    /// 		{
    /// 			result[endPos] = new Dictionary()
    /// 			{
    /// 				{ "color", new Color(1.0f, 1.0f, 1.0f) }
    /// 			};
    /// 		}
    /// 	}
    /// 
    /// 	return result;
    /// }
    /// </code></para>
    /// </summary>
    public Kaizen.Collections.Dictionary GetLineSyntaxHighlighting(int line)
    {
        Kaizen.Collections.Dictionary ret = NativeCalls.kaizen_icall_1_335(MethodBind0, KaizenObject.GetPtr(this), line);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UpdateCache, 3218959716ul);

    /// <summary>
    /// <para>Clears then updates the <see cref="Kaizen.SyntaxHighlighter"/> caches. Override <see cref="Kaizen.SyntaxHighlighter._UpdateCache()"/> for a callback.</para>
    /// <para><b>Note:</b> This is called automatically when the associated <see cref="Kaizen.TextEdit"/> node, updates its own cache.</para>
    /// </summary>
    public void UpdateCache()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ClearHighlightingCache, 3218959716ul);

    /// <summary>
    /// <para>Clears all cached syntax highlighting data.</para>
    /// <para>Then calls overridable method <see cref="Kaizen.SyntaxHighlighter._ClearHighlightingCache()"/>.</para>
    /// </summary>
    public void ClearHighlightingCache()
    {
        NativeCalls.kaizen_icall_0_3(MethodBind2, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextEdit, 1893027089ul);

    /// <summary>
    /// <para>Returns the associated <see cref="Kaizen.TextEdit"/> node.</para>
    /// </summary>
    public TextEdit GetTextEdit()
    {
        TextEdit ret = (TextEdit)NativeCalls.kaizen_icall_0_53(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__clear_highlighting_cache = "_ClearHighlightingCache";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__get_line_syntax_highlighting = "_GetLineSyntaxHighlighting";

    // ReSharper disable once InconsistentNaming
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly StringName MethodProxyName__update_cache = "_UpdateCache";

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
        if ((method == MethodProxyName__clear_highlighting_cache || method == MethodName._ClearHighlightingCache) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__clear_highlighting_cache.NativeValue))
        {
            _ClearHighlightingCache();
            ret = default;
            return true;
        }
        if ((method == MethodProxyName__get_line_syntax_highlighting || method == MethodName._GetLineSyntaxHighlighting) && args.Count == 1 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__get_line_syntax_highlighting.NativeValue))
        {
            var callRet = _GetLineSyntaxHighlighting(VariantUtils.ConvertTo<int>(args[0]));
            ret = VariantUtils.CreateFrom<Kaizen.Collections.Dictionary>(callRet);
            return true;
        }
        if ((method == MethodProxyName__update_cache || method == MethodName._UpdateCache) && args.Count == 0 && HasKaizenClassMethod((kaizen_string_name)MethodProxyName__update_cache.NativeValue))
        {
            _UpdateCache();
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
        if (method == MethodName._ClearHighlightingCache)
        {
            if (HasKaizenClassMethod(MethodProxyName__clear_highlighting_cache.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._GetLineSyntaxHighlighting)
        {
            if (HasKaizenClassMethod(MethodProxyName__get_line_syntax_highlighting.NativeValue.DangerousSelfRef))
            {
                return true;
            }
        }
        if (method == MethodName._UpdateCache)
        {
            if (HasKaizenClassMethod(MethodProxyName__update_cache.NativeValue.DangerousSelfRef))
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
    public new class PropertyName : Resource.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the '_clear_highlighting_cache' method.
        /// </summary>
        public static readonly StringName _ClearHighlightingCache = "_clear_highlighting_cache";
        /// <summary>
        /// Cached name for the '_get_line_syntax_highlighting' method.
        /// </summary>
        public static readonly StringName _GetLineSyntaxHighlighting = "_get_line_syntax_highlighting";
        /// <summary>
        /// Cached name for the '_update_cache' method.
        /// </summary>
        public static readonly StringName _UpdateCache = "_update_cache";
        /// <summary>
        /// Cached name for the 'get_line_syntax_highlighting' method.
        /// </summary>
        public static readonly StringName GetLineSyntaxHighlighting = "get_line_syntax_highlighting";
        /// <summary>
        /// Cached name for the 'update_cache' method.
        /// </summary>
        public static readonly StringName UpdateCache = "update_cache";
        /// <summary>
        /// Cached name for the 'clear_highlighting_cache' method.
        /// </summary>
        public static readonly StringName ClearHighlightingCache = "clear_highlighting_cache";
        /// <summary>
        /// Cached name for the 'get_text_edit' method.
        /// </summary>
        public static readonly StringName GetTextEdit = "get_text_edit";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
