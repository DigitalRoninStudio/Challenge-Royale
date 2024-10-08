using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Map
{
    public string MapId;
    public List<SpawnPosition> SpawnPositions;
    public List<Tile> Tiles => GetTiles();

    protected float offset;
    protected float tileSize;
    protected GameObject tilePrefab;
    protected Dictionary<Vector2Int, Tile> tiles;

    public static readonly Dictionary<Direction, int> directionToRotation = new Dictionary<Direction, int>
    {
        { Direction.UP, 0 },
        { Direction.DOWN, 180 },
        { Direction.RIGHT, 90 },
        { Direction.LEFT, -90 },
        { Direction.UPPER_RIGHT_45, 45 },
        { Direction.UPPER_LEFT_45, -45 },
        { Direction.LOWER_RIGHT_135, 135 },
        { Direction.LOWER_LEFT_135, -135 }
    };

    public Map(int column, int row, float offset, float tileSize, GameObject tilePrefab = null)
    {
        this.offset = offset;
        this.tileSize = tileSize;
        this.tilePrefab = tilePrefab;
        tiles = new Dictionary<Vector2Int, Tile>();
        SpawnPositions = new List<SpawnPosition>();
        CreateMap(column, row);
    }

    public Map(MapBlueprint mapData, GameObject tilePrefab) 
    {
        this.offset = mapData.Offset;
        this.tileSize = mapData.TileSize;
        this.SpawnPositions = mapData.SpawnPositions;
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
        var spawnPositionToRemove = SpawnPositions.Find(sp => sp.Coordinate == tile.coordinate);
        if (spawnPositionToRemove != null)
            SpawnPositions.Remove(spawnPositionToRemove);

        if (tiles.ContainsKey(tile.coordinate))
        {
            tiles.Remove(tile.coordinate);
            return tile;
        }

        return null;
    }
    public Tile RemoveTile(int column, int row)
    {
        Vector2Int coordinate = new Vector2Int(column, row);


        var spawnPositionToRemove = SpawnPositions.Find(sp => sp.Coordinate == coordinate);
        if (spawnPositionToRemove != null)
            SpawnPositions.Remove(spawnPositionToRemove);

        if (tiles.ContainsKey(coordinate))
        {
            Tile tile = tiles[coordinate];
            tiles.Remove(coordinate);
            return tile;
        }

        return null;
    }

    protected List<Tile> GetTiles()
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
    public List<Tile> GetTilesInDirection(Tile tile, Direction direction, int range, bool includeUnwalkableTiles = true)
    {
        List<Tile> directionTiles = new List<Tile>();
        Vector2Int directionCoordinate = DirectionToCoordinate(direction);

        for (int i = 1; i < range + 1; i++)
        {
            Tile tileInDirection = GetTile(
                i * directionCoordinate.x + tile.coordinate.x,
                i * directionCoordinate.y + tile.coordinate.y);

            if (tileInDirection != null)
                if (includeUnwalkableTiles || tileInDirection.Walkable)
                    directionTiles.Add(tileInDirection);
        }
        return directionTiles;
    }
    public abstract Vector2Int DirectionToCoordinate(Direction direction);
    public abstract Direction CoordinateToDirection(Vector2Int coordinate);
    public int DirectionToRotation(Direction direction)
    {
        if (directionToRotation.TryGetValue(direction, out int rotation))
            return rotation;

        throw new ArgumentException("Invalid direction");
    }
    public void Draw()
    {
        foreach (var tile in tiles)
            tile.Value.Draw(tileSize);
    }
}


