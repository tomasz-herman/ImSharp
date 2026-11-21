using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImSharp.Backends;
using ImSharp.ColorTextEdit;
using ImSharp.GuizmoQuat;
using ImSharp.NodeEditor;
using ImSharp.NodesR;
using Xunit;

namespace ImSharp.Tests;

/// <summary>
/// End-to-end tests exercising ALL 9 ImGui-family libraries with ZERO <c>unsafe</c> code.
/// </summary>
public class SafeApiTests
{
    [Fact]
    public void Wrapper_Classes_Expose_Safe_NonPointer_APIs()
    {
        Type[] wrapperClasses =
        [
            typeof(ImGui),
            typeof(ImPlot),
            typeof(ImPlot3D),
            typeof(ImNodes),
            typeof(ImNodesR),
            typeof(ImNodeEditor),
            typeof(ImGuizmo),
            typeof(ImGuizmoQuat),
            typeof(ImColorTextEdit),
        ];

        int totalMethods = 0;
        int safeMethods = 0;

        foreach (var cls in wrapperClasses)
        {
            foreach (var m in cls.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                totalMethods++;
                bool hasPtr = m.ReturnType.IsPointer || m.GetParameters().Any(p => p.ParameterType.IsPointer);
                if (!hasPtr)
                    safeMethods++;
            }
        }

        double safeRatio = (double)safeMethods / totalMethods;
        Assert.True(totalMethods > 1000, $"Expected > 1000 wrapper methods, found {totalMethods}");
        Assert.True(safeRatio > 0.95, $"Expected > 95% safe wrapper methods, got {safeMethods}/{totalMethods} ({safeRatio:P1})");
    }

