#include <imgui.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>
#include <print>
#include "Library.h"
#include <GLFW/glfw3.h>
#include <implot.h>

ImGuiContext* ImGuiCreateContext()
{
    return ImGui::CreateContext();
}

void ImGuiDestroyContext()
{
    ImGui::DestroyContext();
}

bool ImGuiImplGlfwInitForOpenGL(void* window, bool install_callbacks)
{
    if (glfwInit() == GLFW_FALSE)
    {
        return false;
    }
    return ImGui_ImplGlfw_InitForOpenGL((GLFWwindow*)window, install_callbacks);
}

bool ImGuiImplOpenGL3Init(const char* glsl_version)
{
    if (gladLoadGL() == 0)
    {
        return false;
    }
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

void ImGuiShowDemoWindow()
{
    ImGui::ShowDemoWindow();
}

ImPlotContext* ImPlotCreateContext() {
    return ImPlot::CreateContext();
}

void ImPlotShowDemoWindow() {
    ImPlot::ShowDemoWindow();
}

void ImPlotDestroyContext() {
    ImPlot::DestroyContext();
}

void ImGuiNewFrame()
{
    ImGui::NewFrame();
}

void ImGuiRender()
{
    ImGui::Render();
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

void CheckGLError(const std::string& label)
{
    GLenum error;
    static std::unordered_map<GLenum, std::string> errorDescriptions = {
        { GL_NO_ERROR, "No error" },
        { GL_INVALID_ENUM, "An unacceptable value is specified for an enumerated argument" },
        { GL_INVALID_VALUE, "A numeric argument is out of range" },
        { GL_INVALID_OPERATION, "The specified operation is not allowed in the current state" },
        { GL_INVALID_FRAMEBUFFER_OPERATION, "The framebuffer object is not complete" },
        { GL_OUT_OF_MEMORY, "There is not enough memory left to execute the command" },
        { GL_STACK_UNDERFLOW,
            "An attempt has been made to perform an operation that would cause an internal stack to underflow" },
        { GL_STACK_OVERFLOW,
            "An attempt has been made to perform an operation that would cause an internal stack to overflow" },
    };

    while ((error = glGetError()) != GL_NO_ERROR)
    {
        auto description = errorDescriptions.contains(error) ? errorDescriptions[error] : "Unknown Description";

        if (label.empty())
        {
            std::println("[GL ERROR] Error Code: 0x{:X} ({})", error, description);
        }
        else
        {
            std::println("[GL ERROR] {} - Error Code: 0x{:X} ({})", label, error, description);
        }
    }
}
