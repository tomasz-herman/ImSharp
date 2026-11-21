#include "BackendImpl.h"
#include <imgui.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>
#include <GLFW/glfw3.h>

bool ImGuiImplGlfwInitForOpenGL(void* window, bool install_callbacks)
{
    return ImGui_ImplGlfw_InitForOpenGL(static_cast<GLFWwindow*>(window), install_callbacks);
}

bool ImGuiImplOpenGL3Init(const char* glsl_version)
{
    return ImGui_ImplOpenGL3_Init(glsl_version);
}

void ImGuiImplOpenGL3NewFrame()
{
    ImGui_ImplOpenGL3_NewFrame();
}

void ImGuiImplGlfwNewFrame()
{
    ImGui_ImplGlfw_NewFrame();
}

void ImGuiImplOpenGL3RenderDrawData()
{
    ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
}

void ImGuiImplOpenGL3Shutdown()
{
    ImGui_ImplOpenGL3_Shutdown();
}

void ImGuiImplGlfwShutdown()
{
    ImGui_ImplGlfw_Shutdown();
}
