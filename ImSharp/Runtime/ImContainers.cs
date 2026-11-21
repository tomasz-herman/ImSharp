using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ImSharp;

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImVector&lt;T&gt;</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImVector<T> where T : unmanaged
{
    public int Size;
    public int Capacity;
    public T* Data;

    public readonly bool IsEmpty => Size <= 0 || Data == null;

    public readonly ref T this[int index] => ref Data[index];

    public readonly Span<T> AsSpan() => Data == null || Size <= 0 ? Span<T>.Empty : new Span<T>(Data, Size);
}

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImVector&lt;T*&gt;</c> (used where C# generics cannot accept pointer type arguments).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImVectorPtr
{
    public int Size;
    public int Capacity;
    public nint* Data;

    public readonly bool IsEmpty => Size <= 0 || Data == null;

    public readonly ref nint this[int index] => ref Data[index];

    public readonly Span<nint> AsSpan() => Data == null || Size <= 0 ? Span<nint>.Empty : new Span<nint>(Data, Size);
}

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImSpan&lt;T&gt;</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ImSpan<T> where T : unmanaged
{
    public T* Data;
    public T* DataEnd;

    public readonly int Length => Data == null || DataEnd == null ? 0 : (int)(DataEnd - Data);

    public readonly Span<T> AsSpan() => Data == null || Length <= 0 ? Span<T>.Empty : new Span<T>(Data, Length);
}

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImChunkStream&lt;T&gt;</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ImChunkStream
{
    public ImVector<byte> Buf;
}

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImPool&lt;T&gt;</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ImPool<T> where T : unmanaged
{
    public ImVector<T> Buf;
    public ImGuiStorage Map;
    public int FreeIdx;
    public int AliveCount;
}

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImStableVector&lt;T, BLOCK_SIZE&gt;</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ImStableVector<T> where T : unmanaged
{
    public int Size;
    public int Capacity;
    public ImVectorPtr Blocks;
}

/// <summary>
/// Blittable representation of Dear ImGui's <c>ImSmallStack_&lt;ImDrawFlags, 3&gt;</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ImSmallStack__ImDrawFlags__3
{
    public ImDrawFlags LocalData0;
    public ImDrawFlags LocalData1;
    public ImDrawFlags LocalData2;
    public ushort Size;
}
