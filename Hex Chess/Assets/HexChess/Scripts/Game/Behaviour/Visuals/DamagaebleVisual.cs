using TMPro;
using UnityEngine;

public class DamagaebleVisual : GenericBehaviourVisual<DamageableBehaviour, DamageableBlueprint>
{
    [SerializeField] private TextMeshPro Health;   
    protected override void InitializeVisual()
    {
        Health.text = behaviour.CurrentHealth.ToString();

        behaviour.OnDamageReceived += OnDamageReceiverd;
        behaviour.OnDeath += OnDeath;
    }
    private void OnDeath()
    {
        behaviour.OnDamageReceived -= OnDamageReceiverd;
        behaviour.OnDeath -= OnDeath;
    }

    private void OnDamageReceiverd(int currentHealth, int finalDamage)
    {
        Health.text = currentHealth.ToString();
    }
}

public class MovementVisual : GenericBehaviourVisual<MovementBehaviour, MovementBehaviourBlueprint>
{
    protected override void InitializeVisual()
    {
        //TO DO
    }
}
