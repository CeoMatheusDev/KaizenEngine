namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para><see cref="Kaizen.CanvasItemMaterial"/>s provide a means of modifying the textures associated with a CanvasItem. They specialize in describing blend and lighting behaviors for textures. Use a <see cref="Kaizen.ShaderMaterial"/> to more fully customize a material's interactions with a <see cref="Kaizen.CanvasItem"/>.</para>
/// </summary>
public partial class CanvasItemMaterial : Material
{
    public enum BlendModeEnum : long
    {
        /// <summary>
        /// <para>Mix blending mode. Colors are assumed to be independent of the alpha (opacity) value.</para>
        /// </summary>
        Mix = 0,
        /// <summary>
        /// <para>Additive blending mode.</para>
        /// </summary>
        Add = 1,
        /// <summary>
        /// <para>Subtractive blending mode.</para>
        /// </summary>
        Sub = 2,
        /// <summary>
        /// <para>Multiplicative blending mode.</para>
        /// </summary>
        Mul = 3,
        /// <summary>
        /// <para>Mix blending mode. Colors are assumed to be premultiplied by the alpha (opacity) value.</para>
        /// </summary>
        PremultAlpha = 4
    }

    public enum LightModeEnum : long
    {
        /// <summary>
        /// <para>Render the material using both light and non-light sensitive material properties.</para>
        /// </summary>
        Normal = 0,
        /// <summary>
        /// <para>Render the material as if there were no light.</para>
        /// </summary>
        Unshaded = 1,
        /// <summary>
        /// <para>Render the material as if there were only light.</para>
        /// </summary>
        LightOnly = 2
    }

    /// <summary>
    /// <para>The manner in which a material's rendering is applied to underlying textures.</para>
    /// </summary>
    public CanvasItemMaterial.BlendModeEnum BlendMode
    {
        get
        {
            return GetBlendMode();
        }
        set
        {
            SetBlendMode(value);
        }
    }

