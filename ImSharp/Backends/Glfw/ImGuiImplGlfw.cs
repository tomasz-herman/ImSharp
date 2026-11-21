using System.Runtime.InteropServices;

namespace ImSharp.Backends;

public static partial class ImGuiImplGlfw
{
    private const string LibraryName = "ImSharpNative";

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_InitForOpenGL")]
    public static partial bool InitForOpenGL(nint window, [MarshalAs(UnmanagedType.U1)] bool installCallbacks);

    public static unsafe bool InitForOpenGL(void* window, bool installCallbacks) =>
        InitForOpenGL((nint)window, installCallbacks);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_InitForVulkan")]
    public static partial bool InitForVulkan(nint window, [MarshalAs(UnmanagedType.U1)] bool installCallbacks);

    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_InitForOther")]
    public static partial bool InitForOther(nint window, [MarshalAs(UnmanagedType.U1)] bool installCallbacks);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_Shutdown")]
    public static partial void Shutdown();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_NewFrame")]
    public static partial void NewFrame();

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_InstallCallbacks")]
    public static partial void InstallCallbacks(nint window);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_RestoreCallbacks")]
    public static partial void RestoreCallbacks(nint window);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_SetCallbacksChainForAllWindows")]
    public static partial void SetCallbacksChainForAllWindows([MarshalAs(UnmanagedType.U1)] bool chainForAllWindows);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_WindowFocusCallback")]
    public static partial void WindowFocusCallback(nint window, int focused);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_CursorEnterCallback")]
    public static partial void CursorEnterCallback(nint window, int entered);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_CursorPosCallback")]
    public static partial void CursorPosCallback(nint window, double x, double y);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_MouseButtonCallback")]
    public static partial void MouseButtonCallback(nint window, int button, int action, int mods);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_ScrollCallback")]
    public static partial void ScrollCallback(nint window, double xoffset, double yoffset);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_KeyCallback")]
    public static partial void KeyCallback(nint window, int key, int scancode, int action, int mods);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_CharCallback")]
    public static partial void CharCallback(nint window, uint c);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_MonitorCallback")]
    public static partial void MonitorCallback(nint monitor, int @event);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_Sleep")]
    public static partial void Sleep(int milliseconds);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_GetContentScaleForWindow")]
    public static partial float GetContentScaleForWindow(nint window);

    [LibraryImport(LibraryName, EntryPoint = "ImGui_ImplGlfw_GetContentScaleForMonitor")]
    public static partial float GetContentScaleForMonitor(nint monitor);
}
