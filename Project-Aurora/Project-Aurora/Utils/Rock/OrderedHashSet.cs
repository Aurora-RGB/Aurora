using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AuroraRgb.Utils.Rock;

/// <summary>
/// Represents an ordered set of values.
/// From Rock.Collections repository, heavily modified.
/// </summary> 
/// <remarks>
/// Values are kept in order in which they are added.
/// Order can be modified by <see cref="MoveFirst(T)"/>, <see cref="MoveLast(T)"/>, <see cref="MoveBefore(T, T)"/>, <see cref="MoveAfter(T, T)"/>.
/// </remarks>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
public class OrderedHashSet<T> : ICollection<T> where T : notnull
{
    // factor used to increase hashset capacity
    private const int GrowthFactor = 2;

    private int[] _mBuckets;
    private Slot[] _mSlots;
    private int _mLastIndex;
    private int _mFreeList;
    private int _mVersion;
    private int _mFirstOrderIndex; // Index of first entry by order
    private int _mLastOrderIndex; // Index of last entry by order

    /// <summary> 
    /// Number of elements in this hashset
    /// </summary>
    public int Count { get; private set; }

    #region Constructors

    public OrderedHashSet()
        : this(EqualityComparer<T>.Default)
    {
    }

    public OrderedHashSet(int capacity)
        : this(capacity, EqualityComparer<T>.Default)
    {
    }

    public OrderedHashSet(IEqualityComparer<T> comparer)
        : this(0, comparer)
    {
    }

    public OrderedHashSet(int capacity, IEqualityComparer<T>? comparer)
    {
        comparer ??= EqualityComparer<T>.Default;

        Comparer = comparer;
        _mLastIndex = 0;
        Count = 0;
        _mFreeList = -1;
        _mVersion = 0;
        _mFirstOrderIndex = -1;
        _mLastOrderIndex = -1;

        if (capacity > 0)
        {
            var size = HashHelpers.GetPrime(capacity);

            _mBuckets = new int[size];
            _mSlots = new Slot[size];
        }
        else
        {
            _mBuckets = [];
            _mSlots = [];
        }
    }

    public OrderedHashSet(IEnumerable<T> collection)
        : this(collection, EqualityComparer<T>.Default)
    {
    }

    /// <summary>
    /// Implementation Notes: 
    /// Since resizes are relatively expensive (require rehashing), this attempts to minimize 
    /// the need to resize by setting the initial capacity based on size of collection.
    /// </summary> 
    /// <param name="collection"></param>
    /// <param name="comparer"></param>
    public OrderedHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        : this(comparer)
    {
        // to avoid excess resizes, first set size based on collection's count.
        var suggestedCapacity = 0;
        if (collection is ICollection<T> coll)
        {
            suggestedCapacity = coll.Count;
        }

        var size = HashHelpers.GetPrime(suggestedCapacity);

        _mBuckets = new int[size];
        _mSlots = new Slot[size];

        UnionWith(collection);
    }

    #endregion

    // index access
    public T this[int index] => _mSlots[index].Value;

    #region ICollection<T> methods

    /// <summary> 
    /// Whether this is readonly
    /// </summary> 
    bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// Add item to this hashset. This is the explicit implementation of the ICollection
    /// interface. The other Add method returns bool indicating whether item was added. 
    /// </summary>
    /// <param name="item">item to add</param> 
    void ICollection<T>.Add(T item)
    {
        Add(item);
    }

    /// <summary>
    /// Remove all items from this set. This clears the elements but not the underlying
    /// buckets and slots array.
    /// </summary>
    public void Clear()
    {
        if (_mLastIndex > 0)
        {
            Debug.Assert(_mBuckets != null, "m_buckets was null but m_lastIndex > 0");

            // clear the elements so that the gc can reclaim the references.
            // clear only up to m_lastIndex for m_slots
            Array.Clear(_mSlots, 0, _mLastIndex);
            Array.Clear(_mBuckets, 0, _mBuckets.Length);
            _mLastIndex = 0;
            Count = 0;
            _mFreeList = -1;
            _mFirstOrderIndex = -1;
            _mLastOrderIndex = -1;
        }

        _mVersion++;
    }

