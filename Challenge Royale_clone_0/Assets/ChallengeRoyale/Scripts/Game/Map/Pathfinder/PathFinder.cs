using System;
using System.Collections.Generic;

public class PathFinder
{
    public static Queue<Tile> FindPath_BFS(Tile start, Tile end)
    {
        HashSet<Tile> visited = new HashSet<Tile>();
        visited.Add(start);

        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(start);

        start.pathData.prevTile = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();

            if (current == end)
            {
                break;
            }

            foreach (var neighbor in current.Neighbors)
            {
                if (neighbor.Walkable)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        frontier.Enqueue(neighbor);

                        neighbor.pathData.prevTile = current;
                    }
                }
            }
        }

        Queue<Tile> path = BacktrackToPath(end);

        return path;
    }
    public static Queue<Tile> FindPath_Dijkstra(Tile start, Tile end, Map map)
    {
        foreach (Tile tile in map.Tiles)
        {
            tile.pathData.cost = int.MaxValue;
        }

        start.pathData.cost = 0;

        HashSet<Tile> visited = new HashSet<Tile>();
        visited.Add(start);

        MinHeap<Tile> frontier = new MinHeap<Tile>((lhs, rhs) => lhs.pathData.cost.CompareTo(rhs.pathData.cost));
        frontier.Add(start);

        start.pathData = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Remove();

            if (current == end)
            {
                break;
            }

            foreach (var neighbor in current.Neighbors)
            {
                if (neighbor.Walkable)
                {
                    int newNeighborCost = current.pathData.cost + neighbor.pathData.weight;
                    if (newNeighborCost < neighbor.pathData.cost)
                    {
                        neighbor.pathData.cost = newNeighborCost;
                        neighbor.pathData.prevTile = current;
                    }

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        frontier.Add(neighbor);
                    }
                }

            }
        }

        Queue<Tile> path = BacktrackToPath(end);

        return path;
    }
    public static Queue<Tile> FindPath_AStar(Tile start, Tile end, Map map)
    {
        foreach (Tile tile in map.Tiles)
        {
            tile.pathData.cost = int.MaxValue;
        }

        start.pathData.cost = 0;
        Comparison<Tile> heuristicComparison = (lhs, rhs) =>
        {
            float lhsCost = lhs.pathData.cost + GetEuclideanHeuristicCost(lhs, end);
            float rhsCost = rhs.pathData.cost + GetEuclideanHeuristicCost(rhs, end);

            return lhsCost.CompareTo(rhsCost);
        };

        MinHeap<Tile> frontier = new MinHeap<Tile>(heuristicComparison);
        frontier.Add(start);

        HashSet<Tile> visited = new HashSet<Tile>();
        visited.Add(start);

        start.pathData.prevTile = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Remove();

            if (current == end)
            {
                break;
            }

            foreach (var neighbor in current.Neighbors)
            {
                if (neighbor.Walkable)
                {
                    int newNeighborCost = current.pathData.cost + neighbor.pathData.weight;
                    if (newNeighborCost < neighbor.pathData.cost)
                    {
                        neighbor.pathData.cost = newNeighborCost;
                        neighbor.pathData.prevTile = current;
                    }

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        frontier.Add(neighbor);
                    }
                }
            }
        }

        Queue<Tile> path = BacktrackToPath(end);

        return path;
    }
    public static List<Tile> BFS_RangeMovement(Tile start, int range, Map map)
    {
        List<Tile> tilesInRange = new List<Tile>();

        foreach (Tile tile in map.Tiles)
        {
            tile.pathData.cost = int.MaxValue;
        }

        start.pathData.cost = 0;

        HashSet<Tile> visited = new HashSet<Tile>();
        visited.Add(start);

        Queue<Tile> frontier = new Queue<Tile>();
        frontier.Enqueue(start);

        start.pathData.prevTile = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();

            foreach (var neighbor in current.Neighbors)
            {
                if (neighbor.Walkable)
                {
                    int newNeighborCost = current.pathData.cost + neighbor.pathData.weight;

                    if (newNeighborCost < neighbor.pathData.cost)
                    {
                        neighbor.pathData.cost = newNeighborCost;
                        neighbor.pathData.prevTile = current;
                    }


                    if (!visited.Contains(neighbor))
                    {
                        if (range - newNeighborCost >= 0)
                        {
                            tilesInRange.Add(neighbor);
                        }
                        else
                        {
                            return tilesInRange;
                        }
                        visited.Add(neighbor);
                        frontier.Enqueue(neighbor);
                    }
                }
            }
        }
        return tilesInRange;
    }
    private static float GetEuclideanHeuristicCost(Tile current, Tile end)
    {
        float heuristicCost = (current.GetPosition() - end.GetPosition()).magnitude;
        return heuristicCost;
    }
    private static Queue<Tile> BacktrackToPath(Tile end)
    {
        Tile current = end;
        List<Tile> path = new List<Tile>();

        while (current != null)
        {
            path.Add(current);
            current = current.pathData.prevTile;
        }

        path.Reverse();

        return new Queue<Tile>(path);
    }
}



