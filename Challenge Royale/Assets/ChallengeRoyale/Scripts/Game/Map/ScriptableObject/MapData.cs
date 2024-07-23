using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "MapData/Map")]
public class MapData : ScriptableObject
{
    public string Id;
    public MapType MapType;
    public float Offset;
    public float TileSize;
    public List<TileData> TilesData = new List<TileData>();
}