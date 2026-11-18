namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.TubeTrailMesh"/> represents a straight tube-shaped mesh with variable width. The tube is composed of a number of cylindrical sections, each with the same <see cref="Kaizen.TubeTrailMesh.SectionLength"/> and number of <see cref="Kaizen.TubeTrailMesh.SectionRings"/>. A <see cref="Kaizen.TubeTrailMesh.Curve"/> is sampled along the total length of the tube, meaning that the curve determines the radius of the tube along its length.</para>
/// <para>This primitive mesh is usually used for particle trails.</para>
/// </summary>
public partial class TubeTrailMesh : PrimitiveMesh
{
    /// <summary>
    /// <para>The baseline radius of the tube. The radius of a particular section ring is obtained by multiplying this radius by the value of the <see cref="Kaizen.TubeTrailMesh.Curve"/> at the given distance.</para>
    /// </summary>
    public float Radius
    {
        get
        {
            return GetRadius();
        }
        set
        {
            SetRadius(value);
        }
    }

    /// <summary>
    /// <para>The number of sides on the tube. For example, a value of <c>5</c> means the tube will be pentagonal. Higher values result in a more detailed tube at the cost of performance.</para>
    /// </summary>
    public int RadialSteps
    {
        get
        {
            return GetRadialSteps();
        }
        set
        {
            SetRadialSteps(value);
        }
    }

    /// <summary>
    /// <para>The total number of sections on the tube.</para>
    /// </summary>
    public int Sections
    {
        get
        {
            return GetSections();
        }
        set
        {
            SetSections(value);
        }
    }

    /// <summary>
    /// <para>The length of a section of the tube.</para>
    /// </summary>
    public float SectionLength
    {
        get
        {
            return GetSectionLength();
        }
        set
        {
            SetSectionLength(value);
        }
    }

