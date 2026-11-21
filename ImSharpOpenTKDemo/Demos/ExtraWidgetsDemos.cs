using System.Numerics;
using ImSharp;
using ImSharp.ColorTextEdit;
using ImSharp.GuizmoQuat;

namespace ImSharpOpenTKDemo.Demos;

/// <summary>
/// Interactive demos for imGuIZMO.quat (3D quaternion &amp; light gizmos)
/// and ImGuiColorTextEdit (syntax-highlighted code editor).
/// </summary>
public class ExtraWidgetsDemos : IDisposable
{
    private readonly TextEditorPtr _textEditor;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _lightDir = Vector3.Normalize(new Vector3(0.6f, 0.7f, 0.4f));
    private bool _readOnly;
    private bool _darkPalette = true;

    public ExtraWidgetsDemos()
    {
        _textEditor = ImColorTextEdit.Create();
        var lang = ImColorTextEdit.LookupLanguageByExtension(".cs");
        if (lang.IsNull)
            lang = ImColorTextEdit.LookupLanguage("C++");
        if (!lang.IsNull)
            ImColorTextEdit.SetLanguage(_textEditor, lang);
        ImColorTextEdit.SetPalette(_textEditor, ImColorTextEdit.GetDarkPalette());

        ImColorTextEdit.SetText(_textEditor,
            """
            using ImSharp;

            // 100% safe C# bindings for the Dear ImGui family!
            if (ImGui.Begin("Hello ImSharp"))
            {
                ImGui.Text($"Dear ImGui {ImGui.GetVersion()}");
                if (ImGui.Button("Click Me"))
                {
                    Console.WriteLine("Clicked!");
                }
            }
            ImGui.End();
            """);
    }

    public void DrawGuizmoQuat(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(20, 320), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(320, 320), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("imGuIZMO.quat", ref open))
        {
            ImGui.End();
            return;
        }

        ImGui.Text("3D Quaternion + Light Direction");
        ImGuizmoQuat.gizmo3D("##quat_gizmo", ref _rotation, 140f, 0);
        ImGui.SameLine();
        ImGuizmoQuat.gizmo3D("##light_gizmo", ref _lightDir, 140f, 0);

        ImGui.Text($"Quat: ({_rotation.X:F2}, {_rotation.Y:F2}, {_rotation.Z:F2}, {_rotation.W:F2})");
        ImGui.Text($"Light: ({_lightDir.X:F2}, {_lightDir.Y:F2}, {_lightDir.Z:F2})");

        if (ImGui.Button("Reset Rotation"))
            _rotation = Quaternion.Identity;
        ImGui.SameLine();
        if (ImGui.Button("Reset Light"))
            _lightDir = Vector3.Normalize(new Vector3(0.6f, 0.7f, 0.4f));

        ImGui.End();
    }

    public void DrawColorTextEdit(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(460, 600), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(500, 300), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("ImGuiColorTextEdit", ref open))
        {
            ImGui.End();
            return;
        }

        if (ImGui.Checkbox("Read-Only", ref _readOnly))
            ImColorTextEdit.SetReadOnlyEnabled(_textEditor, _readOnly);

        ImGui.SameLine();
        if (ImGui.Checkbox("Dark", ref _darkPalette))
            ImColorTextEdit.SetPalette(_textEditor, _darkPalette ? ImColorTextEdit.GetDarkPalette() : ImColorTextEdit.GetLightPalette());

        ImGui.SameLine();
        if (ImGui.Button("Undo"))
            ImColorTextEdit.Undo(_textEditor);
        ImGui.SameLine();
        if (ImGui.Button("Redo"))
            ImColorTextEdit.Redo(_textEditor);
        ImGui.SameLine();
        ImGui.Text($"Lang: {ImColorTextEdit.GetLanguageName(_textEditor) ?? "Plain"} | Lines: {ImColorTextEdit.GetLineCount(_textEditor)}");

        ImVec2 avail = ImGui.GetContentRegionAvail();
        ImColorTextEdit.Render(_textEditor, "##code_editor", avail);

        ImGui.End();
    }

    public void Dispose()
    {
        ImColorTextEdit.Destroy(_textEditor);
    }
}