    /// <summary>
    /// <para>The manner in which material reacts to lighting.</para>
    /// </summary>
    public CanvasItemMaterial.LightModeEnum LightMode
    {
        get
        {
            return GetLightMode();
        }
        set
        {
            SetLightMode(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, enable spritesheet-based animation features when assigned to <see cref="Kaizen.GpuParticles2D"/> and <see cref="Kaizen.CpuParticles2D"/> nodes. The <see cref="Kaizen.ParticleProcessMaterial.AnimSpeedMax"/> or <see cref="Kaizen.CpuParticles2D.AnimSpeedMax"/> should also be set to a positive value for the animation to play.</para>
    /// <para>This property (and other <c>particles_anim_*</c> properties that depend on it) has no effect on other types of nodes.</para>
    /// </summary>
    public bool ParticlesAnimation
    {
        get
        {
            return GetParticlesAnimation();
        }
        set
        {
            SetParticlesAnimation(value);
        }
    }

    /// <summary>
    /// <para>The number of columns in the spritesheet assigned as <see cref="Kaizen.Texture2D"/> for a <see cref="Kaizen.GpuParticles2D"/> or <see cref="Kaizen.CpuParticles2D"/>.</para>
    /// <para><b>Note:</b> This property is only used and visible in the editor if <see cref="Kaizen.CanvasItemMaterial.ParticlesAnimation"/> is <see langword="true"/>.</para>
    /// </summary>
    public int ParticlesAnimHFrames
    {
        get
        {
            return GetParticlesAnimHFrames();
        }
        set
        {
            SetParticlesAnimHFrames(value);
        }
    }

    /// <summary>
    /// <para>The number of rows in the spritesheet assigned as <see cref="Kaizen.Texture2D"/> for a <see cref="Kaizen.GpuParticles2D"/> or <see cref="Kaizen.CpuParticles2D"/>.</para>
    /// <para><b>Note:</b> This property is only used and visible in the editor if <see cref="Kaizen.CanvasItemMaterial.ParticlesAnimation"/> is <see langword="true"/>.</para>
    /// </summary>
    public int ParticlesAnimVFrames
    {
        get
        {
            return GetParticlesAnimVFrames();
        }
        set
        {
            SetParticlesAnimVFrames(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, the particles animation will loop.</para>
    /// <para><b>Note:</b> This property is only used and visible in the editor if <see cref="Kaizen.CanvasItemMaterial.ParticlesAnimation"/> is <see langword="true"/>.</para>
    /// </summary>
    public bool ParticlesAnimLoop
    {
        get
        {
            return GetParticlesAnimLoop();
        }
        set
        {
            SetParticlesAnimLoop(value);
        }
    }

    private static readonly System.Type CachedType = typeof(CanvasItemMaterial);

    private static readonly StringName NativeName = "CanvasItemMaterial";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public CanvasItemMaterial() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal CanvasItemMaterial(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal CanvasItemMaterial(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBlendMode, 1786054936ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBlendMode(CanvasItemMaterial.BlendModeEnum blendMode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind0, KaizenObject.GetPtr(this), (int)blendMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBlendMode, 3318684035ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public CanvasItemMaterial.BlendModeEnum GetBlendMode()
    {
        CanvasItemMaterial.BlendModeEnum ret = (CanvasItemMaterial.BlendModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLightMode, 628074070ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetLightMode(CanvasItemMaterial.LightModeEnum lightMode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind2, KaizenObject.GetPtr(this), (int)lightMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLightMode, 3863292382ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public CanvasItemMaterial.LightModeEnum GetLightMode()
    {
        CanvasItemMaterial.LightModeEnum ret = (CanvasItemMaterial.LightModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetParticlesAnimation, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParticlesAnimation(bool particlesAnim)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind4, KaizenObject.GetPtr(this), particlesAnim.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetParticlesAnimation, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetParticlesAnimation()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind5, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetParticlesAnimHFrames, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParticlesAnimHFrames(int frames)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind6, KaizenObject.GetPtr(this), frames);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetParticlesAnimHFrames, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetParticlesAnimHFrames()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetParticlesAnimVFrames, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParticlesAnimVFrames(int frames)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind8, KaizenObject.GetPtr(this), frames);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetParticlesAnimVFrames, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetParticlesAnimVFrames()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetParticlesAnimLoop, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetParticlesAnimLoop(bool loop)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind10, KaizenObject.GetPtr(this), loop.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetParticlesAnimLoop, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetParticlesAnimLoop()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind11, KaizenObject.GetPtr(this)).ToBool();
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
    public new class PropertyName : Material.PropertyName
    {
        /// <summary>
        /// Cached name for the 'blend_mode' property.
        /// </summary>
        public static readonly StringName BlendMode = "blend_mode";
        /// <summary>
        /// Cached name for the 'light_mode' property.
        /// </summary>
        public static readonly StringName LightMode = "light_mode";
        /// <summary>
        /// Cached name for the 'particles_animation' property.
        /// </summary>
        public static readonly StringName ParticlesAnimation = "particles_animation";
        /// <summary>
        /// Cached name for the 'particles_anim_h_frames' property.
        /// </summary>
        public static readonly StringName ParticlesAnimHFrames = "particles_anim_h_frames";
        /// <summary>
        /// Cached name for the 'particles_anim_v_frames' property.
        /// </summary>
        public static readonly StringName ParticlesAnimVFrames = "particles_anim_v_frames";
        /// <summary>
        /// Cached name for the 'particles_anim_loop' property.
        /// </summary>
        public static readonly StringName ParticlesAnimLoop = "particles_anim_loop";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Material.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_blend_mode' method.
        /// </summary>
        public static readonly StringName SetBlendMode = "set_blend_mode";
        /// <summary>
        /// Cached name for the 'get_blend_mode' method.
        /// </summary>
        public static readonly StringName GetBlendMode = "get_blend_mode";
        /// <summary>
        /// Cached name for the 'set_light_mode' method.
        /// </summary>
        public static readonly StringName SetLightMode = "set_light_mode";
        /// <summary>
        /// Cached name for the 'get_light_mode' method.
        /// </summary>
        public static readonly StringName GetLightMode = "get_light_mode";
        /// <summary>
        /// Cached name for the 'set_particles_animation' method.
        /// </summary>
        public static readonly StringName SetParticlesAnimation = "set_particles_animation";
        /// <summary>
        /// Cached name for the 'get_particles_animation' method.
        /// </summary>
        public static readonly StringName GetParticlesAnimation = "get_particles_animation";
        /// <summary>
        /// Cached name for the 'set_particles_anim_h_frames' method.
        /// </summary>
        public static readonly StringName SetParticlesAnimHFrames = "set_particles_anim_h_frames";
        /// <summary>
        /// Cached name for the 'get_particles_anim_h_frames' method.
        /// </summary>
        public static readonly StringName GetParticlesAnimHFrames = "get_particles_anim_h_frames";
        /// <summary>
        /// Cached name for the 'set_particles_anim_v_frames' method.
        /// </summary>
        public static readonly StringName SetParticlesAnimVFrames = "set_particles_anim_v_frames";
        /// <summary>
        /// Cached name for the 'get_particles_anim_v_frames' method.
        /// </summary>
        public static readonly StringName GetParticlesAnimVFrames = "get_particles_anim_v_frames";
        /// <summary>
        /// Cached name for the 'set_particles_anim_loop' method.
        /// </summary>
        public static readonly StringName SetParticlesAnimLoop = "set_particles_anim_loop";
        /// <summary>
        /// Cached name for the 'get_particles_anim_loop' method.
        /// </summary>
        public static readonly StringName GetParticlesAnimLoop = "get_particles_anim_loop";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Material.SignalName
    {
    }
}
