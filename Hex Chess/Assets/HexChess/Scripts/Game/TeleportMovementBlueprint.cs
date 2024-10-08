using UnityEngine;

[CreateAssetMenu(fileName = "TeleportMovement", menuName = "BehaviourData/Movement/TeleportMovement")]
public class TeleportMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour() => new TeleportMovementBehaviour(this);
}