using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SelectableTile;

    private Tile tile;
    private Color mainColor = Color.white;

    public void Initialize(Tile tile, Color mainColor)
    {
        this.tile = tile;
        this.mainColor = mainColor;
        SelectableTile.color = mainColor;
    }
    public void OnHover(Color color)
    {
        SelectableTile.color = color;
    }

    public void OnUnhover()
    {
        Refresh();
    }

    public void OnSelect(Color color)
    {
        SelectableTile.color = color;
    }

    public void SetColor(Color color)
    {
        SelectableTile.color = color;
    }

    public void OnUnselect()
    {
        Refresh();
    }

    public void Refresh()
    {
        SelectableTile.color = mainColor;
    }

}
