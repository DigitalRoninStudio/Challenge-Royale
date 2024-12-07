using System;
using System.Collections.Generic;
using UnityEngine;

public class NormalMovementBehaviour : MovementBehaviour
{
    public NormalMovementBehaviour() { }
    public NormalMovementBehaviour(NormalMovementBlueprint blueprint) : base(blueprint) { }

    public override void SetPath(Tile end)
    {
        base.SetPath(end);

        path = PathFinder.FindPath_AStar(currentTile, end, Map);
    }
    public override BehaviourData GetBehaviourData() => new NormalMovementData(this);

    public override List<Tile> GetAvailableTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return availableMoves;

        foreach (var tileInRange in PathFinder.BFS_RangeMovement(tile, range, Map))
            availableMoves.Add(tileInRange);

        return availableMoves;
    }

    public override List<Tile> GetTiles()
    {
        Tile tile = Map.GetTile(Owner);

        if (tile == null) return new List<Tile>();

        return Map.TilesInRange(tile, range);
    }
}

public class KnightMovementBehaviour : MovementBehaviour
{
    public KnightMovementBehaviour() { }
    public KnightMovementBehaviour(KnightMovementBlueprint blueprint) : base(blueprint) { }

    public override List<Tile> GetAvailableTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return availableMoves;
        //move inside map methods
        foreach (var diagonalVector in HexagonMap.diagonalsNeighborsVectors)
        {
            for (int i = 1; i <= range; i++)
            {
                Tile diagonalTile = Map.GetTile(tile.coordinate.x + diagonalVector.x * i, tile.coordinate.y + diagonalVector.y * i);
                if (diagonalTile != null && diagonalTile.Walkable)
                    availableMoves.Add(diagonalTile);
            }
        }

        return availableMoves;
    }

    public override List<Tile> GetTiles()
    {
        List<Tile> tiles = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return tiles;
        //move inside map methods
        foreach (var diagonalVector in HexagonMap.diagonalsNeighborsVectors)
        {
            for (int i = 1; i <= range; i++)
            {
                Tile diagonalTile = Map.GetTile(tile.coordinate.x + diagonalVector.x * i, tile.coordinate.y + diagonalVector.y * i);
                if (diagonalTile != null)
                    tiles.Add(diagonalTile);
            }
        }

        return tiles;
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
            path.Dequeue().RemoveEntity(Owner);
            path.Dequeue().AddEntity(Owner);
            Exit();
        }
    }
    public override List<Tile> GetAvailableTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

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

    public override List<Tile> GetTiles()
    {
        Tile tile = Map.GetTile(Owner);

        if (tile == null) return new List<Tile>();

        return Map.TilesInRange(tile, range);
    }
}

public class DirectionMovementBehaviour : MovementBehaviour
{
    public DirectionMovementBehaviour() { }
    public DirectionMovementBehaviour(DirectionMovementBlueprint blueprint) : base(blueprint) { }

    public override List<Tile> GetAvailableTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return availableMoves;
        //move method inside map
        foreach (var neigborVectors in HexagonMap.neighborsVectors)
            if (HexagonMap.coordinateToDirection.TryGetValue(neigborVectors, out Direction direction))
                availableMoves.AddRange(Map.GetTilesInDirection(tile, direction, range, false, true));

        return availableMoves;
    }
    public override List<Tile> GetTiles()
    {
        List<Tile> availableMoves = new List<Tile>();

        Tile tile = Map.GetTile(Owner);

        if (tile == null) return availableMoves;
        //move method inside map
        foreach (var neigborVectors in HexagonMap.neighborsVectors)
            if (HexagonMap.coordinateToDirection.TryGetValue(neigborVectors, out Direction direction))
                availableMoves.AddRange(Map.GetTilesInDirection(tile, direction, range));

        return availableMoves;
    }
    public override void SetPath(Tile end)
    {
        base.SetPath(end);
        path = PathFinder.FindPath_AStar(currentTile, end, Map);

    }
    public override BehaviourData GetBehaviourData() => new DirectionMovementData(this);
    
}




