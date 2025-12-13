using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public class SimpleUniqueList<T> : IList<T>, IExposable, IDisposable
{
    private LookMode innerListLookMode;
    private List<T> innerList;

    public int Count => innerList.Count;
    public bool IsReadOnly => false;

    public SimpleUniqueList()
    {
        innerListLookMode = LookMode.Deep;
        innerList = [];
    }
    public SimpleUniqueList(LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = [];
    }

    public SimpleUniqueList(int count, LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = new List<T>(count);
    }

    public SimpleUniqueList(IEnumerable<T> collection, LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = [];
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
            if (!innerList.Contains(value))
            {
                innerList[index] = value;
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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(T item) => innerList.IndexOf(item);
    public void Insert(int index, T item)
    {
        if (index < 0 || index > innerList.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        else if (!innerList.Contains(item))
        {
            innerList.Insert(index, item);
        }
    }
    public bool Contains(T item) => innerList.Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    public void Add(T item) => innerList.AddUnique(item);
    public void AddRange(IEnumerable<T> collection)
    {
        if (collection is null)
        {
            return;
        }
        HashSet<T> uniqueSet = [.. innerList];
        foreach (T item in collection)
        {
            if (uniqueSet.Add(item))
            {
                innerList.Add(item);
            }
        }
    }

    public bool Remove(T item) => innerList.Remove(item);
    public void RemoveAt(int index) => innerList.RemoveAt(index);
    public int RemoveAll(Predicate<T> match) => innerList.RemoveAll(match);
    public void Clear() => innerList.Clear();
    public void Dispose() => innerList = null;

    public void EnsureUnique()
    {
        HashSet<T> uniqueSet = [.. innerList];
        innerList.Clear();
        foreach (T item in uniqueSet)
        {
            innerList.Add(item);
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref innerListLookMode, nameof(innerListLookMode), defaultValue: LookMode.Deep);

        Scribe_Collections.Look(ref innerList, nameof(innerList), lookMode: innerListLookMode);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            innerList ??= [];
        }
    }
}