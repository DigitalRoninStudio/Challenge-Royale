using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttack", menuName = "BehaviourData/Attack/RangedAttack")]
public class RangedAttackBlueprint : AttackBehaviourBlueprint
{
    public override Behaviour CreateBehaviour() => new RangedAttackBehaviour(this);
}
