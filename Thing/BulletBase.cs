using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OberoniaAurea_Frame;

/// <summary>
/// 弹药基类。
/// </summary>
public class BulletBase : Bullet
{
    /// <inheritdoc/>
    public override bool AnimalsFleeImpact => true;

    /// <summary>
    /// 本次射击共享的战斗日志记录条。
    /// </summary>
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

    /// <inheritdoc/>
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

        if (Rand.Chance(DamageDef.igniteCellChance))
        {
            FireUtility.TryStartFireIn(position, map, Rand.Range(0.55f, 0.85f), launcher);
        }
    }

    /// <summary>
    /// 处理弹丸命中指定格子的效果，供子类重写。
    /// </summary>
    /// <param name="map">命中的地图。</param>
    /// <param name="hitCell">命中的格子。</param>
    protected virtual void ImpactCell(Map map, IntVec3 hitCell) { }

    /// <summary>
    /// 处理弹丸命中物体时的通用效果。
    /// </summary>
    /// <param name="map">命中的地图。</param>
    /// <param name="hitThing">被命中的物体。</param>
    protected void ImpactThingCommon(Map map, Thing hitThing)
    {
        Quaternion exactRotation = ExactRotation;
        bool instigatorGuilty = launcher is not Pawn pawn || !pawn.Drafted;

        ImpactThing(map, hitThing, exactRotation, instigatorGuilty);

        if (hitThing is Pawn hitPawn)
        {
            ImpactPawn(map, hitPawn);
        }

        if (ExtraDamages is not null)
        {
            foreach (ExtraDamage extraDamage in ExtraDamages)
            {
                if (Rand.Chance(extraDamage.chance))
                {
                    DamageDef extraDamageDef = extraDamage.def;
                    float extraDamageAmount = extraDamage.amount;
                    float extraDamageArmorPenetration = extraDamage.AdjustedArmorPenetration();
                    DamageInfo dinfo2 = new(extraDamageDef, extraDamageAmount, extraDamageArmorPenetration, exactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                    hitThing.TakeDamage(dinfo2).AssociateWithLog(sharedBattleLogEntry);
                }
            }
        }
    }

    /// <summary>
    /// 处理弹丸命中物体时的伤害效果，供子类重写。
    /// </summary>
    /// <param name="map">命中的地图。</param>
    /// <param name="hitThing">被命中的物体。</param>
    /// <param name="exactRotation">弹丸的精确旋转。</param>
    /// <param name="instigatorGuilty">发射者是否负有责任。</param>
    protected virtual void ImpactThing(Map map, Thing hitThing, Quaternion exactRotation, bool instigatorGuilty)
    {
        DamageInfo dinfo = new(DamageDef, DamageAmount, ArmorPenetration, exactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
        dinfo.SetWeaponQuality(equipmentQuality);
        hitThing.TakeDamage(dinfo).AssociateWithLog(sharedBattleLogEntry);
    }

    /// <summary>
    /// 处理弹丸命中<see cref="Pawn"/>时的效果，供子类重写。
    /// </summary>
    /// <param name="map">命中的地图。</param>
    /// <param name="hitPawn">被命中的<see cref="Pawn"/>。</param>
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
