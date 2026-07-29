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

    /// <summary> 获取列表元素数量。 </summary>
    public int Count => innerList.Count;
    /// <summary> 获取列表是否为只读。 </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// 初始化空列表。
    /// </summary>
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

    /// <summary>
    /// 获取或设置指定索引处的元素。
    /// </summary>
    /// <param name="index">索引</param>
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

    /// <inheritdoc />
    public int IndexOf(T item) => innerList.IndexOf(item);

    /// <summary>
    /// 在指定索引处插入元素。
    /// </summary>
    /// <param name="index">插入位置</param>
    /// <param name="item">要插入的元素</param>
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
    /// <param name="item">要查找的元素</param>
    /// <returns>是否包含该元素</returns>
    public bool Contains(T item) => innerList.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

    /// <summary>
    /// 添加元素到列表。
    /// </summary>
    /// <param name="item">要添加的元素</param>
    public void Add(T item) => innerList.AddUnique(item);

    /// <summary>
    /// 批量添加元素。
    /// </summary>
    /// <param name="collection">要添加的元素集合</param>
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
    /// <param name="item">要移除的元素</param>
    /// <returns>是否移除成功</returns>
    public bool Remove(T item) => innerList.Remove(item);
    /// <summary>
    /// 移除指定索引处的元素。
    /// </summary>
    /// <param name="index">要移除的索引</param>
    public void RemoveAt(int index) => innerList.RemoveAt(index);
    /// <summary>
    /// 移除所有匹配指定谓词的元素。
    /// </summary>
    /// <param name="match">用于判断元素是否要移除的谓词</param>
    /// <returns>被移除的元素数量</returns>
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
    /// 序列化/反序列化此对象需持久保存的字段。
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