using UnityEngine;

[CreateAssetMenu(fileName = "FigureData", menuName = "EntityData/FigureData")]
public class FigureBlueprint : EntityBlueprint
{
    public FractionType FractionType;
    public FigureType FigureType;
    public override Entity CreateEntity() => new Figure(this);
}
