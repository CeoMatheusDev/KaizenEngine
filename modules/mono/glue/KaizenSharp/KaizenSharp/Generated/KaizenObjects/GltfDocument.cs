namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>GLTFDocument supports reading data from a glTF file, buffer, or Kaizen scene. This data can then be written to the filesystem, buffer, or used to create a Kaizen scene.</para>
/// <para>All of the data in a glTF scene is stored in the <see cref="Kaizen.GltfState"/> class. GLTFDocument processes state objects, but does not contain any scene data itself. GLTFDocument has member variables to store export configuration settings such as the image format, but is otherwise stateless. Multiple scenes can be processed with the same settings using the same GLTFDocument object and different <see cref="Kaizen.GltfState"/> objects.</para>
/// <para>GLTFDocument can be extended with arbitrary functionality by extending the <see cref="Kaizen.GltfDocumentExtension"/> class and registering it with GLTFDocument via <see cref="Kaizen.GltfDocument.RegisterGltfDocumentExtension(GltfDocumentExtension, bool)"/>. This allows for custom data to be imported and exported.</para>
/// </summary>
[KaizenClassName("GLTFDocument")]
public partial class GltfDocument : Resource
{
    public enum RootNodeModeEnum : long
    {
        /// <summary>
        /// <para>Treat the Kaizen scene's root node as the root node of the glTF file, and mark it as the single root node via the <c>KAIZEN_single_root</c> glTF extension. This will be parsed the same as <see cref="Kaizen.GltfDocument.RootNodeModeEnum.KeepRoot"/> if the implementation does not support <c>KAIZEN_single_root</c>.</para>
        /// </summary>
        SingleRoot = 0,
        /// <summary>
        /// <para>Treat the Kaizen scene's root node as the root node of the glTF file, but do not mark it as anything special. An extra root node will be generated when importing into Kaizen. This uses only vanilla glTF features. This is equivalent to the behavior in Kaizen 4.1 and earlier.</para>
        /// </summary>
        KeepRoot = 1,
        /// <summary>
        /// <para>Treat the Kaizen scene's root node as the name of the glTF scene, and add all of its children as root nodes of the glTF file. This uses only vanilla glTF features. This avoids an extra root node, but only the name of the Kaizen scene's root node will be preserved, as it will not be saved as a node.</para>
        /// </summary>
        MultiRoot = 2
    }

    public enum TextureMapModeEnum : long
    {
        /// <summary>
        /// <para>Import the texture maps in the glTF file as they are, without trying to fit them into specific texture slots suitable for Kaizen's built-in materials. This may be desirable if using the glTF file with custom shaders, but may not display correctly with Kaizen's built-in materials. This is equivalent to the behavior in Kaizen 4.6 and earlier.</para>
        /// </summary>
        DoNotRemap = 0,
        /// <summary>
        /// <para>Import the texture maps in the glTF file remapped to the most suitable texture slots based on Kaizen's <see cref="Kaizen.StandardMaterial3D"/> class. This is the default behavior.</para>
        /// </summary>
        RemapToStandardMaterial = 1
    }

    public enum VisibilityModeEnum : long
    {
        /// <summary>
        /// <para>If the scene contains any non-visible nodes, include them, mark them as non-visible with <c>KHR_node_visibility</c>, and require that importers respect their non-visibility. Downside: If the importer does not support <c>KHR_node_visibility</c>, the file cannot be imported.</para>
        /// </summary>
        IncludeRequired = 0,
        /// <summary>
        /// <para>If the scene contains any non-visible nodes, include them, mark them as non-visible with <c>KHR_node_visibility</c>, and do not impose any requirements on importers. Downside: If the importer does not support <c>KHR_node_visibility</c>, invisible objects will be visible.</para>
        /// </summary>
        IncludeOptional = 1,
        /// <summary>
        /// <para>If the scene contains any non-visible nodes, do not include them in the export. This is the same as the behavior in Kaizen 4.4 and earlier. Downside: Invisible nodes will not exist in the exported file.</para>
        /// </summary>
        Exclude = 2
    }

