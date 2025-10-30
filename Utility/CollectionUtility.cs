using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_CollectionUtility
{
    public static void BinaryInsertion<T>(this IList<T> iList, T addend) where T : IComparable<T>
    {
        int left = 0;
        int right = iList.Count;
        int mid;
        while (left < right)
        {
            mid = (left + right) >> 1;
            if (iList[mid].CompareTo(addend) < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }
        iList.Insert(left, addend);
    }
    public static void BinaryInsertion<T>(this IList<T> iList, T addend, Func<T, T, int> comparerFunc)
    {
        int left = 0;
        int right = iList.Count;
        int mid;
        while (left < right)
        {
            mid = (left + right) >> 1;
            if (comparerFunc.Invoke(iList[mid], addend) < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }
        iList.Insert(left, addend);
    }
    public static void BinaryInsertion<T>(this IList<T> iList, T addend, IComparer<T> comparer)
    {
        int left = 0;
        int right = iList.Count;
        int mid;
        while (left < right)
        {
            mid = (left + right) >> 1;
            if (comparer.Compare(iList[mid], addend) < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid;
            }
        }
        iList.Insert(left, addend);
    }

    /// <summary>
    /// 原地合并两个有序列表；
    /// 浅复制；
    /// 会填充default，注意非引用变量的default构造消耗；
    /// </summary>
    /// <param name="augend">被并入列表（会被修改）</param>
    /// <param name="addend">并入列表（不会被修改）</param>
    public static void MergeSortedListsInplace<T>(List<T> augend, List<T> addend) where T : IComparable<T>
    {
        if (augend is null)
        {
            throw new ArgumentNullException(nameof(augend));
        }
        if (addend is null || addend.Count == 0)
        {
            return;
        }

        int i = augend.Count - 1;
        int j = addend.Count - 1;
        int totalLength = augend.Count + addend.Count;
        int k = totalLength - 1;

        if (augend.Capacity < totalLength)
        {
            augend.Capacity = totalLength;
        }
        for (int tmp = augend.Count; tmp < totalLength; tmp++)
        {
            augend.Add(default);
        }

        while (i >= 0 && j >= 0)
        {
            if (augend[i].CompareTo(addend[j]) > 0)
            {
                augend[k] = augend[i];
                i--;
            }
            else
            {
                augend[k] = addend[j];
                j--;
            }
            k--;
        }

        while (j >= 0)
        {
            augend[k] = addend[j];
            j--;
            k--;
        }
    }
    public static void MergeSortedListsInplace<T>(List<T> augend, List<T> addend, Func<T, T, int> comparerFunc)
    {
        if (augend is null)
        {
            throw new ArgumentNullException(nameof(augend));
        }
        if (addend is null || addend.Count == 0)
        {
            return;
        }

        int i = augend.Count - 1;
        int j = addend.Count - 1;
        int totalLength = augend.Count + addend.Count;
        int k = totalLength - 1;

        if (augend.Capacity < totalLength)
        {
            augend.Capacity = totalLength;
        }
        for (int tmp = augend.Count; tmp < totalLength; tmp++)
        {
            augend.Add(default);
        }

        while (i >= 0 && j >= 0)
        {
            if (comparerFunc(augend[i], addend[j]) > 0)
            {
                augend[k] = augend[i];
                i--;
            }
            else
            {
                augend[k] = addend[j];
                j--;
            }
            k--;
        }

        while (j >= 0)
        {
            augend[k] = addend[j];
            j--;
            k--;
        }
    }
    public static void MergeSortedListsInplace<T>(List<T> augend, List<T> addend, IComparer<T> comparer)
    {
        if (augend is null)
        {
            throw new ArgumentNullException(nameof(augend));
        }
        if (addend is null || addend.Count == 0)
        {
            return;
        }

        int i = augend.Count - 1;
        int j = addend.Count - 1;
        int totalLength = augend.Count + addend.Count;
        int k = totalLength - 1;

        if (augend.Capacity < totalLength)
        {
            augend.Capacity = totalLength;
        }
        for (int tmp = augend.Count; tmp < totalLength; tmp++)
        {
            augend.Add(default);
        }

        while (i >= 0 && j >= 0)
        {
            if (comparer.Compare(augend[i], addend[j]) > 0)
            {
                augend[k] = augend[i];
                i--;
            }
            else
            {
                augend[k] = addend[j];
                j--;
            }
            k--;
        }

        while (j >= 0)
        {
            augend[k] = addend[j];
            j--;
            k--;
        }
    }

    /// <summary>
    /// 从序列中无放回地随机选取最多 <paramref name="count"/> 个元素。
    /// 返回顺序为随机顺序，不保留源序列中的原始顺序。
    /// 本方法不会对元素值进行去重；若源序列包含重复值，结果中可能包含重复。
    /// 若 <paramref name="count"/> 大于序列长度，则返回全部元素（顺序随机）。
    /// </summary>
    public static IEnumerable<T> TakeRandomElements<T>(this IEnumerable<T> iEnum, int count)
    {
        if (count <= 0 || iEnum is null)
        {
            yield break;
        }
        if (iEnum is not IList<T> iList)
        {
            iList = iEnum.ToList();
        }

        int listCount = iList.Count;
        if (count == 1)
        {
            yield return iList[Rand.Range(0, listCount)];
        }
        else
        {
            int[] indices = Enumerable.Range(0, listCount).ToArray();
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Rand.Range(i, listCount);
                (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
                yield return iList[indices[i]];
            }
        }
    }
}