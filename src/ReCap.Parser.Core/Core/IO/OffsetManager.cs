namespace ReCap.Parser.Core.Core;

using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public sealed class OffsetManager
{
    private long _primary;
    private long _secondary;
    private long _displaySecondary;
    private readonly FileStream _stream;

    public OffsetManager(FileStream stream)
    {
        _stream = stream;
    }

    public void SetPrimaryOffset(long offset) => _primary = offset;
    public void SetSecondaryOffset(long offset) => _secondary = offset;
    public void SetDisplaySecondaryOffset(long offset) => _displaySecondary = offset;

    public long GetPrimaryOffset() => _primary;
    public long GetSecondaryOffset() => _displaySecondary > 0 ? _displaySecondary : _secondary;
    public long GetRealSecondaryOffset() => _secondary;

    public void AdvancePrimary(int bytes) => _primary += bytes;
    public void AdvancePrimary(long bytes) => _primary += bytes;
    public void AdvanceSecondary(int bytes) => _secondary += bytes;
    public void AdvanceSecondary(long bytes) => _secondary += bytes;

    private void EnsureReadable(long offset, int size)
    {
        if (offset < 0 || offset + size > _stream.Length) throw new EndOfStreamException();
    }

    public T ReadPrimary<T>() where T : unmanaged
    {
        var size = SizeOf<T>();
        EnsureReadable(_primary, size);
        var v = ReadAtCore<T>(_primary);
        _primary += size;
        return v;
    }

    public T ReadSecondary<T>() where T : unmanaged
    {
        var size = SizeOf<T>();
        EnsureReadable(_secondary, size);
        var v = ReadAtCore<T>(_secondary);
        _secondary += size;
        return v;
    }

    public T ReadAt<T>(long offset) where T : unmanaged
    {
        var size = SizeOf<T>();
        EnsureReadable(offset, size);
        return ReadAtCore<T>(offset);
    }

    private T ReadAtCore<T>(long offset) where T : unmanaged
    {
        var size = SizeOf<T>();
        Span<byte> buf = stackalloc byte[size];
        _stream.Position = offset;
        var read = _stream.Read(buf);
        if (read != size) throw new EndOfStreamException();

        if (typeof(T) == typeof(byte)) return (T)(object)buf[0];
        if (typeof(T) == typeof(bool)) return (T)(object)(buf[0] != 0);
        if (typeof(T) == typeof(ushort)) return (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(buf);
        if (typeof(T) == typeof(short)) return (T)(object)BinaryPrimitives.ReadInt16LittleEndian(buf);
        if (typeof(T) == typeof(uint)) return (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(buf);
        if (typeof(T) == typeof(int)) return (T)(object)BinaryPrimitives.ReadInt32LittleEndian(buf);
        if (typeof(T) == typeof(ulong)) return (T)(object)BinaryPrimitives.ReadUInt64LittleEndian(buf);
        if (typeof(T) == typeof(long)) return (T)(object)BinaryPrimitives.ReadInt64LittleEndian(buf);
        if (typeof(T) == typeof(float)) return (T)(object)MemoryMarshal.Read<float>(buf);

        throw new NotSupportedException(typeof(T).FullName);
    }

    private static int SizeOf<T>() where T : unmanaged
    {
        if (typeof(T) == typeof(byte) || typeof(T) == typeof(bool)) return 1;
        if (typeof(T) == typeof(ushort) || typeof(T) == typeof(short)) return 2;
        if (typeof(T) == typeof(uint) || typeof(T) == typeof(int) || typeof(T) == typeof(float)) return 4;
        if (typeof(T) == typeof(ulong) || typeof(T) == typeof(long)) return 8;
        throw new NotSupportedException(typeof(T).FullName);
    }

    public string ReadString(bool useSecondary = false)
    {
        var start = useSecondary ? _secondary : _primary;
        var sb = new StringBuilder(64);
        _stream.Position = start;
        while (true)
        {
            var b = _stream.ReadByte();
            if (b < 0) break;
            if (b == 0) break;
            sb.Append((char)b);
        }
        var newOffset = start + sb.Length + 1;
        if (useSecondary) _secondary = newOffset;
        else _primary = newOffset;
        return sb.ToString();
    }
}
