using UnityEngine;

public class TileVisual : MonoBehaviour
{
    private Tile tile;
    private Color mainColor = Color.white;
    private Renderer renderer;
    public void Initialize(Tile tile, Color mainColor)
    {
        this.tile = tile;
        this.mainColor = mainColor;

        renderer = GetComponent<Renderer>();
        renderer.material.color = mainColor;
    }
    public void OnHover(Color color)
    {
        renderer.material.color = color;
    }

    public void OnUnhover()
    {
        Refresh();
    }

    public void OnSelect(Color color)
    {
        renderer.material.color = color;
    }

    public void SetColor(Color color)
    {
        renderer.material.color = color;
    }

    public void OnUnselect()
    {
        Refresh();
    }

    public void Refresh()
    {
        renderer.material.color = mainColor;
    }
}
