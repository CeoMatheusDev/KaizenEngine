# Fish completion for the Kaizen editor
# To use it, install this file in `~/.config/fish/completions` then restart your shell.
# You can also `source` this file directly in your shell startup file.
#
# Copyright (c) 2014-present Kaizen Engine contributors (see AUTHORS.md).
# Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

function kaizen_rendering_method_args
    # Use a function instead of a fixed string to customize the argument descriptions.
    echo -e "forward_plus\tHigh-end desktop renderer"
    echo -e "mobile\tHigh-end mobile/desktop renderer"
    echo -e "gl_compatibility\tLow-end desktop, mobile and web renderer"
    echo -e "dummy\tDummy renderer"
end

function kaizen_rendering_driver_args
    # Use a function instead of a fixed string to customize the argument descriptions.
    echo -e "vulkan\tVulkan renderer"
    echo -e "d3d12\tDirect3D 12 renderer"
    echo -e "metal\tMetal renderer"
    echo -e "opengl3\tOpenGL 3.3 renderer"
    echo -e "opengl3_es\tOpenGL ES 3.0 renderer"
    echo -e "opengl3_angle\tOpenGL ES 3.0 renderer using ANGLE"
    echo -e "dummy\tDummy renderer"
end

# Erase existing completions for Kaizen.
complete -c kaizen -e

# General options:
complete -c kaizen -s h -l help -d "Display the full help message"
complete -c kaizen -l version -d "Display the version string"
complete -c kaizen -s v -l verbose -d "Use verbose stdout mode"
complete -c kaizen -s q -l quiet -d "Quiet mode, silences stdout messages (errors are still displayed)"
complete -c kaizen -l no-header -d "Do not print engine version and rendering driver/method header on startup"

# Run options:
complete -c kaizen -s e -l editor -d "Start the editor instead of running the scene"
complete -c kaizen -s p -l project-manager -d "Start the project manager, even if a project is auto-detected"
complete -c kaizen -l recovery-mode -d "Start the editor in recovery mode, which disables features that can typically cause startup crashes"
complete -c kaizen -l debug-server -d "Start the editor debug server (<protocol>://<host/IP>[:<port>] address)" -x
complete -c kaizen -l dap-port -d "Use the specified port for the GDScript Debug Adapter Protocol" -x
complete -c kaizen -l lsp-port -d "Use the specified port for the GDScript Language Server Protocol" -x
complete -c kaizen -l quit -d "Quit after the first iteration"
complete -c kaizen -l quit-after -d "Quit after the given number of iterations (0 to disable)" -x
complete -c kaizen -s l -l language -d "Use a specific locale (<locale> being a two-letter code)" -x
complete -c kaizen -l path -d "Path to a project (<directory> must contain a 'project.kaizen' file)" -r
complete -c kaizen -l scene -d "Path or UID of a scene in the project that should be started" -r
complete -c kaizen -l main-pack -d "Path to a pack (.pck) file to load" -r
complete -c kaizen -l render-thread -d "Set the render thread mode" -x -a "safe separate"
complete -c kaizen -l remote-fs -d "Use a remote filesystem (<host/IP>[:<port>] address)" -x
complete -c kaizen -l remote-fs-password -d "Password for remote filesystem" -x
complete -c kaizen -l audio-driver -d "Set the audio driver" -x -a "PulseAudio ALSA CoreAudio WASAPI XAudio2 Dummy"
complete -c kaizen -l display-driver -d "Set the display driver" -x -a "x11 wayland macos windows headless"
complete -c kaizen -l audio-output-latency -d "Override audio output latency in milliseconds (default is 15 ms)" -x
complete -c kaizen -l rendering-method -d "Set the renderer" -x -a "(kaizen_rendering_method_args)"
complete -c kaizen -l rendering-driver -d "Set the rendering driver" -x -a "(kaizen_rendering_driver_args)"
complete -c kaizen -l gpu-index -d "Use a specific GPU (run with --verbose to get available device list)" -x
complete -c kaizen -l text-driver -d "Set the text driver" -x
complete -c kaizen -l tablet-driver -d "Set the pen tablet input driver" -x
complete -c kaizen -l headless -d "Enable headless mode (--display-driver headless --audio-driver Dummy). Useful for servers and with --script"
complete -c kaizen -l log-file -d "Write output/error log to the specified path instead of the default location defined by the project" -r
complete -c kaizen -l write-movie -d "Write a video to the specified path (usually with .avi or .png extension). --fixed-fps is forced when enabled" -r

