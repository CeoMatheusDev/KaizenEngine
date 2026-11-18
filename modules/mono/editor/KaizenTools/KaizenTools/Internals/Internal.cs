#pragma warning disable IDE1006 // Naming rule violation
// ReSharper disable InconsistentNaming

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Kaizen;
using Kaizen.NativeInterop;
using Kaizen.SourceGenerators.Internal;
using KaizenTools.IdeMessaging.Requests;

namespace KaizenTools.Internals
{
    [GenerateUnmanagedCallbacks(typeof(InternalUnmanagedCallbacks))]
    internal static partial class Internal
    {
        public const string CSharpLanguageType = "CSharpScript";
        public const string CSharpLanguageExtension = ".cs";

        public static string FullExportTemplatesDir
        {
            get
            {
                kaizen_icall_Internal_FullExportTemplatesDir(out kaizen_string dest);
                using (dest)
                    return Marshaling.ConvertStringToManaged(dest);
            }
        }

        public static string SimplifyKaizenPath(this string path) => Kaizen.StringExtensions.SimplifyPath(path);

        public static bool IsMacOSAppBundleInstalled(string bundleId)
        {
            using kaizen_string bundleIdIn = Marshaling.ConvertStringToNative(bundleId);
            return kaizen_icall_Internal_IsMacOSAppBundleInstalled(bundleIdIn);
        }

        public static bool LipOCreateFile(string outputPath, string[] files)
        {
            using kaizen_string outputPathIn = Marshaling.ConvertStringToNative(outputPath);
            using kaizen_packed_string_array filesIn = Marshaling.ConvertSystemArrayToNativePackedStringArray(files);
            return kaizen_icall_Internal_LipOCreateFile(outputPathIn, filesIn);
        }

        public static bool KaizenIs32Bits() => kaizen_icall_Internal_KaizenIs32Bits();

        public static bool KaizenIsRealTDouble() => kaizen_icall_Internal_KaizenIsRealTDouble();

        public static void KaizenMainIteration() => kaizen_icall_Internal_KaizenMainIteration();

        public static bool IsAssembliesReloadingNeeded() => kaizen_icall_Internal_IsAssembliesReloadingNeeded();

        public static void ReloadAssemblies() => kaizen_icall_Internal_ReloadAssemblies();

        public static void EditorDebuggerNodeReloadScripts() => kaizen_icall_Internal_EditorDebuggerNodeReloadScripts();

        public static bool ScriptEditorEdit(Resource resource, int line, int col, bool grabFocus = true) =>
            kaizen_icall_Internal_ScriptEditorEdit(resource.NativeInstance, line, col, grabFocus);

        public static void EditorNodeShowScriptScreen() => kaizen_icall_Internal_EditorNodeShowScriptScreen();

        public static void EditorRunPlay() => kaizen_icall_Internal_EditorRunPlay();

        public static void EditorRunStop() => kaizen_icall_Internal_EditorRunStop();

        public static void EditorPlugin_AddControlToEditorRunBar(Control control) =>
            kaizen_icall_Internal_EditorPlugin_AddControlToEditorRunBar(control.NativeInstance);

        public static void ScriptEditorDebugger_ReloadScripts() =>
            kaizen_icall_Internal_ScriptEditorDebugger_ReloadScripts();

        public static string[] CodeCompletionRequest(CodeCompletionRequest.CompletionKind kind,
            string scriptFile)
        {
            using kaizen_string scriptFileIn = Marshaling.ConvertStringToNative(scriptFile);
            kaizen_icall_Internal_CodeCompletionRequest((int)kind, scriptFileIn, out kaizen_packed_string_array res);
            using (res)
                return Marshaling.ConvertNativePackedStringArrayToSystemArray(res);
        }

        #region Internal

        private static bool initialized = false;

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        internal static unsafe void Initialize(IntPtr unmanagedCallbacks, int unmanagedCallbacksSize)
        {
            if (initialized)
                throw new InvalidOperationException("Already initialized.");
            initialized = true;

            if (unmanagedCallbacksSize != sizeof(InternalUnmanagedCallbacks))
                throw new ArgumentException("Unmanaged callbacks size mismatch.", nameof(unmanagedCallbacksSize));

            _unmanagedCallbacks = Unsafe.AsRef<InternalUnmanagedCallbacks>((void*)unmanagedCallbacks);
        }

