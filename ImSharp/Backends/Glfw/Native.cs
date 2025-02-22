using System.Runtime.InteropServices;

namespace ImSharp;

public static partial class Native
{
    [return: MarshalAs(UnmanagedType.U1)]
    [LibraryImport("ImSharpNative")]
    public static unsafe partial bool ImGuiImplGlfwInitForOpenGL(void* window, [MarshalAs(UnmanagedType.U1)] bool installCallbacks);
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiImplGlfwNewFrame();   
    
    [LibraryImport("ImSharpNative")]
    public static partial void ImGuiImplGlfwShutdown();
}