# Display options:
complete -c kaizen -s f -l fullscreen -d "Request fullscreen mode"
complete -c kaizen -s m -l maximized -d "Request a maximized window"
complete -c kaizen -s w -l windowed -d "Request windowed mode"
complete -c kaizen -s t -l always-on-top -d "Request an always-on-top window"
complete -c kaizen -l resolution -d "Request window resolution" -x
complete -c kaizen -l position -d "Request window position" -x
complete -c kaizen -l screen -d "Request window screen" -x
complete -c kaizen -l single-window -d "Use a single window (no separate subwindows)"
complete -c kaizen -l xr-mode -d "Select Extended Reality (XR) mode" -x -a "default off on"
complete -c kaizen -l wid -d "Request parented to window" -x
complete -c kaizen -l accessibility -d "Select accessibility mode" -x -a "auto always disabled"
complete -c kaizen -l accessibility-driver -d "Select accessibility driver" -x -a "accesskit dummy"

# Debug options:
complete -c kaizen -s d -l debug -d "Debug (local stdout debugger)"
complete -c kaizen -s b -l breakpoints -d "Specify the breakpoint list as source:line comma-separated pairs, no spaces (use %20 instead)" -x
complete -c kaizen -l skip-breakpoints -d "If debugger is connected, skips all breakpoints"
complete -c kaizen -l ignore-error-breaks -d "If debugger is connected, prevents sending error breakpoints"
complete -c kaizen -l profiling -d "Enable profiling in the script debugger"
complete -c kaizen -l gpu-profile -d "Show a GPU profile of the tasks that took the most time during frame rendering"
complete -c kaizen -l gpu-validation -d "Enable graphics API validation layers for debugging"
complete -c kaizen -l gpu-abort -d "Abort on graphics API usage errors (usually validation layer errors)"
complete -c kaizen -l generate-spirv-debug-info -d "Generate SPIR-V debug information (Vulkan only)"
complete -c kaizen -l clear-shader-cache -d "Clear the shader_cache directory at launch, so it's re-generated"
complete -c kaizen -l extra-gpu-memory-tracking -d "Enable additional memory tracking (Vulkan only)"
complete -c kaizen -l accurate-breadcrumbs -d "Force barriers between breadcrumbs (Vulkan only)"
complete -c kaizen -l remote-debug -d "Enable remote debugging (<protocol>://<host/IP>[:<port>] address)" -x
complete -c kaizen -l debug-collisions -d "Show collision shapes when running the scene"
complete -c kaizen -l debug-paths -d "Show path lines when running the scene"
complete -c kaizen -l debug-navigation -d "Show navigation polygons when running the scene"
complete -c kaizen -l debug-avoidance -d "Show navigation avoidance debug visuals when running the scene"
complete -c kaizen -l debug-stringnames -d "Print all StringName allocations to stdout when the engine quits"
complete -c kaizen -l debug-canvas-item-redraw -d "Display a rectangle each time a canvas item requests a redraw"
complete -c kaizen -l debug-mute-audio -d "Mute all audio output when running the scene"
complete -c kaizen -l max-fps -d "Set a maximum number of frames per second rendered (can be used to limit power usage), a value of 0 results in unlimited framerate" -x
complete -c kaizen -l frame-delay -d "Simulate high CPU load (delay each frame by the given number of milliseconds)" -x
complete -c kaizen -l time-scale -d "Force time scale (higher values are faster, 1.0 is normal speed)" -x
complete -c kaizen -l disable-vsync -d "Disable vertical synchronization, even if enabled in the project settings"
complete -c kaizen -l disable-render-loop -d "Disable render loop so rendering only occurs when called explicitly from script"
complete -c kaizen -l disable-crash-handler -d "Disable crash handler when supported by the platform code"
complete -c kaizen -l fixed-fps -d "Force a fixed number of frames per second (this setting disables real-time synchronization)" -x
complete -c kaizen -l delta-smoothing -d "Enable or disable frame delta smoothing" -x -a "enable disable"
complete -c kaizen -l print-fps -d "Print the frames per second to the stdout"
complete -c kaizen -l single-threaded-scene -d "Force scene tree to run in single-threaded mode"
complete -c kaizen -l editor-pseudolocalization -d "Enable pseudolocalization for the editor and the project manager"

