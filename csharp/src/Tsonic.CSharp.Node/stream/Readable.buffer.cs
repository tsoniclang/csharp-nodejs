using System;

namespace Tsonic.CSharp.Node;

public partial class Readable
{
    private readonly struct ReadableChunk(object? value, int size)
    {
        public object? Value { get; } = value;
        public int Size { get; } = size;
    }

    private sealed class ReadableChunkQueue
    {
        private ReadableChunk[] _items = Array.Empty<ReadableChunk>();
        private int _head;

        public int Count { get; private set; }

        public ref ReadableChunk First
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException("The readable buffer is empty.");
                return ref _items[_head];
            }
        }

        public void AddFirst(ReadableChunk chunk)
        {
            EnsureCapacity();
            _head = _head == 0 ? _items.Length - 1 : _head - 1;
            _items[_head] = chunk;
            Count++;
        }

        public void AddLast(ReadableChunk chunk)
        {
            EnsureCapacity();
            var remaining = _items.Length - _head;
            var index = Count >= remaining ? Count - remaining : _head + Count;
            _items[index] = chunk;
            Count++;
        }

        public void RemoveFirst()
        {
            if (Count == 0)
                throw new InvalidOperationException("The readable buffer is empty.");
            _items[_head] = default;
            _head = _head == _items.Length - 1 ? 0 : _head + 1;
            if (--Count == 0)
                _head = 0;
        }

        public void Clear()
        {
            var prefix = Math.Min(Count, _items.Length - _head);
            Array.Clear(_items, _head, prefix);
            Array.Clear(_items, 0, Count - prefix);
            _head = 0;
            Count = 0;
        }

        private void EnsureCapacity()
        {
            if (Count < _items.Length)
                return;
            if (Count == Array.MaxLength)
                throw new OutOfMemoryException("The readable buffer exceeds the native array capacity.");
            var capacity = _items.Length == 0 ? 4
                : _items.Length > Array.MaxLength / 2 ? Array.MaxLength : _items.Length * 2;
            var items = new ReadableChunk[capacity];
            var prefix = Math.Min(Count, _items.Length - _head);
            Array.Copy(_items, _head, items, 0, prefix);
            Array.Copy(_items, 0, items, prefix, Count - prefix);
            _items = items;
            _head = 0;
        }
    }
}
