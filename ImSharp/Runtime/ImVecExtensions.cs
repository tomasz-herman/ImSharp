using System.Numerics;

namespace ImSharp;

public partial struct ImVec2
{
    public static ImVec2 Zero => new(0f, 0f);
    public static ImVec2 One => new(1f, 1f);

    public ImVec2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2(ImVec2 v) => new(v.x, v.y);
    public static implicit operator ImVec2(Vector2 v) => new(v.X, v.Y);

    public static ImVec2 operator +(ImVec2 a, ImVec2 b) => new(a.x + b.x, a.y + b.y);
    public static ImVec2 operator -(ImVec2 a, ImVec2 b) => new(a.x - b.x, a.y - b.y);
    public static ImVec2 operator *(ImVec2 a, float s) => new(a.x * s, a.y * s);
    public static ImVec2 operator /(ImVec2 a, float s) => new(a.x / s, a.y / s);

    public readonly override string ToString() => $"({x}, {y})";
}

public partial struct ImVec4
{
    public static ImVec4 Zero => new(0f, 0f, 0f, 0f);
    public static ImVec4 One => new(1f, 1f, 1f, 1f);

    public ImVec4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator Vector4(ImVec4 v) => new(v.x, v.y, v.z, v.w);
    public static implicit operator ImVec4(Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public readonly override string ToString() => $"({x}, {y}, {z}, {w})";
}

public partial struct ImColor
{
    public ImColor(float r, float g, float b, float a = 1f)
    {
        Value = new ImVec4(r, g, b, a);
    }

    public ImColor(ImVec4 rgba)
    {
        Value = rgba;
    }

    public ImColor(Vector4 rgba)
    {
        Value = rgba;
    }
}

public partial struct ImPlotPoint
{
    public ImPlotPoint(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public readonly override string ToString() => $"({x}, {y})";
}

public partial struct ImPlotRange
{
    public ImPlotRange(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public readonly override string ToString() => $"[{Min}, {Max}]";
}

public unsafe partial struct ImPlotSpec
{
    public static readonly ImVec4 AutoColor = new(0f, 0f, 0f, -1f);
    public const int Auto = -1;

    public static ImPlotSpec Default => new();

    public ImPlotSpec()
    {
        LineColor = AutoColor;
        LineColors = null;
        LineWeight = 1.0f;
        FillColor = AutoColor;
        FillColors = null;
        FillAlpha = 1.0f;
        Marker = ImPlotMarker.None;
        MarkerSize = 4.0f;
        MarkerSizes = null;
        MarkerLineColor = AutoColor;
        MarkerLineColors = null;
        MarkerFillColor = AutoColor;
        MarkerFillColors = null;
        Size = 4.0f;
        Offset = 0;
        Stride = Auto;
        Flags = ImPlotItemFlags.None;
    }
}

public unsafe partial struct ImPlot3DSpec
{
    public static readonly ImVec4 AutoColor = new(0f, 0f, 0f, -1f);
    public const int Auto = -1;

    public static ImPlot3DSpec Default => new();

    public ImPlot3DSpec()
    {
        LineColor = AutoColor;
        LineColors = null;
        LineWeight = 1.0f;
        FillColor = AutoColor;
        FillColors = null;
        FillAlpha = Auto;
        Marker = ImPlot3DMarker.Auto;
        MarkerSize = Auto;
        MarkerSizes = null;
        MarkerLineColor = AutoColor;
        MarkerLineColors = null;
        MarkerFillColor = AutoColor;
        MarkerFillColors = null;
        Offset = 0;
        Stride = Auto;
        Flags = ImPlot3DItemFlags.None;
    }
}
