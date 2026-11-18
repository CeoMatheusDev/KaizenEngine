namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Compares <c>a</c> and <c>b</c> of <see cref="Kaizen.VisualShaderNodeCompare.Type"/> by <see cref="Kaizen.VisualShaderNodeCompare.Function"/>. Returns a boolean scalar. Translates to <c>if</c> instruction in shader code.</para>
/// </summary>
public partial class VisualShaderNodeCompare : VisualShaderNode
{
    public enum ComparisonType : long
    {
        /// <summary>
        /// <para>A floating-point scalar.</para>
        /// </summary>
        Scalar = 0,
        /// <summary>
        /// <para>An integer scalar.</para>
        /// </summary>
        ScalarInt = 1,
        /// <summary>
        /// <para>An unsigned integer scalar.</para>
        /// </summary>
        ScalarUint = 2,
        /// <summary>
        /// <para>A 2D vector type.</para>
        /// </summary>
        Vector2D = 3,
        /// <summary>
        /// <para>A 3D vector type.</para>
        /// </summary>
        Vector3D = 4,
        /// <summary>
        /// <para>A 4D vector type.</para>
        /// </summary>
        Vector4D = 5,
        /// <summary>
        /// <para>A boolean type.</para>
        /// </summary>
        Boolean = 6,
        /// <summary>
        /// <para>A transform (<c>mat4</c>) type.</para>
        /// </summary>
        Transform = 7,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType"/> enum.</para>
        /// </summary>
        Max = 8
    }

    public enum FunctionEnum : long
    {
        /// <summary>
        /// <para>Comparison for equality (<c>a == b</c>).</para>
        /// </summary>
        Equal = 0,
        /// <summary>
        /// <para>Comparison for inequality (<c>a != b</c>).</para>
        /// </summary>
        NotEqual = 1,
        /// <summary>
        /// <para>Comparison for greater than (<c>a &gt; b</c>). Cannot be used if <see cref="Kaizen.VisualShaderNodeCompare.Type"/> set to <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Boolean"/> or <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Transform"/>.</para>
        /// </summary>
        GreaterThan = 2,
        /// <summary>
        /// <para>Comparison for greater than or equal (<c>a &gt;= b</c>). Cannot be used if <see cref="Kaizen.VisualShaderNodeCompare.Type"/> set to <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Boolean"/> or <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Transform"/>.</para>
        /// </summary>
        GreaterThanEqual = 3,
        /// <summary>
        /// <para>Comparison for less than (<c>a &lt; b</c>). Cannot be used if <see cref="Kaizen.VisualShaderNodeCompare.Type"/> set to <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Boolean"/> or <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Transform"/>.</para>
        /// </summary>
        LessThan = 4,
        /// <summary>
        /// <para>Comparison for less than or equal (<c>a &lt;= b</c>). Cannot be used if <see cref="Kaizen.VisualShaderNodeCompare.Type"/> set to <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Boolean"/> or <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Transform"/>.</para>
        /// </summary>
        LessThanEqual = 5,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.VisualShaderNodeCompare.FunctionEnum"/> enum.</para>
        /// </summary>
        Max = 6
    }

    public enum ConditionEnum : long
    {
        /// <summary>
        /// <para>The result will be <see langword="true"/> if all components in the vector satisfy the comparison condition.</para>
        /// </summary>
        All = 0,
        /// <summary>
        /// <para>The result will be <see langword="true"/> if any component in the vector satisfies the comparison condition.</para>
        /// </summary>
        Any = 1,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.VisualShaderNodeCompare.ConditionEnum"/> enum.</para>
        /// </summary>
        Max = 2
    }

    /// <summary>
    /// <para>The type to be used in the comparison.</para>
    /// </summary>
    public VisualShaderNodeCompare.ComparisonType Type
    {
        get
        {
            return GetComparisonType();
        }
        set
        {
            SetComparisonType(value);
        }
    }

    /// <summary>
    /// <para>A comparison function.</para>
    /// </summary>
    public VisualShaderNodeCompare.FunctionEnum Function
    {
        get
        {
            return GetFunction();
        }
        set
        {
            SetFunction(value);
        }
    }

    /// <summary>
    /// <para>Extra condition which is applied if <see cref="Kaizen.VisualShaderNodeCompare.Type"/> is set to <see cref="Kaizen.VisualShaderNodeCompare.ComparisonType.Vector3D"/>.</para>
    /// </summary>
    public VisualShaderNodeCompare.ConditionEnum Condition
    {
        get
        {
            return GetCondition();
        }
        set
        {
            SetCondition(value);
        }
    }

    private static readonly System.Type CachedType = typeof(VisualShaderNodeCompare);

    private static readonly StringName NativeName = "VisualShaderNodeCompare";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public VisualShaderNodeCompare() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal VisualShaderNodeCompare(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal VisualShaderNodeCompare(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetComparisonType, 516558320ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetComparisonType(VisualShaderNodeCompare.ComparisonType type)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), (int)type);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetComparisonType, 3495315961ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public VisualShaderNodeCompare.ComparisonType GetComparisonType()
    {
        VisualShaderNodeCompare.ComparisonType ret = (VisualShaderNodeCompare.ComparisonType)NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFunction, 2370951349ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFunction(VisualShaderNodeCompare.FunctionEnum func)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)func);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFunction, 4089164265ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public VisualShaderNodeCompare.FunctionEnum GetFunction()
    {
        VisualShaderNodeCompare.FunctionEnum ret = (VisualShaderNodeCompare.FunctionEnum)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCondition, 918742392ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCondition(VisualShaderNodeCompare.ConditionEnum condition)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind4, KaizenObject.GetPtr(this), (int)condition);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCondition, 3281078941ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public VisualShaderNodeCompare.ConditionEnum GetCondition()
    {
        VisualShaderNodeCompare.ConditionEnum ret = (VisualShaderNodeCompare.ConditionEnum)NativeCalls.kaizen_icall_0_39(MethodBind5, KaizenObject.GetPtr(this));
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
    public new class PropertyName : VisualShaderNode.PropertyName
    {
        /// <summary>
        /// Cached name for the 'type' property.
        /// </summary>
        public static readonly StringName Type = "type";
        /// <summary>
        /// Cached name for the 'function' property.
        /// </summary>
        public static readonly StringName Function = "function";
        /// <summary>
        /// Cached name for the 'condition' property.
        /// </summary>
        public static readonly StringName Condition = "condition";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : VisualShaderNode.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_comparison_type' method.
        /// </summary>
        public static readonly StringName SetComparisonType = "set_comparison_type";
        /// <summary>
        /// Cached name for the 'get_comparison_type' method.
        /// </summary>
        public static readonly StringName GetComparisonType = "get_comparison_type";
        /// <summary>
        /// Cached name for the 'set_function' method.
        /// </summary>
        public static readonly StringName SetFunction = "set_function";
        /// <summary>
        /// Cached name for the 'get_function' method.
        /// </summary>
        public static readonly StringName GetFunction = "get_function";
        /// <summary>
        /// Cached name for the 'set_condition' method.
        /// </summary>
        public static readonly StringName SetCondition = "set_condition";
        /// <summary>
        /// Cached name for the 'get_condition' method.
        /// </summary>
        public static readonly StringName GetCondition = "get_condition";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : VisualShaderNode.SignalName
    {
    }
}
