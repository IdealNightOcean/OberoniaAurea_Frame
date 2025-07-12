using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(WorldPawnGC), "GetCriticalPawnReason")]
public static class GetCriticalPawnReason_Patch
{
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
