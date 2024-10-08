using System;
using UnityEditor;
using UnityEngine;

public class MapEditor : Singleton<MapEditor>
{
    [Header("Map Data")]
    public MapType MapType;
    public Vector2Int MaxCordinate;
    public float Offset;
    public float TileSize;

    [Header("Path")]
    [SerializeField][ReadOnly] private string hexMapDataPath = "Assets/Resources/Data/Map/Hex";
    [SerializeField][ReadOnly] private string squareMapDataPath = "Assets/Resources/Data/Map/Square";

    [Header("Save & Load")]
    public string SaveAs;
    public string MapId;

    [Header("Prefab")]
    public GameObject hex;
    public GameObject square;

    public Action<Tile> OnSelectTile;

    public Map Map => map;
    private Map map;
    public Tile CurrentTile => currentTile;
    private Tile currentTile;

    public void CreateMap()
    {
        switch (MapType)
        {
            case MapType.HEX:
                map = new HexagonMap(MaxCordinate.x, MaxCordinate.y, Offset, TileSize, hex);
                break;

            case MapType.SQUARE:
                map = new SquareMap(MaxCordinate.x, MaxCordinate.y, Offset, TileSize, square);
                break;
        }
    }
    
    public void SaveMap()
    {
        if (!ValidateSaveMap()) return;

        MapBlueprint mapData = GetMapById(MapId);
        string assetPathAndName = "";

        if (mapData == null)
        {
            mapData = ScriptableObject.CreateInstance<MapBlueprint>();
            mapData.Id = MapId;
            mapData.Offset = Offset;
            mapData.TileSize = TileSize;
            mapData.MapType = MapType;
            assetPathAndName = GetPathByMapType(MapType);
        }
        else
            assetPathAndName = GetPathByMapType(mapData.MapType);

        mapData.TilesData.Clear();
        foreach (var tile in map.Tiles)
        {
            mapData.TilesData.Add(new TileBlueprint()
            {
                Coordinate = tile.coordinate,
                Position = tile.GetPosition(),
                Color = tile.GetColor()
            });
        }

        mapData.SpawnPositions = map.SpawnPositions;

        if (AssetDatabase.Contains(mapData))
            EditorUtility.SetDirty(mapData);
        else
            AssetDatabase.CreateAsset(mapData, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = mapData;
    }

    public void LoadMap()
    {
        if (!ValidateLoadMap()) return;

        MapBlueprint mapData = GetMapById(MapId);
        if(mapData != null)
        {
            switch (mapData.MapType)
            {
                case MapType.HEX:
                    map = new HexagonMap(mapData, hex);
                    break;
                case MapType.SQUARE:
                    map = new SquareMap(mapData,square);
                    break;
            }
        }
        else
            Debug.LogWarning("Fail to load the map: The map with that id does not exist !!!");
    }
    public void ClearMap()
    {
        if(map != null)
        {
            foreach (var tile in map.Tiles)
                tile.Dispose();

            map.ClearTiles();
        }
    }

    public void DeleteMap()
    {
        MapBlueprint mapData = GetMapById(MapId);
        if (mapData != null)
        {
            ClearMap();
            string assetPath = AssetDatabase.GetAssetPath(mapData);
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
        }
    }
    private string GetPathByMapType(MapType mapType)
    {
        switch (MapType)
        {
            case MapType.HEX:
                return $"{hexMapDataPath}/{SaveAs}.asset";
            case MapType.SQUARE:
                return $"{squareMapDataPath}/{SaveAs}.asset";
            default:
                return "";
        }
    }
    private MapBlueprint GetMapById(string id)
    {
        string[] guids = AssetDatabase.FindAssets("t:MapBlueprint");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MapBlueprint mapData = AssetDatabase.LoadAssetAtPath<MapBlueprint>(path);
            if (mapData != null && string.Equals(id, mapData.Id))
                return mapData;
        }

        return null;
    }

