using System.Runtime.InteropServices;

namespace ImSharp.Backends;

public static partial class ImGuiImplOpenGL2
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_Init")]
    public static partial bool Init();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData);

    public static void RenderDrawData(ImDrawDataPtr drawData) => RenderDrawDataNative(drawData.Handle);

    public static void RenderDrawData() => RenderDrawDataNative(ImGui.GetDrawData().Handle);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_CreateDeviceObjects")]
    public static partial bool CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_DestroyDeviceObjects")]
    public static partial void DestroyDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplOpenGL2_UpdateTexture")]
    private static partial void UpdateTextureNative(nint tex);

    public static void UpdateTexture(ImTextureDataPtr tex) => UpdateTextureNative(tex.Handle);
}
