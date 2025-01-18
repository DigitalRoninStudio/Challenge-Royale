using System;

public class Figure : Entity
{
    public FractionType FractionType { get; private set; }
    public FigureType FigureType { get; private set; }

    public class Builder : Builder<Figure, FigureBlueprint, FigureData>
    {
        public new Builder WithBlueprint(FigureBlueprint blueprint)
        {
            base.WithBlueprint(blueprint);
            _entity.FigureType = blueprint.FigureType;
            _entity.FractionType = blueprint.FractionType;

            _entity.gameObject.GetComponent<FigureVisual>().Initialize(_entity, blueprint);  //???
            return this;
        }
    }
    public override EntityData GetEntityData() => new FigureData(this);
}





