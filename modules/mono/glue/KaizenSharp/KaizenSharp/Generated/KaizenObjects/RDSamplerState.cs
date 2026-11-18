namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>This object is used by <see cref="Kaizen.RenderingDevice"/>.</para>
/// </summary>
public partial class RDSamplerState : RefCounted
{
    /// <summary>
    /// <para>The sampler's magnification filter. It is the filtering method used when sampling texels that appear bigger than on-screen pixels.</para>
    /// </summary>
    public RenderingDevice.SamplerFilter MagFilter
    {
        get
        {
            return GetMagFilter();
        }
        set
        {
            SetMagFilter(value);
        }
    }

    /// <summary>
    /// <para>The sampler's minification filter. It is the filtering method used when sampling texels that appear smaller than on-screen pixels.</para>
    /// </summary>
    public RenderingDevice.SamplerFilter MinFilter
    {
        get
        {
            return GetMinFilter();
        }
        set
        {
            SetMinFilter(value);
        }
    }

    /// <summary>
    /// <para>The filtering method to use for mipmaps.</para>
    /// </summary>
    public RenderingDevice.SamplerFilter MipFilter
    {
        get
        {
            return GetMipFilter();
        }
        set
        {
            SetMipFilter(value);
        }
    }

    /// <summary>
    /// <para>The repeat mode to use along the U axis of UV coordinates. This affects the returned values if sampling outside the UV bounds.</para>
    /// </summary>
    public RenderingDevice.SamplerRepeatMode RepeatU
    {
        get
        {
            return GetRepeatU();
        }
        set
        {
            SetRepeatU(value);
        }
    }

    /// <summary>
    /// <para>The repeat mode to use along the V axis of UV coordinates. This affects the returned values if sampling outside the UV bounds.</para>
    /// </summary>
    public RenderingDevice.SamplerRepeatMode RepeatV
    {
        get
        {
            return GetRepeatV();
        }
        set
        {
            SetRepeatV(value);
        }
    }

    /// <summary>
    /// <para>The repeat mode to use along the W axis of UV coordinates. This affects the returned values if sampling outside the UV bounds. Only effective for 3D samplers.</para>
    /// </summary>
    public RenderingDevice.SamplerRepeatMode RepeatW
    {
        get
        {
            return GetRepeatW();
        }
        set
        {
            SetRepeatW(value);
        }
    }

