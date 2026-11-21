#include "BackendImpl.h"

#ifdef _WIN32
#include <imgui_impl_win32.h>

extern "C" EXPORT bool ImGuiImplWin32_Init(void* hwnd) { return ImGui_ImplWin32_Init(hwnd); }
extern "C" EXPORT bool ImGuiImplWin32_InitForOpenGL(void* hwnd) { return ImGui_ImplWin32_InitForOpenGL(hwnd); }
extern "C" EXPORT void ImGuiImplWin32_Shutdown() { ImGui_ImplWin32_Shutdown(); }
extern "C" EXPORT void ImGuiImplWin32_NewFrame() { ImGui_ImplWin32_NewFrame(); }
extern "C" EXPORT void ImGuiImplWin32_EnableDpiAwareness() { ImGui_ImplWin32_EnableDpiAwareness(); }
extern "C" EXPORT float ImGuiImplWin32_GetDpiScaleForHwnd(void* hwnd) { return ImGui_ImplWin32_GetDpiScaleForHwnd(hwnd); }
extern "C" EXPORT float ImGuiImplWin32_GetDpiScaleForMonitor(void* monitor) { return ImGui_ImplWin32_GetDpiScaleForMonitor(monitor); }
extern "C" EXPORT void ImGuiImplWin32_EnableAlphaCompositing(void* hwnd) { ImGui_ImplWin32_EnableAlphaCompositing(hwnd); }
#endif
