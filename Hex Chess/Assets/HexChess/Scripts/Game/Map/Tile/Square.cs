using TMPro;
using UnityEngine;

public class Square : Tile
{
    public Square(Vector2Int coordinate, Vector3 position) : base(coordinate, position) { }

    public static float OuterRadius(float size) { return Mathf.Sqrt(2 * Mathf.Pow(size, 2)); }
    public static float InnerRadius(float size) { return size / 2f; }
    public static float Width(float size) { return size * 2; }
    public static float Height(float size) { return size * 2; }
    public override Vector3[] Corners(float size)
    {
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < corners.Length; i++)
            corners[i] = Corner(i, size);
        return corners;
    }

    protected override Vector3 Corner(int index, float size)
    {
        float angle = 90f * index + 45f;
        Vector3 corner = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0) *
             OuterRadius(size) + position;
        return corner;
    }
    public override GameObject InstatniateTileGameObject(GameObject prefab, Color color)
    {
        gameObject = GameObject.Instantiate(prefab, position, Quaternion.identity);
        gameObject.name = $"Square [{coordinate.x},{coordinate.y}]";
        gameObject.GetComponentInChildren<TextMeshPro>(true).text = $"[{coordinate.x},{coordinate.y}]";
        SetColor(color);
        return gameObject;
    }
}