    [System.Flags]
    public enum ImportFlags : long
    {
        /// <summary>
        /// <para>If <see langword="true"/>, generate vertex tangents using <a href="http://www.mikktspace.com/">Mikktspace</a> if the input meshes don't have tangent data. When possible, it's recommended to let the 3D modeling software generate tangents on export instead of relying on this option. Tangents are required for correct display of normal and height maps, along with any material/shader features that require tangents.</para>
        /// <para>If you don't need material features that require tangents, disabling this can reduce output file size and speed up importing if the source 3D file doesn't contain tangents.</para>
        /// </summary>
        GenerateTangentArrays = 8,
        /// <summary>
        /// <para>If checked, use named <see cref="Kaizen.Skin"/>s for animation. The <see cref="Kaizen.MeshInstance3D"/> node contains 3 properties of relevance here: a skeleton <see cref="Kaizen.NodePath"/> pointing to the <see cref="Kaizen.Skeleton3D"/> node (usually <c>..</c>), a mesh, and a skin:</para>
        /// <para>- The <see cref="Kaizen.Skeleton3D"/> node contains a list of bones with names, their pose and rest, a name, and a parent bone.</para>
        /// <para>- The mesh is all of the raw vertex data needed to display a mesh. In terms of the mesh, it knows how vertices are weight-painted and uses some internal numbering often imported from 3D modeling software.</para>
        /// <para>- The skin contains the information necessary to bind this mesh onto this Skeleton3D. For each of the internal bone IDs chosen by the 3D modeling software, it contains two things. Firstly, a matrix known as the Bind Pose Matrix, Inverse Bind Matrix, or IBM for short. Secondly, the <see cref="Kaizen.Skin"/> contains each bone's name (if this flag is enabled), or the bone's index within the <see cref="Kaizen.Skeleton3D"/> list (if this flag is disabled).</para>
        /// <para>Together, this information is enough to tell Kaizen how to use the bone poses in the <see cref="Kaizen.Skeleton3D"/> node to render the mesh from each <see cref="Kaizen.MeshInstance3D"/>. Note that each <see cref="Kaizen.MeshInstance3D"/> may share binds, as is common in models exported from Blender, or each <see cref="Kaizen.MeshInstance3D"/> may use a separate <see cref="Kaizen.Skin"/> object, as is common in models exported from other tools such as Maya.</para>
        /// </summary>
        UseNamedSkinBinds = 16,
        /// <summary>
        /// <para>Ignore meshes and materials on import. When importing a scene as an <see cref="Kaizen.AnimationLibrary"/>, this flag is always enabled.</para>
        /// </summary>
        DiscardMeshesAndMaterials = 32,
        /// <summary>
        /// <para>If <see langword="true"/>, mesh compression will not be used. Consider enabling if you notice blocky artifacts in your mesh normals or UVs, or if you have meshes that are larger than a few thousand meters in each direction.</para>
        /// </summary>
        ForceDisableMeshCompression = 64
    }

    /// <summary>
    /// <para>The user-friendly name of the export image format. This is used when exporting the glTF file, including writing to a file and writing to a byte array.</para>
    /// <para>By default, Kaizen allows the following options: "None", "PNG", "JPEG", "Lossless WebP", and "Lossy WebP". Support for more image formats can be added in <see cref="Kaizen.GltfDocumentExtension"/> classes. A single extension class can provide multiple options for the specific format to use, or even an option that uses multiple formats at once.</para>
    /// </summary>
    public string ImageFormat
    {
        get
        {
            return GetImageFormat();
        }
        set
        {
            SetImageFormat(value);
        }
    }

    /// <summary>
    /// <para>If <see cref="Kaizen.GltfDocument.ImageFormat"/> is a lossy image format, this determines the lossy quality of the image. On a range of <c>0.0</c> to <c>1.0</c>, where <c>0.0</c> is the lowest quality and <c>1.0</c> is the highest quality. A lossy quality of <c>1.0</c> is not the same as lossless.</para>
    /// </summary>
    public float LossyQuality
    {
        get
        {
            return GetLossyQuality();
        }
        set
        {
            SetLossyQuality(value);
        }
    }

