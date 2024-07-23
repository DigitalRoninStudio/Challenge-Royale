using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Tile
{
    public readonly Vector2Int coordinate;
    protected List<Entity> entities;
    protected List<Tile> neighbors;
    protected Vector3 position;
    protected GameObject obj;

    public Tile(Vector2Int coordinate, Vector3 position)
    {
        this.coordinate = coordinate;
        entities = new List<Entity>();
        neighbors = new List<Tile>();
        this.position = position;
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
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

    public bool IsWalkable()
    {
        foreach(var entity in entities)
            if(entity.IsBlockingMovement)
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

    public List<Tile> GetNeighbors() { return neighbors; }

    public GameObject GetObject() { return obj; }

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

    public void SetColor(Color color)
    {
        obj.GetComponent<SpriteRenderer>().color = color;
    }

    public Color GetColor()
    {
        return obj.GetComponent<SpriteRenderer>().color;
    }
    public abstract Vector3[] Corners(float tileSize);
    protected abstract Vector3 Corner(int index, float tileSize);
    public abstract GameObject InstatniateTileGameObject(GameObject prefab, Color color);

    public void Dispose()
    {
        if (obj != null) GameObject.Destroy(obj);

        foreach (var neighbor in neighbors)
        {
            if(neighbor.GetNeighbors().Contains(this))
                neighbor.GetNeighbors().Remove(this);
        }             
    }
}


