﻿using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DodgeCastAttack", menuName = "StatusEffectBlueprint/DodgeCastAttack")]
public class DodgeCastAttackBlueprint : StatusEffectBlueprint
{
    [Range(1, 12)]
    public int DodgeChance;
    public override StatusEffect CreateStatusEffect(Behaviour owner, Entity target)
    {
        return new DodgeCastAttack.Builder(owner, target)
             .WithBlueprint(this)
             .WithGeneratedId()
             .Build();
    }

    public override StatusEffect CreateStatusEffect(RandomGenerator randomGenerator, Behaviour owner, Entity target)
    {
        return new DodgeCastAttack.Builder(owner, target)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override StatusEffect CreateStatusEffect(StatusEffectData statusEffectData, Behaviour owner, Entity target)
    {
        return new DodgeCastAttack.Builder(owner, target)
            .WithBlueprint(this)
            .WithData(statusEffectData as DodgeCastAttackData)
            .Build();
    }
}
