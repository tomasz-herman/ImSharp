using System.Numerics;
using System.Runtime.InteropServices;
using ImSharp;

namespace ImSharpOpenTKDemo.Demos;

/// <summary>
/// Interactive 3D ImGuizmo showcase with a perspective camera, ground grid,
/// manipulatable 3D cube, Translate/Rotate/Scale/Universal modes, Local/World space,
/// snapping, ViewManipulate orientation cube, and live TRS inspector.
/// </summary>
public class ImGuizmoDemo
{
    private readonly float[] _view = new float[16];
    private readonly float[] _projection = new float[16];
    private readonly float[] _identity =
    [
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    ];
    private readonly float[] _objectMatrix =
    [
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0.5f, 0, 1
    ];

    private ImGuizmoOperation _operation = ImGuizmoOperation.TRANSLATE;
    private ImGuizmoMode _mode = ImGuizmoMode.LOCAL;
    private bool _useSnap;
    private float _translateSnap = 0.5f;
    private float _rotateSnap = 15.0f;
    private float _scaleSnap = 0.25f;
    private float _fov = 45.0f;
    private bool _isOrthographic;
    private float _cameraDistance = 8.0f;

    public ImGuizmoDemo()
    {
        ResetCamera();
    }

    private void ResetCamera()
    {
        var eye = new Vector3(4.5f, 3.5f, 5.5f);
        var target = Vector3.Zero;
        var up = Vector3.UnitY;
        _cameraDistance = Vector3.Distance(eye, target);
        CopyMatrix(Matrix4x4.CreateLookAt(eye, target, up), _view);
    }

    private void ResetObject()
    {
        Array.Copy(_identity, _objectMatrix, 16);
        _objectMatrix[13] = 0.5f;
    }

    public void Draw(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(320, 20), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(640, 560), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("ImGuizmo 3D Gizmo", ref open))
        {
            ImGui.End();
            return;
        }

        // Toolbar: Operation mode
        if (ImGui.RadioButton("Translate", _operation == ImGuizmoOperation.TRANSLATE))
            _operation = ImGuizmoOperation.TRANSLATE;
        ImGui.SameLine();
        if (ImGui.RadioButton("Rotate", _operation == ImGuizmoOperation.ROTATE))
            _operation = ImGuizmoOperation.ROTATE;
        ImGui.SameLine();
        if (ImGui.RadioButton("Scale", _operation == ImGuizmoOperation.SCALE))
            _operation = ImGuizmoOperation.SCALE;
        ImGui.SameLine();
        if (ImGui.RadioButton("Universal", _operation == ImGuizmoOperation.UNIVERSAL))
            _operation = ImGuizmoOperation.UNIVERSAL;

        ImGui.SameLine();
        ImGui.Text("|");
        ImGui.SameLine();

        if (_operation != ImGuizmoOperation.SCALE)
        {
            if (ImGui.RadioButton("Local", _mode == ImGuizmoMode.LOCAL))
                _mode = ImGuizmoMode.LOCAL;
            ImGui.SameLine();
            if (ImGui.RadioButton("World", _mode == ImGuizmoMode.WORLD))
                _mode = ImGuizmoMode.WORLD;
            ImGui.SameLine();
        }

        if (ImGui.Button("Reset Cube"))
            ResetObject();
        ImGui.SameLine();
        if (ImGui.Button("Reset Camera"))
            ResetCamera();

        // Transform inspector (TRS <-> Matrix)
        Span<float> translation = stackalloc float[3];
        Span<float> rotation = stackalloc float[3];
        Span<float> scale = stackalloc float[3];
        ImGuizmo.DecomposeMatrixToComponents(_objectMatrix, translation, rotation, scale);

