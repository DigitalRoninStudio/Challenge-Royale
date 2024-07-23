using TMPro;
using UnityEngine;

public class Hex : Tile
{
    public int S { set; get; }
    public Hex(Vector2Int coordinate, Vector3 position) : base(coordinate, position)
    {
        S = -coordinate.x - coordinate.y;
    }
    public static float OuterRadius(float size) { return size; }
    public static float InnerRadius(float size) { return size * Mathf.Sqrt(3) / 2f; }
    public static float Width(float size) { return size * 2f; }
    public static float Height(float size) { return size * Mathf.Sqrt(3); }
    public override Vector3[] Corners(float size)
    {
        Vector3[] corners = new Vector3[6];
        for (int i = 0; i < corners.Length; i++)
            corners[i] = Corner(i, size);
        return corners;
    }
    protected override Vector3 Corner(int index, float size)
    {
        float angle = 60f * index;
        Vector3 corner = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) * OuterRadius(size) + position;
        return corner;
    }
    public override GameObject InstatniateTileGameObject(GameObject prefab, Color color)
    {
        obj = GameObject.Instantiate(prefab, position, Quaternion.identity);
        obj.name = $"Hex [{coordinate.x},{coordinate.y}]";
        obj.GetComponentInChildren<TextMeshPro>().text = $"[{coordinate.x},{coordinate.y}]";
        SetColor(color);
        return obj;
    }
}


