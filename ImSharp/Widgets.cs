using System.Runtime.InteropServices;

namespace ImSharp;

public partial class Widgets
{
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiShowDemoWindow();
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImPlotShowDemoWindow();
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiNewFrame();
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiRender();
}