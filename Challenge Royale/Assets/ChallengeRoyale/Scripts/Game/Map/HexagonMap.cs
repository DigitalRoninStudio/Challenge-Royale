﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class HexagonMap : Map
{
    public static readonly List<Vector2Int> neighborsVectors = new List<Vector2Int> 
    {
        new Vector2Int(0, 1),  // UP
        new Vector2Int(1, 0),  // UPPER_RIGHT
        new Vector2Int(1, -1), // LOWER_RIGHT
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, 0), // LOWER_LEFT
        new Vector2Int(-1, 1)  // UPPER_LEFT
    };

    public static readonly List<Vector2Int> diagonalsNeighborsVectors = new List<Vector2Int>() {
        new Vector2Int(-2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(-1, 2),
        new Vector2Int(1, 1),
        new Vector2Int(1, -2),
        new Vector2Int(-1, -1)};

    public static readonly Dictionary<Vector2Int, Direction> coordinateToDirection = new Dictionary<Vector2Int, Direction>
    {
        { new Vector2Int(0, 1), Direction.UP },
        { new Vector2Int(1, 0), Direction.UPPER_RIGHT_60 },
        { new Vector2Int(1, -1), Direction.LOWER_RIGHT_120 },
        { new Vector2Int(0, -1), Direction.DOWN },
        { new Vector2Int(-1, 0), Direction.LOWER_LEFT_120 },
        { new Vector2Int(-1, 1), Direction.UPPER_LEFT_60 },


        { new Vector2Int(-2, 1), Direction.LEFT },
        { new Vector2Int(2, -1), Direction.RIGHT },
        { new Vector2Int(-1, 2), Direction.UPPER_LEFT_45 },
        { new Vector2Int(1, 1), Direction.UPPER_RIGHT_45 },
        { new Vector2Int(1, -2), Direction.LOWER_RIGHT_135 },
        { new Vector2Int(-1, -1), Direction.LOWER_LEFT_135 },
    };

    public static readonly Dictionary<Direction, Vector2Int> directionToCoordinate = new Dictionary<Direction, Vector2Int>
    {
        { Direction.UP, new Vector2Int(0, 1) },            
        { Direction.UPPER_RIGHT_60, new Vector2Int(1, 0) },  
        { Direction.LOWER_RIGHT_120, new Vector2Int(1, -1) }, 
        { Direction.DOWN, new Vector2Int(0, -1) },           
        { Direction.LOWER_LEFT_120, new Vector2Int(-1, 0) },  
        { Direction.UPPER_LEFT_60, new Vector2Int(-1, 1) },    

        { Direction.LEFT, new Vector2Int(-2, 1) },
        { Direction.RIGHT, new Vector2Int(2, -1) },
        { Direction.UPPER_LEFT_45, new Vector2Int(-1, 2) },
        { Direction.UPPER_RIGHT_45, new Vector2Int(1, 1) },
        { Direction.LOWER_RIGHT_135, new Vector2Int(1, -2) },
        { Direction.LOWER_LEFT_135, new Vector2Int(-1, -1) },
    };


    public HexagonMap(int column, int row, float offset, float tileSize, GameObject tilePrefab = null) :
        base(column, row, offset, tileSize, tilePrefab) { }

    public HexagonMap(MapBlueprint mapData, GameObject tilePrefab) : base(mapData, tilePrefab) 
    {
        this.tilePrefab = tilePrefab;
        this.offset = mapData.Offset;
        this.tileSize = mapData.TileSize;
        CreateMap(mapData.ColumnsAndRows, mapData.ColumnsAndRows);
    }

    public override void CreateMap(int column, int row)
    {
        Vector3 pos = Vector3.zero;

        for (int c = -column; c <= column; c++)
        {
            int r1 = Mathf.Max(-column, -c - column);
            int r2 = Mathf.Min(column, -c + column);


            int counter = 0;
            int index = 0;

            for (int i = 0; i < Mathf.Abs(c); i++)
            {
                counter++;
                if (counter == 3 || counter == 0)
                {
                    counter = index = 0;
                    continue;
                }
                index = counter == 1 ? 2 : 1;
            }

            for (int r = r1; r <= r2; r++)
            {

                if (index == 3)
                    index = 0;

                pos.x = 3f / 4f * Hex.Width(tileSize) * c * offset;
                pos.z = Hex.Height(tileSize) * (r + c / 2f) * offset;

                Hex hex = new Hex(new Vector2Int(c, r), pos);
                if (tilePrefab != null)
                {
                    hex.InstatniateTileGameObject(tilePrefab, GameManager.Instance.palletes[0].Colors[index]);
                }

                tiles.Add(hex.coordinate, hex);
                index++;
            }
        }
            foreach (Tile tile in tiles.Values)
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
        Tile closestTile = GetTile(tilePosition.x, tilePosition.y);

        if (closestTile == null) return null;

        float closestDistance = Vector2.Distance(worldPosition, closestTile.GetPosition());

        foreach (var neighbor in closestTile.Neighbors)
        {
            float newDistance = Vector2.Distance(worldPosition, neighbor.GetPosition());

            if (newDistance < closestDistance)
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
        tileCoordinate.x * (Hex.Width(tileSize) * offset) * 0.75f,
        tileCoordinate.y * (Hex.Height(tileSize) * offset) + tileCoordinate.x * (Hex.Height(tileSize) * offset) * 0.5f);
    }
    public override Vector2Int GetTileCoordinatesFromWorldPosition(Vector2 worldPosition)
    {
        Vector2 axialCoord = GetAxialFromPosition(worldPosition);
        Vector3 cubeCoord = AxialToCube(axialCoord);
        Vector3 roundedCubeCoord = CubeRound(cubeCoord);
        return CubeToAxial(roundedCubeCoord);
    }

    private Vector2 GetAxialFromPosition(Vector3 position)
    {
        float q = (2f / 3f * position.x) / (Hex.OuterRadius(tileSize) * offset);
        float r = (-1f / 3f * position.x + Mathf.Sqrt(3) / 3f * position.y) / (Hex.OuterRadius(tileSize) * offset);
        return new Vector2(q, r);
    }

    private Vector3 AxialToCube(Vector2 axial)
    {
        float x = axial.x;
        float z = axial.y;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    private Vector2Int CubeToAxial(Vector3 cube)
    {
        return new Vector2Int(Mathf.RoundToInt(cube.x), Mathf.RoundToInt(cube.z));
    }

    private Vector3 CubeRound(Vector3 cube)
    {
        int rx = Mathf.RoundToInt(cube.x);
        int ry = Mathf.RoundToInt(cube.y);
        int rz = Mathf.RoundToInt(cube.z);

        float xDiff = Mathf.Abs(rx - cube.x);
        float yDiff = Mathf.Abs(ry - cube.y);
        float zDiff = Mathf.Abs(rz - cube.z);

        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        return new Vector3(rx, ry, rz);
    }
    public override List<Tile> TilesInRange(Tile tile, int range)
    {
        List<Tile> listOfTiles = new List<Tile>();

        for (int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range, -q - range); r <= Mathf.Min(range, -q + range); r++)
            {
                Vector2Int neighborOffset = new Vector2Int(q, r);
                Tile neighborInRange = GetTile(tile.coordinate + neighborOffset);
                if (neighborInRange != null)
                    listOfTiles.Add(neighborInRange);
            }
        }

        return listOfTiles;
    }

    public override void AddTile(int column, int row, Vector3 pos)
    {
        Hex hex = new Hex(new Vector2Int(column, row), pos);
        tiles.Add(new Vector2Int(column, row), hex);
    }

    public static (Vector2Int left, Vector2Int right) GetLeftAndRightCoordinate(Direction direction)
    {
        if(!directionToCoordinate.TryGetValue(direction, out var facingVector))
            throw new ArgumentException("Invalid facing direction!");

        int index = neighborsVectors.IndexOf(facingVector);

        int leftIndex = (index + 5) % 6;
        int rightIndex = (index + 1) % 6;

        return (
           neighborsVectors[leftIndex],
           neighborsVectors[rightIndex]
       );
    }

    public static (Direction left, Direction right) GetLeftAndRightDirections(Direction direction)
    {
        if (directionToCoordinate.TryGetValue(direction, out var facingVector))
            throw new ArgumentException("Invalid facing direction!");

        int index = neighborsVectors.IndexOf(facingVector);
        int leftIndex = (index + 5) % 6;
        int rightIndex = (index + 1) % 6;

        var leftDirection = coordinateToDirection[neighborsVectors[leftIndex]];
        var rightDirection = coordinateToDirection[neighborsVectors[rightIndex]];

        return (leftDirection, rightDirection);
    }
    public static Vector2Int TransformCoordinatesToUnitCoordinates(Vector2Int coordinates)
    {
        int x = coordinates.x != 0 ? Math.Sign(coordinates.x) : 0;
        int y = coordinates.y != 0 ? Math.Sign(coordinates.y) : 0;

        return new Vector2Int(x, y);
    }
}

