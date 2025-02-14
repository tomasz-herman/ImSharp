#pragma once

#if defined(_WIN32) || defined(_WIN64)
    #define EXPORT __declspec(dllexport)
#else
    #define EXPORT __attribute__((visibility("default")))
#endif

extern "C" EXPORT bool ImGuiImplGlfwInitForOpenGL(void* window, bool install_callbacks);
extern "C" EXPORT bool ImGuiImplOpenGL3Init(const char* glsl_version);

extern "C" EXPORT void ImGuiImplOpenGL3NewFrame();
extern "C" EXPORT void ImGuiImplGlfwNewFrame();

extern "C" EXPORT void ImGuiImplOpenGL3RenderDrawData();

extern "C" EXPORT void ImGuiImplOpenGL3Shutdown();
extern "C" EXPORT void ImGuiImplGlfwShutdown();