    /// <summary>
    /// Checks if this hashset contains the item 
    /// </summary>
    /// <param name="item">item to check for containment</param> 
    /// <returns>true if item contained; false if not</returns> 
    public bool Contains(T item)
    {
        if (_mBuckets.Length == 0)
        {
            return false;
        }

        var hashCode = InternalGetHashCode(item);
        // see note at "HashSet" level describing why "- 1" appears in for loop
        for (var i = _mBuckets[hashCode % _mBuckets.Length] - 1; i >= 0; i = _mSlots[i].Next)
        {
            if (_mSlots[i].HashCode == hashCode && Comparer.Equals(_mSlots[i].Value, item))
            {
                return true;
            }
        }

        // either m_buckets is null or wasn't found 
        return false;
    }

    /// <summary> 
    /// Take the union of this HashSet with other. Modifies this set.
    /// 
    /// Implementation note: GetSuggestedCapacity (to increase capacity in advance avoiding
    /// multiple resizes ended up not being useful in practice; quickly gets to the
    /// point where it's a wasteful check.
    /// </summary> 
    /// <param name="other">enumerable with items to add</param>
    public void UnionWith(IEnumerable<T> other)
    {
        // Fast path for ICollection<T> (List<T>, T[], HashSet<T>, etc.)
        if (other is ICollection<T> { Count: > 0 } coll)
        {
            EnsureCapacityForUnion(coll.Count);
        }

        // Enumerate and insert
        foreach (var item in other)
        {
            AddFast(item);
        }
    }

    private void EnsureCapacityForUnion(int incomingCount)
    {
        var needed = Count + incomingCount;
        if (needed <= _mSlots.Length)
            return;

        // Proactively grow to final size
        var newSize = HashHelpers.GetPrime(needed);
        ResizeTo(newSize);
    }
    
