using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class SimpleHashList<T> : IList<T>, IExposable, IDisposable
{
    private LookMode innerListLookMode;
    private List<T> innerList;
    private HashSet<T> innerHashSet;

    public int Count => innerList.Count;
    public bool IsReadOnly => false;

    public SimpleHashList()
    {
        innerListLookMode = LookMode.Deep;
        innerList = [];
        innerHashSet = [];
    }
    public SimpleHashList(LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = [];
        innerHashSet = [];
    }

    public SimpleHashList(int count, LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = new List<T>(count);
        innerHashSet = new HashSet<T>(count);
    }

    public SimpleHashList(IEnumerable<T> collection, LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = [];
        innerHashSet = [];
        AddRange(collection);
    }

    public T this[int index]
    {
        get
        {
            return innerList[index];
        }
        set
        {
            T oldValue = innerList[index];
            if (innerHashSet.Add(value))
            {
                innerList[index] = value;
                innerHashSet.Remove(oldValue);
            }
            else
            {
                throw new InvalidOperationException("The value already exists and cannot be replaced.");
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return innerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(T item) => innerList.IndexOf(item);
    public void Insert(int index, T item)
    {
        if (index < 0 || index > innerList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        else if (innerHashSet.Add(item))
        {
            innerList.Insert(index, item);
        }
    }
    public bool Contains(T item) => innerHashSet.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    public void Add(T item)
    {
        if (innerHashSet.Add(item))
        {
            innerList.Add(item);
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        if (collection is null)
        {
            return;
        }

        foreach (T item in collection)
        {
            if (innerHashSet.Add(item))
            {
                innerList.Add(item);
            }
        }
    }

    public bool Remove(T item)
    {
        if (innerHashSet.Remove(item))
        {
            innerList.Remove(item);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        T toRemove = innerList[index];
        innerList.RemoveAt(index);
        innerHashSet.Remove(toRemove);
    }

    public int RemoveAll(Predicate<T> match)
    {
        innerHashSet.RemoveWhere(match);
        return innerList.RemoveAll(match);
    }

    public void EnsureUnique()
    {
        innerList.Clear();
        foreach (T item in innerHashSet)
        {
            innerList.Add(item);
        }
    }

    public void Clear()
    {
        innerList.Clear();
        innerHashSet.Clear();
    }

    public void Dispose()
    {
        innerList = null;
        innerHashSet = null;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref innerListLookMode, "innerListLookMode", defaultValue: LookMode.Deep);

        Scribe_Collections.Look(ref innerList, "innerList", lookMode: innerListLookMode);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (innerList is null)
            {
                innerList = [];
                innerHashSet = [];
            }
            else
            {
                innerHashSet = [.. innerList];
            }
        }
    }
}
