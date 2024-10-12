using System;

public class Figure : Entity
{
    public FractionType FractionType { get; private set; }
    public FigureType FigureType { get; private set; }

    public Figure() : base() { }

    public Figure(FigureBlueprint figureData) : base(figureData)
    {
        FigureType = figureData.FigureType;
        FractionType = figureData.FractionType;

        gameObject.GetComponent<FigureVisual>().Initialize(this, figureData);
    }
    public override EntityData GetEntityData() => new FigureData(this);
}





