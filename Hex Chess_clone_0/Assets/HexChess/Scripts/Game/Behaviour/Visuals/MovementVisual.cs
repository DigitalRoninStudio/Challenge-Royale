using UnityEngine;

public class MovementVisual : GenericBehaviourVisual<MovementBehaviour>
{
    [SerializeField] private GameObject Trail;
    private ParticleSystem[] trailPartycleSystem;
    protected override void InitializeVisual()
    {
        trailPartycleSystem = Trail.GetComponentsInChildren<ParticleSystem>();
        if (trailPartycleSystem != null)
        {
            StopTrailParticle();
            behaviour.OnTileExit += OnTileExit;
        }
    }

    private void OnTileExit(Tile currentTile, Tile nextTile)
    {
        Vector3 direction = (nextTile.GetPosition() - behaviour.Owner.GameObject.transform.position).normalized;
        Trail.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        PlayTrailParticle();
    }

    private void PlayTrailParticle()
    {
        foreach (ParticleSystem particle in trailPartycleSystem)
            particle.Play();
    }

    private void StopTrailParticle()
    {
        foreach (ParticleSystem particle in trailPartycleSystem)
            particle.Stop();
    }
}