    [Fact]
    public void Safe_EndToEnd_All_Nine_ImGui_Family_Libraries()
    {
        var imguiCtx = ImGui.CreateContext();
        var implotCtx = ImPlot.CreateContext();
        var implot3dCtx = ImPlot3D.CreateContext();
        var imnodesCtx = ImNodes.CreateContext();
        var nodesRCanvas = ImNodesR.CreateCanvasState();
        var nodeEditorCtx = ImNodeEditor.CreateEditor();
        var textEditor = ImColorTextEdit.Create();

        Assert.False(imguiCtx.IsNull);
        Assert.False(implotCtx.IsNull);
        Assert.False(implot3dCtx.IsNull);
        Assert.False(imnodesCtx.IsNull);
        Assert.False(nodesRCanvas.IsNull);
        Assert.False(nodeEditorCtx.IsNull);
        Assert.False(textEditor.IsNull);

        try
        {
            ImGuizmo.SetImGuiContext(imguiCtx);

            var io = ImGui.GetIO();
            io.DisplaySize = new ImVec2(1280, 720);
            io.DeltaTime = 1.0f / 60.0f;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures;

            ReadOnlySpan<float> xs = [0f, 1f, 2f, 3f];
            ReadOnlySpan<float> ys = [0f, 1f, 4f, 9f];
            ReadOnlySpan<float> zs = [0f, 0.5f, 1f, 1.5f];

            for (int frame = 0; frame < 2; frame++)
            {
                ImGui.NewFrame();
                ImGuizmo.BeginFrame();
                ImGui.DockSpaceOverViewport(0, ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

                // 1. Dear ImGui (safe string, ref bool, ref float, default args)
                bool open = true;
                bool check = false;
                float slider = 0.5f;
                if (ImGui.Begin("Safe ImGui Window", ref open))
                {
                    ImGui.Text($"Dear ImGui {ImGui.GetVersion()}");
                    ImGui.Checkbox("Check", ref check);
                    ImGui.SliderFloat("Slider", ref slider, 0f, 1f);
                    ImGui.Button("Click");
                }
                ImGui.End();

                ImGui.ShowDemoWindow();

                // 2. ImPlot (safe ReadOnlySpan<float>)
                if (ImGui.Begin("Safe ImPlot"))
                {
                    if (ImPlot.BeginPlot("2D Plot", new ImVec2(-1, 0)))
                    {
                        ImPlot.PlotLine("y = x^2", xs, ys, xs.Length);
                        ImPlot.EndPlot();
                    }
                }
                ImGui.End();

                // 3. ImPlot3D (safe ReadOnlySpan<float>)
                if (ImGui.Begin("Safe ImPlot3D"))
                {
                    if (ImPlot3D.BeginPlot("3D Plot", new ImVec2(-1, 0)))
                    {
                        ImPlot3D.PlotLine("3D Line", xs, ys, zs, xs.Length);
                        ImPlot3D.EndPlot();
                    }
                }
                ImGui.End();

                // 4. imnodes (Nelarius)
                if (ImGui.Begin("Safe ImNodes"))
                {
                    ImNodes.BeginNodeEditor();
                    ImNodes.BeginNode(1);
                    ImNodes.BeginOutputAttribute(2);
                    ImGui.Text("Out");
                    ImNodes.EndOutputAttribute();
                    ImNodes.EndNode();
                    ImNodes.EndNodeEditor();
                }
                ImGui.End();

                // 5. ImNodes (rokups)
                if (ImGui.Begin("Safe ImNodesR"))
                {
                    ImNodesR.BeginCanvas(nodesRCanvas);
                    ImNodesR.EndCanvas();
                }
                ImGui.End();

                // 6. imgui-node-editor (thedmd)
                if (ImGui.Begin("Safe NodeEditor"))
                {
                    ImNodeEditor.SetCurrentEditor(nodeEditorCtx);
                    ImNodeEditor.Begin("SafeEditor");
                    ImNodeEditor.BeginNode(new NodeId { value_c = 1 });
                    ImGui.Text("Blueprint Node");
                    ImNodeEditor.EndNode();
                    ImNodeEditor.End();
                }
                ImGui.End();

                // 7. imGuIZMO.quat (safe ref Quaternion)
                if (ImGui.Begin("Safe GuizmoQuat"))
                {
                    var q = Quaternion.Identity;
                    ImGuizmoQuat.gizmo3D("##quat", ref q, 120f, 0);
                }
                ImGui.End();

                // 8. ImGuiColorTextEdit (safe TextEditorPtr & string)
                if (ImGui.Begin("Safe Code Editor"))
                {
                    ImColorTextEdit.SetText(textEditor, "line 1\nline 2\nline 3");
                    Assert.Equal((nuint)3, ImColorTextEdit.GetLineCount(textEditor));
                    ImColorTextEdit.Render(textEditor, "Code", new ImVec2(400, 300));
                }
                ImGui.End();

                ImGui.Render();
            }

            // 9. ImGuizmo (safe Span<float> / ReadOnlySpan<float> matrix round-trip)
            Span<float> matrix = stackalloc float[16];
            ImGuizmo.RecomposeMatrixFromComponents([1f, 2f, 3f], [0f, 0f, 0f], [2f, 2f, 2f], matrix);

            Span<float> outT = stackalloc float[3];
            Span<float> outR = stackalloc float[3];
            Span<float> outS = stackalloc float[3];
            ImGuizmo.DecomposeMatrixToComponents(matrix, outT, outR, outS);
            Assert.Equal(1f, outT[0], 3);
            Assert.Equal(2f, outT[1], 3);
            Assert.Equal(3f, outT[2], 3);
            Assert.Equal(2f, outS[0], 3);

            // Verify ImPlotSpec / ImPlot3DSpec defaults match C++ constructors (not all-zeros)
            Assert.Equal(-1f, ImPlotSpec.Default.LineColor.w);
            Assert.Equal(1f, ImPlotSpec.Default.LineWeight);
            Assert.Equal(-1, ImPlotSpec.Default.Stride);
            Assert.Equal(-1f, ImPlot3DSpec.Default.LineColor.w);
            Assert.Equal(1f, ImPlot3DSpec.Default.LineWeight);
            Assert.Equal(-1, ImPlot3DSpec.Default.Stride);

            // Safe DrawData inspection via ImDrawDataPtr properties
            var drawData = ImGui.GetDrawData();
            Assert.False(drawData.IsNull);
            Assert.True(drawData.Valid);
            Assert.True(drawData.TotalVtxCount > 0);
            Assert.True(drawData.TotalIdxCount > 0);
        }
        finally
        {
            ImColorTextEdit.Destroy(textEditor);
            ImNodeEditor.DestroyEditor(nodeEditorCtx);
            ImNodesR.DestroyCanvasState(nodesRCanvas);
            ImNodes.DestroyContext(imnodesCtx);
            ImPlot3D.DestroyContext(implot3dCtx);
            ImPlot.DestroyContext(implotCtx);
            ImGui.DestroyContext(imguiCtx);
        }
    }

    [Fact]
    public void Backends_Null_EndToEnd_And_All_Compiled_Backend_Symbols_Resolve()
    {
        // 1. Run Dear ImGui's official Null backend end-to-end (zero unsafe code)
        var ctx = ImGui.CreateContext();
        try
        {
            ImGuiImplNull.Init();
            for (int i = 0; i < 2; i++)
            {
                ImGuiImplNull.NewFrame();
                ImGui.NewFrame();
                if (ImGui.Begin("Null Backend Window"))
                {
                    ImGui.Text("Rendered via ImGuiImplNull");
                }
                ImGui.End();
                ImGui.Render();
                ImGuiImplNull.RenderDrawData();
            }
            Assert.True(ImGui.GetDrawData().Valid);
            Assert.True(ImGui.GetDrawData().TotalVtxCount > 0);
            ImGuiImplNull.Shutdown();
        }
        finally
        {
            ImGui.DestroyContext(ctx);
        }

        // 2. Verify all key backend symbols resolve in ImSharpNative
        nint lib = NativeLibrary.Load("ImSharpNative", typeof(ImGui).Assembly, null);
        try
        {
            // Always-built cross-platform backend symbols
            string[] expectedBackendSymbols =
            [
                "ImGui_ImplGlfw_InitForOpenGL",
                "ImGui_ImplGlfw_InitForVulkan",
                "ImGui_ImplGlfw_NewFrame",
                "ImGui_ImplGlfw_Shutdown",
                "ImGui_ImplOpenGL3_Init",
                "ImGui_ImplOpenGL3_NewFrame",
                "ImGui_ImplOpenGL3_RenderDrawData",
                "ImGui_ImplOpenGL3_Shutdown",
                "ImGui_ImplOpenGL2_Init",
                "ImGui_ImplOpenGL2_RenderDrawData",
                "ImGui_ImplNull_Init",
                "ImGui_ImplNullRender_RenderDrawData",
            ];

            foreach (var sym in expectedBackendSymbols)
            {
                Assert.True(NativeLibrary.TryGetExport(lib, sym, out _), $"Missing backend symbol: {sym}");
            }
        }
        finally
        {
            NativeLibrary.Free(lib);
        }
    }
}