        bool changed = false;
        ImGui.SetNextItemWidth(220);
        changed |= ImGui.DragFloat3("Position", translation, 0.05f);
        ImGui.SameLine();
        ImGui.Checkbox("Snap", ref _useSnap);
        if (_useSnap)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(80);
            if (_operation == ImGuizmoOperation.ROTATE)
                ImGui.DragFloat("Deg", ref _rotateSnap, 1f, 1f, 90f);
            else if (_operation == ImGuizmoOperation.SCALE)
                ImGui.DragFloat("Step", ref _scaleSnap, 0.05f, 0.05f, 2f);
            else
                ImGui.DragFloat("Unit", ref _translateSnap, 0.05f, 0.05f, 5f);
        }

        ImGui.SetNextItemWidth(220);
        changed |= ImGui.DragFloat3("Rotation", rotation, 0.5f);
        ImGui.SameLine();
        ImGui.Checkbox("Ortho", ref _isOrthographic);

        ImGui.SetNextItemWidth(220);
        changed |= ImGui.DragFloat3("Scale", scale, 0.02f);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat("FOV", ref _fov, 20f, 90f);

        if (changed)
            ImGuizmo.RecomposeMatrixFromComponents(translation, rotation, scale, _objectMatrix);

        ImGui.Separator();

        // 3D Viewport child region (NoMove prevents dragging the gizmo from dragging the parent window)
        ImVec2 avail = ImGui.GetContentRegionAvail();
        float vpWidth = MathF.Max(avail.x, 120f);
        float vpHeight = MathF.Max(avail.y, 120f);

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new ImVec4(0.10f, 0.12f, 0.15f, 1.0f));
        if (ImGui.BeginChild("##ImGuizmoViewport", new ImVec2(vpWidth, vpHeight), ImGuiChildFlags.Borders, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar))
        {
            ImVec2 vpPos = ImGui.GetWindowPos();
            ImVec2 vpSize = ImGui.GetWindowSize();
            float aspect = vpSize.x / MathF.Max(vpSize.y, 1f);

            if (_isOrthographic)
            {
                float orthoHeight = 6.0f;
                float orthoWidth = orthoHeight * aspect;
                CopyMatrix(Matrix4x4.CreateOrthographic(orthoWidth, orthoHeight, 0.1f, 100f), _projection);
            }
            else
            {
                CopyMatrix(Matrix4x4.CreatePerspectiveFieldOfView(_fov * (MathF.PI / 180f), aspect, 0.1f, 100f), _projection);
            }

            ImGuizmo.SetOrthographic(_isOrthographic);
            ImGuizmo.SetDrawlist(ImGui.GetWindowDrawList());
            ImGuizmo.SetRect(vpPos.x, vpPos.y, vpSize.x, vpSize.y);

            // Render ground grid and 3D cube
            ImGuizmo.DrawGrid(_view, _projection, _identity, 10.0f);
            ImGuizmo.DrawCubes(_view, _projection, _objectMatrix, 1);

            // Snap vector if enabled
            Span<float> snap = stackalloc float[3];
            if (_useSnap)
            {
                float s = _operation switch
                {
                    ImGuizmoOperation.ROTATE => _rotateSnap,
                    ImGuizmoOperation.SCALE => _scaleSnap,
                    _ => _translateSnap
                };
                snap[0] = s;
                snap[1] = s;
                snap[2] = s;
            }

            // Interactive gizmo manipulation
            ImGuizmo.Manipulate(
                _view,
                _projection,
                _operation,
                _operation == ImGuizmoOperation.SCALE ? ImGuizmoMode.LOCAL : _mode,
                _objectMatrix,
                deltaMatrix: default,
                snap: _useSnap ? snap : default);

            // Orientation cube in top-right of the viewport (drag to orbit camera!)
            float manipSize = 110f;
            ImGuizmo.ViewManipulate(
                _view,
                _cameraDistance,
                new ImVec2(vpPos.x + vpSize.x - manipSize - 8f, vpPos.y + 8f),
                new ImVec2(manipSize, manipSize),
                0x20101010u);
        }
        ImGui.EndChild();
        ImGui.PopStyleColor();

        ImGui.End();
    }

    private static void CopyMatrix(Matrix4x4 m, Span<float> dst)
    {
        MemoryMarshal.Cast<Matrix4x4, float>(MemoryMarshal.CreateReadOnlySpan(ref m, 1)).CopyTo(dst);
    }
}
