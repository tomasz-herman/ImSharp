using ImSharp.Backends;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ImSharpOpenTKDemo;

public class DemoWindow
{
    public static unsafe void Main()
    {
        GLFW.Init();

        Window* window = GLFW.CreateWindow(800, 600, "ImSharp OpenTK Demo", null, null);
        if (window == null)
        {
            GLFW.Terminate();
        }

        GLFW.MakeContextCurrent(window);
        GL.LoadBindings(new GLFWBindingsContext());
        
        GLFW.GetFramebufferSize(window, out int width, out int height);
        GL.Viewport(0, 0, width, height);

        ImGuiCreateContext();
        ImPlotCreateContext();
        ImGuiImplGlfwInitForOpenGL(window, true);
        ImGuiImplOpenGL3Init();
        
        GL.ClearColor(0.3f, 0.5f, 0.8f, 1.0f);

        while (!GLFW.WindowShouldClose(window))
        {
            GLFW.PollEvents();
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            Native.ImGuiImplOpenGL3NewFrame();
            ImGuiImplGlfwNewFrame();
            ImGuiNewFrame();
        
            ImGuiShowDemoWindow();
            ImPlotShowDemoWindow();
        
            ImGuiRender();
        
            ImGuiImplOpenGL3RenderDrawData();
        
            GLFW.SwapBuffers(window);
        }
        
        ImGuiImplOpenGL3Shutdown();
        ImGuiImplGlfwShutdown();
        ImPlotDestroyContext();
        ImGuiDestroyContext();
        
        GLFW.DestroyWindow(window);
        GLFW.Terminate();
    }
}