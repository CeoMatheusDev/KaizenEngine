namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
public partial class LineBase3D : GeometryInstance3D
{
    public enum MeshAlignmentEnum : long
    {
        /// <summary>
        /// <para>Align the line normal to the local Y axis of the line node.</para>
        /// </summary>
        Local = 0,
        /// <summary>
        /// <para>Align the normal of each section of the LineBase3D to face the camera.</para>
        /// <para>See also <see cref="Kaizen.SpriteBase3D.Billboard"/> and <see cref="Kaizen.BaseMaterial3D.BillboardModeEnum"/></para>
        /// </summary>
        Billboard = 1,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.LineBase3D.MeshAlignmentEnum"/> enum.</para>
        /// </summary>
        Max = 2
    }

    public enum TilingModeEnum : long
    {
        /// <summary>
        /// <para>Assign UVs from <c>0</c> to <c>1</c> across the length of the line.</para>
        /// </summary>
        ModeStretch = 0,
        /// <summary>
        /// <para>Assign UVs from <c>0</c> to current length, across the length of the line.</para>
        /// </summary>
        ModeTile = 1,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.LineBase3D.TilingModeEnum"/> enum.</para>
        /// </summary>
        Max = 2
    }

    public enum MaterialModeEnum : long
    {
        /// <summary>
        /// <para>Assign a built-in mix material to the line.</para>
        /// </summary>
        Mix = 0,
        /// <summary>
        /// <para>Assign a built-in additive material to the line.</para>
        /// </summary>
        Add = 1,
        /// <summary>
        /// <para>Assign a custom material to the line.</para>
        /// </summary>
        Custom = 2,
        /// <summary>
        /// <para>Represents the size of the <see cref="Kaizen.LineBase3D.MaterialModeEnum"/> enum.</para>
        /// </summary>
        Max = 3
    }

    private static readonly System.Type CachedType = typeof(LineBase3D);

    private static readonly StringName NativeName = "LineBase3D";

    internal LineBase3D() : this(false)
    {
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal LineBase3D(IntPtr ptr) : this(false)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: false);
        }
    }

    internal LineBase3D(bool memoryOwn) : base(memoryOwn) { }

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
    public new class PropertyName : GeometryInstance3D.PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : GeometryInstance3D.MethodName
    {
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : GeometryInstance3D.SignalName
    {
    }
}
