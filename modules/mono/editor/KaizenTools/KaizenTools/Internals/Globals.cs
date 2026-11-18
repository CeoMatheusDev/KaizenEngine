using Kaizen;
using Kaizen.NativeInterop;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace KaizenTools.Internals
{
    public static class Globals
    {
        public static float EditorScale => Internal.kaizen_icall_Globals_EditorScale();

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Variant GlobalDef(string setting, Variant defaultValue, bool restartIfChanged = false)
        {
            using kaizen_string settingIn = Marshaling.ConvertStringToNative(setting);
            using kaizen_variant defaultValueIn = defaultValue.CopyNativeVariant();
            Internal.kaizen_icall_Globals_GlobalDef(settingIn, defaultValueIn, restartIfChanged,
                out kaizen_variant result);
            return Variant.CreateTakingOwnershipOfDisposableValue(result);
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public static Variant EditorDef(string setting, Variant defaultValue, bool restartIfChanged = false)
        {
            using kaizen_string settingIn = Marshaling.ConvertStringToNative(setting);
            using kaizen_variant defaultValueIn = defaultValue.CopyNativeVariant();
            Internal.kaizen_icall_Globals_EditorDef(settingIn, defaultValueIn, restartIfChanged,
                out kaizen_variant result);
            return Variant.CreateTakingOwnershipOfDisposableValue(result);
        }

        public static Shortcut EditorDefShortcut(string setting, string name, Key keycode = Key.None, bool physical = false)
        {
            using kaizen_string settingIn = Marshaling.ConvertStringToNative(setting);
            using kaizen_string nameIn = Marshaling.ConvertStringToNative(name);
            Internal.kaizen_icall_Globals_EditorDefShortcut(settingIn, nameIn, keycode, physical.ToKaizenBool(), out kaizen_variant result);
            return (Shortcut)Variant.CreateTakingOwnershipOfDisposableValue(result);
        }

        public static Shortcut EditorGetShortcut(string setting)
        {
            using kaizen_string settingIn = Marshaling.ConvertStringToNative(setting);
            Internal.kaizen_icall_Globals_EditorGetShortcut(settingIn, out kaizen_variant result);
            return (Shortcut)Variant.CreateTakingOwnershipOfDisposableValue(result);
        }

        public static void EditorShortcutOverride(string setting, string feature, Key keycode = Key.None, bool physical = false)
        {
            using kaizen_string settingIn = Marshaling.ConvertStringToNative(setting);
            using kaizen_string featureIn = Marshaling.ConvertStringToNative(feature);
            Internal.kaizen_icall_Globals_EditorShortcutOverride(settingIn, featureIn, keycode, physical.ToKaizenBool());
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static string TTR(this string text)
        {
            using kaizen_string textIn = Marshaling.ConvertStringToNative(text);
            Internal.kaizen_icall_Globals_TTR(textIn, out kaizen_string dest);
            using (dest)
                return Marshaling.ConvertStringToManaged(dest);
        }
    }
}
