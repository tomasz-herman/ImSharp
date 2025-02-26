namespace ImSharp;

public unsafe struct ImBitArrayForNamedKeys
{
    private const int BitCount = (int)ImGuiKey.NamedKey_COUNT;
    private const int Offset = (int)ImGuiKey.NamedKey_BEGIN;
    private const int StorageSize = (BitCount + 31) >> 5;

    public fixed uint Storage[StorageSize];
}