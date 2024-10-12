
using TMPro;
using UnityEngine;

public class FigureVisual : Visual<Figure, FigureBlueprint>
{
    [SerializeField] private TextMeshPro Damage;
    [SerializeField] private TextMeshPro Health;
    public override void Initialize(Figure figure, FigureBlueprint blueprint)
    {
        base.Initialize(figure, blueprint);

        DamageableBehaviour damageableBehaviour = figure.GetBehaviour<DamageableBehaviour>();
        if (damageableBehaviour != null)
            Health.text = damageableBehaviour.CurrentHealth.ToString();
        else
            Health.transform.parent.gameObject.SetActive(false);

        AttackBehaviour attackBehaviour = figure.GetBehaviour<AttackBehaviour>();
        if (attackBehaviour != null)
            Damage.text = attackBehaviour.AttackDamage.ToString();
        else
            Damage.transform.parent.gameObject.SetActive(false);

    }
}
