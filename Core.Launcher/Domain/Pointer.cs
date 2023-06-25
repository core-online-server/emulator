using System.Runtime.CompilerServices;

namespace Core.Launcher.Domain;

public readonly struct Pointer
{
    public unsafe byte* Value { get; }

    public unsafe Pointer(byte* value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pointer Offset(int offset)
    {
        unsafe
        {
            return new Pointer(Value + offset);
        }
    }

    public override string ToString()
    {
        unsafe
        {
            return new IntPtr(Value).ToString();
        }
    }
}