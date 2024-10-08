using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SquareMap : Map
{
    public static readonly List<Vector2Int> neighborsVectors = new List<Vector2Int>
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

    public static readonly List<Vector2Int> diagonalsNeighborsVectors = new List<Vector2Int>
    {
        new Vector2Int(1, 1), 
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, -1) 
    };

    public static readonly Dictionary<Vector2Int, Direction> coordinateToDirection = new Dictionary<Vector2Int, Direction>
    {
        { new Vector2Int(0, 1), Direction.UP },
        { new Vector2Int(0, -1), Direction.DOWN },
        { new Vector2Int(1, 0), Direction.RIGHT },
        { new Vector2Int(-1, 0), Direction.LEFT },

        { new Vector2Int(1, 1), Direction.UPPER_RIGHT_45 },
        { new Vector2Int(-1, 1), Direction.UPPER_LEFT_45 },
        { new Vector2Int(1, -1), Direction.LOWER_RIGHT_135 },
        { new Vector2Int(-1, -1), Direction.LOWER_LEFT_135 }
    };

    public static readonly Dictionary<Direction, Vector2Int> directionToCoordinate = new Dictionary<Direction, Vector2Int>
    {
        { Direction.UP, new Vector2Int(0, 1) },
        { Direction.DOWN, new Vector2Int(0, -1) },
        { Direction.RIGHT, new Vector2Int(1, 0) },
        { Direction.LEFT, new Vector2Int(-1, 0) },

        { Direction.UPPER_RIGHT_45, new Vector2Int(1, 1) },
        { Direction.UPPER_LEFT_45, new Vector2Int(-1, 1) },
        { Direction.LOWER_RIGHT_135, new Vector2Int(1, -1) },
        { Direction.LOWER_LEFT_135, new Vector2Int(-1, -1) }
    };

    public SquareMap(int column, int row, float offset, float tileSize, GameObject tilePrefab = null) :
        base(column, row, offset, tileSize, tilePrefab) { }

    public SquareMap(MapData mapData, GameObject tilePrefab) : base(mapData, tilePrefab)
    {
        foreach (var tileData in mapData.TilesData)
        {
            Square square = new Square(tileData.coordinate, tileData.position);
            if (tilePrefab != null)
                square.InstatniateTileGameObject(tilePrefab, tileData.color);

            tiles.Add(tileData.coordinate, square);
        }

        foreach (Tile tile in tiles.Values)
            tile.SetNeighbors(this);
    }

    public override void CreateMap(int column, int row)
    {
        Vector3 pos = Vector3.zero;
        Vector2Int center = new Vector2Int(column / 2, row / 2);

        for (int c = -center.x; c <= center.x; c++)
        {
            for (int r = -center.y; r <= center.y; r++)
            {
                pos.x = c * Square.Width(tileSize) * offset;
                pos.y = r * Square.Height(tileSize) * offset;

                Square square = new Square(new Vector2Int(c, r), pos);
                if (tilePrefab != null)
                    square.InstatniateTileGameObject(tilePrefab, Color.white);

                tiles.Add(new Vector2Int(c, r), square);
            }
        }

        foreach (Square tile in tiles.Values)
            tile.SetNeighbors(this);
    }


    public override Direction CoordinateToDirection(Vector2Int coordinate)
    {
        if (coordinateToDirection.TryGetValue(coordinate, out Direction direction))
            return direction;

        throw new ArgumentException("Invalid coordinates");
    }

    public override Vector2Int DirectionToCoordinate(Direction direction)
    {
        if (directionToCoordinate.TryGetValue(direction, out Vector2Int coordinate))
            return coordinate;

        throw new ArgumentException("Invalid direction");
    }

    public override List<Vector2Int> GetDiagonalsNeighborsVectors()
    {
        return diagonalsNeighborsVectors;
    }

    public override List<Vector2Int> GetNeighborsVectors()
    {
        return neighborsVectors;
    }
    public override Tile OnHoverMapGetTile(Vector2 worldPosition)
    {
        Vector2Int tilePosition = GetTileCoordinatesFromWorldPosition(worldPosition);
        Tile closestTile = GetTile(tilePosition);

        if (closestTile == null) return null;

        float closestDistance = Vector2.Distance(worldPosition, closestTile.GetPosition());

        foreach (var neighbor in closestTile.Neighbors)
        {
            float newDistance = Vector2.Distance(worldPosition, neighbor.GetPosition());

            if(newDistance < closestDistance)
            {
                closestDistance = newDistance;
                closestTile = neighbor;
            }
        }

        return closestTile;
    }
    public override Vector2 GetTilePositionFromWorldPosition(Vector2 worldPosition)
    {
        Vector2Int tileCoordinate = GetTileCoordinatesFromWorldPosition(worldPosition);

        return new Vector2(
        tileCoordinate.x * Square.Width(tileSize) * offset,
        tileCoordinate.y * Square.Height(tileSize) * offset);
    }
    public override Vector2Int GetTileCoordinatesFromWorldPosition(Vector2 worldPosition)
    {
        int column = Mathf.RoundToInt(worldPosition.x / (Square.Width(tileSize) * offset));
        int row = Mathf.RoundToInt(worldPosition.y / (Square.Height(tileSize) * offset));
        return new Vector2Int(column, row);
    }

    public override List<Tile> TilesInRange(Tile tile, int range)
    {
        List<Tile> listOfTiles = new List<Tile>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (Mathf.Abs(dx) + Mathf.Abs(dy) <= range)
                {
                    Vector2Int neighborOffset = new Vector2Int(dx, dy);
                    Tile neighborInRange = GetTile(tile.coordinate + neighborOffset);
                    if (neighborInRange != null)
                        listOfTiles.Add(neighborInRange);
                }
            }
        }

        return listOfTiles;
    }
    public override void AddTile(int column, int row, Vector3 pos)
    {
        Square square = new Square(new Vector2Int(column, row), pos);
        tiles.Add(new Vector2Int(column, row), square);
    }
}


