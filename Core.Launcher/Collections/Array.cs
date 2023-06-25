using Core.Launcher.Domain;
using System.Collections;

namespace Core.Launcher.Collections;

public readonly struct Array<TSave, TElement> : IReadOnlyList<TElement>
    where TElement : IElement<TSave, TElement>
{
    public TSave Save { get; }

    public Pointer Pointer { get; }

    public int Count { get; }

    public TElement this[int index] => index < Count ? TElement.Create(Save, Pointer.Offset(index * TElement.GetSize())) : throw new IndexOutOfRangeException();

    public Array(TSave save, Pointer pointer, int count)
    {
        Save = save;
        Pointer = pointer;
        Count = count;
    }

    public IEnumerator<TElement> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<TElement>
    {
        private readonly Array<TSave, TElement> _array;

        private int _next = -1;

        public Enumerator(Array<TSave, TElement> array)
        {
            _array = array;
        }

        public bool MoveNext()
        {
            _next++;

            return _next < _array.Count;
        }

        public void Reset()
        {
            _next = -1;
        }

        public TElement Current => _array[_next];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}