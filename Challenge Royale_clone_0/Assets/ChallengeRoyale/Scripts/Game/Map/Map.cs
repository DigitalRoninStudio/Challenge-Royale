﻿using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Map
{
    protected float offset;
    protected float tileSize;
    protected GameObject tilePrefab;
    protected Dictionary<Vector2Int, Tile> tiles;
    public Map(int column, int row, float offset, float tileSize, GameObject tilePrefab = null)
    {
        this.offset = offset;
        this.tileSize = tileSize;
        this.tilePrefab = tilePrefab;
        tiles = new Dictionary<Vector2Int, Tile>();
        CreateMap(column, row);
    }

    public Map(MapData mapData, GameObject tilePrefab) 
    {
        this.offset = mapData.Offset;
        this.tileSize = mapData.TileSize;
        this.tilePrefab = tilePrefab;
        tiles = new Dictionary<Vector2Int, Tile>();
    }

    public Tile GetTile(Vector2Int coordinate)
    {
        return tiles.ContainsKey(coordinate) ? tiles[coordinate] : null;
    }

    public Tile GetTile(int column, int row)
    {
        Vector2Int coordinate = new Vector2Int(column, row);
        return tiles.ContainsKey(coordinate) ? tiles[coordinate] : null;
    }

    public Tile GetTile(Entity entity)
    {
        foreach (var tile in tiles.Values) 
            if(tile.GetEntities().Contains(entity))
                return tile;

        return null;
    }
    public Tile RemoveTile(Tile tile)
    {
        if (tiles.ContainsKey(tile.coordinate))
        {
            tiles.Remove(tile.coordinate);
            return tile;
        }

        return null;
    }
    public Tile RemoveTile(int column, int row)
    {
        Vector2Int position = new Vector2Int(column, row);
        if (tiles.ContainsKey(position))
        {
            Tile tile = tiles[position];
            tiles.Remove(position);
            return tile;
        }

        return null;
    }

    public List<Tile> GetTiles()
    {
        return tiles.Values.Cast<Tile>().ToList();  
    }

    public void AddTile(Tile tile)
    {
        tiles.Add(tile.coordinate, tile);
    }

    public void ClearTiles()
    {
        tiles.Clear();
    }

    public abstract void AddTile(int column, int row, Vector3 pos);

    public abstract void CreateMap(int column, int row);
    public abstract Tile OnHoverMapGetTile(Vector2 worldPosition);
    public abstract Vector2 GetTilePositionFromWorldPosition(Vector2 worldPosition);
    public abstract Vector2Int GetTileCoordinatesFromWorldPosition(Vector2 worldPosition);
    public abstract List<Vector2Int> GetNeighborsVectors();
    public abstract List<Vector2Int> GetDiagonalsNeighborsVectors();
    public abstract List<Tile> TilesInRange(Tile tile, int range);
    public List<Tile> GetTilesInDirection(Direction direction, Tile tile, int range, bool includeUnwalkableTiles = true)
    {
        List<Tile> directionTiles = new List<Tile>();
        Vector2Int directionCoordinate = DirectionToCoordinate(direction);

        for (int i = 1; i < range + 1; i++)
        {
            Tile tileInDirection = GetTile(
                i * directionCoordinate.x + tile.coordinate.x,
                i * directionCoordinate.y + tile.coordinate.y);

            if (tileInDirection != null)
                if (includeUnwalkableTiles || tileInDirection.IsWalkable())
                    directionTiles.Add(tileInDirection);
        }
        return directionTiles;
    }
    public abstract Vector2Int DirectionToCoordinate(Direction direction);
    public abstract Direction CoordinateToDirection(Vector2Int coordinate);
    public void Draw()
    {
        foreach (var tile in tiles)
            tile.Value.Draw(tileSize);
    }
}

