using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FigureData", menuName = "EntityData/FigureData")]
public class FigureBlueprint : EntityBlueprint
{
    public FractionType FractionType;
    public FigureType FigureType;

    public override Entity CreateEntity()
    {
        return new Figure.Builder()
            .WithBlueprint(this)
            .WithGeneratedId()
            .WithBehaviours(this)
            .Build();
    }
    public override Entity CreateEntity(RandomGenerator randomGenerator)
    {
        return new Figure.Builder()
            .WithBlueprint(this)
            .WithSyncGeneratedId(randomGenerator.NextGuid())
            .WithBehaviours(this)
            .Build();
    }
    public override Entity CreateEntity(EntityData entityData)
    {
        return new Figure.Builder()
            .WithBlueprint(this)
            .WithData(entityData as FigureData)
            .Build();
    }
}
