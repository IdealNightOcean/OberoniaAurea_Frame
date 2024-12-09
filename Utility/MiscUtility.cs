using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea_Frame;


[StaticConstructorOnStartup]
public static class OAFrame_MiscUtility
{
    //尝试立刻触发事件
    public static bool TryFireIncidentNow(IncidentDef incidentDef, IncidentParms parms)
    {
        if (incidentDef.Worker.CanFireNow(parms))
        {
            return incidentDef.Worker.TryExecute(parms);
        }
        return false;
    }
    //添加队列事件
    public static void AddNewQueuedIncident(IncidentDef incidentDef, int delayTicks, IncidentParms parms, int retryDurationTicks = 0)
    {
        if (parms == null)
        {
            Log.Error("Try add a new queued incident,but IncidentParms is NULL.");
            return;
        }
        Find.Storyteller.incidentQueue.Add(incidentDef, Find.TickManager.TicksGame + delayTicks, parms, retryDurationTicks);
    }
    //创建物品
    public static List<Thing> TryGenerateThing(ThingDef def, int count)
    {
        List<Thing> list = [];
        int stackLimit = def.stackLimit;
        int remaining = count;
        while (remaining > 0)
        {
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = Mathf.Min(remaining, stackLimit);
            list.Add(thing);
            remaining -= stackLimit;
        }
        return list;
    }

    public static List<List<Thing>> TryGengrateThingGroup(ThingDef def, int count)
    {
        List<List<Thing>> lists = [];
        int perPodCount = Mathf.Max(1, Mathf.FloorToInt(150 / def.GetStatValueAbstract(StatDefOf.Mass)));
        int remaining = count;
        while (remaining > 0)
        {
            lists.Add(TryGenerateThing(def, Mathf.Min(remaining, perPodCount)));
            remaining -= perPodCount;
        }
        return lists;
    }

    public static void DrawText(Vector2 worldPos, string text, Color textColor)
    {
        Vector3 position = new(worldPos.x, 0f, worldPos.y);
        Vector2 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
        vector.y = (float)UI.screenHeight - vector.y;
        Text.Font = GameFont.Tiny;
        float x = Text.CalcSize(text).x;
        GUI.DrawTexture(new Rect(vector.x - x / 2f - 4f, vector.y, x + 8f, 16f), TexUI.GrayTextBG);
        GUI.color = textColor;
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(new Rect(vector.x - x / 2f, vector.y - 2f, x, 999f), text);
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

}