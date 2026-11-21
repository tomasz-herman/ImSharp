using System.Runtime.InteropServices;

namespace ImSharp.Backends;

public static partial class ImGuiImplSDL2
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_InitForOpenGL")]
    public static partial bool InitForOpenGL(nint window, nint sdlGlContext);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_InitForVulkan")]
    public static partial bool InitForVulkan(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_InitForD3D")]
    public static partial bool InitForD3D(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_InitForMetal")]
    public static partial bool InitForMetal(nint window);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_InitForSDLRenderer")]
    public static partial bool InitForSDLRenderer(nint window, nint renderer);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_InitForOther")]
    public static partial bool InitForOther(nint window);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_NewFrame")]
    public static partial void NewFrame();

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_ProcessEvent")]
    public static partial bool ProcessEvent(nint sdlEvent);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_GetContentScaleForWindow")]
    public static partial float GetContentScaleForWindow(nint window);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDL2_GetContentScaleForDisplay")]
    public static partial float GetContentScaleForDisplay(int displayIndex);
}

public static partial class ImGuiImplSDLRenderer2
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_Init")]
    public static partial bool Init(nint renderer);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData, nint renderer);

    public static void RenderDrawData(ImDrawDataPtr drawData, nint renderer) => RenderDrawDataNative(drawData.Handle, renderer);

    public static void RenderDrawData(nint renderer) => RenderDrawDataNative(ImGui.GetDrawData().Handle, renderer);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_CreateDeviceObjects")]
    public static partial bool CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_DestroyDeviceObjects")]
    public static partial void DestroyDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplSDLRenderer2_UpdateTexture")]
    private static partial void UpdateTextureNative(nint tex);

    public static void UpdateTexture(ImTextureDataPtr tex) => UpdateTextureNative(tex.Handle);
}