    /// <summary>
    /// <para>The user-friendly name of the fallback image format. This is used when exporting the glTF file, including writing to a file and writing to a byte array.</para>
    /// <para>This property may only be one of "None", "PNG", or "JPEG", and is only used when the <see cref="Kaizen.GltfDocument.ImageFormat"/> is not one of "None", "PNG", or "JPEG". If having multiple extension image formats is desired, that can be done using a <see cref="Kaizen.GltfDocumentExtension"/> class - this property only covers the use case of providing a base glTF fallback image when using a custom image format.</para>
    /// </summary>
    public string FallbackImageFormat
    {
        get
        {
            return GetFallbackImageFormat();
        }
        set
        {
            SetFallbackImageFormat(value);
        }
    }

    /// <summary>
    /// <para>The quality of the fallback image, if any. For PNG files, this downscales the image on both dimensions by this factor. For JPEG files, this is the lossy quality of the image. A low value is recommended, since including multiple high quality images in a glTF file defeats the file size gains of using a more efficient image format.</para>
    /// </summary>
    public float FallbackImageQuality
    {
        get
        {
            return GetFallbackImageQuality();
        }
        set
        {
            SetFallbackImageQuality(value);
        }
    }

    /// <summary>
    /// <para>How to process the root node during export. The default and recommended value is <see cref="Kaizen.GltfDocument.RootNodeModeEnum.SingleRoot"/>.</para>
    /// <para><b>Note:</b> Regardless of how the glTF file is exported, when importing, the root node type and name can be overridden in the scene import settings tab.</para>
    /// </summary>
    public GltfDocument.RootNodeModeEnum RootNodeMode
    {
        get
        {
            return GetRootNodeMode();
        }
        set
        {
            SetRootNodeMode(value);
        }
    }

    /// <summary>
    /// <para>How to handle texture maps during import. The default and recommended value is <see cref="Kaizen.GltfDocument.TextureMapModeEnum.RemapToStandardMaterial"/>, which automatically remaps from glTF's flexible texture map system to the more specific texture map slots in Kaizen's <see cref="Kaizen.StandardMaterial3D"/> class. Alternatively, <see cref="Kaizen.GltfDocument.TextureMapModeEnum.DoNotRemap"/> can be used to preserve the original texture maps from the glTF file, which may be desirable if using the glTF file with custom shaders, but may not display correctly with Kaizen's built-in materials.</para>
    /// </summary>
    public GltfDocument.TextureMapModeEnum TextureMapMode
    {
        get
        {
            return GetTextureMapMode();
        }
        set
        {
            SetTextureMapMode(value);
        }
    }

    /// <summary>
    /// <para>How to deal with node visibility during export. This setting does nothing if all nodes are visible. The default and recommended value is <see cref="Kaizen.GltfDocument.VisibilityModeEnum.IncludeRequired"/>, which uses the <c>KHR_node_visibility</c> extension.</para>
    /// </summary>
    public GltfDocument.VisibilityModeEnum VisibilityMode
    {
        get
        {
            return GetVisibilityMode();
        }
        set
        {
            SetVisibilityMode(value);
        }
    }

    private static readonly System.Type CachedType = typeof(GltfDocument);

    private static readonly StringName NativeName = "GLTFDocument";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly unsafe delegate* unmanaged<kaizen_bool, IntPtr> NativeCtor = ClassDB_get_constructor(NativeName);

