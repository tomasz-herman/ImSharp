using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ImSharp;

/// <summary>
/// Low-overhead UTF-8 marshalling helpers for native C interop.
/// </summary>
public static unsafe class Utf8StringHelper
{
    /// <summary>
    /// Converts a null-terminated native UTF-8 byte pointer to a managed string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? PtrToStringUtf8(byte* ptr)
    {
        return ptr == null ? null : Marshal.PtrToStringUTF8((nint)ptr);
    }

    /// <summary>
    /// Allocates a null-terminated UTF-8 buffer on the unmanaged heap for the given string,
    /// or returns <see langword="null"/> if <paramref name="value"/> is <see langword="null"/>.
    /// Must be freed with <see cref="Free"/>.
    /// </summary>
    public static byte* StringToUtf8Ptr(string? value)
    {
        if (value is null)
            return null;

        int byteCount = Encoding.UTF8.GetByteCount(value);
        byte* ptr = (byte*)NativeMemory.Alloc((nuint)(byteCount + 1));
        int written = Encoding.UTF8.GetBytes(value, new Span<byte>(ptr, byteCount));
        ptr[written] = 0;
        return ptr;
    }

    /// <summary>
    /// Frees a buffer previously allocated by <see cref="StringToUtf8Ptr"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Free(byte* ptr)
    {
        if (ptr != null)
            NativeMemory.Free(ptr);
    }
}

/// <summary>
/// Stack-friendly scoped UTF-8 null-terminated string buffer for P/Invoke calls.
/// Uses a pooled buffer only when the string exceeds stack capacity.
/// </summary>
public unsafe ref struct ScopedUtf8String
{
    private byte* _heapPtr;
    private GCHandle _pinHandle;
    private byte[]? _pooled;
    public readonly byte* Pointer;

    public ScopedUtf8String(string? value, Span<byte> stackBuffer)
    {
        _heapPtr = null;
        _pinHandle = default;
        _pooled = null;

        if (value is null)
        {
            Pointer = null;
            return;
        }

        int maxBytes = Encoding.UTF8.GetMaxByteCount(value.Length) + 1;
        if (maxBytes <= stackBuffer.Length)
        {
            int written = Encoding.UTF8.GetBytes(value, stackBuffer);
            stackBuffer[written] = 0;
            Pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(stackBuffer));
        }
        else
        {
            int exactBytes = Encoding.UTF8.GetByteCount(value) + 1;
            _heapPtr = (byte*)NativeMemory.Alloc((nuint)exactBytes);
            int written = Encoding.UTF8.GetBytes(value, new Span<byte>(_heapPtr, exactBytes - 1));
            _heapPtr[written] = 0;
            Pointer = _heapPtr;
        }
    }

    public void Dispose()
    {
        if (_heapPtr != null)
        {
            NativeMemory.Free(_heapPtr);
            _heapPtr = null;
        }
        if (_pinHandle.IsAllocated)
        {
            _pinHandle.Free();
        }
        if (_pooled != null)
        {
            ArrayPool<byte>.Shared.Return(_pooled);
            _pooled = null;
        }
    }
}
