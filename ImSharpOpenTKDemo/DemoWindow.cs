using ImSharp;
using ImSharp.Backends;
using ImSharpOpenTKDemo.Demos;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ImSharpOpenTKDemo;

public class DemoWindow
{
    public static unsafe void Main()
    {
        if (!GLFW.Init())
            return;

        Window* window = GLFW.CreateWindow(1480, 920, "ImSharp Showcase (ImGui 1.93 + All 9 Family Products)", null, null);
        if (window == null)
        {
            GLFW.Terminate();
            return;
        }

        GLFW.MakeContextCurrent(window);
        GLFW.SwapInterval(1);
        GL.LoadBindings(new GLFWBindingsContext());

        var imguiCtx = ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard
                        | ImGuiConfigFlags.DockingEnable
                        | ImGuiConfigFlags.ViewportsEnable;

        // When viewports are enabled, tweak WindowRounding/WindowBg so platform windows look identical to regular ones
        var style = ImGui.GetStyle();
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.AsRef().Colors[(int)ImGuiCol.WindowBg].w = 1.0f;
        }

        var implotCtx = ImPlot.CreateContext();
        var implot3dCtx = ImPlot3D.CreateContext();
        var imnodesCtx = ImNodes.CreateContext();
        ImGuizmo.SetImGuiContext(imguiCtx);

        ImGuiImplGlfw.InitForOpenGL(window, true);
        ImGuiImplOpenGL3.Init("#version 330 core");

        var guizmoDemo = new ImGuizmoDemo();
        var plotDemos = new ImPlotDemos();
        using var nodeDemos = new NodeEditorDemos();
        using var extraDemos = new ExtraWidgetsDemos();

        bool showGuizmo = true;
        bool showImPlot2D = true;
        bool showImPlot3D = true;
        bool showImNodes = true;
        bool showBlueprintNodes = true;
        bool showImNodesR = false;
        bool showGuizmoQuat = true;
        bool showCodeEditor = true;
        bool showImGuiDemo = false;
        bool showImPlotDemo = false;
        bool showImPlot3DDemo = false;

        double lastTime = GLFW.GetTime();

        while (!GLFW.WindowShouldClose(window))
        {
            GLFW.PollEvents();

            double now = GLFW.GetTime();
            float dt = (float)Math.Clamp(now - lastTime, 0.0001, 0.1);
            lastTime = now;

            plotDemos.Update(dt);

            GLFW.GetFramebufferSize(window, out int width, out int height);
            GL.Viewport(0, 0, width, height);
            GL.ClearColor(0.11f, 0.13f, 0.17f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ImGuiImplOpenGL3.NewFrame();
            ImGuiImplGlfw.NewFrame();
            ImGui.NewFrame();
            ImGuizmo.BeginFrame();

            // Fullscreen dockspace over the main viewport (allows docking, splitting, tabbing, and dragging windows out)
            if ((io.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0)
            {
                ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            }

            // Hub window
            ImGui.SetNextWindowPos(new ImVec2(20, 20), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new ImVec2(280, 360), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("ImSharp Showcase Hub"))
            {
                ImGui.Text($"Dear ImGui {ImGui.GetVersion()}");
                ImGui.Text($"FPS: {io.Framerate:F1} ({1000f / MathF.Max(io.Framerate, 1f):F2} ms)");
                ImGui.Separator();

                bool dockingEnabled = (io.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0;
                if (ImGui.Checkbox("Enable Docking", ref dockingEnabled))
                {
                    if (dockingEnabled) io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                    else io.ConfigFlags &= ~ImGuiConfigFlags.DockingEnable;
                }

                bool platformSupportsViewports = (io.BackendFlags & ImGuiBackendFlags.PlatformHasViewports) != 0;
                if (!platformSupportsViewports)
                    ImGui.BeginDisabled();

                bool viewportsEnabled = (io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0;
                if (ImGui.Checkbox("Multi-Viewports (Drag Out)", ref viewportsEnabled))
                {
                    if (viewportsEnabled) io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                    else io.ConfigFlags &= ~ImGuiConfigFlags.ViewportsEnable;
                }

                if (!platformSupportsViewports)
                {
                    ImGui.EndDisabled();
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Unavailable on native Wayland (Wayland disallows client window positioning).\nRun with: GLFW_PLATFORM=x11 dotnet run");
                    }
                }

                bool dockWithShift = io.ConfigDockingWithShift;
                if (ImGui.Checkbox("Dock Only With Shift", ref dockWithShift))
                    io.ConfigDockingWithShift = dockWithShift;

                ImGui.Separator();

                ImGui.Checkbox("ImGuizmo (3D Transform)", ref showGuizmo);
                ImGui.Checkbox("imGuIZMO.quat (3D Quat)", ref showGuizmoQuat);
                ImGui.Checkbox("ImPlot 2D (Live)", ref showImPlot2D);
                ImGui.Checkbox("ImPlot3D (Live Helix)", ref showImPlot3D);
                ImGui.Checkbox("imnodes (Nelarius)", ref showImNodes);
                ImGui.Checkbox("imgui-node-editor (thedmd)", ref showBlueprintNodes);
                ImGui.Checkbox("ImNodes (rokups)", ref showImNodesR);
                ImGui.Checkbox("ImGuiColorTextEdit", ref showCodeEditor);
                ImGui.Separator();
                ImGui.Checkbox("Dear ImGui Demo", ref showImGuiDemo);
                ImGui.Checkbox("ImPlot Full Demo", ref showImPlotDemo);
                ImGui.Checkbox("ImPlot3D Full Demo", ref showImPlot3DDemo);
            }
            ImGui.End();

            if (showGuizmo) guizmoDemo.Draw(ref showGuizmo);
            if (showImPlot2D) plotDemos.DrawImPlot2D(ref showImPlot2D);
            if (showImPlot3D) plotDemos.DrawImPlot3D(ref showImPlot3D);
            if (showImNodes) nodeDemos.DrawImNodes(ref showImNodes);
            if (showBlueprintNodes) nodeDemos.DrawBlueprintNodeEditor(ref showBlueprintNodes);
            if (showImNodesR) nodeDemos.DrawImNodesR(ref showImNodesR);
            if (showGuizmoQuat) extraDemos.DrawGuizmoQuat(ref showGuizmoQuat);
            if (showCodeEditor) extraDemos.DrawColorTextEdit(ref showCodeEditor);
            if (showImGuiDemo) ImGui.ShowDemoWindow(ref showImGuiDemo);
            if (showImPlotDemo) ImPlot.ShowDemoWindow(ref showImPlotDemo);
            if (showImPlot3DDemo) ImPlot3D.ShowDemoWindow(ref showImPlot3DDemo);

            ImGui.Render();
            ImGuiImplOpenGL3.RenderDrawData();

            // Update and render additional platform windows when Multi-Viewports are enabled
            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                Window* backupCurrentContext = GLFW.GetCurrentContext();
                ImGui.UpdatePlatformWindows();
                ImGui.RenderPlatformWindowsDefault();
                GLFW.MakeContextCurrent(backupCurrentContext);
            }

            GLFW.SwapBuffers(window);
        }

        ImGuiImplOpenGL3.Shutdown();
        ImGuiImplGlfw.Shutdown();
        ImNodes.DestroyContext(imnodesCtx);
        ImPlot3D.DestroyContext(implot3dCtx);
        ImPlot.DestroyContext(implotCtx);
        ImGui.DestroyContext(imguiCtx);

        GLFW.DestroyWindow(window);
        GLFW.Terminate();
    }
}
