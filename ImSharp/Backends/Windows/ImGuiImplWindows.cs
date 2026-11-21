using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ImSharp.Backends;

[SupportedOSPlatform("windows")]
public static partial class ImGuiImplWin32
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_Init")]
    public static partial bool Init(nint hwnd);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_InitForOpenGL")]
    public static partial bool InitForOpenGL(nint hwnd);

    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_EnableDpiAwareness")]
    public static partial void EnableDpiAwareness();

    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_GetDpiScaleForHwnd")]
    public static partial float GetDpiScaleForHwnd(nint hwnd);

    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_GetDpiScaleForMonitor")]
    public static partial float GetDpiScaleForMonitor(nint monitor);

    [LibraryImport(LibraryName, EntryPoint = "ImGuiImplWin32_EnableAlphaCompositing")]
    public static partial void EnableAlphaCompositing(nint hwnd);
}

[SupportedOSPlatform("windows")]
public static partial class ImGuiImplDX9
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX9_Init")]
    public static partial bool Init(nint device);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX9_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX9_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX9_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData);

    public static void RenderDrawData(ImDrawDataPtr drawData) => RenderDrawDataNative(drawData.Handle);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX9_CreateDeviceObjects")]
    public static partial bool CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX9_InvalidateDeviceObjects")]
    public static partial void InvalidateDeviceObjects();
}

[SupportedOSPlatform("windows")]
public static partial class ImGuiImplDX10
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX10_Init")]
    public static partial bool Init(nint device);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX10_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX10_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX10_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData);

    public static void RenderDrawData(ImDrawDataPtr drawData) => RenderDrawDataNative(drawData.Handle);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX10_CreateDeviceObjects")]
    public static partial bool CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX10_InvalidateDeviceObjects")]
    public static partial void InvalidateDeviceObjects();
}

[SupportedOSPlatform("windows")]
public static partial class ImGuiImplDX11
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX11_Init")]
    public static partial bool Init(nint device, nint deviceContext);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX11_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX11_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX11_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData);

    public static void RenderDrawData(ImDrawDataPtr drawData) => RenderDrawDataNative(drawData.Handle);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX11_CreateDeviceObjects")]
    public static partial bool CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX11_InvalidateDeviceObjects")]
    public static partial void InvalidateDeviceObjects();
}

[SupportedOSPlatform("windows")]
public static partial class ImGuiImplDX12
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX12_Init")]
    public static partial bool Init(nint initInfo);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX12_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX12_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX12_RenderDrawData")]
    private static partial void RenderDrawDataNative(nint drawData, nint graphicsCommandList);

    public static void RenderDrawData(ImDrawDataPtr drawData, nint graphicsCommandList) =>
        RenderDrawDataNative(drawData.Handle, graphicsCommandList);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX12_CreateDeviceObjects")]
    public static partial bool CreateDeviceObjects();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplDX12_InvalidateDeviceObjects")]
    public static partial void InvalidateDeviceObjects();
}
