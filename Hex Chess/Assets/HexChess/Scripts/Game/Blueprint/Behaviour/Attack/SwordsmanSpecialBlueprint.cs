using UnityEngine;

[CreateAssetMenu(fileName = "SwordsmanSpecial", menuName = "BehaviourData/Ability/Active/Swordsman/SwordsmanSpecial")]
public class SwordsmanSpecialBlueprint : ActiveAbilityBlueprint
{
    public int HealthModifierValue;
    public override Behaviour CreateBehaviour()
    {
        return new SwordsmanSpecial.Builder()
           .WithBlueprint(this)
           .WithGeneratedId()
           .Build();
    }

    public override Behaviour CreateBehaviour(RandomGenerator randomGenerator)
    {
        return new SwordsmanSpecial.Builder()
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .Build();
    }

    public override Behaviour CreateBehaviour(BehaviourData behaviourData)
    {
        return new SwordsmanSpecial.Builder()
            .WithBlueprint(this)
            .WithData(behaviourData as SwordsmanSpecialData)
            .Build();
    }
}
