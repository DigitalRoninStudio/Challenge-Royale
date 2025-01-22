
using System;
using TMPro;
using UnityEngine;

public class FigureVisual : Visual<Figure, FigureBlueprint>
{
    /* [SerializeField] private TextMeshPro Damage;
     [SerializeField] private TextMeshPro Health;*/
    // private DamageableBehaviour damageableBehaviour;
    public Animator animator;
    public override void Initialize(Figure figure, FigureBlueprint blueprint)
    {
        base.Initialize(figure, blueprint);

        //visual.GetComponent<Renderer>().material.color = blueprint.Color;
            /* damageableBehaviour = figure.GetBehaviour<DamageableBehaviour>();
             if (damageableBehaviour != null)
             {
                 Health.text = damageableBehaviour.CurrentHealth.ToString();
                 damageableBehaviour.OnDamageReceived += OnDamageReceiverd;
                 damageableBehaviour.OnDeath += OnDeath;
             }
             else
                 Health.transform.parent.gameObject.SetActive(false);

             AttackBehaviour attackBehaviour = figure.GetBehaviour<AttackBehaviour>();
             if (attackBehaviour != null)
                 Damage.text = attackBehaviour.AttackDamage.ToString();
             else
                 Damage.transform.parent.gameObject.SetActive(false);*/

    }

  /*  private void OnDeath()
    {
        damageableBehaviour.OnDamageReceived -= OnDamageReceiverd;
        damageableBehaviour.OnDeath -= OnDeath;
    }

    private void OnDamageReceiverd(int currentHealth, int finalDamage)
    {
        Health.text = currentHealth.ToString();
    }*/


}
