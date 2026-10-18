using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea_Frame;

/// <summary>
/// 为世界重要角色理由添加保留持有者检查的补丁。
/// </summary>
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(WorldPawnGC), "GetCriticalPawnReason")]
public static class GetCriticalPawnReason_Patch
{
    /// <summary>
    /// 当原始结果为 <see langword="null"/> 时，检查<see cref="Pawn"/>是否为保留持有者并补充理由。
    /// </summary>
    /// <param name="__result">原始补丁结果。</param>
    /// <param name="pawn">需要检查的<see cref="Pawn"/>。</param>
    [HarmonyPostfix]
    public static void Postfix(ref string __result, Pawn pawn)
    {
        if (__result is null && !pawn.Discarded)
        {
            if (pawn.IsChildOfRetentionHolder())
            {
                __result = "OAFrame_ChildOfRetentionHolder";
                return;
            }
        }
    }
}
