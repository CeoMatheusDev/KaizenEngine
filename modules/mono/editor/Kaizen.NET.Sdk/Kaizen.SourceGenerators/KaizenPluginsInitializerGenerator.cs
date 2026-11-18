using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Kaizen.SourceGenerators
{
    [Generator]
    public class KaizenPluginsInitializerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.IsKaizenToolsProject() || context.IsKaizenSourceGeneratorDisabled("KaizenPluginsInitializer"))
                return;

            string source =
                @"using System;
using System.Runtime.InteropServices;
using Kaizen.Bridge;
using Kaizen.NativeInterop;

namespace KaizenPlugins.Game
{
    internal static partial class Main
    {
        [UnmanagedCallersOnly(EntryPoint = ""kaizensharp_game_main_init"")]
        private static kaizen_bool InitializeFromGameProject(IntPtr kaizenDllHandle, IntPtr outManagedCallbacks,
            IntPtr unmanagedCallbacks, int unmanagedCallbacksSize)
        {
            try
            {
                NativeFuncs.Initialize(unmanagedCallbacks, unmanagedCallbacksSize);

                Marshaling.Initialize();

                DllImportResolver dllImportResolver = new KaizenDllImportResolver(kaizenDllHandle).OnResolveDllImport;

                var coreApiAssembly = typeof(global::Kaizen.KaizenObject).Assembly;

                NativeLibrary.SetDllImportResolver(coreApiAssembly, dllImportResolver);

                ManagedCallbacks.Create(outManagedCallbacks);

                ScriptManagerBridge.LookupScriptsInAssembly(typeof(global::KaizenPlugins.Game.Main).Assembly);

                return kaizen_bool.True;
            }
            catch (Exception e)
            {
                global::System.Console.Error.WriteLine(e);
                return false.ToKaizenBool();
            }
        }
    }
}
";

            context.AddSource("KaizenPlugins.Game.generated",
                SourceText.From(source, Encoding.UTF8));
        }
    }
}
