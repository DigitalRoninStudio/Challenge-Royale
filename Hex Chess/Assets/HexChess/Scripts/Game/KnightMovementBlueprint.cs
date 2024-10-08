using UnityEngine;

[CreateAssetMenu(fileName = "KnightMovement", menuName = "BehaviourData/Movement/KnightMovement")]
public class KnightMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour() => new KnightMovementBehaviour(this);
}
