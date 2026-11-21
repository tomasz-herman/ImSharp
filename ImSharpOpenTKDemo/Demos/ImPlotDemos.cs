using ImSharp;

namespace ImSharpOpenTKDemo.Demos;

/// <summary>
/// Interactive real-time 2D (ImPlot) and 3D (ImPlot3D) plotting demos.
/// </summary>
public class ImPlotDemos
{
    private const int SampleCount = 200;
    private readonly float[] _xs = new float[SampleCount];
    private readonly float[] _sinYs = new float[SampleCount];
    private readonly float[] _cosYs = new float[SampleCount];

    private readonly float[] _helixX = new float[SampleCount];
    private readonly float[] _helixY = new float[SampleCount];
    private readonly float[] _helixZ = new float[SampleCount];

    private float _frequency = 2.0f;
    private float _amplitude = 1.0f;
    private float _phase;
    private bool _animate = true;

    public void Update(float deltaTime)
    {
        if (_animate)
            _phase += deltaTime * 2.0f;

        for (int i = 0; i < SampleCount; i++)
        {
            float t = i / (float)(SampleCount - 1) * MathF.PI * 2f;
            _xs[i] = t;
            _sinYs[i] = MathF.Sin(t * _frequency + _phase) * _amplitude;
            _cosYs[i] = MathF.Cos(t * _frequency + _phase) * _amplitude;

            float turns = t * 2.5f + _phase;
            _helixX[i] = MathF.Cos(turns) * _amplitude;
            _helixY[i] = MathF.Sin(turns) * _amplitude;
            _helixZ[i] = (i / (float)(SampleCount - 1)) * 2f - 1f;
        }
    }

    public void DrawImPlot2D(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(20, 320), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(480, 360), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("ImPlot 2D (Live)", ref open))
        {
            ImGui.End();
            return;
        }

        ImGui.Checkbox("Animate", ref _animate);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(110);
        ImGui.SliderFloat("Freq", ref _frequency, 0.5f, 6.0f);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(110);
        ImGui.SliderFloat("Amp", ref _amplitude, 0.2f, 2.5f);

        if (ImPlot.BeginPlot("Trigonometric Waves", new ImVec2(-1, -1)))
        {
            ImPlot.PlotLine("sin(x)", _xs, _sinYs, SampleCount);
            ImPlot.PlotLine("cos(x)", _xs, _cosYs, SampleCount);
            ImPlot.EndPlot();
        }

        ImGui.End();
    }

    public void DrawImPlot3D(ref bool open)
    {
        ImGui.SetNextWindowPos(new ImVec2(520, 320), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new ImVec2(460, 360), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("ImPlot3D (Live Helix)", ref open))
        {
            ImGui.End();
            return;
        }

        if (ImPlot3D.BeginPlot("3D Helix", new ImVec2(-1, -1)))
        {
            ImPlot3D.SetupAxesLimits(-1.5, 1.5, -1.5, 1.5, -1.2, 1.2);
            ImPlot3D.PlotLine("Helix", _helixX, _helixY, _helixZ, SampleCount, new ImPlot3DSpec { LineWeight = 2.5f });
            ImPlot3D.EndPlot();
        }

        ImGui.End();
    }
}
