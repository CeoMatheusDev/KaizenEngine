namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>Contains all nodes and resources of a glTF file. This is used by <see cref="Kaizen.GltfDocument"/> as data storage, which allows <see cref="Kaizen.GltfDocument"/> and all <see cref="Kaizen.GltfDocumentExtension"/> classes to remain stateless.</para>
/// <para>GLTFState can be populated by <see cref="Kaizen.GltfDocument"/> reading a file or by converting a Kaizen scene. Then the data can either be used to create a Kaizen scene or save to a glTF file. The code that converts to/from a Kaizen scene can be intercepted at arbitrary points by <see cref="Kaizen.GltfDocumentExtension"/> classes. This allows for custom data to be stored in the glTF file or for custom data to be converted to/from Kaizen nodes.</para>
/// </summary>
[KaizenClassName("GLTFState")]
public partial class GltfState : Resource
{
    /// <summary>
    /// <para>Discards all embedded textures and uses untextured materials.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.GltfState.HandleBinaryImageModeEnum.DiscardTextures' instead.")]
    public const long HandleBinaryDiscardTextures = 0;
    /// <summary>
    /// <para>Extracts embedded textures to be reimported and compressed. Editor only. Acts as uncompressed at runtime.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.GltfState.HandleBinaryImageModeEnum.ExtractTextures' instead.")]
    public const long HandleBinaryExtractTextures = 1;
    /// <summary>
    /// <para>Embeds textures VRAM compressed with Basis Universal into the generated scene.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.GltfState.HandleBinaryImageModeEnum.EmbedAsBasisu' instead.")]
    public const long HandleBinaryEmbedAsBasisu = 2;
    /// <summary>
    /// <para>Embeds textures compressed losslessly into the generated scene, matching old behavior.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.GltfState.HandleBinaryImageModeEnum.EmbedAsUncompressed' instead.")]
    public const long HandleBinaryEmbedAsUncompressed = 3;

    public enum HandleBinaryImageModeEnum : long
    {
        /// <summary>
        /// <para>When importing a glTF file with embedded binary images, discards all images and uses untextured materials in their place. Images stored as separate files in the <c>res://</c> folder are not affected by this; those will be used as Kaizen imported them.</para>
        /// </summary>
        DiscardTextures = 0,
        /// <summary>
        /// <para>When importing a glTF file with embedded binary images, extracts them and saves them to their own files. This allows the image to be imported by Kaizen's image importer, which can then have their import options customized by the user, including optionally compressing the image to VRAM texture formats.</para>
        /// <para>This will save the images' bytes exactly as-is, without recompression. For image formats supplied by glTF extensions, the file will have a filename ending with the file extension supplied by <see cref="Kaizen.GltfDocumentExtension._GetImageFileExtension()"/> of the extension class.</para>
        /// <para><b>Note:</b> This option is editor-only. At runtime, this acts the same as <see cref="Kaizen.GltfState.HandleBinaryImageModeEnum.EmbedAsUncompressed"/>.</para>
        /// </summary>
        ExtractTextures = 1,
        /// <summary>
        /// <para>When importing a glTF file with embedded binary images, embeds textures VRAM compressed with Basis Universal into the generated scene. Images stored as separate files in the <c>res://</c> folder are not affected by this; those will be used as Kaizen imported them.</para>
        /// </summary>
        EmbedAsBasisu = 2,
        /// <summary>
        /// <para>When importing a glTF file with embedded binary images, embeds textures compressed losslessly into the generated scene. Images stored as separate files in the <c>res://</c> folder are not affected by this; those will be used as Kaizen imported them.</para>
        /// </summary>
        EmbedAsUncompressed = 3
    }

    /// <summary>
    /// <para>The original raw JSON document corresponding to this GLTFState.</para>
    /// </summary>
    public Kaizen.Collections.Dictionary Json
    {
        get
        {
            return GetJson();
        }
        set
        {
            SetJson(value);
        }
    }

    public int MajorVersion
    {
        get
        {
            return GetMajorVersion();
        }
        set
        {
            SetMajorVersion(value);
        }
    }

    public int MinorVersion
    {
        get
        {
            return GetMinorVersion();
        }
        set
        {
            SetMinorVersion(value);
        }
    }

    /// <summary>
    /// <para>The copyright string in the asset header of the glTF file. This is set during import if present and export if non-empty. See the glTF asset header documentation for more information.</para>
    /// </summary>
    public string Copyright
    {
        get
        {
            return GetCopyright();
        }
        set
        {
            SetCopyright(value);
        }
    }

    /// <summary>
    /// <para>The binary buffer attached to a .glb file.</para>
    /// </summary>
    public byte[] GlbData
    {
        get
        {
            return GetGlbData();
        }
        set
        {
            SetGlbData(value);
        }
    }