    public void SetTileTeam(Team team)
    {
        if(currentTile != null)
        {
            bool containsTile = false;
            foreach (var spawnPosition in map.SpawnPositions)
                if (currentTile.coordinate == spawnPosition.Coordinate)
                {
                    containsTile = true;
                    spawnPosition.Team = team;
                }

            if(!containsTile)
            {
                SpawnPosition spawnPos = new SpawnPosition()
                {
                    Coordinate = currentTile.coordinate,
                    Team = team,
                };
                map.SpawnPositions.Add(spawnPos);
            }

        }
    }

    public Team GetTileTeam(Tile tile)
    {
        if (tile != null)
        {
            foreach (var spawnPosition in map.SpawnPositions)
                if (tile.coordinate == spawnPosition.Coordinate)
                    return spawnPosition.Team;
        }

        return Team.NONE;
    }

    public void SetTileFigure(FigureType figureType)
    {
        if (currentTile != null)
        {
            bool containsTile = false;

            foreach (var spawnPosition in map.SpawnPositions)
                if (currentTile.coordinate == spawnPosition.Coordinate)
                {
                    containsTile = true;
                    spawnPosition.FigureType = figureType;
                }

            if (!containsTile)
            {
                SpawnPosition spawnPos = new SpawnPosition()
                {
                    Coordinate = currentTile.coordinate,
                    FigureType = figureType,
                };
                map.SpawnPositions.Add(spawnPos);
            }
        }
    }

    public FigureType GetTileFigure(Tile tile)
    {
        if (tile != null)
        {
            foreach (var spawnPosition in map.SpawnPositions)
                if (tile.coordinate == spawnPosition.Coordinate)
                    return spawnPosition.FigureType;
        }

        return FigureType.None;
    }

    public void SetCurrentTile(Tile tile) 
    { 
        currentTile = tile;
        OnSelectTile?.Invoke(currentTile);
    }

    public void AddTile(Vector2 worldPosition)
    {
        Vector2Int tileCoordinate = map.GetTileCoordinatesFromWorldPosition(worldPosition);
        Vector2 tilePosition = map.GetTilePositionFromWorldPosition(worldPosition);
        Tile tile = null;

        if (map is HexagonMap)
        {
            tile = new Hex(tileCoordinate, tilePosition);
            if (hex != null)
                tile.InstatniateTileGameObject(hex, Color.white);
        }
        else if (map is SquareMap)
        {
            tile = new Square(tileCoordinate, tilePosition);
            if (square != null)
                tile.InstatniateTileGameObject(square, Color.white);
        }

        map.AddTile(tile);
        tile.SetNeighbors(map);
        OnSelectTile?.Invoke(currentTile);
    }

    public void RemoveTile(Tile tile)
    {
        map.RemoveTile(tile);
        tile.Dispose();
    }

    private bool ValidateSaveMap()
    {
        if (string.IsNullOrEmpty(SaveAs))
        {
            Debug.LogWarning("Fail to save the map: Save As not entered !!!");
            return false;
        }
        if (string.IsNullOrEmpty(MapId))
        {
            Debug.LogWarning("Fail to save the map: Map id not entered !!!");
            return false;
        }
        if (MaxCordinate == Vector2Int.zero)
        {
            Debug.LogWarning("Fail to save the map: Max Coordinate are both 0 !!!");
            return false;
        }
        if (Offset < 1)
        {
            Debug.LogWarning("Fail to save the map: Offset is smaller then 1 !!!");
            return false;
        }
        if (TileSize <= 0)
        {
            Debug.LogWarning("Fail to save the map: Tile Size is smaller or equal to 0 !!!");
            return false;
        }
        if (map == null)
        {
            Debug.LogWarning("Fail to save the map: Map is not created !!!");
            return false;
        }else if(map.Tiles.Count == 0)
        {
            Debug.LogWarning("Fail to save the map: Map is empty !!!");
            return false;
        }


        return true;
    }
    private bool ValidateLoadMap()
    {
        if (string.IsNullOrEmpty(MapId))
        {
            Debug.LogWarning("Fail to load the map: Map id not entered !!!");
            return false;
        }

        return true;
    }

}

public enum MapType
{
    HEX, SQUARE
}
