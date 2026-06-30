using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 基于哈希集合的简单列表，保证元素唯一性。
/// </summary>
public class SimpleHashList<T> : IList<T>, IExposable
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
    /// <summary>
    /// 使用指定的保存模式初始化列表。
    /// </summary>
    public SimpleHashList(LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = [];
        innerHashSet = [];
    }

    /// <summary>
    /// 使用指定容量和保存模式初始化列表。
    /// </summary>
    public SimpleHashList(int count, LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = new List<T>(count);
        innerHashSet = new HashSet<T>(count);
    }

    /// <summary>
    /// 使用集合和保存模式初始化列表。
    /// </summary>
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

    /// <summary>
    /// 获取列表的枚举器。
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return innerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 获取指定元素的索引。
    /// </summary>
    public int IndexOf(T item) => innerList.IndexOf(item);

    /// <summary>
    /// 在指定索引处插入元素。
    /// </summary>
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

    /// <summary>
    /// 确定列表是否包含指定元素。
    /// </summary>
    public bool Contains(T item) => innerHashSet.Contains(item);

    /// <summary>
    /// 将列表元素复制到数组。
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    /// <summary>
    /// 添加元素到列表。
    /// </summary>
    public void Add(T item)
    {
        if (innerHashSet.Add(item))
        {
            innerList.Add(item);
        }
    }

    /// <summary>
    /// 批量添加元素。
    /// </summary>
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

    /// <summary>
    /// 从列表中移除指定元素。
    /// </summary>
    public bool Remove(T item)
    {
        if (innerHashSet.Remove(item))
        {
            innerList.Remove(item);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 移除指定索引处的元素。
    /// </summary>
    public void RemoveAt(int index)
    {
        T toRemove = innerList[index];
        innerList.RemoveAt(index);
        innerHashSet.Remove(toRemove);
    }

    /// <summary>
    /// 移除所有匹配指定谓词的元素。
    /// </summary>
    public int RemoveAll(Predicate<T> match)
    {
        innerHashSet.RemoveWhere(match);
        return innerList.RemoveAll(match);
    }

    /// <summary>
    /// 确保列表元素唯一性。
    /// </summary>
    public void EnsureUnique()
    {
        innerList.Clear();
        innerList.AddRange(innerHashSet);
    }

    /// <summary>
    /// 清空列表中的所有元素。
    /// </summary>
    public void Clear()
    {
        innerList.Clear();
        innerHashSet.Clear();
    }

    /// <summary>
    /// 暴露数据以进行存档保存和加载。
    /// </summary>
    public void ExposeData()
    {
        Scribe_Values.Look(ref innerListLookMode, nameof(innerListLookMode), defaultValue: LookMode.Deep);

        Scribe_Collections.Look(ref innerList, nameof(innerList), lookMode: innerListLookMode);

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