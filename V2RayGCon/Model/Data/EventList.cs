﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace V2RayGCon.Model.Data
{
    public class EventList<T> : IList<T>
    {
        public delegate void ListChangedEventDelegate();
        public event ListChangedEventDelegate ListChanged;

        List<T> list;

        public EventList(List<T> fromList)
        {
            if (fromList != null)
            {
                list = fromList;
            }
            else
            {
                list = new List<T>();
            }


            // eventhandler not attach yet, so notify() is in vain.
            // notify();
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return list.AsReadOnly();
        }

        public EventList()
        {
            list = new List<T>();
        }

        private void notify()
        {
            ListChanged?.Invoke();
            // Debug.WriteLine("EventList changed!");
        }


        public T this[int index]
        {
            get => ((IList<T>)list)[index];
            set
            {
                ((IList<T>)list)[index] = value;
                notify();
            }
        }

        public int Count => ((IList<T>)list).Count;

        public bool IsReadOnly => ((IList<T>)list).IsReadOnly;

        public void Add(T item)
        {
            ((IList<T>)list).Add(item);
            notify();
        }

        public void Clear()
        {
            ((IList<T>)list).Clear();
            notify();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)list).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>)list).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)list).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return ((IList<T>)list).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)list).Insert(index, item);
            notify();
        }

        public bool Remove(T item)
        {
            var l = ((IList<T>)list).Remove(item);
            notify();
            return l;
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)list).RemoveAt(index);
            notify();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)list).GetEnumerator();
        }
    }
}
