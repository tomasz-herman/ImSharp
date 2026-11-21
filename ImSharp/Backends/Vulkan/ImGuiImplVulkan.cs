using System.Runtime.InteropServices;

namespace ImSharp.Backends;

public static partial class ImGuiImplVulkan
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_Init")]
    public static partial bool Init(nint initInfo);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData, nint commandBuffer, nint pipeline);

    public static void RenderDrawData(ImDrawDataPtr drawData, nint commandBuffer, nint pipeline = 0) =>
        RenderDrawDataNative(drawData.Handle, commandBuffer, pipeline);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_SetMinImageCount")]
    public static partial void SetMinImageCount(uint minImageCount);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_UpdateTexture")]
    private static partial void UpdateTextureNative(nint tex);

    public static void UpdateTexture(ImTextureDataPtr tex) => UpdateTextureNative(tex.Handle);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_CreateMainPipeline")]
    public static partial void CreateMainPipeline(nint pipelineInfo);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_AddTexture")]
    public static partial nint AddTexture(nint sampler, nint imageView, int imageLayout);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_RemoveTexture")]
    public static partial void RemoveTexture(nint descriptorSet);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplVulkan_LoadFunctions")]
    public static partial bool LoadFunctions(uint apiVersion, nint loaderFunc, nint userData = 0);
}