    public GltfDocument() : this(true)
    {
        unsafe
        {
            ConstructAndInitialize(NativeCtor, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfDocument(IntPtr ptr) : this(true)
    {
        NativePtr = ptr;
        unsafe
        {
            ConstructAndInitialize(null, NativeName, CachedType, refCounted: true);
        }
    }

    internal GltfDocument(bool memoryOwn) : base(memoryOwn) { }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetImageFormat, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetImageFormat(string imageFormat)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind0, KaizenObject.GetPtr(this), imageFormat);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetImageFormat, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetImageFormat()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind1, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetLossyQuality, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetLossyQuality(float lossyQuality)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind2, KaizenObject.GetPtr(this), lossyQuality);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetLossyQuality, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetLossyQuality()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind3, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFallbackImageFormat, 83702148ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFallbackImageFormat(string fallbackImageFormat)
    {
        NativeCalls.kaizen_icall_1_57(MethodBind4, KaizenObject.GetPtr(this), fallbackImageFormat);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFallbackImageFormat, 201670096ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string GetFallbackImageFormat()
    {
        string ret = NativeCalls.kaizen_icall_0_58(MethodBind5, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind6 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetFallbackImageQuality, 373806689ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetFallbackImageQuality(float fallbackImageQuality)
    {
        NativeCalls.kaizen_icall_1_91(MethodBind6, KaizenObject.GetPtr(this), fallbackImageQuality);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind7 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetFallbackImageQuality, 1740695150ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public float GetFallbackImageQuality()
    {
        float ret = NativeCalls.kaizen_icall_0_92(MethodBind7, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind8 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetRootNodeMode, 463633402ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetRootNodeMode(GltfDocument.RootNodeModeEnum rootNodeMode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind8, KaizenObject.GetPtr(this), (int)rootNodeMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind9 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRootNodeMode, 948057992ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public GltfDocument.RootNodeModeEnum GetRootNodeMode()
    {
        GltfDocument.RootNodeModeEnum ret = (GltfDocument.RootNodeModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind9, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind10 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetTextureMapMode, 3144426102ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetTextureMapMode(GltfDocument.TextureMapModeEnum textureMapMode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind10, KaizenObject.GetPtr(this), (int)textureMapMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind11 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetTextureMapMode, 2113256994ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public GltfDocument.TextureMapModeEnum GetTextureMapMode()
    {
        GltfDocument.TextureMapModeEnum ret = (GltfDocument.TextureMapModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind11, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind12 = ClassDB_get_method_with_compatibility(NativeName, MethodName.SetVisibilityMode, 2803579218ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SetVisibilityMode(GltfDocument.VisibilityModeEnum visibilityMode)
    {
        NativeCalls.kaizen_icall_1_38(MethodBind12, KaizenObject.GetPtr(this), (int)visibilityMode);
        global::System.GC.KeepAlive(this);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind13 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetVisibilityMode, 3885445962ul);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public GltfDocument.VisibilityModeEnum GetVisibilityMode()
    {
        GltfDocument.VisibilityModeEnum ret = (GltfDocument.VisibilityModeEnum)NativeCalls.kaizen_icall_0_39(MethodBind13, KaizenObject.GetPtr(this));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind14 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendFromFile, 866380864ul);

    /// <summary>
    /// <para>Takes a path to a glTF file and imports the data at that file path to the given <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter.</para>
    /// <para><b>Note:</b> The <paramref name="basePath"/> tells <see cref="Kaizen.GltfDocument.AppendFromFile(string, GltfState, uint, string)"/> where to find dependencies and can be empty.</para>
    /// </summary>
    public Error AppendFromFile(string path, GltfState state, uint flags = (uint)(0), string basePath = "")
    {
        Error ret = (Error)NativeCalls.kaizen_icall_4_668(MethodBind14, KaizenObject.GetPtr(this), path, KaizenObject.GetPtr(state), flags, basePath);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind15 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendFromBuffer, 1616081266ul);

    /// <summary>
    /// <para>Takes a <see cref="byte"/>[] defining a glTF and imports the data to the given <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter.</para>
    /// <para><b>Note:</b> The <paramref name="basePath"/> tells <see cref="Kaizen.GltfDocument.AppendFromBuffer(byte[], string, GltfState, uint)"/> where to find dependencies and can be empty.</para>
    /// </summary>
    public Error AppendFromBuffer(byte[] bytes, string basePath, GltfState state, uint flags = (uint)(0))
    {
        Error ret = (Error)NativeCalls.kaizen_icall_4_669(MethodBind15, KaizenObject.GetPtr(this), bytes, basePath, KaizenObject.GetPtr(state), flags);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    /// <summary>
    /// <para>Takes a <see cref="byte"/>[] defining a glTF and imports the data to the given <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter.</para>
    /// <para><b>Note:</b> The <paramref name="basePath"/> tells <see cref="Kaizen.GltfDocument.AppendFromBuffer(byte[], string, GltfState, uint)"/> where to find dependencies and can be empty.</para>
    /// </summary>
    public Error AppendFromBuffer(ReadOnlySpan<byte> bytes, string basePath, GltfState state, uint flags)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_4_669(MethodBind15, KaizenObject.GetPtr(this), bytes, basePath, KaizenObject.GetPtr(state), flags);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind16 = ClassDB_get_method_with_compatibility(NativeName, MethodName.AppendFromScene, 1622574258ul);

    /// <summary>
    /// <para>Takes a Kaizen Engine scene node and exports it and its descendants to the given <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter.</para>
    /// </summary>
    public Error AppendFromScene(Node node, GltfState state, uint flags = (uint)(0))
    {
        Error ret = (Error)NativeCalls.kaizen_icall_3_670(MethodBind16, KaizenObject.GetPtr(this), KaizenObject.GetPtr(node), KaizenObject.GetPtr(state), flags);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind17 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GenerateScene, 596118388ul);

    /// <summary>
    /// <para>Takes a <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter and returns a Kaizen Engine scene node.</para>
    /// <para>The <paramref name="bakeFps"/> parameter overrides the bake_fps in <paramref name="state"/>.</para>
    /// </summary>
    public Node GenerateScene(GltfState state, float bakeFps = (float)(30), bool trimming = false, bool removeImmutableTracks = true)
    {
        Node ret = (Node)NativeCalls.kaizen_icall_4_671(MethodBind17, KaizenObject.GetPtr(this), KaizenObject.GetPtr(state), bakeFps, trimming.ToKaizenBool(), removeImmutableTracks.ToKaizenBool());
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind18 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GenerateBuffer, 741783455ul);

    /// <summary>
    /// <para>Takes a <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter and returns a glTF <see cref="byte"/>[].</para>
    /// </summary>
    public byte[] GenerateBuffer(GltfState state)
    {
        byte[] ret = NativeCalls.kaizen_icall_1_666(MethodBind18, KaizenObject.GetPtr(this), KaizenObject.GetPtr(state));
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind19 = ClassDB_get_method_with_compatibility(NativeName, MethodName.WriteToFilesystem, 1784551478ul);

    /// <summary>
    /// <para>Takes a <see cref="Kaizen.GltfState"/> object through the <paramref name="state"/> parameter and writes a glTF file to the filesystem.</para>
    /// <para><b>Note:</b> The extension of the glTF file determines if it is a .glb binary file or a .gltf text file.</para>
    /// </summary>
    public Error WriteToFilesystem(GltfState state, string path)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_2_672(MethodBind19, KaizenObject.GetPtr(this), KaizenObject.GetPtr(state), path);
        global::System.GC.KeepAlive(this);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind20 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ImportObjectModelProperty, 1206708632ul);

    /// <summary>
    /// <para>Determines a mapping between the given glTF Object Model <paramref name="jsonPointer"/> and the corresponding Kaizen node path(s) in the generated Kaizen scene. The details of this mapping are returned in a <see cref="Kaizen.GltfObjectModelProperty"/> object. Additional mappings can be supplied via the <see cref="Kaizen.GltfDocumentExtension._ExportObjectModelProperty(GltfState, NodePath, Node, int, KaizenObject, int)"/> callback method.</para>
    /// </summary>
    public static GltfObjectModelProperty ImportObjectModelProperty(GltfState state, string jsonPointer)
    {
        GltfObjectModelProperty ret = (GltfObjectModelProperty)NativeCalls.kaizen_icall_2_673(MethodBind20, KaizenObject.GetPtr(state), jsonPointer);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind21 = ClassDB_get_method_with_compatibility(NativeName, MethodName.ExportObjectModelProperty, 314209806ul);

    /// <summary>
    /// <para>Determines a mapping between the given Kaizen <paramref name="nodePath"/> and the corresponding glTF Object Model JSON pointer(s) in the generated glTF file. The details of this mapping are returned in a <see cref="Kaizen.GltfObjectModelProperty"/> object. Additional mappings can be supplied via the <see cref="Kaizen.GltfDocumentExtension._ImportObjectModelProperty(GltfState, string[], Kaizen.Collections.Array{NodePath})"/> callback method.</para>
    /// </summary>
    public static GltfObjectModelProperty ExportObjectModelProperty(GltfState state, NodePath nodePath, Node kaizenNode, int gltfNodeIndex)
    {
        GltfObjectModelProperty ret = (GltfObjectModelProperty)NativeCalls.kaizen_icall_4_674(MethodBind21, KaizenObject.GetPtr(state), (kaizen_node_path)(nodePath?.NativeValue ?? default), KaizenObject.GetPtr(kaizenNode), gltfNodeIndex);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind22 = ClassDB_get_method_with_compatibility(NativeName, MethodName.RegisterGltfDocumentExtension, 3752678331ul);

    /// <summary>
    /// <para>Registers the given <see cref="Kaizen.GltfDocumentExtension"/> instance with GLTFDocument. If <paramref name="firstPriority"/> is <see langword="true"/>, this extension will be run first. Otherwise, it will be run last.</para>
    /// <para><b>Note:</b> Like GLTFDocument itself, all GLTFDocumentExtension classes must be stateless in order to function properly. If you need to store data, use the <c>set_additional_data</c> and <c>get_additional_data</c> methods in <see cref="Kaizen.GltfState"/> or <see cref="Kaizen.GltfNode"/>.</para>
    /// </summary>
    public static void RegisterGltfDocumentExtension(GltfDocumentExtension extension, bool firstPriority = false)
    {
        NativeCalls.kaizen_icall_2_675(MethodBind22, KaizenObject.GetPtr(extension), firstPriority.ToKaizenBool());
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind23 = ClassDB_get_method_with_compatibility(NativeName, MethodName.UnregisterGltfDocumentExtension, 2684415758ul);

    /// <summary>
    /// <para>Unregisters the given <see cref="Kaizen.GltfDocumentExtension"/> instance.</para>
    /// </summary>
    public static void UnregisterGltfDocumentExtension(GltfDocumentExtension extension)
    {
        NativeCalls.kaizen_icall_1_676(MethodBind23, KaizenObject.GetPtr(extension));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind24 = ClassDB_get_method_with_compatibility(NativeName, MethodName.GetSupportedGltfExtensions, 2981934095ul);

    /// <summary>
    /// <para>Returns a list of all support glTF extensions, including extensions supported directly by the engine, and extensions supported by user plugins registering <see cref="Kaizen.GltfDocumentExtension"/> classes.</para>
    /// <para><b>Note:</b> If this method is run before a GLTFDocumentExtension is registered, its extensions won't be included in the list. Be sure to only run this method after all extensions are registered. If you run this when the engine starts, consider waiting a frame before calling this method to ensure all extensions are registered.</para>
    /// </summary>
    public static string[] GetSupportedGltfExtensions()
    {
        string[] ret = NativeCalls.kaizen_icall_0_498(MethodBind24);
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
        /// Cached name for the 'image_format' property.
        /// </summary>
        public static readonly StringName ImageFormat = "image_format";
        /// <summary>
        /// Cached name for the 'lossy_quality' property.
        /// </summary>
        public static readonly StringName LossyQuality = "lossy_quality";
        /// <summary>
        /// Cached name for the 'fallback_image_format' property.
        /// </summary>
        public static readonly StringName FallbackImageFormat = "fallback_image_format";
        /// <summary>
        /// Cached name for the 'fallback_image_quality' property.
        /// </summary>
        public static readonly StringName FallbackImageQuality = "fallback_image_quality";
        /// <summary>
        /// Cached name for the 'root_node_mode' property.
        /// </summary>
        public static readonly StringName RootNodeMode = "root_node_mode";
        /// <summary>
        /// Cached name for the 'texture_map_mode' property.
        /// </summary>
        public static readonly StringName TextureMapMode = "texture_map_mode";
        /// <summary>
        /// Cached name for the 'visibility_mode' property.
        /// </summary>
        public static readonly StringName VisibilityMode = "visibility_mode";
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public new class MethodName : Resource.MethodName
    {
        /// <summary>
        /// Cached name for the 'set_image_format' method.
        /// </summary>
        public static readonly StringName SetImageFormat = "set_image_format";
        /// <summary>
        /// Cached name for the 'get_image_format' method.
        /// </summary>
        public static readonly StringName GetImageFormat = "get_image_format";
        /// <summary>
        /// Cached name for the 'set_lossy_quality' method.
        /// </summary>
        public static readonly StringName SetLossyQuality = "set_lossy_quality";
        /// <summary>
        /// Cached name for the 'get_lossy_quality' method.
        /// </summary>
        public static readonly StringName GetLossyQuality = "get_lossy_quality";
        /// <summary>
        /// Cached name for the 'set_fallback_image_format' method.
        /// </summary>
        public static readonly StringName SetFallbackImageFormat = "set_fallback_image_format";
        /// <summary>
        /// Cached name for the 'get_fallback_image_format' method.
        /// </summary>
        public static readonly StringName GetFallbackImageFormat = "get_fallback_image_format";
        /// <summary>
        /// Cached name for the 'set_fallback_image_quality' method.
        /// </summary>
        public static readonly StringName SetFallbackImageQuality = "set_fallback_image_quality";
        /// <summary>
        /// Cached name for the 'get_fallback_image_quality' method.
        /// </summary>
        public static readonly StringName GetFallbackImageQuality = "get_fallback_image_quality";
        /// <summary>
        /// Cached name for the 'set_root_node_mode' method.
        /// </summary>
        public static readonly StringName SetRootNodeMode = "set_root_node_mode";
        /// <summary>
        /// Cached name for the 'get_root_node_mode' method.
        /// </summary>
        public static readonly StringName GetRootNodeMode = "get_root_node_mode";
        /// <summary>
        /// Cached name for the 'set_texture_map_mode' method.
        /// </summary>
        public static readonly StringName SetTextureMapMode = "set_texture_map_mode";
        /// <summary>
        /// Cached name for the 'get_texture_map_mode' method.
        /// </summary>
        public static readonly StringName GetTextureMapMode = "get_texture_map_mode";
        /// <summary>
        /// Cached name for the 'set_visibility_mode' method.
        /// </summary>
        public static readonly StringName SetVisibilityMode = "set_visibility_mode";
        /// <summary>
        /// Cached name for the 'get_visibility_mode' method.
        /// </summary>
        public static readonly StringName GetVisibilityMode = "get_visibility_mode";
        /// <summary>
        /// Cached name for the 'append_from_file' method.
        /// </summary>
        public static readonly StringName AppendFromFile = "append_from_file";
        /// <summary>
        /// Cached name for the 'append_from_buffer' method.
        /// </summary>
        public static readonly StringName AppendFromBuffer = "append_from_buffer";
        /// <summary>
        /// Cached name for the 'append_from_scene' method.
        /// </summary>
        public static readonly StringName AppendFromScene = "append_from_scene";
        /// <summary>
        /// Cached name for the 'generate_scene' method.
        /// </summary>
        public static readonly StringName GenerateScene = "generate_scene";
        /// <summary>
        /// Cached name for the 'generate_buffer' method.
        /// </summary>
        public static readonly StringName GenerateBuffer = "generate_buffer";
        /// <summary>
        /// Cached name for the 'write_to_filesystem' method.
        /// </summary>
        public static readonly StringName WriteToFilesystem = "write_to_filesystem";
        /// <summary>
        /// Cached name for the 'import_object_model_property' method.
        /// </summary>
        public static readonly StringName ImportObjectModelProperty = "import_object_model_property";
        /// <summary>
        /// Cached name for the 'export_object_model_property' method.
        /// </summary>
        public static readonly StringName ExportObjectModelProperty = "export_object_model_property";
        /// <summary>
        /// Cached name for the 'register_gltf_document_extension' method.
        /// </summary>
        public static readonly StringName RegisterGltfDocumentExtension = "register_gltf_document_extension";
        /// <summary>
        /// Cached name for the 'unregister_gltf_document_extension' method.
        /// </summary>
        public static readonly StringName UnregisterGltfDocumentExtension = "unregister_gltf_document_extension";
        /// <summary>
        /// Cached name for the 'get_supported_gltf_extensions' method.
        /// </summary>
        public static readonly StringName GetSupportedGltfExtensions = "get_supported_gltf_extensions";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public new class SignalName : Resource.SignalName
    {
    }
}
