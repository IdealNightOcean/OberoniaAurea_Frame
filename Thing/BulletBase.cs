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

    private void ProjectileImpact(Map map, IntVec3 position, bool blockedByShield = false)
    {
        GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
        if (!blockedByShield && def.projectile.landedEffecter is not null)
        {
            def.projectile.landedEffecter.Spawn(position, map).Cleanup();
        }
        Destroy();
    }

    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        Map map = Map;
        IntVec3 position = Position;
        Vector3 exactPosition = ExactPosition;

        ProjectileImpact(map, position, blockedByShield);
        BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new(launcher, hitThing, intendedTarget.Thing, equipmentDef, def, targetCoverDef);
        sharedBattleLogEntry = battleLogEntry_RangedImpact;
        Find.BattleLog.Add(battleLogEntry_RangedImpact);
        NotifyImpact(map, hitThing, position);
        ImpactCell(map, position);

        if (hitThing is not null)
        {
            ImpactThingCommon(map, hitThing);
            sharedBattleLogEntry = null;
            return;
        }

        sharedBattleLogEntry = null;
        if (!blockedByShield)
        {
            SoundDefOf.BulletImpact_Ground.PlayOneShot(new TargetInfo(position, map));
            if (position.GetTerrain(map).takeSplashes)
            {
                FleckMaker.WaterSplash(exactPosition, map, Mathf.Sqrt(DamageAmount) * 1f, 4f);
            }
            else
            {
                FleckMaker.Static(exactPosition, map, FleckDefOf.ShotHit_Dirt);
            }
        }

        if (Rand.Chance(def.projectile.bulletChanceToStartFire))
        {
            FireUtility.TryStartFireIn(position, map, def.projectile.bulletFireSizeRange.RandomInRange, launcher);
        }
    }

    protected virtual void ImpactCell(Map map, IntVec3 hitCell) { }
    protected void ImpactThingCommon(Map map, Thing hitThing)
    {
        Quaternion exactRotation = ExactRotation;
        bool instigatorGuilty = launcher is not Pawn pawn || !pawn.Drafted;

        ImpactThing(map, hitThing, exactRotation, instigatorGuilty);

        Pawn hitPawn = hitThing as Pawn;
        if (hitPawn is not null)
        {
            ImpactPawn(map, hitPawn);
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

    protected virtual void ImpactThing(Map map, Thing hitThing, Quaternion exactRotation, bool instigatorGuilty)
    {
        DamageInfo dinfo = new(def.projectile.damageDef, DamageAmount, ArmorPenetration, exactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
        dinfo.SetWeaponQuality(equipmentQuality);
        hitThing.TakeDamage(dinfo).AssociateWithLog(sharedBattleLogEntry);
    }

    protected virtual void ImpactPawn(Map map, Pawn hitPawn)
    {
        hitPawn.stances?.stagger.Notify_BulletImpact(this);
    }

    private void NotifyImpact(Map map, Thing hitThing, IntVec3 position)
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
