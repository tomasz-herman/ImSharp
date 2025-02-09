using System.Runtime.InteropServices;

namespace ImSharp;

public partial class Context
{
    [LibraryImport("ImSharpNative")]
    public static partial nint ImGuiCreateContext();
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiDestroyContext();
}