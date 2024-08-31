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
        if (__result == null && !pawn.Discarded)
        {
            if (pawn.IsFixedCaravanMember())
            {
                __result = "OAFrame_FixedCaravanMember";
                return;
            }
            if(pawn.IsSiteTraderGood())
            {
                __result = "OAFrame_SiteTraderGood";
                return;
            }
        }
    }
}
