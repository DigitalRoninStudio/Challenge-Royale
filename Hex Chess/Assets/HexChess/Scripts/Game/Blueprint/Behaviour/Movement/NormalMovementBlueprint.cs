using UnityEngine;

[CreateAssetMenu(fileName = "NormalMovement", menuName = "BehaviourData/Movement/NormalMovement")]
public class NormalMovementBlueprint : MovementBehaviourBlueprint
{
    public override Behaviour CreateBehaviour() => new NormalMovementBehaviour(this);
}

public abstract class MovementBehaviourBlueprint : BehaviourBlueprint
{
    public float Speed;
    public int Range;

    [Header("VFX")]
    public GameObject Trail;
}
