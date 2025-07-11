using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea_Frame;

public class BulletBase : Bullet
{
    public override bool AnimalsFleeImpact => true;

    protected BattleLogEntry_RangedImpact sharedBattleLogEntry;

    private void ProjectileImpact(Thing hitThing, bool blockedByShield = false)
    {
        GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
        if (!blockedByShield && def.projectile.landedEffecter is not null)
        {
            def.projectile.landedEffecter.Spawn(Position, Map).Cleanup();
        }
        Destroy();
    }

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        Map map = Map;
        IntVec3 position = Position;
        ProjectileImpact(hitThing, blockedByShield);
        BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
        sharedBattleLogEntry = battleLogEntry_RangedImpact;
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        NotifyImpact(hitThing, map, position);
        ImpactCell(position);

        if (hitThing is not null)
        {
            ImpactThingCommon(hitThing);
            sharedBattleLogEntry = null;
            return;
        }

        sharedBattleLogEntry = null;
        if (!blockedByShield)
        {
            SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(Position, map));
            if (Position.GetTerrain(map).takeSplashes)
            {
                FleckMaker.WaterSplash(ExactPosition, map, Mathf.Sqrt(DamageAmount) * 1f, 4f);
            }
            else
            {
                FleckMaker.Static(ExactPosition, map, FleckDefOf.ShotHit_Dirt);
            }
        }

        if (Rand.Chance(def.projectile.bulletChanceToStartFire))
        {
            FireUtility.TryStartFireIn(Position, map, def.projectile.bulletFireSizeRange.RandomInRange, launcher);
        }
    }

    protected virtual void ImpactCell(IntVec3 hitCell) { }
    protected void ImpactThingCommon(Thing hitThing)
    {
        Quaternion exactRotation = ExactRotation;
        bool instigatorGuilty = launcher is not Pawn pawn || !pawn.Drafted;

        ImpactThing(hitThing, exactRotation, instigatorGuilty);

        Pawn hitPawn = hitThing as Pawn;
        if (hitPawn is not null)
        {
            ImpactPawn(hitPawn);
        }

        if (def.projectile.extraDamages is not null)
        {
            foreach (ExtraDamage extraDamage in def.projectile.extraDamages)
            {
                if (Rand.Chance(extraDamage.chance))
                {
                    DamageInfo dinfo2 = new(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), exactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(sharedBattleLogEntry);
                }
            }
        }
        if (Rand.Chance(def.projectile.bulletChanceToStartFire) && (hitPawn is null || Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(hitPawn))))
        {
            hitThing.TryAttachFire(def.projectile.bulletFireSizeRange.RandomInRange, launcher);
        }
    }

    protected virtual void ImpactThing(Thing hitThing, Quaternion exactRotation, bool instigatorGuilty)
    {
        DamageInfo dinfo = new(def.projectile.damageDef, DamageAmount, ArmorPenetration, exactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
        dinfo.SetWeaponQuality(equipmentQuality);
        hitThing.TakeDamage(dinfo).AssociateWithLog(sharedBattleLogEntry);
    }

    protected virtual void ImpactPawn(Pawn hitPawn)
    {
        hitPawn.stances?.stagger.Notify_BulletImpact(this);
    }

    private void NotifyImpact(Thing hitThing, Map map, IntVec3 position)
    {
        BulletImpactData impactData = new()
        {
            bullet = this,
            hitThing = hitThing,
            impactPosition = position
        };
        hitThing?.Notify_BulletImpactNearby(impactData);
        int num = 9;
        for (int i = 0; i < num; i++)
        {
            IntVec3 c = position + GenRadial.RadialPattern[i];
            if (!c.InBounds(map))
            {
                continue;
            }
            List<Thing> thingList = c.GetThingList(map);
            for (int j = 0; j < thingList.Count; j++)
            {
                if (thingList[j] != hitThing)
                {
                    thingList[j].Notify_BulletImpactNearby(impactData);
                }
            }
        }
    }
}
