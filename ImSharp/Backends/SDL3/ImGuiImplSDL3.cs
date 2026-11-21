using System.Runtime.InteropServices;

namespace ImSharp.Backends;

public static partial class ImGuiImplSDL3
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForOpenGL")]
    public static partial bool InitForOpenGL(nint window, nint sdlGlContext);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForVulkan")]
    public static partial bool InitForVulkan(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForD3D")]
    public static partial bool InitForD3D(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForMetal")]
    public static partial bool InitForMetal(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForSDLRenderer")]
    public static partial bool InitForSDLRenderer(nint window, nint renderer);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForSDLGPU")]
    public static partial bool InitForSDLGPU(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_InitForOther")]
    public static partial bool InitForOther(nint window);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_NewFrame")]
    public static partial void NewFrame();

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL3_ProcessEvent")]
    public static partial bool ProcessEvent(nint sdlEvent);
}

public static partial class ImGuiImplSDLRenderer3
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_Init")]
    public static partial bool Init(nint renderer);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData, nint renderer);

    public static void RenderDrawData(ImDrawDataPtr drawData, nint renderer) => RenderDrawDataNative(drawData.Handle, renderer);

    public static void RenderDrawData(nint renderer) => RenderDrawDataNative(ImGui.GetDrawData().Handle, renderer);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_CreateDeviceObjects")]
    public static partial void CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_DestroyDeviceObjects")]
    public static partial void DestroyDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer3_UpdateTexture")]
    private static partial void UpdateTextureNative(nint tex);

    public static void UpdateTexture(ImTextureDataPtr tex) => UpdateTextureNative(tex.Handle);
}

public static partial class ImGuiImplSDLGPU3
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_Init")]
    public static partial bool Init(nint initInfo);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_PrepareDrawData")]
    private static partial void PrepareDrawDataNative(nint drawData, nint commandBuffer);

    public static void PrepareDrawData(ImDrawDataPtr drawData, nint commandBuffer) => PrepareDrawDataNative(drawData.Handle, commandBuffer);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData, nint commandBuffer, nint renderPass, nint pipeline);

    public static void RenderDrawData(ImDrawDataPtr drawData, nint commandBuffer, nint renderPass, nint pipeline = 0) =>
        RenderDrawDataNative(drawData.Handle, commandBuffer, renderPass, pipeline);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_CreateDeviceObjects")]
    public static partial void CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_DestroyDeviceObjects")]
    public static partial void DestroyDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLGPU3_UpdateTexture")]
    private static partial void UpdateTextureNative(nint tex);

    public static void UpdateTexture(ImTextureDataPtr tex) => UpdateTextureNative(tex.Handle);
}
