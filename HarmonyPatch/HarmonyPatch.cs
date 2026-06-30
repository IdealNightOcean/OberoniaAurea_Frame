using HarmonyLib;
using JetBrains.Annotations;
using System.Reflection;
using Verse;

namespace OberoniaAurea_Frame;


/// <summary>
/// Mod 的 Harmony 补丁入口。
/// </summary>
[UsedImplicitly]
[StaticConstructorOnStartup]
public static class ModHarmonyPatch
{
    private static Harmony harmonyInstance;

    internal static Harmony HarmonyInstance
    {
        get
        {
            harmonyInstance ??= new Harmony("OberoniaAureaFrame_Hramony");
            return harmonyInstance;
        }
    }

    static ModHarmonyPatch()
    {
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
    }
}