    private void ResizeTo(int newSize)
    {
        var newSlots = new Slot[newSize];
        Array.Copy(_mSlots, 0, newSlots, 0, _mLastIndex);

        var newBuckets = new int[newSize];

        for (var i = 0; i < _mLastIndex; i++)
        {
            var hash = newSlots[i].HashCode;
            if (hash >= 0)
            {
                var bucket = hash % newSize;
                newSlots[i].Next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
        }

        _mSlots = newSlots;
        _mBuckets = newBuckets;
    }

    private int InternalIndexOf(T item)
    {
        if (_mBuckets.Length == 0)
        {
            return -1;
        }

        var num = InternalGetHashCode(item);
        for (var i = _mBuckets[num % _mBuckets.Length] - 1; i >= 0; i = _mSlots[i].Next)
        {
            if (_mSlots[i].HashCode == num && Comparer.Equals(_mSlots[i].Value, item))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary> 
    /// Remove item from this hashset
    /// </summary> 
    /// <param name="item">item to remove</param> 
    /// <returns>true if removed; false if not (i.e. if the item wasn't in the HashSet)</returns>
    public bool Remove(T item)
    {
        var hashCode = InternalGetHashCode(item);
        var bucket = hashCode % _mBuckets.Length;
        var last = -1;
        for (var i = _mBuckets[bucket] - 1; i >= 0; last = i, i = _mSlots[i].Next)
        {
            if (_mSlots[i].HashCode != hashCode || !Comparer.Equals(_mSlots[i].Value, item)) continue;
            if (last < 0)
            {
                // first iteration; update buckets
                _mBuckets[bucket] = _mSlots[i].Next + 1;
            }
            else
            {
                // subsequent iterations; update 'next' pointers
                _mSlots[last].Next = _mSlots[i].Next;
            }

            // Connect linked list
            if (_mFirstOrderIndex == i) // Is first
            {
                _mFirstOrderIndex = _mSlots[i].NextOrder;
            }

            if (_mLastOrderIndex == i) // Is last
            {
                _mLastOrderIndex = _mSlots[i].PreviousOrder;
            }

            var next = _mSlots[i].NextOrder;
            var prev = _mSlots[i].PreviousOrder;
            if (next != -1)
            {
                _mSlots[next].PreviousOrder = prev;
            }

            if (prev != -1)
            {
                _mSlots[prev].NextOrder = next;
            }

            _mSlots[i].HashCode = -1;
            _mSlots[i].Value = default;
            _mSlots[i].Next = _mFreeList;
            _mSlots[i].PreviousOrder = -1;
            _mSlots[i].NextOrder = -1;

            Count--;
            _mVersion++;
            if (Count == 0)
            {
                _mLastIndex = 0;
                _mFreeList = -1;
            }
            else
            {
                _mFreeList = i;
            }

            return true;
        }

        // either m_buckets is null or wasn't found
        return false;
    }

    #endregion

    #region IEnumerable methods

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    #endregion

    #region HashSet methods

    /// <summary> 
    /// Copy items in this hashset to array, starting at arrayIndex
    /// </summary> 
    /// <param name="array">array to add items to</param> 
    /// <param name="arrayIndex">index to start at</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo(array, arrayIndex, Count);
    }

    /// <summary>
    /// Copies the elements to an array.
    /// </summary>
    public void CopyTo(T[] array)
    {
        CopyTo(array, 0, Count);
    }

    /// <summary>
    /// Copies the specified number of elements to an array, starting at the specified array index.
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex, int count)
    {
        // check array index valid index into array 
        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), @"ArgumentOutOfRange_NeedNonNegNum");
        }

        // also throw if count less than 0
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), @"ArgumentOutOfRange_NeedNonNegNum");
        }

        // will array, starting at arrayIndex, be able to hold elements? Note: not
        // checking arrayIndex >= array.Length (consistency with list of allowing
        // count of 0; subsequent check takes care of the rest)
        if (arrayIndex > array.Length || count > array.Length - arrayIndex)
        {
            throw new ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        var numCopied = 0;
        for (var i = _mFirstOrderIndex; i != -1 && numCopied < count; i = _mSlots[i].NextOrder)
        {
            array[arrayIndex + numCopied] = _mSlots[i].Value;
            numCopied++;
        }
    }

    public bool MoveFirst(T item)
    {
        var index = InternalIndexOf(item);
        if (index != -1)
        {
            var prev = _mSlots[index].PreviousOrder;
            if (prev != -1) // Not first
            {
                // Disconnect
                var next = _mSlots[index].NextOrder;
                if (next == -1) // Last
                {
                    _mLastOrderIndex = prev;
                }
                else
                {
                    _mSlots[next].PreviousOrder = prev;
                }

                _mSlots[prev].NextOrder = next;

                // Reconnect
                _mSlots[index].PreviousOrder = -1;
                _mSlots[index].NextOrder = _mFirstOrderIndex;
                _mSlots[_mFirstOrderIndex].PreviousOrder = index;
                _mFirstOrderIndex = index;
            }

            return true;
        }

        return false;
    }

    public bool MoveLast(T item)
    {
        var index = InternalIndexOf(item);
        if (index != -1)
        {
            var next = _mSlots[index].NextOrder;
            if (next != -1) // Not last
            {
                // Disconnect
                var prev = _mSlots[index].PreviousOrder;
                if (prev == -1) // First
                {
                    _mFirstOrderIndex = next;
                }
                else
                {
                    _mSlots[prev].NextOrder = next;
                }

                _mSlots[next].PreviousOrder = prev;

                // Reconnect
                _mSlots[index].NextOrder = -1;
                _mSlots[index].PreviousOrder = _mLastOrderIndex;
                _mSlots[_mLastOrderIndex].NextOrder = index;
                _mLastOrderIndex = index;
            }

            return true;
        }

        return false;
    }

    public bool MoveBefore(T itemToMove, T mark)
    {
        var index = InternalIndexOf(itemToMove);
        var markIndex = InternalIndexOf(mark);
        if (index != -1 && markIndex != -1 && index != markIndex)
        {
            // Disconnect
            var next = _mSlots[index].NextOrder;
            var prev = _mSlots[index].PreviousOrder;
            if (prev == -1) // First
            {
                _mFirstOrderIndex = next;
            }
            else
            {
                _mSlots[prev].NextOrder = next;
            }

            if (next == -1) // Last
            {
                _mLastOrderIndex = prev;
            }
            else
            {
                _mSlots[next].PreviousOrder = prev;
            }

            // Reconnect
            var preMark = _mSlots[markIndex].PreviousOrder;
            _mSlots[index].NextOrder = markIndex;
            _mSlots[index].PreviousOrder = preMark;
            _mSlots[markIndex].PreviousOrder = index;
            if (preMark == -1)
            {
                _mFirstOrderIndex = index;
            }
            else
            {
                _mSlots[preMark].NextOrder = index;
            }

            return true;
        }

        return false;
    }

    public bool MoveAfter(T itemToMove, T mark)
    {
        var index = InternalIndexOf(itemToMove);
        var markIndex = InternalIndexOf(mark);
        if (index != -1 && markIndex != -1 && index != markIndex)
        {
            // Disconnect
            var next = _mSlots[index].NextOrder;
            var prev = _mSlots[index].PreviousOrder;
            if (prev == -1) // First
            {
                _mFirstOrderIndex = next;
            }
            else
            {
                _mSlots[prev].NextOrder = next;
            }

            if (next == -1) // Last
            {
                _mLastOrderIndex = prev;
            }
            else
            {
                _mSlots[next].PreviousOrder = prev;
            }

            // Reconnect
            var postMark = _mSlots[markIndex].NextOrder;
            _mSlots[index].PreviousOrder = markIndex;
            _mSlots[index].NextOrder = postMark;
            _mSlots[markIndex].NextOrder = index;
            if (postMark == -1)
            {
                _mLastOrderIndex = index;
            }
            else
            {
                _mSlots[postMark].PreviousOrder = index;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns enumeration which goes from <paramref name="item"/> to last element in the set (including both).
    /// When <paramref name="item"/> is not found, returns empty enumeration.
    /// </summary>
    public Range StartWith(T item)
    {
        return new Range(this, item);
    }

    /// <summary>
    /// Remove elements that match specified predicate. Returns the number of elements removed 
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    public int RemoveWhere(Predicate<T> match)
    {
        var numRemoved = 0;
        for (var i = 0; i < _mLastIndex; i++)
        {
            if (_mSlots[i].HashCode < 0) continue;
            // cache value in case delegate removes it
            var value = _mSlots[i].Value;
            if (match(value) && Remove(value))
            {
                // check again that remove actually removed it 
                numRemoved++;
            }
        }

        return numRemoved;
    }

    /// <summary> 
    /// Gets the IEqualityComparer that is used to determine equality of keys for
    /// the HashSet. 
    /// </summary>
    public IEqualityComparer<T> Comparer { get; private set; }

    #endregion

    #region Helper methods

    /// <summary> 
    /// Expand to new capacity. New capacity is next prime greater than or equal to suggested
    /// size. This is called when the underlying array is filled. This performs no 
    /// defragmentation, allowing faster execution; note that this is reasonable since
    /// AddIfNotPresent attempts to insert new elements in re-opened spots.
    /// </summary>
    private void IncreaseCapacity()
    {
        Debug.Assert(_mBuckets != null, "IncreaseCapacity called on a set with no elements");

        // Handle overflow conditions. Try to expand capacity by GrowthFactor. If that causes
        // overflow, use size suggestion of m_count and see if HashHelpers returns a value 
        // greater than that. If not, capacity can't be increased so throw capacity overflow
        // exception.
        var sizeSuggestion = unchecked(Count * GrowthFactor);
        if (sizeSuggestion < 0)
        {
            sizeSuggestion = Count;
        }

        var newSize = HashHelpers.GetPrime(sizeSuggestion);
        if (newSize <= Count)
        {
            throw new ArgumentException("Arg_HSCapacityOverflow");
        }

        // Able to increase capacity; copy elements to larger array and rehash
        var newSlots = new Slot[newSize];
            Array.Copy(_mSlots, 0, newSlots, 0, _mLastIndex);

        var newBuckets = new int[newSize];
        for (var i = 0; i < _mLastIndex; i++)
        {
            var bucket = newSlots[i].HashCode % newSize;
            newSlots[i].Next = newBuckets[bucket] - 1;
            newBuckets[bucket] = i + 1;
        }

        _mSlots = newSlots;
        _mBuckets = newBuckets;
    }

    /// <summary> 
    /// Add item to this HashSet. Returns bool indicating whether item was added (won't be 
    /// added if already present)
    /// </summary> 
    /// <param name="value"></param>
    /// <returns>true if added, false if already present</returns>
    public bool Add(T value)
    {
        var hashCode = InternalGetHashCode(value);
        var bucket = hashCode % _mBuckets.Length;
        for (var i = _mBuckets[hashCode % _mBuckets.Length] - 1; i >= 0; i = _mSlots[i].Next)
        {
            if (_mSlots[i].HashCode == hashCode && Comparer.Equals(_mSlots[i].Value, value))
            {
                return false;
            }
        }

        int index;
        if (_mFreeList >= 0)
        {
            index = _mFreeList;
            _mFreeList = _mSlots[index].Next;
        }
        else
        {
            if (_mLastIndex == _mSlots.Length)
            {
                IncreaseCapacity();
                // this will change during resize
                bucket = hashCode % _mBuckets.Length;
            }

            index = _mLastIndex;
            _mLastIndex++;
        }

        _mSlots[index].HashCode = hashCode;
        _mSlots[index].Value = value;
        _mSlots[index].Next = _mBuckets[bucket] - 1;

        // Append to linked list
        if (_mLastOrderIndex != -1)
        {
            _mSlots[_mLastOrderIndex].NextOrder = index;
        }

        if (_mFirstOrderIndex == -1)
        {
            _mFirstOrderIndex = index;
        }

        _mSlots[index].NextOrder = -1;
        _mSlots[index].PreviousOrder = _mLastOrderIndex;
        _mLastOrderIndex = index;

        _mBuckets[bucket] = index + 1;
        Count++;
        _mVersion++;
        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AddFast(T value)
    {
        var hash = Comparer.GetHashCode(value);
        var bucketIndex = hash % _mBuckets.Length;

        // search for existing
        for (var i = _mBuckets[bucketIndex] - 1; i >= 0; i = _mSlots[i].Next)
        {
            if (_mSlots[i].HashCode == hash && Comparer.Equals(_mSlots[i].Value, value))
                return false;
        }

        int index;
        if (_mFreeList >= 0)
        {
            index = _mFreeList;
            _mFreeList = _mSlots[index].Next;
        }
        else
        {
            index = _mLastIndex++;
        }

        _mSlots[index].HashCode = hash;
        _mSlots[index].Value = value;
        _mSlots[index].Next = _mBuckets[bucketIndex] - 1;

        // append to ordering
        var last = _mLastOrderIndex;
        if (last != -1)
            _mSlots[last].NextOrder = index;

        if (_mFirstOrderIndex == -1)
            _mFirstOrderIndex = index;

        _mSlots[index].PreviousOrder = last;
        _mSlots[index].NextOrder = -1;

        _mLastOrderIndex = index;

        _mBuckets[bucketIndex] = index + 1;
        Count++;
        _mVersion++;

        return true;
    }

    /// <summary> 
    /// Workaround Comparers that throw ArgumentNullException for GetHashCode(null).
    /// </summary> 
    /// <param name="item"></param>
    /// <returns>hash code</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int InternalGetHashCode(T item)
    {
        return Comparer.GetHashCode(item);
    }

    #endregion

    internal struct Slot
    {
        internal int HashCode; // Lower 31 bits of hash code, -1 if unused
        internal T Value;
        internal int Next; // Index of next entry, -1 if last
        internal int NextOrder; // Index of next entry by order, -1 if last
        internal int PreviousOrder; // Index of previous entry by order, -1 if first
    }

    /// <summary>
    /// Part of <see cref="OrderedHashSet{T}"/> starting with specified element.
    /// Enumeration goes from specified element to last element in collection.
    /// Returns empty enumeration when specified item is not in collection.
    /// </summary>
    public struct Range(OrderedHashSet<T> set, T startingItem) : IEnumerable<T>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(set, set.InternalIndexOf(startingItem));
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(set, set.InternalIndexOf(startingItem));
        }
    }

    [Serializable]
    public sealed class Enumerator : IEnumerator<T>
    {
        private OrderedHashSet<T> _mSet;
        private int _mIndex;
        private int _mVersion;

        public T Current { get; private set; }

        internal Enumerator(OrderedHashSet<T> set)
            : this(set, set._mFirstOrderIndex)
        {
        }

        internal Enumerator(OrderedHashSet<T> set, int startIndex)
        {
            _mSet = set;
            _mIndex = startIndex;
            _mVersion = set._mVersion;
            Current = _mSet._mSlots[_mIndex].Value;
        }

        public bool MoveNext()
        {
            if (_mVersion != _mSet._mVersion)
            {
                throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
            }

            if (_mIndex == -1)
            {
                return false;
            }

            Current = _mSet._mSlots[_mIndex].Value;
            _mIndex = _mSet._mSlots[_mIndex].NextOrder;
            return true;
        }

        void IDisposable.Dispose()
        {
        }

        object IEnumerator.Current
        {
            get
            {
                if (_mIndex == _mSet._mFirstOrderIndex || _mIndex == -1)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                }

                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }
}