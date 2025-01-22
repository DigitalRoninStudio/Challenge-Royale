using TMPro;
using UnityEngine;

public class AttackableVisual : GenericBehaviourVisual<AttackBehaviour>
{
    protected override void InitializeVisual()
    {
        Animator animator = ((Figure)behaviour.Owner).GameObject.GetComponent<FigureVisual>().animator;

        if (animator == null) return;
        behaviour.OnActionStart += () =>
        {
            animator?.SetTrigger("Attack");
        };
    }


    /*[SerializeField] private TextMeshPro Damage;

    [SerializeField] private GameObject HitImpact; 
    private ParticleSystem[] hitPartycleSystem;

    protected override void InitializeVisual()
    {
        Damage.text = behaviour.AttackDamage.ToString();
        hitPartycleSystem = HitImpact.GetComponentsInChildren<ParticleSystem>();

        if (hitPartycleSystem != null)
        {
            StopHitParticle();
            behaviour.OnAttackPerformed += OnAttackPerformed;
        }
    }
    private void OnAttackPerformed(DamageableBehaviour damageable, Damage damage)
    {
        HitImpact.transform.position = damageable.Owner.gameObject.transform.position;
        PlayHitParticle();
    }

    private void PlayHitParticle()
    {
        foreach (ParticleSystem particle in hitPartycleSystem)
            particle.Play();
    }

    private void StopHitParticle()
    {
        foreach (ParticleSystem particle in hitPartycleSystem)
            particle.Stop();
    }*/
}
