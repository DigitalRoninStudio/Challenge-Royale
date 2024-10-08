using System.Collections.Generic;
using System.IO;
using TMPro;
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

    [Header("Coloring")]
    public List<Color> colors = new List<Color>()
    { 
        Color.white, Color.grey, Color.black, Color.yellow
    };

    private Map map;
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

    public void RemoveTile(Tile tile)
    {
        map.RemoveTile(tile);
    }

    public void AddTile(int column, int row, Vector3 pos)
    {
        map.AddTile(column, row, pos);
    }
    
    public void SaveMap()
    {
        if (!ValidateSaveMap()) return;

        MapData mapData = GetMapById(MapId);
        string assetPathAndName = "";

        if (mapData == null)
        {
            mapData = ScriptableObject.CreateInstance<MapData>();
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
            mapData.TilesData.Add(new TileData()
            {
                coordinate = tile.coordinate,
                position = tile.GetPosition(),
                color = tile.GetColor()
            });
        }

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

        MapData mapData = GetMapById(MapId);
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
        MapData mapData = GetMapById(MapId);
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
    private MapData GetMapById(string id)
    {
        string[] guids = AssetDatabase.FindAssets("t:MapData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MapData mapData = AssetDatabase.LoadAssetAtPath<MapData>(path);
            if (mapData != null && string.Equals(id, mapData.Id))
                return mapData;
        }

        return null;
    }

    private void Update()
    {
        map?.Draw();

        if (Input.GetKeyDown(KeyCode.Space))
            SaveMap();

        if(Input.GetMouseButtonDown(1))
        {
            Tile tile = map?.OnHoverMapGetTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (tile != null && tile.GetObject() != null)
            {
                map.RemoveTile(tile);
                tile.Dispose();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Tile tile = map?.OnHoverMapGetTile(worldPosition);
            if (tile == null)
            {
                Vector2Int tileCoordinate = map.GetTileCoordinatesFromWorldPosition(worldPosition);
                Vector2 tilePosition = map.GetTilePositionFromWorldPosition(worldPosition);

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
            }
            else
                currentTile = tile;
        }

        if (Input.GetKeyDown(KeyCode.W) && currentTile != null)
            currentTile.SetColor(colors[0]);
        if (Input.GetKeyDown(KeyCode.G) && currentTile != null)
            currentTile.SetColor(colors[1]);
        if (Input.GetKeyDown(KeyCode.B) && currentTile != null)
            currentTile.SetColor(colors[2]);
        if (Input.GetKeyDown(KeyCode.Y) && currentTile != null)
            currentTile.SetColor(colors[3]);
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
