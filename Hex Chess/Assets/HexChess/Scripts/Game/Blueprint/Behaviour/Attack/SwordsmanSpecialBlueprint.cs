using UnityEngine;

[CreateAssetMenu(fileName = "SwordsmanSpecial", menuName = "BehaviourData/Ability/Active/Swordsman/SwordsmanSpecial")]
public class SwordsmanSpecialBlueprint : ActiveAbilityBlueprint
{
    public DamageModifierBlueprint DamageBlueprint;
    public HealthModifierBlueprint HealthBlueprint;
    public DodgeCastAttackBlueprint DodgeCastAttackBlueprint;
    public override Behaviour CreateBehaviour(Entity owner)
    {
        return new SwordsmanSpecial.Builder(owner)
           .WithBlueprint(this)
           .WithGeneratedId()
           .WithSubscription()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator, Entity owner)
    {
        return new SwordsmanSpecial.Builder(owner)
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithSubscription()
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData, Entity owner)
    {
        return new SwordsmanSpecial.Builder(owner)
            .WithBlueprint(this)
            .WithData(behaviourData as SwordsmanSpecialData)
            .WithSubscription()
            .Build();
    }
}
