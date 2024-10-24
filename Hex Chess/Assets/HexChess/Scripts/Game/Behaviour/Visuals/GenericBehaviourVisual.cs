
public abstract class GenericBehaviourVisual<TBehaviour> : BehaviourVisual where TBehaviour : Behaviour
{
    protected TBehaviour behaviour;

    public override void Initialize(Behaviour behaviour)
    {
        this.behaviour = behaviour as TBehaviour;

        InitializeVisual();
    }

    protected abstract void InitializeVisual();
}
