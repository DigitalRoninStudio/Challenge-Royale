using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorMapData", menuName = "EditorMapData/Map")]
public class MapBlueprint : ScriptableObject
{
    public int ColumnsAndRows;
    public float Offset;
    public float TileSize;
    public GameObject TilePrefab;

    public Map CreateMap()
    {
        return new HexagonMap(this, TilePrefab);
    }
}


