
public abstract class GenericBehaviourVisual<TBehaviour, TBlueprint> : BehaviourVisual where TBehaviour : Behaviour
    where TBlueprint : BehaviourBlueprint
{

    protected TBehaviour behaviour;
    protected TBlueprint blueprint;

    public override void Initialize(Behaviour behaviour, BehaviourBlueprint blueprint)
    {
        this.behaviour = behaviour as TBehaviour;
        this.blueprint = blueprint as TBlueprint;

        InitializeVisual();
    }

    protected abstract void InitializeVisual();
}
