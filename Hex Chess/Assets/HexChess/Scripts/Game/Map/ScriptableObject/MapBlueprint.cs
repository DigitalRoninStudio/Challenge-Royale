using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorMapData", menuName = "EditorMapData/Map")]
public class MapBlueprint : ScriptableObject
{
    public string Id;
    public MapType MapType;
    public float Offset;
    public float TileSize;
    public List<TileBlueprint> TilesData = new List<TileBlueprint>();
    public List<SpawnPosition> SpawnPositions = new List<SpawnPosition>();

    public Map CreateMap()
    {
        Map map = null;
        switch (MapType)
        {
            case MapType.HEX:
                map = new HexagonMap(this, MapEditor.Instance.hex);
                break;
            case MapType.SQUARE:
                map = new SquareMap(this, MapEditor.Instance.square);
                break;
        }
        return map;
    }
}

[Serializable]
public class SpawnPosition
{
    public Vector2Int Coordinate;
    public Team Team;
    public FigureType FigureType;
}
