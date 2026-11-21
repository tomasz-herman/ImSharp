using System.Runtime.InteropServices;

namespace ImSharp.Backends;

/// <summary>
/// Dear ImGui official Null backend for headless rendering, benchmarking, and automated testing.
/// </summary>
public static partial class ImGuiImplNull
{
    private const string LibraryName = "ImSharpNative";

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNull_Init")]
    public static partial void Init();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNull_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNull_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullPlatform_Init")]
    public static partial void InitPlatform();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullPlatform_Shutdown")]
    public static partial void ShutdownPlatform();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullPlatform_NewFrame")]
    public static partial void NewFramePlatform();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullRender_Init")]
    public static partial void InitRender();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullRender_Shutdown")]
    public static partial void ShutdownRender();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullRender_NewFrame")]
    public static partial void NewFrameRender();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplNullRender_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData);

    public static void RenderDrawData(ImDrawDataPtr drawData) => RenderDrawDataNative(drawData.Handle);

    public static void RenderDrawData() => RenderDrawDataNative(ImGui.GetDrawData().Handle);
}
