using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapEditorUI : MonoBehaviour
{
    [SerializeField] MapEditor mapEditor;
    [SerializeField] Sprite hex;
    [SerializeField] Sprite square;

    [SerializeField] GameObject container;
    [SerializeField] Image tile;
    [SerializeField] TextMeshProUGUI coordinate;
    [SerializeField] TMP_Dropdown team;
    [SerializeField] TMP_Dropdown figure;
    [SerializeField] TMP_InputField color;

    [SerializeField] Transform topContainer;
    private Toggle ActiveButton =>
          topContainer.GetComponentsInChildren<Toggle>().FirstOrDefault(i => i.isOn && i.group != null);

    private void Start()
    {
        mapEditor.OnSelectTile += SelectTile;

        team.onValueChanged.AddListener((newValue) =>
        {
            mapEditor.SetTileTeam((Team)newValue);
        });

        figure.onValueChanged.AddListener((newValue) =>
        {
            mapEditor.SetTileFigure((FigureType)newValue);
        });

        color.onValueChanged.AddListener((newValue) =>
        {
            mapEditor.CurrentTile.SetColor(ParseHexString(newValue));
        });
    }

    private void SelectTile(Tile tile)
    {
        if (tile == null) return;
        if (tile is Hex)
            this.tile.sprite = hex;
        else
            this.tile.sprite = square;

        coordinate.text = $"[{tile.coordinate.x},{tile.coordinate.y}]";
        team.SetValueWithoutNotify((int)mapEditor.GetTileTeam(tile));
        figure.SetValueWithoutNotify((int)mapEditor.GetTileFigure(tile));
        color.text = ParseColor(tile.GetColor());
    }

    public static Color ParseHexString(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        return Color.white;
    }

    public static string ParseColor(Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGBA(color);
    }

    public void DebugMode(bool value)
    {
        if (mapEditor.Map == null) return;
        if (value)
        {
            foreach (var tile in mapEditor.Map.Tiles)
                tile.GameObject.transform.GetChild(0).gameObject.SetActive(true);
        }else
        {
            foreach (var tile in mapEditor.Map.Tiles)
                tile.GameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void SelectButton(bool value)
    {
        if (!value) return;

        switch (ActiveButton.name)
        {
            case "Select":
                break;
            case "AddTile":
                container.SetActive(false);
                break;
            case "RemoveTile":
                container.SetActive(false);
                break;
        }
    }

    private void Update()
    {
       /* if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Tile tile = mapEditor.Map?.OnHoverMapGetTile(worldPosition);

            switch (ActiveButton.name)
            {
                case "Select":
                    if (tile != null)
                        mapEditor.SetCurrentTile(tile);
                    container.SetActive(true);
                    break;
                case "AddTile":
                    if(tile == null)
                        mapEditor.AddTile(worldPosition);
                    break;
                case "RemoveTile":
                    if (tile != null)
                        mapEditor.RemoveTile(tile);
                    break;
            }
        }*/
    }

    private void OnDisable()
    {
        mapEditor.OnSelectTile -= SelectTile;
        team.onValueChanged.RemoveAllListeners();
        figure.onValueChanged.RemoveAllListeners();
        color.onValueChanged.RemoveAllListeners();
    }
}

public enum MapEditorAction
{ 
    SELECT, ADD, REMOVE
}

