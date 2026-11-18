namespace Kaizen;

using System;
using System.ComponentModel;
using System.Diagnostics;
using Kaizen.NativeInterop;

#nullable disable
/// <summary>
/// <para>A singleton for saving resource types to the filesystem.</para>
/// <para>It uses the many <see cref="Kaizen.ResourceFormatSaver"/> classes registered in the engine (either built-in or from a plugin) to save resource data to text-based (e.g. <c>.tres</c> or <c>.tscn</c>) or binary files (e.g. <c>.res</c> or <c>.scn</c>).</para>
/// </summary>
public static partial class ResourceSaver
{
    [System.Flags]
    public enum SaverFlags : long
    {
        /// <summary>
        /// <para>No resource saving option.</para>
        /// </summary>
        None = 0,
        /// <summary>
        /// <para>Save the resource with a path relative to the scene which uses it.</para>
        /// </summary>
        RelativePaths = 1,
        /// <summary>
        /// <para>Bundles external resources.</para>
        /// </summary>
        BundleResources = 2,
        /// <summary>
        /// <para>Changes the <see cref="Kaizen.Resource.ResourcePath"/> of the saved resource to match its new location.</para>
        /// </summary>
        ChangePath = 4,
        /// <summary>
        /// <para>Do not save editor-specific metadata (identified by their <c>__editor</c> prefix).</para>
        /// </summary>
        OmitEditorProperties = 8,
        /// <summary>
        /// <para>Save as big endian (see <see cref="Kaizen.FileAccess.BigEndian"/>).</para>
        /// </summary>
        SaveBigEndian = 16,
        /// <summary>
        /// <para>Compress the resource on save using <see cref="Kaizen.FileAccess.CompressionMode.Zstd"/>. Only available for binary resource types.</para>
        /// </summary>
        Compress = 32,
        /// <summary>
        /// <para>Take over the paths of the saved subresources (see <see cref="Kaizen.Resource.TakeOverPath(string)"/>).</para>
        /// </summary>
        ReplaceSubresourcePaths = 64
    }

    private static readonly StringName NativeName = "ResourceSaver";

    private static ResourceSaverInstance singleton;

    public static ResourceSaverInstance Singleton =>
        singleton ??= (ResourceSaverInstance)InteropUtils.EngineGetSingleton("ResourceSaver");

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind0 = KaizenObject.ClassDB_get_method_with_compatibility(NativeName, MethodName.Save, 2983274697ul);