    /// <summary>
    /// <para>The number of rings in a section. The <see cref="Kaizen.TubeTrailMesh.Curve"/> is sampled on each ring to determine its radius. Higher values result in a more detailed tube at the cost of performance.</para>
    /// </summary>
    public int SectionRings
    {
        get
        {
            return GetSectionRings();
        }
        set
        {
            SetSectionRings(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, generates a cap at the top of the tube. This can be set to <see langword="false"/> to speed up generation and rendering when the cap is never seen by the camera.</para>
    /// </summary>
    public bool CapTop
    {
        get
        {
            return IsCapTop();
        }
        set
        {
            SetCapTop(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, generates a cap at the bottom of the tube. This can be set to <see langword="false"/> to speed up generation and rendering when the cap is never seen by the camera.</para>
    /// </summary>
    public bool CapBottom
    {
        get
        {
            return IsCapBottom();
        }
        set
        {
            SetCapBottom(value);
        }
    }

    /// <summary>
    /// <para>Determines the radius of the tube along its length. The radius of a particular section ring is obtained by multiplying the baseline <see cref="Kaizen.TubeTrailMesh.Radius"/> by the value of this curve at the given distance. For values smaller than <c>0</c>, the faces will be inverted. Should be a unit <see cref="Kaizen.Curve"/>.</para>
    /// </summary>
    public Curve Curve
    {
        get
        {
            return GetCurve();
        }
        set
        {
            SetCurve(value);
        }
    }

    private static readonly System.Type CachedType = typeof(TubeTrailMesh);

    private static readonly StringName NativeName = "TubeTrailMesh";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public TubeTrailMesh() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal TubeTrailMesh(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal TubeTrailMesh(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRadius, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRadius(float radius)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind0, KaizenObject.GetPtr(this), radius);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRadius, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetRadius()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRadialSteps, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRadialSteps(int radialSteps)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), radialSteps);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRadialSteps, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetRadialSteps()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSections, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSections(int sections)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind4, KaizenObject.GetPtr(this), sections);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSections, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetSections()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSectionLength, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSectionLength(float sectionLength)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind6, KaizenObject.GetPtr(this), sectionLength);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSectionLength, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetSectionLength()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSectionRings, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSectionRings(int sectionRings)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind8, KaizenObject.GetPtr(this), sectionRings);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSectionRings, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetSectionRings()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCapTop, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCapTop(bool capTop)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind10, KaizenObject.GetPtr(this), capTop.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCapTop, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsCapTop()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind11, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCapBottom, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCapBottom(bool capBottom)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind12, KaizenObject.GetPtr(this), capBottom.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.IsCapBottom, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsCapBottom()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind13, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCurve, 270443179ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCurve(Curve curve)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind14, KaizenObject.GetPtr(this), KaizenObject.GetPtr(curve));
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCurve, 2460114913ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Curve GetCurve()
    {
        Curve ret = (Curve)NativeCalls.kaizen_icall_0_87(MethodBind15, KaizenObject.GetPtr(this));
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
    public new class PropertyName : PrimitiveMesh.PropertyName
    {
        /// <summary>
        /// Cached name for the 'radius' property.
        /// </summary>
        public static readonly StringName Radius = "radius";
        /// <summary>
        /// Cached name for the 'radial_steps' property.
        /// </summary>
        public static readonly StringName RadialSteps = "radial_steps";
        /// <summary>
        /// Cached name for the 'sections' property.
        /// </summary>
        public static readonly StringName Sections = "sections";
        /// <summary>
        /// Cached name for the 'section_length' property.
        /// </summary>
        public static readonly StringName SectionLength = "section_length";
        /// <summary>
        /// Cached name for the 'section_rings' property.
        /// </summary>
        public static readonly StringName SectionRings = "section_rings";
        /// <summary>
        /// Cached name for the 'cap_top' property.
        /// </summary>
        public static readonly StringName CapTop = "cap_top";
        /// <summary>
        /// Cached name for the 'cap_bottom' property.
        /// </summary>
        public static readonly StringName CapBottom = "cap_bottom";
        /// <summary>
        /// Cached name for the 'curve' property.
        /// </summary>
        public static readonly StringName Curve = "curve";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : PrimitiveMesh.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_radius' method.
        /// </summary>
        public static readonly StringName SetRadius = "set_radius";
        /// <summary>
        /// Cached name for the 'get_radius' method.
        /// </summary>
        public static readonly StringName GetRadius = "get_radius";
        /// <summary>
        /// Cached name for the 'set_radial_steps' method.
        /// </summary>
        public static readonly StringName SetRadialSteps = "set_radial_steps";
        /// <summary>
        /// Cached name for the 'get_radial_steps' method.
        /// </summary>
        public static readonly StringName GetRadialSteps = "get_radial_steps";
        /// <summary>
        /// Cached name for the 'set_sections' method.
        /// </summary>
        public static readonly StringName SetSections = "set_sections";
        /// <summary>
        /// Cached name for the 'get_sections' method.
        /// </summary>
        public static readonly StringName GetSections = "get_sections";
        /// <summary>
        /// Cached name for the 'set_section_length' method.
        /// </summary>
        public static readonly StringName SetSectionLength = "set_section_length";
        /// <summary>
        /// Cached name for the 'get_section_length' method.
        /// </summary>
        public static readonly StringName GetSectionLength = "get_section_length";
        /// <summary>
        /// Cached name for the 'set_section_rings' method.
        /// </summary>
        public static readonly StringName SetSectionRings = "set_section_rings";
        /// <summary>
        /// Cached name for the 'get_section_rings' method.
        /// </summary>
        public static readonly StringName GetSectionRings = "get_section_rings";
        /// <summary>
        /// Cached name for the 'set_cap_top' method.
        /// </summary>
        public static readonly StringName SetCapTop = "set_cap_top";
        /// <summary>
        /// Cached name for the 'is_cap_top' method.
        /// </summary>
        public static readonly StringName IsCapTop = "is_cap_top";
        /// <summary>
        /// Cached name for the 'set_cap_bottom' method.
        /// </summary>
        public static readonly StringName SetCapBottom = "set_cap_bottom";
        /// <summary>
        /// Cached name for the 'is_cap_bottom' method.
        /// </summary>
        public static readonly StringName IsCapBottom = "is_cap_bottom";
        /// <summary>
        /// Cached name for the 'set_curve' method.
        /// </summary>
        public static readonly StringName SetCurve = "set_curve";
        /// <summary>
        /// Cached name for the 'get_curve' method.
        /// </summary>
        public static readonly StringName GetCurve = "get_curve";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : PrimitiveMesh.SignalName
    {
    }
}
