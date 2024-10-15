using UnityEngine;

[CreateAssetMenu(fileName = "Damageable", menuName = "BehaviourData/Damageable/Damageable")]
public class DamageableBlueprint : BehaviourBlueprint
{
    public int MaxHealth;
    public override Behaviour CreateBehaviour() => new DamageableBehaviour(this);
}