    /// <summary>
    /// <para>Saves a resource to disk to the given path, using a <see cref="Kaizen.ResourceFormatSaver"/> that recognizes the resource object. If <paramref name="path"/> is empty, <see cref="Kaizen.ResourceSaver"/> will try to use <see cref="Kaizen.Resource.ResourcePath"/>.</para>
    /// <para>The <paramref name="flags"/> bitmask can be specified to customize the save behavior.</para>
    /// <para>Returns <see cref="Kaizen.Error.Ok"/> on success.</para>
    /// <para><b>Note:</b> When the project is running, any generated UID associated with the resource will not be saved as the required code is only executed in editor mode.</para>
    /// </summary>
    public static Error Save(Resource resource, string path = "", ResourceSaver.SaverFlags flags = (ResourceSaver.SaverFlags)(0))
    {
        Error ret = (Error)NativeCalls.kaizen_icall_3_1286(MethodBind0, KaizenObject.GetPtr(Singleton), KaizenObject.GetPtr(resource), path, (int)flags);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind1 = KaizenObject.ClassDB_get_method_with_compatibility(NativeName, MethodName.SetUid, 993915709ul);

    /// <summary>
    /// <para>Sets the UID of the given <paramref name="resource"/> path to <paramref name="uid"/>. You can generate a new UID using <see cref="Kaizen.ResourceUid.CreateId()"/>.</para>
    /// <para>Since resources will normally get a UID automatically, this method is only useful in very specific cases.</para>
    /// </summary>
    public static Error SetUid(string resource, long uid)
    {
        Error ret = (Error)NativeCalls.kaizen_icall_2_1287(MethodBind1, KaizenObject.GetPtr(Singleton), resource, uid);
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind2 = KaizenObject.ClassDB_get_method_with_compatibility(NativeName, MethodName.GetRecognizedExtensions, 4223597960ul);

    /// <summary>
    /// <para>Returns the list of extensions available for saving a resource of a given type.</para>
    /// </summary>
    public static string[] GetRecognizedExtensions(Resource type)
    {
        string[] ret = NativeCalls.kaizen_icall_1_1288(MethodBind2, KaizenObject.GetPtr(Singleton), KaizenObject.GetPtr(type));
        return ret;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind3 = KaizenObject.ClassDB_get_method_with_compatibility(NativeName, MethodName.AddResourceFormatSaver, 362894272ul);

    /// <summary>
    /// <para>Registers a new <see cref="Kaizen.ResourceFormatSaver"/>. The ResourceSaver will use the ResourceFormatSaver as described in <see cref="Kaizen.ResourceSaver.Save(Resource, string, ResourceSaver.SaverFlags)"/>.</para>
    /// <para>This method is performed implicitly for ResourceFormatSavers written in GDScript (see <see cref="Kaizen.ResourceFormatSaver"/> for more information).</para>
    /// </summary>
    public static void AddResourceFormatSaver(ResourceFormatSaver formatSaver, bool atFront = false)
    {
        NativeCalls.kaizen_icall_2_526(MethodBind3, KaizenObject.GetPtr(Singleton), KaizenObject.GetPtr(formatSaver), atFront.ToKaizenBool());
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind4 = KaizenObject.ClassDB_get_method_with_compatibility(NativeName, MethodName.RemoveResourceFormatSaver, 3373026878ul);

    /// <summary>
    /// <para>Unregisters the given <see cref="Kaizen.ResourceFormatSaver"/>.</para>
    /// </summary>
    public static void RemoveResourceFormatSaver(ResourceFormatSaver formatSaver)
    {
        NativeCalls.kaizen_icall_1_56(MethodBind4, KaizenObject.GetPtr(Singleton), KaizenObject.GetPtr(formatSaver));
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly IntPtr MethodBind5 = KaizenObject.ClassDB_get_method_with_compatibility(NativeName, MethodName.GetResourceIdForPath, 150756522ul);

    /// <summary>
    /// <para>Returns the resource ID for the given path. If <paramref name="generate"/> is <see langword="true"/>, a new resource ID will be generated if one for the path is not found. If <paramref name="generate"/> is <see langword="false"/> and the path is not found, <see cref="Kaizen.ResourceUid.InvalidId"/> is returned.</para>
    /// </summary>
    public static long GetResourceIdForPath(string path, bool generate = false)
    {
        long ret = NativeCalls.kaizen_icall_2_1289(MethodBind5, KaizenObject.GetPtr(Singleton), path, generate.ToKaizenBool());
        return ret;
    }

    /// <summary>
    /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
    /// </summary>
    public class PropertyName
    {
    }

    /// <summary>
    /// Cached StringNames for the methods contained in this class, for fast lookup.
    /// </summary>
    public class MethodName
    {
        /// <summary>
        /// Cached name for the 'save' method.
        /// </summary>
        public static readonly StringName Save = "save";
        /// <summary>
        /// Cached name for the 'set_uid' method.
        /// </summary>
        public static readonly StringName SetUid = "set_uid";
        /// <summary>
        /// Cached name for the 'get_recognized_extensions' method.
        /// </summary>
        public static readonly StringName GetRecognizedExtensions = "get_recognized_extensions";
        /// <summary>
        /// Cached name for the 'add_resource_format_saver' method.
        /// </summary>
        public static readonly StringName AddResourceFormatSaver = "add_resource_format_saver";
        /// <summary>
        /// Cached name for the 'remove_resource_format_saver' method.
        /// </summary>
        public static readonly StringName RemoveResourceFormatSaver = "remove_resource_format_saver";
        /// <summary>
        /// Cached name for the 'get_resource_id_for_path' method.
        /// </summary>
        public static readonly StringName GetResourceIdForPath = "get_resource_id_for_path";
    }

    /// <summary>
    /// Cached StringNames for the signals contained in this class, for fast lookup.
    /// </summary>
    public class SignalName
    {
    }
}
