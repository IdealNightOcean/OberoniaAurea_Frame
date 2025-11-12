using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea_Frame;

public static class OAFrame_CollectionUtility
{
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
        int takeCount = count > listCount ? listCount : count;
        if (takeCount == 1)
        {
            yield return iList[Rand.Range(0, listCount)];
        }
        else
        {
            int[] indices = Enumerable.Range(0, listCount).ToArray();
            for (int i = 0; i < takeCount; i++)
            {
                int randomIndex = Rand.Range(i, listCount);
                (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
                yield return iList[indices[i]];
            }
        }
    }
}