# Standalone tools:
complete -c kaizen -s s -l script -d "Run a script" -r
complete -c kaizen -l main-loop -d "Run a MainLoop specified by its global class name" -x
complete -c kaizen -l check-only -d "Only parse for errors and quit (use with --script)"
complete -c kaizen -l import -d "Start the editor, wait for any resources to be imported, and then quit"
complete -c kaizen -l export-release -d "Export the project in release mode using the given preset and output path" -x
complete -c kaizen -l export-debug -d "Export the project in debug mode using the given preset and output path" -x
complete -c kaizen -l export-pack -d "Export the project data only as a PCK or ZIP file using the given preset and output path" -x
complete -c kaizen -l export-patch -d "Export pack with changed files only, using the given preset and output path" -x
complete -c kaizen -l patches -d "List of patches to use with --export-patch (comma-separated)" -x
complete -c kaizen -l install-android-build-template -d "Install the Android build template (use with --export-release or --export-debug)"
complete -c kaizen -l convert-3to4 -d "Converts project from Kaizen 3.x to Kaizen 4.x"
complete -c kaizen -l validate-conversion-3to4 -d "Shows what elements will be renamed when converting project from Kaizen 3.x to Kaizen 4.x"
complete -c kaizen -l doctool -d "Dump the engine API reference to the given path (defaults to current directory) in XML format, merging if existing files are found" -r
complete -c kaizen -l no-docbase -d "Disallow dumping the base types (used with --doctool)"
complete -c kaizen -l gdextension-docs -d "Generate API reference from all the GDExtensions loaded in the current project (used with --doctool)"
complete -c kaizen -l gdscript-docs -d "Generate API reference from the inline documentation in the GDScript files found in the given path (used with --doctool)" -r
complete -c kaizen -l build-solutions -d "Build the scripting solutions (e.g. for C# projects)"
complete -c kaizen -l dump-gdextension-interface -d "Generate GDExtension header file 'gdextension_interface.h' in the current folder. This file is the base file required to implement a GDExtension"
complete -c kaizen -l dump-gdextension-interface-json -d "Generate JSON dump of the GDExtension interface named 'gdextension_interface.json' in the current folder"
complete -c kaizen -l dump-extension-api -d "Generate JSON dump of the Kaizen API for GDExtension bindings named 'extension_api.json' in the current folder"
complete -c kaizen -l dump-extension-api-with-docs -d "Generate JSON dump of the Kaizen API for GDExtension bindings, including documentation"
complete -c kaizen -l validate-extension-api -d "Validate an extension API file dumped from a previous version of the engine to ensure API compatibility" -r
complete -c kaizen -l benchmark -d "Benchmark the run time and print it to console"
complete -c kaizen -l benchmark-file -d "Benchmark the run time and save it to a given file in JSON format" -r
complete -c kaizen -l test -d "Run all unit tests; run with '--test --help' for more information"
