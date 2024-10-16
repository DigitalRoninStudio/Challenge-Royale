using System;
using System.Collections.Generic;
using UnityEngine;

public class NormalMovementBehaviour : MovementBehaviour
{
    public NormalMovementBehaviour() { }
    public NormalMovementBehaviour(NormalMovementBlueprint blueprint) : base(blueprint) { }

    public override List<Tile> GetAvailableMoves()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(owner);

        if (tile == null) return availableMoves;

        foreach (var tileInRange in PathFinder.BFS_RangeMovement(tile, range, Map))
            availableMoves.Add(tileInRange);

        return availableMoves;
    }

    public override void SetPath(Tile end)
    {
        base.SetPath(end);

        path = PathFinder.FindPath_AStar(currentTile, end, Map);
    }
    public override BehaviourData GetBehaviourData() => new NormalMovementData(this);
}

public class KnightMovementBehaviour : MovementBehaviour
{
    public KnightMovementBehaviour() { }
    public KnightMovementBehaviour(KnightMovementBlueprint blueprint) : base(blueprint) { }

    public override List<Tile> GetAvailableMoves()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(owner);

        if (tile == null) return availableMoves;

        foreach (var diagonalVector in HexagonMap.diagonalsNeighborsVectors)
        {
            for (int i = 1; i <= range; i++)
            {
                Tile diagonalTile = Map.GetTile(tile.coordinate.x + diagonalVector.x * i, tile.coordinate.y + diagonalVector.y * i);
                if (diagonalTile != null)
                    availableMoves.Add(diagonalTile);
            }
        }

        return availableMoves;
    }

    public override void SetPath(Tile end)
    {
        base.SetPath(end);

        path.Enqueue(currentTile);
        path.Enqueue(end);
    }
    public override BehaviourData GetBehaviourData() => new KnightMovementData(this);
}

public class TeleportMovementBehaviour : MovementBehaviour
{
    public TeleportMovementBehaviour() { }
    public TeleportMovementBehaviour(TeleportMovementBlueprint blueprint) : base(blueprint) { }

    public override void Execute()
    {
        if (Time.time >= time + speed)
        {
            path.Dequeue().RemoveEntity(owner);
            path.Dequeue().AddEntity(owner);
            Exit();
        }
    }
    public override List<Tile> GetAvailableMoves()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(owner);

        if (tile == null) return availableMoves;

        foreach (var tileInRange in PathFinder.BFS_RangeMovement(tile, range, Map))
            availableMoves.Add(tileInRange);

        return availableMoves;
    }

    public override void SetPath(Tile end)
    {
        base.SetPath(end);

        path.Enqueue(currentTile);
        path.Enqueue(end);
    }
    public override BehaviourData GetBehaviourData() => new TeleportMovementData(this);
}

public class DirectionMovementBehaviour : MovementBehaviour
{
    public DirectionMovementBehaviour() { }
    public DirectionMovementBehaviour(DirectionMovementBlueprint blueprint) : base(blueprint) { }

    public override List<Tile> GetAvailableMoves()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(owner);

        if (tile == null) return availableMoves;

        foreach (var neigborVectors in HexagonMap.neighborsVectors)
            if (HexagonMap.coordinateToDirection.TryGetValue(neigborVectors, out Direction direction))
                availableMoves.AddRange(Map.GetTilesInDirection(tile, direction, range, false, true));

        return availableMoves;
    }
    public override void SetPath(Tile end)
    {
        base.SetPath(end);
        path = PathFinder.FindPath_AStar(currentTile, end, Map);

    }
    public override BehaviourData GetBehaviourData() => new DirectionMovementData(this);
    
}




