using UnityEngine;

[CreateAssetMenu(fileName = "DirectionMovement", menuName = "BehaviourData/Movement/DirectionMovement")]
public class DirectionMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour() => new DirectionMovementBehaviour(this);
}