    public bool UseNamedSkinBinds
    {
        get
        {
            return GetUseNamedSkinBinds();
        }
        set
        {
            SetUseNamedSkinBinds(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfNode> Nodes
    {
        get
        {
            return GetNodes();
        }
        set
        {
            SetNodes(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    public Kaizen.Collections.Array<byte[]> Buffers
    {
        get
        {
            return GetBuffers();
        }
        set
        {
            SetBuffers(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfBufferView> BufferViews
    {
        get
        {
            return GetBufferViews();
        }
        set
        {
            SetBufferViews(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfAccessor> Accessors
    {
        get
        {
            return GetAccessors();
        }
        set
        {
            SetAccessors(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfMesh> Meshes
    {
        get
        {
            return GetMeshes();
        }
        set
        {
            SetMeshes(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<Material> Materials
    {
        get
        {
            return GetMaterials();
        }
        set
        {
            SetMaterials(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    /// <summary>
    /// <para>The name of the scene. When importing, if not specified, this will be the file name. When exporting, if specified, the scene name will be saved to the glTF file.</para>
    /// </summary>
    public string SceneName
    {
        get
        {
            return GetSceneName();
        }
        set
        {
            SetSceneName(value);
        }
    }

    /// <summary>
    /// <para>The folder path associated with this glTF data. This is used to find other files the glTF file references, like images or binary buffers. This will be set during import when appending from a file, and will be set during export when writing to a file.</para>
    /// </summary>
    public string BasePath
    {
        get
        {
            return GetBasePath();
        }
        set
        {
            SetBasePath(value);
        }
    }

    /// <summary>
    /// <para>The file name associated with this glTF data. If it ends with <c>.gltf</c>, this is text-based glTF, otherwise this is binary GLB. This will be set during import when appending from a file, and will be set during export when writing to a file. If writing to a buffer, this will be an empty string.</para>
    /// </summary>
    public string FileName
    {
        get
        {
            return GetFileName();
        }
        set
        {
            SetFileName(value);
        }
    }

    /// <summary>
    /// <para>The root nodes of the glTF file. Typically, a glTF file will only have one scene, and therefore one root node. However, a glTF file may have multiple scenes and therefore multiple root nodes, which will be generated as siblings of each other and as children of the root node of the generated Kaizen scene.</para>
    /// </summary>
    public int[] RootNodes
    {
        get
        {
            return GetRootNodes();
        }
        set
        {
            SetRootNodes(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfTexture> Textures
    {
        get
        {
            return GetTextures();
        }
        set
        {
            SetTextures(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfTextureSampler> TextureSamplers
    {
        get
        {
            return GetTextureSamplers();
        }
        set
        {
            SetTextureSamplers(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<Texture2D> Images
    {
        get
        {
            return GetImages();
        }
        set
        {
            SetImages(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfSkin> Skins
    {
        get
        {
            return GetSkins();
        }
        set
        {
            SetSkins(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfCamera> Cameras
    {
        get
        {
            return GetCameras();
        }
        set
        {
            SetCameras(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfLight> Lights
    {
        get
        {
            return GetLights();
        }
        set
        {
            SetLights(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<string> UniqueNames
    {
        get
        {
            return GetUniqueNames();
        }
        set
        {
            SetUniqueNames(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<string> UniqueAnimationNames
    {
        get
        {
            return GetUniqueAnimationNames();
        }
        set
        {
            SetUniqueAnimationNames(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfSkeleton> Skeletons
    {
        get
        {
            return GetSkeletons();
        }
        set
        {
            SetSkeletons(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    public bool CreateAnimations
    {
        get
        {
            return GetCreateAnimations();
        }
        set
        {
            SetCreateAnimations(value);
        }
    }

    /// <summary>
    /// <para>If <see langword="true"/>, forces all GLTFNodes in the document to be bones of a single <see cref="Kaizen.Skeleton3D"/> Kaizen node.</para>
    /// </summary>
    public bool ImportAsSkeletonBones
    {
        get
        {
            return GetImportAsSkeletonBones();
        }
        set
        {
            SetImportAsSkeletonBones(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public Kaizen.Collections.Array<GltfAnimation> Animations
    {
        get
        {
            return GetAnimations();
        }
        set
        {
            SetAnimations(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    /// <summary>
    /// <para>When importing a glTF file with unimported raw binary images embedded inside of binary blob buffers, in data URIs, or separate files not imported by Kaizen, this controls how the images are handled. Images can be discarded, saved as separate files, or embedded in the scene lossily or losslessly. See <see cref="Kaizen.GltfState.HandleBinaryImageModeEnum"/> for options.</para>
    /// <para>This property does nothing for image files in the <c>res://</c> folder imported by Kaizen, as those are handled by Kaizen's image importer directly, and then the Kaizen scene generated from the glTF file will use the images as Kaizen imported them.</para>
    /// </summary>
    public GltfState.HandleBinaryImageModeEnum HandleBinaryImageMode
    {
        get
        {
            return GetHandleBinaryImageMode();
        }
        set
        {
            SetHandleBinaryImageMode(value);
        }
    }

    /// <summary>
    /// <para>The baking fps of the animation for either import or export.</para>
    /// </summary>
    public double BakeFps
    {
        get
        {
            return GetBakeFps();
        }
        set
        {
            SetBakeFps(value);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0618 // Type or member is obsolete.
    public int HandleBinaryImage
    {
        get
        {
            return GetHandleBinaryImage();
        }
        set
        {
            SetHandleBinaryImage(value);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete.

    private static readonly System.Type CachedType = typeof(GltfState);

    private static readonly StringName NativeName = "GLTFState";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public GltfState() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfState(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfState(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AddUsedExtension, 2678287736ul);

    /// <summary>
    /// <para>Appends an extension to the list of extensions used by this glTF file during serialization. If <paramref name="required"/> is <see langword="true"/>, the extension will also be added to the list of required extensions. Do not run this in <see cref="Kaizen.GltfDocumentExtension._ExportPost(GltfState)"/>, as that stage is too late to add extensions. The final list is sorted alphabetically.</para>
    /// </summary>
    public void AddUsedExtension(string extensionName, bool required)
    {
        NativeCalls.kaizen_icall_2_515(MethodBind0, KaizenObject.GetPtr(this), extensionName, required.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendDataToBuffers, 1460416665ul);

    /// <summary>
    /// <para>Appends the given byte array <paramref name="data"/> to the buffers and creates a <see cref="Kaizen.GltfBufferView"/> for it. The index of the destination <see cref="Kaizen.GltfBufferView"/> is returned. If <paramref name="deduplication"/> is <see langword="true"/>, the buffers are first searched for duplicate data, otherwise new bytes are always appended.</para>
    /// </summary>
    public int AppendDataToBuffers(byte[] data, bool deduplication)
    {
        int ret = NativeCalls.kaizen_icall_2_685(MethodBind1, KaizenObject.GetPtr(this), data, deduplication.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Appends the given byte array <paramref name="data"/> to the buffers and creates a <see cref="Kaizen.GltfBufferView"/> for it. The index of the destination <see cref="Kaizen.GltfBufferView"/> is returned. If <paramref name="deduplication"/> is <see langword="true"/>, the buffers are first searched for duplicate data, otherwise new bytes are always appended.</para>
    /// </summary>
    public int AppendDataToBuffers(ReadOnlySpan<byte> data, bool deduplication)
    {
        int ret = NativeCalls.kaizen_icall_2_685(MethodBind1, KaizenObject.GetPtr(this), data, deduplication.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendGltfNode, 3562288551ul);

    /// <summary>
    /// <para>Appends the given <see cref="Kaizen.GltfNode"/> to the state, and returns its new index. This can be used to export one Kaizen node as multiple glTF nodes, or inject new glTF nodes at import time. On import, this must be called before <see cref="Kaizen.GltfDocumentExtension._GenerateSceneNode(GltfState, GltfNode, Node)"/> finishes for the parent node. On export, this must be called before <see cref="Kaizen.GltfDocumentExtension._ExportNode(GltfState, GltfNode, Kaizen.Collections.Dictionary, Node)"/> runs for the parent node.</para>
    /// <para>The <paramref name="kaizenSceneNode"/> parameter is the Kaizen scene node that corresponds to this glTF node. This is highly recommended to be set to a valid node, but may be <see langword="null"/> if there is no corresponding Kaizen scene node. One Kaizen scene node may be used for multiple glTF nodes, so if exporting multiple glTF nodes for one Kaizen scene node, use the same Kaizen scene node for each.</para>
    /// <para>The <paramref name="parentNodeIndex"/> parameter is the index of the parent <see cref="Kaizen.GltfNode"/> in the state. If <c>-1</c>, the node will be a root node, otherwise the new node will be added to the parent's list of children. The index will also be written to the <see cref="Kaizen.GltfNode.Parent"/> property of the new node.</para>
    /// </summary>
    public int AppendGltfNode(GltfNode gltfNode, Node kaizenSceneNode, int parentNodeIndex)
    {
        int ret = NativeCalls.kaizen_icall_3_686(MethodBind2, KaizenObject.GetPtr(this), KaizenObject.GetPtr(gltfNode), KaizenObject.GetPtr(kaizenSceneNode), parentNodeIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetJson, 3102165223ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Dictionary GetJson()
    {
        Kaizen.Collections.Dictionary ret = NativeCalls.kaizen_icall_0_148(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetJson, 4155329257ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetJson(Kaizen.Collections.Dictionary json)
    {
        NativeCalls.kaizen_icall_1_147(MethodBind4, KaizenObject.GetPtr(this), (kaizen_dictionary)(json ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMajorVersion, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetMajorVersion()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMajorVersion, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMajorVersion(int majorVersion)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind6, KaizenObject.GetPtr(this), majorVersion);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMinorVersion, 3905245786ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int GetMinorVersion()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMinorVersion, 1286410249ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetMinorVersion(int minorVersion)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind8, KaizenObject.GetPtr(this), minorVersion);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCopyright, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetCopyright()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCopyright, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCopyright(string copyright)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind10, KaizenObject.GetPtr(this), copyright);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetGlbData, 2362200018ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public byte[] GetGlbData()
    {
        byte[] ret = NativeCalls.kaizen_icall_0_2(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetGlbData, 2971499966ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetGlbData(byte[] glbData)
    {
        NativeCalls.kaizen_icall_1_230(MethodBind12, KaizenObject.GetPtr(this), glbData);
        global::System.GC.KeepAlive(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetGlbData(ReadOnlySpan<byte> glbData)
    {
        NativeCalls.kaizen_icall_1_230(MethodBind12, KaizenObject.GetPtr(this), glbData);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUseNamedSkinBinds, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetUseNamedSkinBinds()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind13, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUseNamedSkinBinds, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetUseNamedSkinBinds(bool useNamedSkinBinds)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind14, KaizenObject.GetPtr(this), useNamedSkinBinds.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNodes, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfNode"/>s in the glTF file. These are the nodes that <see cref="Kaizen.GltfNode.Children"/> and <see cref="Kaizen.GltfState.RootNodes"/> refer to. This includes nodes that may not be generated in the Kaizen scene, or nodes that may generate multiple Kaizen scene nodes.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfNode> GetNodes()
    {
        Kaizen.Collections.Array<GltfNode> ret = new Kaizen.Collections.Array<GltfNode>(NativeCalls.kaizen_icall_0_146(MethodBind15, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetNodes, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfNode"/>s in the state. These are the nodes that <see cref="Kaizen.GltfNode.Children"/> and <see cref="Kaizen.GltfState.RootNodes"/> refer to. Some of the nodes set here may not be generated in the Kaizen scene, or may generate multiple Kaizen scene nodes.</para>
    /// </summary>
    public void SetNodes(Kaizen.Collections.Array<GltfNode> nodes)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind16, KaizenObject.GetPtr(this), (kaizen_array)(nodes ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBuffers, 3995934104ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public Kaizen.Collections.Array<byte[]> GetBuffers()
    {
        Kaizen.Collections.Array<byte[]> ret = new Kaizen.Collections.Array<byte[]>(NativeCalls.kaizen_icall_0_146(MethodBind17, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBuffers, 381264803ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBuffers(Kaizen.Collections.Array<byte[]> buffers)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind18, KaizenObject.GetPtr(this), (kaizen_array)(buffers ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBufferViews, 3995934104ul);

    public Kaizen.Collections.Array<GltfBufferView> GetBufferViews()
    {
        Kaizen.Collections.Array<GltfBufferView> ret = new Kaizen.Collections.Array<GltfBufferView>(NativeCalls.kaizen_icall_0_146(MethodBind19, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBufferViews, 381264803ul);

    public void SetBufferViews(Kaizen.Collections.Array<GltfBufferView> bufferViews)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind20, KaizenObject.GetPtr(this), (kaizen_array)(bufferViews ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAccessors, 3995934104ul);

    public Kaizen.Collections.Array<GltfAccessor> GetAccessors()
    {
        Kaizen.Collections.Array<GltfAccessor> ret = new Kaizen.Collections.Array<GltfAccessor>(NativeCalls.kaizen_icall_0_146(MethodBind21, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAccessors, 381264803ul);

    public void SetAccessors(Kaizen.Collections.Array<GltfAccessor> accessors)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind22, KaizenObject.GetPtr(this), (kaizen_array)(accessors ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMeshes, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfMesh"/>es in the glTF file. These are the meshes that the <see cref="Kaizen.GltfNode.Mesh"/> index refers to.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfMesh> GetMeshes()
    {
        Kaizen.Collections.Array<GltfMesh> ret = new Kaizen.Collections.Array<GltfMesh>(NativeCalls.kaizen_icall_0_146(MethodBind23, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMeshes, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfMesh"/>es in the state. These are the meshes that the <see cref="Kaizen.GltfNode.Mesh"/> index refers to.</para>
    /// </summary>
    public void SetMeshes(Kaizen.Collections.Array<GltfMesh> meshes)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind24, KaizenObject.GetPtr(this), (kaizen_array)(meshes ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind25 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAnimationPlayersCount, 923996154ul);

    /// <summary>
    /// <para>Returns the number of <see cref="Kaizen.AnimationPlayer"/> nodes in this <see cref="Kaizen.GltfState"/>. These nodes are only used during the export process when converting Kaizen <see cref="Kaizen.AnimationPlayer"/> nodes to glTF animations.</para>
    /// </summary>
    public int GetAnimationPlayersCount(int animPlayerIndex)
    {
        int ret = NativeCalls.kaizen_icall_1_85(MethodBind25, KaizenObject.GetPtr(this), animPlayerIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind26 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAnimationPlayer, 1550200483ul);

    /// <summary>
    /// <para>Returns the <see cref="Kaizen.AnimationPlayer"/> node with the given index. These nodes are only used during the export process when converting Kaizen <see cref="Kaizen.AnimationPlayer"/> nodes to glTF animations.</para>
    /// </summary>
    public AnimationPlayer GetAnimationPlayer(int animPlayerIndex)
    {
        AnimationPlayer ret = (AnimationPlayer)NativeCalls.kaizen_icall_1_365(MethodBind26, KaizenObject.GetPtr(this), animPlayerIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind27 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetMaterials, 3995934104ul);

    public Kaizen.Collections.Array<Material> GetMaterials()
    {
        Kaizen.Collections.Array<Material> ret = new Kaizen.Collections.Array<Material>(NativeCalls.kaizen_icall_0_146(MethodBind27, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind28 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetMaterials, 381264803ul);

    public void SetMaterials(Kaizen.Collections.Array<Material> materials)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind28, KaizenObject.GetPtr(this), (kaizen_array)(materials ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind29 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSceneName, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetSceneName()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind29, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind30 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSceneName, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetSceneName(string sceneName)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind30, KaizenObject.GetPtr(this), sceneName);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind31 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBasePath, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetBasePath()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind31, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind32 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBasePath, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBasePath(string basePath)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind32, KaizenObject.GetPtr(this), basePath);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind33 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFileName, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetFileName()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind33, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind34 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFileName, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFileName(string fileName)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind34, KaizenObject.GetPtr(this), fileName);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind35 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRootNodes, 1930428628ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public int[] GetRootNodes()
    {
        int[] ret = NativeCalls.kaizen_icall_0_227(MethodBind35, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind36 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRootNodes, 3614634198ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRootNodes(int[] rootNodes)
    {
        NativeCalls.kaizen_icall_1_213(MethodBind36, KaizenObject.GetPtr(this), rootNodes);
        global::System.GC.KeepAlive(this);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRootNodes(ReadOnlySpan<int> rootNodes)
    {
        NativeCalls.kaizen_icall_1_213(MethodBind36, KaizenObject.GetPtr(this), rootNodes);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind37 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextures, 3995934104ul);

    public Kaizen.Collections.Array<GltfTexture> GetTextures()
    {
        Kaizen.Collections.Array<GltfTexture> ret = new Kaizen.Collections.Array<GltfTexture>(NativeCalls.kaizen_icall_0_146(MethodBind37, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind38 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTextures, 381264803ul);

    public void SetTextures(Kaizen.Collections.Array<GltfTexture> textures)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind38, KaizenObject.GetPtr(this), (kaizen_array)(textures ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind39 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextureSamplers, 3995934104ul);

    /// <summary>
    /// <para>Retrieves the array of texture samplers that are used by the textures contained in the glTF.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfTextureSampler> GetTextureSamplers()
    {
        Kaizen.Collections.Array<GltfTextureSampler> ret = new Kaizen.Collections.Array<GltfTextureSampler>(NativeCalls.kaizen_icall_0_146(MethodBind39, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind40 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTextureSamplers, 381264803ul);

    /// <summary>
    /// <para>Sets the array of texture samplers that are used by the textures contained in the glTF.</para>
    /// </summary>
    public void SetTextureSamplers(Kaizen.Collections.Array<GltfTextureSampler> textureSamplers)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind40, KaizenObject.GetPtr(this), (kaizen_array)(textureSamplers ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind41 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetImages, 3995934104ul);

    /// <summary>
    /// <para>Gets the images of the glTF file as an array of <see cref="Kaizen.Texture2D"/>s. These are the images that the <see cref="Kaizen.GltfTexture.SrcImage"/> index refers to.</para>
    /// </summary>
    public Kaizen.Collections.Array<Texture2D> GetImages()
    {
        Kaizen.Collections.Array<Texture2D> ret = new Kaizen.Collections.Array<Texture2D>(NativeCalls.kaizen_icall_0_146(MethodBind41, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind42 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetImages, 381264803ul);

    /// <summary>
    /// <para>Sets the images in the state stored as an array of <see cref="Kaizen.Texture2D"/>s. This can be used during export. These are the images that the <see cref="Kaizen.GltfTexture.SrcImage"/> index refers to.</para>
    /// </summary>
    public void SetImages(Kaizen.Collections.Array<Texture2D> images)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind42, KaizenObject.GetPtr(this), (kaizen_array)(images ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind43 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSkins, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfSkin"/>s in the glTF file. These are the skins that the <see cref="Kaizen.GltfNode.Skin"/> index refers to.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfSkin> GetSkins()
    {
        Kaizen.Collections.Array<GltfSkin> ret = new Kaizen.Collections.Array<GltfSkin>(NativeCalls.kaizen_icall_0_146(MethodBind43, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind44 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSkins, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfSkin"/>s in the state. These are the skins that the <see cref="Kaizen.GltfNode.Skin"/> index refers to.</para>
    /// </summary>
    public void SetSkins(Kaizen.Collections.Array<GltfSkin> skins)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind44, KaizenObject.GetPtr(this), (kaizen_array)(skins ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind45 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCameras, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfCamera"/>s in the glTF file. These are the cameras that the <see cref="Kaizen.GltfNode.Camera"/> index refers to.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfCamera> GetCameras()
    {
        Kaizen.Collections.Array<GltfCamera> ret = new Kaizen.Collections.Array<GltfCamera>(NativeCalls.kaizen_icall_0_146(MethodBind45, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind46 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCameras, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfCamera"/>s in the state. These are the cameras that the <see cref="Kaizen.GltfNode.Camera"/> index refers to.</para>
    /// </summary>
    public void SetCameras(Kaizen.Collections.Array<GltfCamera> cameras)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind46, KaizenObject.GetPtr(this), (kaizen_array)(cameras ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind47 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLights, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfLight"/>s in the glTF file. These are the lights that the <see cref="Kaizen.GltfNode.Light"/> index refers to.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfLight> GetLights()
    {
        Kaizen.Collections.Array<GltfLight> ret = new Kaizen.Collections.Array<GltfLight>(NativeCalls.kaizen_icall_0_146(MethodBind47, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind48 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLights, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfLight"/>s in the state. These are the lights that the <see cref="Kaizen.GltfNode.Light"/> index refers to.</para>
    /// </summary>
    public void SetLights(Kaizen.Collections.Array<GltfLight> lights)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind48, KaizenObject.GetPtr(this), (kaizen_array)(lights ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind49 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUniqueNames, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of unique node names. This is used in both the import process and export process.</para>
    /// </summary>
    public Kaizen.Collections.Array<string> GetUniqueNames()
    {
        Kaizen.Collections.Array<string> ret = new Kaizen.Collections.Array<string>(NativeCalls.kaizen_icall_0_146(MethodBind49, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind50 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUniqueNames, 381264803ul);

    /// <summary>
    /// <para>Sets the unique node names in the state. This is used in both the import process and export process.</para>
    /// </summary>
    public void SetUniqueNames(Kaizen.Collections.Array<string> uniqueNames)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind50, KaizenObject.GetPtr(this), (kaizen_array)(uniqueNames ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind51 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetUniqueAnimationNames, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of unique animation names. This is only used during the import process.</para>
    /// </summary>
    public Kaizen.Collections.Array<string> GetUniqueAnimationNames()
    {
        Kaizen.Collections.Array<string> ret = new Kaizen.Collections.Array<string>(NativeCalls.kaizen_icall_0_146(MethodBind51, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind52 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUniqueAnimationNames, 381264803ul);

    /// <summary>
    /// <para>Sets the unique animation names in the state. This is only used during the import process.</para>
    /// </summary>
    public void SetUniqueAnimationNames(Kaizen.Collections.Array<string> uniqueAnimationNames)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind52, KaizenObject.GetPtr(this), (kaizen_array)(uniqueAnimationNames ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind53 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSkeletons, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfSkeleton"/>s in the glTF file. These are the skeletons that the <see cref="Kaizen.GltfNode.Skeleton"/> index refers to.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfSkeleton> GetSkeletons()
    {
        Kaizen.Collections.Array<GltfSkeleton> ret = new Kaizen.Collections.Array<GltfSkeleton>(NativeCalls.kaizen_icall_0_146(MethodBind53, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind54 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetSkeletons, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfSkeleton"/>s in the state. These are the skeletons that the <see cref="Kaizen.GltfNode.Skeleton"/> index refers to.</para>
    /// </summary>
    public void SetSkeletons(Kaizen.Collections.Array<GltfSkeleton> skeletons)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind54, KaizenObject.GetPtr(this), (kaizen_array)(skeletons ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind55 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetCreateAnimations, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetCreateAnimations()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind55, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind56 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetCreateAnimations, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetCreateAnimations(bool createAnimations)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind56, KaizenObject.GetPtr(this), createAnimations.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind57 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetImportAsSkeletonBones, 36873697ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetImportAsSkeletonBones()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind57, KaizenObject.GetPtr(this)).ToBool();
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind58 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetImportAsSkeletonBones, 2586408642ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetImportAsSkeletonBones(bool importAsSkeletonBones)
    {
        NativeCalls.kaizen_icall_1_14(MethodBind58, KaizenObject.GetPtr(this), importAsSkeletonBones.ToKaizenBool());
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind59 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAnimations, 3995934104ul);

    /// <summary>
    /// <para>Returns an array of all <see cref="Kaizen.GltfAnimation"/>s in the glTF file. When importing, these will be generated as animations in an <see cref="Kaizen.AnimationPlayer"/> node. When exporting, these will be generated from Kaizen <see cref="Kaizen.AnimationPlayer"/> nodes.</para>
    /// </summary>
    public Kaizen.Collections.Array<GltfAnimation> GetAnimations()
    {
        Kaizen.Collections.Array<GltfAnimation> ret = new Kaizen.Collections.Array<GltfAnimation>(NativeCalls.kaizen_icall_0_146(MethodBind59, KaizenObject.GetPtr(this)));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind60 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAnimations, 381264803ul);

    /// <summary>
    /// <para>Sets the <see cref="Kaizen.GltfAnimation"/>s in the state. When importing, these will be generated as animations in an <see cref="Kaizen.AnimationPlayer"/> node. When exporting, these will be generated from Kaizen <see cref="Kaizen.AnimationPlayer"/> nodes.</para>
    /// </summary>
    public void SetAnimations(Kaizen.Collections.Array<GltfAnimation> animations)
    {
        NativeCalls.kaizen_icall_1_163(MethodBind60, KaizenObject.GetPtr(this), (kaizen_array)(animations ?? new()).NativeValue);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind61 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSceneNode, 539202265ul);

    /// <summary>
    /// <para>Returns the Kaizen scene node that corresponds to the same index as the <see cref="Kaizen.GltfNode"/> it was generated from. This is the inverse of <see cref="Kaizen.GltfState.GetNodeIndex(Node)"/>. Useful during the import process.</para>
    /// <para><b>Note:</b> Not every <see cref="Kaizen.GltfNode"/> will have a scene node generated, and not every generated scene node will have a corresponding <see cref="Kaizen.GltfNode"/>. If there is no scene node for this <see cref="Kaizen.GltfNode"/> index, <see langword="null"/> is returned.</para>
    /// </summary>
    public Node GetSceneNode(int gltfNodeIndex)
    {
        Node ret = (Node)NativeCalls.kaizen_icall_1_365(MethodBind61, KaizenObject.GetPtr(this), gltfNodeIndex);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind62 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetNodeIndex, 3810805390ul);

    /// <summary>
    /// <para>Returns the index of the <see cref="Kaizen.GltfNode"/> corresponding to this Kaizen scene node. This is the inverse of <see cref="Kaizen.GltfState.GetSceneNode(int)"/>. Useful during the export process.</para>
    /// <para><b>Note:</b> Not every Kaizen scene node will have a corresponding <see cref="Kaizen.GltfNode"/>, and not every <see cref="Kaizen.GltfNode"/> will have a scene node generated. If there is no <see cref="Kaizen.GltfNode"/> index for this scene node, <c>-1</c> is returned.</para>
    /// </summary>
    public int GetNodeIndex(Node sceneNode)
    {
        int ret = NativeCalls.kaizen_icall_1_399(MethodBind62, KaizenObject.GetPtr(this), KaizenObject.GetPtr(sceneNode));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind63 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetAdditionalData, 2760726917ul);

    /// <summary>
    /// <para>Gets additional arbitrary data in this <see cref="Kaizen.GltfState"/> instance. This can be used to keep per-file state data in <see cref="Kaizen.GltfDocumentExtension"/> classes, which is important because they are stateless.</para>
    /// <para>The argument should be the <see cref="Kaizen.GltfDocumentExtension"/> name (does not have to match the extension name in the glTF file), and the return value can be anything you set. If nothing was set, the return value is <see langword="null"/>.</para>
    /// </summary>
    public Variant GetAdditionalData(StringName extensionName)
    {
        Variant ret = NativeCalls.kaizen_icall_1_168(MethodBind63, KaizenObject.GetPtr(this), (kaizen_string_name)(extensionName?.NativeValue ?? default));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind64 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetAdditionalData, 3776071444ul);

    /// <summary>
    /// <para>Sets additional arbitrary data in this <see cref="Kaizen.GltfState"/> instance. This can be used to keep per-file state data in <see cref="Kaizen.GltfDocumentExtension"/> classes, which is important because they are stateless.</para>
    /// <para>The first argument should be the <see cref="Kaizen.GltfDocumentExtension"/> name (does not have to match the extension name in the glTF file), and the second argument can be anything you want.</para>
    /// </summary>
    public void SetAdditionalData(StringName extensionName, Variant additionalData)
    {
        NativeCalls.kaizen_icall_2_167(MethodBind64, KaizenObject.GetPtr(this), (kaizen_string_name)(extensionName?.NativeValue ?? default), additionalData);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind65 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHandleBinaryImageMode, 1363384196ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public GltfState.HandleBinaryImageModeEnum GetHandleBinaryImageMode()
    {
        GltfState.HandleBinaryImageModeEnum ret = (GltfState.HandleBinaryImageModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind65, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind66 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetHandleBinaryImageMode, 854676334ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetHandleBinaryImageMode(GltfState.HandleBinaryImageModeEnum method)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind66, KaizenObject.GetPtr(this), (int)method);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind67 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetBakeFps, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetBakeFps(double value)
    {
        NativeCalls.kaizen_icall_1_135(MethodBind67, KaizenObject.GetPtr(this), value);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind68 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetBakeFps, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public double GetBakeFps()
    {
        double ret = NativeCalls.kaizen_icall_0_136(MethodBind68, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind69 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetHandleBinaryImage, 3905245786ul);

    /// <summary>
    /// <para>Deprecated untyped alias for <see cref="Kaizen.GltfState.HandleBinaryImageMode"/>. When importing a glTF file with unimported raw binary images embedded inside of binary blob buffers, in data URIs, or separate files not imported by Kaizen, this controls how the images are handled.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.GltfState.HandleBinaryImageMode' instead.")]
    public int GetHandleBinaryImage()
    {
        int ret = NativeCalls.kaizen_icall_0_39(MethodBind69, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind70 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetHandleBinaryImage, 1286410249ul);

    /// <summary>
    /// <para>Deprecated untyped alias for <see cref="Kaizen.GltfState.HandleBinaryImageMode"/>. When importing a glTF file with unimported raw binary images embedded inside of binary blob buffers, in data URIs, or separate files not imported by Kaizen, this controls how the images are handled.</para>
    /// </summary>
    [Obsolete("Use 'Kaizen.GltfState.HandleBinaryImageMode' instead.")]
    public void SetHandleBinaryImage(int method)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind70, KaizenObject.GetPtr(this), method);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind71 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetExtractPrefix, 2841200299ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetExtractPrefix()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind71, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind72 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetExtractPath, 2841200299ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetExtractPath()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind72, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind73 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetDiscardMeshesAndMaterials, 2240911060ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool GetDiscardMeshesAndMaterials()
    {
        bool ret = NativeCalls.kaizen_icall_0_15(MethodBind73, KaizenObject.GetPtr(this)).ToBool();
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
    public new class PropertyName : Resource.PropertyName
    {
        /// <summary>
        /// Cached name for the 'json' property.
        /// </summary>
        public static readonly StringName Json = "json";
        /// <summary>
        /// Cached name for the 'major_version' property.
        /// </summary>
        public static readonly StringName MajorVersion = "major_version";
        /// <summary>
        /// Cached name for the 'minor_version' property.
        /// </summary>
        public static readonly StringName MinorVersion = "minor_version";
        /// <summary>
        /// Cached name for the 'copyright' property.
        /// </summary>
        public static readonly StringName Copyright = "copyright";
        /// <summary>
        /// Cached name for the 'glb_data' property.
        /// </summary>
        public static readonly StringName GlbData = "glb_data";
        /// <summary>
        /// Cached name for the 'use_named_skin_binds' property.
        /// </summary>
        public static readonly StringName UseNamedSkinBinds = "use_named_skin_binds";
        /// <summary>
        /// Cached name for the 'nodes' property.
        /// </summary>
        public static readonly StringName Nodes = "nodes";
        /// <summary>
        /// Cached name for the 'buffers' property.
        /// </summary>
        public static readonly StringName Buffers = "buffers";
        /// <summary>
        /// Cached name for the 'buffer_views' property.
        /// </summary>
        public static readonly StringName BufferViews = "buffer_views";
        /// <summary>
        /// Cached name for the 'accessors' property.
        /// </summary>
        public static readonly StringName Accessors = "accessors";
        /// <summary>
        /// Cached name for the 'meshes' property.
        /// </summary>
        public static readonly StringName Meshes = "meshes";
        /// <summary>
        /// Cached name for the 'materials' property.
        /// </summary>
        public static readonly StringName Materials = "materials";
        /// <summary>
        /// Cached name for the 'scene_name' property.
        /// </summary>
        public static readonly StringName SceneName = "scene_name";
        /// <summary>
        /// Cached name for the 'base_path' property.
        /// </summary>
        public static readonly StringName BasePath = "base_path";
        /// <summary>
        /// Cached name for the 'filename' property.
        /// </summary>
        public static readonly StringName FileName = "filename";
        /// <summary>
        /// Cached name for the 'root_nodes' property.
        /// </summary>
        public static readonly StringName RootNodes = "root_nodes";
        /// <summary>
        /// Cached name for the 'textures' property.
        /// </summary>
        public static readonly StringName Textures = "textures";
        /// <summary>
        /// Cached name for the 'texture_samplers' property.
        /// </summary>
        public static readonly StringName TextureSamplers = "texture_samplers";
        /// <summary>
        /// Cached name for the 'images' property.
        /// </summary>
        public static readonly StringName Images = "images";
        /// <summary>
        /// Cached name for the 'skins' property.
        /// </summary>
        public static readonly StringName Skins = "skins";
        /// <summary>
        /// Cached name for the 'cameras' property.
        /// </summary>
        public static readonly StringName Cameras = "cameras";
        /// <summary>
        /// Cached name for the 'lights' property.
        /// </summary>
        public static readonly StringName Lights = "lights";
        /// <summary>
        /// Cached name for the 'unique_names' property.
        /// </summary>
        public static readonly StringName UniqueNames = "unique_names";
        /// <summary>
        /// Cached name for the 'unique_animation_names' property.
        /// </summary>
        public static readonly StringName UniqueAnimationNames = "unique_animation_names";
        /// <summary>
        /// Cached name for the 'skeletons' property.
        /// </summary>
        public static readonly StringName Skeletons = "skeletons";
        /// <summary>
        /// Cached name for the 'create_animations' property.
        /// </summary>
        public static readonly StringName CreateAnimations = "create_animations";
        /// <summary>
        /// Cached name for the 'import_as_skeleton_bones' property.
        /// </summary>
        public static readonly StringName ImportAsSkeletonBones = "import_as_skeleton_bones";
        /// <summary>
        /// Cached name for the 'animations' property.
        /// </summary>
        public static readonly StringName Animations = "animations";
        /// <summary>
        /// Cached name for the 'handle_binary_image_mode' property.
        /// </summary>
        public static readonly StringName HandleBinaryImageMode = "handle_binary_image_mode";
        /// <summary>
        /// Cached name for the 'bake_fps' property.
        /// </summary>
        public static readonly StringName BakeFps = "bake_fps";
        /// <summary>
        /// Cached name for the 'handle_binary_image' property.
        /// </summary>
        public static readonly StringName HandleBinaryImage = "handle_binary_image";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'add_used_extension' method.
        /// </summary>
        public static readonly StringName AddUsedExtension = "add_used_extension";
        /// <summary>
        /// Cached name for the 'append_data_to_buffers' method.
        /// </summary>
        public static readonly StringName AppendDataToBuffers = "append_data_to_buffers";
        /// <summary>
        /// Cached name for the 'append_gltf_node' method.
        /// </summary>
        public static readonly StringName AppendGltfNode = "append_gltf_node";
        /// <summary>
        /// Cached name for the 'get_json' method.
        /// </summary>
        public static readonly StringName GetJson = "get_json";
        /// <summary>
        /// Cached name for the 'set_json' method.
        /// </summary>
        public static readonly StringName SetJson = "set_json";
        /// <summary>
        /// Cached name for the 'get_major_version' method.
        /// </summary>
        public static readonly StringName GetMajorVersion = "get_major_version";
        /// <summary>
        /// Cached name for the 'set_major_version' method.
        /// </summary>
        public static readonly StringName SetMajorVersion = "set_major_version";
        /// <summary>
        /// Cached name for the 'get_minor_version' method.
        /// </summary>
        public static readonly StringName GetMinorVersion = "get_minor_version";
        /// <summary>
        /// Cached name for the 'set_minor_version' method.
        /// </summary>
        public static readonly StringName SetMinorVersion = "set_minor_version";
        /// <summary>
        /// Cached name for the 'get_copyright' method.
        /// </summary>
        public static readonly StringName GetCopyright = "get_copyright";
        /// <summary>
        /// Cached name for the 'set_copyright' method.
        /// </summary>
        public static readonly StringName SetCopyright = "set_copyright";
        /// <summary>
        /// Cached name for the 'get_glb_data' method.
        /// </summary>
        public static readonly StringName GetGlbData = "get_glb_data";
        /// <summary>
        /// Cached name for the 'set_glb_data' method.
        /// </summary>
        public static readonly StringName SetGlbData = "set_glb_data";
        /// <summary>
        /// Cached name for the 'get_use_named_skin_binds' method.
        /// </summary>
        public static readonly StringName GetUseNamedSkinBinds = "get_use_named_skin_binds";
        /// <summary>
        /// Cached name for the 'set_use_named_skin_binds' method.
        /// </summary>
        public static readonly StringName SetUseNamedSkinBinds = "set_use_named_skin_binds";
        /// <summary>
        /// Cached name for the 'get_nodes' method.
        /// </summary>
        public static readonly StringName GetNodes = "get_nodes";
        /// <summary>
        /// Cached name for the 'set_nodes' method.
        /// </summary>
        public static readonly StringName SetNodes = "set_nodes";
        /// <summary>
        /// Cached name for the 'get_buffers' method.
        /// </summary>
        public static readonly StringName GetBuffers = "get_buffers";
        /// <summary>
        /// Cached name for the 'set_buffers' method.
        /// </summary>
        public static readonly StringName SetBuffers = "set_buffers";
        /// <summary>
        /// Cached name for the 'get_buffer_views' method.
        /// </summary>
        public static readonly StringName GetBufferViews = "get_buffer_views";
        /// <summary>
        /// Cached name for the 'set_buffer_views' method.
        /// </summary>
        public static readonly StringName SetBufferViews = "set_buffer_views";
        /// <summary>
        /// Cached name for the 'get_accessors' method.
        /// </summary>
        public static readonly StringName GetAccessors = "get_accessors";
        /// <summary>
        /// Cached name for the 'set_accessors' method.
        /// </summary>
        public static readonly StringName SetAccessors = "set_accessors";
        /// <summary>
        /// Cached name for the 'get_meshes' method.
        /// </summary>
        public static readonly StringName GetMeshes = "get_meshes";
        /// <summary>
        /// Cached name for the 'set_meshes' method.
        /// </summary>
        public static readonly StringName SetMeshes = "set_meshes";
        /// <summary>
        /// Cached name for the 'get_animation_players_count' method.
        /// </summary>
        public static readonly StringName GetAnimationPlayersCount = "get_animation_players_count";
        /// <summary>
        /// Cached name for the 'get_animation_player' method.
        /// </summary>
        public static readonly StringName GetAnimationPlayer = "get_animation_player";
        /// <summary>
        /// Cached name for the 'get_materials' method.
        /// </summary>
        public static readonly StringName GetMaterials = "get_materials";
        /// <summary>
        /// Cached name for the 'set_materials' method.
        /// </summary>
        public static readonly StringName SetMaterials = "set_materials";
        /// <summary>
        /// Cached name for the 'get_scene_name' method.
        /// </summary>
        public static readonly StringName GetSceneName = "get_scene_name";
        /// <summary>
        /// Cached name for the 'set_scene_name' method.
        /// </summary>
        public static readonly StringName SetSceneName = "set_scene_name";
        /// <summary>
        /// Cached name for the 'get_base_path' method.
        /// </summary>
        public static readonly StringName GetBasePath = "get_base_path";
        /// <summary>
        /// Cached name for the 'set_base_path' method.
        /// </summary>
        public static readonly StringName SetBasePath = "set_base_path";
        /// <summary>
        /// Cached name for the 'get_filename' method.
        /// </summary>
        public static readonly StringName GetFileName = "get_filename";
        /// <summary>
        /// Cached name for the 'set_filename' method.
        /// </summary>
        public static readonly StringName SetFileName = "set_filename";
        /// <summary>
        /// Cached name for the 'get_root_nodes' method.
        /// </summary>
        public static readonly StringName GetRootNodes = "get_root_nodes";
        /// <summary>
        /// Cached name for the 'set_root_nodes' method.
        /// </summary>
        public static readonly StringName SetRootNodes = "set_root_nodes";
        /// <summary>
        /// Cached name for the 'get_textures' method.
        /// </summary>
        public static readonly StringName GetTextures = "get_textures";
        /// <summary>
        /// Cached name for the 'set_textures' method.
        /// </summary>
        public static readonly StringName SetTextures = "set_textures";
        /// <summary>
        /// Cached name for the 'get_texture_samplers' method.
        /// </summary>
        public static readonly StringName GetTextureSamplers = "get_texture_samplers";
        /// <summary>
        /// Cached name for the 'set_texture_samplers' method.
        /// </summary>
        public static readonly StringName SetTextureSamplers = "set_texture_samplers";
        /// <summary>
        /// Cached name for the 'get_images' method.
        /// </summary>
        public static readonly StringName GetImages = "get_images";
        /// <summary>
        /// Cached name for the 'set_images' method.
        /// </summary>
        public static readonly StringName SetImages = "set_images";
        /// <summary>
        /// Cached name for the 'get_skins' method.
        /// </summary>
        public static readonly StringName GetSkins = "get_skins";
        /// <summary>
        /// Cached name for the 'set_skins' method.
        /// </summary>
        public static readonly StringName SetSkins = "set_skins";
        /// <summary>
        /// Cached name for the 'get_cameras' method.
        /// </summary>
        public static readonly StringName GetCameras = "get_cameras";
        /// <summary>
        /// Cached name for the 'set_cameras' method.
        /// </summary>
        public static readonly StringName SetCameras = "set_cameras";
        /// <summary>
        /// Cached name for the 'get_lights' method.
        /// </summary>
        public static readonly StringName GetLights = "get_lights";
        /// <summary>
        /// Cached name for the 'set_lights' method.
        /// </summary>
        public static readonly StringName SetLights = "set_lights";
        /// <summary>
        /// Cached name for the 'get_unique_names' method.
        /// </summary>
        public static readonly StringName GetUniqueNames = "get_unique_names";
        /// <summary>
        /// Cached name for the 'set_unique_names' method.
        /// </summary>
        public static readonly StringName SetUniqueNames = "set_unique_names";
        /// <summary>
        /// Cached name for the 'get_unique_animation_names' method.
        /// </summary>
        public static readonly StringName GetUniqueAnimationNames = "get_unique_animation_names";
        /// <summary>
        /// Cached name for the 'set_unique_animation_names' method.
        /// </summary>
        public static readonly StringName SetUniqueAnimationNames = "set_unique_animation_names";
        /// <summary>
        /// Cached name for the 'get_skeletons' method.
        /// </summary>
        public static readonly StringName GetSkeletons = "get_skeletons";
        /// <summary>
        /// Cached name for the 'set_skeletons' method.
        /// </summary>
        public static readonly StringName SetSkeletons = "set_skeletons";
        /// <summary>
        /// Cached name for the 'get_create_animations' method.
        /// </summary>
        public static readonly StringName GetCreateAnimations = "get_create_animations";
        /// <summary>
        /// Cached name for the 'set_create_animations' method.
        /// </summary>
        public static readonly StringName SetCreateAnimations = "set_create_animations";
        /// <summary>
        /// Cached name for the 'get_import_as_skeleton_bones' method.
        /// </summary>
        public static readonly StringName GetImportAsSkeletonBones = "get_import_as_skeleton_bones";
        /// <summary>
        /// Cached name for the 'set_import_as_skeleton_bones' method.
        /// </summary>
        public static readonly StringName SetImportAsSkeletonBones = "set_import_as_skeleton_bones";
        /// <summary>
        /// Cached name for the 'get_animations' method.
        /// </summary>
        public static readonly StringName GetAnimations = "get_animations";
        /// <summary>
        /// Cached name for the 'set_animations' method.
        /// </summary>
        public static readonly StringName SetAnimations = "set_animations";
        /// <summary>
        /// Cached name for the 'get_scene_node' method.
        /// </summary>
        public static readonly StringName GetSceneNode = "get_scene_node";
        /// <summary>
        /// Cached name for the 'get_node_index' method.
        /// </summary>
        public static readonly StringName GetNodeIndex = "get_node_index";
        /// <summary>
        /// Cached name for the 'get_additional_data' method.
        /// </summary>
        public static readonly StringName GetAdditionalData = "get_additional_data";
        /// <summary>
        /// Cached name for the 'set_additional_data' method.
        /// </summary>
        public static readonly StringName SetAdditionalData = "set_additional_data";
        /// <summary>
        /// Cached name for the 'get_handle_binary_image_mode' method.
        /// </summary>
        public static readonly StringName GetHandleBinaryImageMode = "get_handle_binary_image_mode";
        /// <summary>
        /// Cached name for the 'set_handle_binary_image_mode' method.
        /// </summary>
        public static readonly StringName SetHandleBinaryImageMode = "set_handle_binary_image_mode";
        /// <summary>
        /// Cached name for the 'set_bake_fps' method.
        /// </summary>
        public static readonly StringName SetBakeFps = "set_bake_fps";
        /// <summary>
        /// Cached name for the 'get_bake_fps' method.
        /// </summary>
        public static readonly StringName GetBakeFps = "get_bake_fps";
        /// <summary>
        /// Cached name for the 'get_handle_binary_image' method.
        /// </summary>
        public static readonly StringName GetHandleBinaryImage = "get_handle_binary_image";
        /// <summary>
        /// Cached name for the 'set_handle_binary_image' method.
        /// </summary>
        public static readonly StringName SetHandleBinaryImage = "set_handle_binary_image";
        /// <summary>
        /// Cached name for the 'get_extract_prefix' method.
        /// </summary>
        public static readonly StringName GetExtractPrefix = "get_extract_prefix";
        /// <summary>
        /// Cached name for the 'get_extract_path' method.
        /// </summary>
        public static readonly StringName GetExtractPath = "get_extract_path";
        /// <summary>
        /// Cached name for the 'get_discard_meshes_and_materials' method.
        /// </summary>
        public static readonly StringName GetDiscardMeshesAndMaterials = "get_discard_meshes_and_materials";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
