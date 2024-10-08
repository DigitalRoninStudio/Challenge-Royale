using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalMovement : MovementBehaviour
{
    public NormalMovement() { }
    public NormalMovement(Entity entity, float speed, int range) : base(entity, speed, range) { }

    public override List<Tile> GetAvailableMoves(Tile tile, Map map)
    {
        List<Tile> availableMoves = new List<Tile>();

        foreach (var tileInRange in PathFinder.BFS_RangeMovement(tile, Range, map))
            availableMoves.Add(tileInRange);

        return availableMoves;
    }

    public override void SetPath(Tile start, Tile end)
    {
        base.SetPath(start, end);

        path = PathFinder.FindPath_AStar(start, end, entity.Map);
    }
}



