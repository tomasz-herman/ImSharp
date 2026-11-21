using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImSharp.ColorTextEdit;
using ImSharp.GuizmoQuat;
using ImSharp.NodeEditor;
using ImSharp.NodesR;
using Xunit;

namespace ImSharp.Tests;

public unsafe class BindingsTests
{
    [Fact]
    public void ImGui_Version_Is_1_93()
    {
        string? version = ImGui.GetVersion();
        Assert.NotNull(version);
        Assert.StartsWith("1.93", version);
    }

    [Fact]
    public void Struct_Sizes_Match_Native_ABI()
    {
        Assert.Equal(8, sizeof(ImVec2));
        Assert.Equal(16, sizeof(ImVec4));
        Assert.Equal(16, sizeof(ImColor));
        Assert.Equal(20, sizeof(ImDrawVert));
        Assert.Equal(72, sizeof(ImDrawCmd));
        Assert.Equal(248, sizeof(ImDrawList));
        Assert.Equal(72, sizeof(ImDrawData));
        Assert.Equal(3080, sizeof(ImGuiIO));
        Assert.Equal(1368, sizeof(ImGuiStyle));
        Assert.Equal(120, sizeof(ImGuiViewport));
        Assert.Equal(72, sizeof(ImFont));
        Assert.Equal(152, sizeof(ImFontConfig));
        Assert.Equal(44, sizeof(ImFontGlyph));
        Assert.Equal(16, sizeof(ImGuiKeyData));
        Assert.Equal(16, sizeof(ImGuiStoragePair));
        Assert.Equal(16, sizeof(ImPlotPoint));
        Assert.Equal(16, sizeof(ImPlotRange));
        Assert.Equal(32, sizeof(ImPlotRect));
        Assert.Equal(408, sizeof(ImPlotStyle));
        Assert.Equal(32, sizeof(ImNodesIO));
        Assert.Equal(192, sizeof(ImNodesStyle));
        Assert.Equal(272, sizeof(ImGuizmoStyle));

        // Extended ImGui-family structs
        Assert.Equal(24, sizeof(ImPlot3DPoint));
        Assert.Equal(16, sizeof(ImPlot3DRange));
        Assert.Equal(48, sizeof(ImPlot3DBox));
        Assert.Equal(48, sizeof(ImPlot3DPlane));
        Assert.Equal(32, sizeof(ImPlot3DQuat));
        Assert.Equal(48, sizeof(ImPlot3DRay));
        Assert.Equal(304, sizeof(ImPlot3DStyle));
        Assert.Equal(28, sizeof(CanvasStyle));
        Assert.Equal(8, sizeof(NodeId));
        Assert.Equal(8, sizeof(PinId));
        Assert.Equal(8, sizeof(LinkId));
        Assert.Equal(444, sizeof(cimnodes_editor_Style));
    }

    [Fact]
    public void All_Native_EntryPoints_Resolve_In_ImSharpNative()
    {
        nint libHandle = NativeLibrary.Load("ImSharpNative", typeof(ImGui).Assembly, null);
        try
        {
            Type[] nativeClasses =
            [
                typeof(ImGuiNative),
                typeof(ImPlotNative),
                typeof(ImPlot3DNative),
                typeof(ImNodesNative),
                typeof(ImNodesRNative),
                typeof(ImNodeEditorNative),
                typeof(ImGuizmoNative),
                typeof(ImGuizmoQuatNative),
                typeof(ImColorTextEditNative),
            ];

            int totalChecked = 0;
            var missing = new List<string>();

            foreach (var cls in nativeClasses)
            {
                foreach (var method in cls.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var dllImport = method.GetCustomAttribute<DllImportAttribute>();
                    if (dllImport == null)
                        continue;

                    string entryPoint = dllImport.EntryPoint ?? method.Name;
                    totalChecked++;
                    if (!NativeLibrary.TryGetExport(libHandle, entryPoint, out _))
                    {
                        missing.Add($"{cls.Name}.{entryPoint}");
                    }
                }
            }

            Assert.True(totalChecked > 2500, $"Expected > 2500 native entry points, found {totalChecked}");
            Assert.True(missing.Count == 0, $"Missing {missing.Count} native symbols: {string.Join(", ", missing.Take(20))}");
        }
        finally
        {
            NativeLibrary.Free(libHandle);
        }
    }

    [Fact]
    public void Utf8StringHelper_And_ScopedUtf8String_Handle_All_Cases()
    {
        Span<byte> stackBuf = stackalloc byte[256];

        using (var sNull = new ScopedUtf8String(null, stackBuf))
        {
            Assert.True(sNull.Pointer == null);
            Assert.Null(Utf8StringHelper.PtrToStringUtf8(sNull.Pointer));
        }

        using (var sEmpty = new ScopedUtf8String("", stackBuf))
        {
            Assert.True(sEmpty.Pointer != null);
            Assert.Equal("", Utf8StringHelper.PtrToStringUtf8(sEmpty.Pointer));
        }

        const string unicode = "Zażółć gęślą jaźń / 日本語 / 🚀";
        using (var sUni = new ScopedUtf8String(unicode, stackBuf))
        {
            Assert.Equal(unicode, Utf8StringHelper.PtrToStringUtf8(sUni.Pointer));
        }

        string longStr = new string('A', 1024) + "—end";
        using (var sLong = new ScopedUtf8String(longStr, stackBuf))
        {
            Assert.Equal(longStr, Utf8StringHelper.PtrToStringUtf8(sLong.Pointer));
        }
    }

    [Fact]
    public void ImVec_Conversions_And_Operators_Work()
    {
        ImVec2 a = new Vector2(1f, 2f);
        ImVec2 b = new(3f, 4f);
        ImVec2 sum = a + b;
        Assert.Equal(4f, sum.x);
        Assert.Equal(6f, sum.y);

        Vector2 v2 = sum;
        Assert.Equal(new Vector2(4f, 6f), v2);

        ImVec4 c = new Vector4(0.1f, 0.2f, 0.3f, 1.0f);
        Vector4 v4 = c;
        Assert.Equal(new Vector4(0.1f, 0.2f, 0.3f, 1.0f), v4);
    }
}