    /// <summary>
    /// <para>The mipmap LOD bias to use. Positive values will make the sampler blurrier at a given distance, while negative values will make the sampler sharper at a given distance (at the risk of looking grainy). Recommended values are between <c>-0.5</c> and <c>0.0</c>. Only effective if the sampler has mipmaps available.</para>
    /// </summary>
    public float LodBias
    {
        get
        {
            return GetLodBias();
        }
        set
        {
            SetLodBias(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, perform anisotropic sampling. See <see cref="Kaizen.RDSamplerState.AnisotropyMax"/>.</para>
    /// </summary>
    public bool UseAnisotropy
    {
        get
        {
            return GetUseAnisotropy();
        }
        set
        {
            SetUseAnisotropy(value);
        }
    }

    /// <summary>
    /// <para>Maximum anisotropy that can be used when sampling. Only effective if <see cref="Kaizen.RDSamplerState.UseAnisotropy"/> is <see langword="true"/>. Higher values result in a sharper sampler at oblique angles, at the cost of performance (due to memory bandwidth). This value may be limited by the graphics hardware in use. Most graphics hardware only supports values up to <c>16.0</c>.</para>
    /// <para>If <see cref="Kaizen.RDSamplerState.AnisotropyMax"/> is <c>1.0</c>, forcibly disables anisotropy even if <see cref="Kaizen.RDSamplerState.UseAnisotropy"/> is <see langword="true"/>.</para>
    /// </summary>
    public float AnisotropyMax
    {
        get
        {
            return GetAnisotropyMax();
        }
        set
        {
            SetAnisotropyMax(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, returned values will be based on the comparison operation defined in <see cref="Kaizen.RDSamplerState.CompareOp"/>. This is a hardware-based approach and is therefore faster than performing this manually in a shader. For example, compare operations are used for shadow map rendering by comparing depth values from a shadow sampler.</para>
    /// </summary>
    public bool EnableCompare
    {
        get
        {
            return GetEnableCompare();
        }
        set
        {
            SetEnableCompare(value);
        }
    }

    /// <summary>
    /// <para>The compare operation to use. Only effective if <see cref="Kaizen.RDSamplerState.EnableCompare"/> is <see langword="true"/>.</para>
    /// </summary>
    public RenderingDevice.CompareOperator CompareOp
    {
        get
        {
            return GetCompareOp();
        }
        set
        {
            SetCompareOp(value);
        }
    }

    /// <summary>
    /// <para>The minimum mipmap LOD bias to display (highest resolution). Only effective if the sampler has mipmaps available.</para>
    /// </summary>
    public float MinLod
    {
        get
        {
            return GetMinLod();
        }
        set
        {
            SetMinLod(value);
        }
    }

    /// <summary>
    /// <para>The maximum mipmap LOD bias to display (lowest resolution). Only effective if the sampler has mipmaps available.</para>
    /// </summary>
    public float MaxLod
    {
        get
        {
            return GetMaxLod();
        }
        set
        {
            SetMaxLod(value);
        }
    }

    /// <summary>
    /// <para>The border color that will be returned when sampling outside the sampler's bounds and the <see cref="Kaizen.RDSamplerState.RepeatU"/>, <see cref="Kaizen.RDSamplerState.RepeatV"/> or <see cref="Kaizen.RDSamplerState.RepeatW"/> modes have repeating disabled.</para>
    /// </summary>
    public RenderingDevice.SamplerBorderColor BorderColor
    {
        get
        {
            return GetBorderColor();
        }
        set
        {
            SetBorderColor(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the texture will be sampled with coordinates ranging from 0 to the texture's resolution. Otherwise, the coordinates will be normalized and range from 0 to 1.</para>
    /// </summary>
    public bool UnnormalizedUvw
    {
        get
        {
            return GetUnnormalizedUvw();
        }
        set
        {
            SetUnnormalizedUvw(value);
        }
    }

    private static readonly System.Type CachedType = typeof(RDSamplerState);

    private static readonly StringName NativeName = "RDSamplerState";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public RDSamplerState() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal RDSamplerState(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal RDSamplerState(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMagFilter, 1493420382ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMagFilter(RenderingDevice.SamplerFilter pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMagFilter, 2209202801ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerFilter GetMagFilter()
    {
        RenderingDevice.SamplerFilter ret = (RenderingDevice.SamplerFilter)NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMinFilter, 1493420382ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMinFilter(RenderingDevice.SamplerFilter pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMinFilter, 2209202801ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerFilter GetMinFilter()
    {
        RenderingDevice.SamplerFilter ret = (RenderingDevice.SamplerFilter)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMipFilter, 1493420382ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMipFilter(RenderingDevice.SamplerFilter pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind4, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMipFilter, 2209202801ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerFilter GetMipFilter()
    {
        RenderingDevice.SamplerFilter ret = (RenderingDevice.SamplerFilter)NativeCalls.kaizen_icall_0_39(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRepeatU, 246127626ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRepeatU(RenderingDevice.SamplerRepeatMode pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind6, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRepeatU, 3227895872ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerRepeatMode GetRepeatU()
    {
        RenderingDevice.SamplerRepeatMode ret = (RenderingDevice.SamplerRepeatMode)NativeCalls.kaizen_icall_0_39(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRepeatV, 246127626ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRepeatV(RenderingDevice.SamplerRepeatMode pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind8, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRepeatV, 3227895872ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerRepeatMode GetRepeatV()
    {
        RenderingDevice.SamplerRepeatMode ret = (RenderingDevice.SamplerRepeatMode)NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRepeatW, 246127626ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRepeatW(RenderingDevice.SamplerRepeatMode pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind10, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRepeatW, 3227895872ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerRepeatMode GetRepeatW()
    {
        RenderingDevice.SamplerRepeatMode ret = (RenderingDevice.SamplerRepeatMode)NativeCalls.kaizen_icall_0_39(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLodBias, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetLodBias(float pMember)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind12, KaizenObject.GetPtr(this), pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLodBias, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetLodBias()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUseAnisotropy, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUseAnisotropy(bool pMember)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind14, KaizenObject.GetPtr(this), pMember.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUseAnisotropy, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetUseAnisotropy()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind15, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAnisotropyMax, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetAnisotropyMax(float pMember)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind16, KaizenObject.GetPtr(this), pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAnisotropyMax, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetAnisotropyMax()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind17, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetEnableCompare, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetEnableCompare(bool pMember)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind18, KaizenObject.GetPtr(this), pMember.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetEnableCompare, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetEnableCompare()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind19, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCompareOp, 2573711505ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCompareOp(RenderingDevice.CompareOperator pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind20, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCompareOp, 269730778ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.CompareOperator GetCompareOp()
    {
        RenderingDevice.CompareOperator ret = (RenderingDevice.CompareOperator)NativeCalls.kaizen_icall_0_39(MethodBind21, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMinLod, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMinLod(float pMember)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind22, KaizenObject.GetPtr(this), pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMinLod, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetMinLod()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind23, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMaxLod, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMaxLod(float pMember)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind24, KaizenObject.GetPtr(this), pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMaxLod, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetMaxLod()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind25, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBorderColor, 1115869595ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBorderColor(RenderingDevice.SamplerBorderColor pMember)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind26, KaizenObject.GetPtr(this), (int)pMember);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBorderColor, 3514246478ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public RenderingDevice.SamplerBorderColor GetBorderColor()
    {
        RenderingDevice.SamplerBorderColor ret = (RenderingDevice.SamplerBorderColor)NativeCalls.kaizen_icall_0_39(MethodBind27, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUnnormalizedUvw, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUnnormalizedUvw(bool pMember)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind28, KaizenObject.GetPtr(this), pMember.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUnnormalizedUvw, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetUnnormalizedUvw()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind29, KaizenObject.GetPtr(this)).ToBool();
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
    public new class PropertyName : RefCounted.PropertyName
    {
        /// <summary>
        /// Cached name for the 'mag_filter' property.
        /// </summary>
        public static readonly StringName MagFilter = "mag_filter";
        /// <summary>
        /// Cached name for the 'min_filter' property.
        /// </summary>
        public static readonly StringName MinFilter = "min_filter";
        /// <summary>
        /// Cached name for the 'mip_filter' property.
        /// </summary>
        public static readonly StringName MipFilter = "mip_filter";
        /// <summary>
        /// Cached name for the 'repeat_u' property.
        /// </summary>
        public static readonly StringName RepeatU = "repeat_u";
        /// <summary>
        /// Cached name for the 'repeat_v' property.
        /// </summary>
        public static readonly StringName RepeatV = "repeat_v";
        /// <summary>
        /// Cached name for the 'repeat_w' property.
        /// </summary>
        public static readonly StringName RepeatW = "repeat_w";
        /// <summary>
        /// Cached name for the 'lod_bias' property.
        /// </summary>
        public static readonly StringName LodBias = "lod_bias";
        /// <summary>
        /// Cached name for the 'use_anisotropy' property.
        /// </summary>
        public static readonly StringName UseAnisotropy = "use_anisotropy";
        /// <summary>
        /// Cached name for the 'anisotropy_max' property.
        /// </summary>
        public static readonly StringName AnisotropyMax = "anisotropy_max";
        /// <summary>
        /// Cached name for the 'enable_compare' property.
        /// </summary>
        public static readonly StringName EnableCompare = "enable_compare";
        /// <summary>
        /// Cached name for the 'compare_op' property.
        /// </summary>
        public static readonly StringName CompareOp = "compare_op";
        /// <summary>
        /// Cached name for the 'min_lod' property.
        /// </summary>
        public static readonly StringName MinLod = "min_lod";
        /// <summary>
        /// Cached name for the 'max_lod' property.
        /// </summary>
        public static readonly StringName MaxLod = "max_lod";
        /// <summary>
        /// Cached name for the 'border_color' property.
        /// </summary>
        public static readonly StringName BorderColor = "border_color";
        /// <summary>
        /// Cached name for the 'unnormalized_uvw' property.
        /// </summary>
        public static readonly StringName UnnormalizedUvw = "unnormalized_uvw";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : RefCounted.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_mag_filter' method.
        /// </summary>
        public static readonly StringName SetMagFilter = "set_mag_filter";
        /// <summary>
        /// Cached name for the 'get_mag_filter' method.
        /// </summary>
        public static readonly StringName GetMagFilter = "get_mag_filter";
        /// <summary>
        /// Cached name for the 'set_min_filter' method.
        /// </summary>
        public static readonly StringName SetMinFilter = "set_min_filter";
        /// <summary>
        /// Cached name for the 'get_min_filter' method.
        /// </summary>
        public static readonly StringName GetMinFilter = "get_min_filter";
        /// <summary>
        /// Cached name for the 'set_mip_filter' method.
        /// </summary>
        public static readonly StringName SetMipFilter = "set_mip_filter";
        /// <summary>
        /// Cached name for the 'get_mip_filter' method.
        /// </summary>
        public static readonly StringName GetMipFilter = "get_mip_filter";
        /// <summary>
        /// Cached name for the 'set_repeat_u' method.
        /// </summary>
        public static readonly StringName SetRepeatU = "set_repeat_u";
        /// <summary>
        /// Cached name for the 'get_repeat_u' method.
        /// </summary>
        public static readonly StringName GetRepeatU = "get_repeat_u";
        /// <summary>
        /// Cached name for the 'set_repeat_v' method.
        /// </summary>
        public static readonly StringName SetRepeatV = "set_repeat_v";
        /// <summary>
        /// Cached name for the 'get_repeat_v' method.
        /// </summary>
        public static readonly StringName GetRepeatV = "get_repeat_v";
        /// <summary>
        /// Cached name for the 'set_repeat_w' method.
        /// </summary>
        public static readonly StringName SetRepeatW = "set_repeat_w";
        /// <summary>
        /// Cached name for the 'get_repeat_w' method.
        /// </summary>
        public static readonly StringName GetRepeatW = "get_repeat_w";
        /// <summary>
        /// Cached name for the 'set_lod_bias' method.
        /// </summary>
        public static readonly StringName SetLodBias = "set_lod_bias";
        /// <summary>
        /// Cached name for the 'get_lod_bias' method.
        /// </summary>
        public static readonly StringName GetLodBias = "get_lod_bias";
        /// <summary>
        /// Cached name for the 'set_use_anisotropy' method.
        /// </summary>
        public static readonly StringName SetUseAnisotropy = "set_use_anisotropy";
        /// <summary>
        /// Cached name for the 'get_use_anisotropy' method.
        /// </summary>
        public static readonly StringName GetUseAnisotropy = "get_use_anisotropy";
        /// <summary>
        /// Cached name for the 'set_anisotropy_max' method.
        /// </summary>
        public static readonly StringName SetAnisotropyMax = "set_anisotropy_max";
        /// <summary>
        /// Cached name for the 'get_anisotropy_max' method.
        /// </summary>
        public static readonly StringName GetAnisotropyMax = "get_anisotropy_max";
        /// <summary>
        /// Cached name for the 'set_enable_compare' method.
        /// </summary>
        public static readonly StringName SetEnableCompare = "set_enable_compare";
        /// <summary>
        /// Cached name for the 'get_enable_compare' method.
        /// </summary>
        public static readonly StringName GetEnableCompare = "get_enable_compare";
        /// <summary>
        /// Cached name for the 'set_compare_op' method.
        /// </summary>
        public static readonly StringName SetCompareOp = "set_compare_op";
        /// <summary>
        /// Cached name for the 'get_compare_op' method.
        /// </summary>
        public static readonly StringName GetCompareOp = "get_compare_op";
        /// <summary>
        /// Cached name for the 'set_min_lod' method.
        /// </summary>
        public static readonly StringName SetMinLod = "set_min_lod";
        /// <summary>
        /// Cached name for the 'get_min_lod' method.
        /// </summary>
        public static readonly StringName GetMinLod = "get_min_lod";
        /// <summary>
        /// Cached name for the 'set_max_lod' method.
        /// </summary>
        public static readonly StringName SetMaxLod = "set_max_lod";
        /// <summary>
        /// Cached name for the 'get_max_lod' method.
        /// </summary>
        public static readonly StringName GetMaxLod = "get_max_lod";
        /// <summary>
        /// Cached name for the 'set_border_color' method.
        /// </summary>
        public static readonly StringName SetBorderColor = "set_border_color";
        /// <summary>
        /// Cached name for the 'get_border_color' method.
        /// </summary>
        public static readonly StringName GetBorderColor = "get_border_color";
        /// <summary>
        /// Cached name for the 'set_unnormalized_uvw' method.
        /// </summary>
        public static readonly StringName SetUnnormalizedUvw = "set_unnormalized_uvw";
        /// <summary>
        /// Cached name for the 'get_unnormalized_uvw' method.
        /// </summary>
        public static readonly StringName GetUnnormalizedUvw = "get_unnormalized_uvw";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : RefCounted.SignalName
    {
    }
}
