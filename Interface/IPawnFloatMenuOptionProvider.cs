using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Frame;

public interface IPawnFloatMenuOptionProvider
{
    bool IsSelectedPawnValid(Pawn selPawn);
    FloatMenuOption GetFloatMenuOption(Pawn selPawn);
}

public class CompProperties_PawnFloatMenuOptionHandler : CompProperties
{
    public CompProperties_PawnFloatMenuOptionHandler()
    {
        compClass = typeof(CompPawnFloatMenuOptionHandler);
    }
}

public class CompPawnFloatMenuOptionHandler : ThingComp
{
    [Unsaved] private List<IPawnFloatMenuOptionProvider> providers;

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        if (providers is null)
        {
            yield break;
        }

        List<FloatMenuOption> options = [];
        foreach (IPawnFloatMenuOptionProvider provider in providers)
        {
            try
            {
                if (provider.IsSelectedPawnValid(selPawn))
                {
                    options.Add(provider.GetFloatMenuOption(selPawn));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error in FloatMenuWorker {provider.GetType().Name}: {ex}");
            }
        }

        for (int i = 0; i < options.Count; i++)
        {
            yield return options[i];
        }
    }


    public void RegisterProvider(IPawnFloatMenuOptionProvider provider)
    {
        if (providers is null)
        {
            providers = [provider];
        }
        else
        {
            providers.AddUnique(provider);
        }
    }

    public void DeregisterProvider(IPawnFloatMenuOptionProvider provider)
    {
        if (providers is not null && providers.Remove(provider))
        {
            if (providers.Count == 0)
            {
                providers = null;
            }
        }
    }
}
