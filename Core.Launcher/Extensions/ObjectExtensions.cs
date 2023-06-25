using System.Runtime.CompilerServices;
using Core.Abstract.Domain;
using Core.Launcher.Domain;

namespace Core.Launcher.Extensions;

public static class ObjectExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue GetStored<TObject, TValue>(this TObject @object, IStore<TValue> store, int offset)
        where TObject : IObject
    {
        var id = @object.GetInt32(offset);

        return store.Get(id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetStored<TObject, TValue>(this TObject @object, IStore<TValue> store, int offset, TValue value)
        where TObject : IObject
    {
        var id = store.Get(value);

        @object.SetInt32(offset, id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<TValue> GetSpan<TObject, TValue>(this TObject @object, int offset, int length)
        where TValue : struct
        where TObject : IObject
    {
        var pointer = @object.GetPointer(offset);

        return new Span<TValue>(pointer, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int ExchangeInt32<TObject>(this TObject @object, int offset, int value)
        where TObject : IObject
    {
        var p = @object.GetInt32Pointer(offset);

        var result = *p;

        *p = value;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool GetFlag<TObject>(this TObject @object, int offset, int flag)
        where TObject : IObject
    {
        return (*@object.GetUInt32Pointer(offset) & (1 << flag)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetFlag<TObject>(this TObject @object, int offset, int flag, bool value)
        where TObject : IObject
    {
        var p = @object.GetUInt32Pointer(offset);

        if (value) *p |= (uint)(1 << flag);

        else *p &= (uint)~(1 << flag);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe sbyte GetSByte<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetSBytePointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetSByte<TObject>(this TObject @object, int offset, sbyte value)
        where TObject : IObject
    {
        *@object.GetSBytePointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe sbyte* GetSBytePointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return (sbyte*)@object.GetPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ushort GetUInt16<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetUInt16Pointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetUInt16<TObject>(this TObject @object, int offset, ushort value)
        where TObject : IObject
    {
        *@object.GetUInt16Pointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe ushort* GetUInt16Pointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return (ushort*)@object.GetPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetInt32<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetInt32Pointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetInt32<TObject>(this TObject @object, int offset, int value)
        where TObject : IObject
    {
        *@object.GetInt32Pointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int* GetInt32Pointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return (int*)@object.GetPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint GetUInt32<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetUInt32Pointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetUInt32<TObject>(this TObject @object, int offset, uint value)
        where TObject : IObject
    {
        *@object.GetUInt32Pointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe uint* GetUInt32Pointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return (uint*)@object.GetPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long GetInt64<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetInt64Pointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetInt64<TObject>(this TObject @object, int offset, long value)
        where TObject : IObject
    {
        *@object.GetInt64Pointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe long* GetInt64Pointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return (long*)@object.GetPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool GetBoolean<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetBooleanPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetBoolean<TObject>(this TObject @object, int offset, bool value)
        where TObject : IObject
    {
        *@object.GetBooleanPointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe bool* GetBooleanPointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return (bool*)@object.GetPointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe byte GetByte<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return *@object.GetBytePointer(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SetByte<TObject>(this TObject @object, int offset, byte value)
        where TObject : IObject
    {
        *@object.GetBytePointer(offset) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe byte* GetBytePointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return @object.Pointer.Value + offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void* GetPointer<TObject>(this TObject @object, int offset)
        where TObject : IObject
    {
        return @object.Pointer.Value + offset;
    }
}