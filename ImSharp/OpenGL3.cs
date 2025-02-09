using System.Runtime.InteropServices;

namespace ImSharp;

public partial class OpenGL3
{
    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport("ImSharpNative", StringMarshalling = StringMarshalling.Utf8)]
    public static unsafe partial bool ImGuiImplOpenGL3Init(string glslVersion = "#version 460 core");
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiImplOpenGL3NewFrame();   
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiImplOpenGL3RenderDrawData();   
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiImplOpenGL3Shutdown();
}