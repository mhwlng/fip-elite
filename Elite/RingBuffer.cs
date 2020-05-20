
namespace Elite
{
    #region License
    /* Copyright 2015 Joe Osborne
     * 
     * This file is part of RingBuffer.
     *
     *  RingBuffer is free software: you can redistribute it and/or modify
     *  it under the terms of the GNU General Public License as published by
     *  the Free Software Foundation, either version 3 of the License, or
     *  (at your option) any later version.
     *
     *  RingBuffer is distributed in the hope that it will be useful,
     *  but WITHOUT ANY WARRANTY; without even the implied warranty of
     *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     *  GNU General Public License for more details.
     *
     *  You should have received a copy of the GNU General Public License
     *  along with RingBuffer. If not, see <http://www.gnu.org/licenses/>.
     */
    #endregion
    using System;
    using System.Collections;
    using System.Collections.Generic;

    namespace RingBuffer
    {
        /// <summary>
        /// A generic ring buffer with fixed capacity.
        /// </summary>
        /// <typeparam name="T">The type of data stored in the buffer</typeparam>
        public class RingBuffer<T> : ICollection<T>,
            ICollection
        {
            private int _head;
            private int _tail;
            private int _size;

            private T[] _buffer;

            private bool allowOverflow;
            public bool AllowOverflow => allowOverflow;

            /// <summary>
            /// The total number of elements the buffer can store (grows).
            /// </summary>
            private int Capacity => _buffer.Length;

            /// <summary>
            /// The number of elements currently contained in the buffer.
            /// </summary>
            public int Size => _size;

            /// <summary>
            /// Retrieve the next item from the buffer.
            /// </summary>
            /// <returns>The oldest item added to the buffer.</returns>
            public T Get()
            {
                if (_size == 0) throw new InvalidOperationException("Buffer is empty.");
                var item = _buffer[_head];
                _head = (_head + 1) % Capacity;
                _size--;
                return item;
            }

            /// <summary>
            /// Adds an item to the end of the buffer.
            /// </summary>
            /// <param name="item">The item to be added.</param>
            public void Put(T item)
            {
                // If tail & head are equal and the buffer is not empty, assume
                // that it will overflow and throw an exception.
                if (_tail == _head && _size != 0)
                {
                    if (allowOverflow)
                    {
                        AddToBuffer(item, true);
                    }
                    else
                    {
                        throw new InvalidOperationException("The RingBuffer is full");
                    }
                }
                // If the buffer will not overflow, just add the item.
                else
                {
                    AddToBuffer(item, false);
                }
            }

            private void AddToBuffer(T toAdd, bool overflow)
            {
                if (overflow)
                {
                    _head = (_head + 1) % Capacity;
                }
                else
                {
                    _size++;
                }
                _buffer[_tail] = toAdd;
                _tail = (_tail + 1) % Capacity;
            }

            #region Constructors
            // Default capacity is 4, default overflow behavior is false.
            public RingBuffer() : this(4) { }

            private RingBuffer(int capacity) : this(capacity, false) { }

            public RingBuffer(int capacity, bool overflow)
            {
                _buffer = new T[capacity];
                allowOverflow = overflow;
            }
            #endregion

            #region IEnumerable Members
            public IEnumerator<T> GetEnumerator()
            {
                var index = _head;
                for (var i = 0; i < _size; i++, index = (index + 1) % Capacity)
                {
                    yield return _buffer[index];
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion

            #region ICollection<T> Members
            public int Count => _size;
            public bool IsReadOnly => false;

            public void Add(T item)
            {
                Put(item);
            }

            /// <summary>
            /// Determines whether the RingBuffer contains a specific value.
            /// </summary>
            /// <param name="item">The value to check the RingBuffer for.</param>
            /// <returns>True if the RingBuffer contains <paramref name="item"/>
            /// , false if it does not.
            /// </returns>
            public bool Contains(T item)
            {
                var comparer = EqualityComparer<T>.Default;
                var index = _head;
                for (var i = 0; i < _size; i++, index = (index + 1) % Capacity)
                {
                    if (comparer.Equals(item, _buffer[index])) return true;
                }
                return false;
            }

            /// <summary>
            /// Removes all items from the RingBuffer.
            /// </summary>
            public void Clear()
            {
                for (var i = 0; i < Capacity; i++)
                {
                    _buffer[i] = default;
                }
                _head = 0;
                _tail = 0;
                _size = 0;
            }

            /// <summary>
            /// Copies the contents of the RingBuffer to <paramref name="array"/>
            /// starting at <paramref name="arrayIndex"/>.
            /// </summary>
            /// <param name="array">The array to be copied to.</param>
            /// <param name="arrayIndex">The index of <paramref name="array"/>
            /// where the buffer should begin copying to.</param>
            public void CopyTo(T[] array, int arrayIndex)
            {
                var index = _head;
                for (var i = 0; i < _size; i++, arrayIndex++, index = (index + 1) %
                                                                      Capacity)
                {
                    array[arrayIndex] = _buffer[index];
                }
            }

            /// <summary>
            /// Removes <paramref name="item"/> from the buffer.
            /// </summary>
            /// <param name="item"></param>
            /// <returns>True if <paramref name="item"/> was found and 
            /// successfully removed. False if <paramref name="item"/> was not
            /// found or there was a problem removing it from the RingBuffer.
            /// </returns>
            public bool Remove(T item)
            {
                var index = _head;
                var removeIndex = 0;
                var foundItem = false;
                var comparer = EqualityComparer<T>.Default;
                for (var i = 0; i < _size; i++, index = (index + 1) % Capacity)
                {
                    if (comparer.Equals(item, _buffer[index]))
                    {
                        removeIndex = index;
                        foundItem = true;
                        break;
                    }
                }
                if (foundItem)
                {
                    var newBuffer = new T[_size - 1];
                    index = _head;
                    var pastItem = false;
                    for (var i = 0; i < _size - 1; i++, index = (index + 1) % Capacity)
                    {
                        if (index == removeIndex)
                        {
                            pastItem = true;
                        }
                        if (pastItem)
                        {
                            newBuffer[index] = _buffer[(index + 1) % Capacity];
                        }
                        else
                        {
                            newBuffer[index] = _buffer[index];
                        }
                    }
                    _size--;
                    _buffer = newBuffer;
                    return true;
                }
                return false;
            }
            #endregion

            #region ICollection Members
            /// <summary>
            /// Gets an object that can be used to synchronize access to the
            /// RingBuffer.
            /// </summary>
            public object SyncRoot => this;

            /// <summary>
            /// Gets a value indicating whether access to the RingBuffer is 
            /// synchronized (thread safe).
            /// </summary>
            public bool IsSynchronized => false;

            /// <summary>
            /// Copies the elements of the RingBuffer to <paramref name="array"/>, 
            /// starting at a particular Array <paramref name="index"/>.
            /// </summary>
            /// <param name="array">The one-dimensional Array that is the 
            /// destination of the elements copied from RingBuffer. The Array must 
            /// have zero-based indexing.</param>
            /// <param name="index">The zero-based index in 
            /// <paramref name="array"/> at which copying begins.</param>
            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((T[])array, index);
            }
            #endregion
        }
    }
}
