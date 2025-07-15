using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(MapParent), "GetShuttleFloatMenuOptions")]
public class MapParent_GetShuttleFloatMenuOptions_Patch
{
    [HarmonyPostfix]
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> origin, PlanetTile ___tile, IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
    {
        foreach (FloatMenuOption menuOption in origin)
        {
            yield return menuOption;
        }

        if (TransportersArrivalAction_FormCaravan.CanFormCaravanAt(pods, ___tile))
        {
            yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
            {
                launchAction(___tile, new TransportersArrivalAction_FormCaravan("MessageShuttleArrived"));
            });
        }
    }
}
