using System;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 简单的唯一元素列表。
/// </summary>
public class SimpleUniqueList<T> : IList<T>, IExposable
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

    /// <summary>
    /// 使用指定的保存模式初始化列表。
    /// </summary>
    public SimpleUniqueList(LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = [];
    }

    /// <summary>
    /// 使用指定容量和保存模式初始化列表。
    /// </summary>
    public SimpleUniqueList(int count, LookMode innerListLookMode)
    {
        this.innerListLookMode = innerListLookMode;
        innerList = new List<T>(count);
    }

    /// <summary>
    /// 使用集合和保存模式初始化列表。
    /// </summary>
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

    /// <summary>
    /// 获取列表的枚举器。
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return innerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
        else if (!innerList.Contains(item))
        {
            innerList.Insert(index, item);
        }
    }

    /// <summary>
    /// 确定列表是否包含指定元素。
    /// </summary>
    public bool Contains(T item) => innerList.Contains(item);

    /// <summary>
    /// 将列表元素复制到数组。
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    /// <summary>
    /// 添加元素到列表。
    /// </summary>
    public void Add(T item) => innerList.AddUnique(item);

    /// <summary>
    /// 批量添加元素。
    /// </summary>
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

    /// <summary>
    /// 从列表中移除指定元素。
    /// </summary>
    public bool Remove(T item) => innerList.Remove(item);
    /// <summary>
    /// 移除指定索引处的元素。
    /// </summary>
    public void RemoveAt(int index) => innerList.RemoveAt(index);
    /// <summary>
    /// 移除所有匹配指定谓词的元素。
    /// </summary>
    public int RemoveAll(Predicate<T> match) => innerList.RemoveAll(match);
    /// <summary>
    /// 清空列表中的所有元素。
    /// </summary>
    public void Clear() => innerList.Clear();

    /// <summary>
    /// 确保列表元素唯一性。
    /// </summary>
    public void EnsureUnique()
    {
        HashSet<T> uniqueSet = [.. innerList];
        innerList.Clear();
        foreach (T item in uniqueSet)
        {
            innerList.Add(item);
        }
    }

    /// <summary>
    /// 序列化/反序列化此对象的所有数据字段。
    /// </summary>
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