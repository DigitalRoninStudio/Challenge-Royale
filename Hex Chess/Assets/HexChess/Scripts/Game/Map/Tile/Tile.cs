using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Tile
{
    public readonly Vector2Int coordinate;
    protected List<Entity> entities;
    public List<Tile> Neighbors => neighbors;
    protected List<Tile> neighbors;
    protected Vector3 position;
    public TilePathData pathData;
    public bool Walkable => IsWalkable();
    public GameObject GameObject => gameObject;
    protected GameObject gameObject;

    public Action<Entity> OnOccupied;

    public Tile(Vector2Int coordinate, Vector3 position)
    {
        this.coordinate = coordinate;
        entities = new List<Entity>();
        neighbors = new List<Tile>();
        this.position = position;
        pathData = new TilePathData();
    }

    public void AddEntity(Entity entity)
    {
        entity.GameObject.transform.position = this.position;
        entities.Add(entity);

        OnOccupied?.Invoke(entity);
    }

    public void RemoveEntity(Entity entity) 
    {
        entities.Remove(entity);
    }

    public T TryToGetEntityOfType<T>() where T : Entity
    {
        foreach (var entity in entities)
            if (entity is T type)
                return type;

        return null;
    }

    private bool IsWalkable()
    {
        foreach(var entity in entities)
            if(entity.isBlockingMovement)
                return false;

        return true;
    }
    public List<Entity> GetEntities() { return entities; }

    public void SetNeighbors(Map map)
    {
        foreach (Vector2Int neigborVector in map.GetNeighborsVectors())
        {
            Tile _neigbor_tile = map.GetTile(coordinate.x + neigborVector.x, coordinate.y + neigborVector.y);
            if (_neigbor_tile != null && !neighbors.Contains(_neigbor_tile))
            {
                neighbors.Add(_neigbor_tile);
                _neigbor_tile.AddNeighbor(this);
            }
        }
    }

    public void AddNeighbor(Tile tile)
    {
        if (!neighbors.Contains(tile))
            neighbors.Add(tile);
    }

    public GameObject GetObject() { return gameObject; }

    public void Draw(float tileSize)
    {
        Vector3[] corners = Corners(tileSize);
        for (int i = 0; i < corners.Length - 1; i++)
            Debug.DrawLine(corners[i], corners[i + 1], Color.red);
        Debug.DrawLine(corners[corners.Length - 1], corners[0], Color.red);
    }
    public Vector3 GetPosition()
    {
        return position;
    }
    public void SetMainColor(Color color)
    {
        gameObject.GetComponent<TileVisual>().Initialize(this, color);
    }
    public void SetColor(Color color)
    {
        gameObject.GetComponent<TileVisual>().SetColor(color);
    }
    public void RefreshColor()
    {
        gameObject.GetComponent<TileVisual>().Refresh();
    }
    public void SetAttack()
    {
        gameObject.GetComponent<TileVisual>().SetAttack();
    }
    public Color GetColor()
    {
        return gameObject.GetComponent<SpriteRenderer>().color;
    }
    public abstract Vector3[] Corners(float tileSize);
    protected abstract Vector3 Corner(int index, float tileSize);
    public abstract GameObject InstatniateTileGameObject(GameObject prefab, Color color);

    public void Dispose()
    {
        if (gameObject != null) GameObject.Destroy(gameObject);

        foreach (var neighbor in neighbors)
        {
            if(neighbor.Neighbors.Contains(this))
                neighbor.Neighbors.Remove(this);
        }             
    }

    public class TilePathData
    {
        public int weight = 1;
        public int cost;
        public Tile prevTile;

        public TilePathData()
        {
            weight = 1;
            cost = 0;
            prevTile = null;
        }

    }
}