        private partial struct InternalUnmanagedCallbacks
        {
        }

        /*
         * IMPORTANT:
         * The order of the methods defined in NativeFuncs must match the order
         * in the array defined at the bottom of 'editor/editor_internal_calls.cpp'.
         */

        public static partial void kaizen_icall_KaizenSharpDirs_ResMetadataDir(out kaizen_string r_dest);

        public static partial void kaizen_icall_KaizenSharpDirs_MonoUserDir(out kaizen_string r_dest);

        public static partial void kaizen_icall_KaizenSharpDirs_BuildLogsDirs(out kaizen_string r_dest);

        public static partial void kaizen_icall_KaizenSharpDirs_DataEditorToolsDir(out kaizen_string r_dest);

        public static partial void kaizen_icall_KaizenSharpDirs_CSharpProjectName(out kaizen_string r_dest);

        public static partial void kaizen_icall_EditorProgress_Create(in kaizen_string task, in kaizen_string label,
            int amount, bool canCancel);

        public static partial void kaizen_icall_EditorProgress_Dispose(in kaizen_string task);

        public static partial bool kaizen_icall_EditorProgress_Step(in kaizen_string task, in kaizen_string state,
            int step,
            bool forceRefresh);

        private static partial void kaizen_icall_Internal_FullExportTemplatesDir(out kaizen_string dest);

        private static partial bool kaizen_icall_Internal_IsMacOSAppBundleInstalled(in kaizen_string bundleId);

        private static partial bool kaizen_icall_Internal_LipOCreateFile(in kaizen_string outputPath, in kaizen_packed_string_array files);

        private static partial bool kaizen_icall_Internal_KaizenIs32Bits();

        private static partial bool kaizen_icall_Internal_KaizenIsRealTDouble();

        private static partial void kaizen_icall_Internal_KaizenMainIteration();

        private static partial bool kaizen_icall_Internal_IsAssembliesReloadingNeeded();

        private static partial void kaizen_icall_Internal_ReloadAssemblies();

        private static partial void kaizen_icall_Internal_EditorDebuggerNodeReloadScripts();

        private static partial bool kaizen_icall_Internal_ScriptEditorEdit(IntPtr resource, int line, int col,
            bool grabFocus);

        private static partial void kaizen_icall_Internal_EditorNodeShowScriptScreen();

        private static partial void kaizen_icall_Internal_EditorRunPlay();

        private static partial void kaizen_icall_Internal_EditorRunStop();

        private static partial void kaizen_icall_Internal_EditorPlugin_AddControlToEditorRunBar(IntPtr p_control);

        private static partial void kaizen_icall_Internal_ScriptEditorDebugger_ReloadScripts();

        private static partial void kaizen_icall_Internal_CodeCompletionRequest(int kind, in kaizen_string scriptFile,
            out kaizen_packed_string_array res);

        public static partial float kaizen_icall_Globals_EditorScale();

        public static partial void kaizen_icall_Globals_GlobalDef(in kaizen_string setting, in kaizen_variant defaultValue,
            bool restartIfChanged, out kaizen_variant result);

        public static partial void kaizen_icall_Globals_EditorDef(in kaizen_string setting, in kaizen_variant defaultValue,
            bool restartIfChanged, out kaizen_variant result);

        public static partial void
            kaizen_icall_Globals_EditorDefShortcut(in kaizen_string setting, in kaizen_string name, Key keycode, kaizen_bool physical, out kaizen_variant result);

        public static partial void
            kaizen_icall_Globals_EditorGetShortcut(in kaizen_string setting, out kaizen_variant result);

        public static partial void
            kaizen_icall_Globals_EditorShortcutOverride(in kaizen_string setting, in kaizen_string feature, Key keycode, kaizen_bool physical);

        public static partial void kaizen_icall_Globals_TTR(in kaizen_string text, out kaizen_string dest);

        public static partial void kaizen_icall_Utils_OS_GetPlatformName(out kaizen_string dest);

        public static partial bool kaizen_icall_Utils_OS_UnixFileHasExecutableAccess(in kaizen_string filePath);

        #endregion
    }
}
