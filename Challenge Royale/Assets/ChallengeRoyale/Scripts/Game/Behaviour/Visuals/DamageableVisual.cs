using System;
using TMPro;
using UnityEngine;

public class DamageableVisual : GenericBehaviourVisual<DamageableBehaviour>
{  
    protected override void InitializeVisual()
    {
        Animator animator = ((Figure)behaviour.Owner).GameObject.GetComponent<FigureVisual>().animator;

        if (animator == null) return;
        behaviour.OnDamageReceived += (currentHealth, damageAmount) =>
        {
            animator?.SetTrigger("Dmg");
        };
    }
}
