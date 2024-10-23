using TMPro;
using UnityEngine;

public class AttackableVisual : GenericBehaviourVisual<AttackBehaviour, AttackBehaviourBlueprint>
{
    [SerializeField] private TextMeshPro Damage;
    private GameObject HitImpact;

    protected override void InitializeVisual()
    {
        Damage.text = behaviour.AttackDamage.ToString();
        HitImpact = blueprint.HitImpact;

        behaviour.OnAttackPerformed += OnAttackPerformed;
    }
    private void OnAttackPerformed(DamageableBehaviour damageable, Damage damage)
    {
        if (HitImpact == null) return;

        Vector3 direction = (damageable.Owner.gameObject.transform.position - behaviour.Owner.gameObject.transform.position).normalized;
        GameObject.Destroy(GameObject.Instantiate(HitImpact, damageable.Owner.gameObject.transform.position, /*Quaternion.LookRotation(direction)*/ Quaternion.identity), 3f);
    }
}
