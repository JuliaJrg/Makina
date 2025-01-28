using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnvironmentUtility
{
    // returns true if unobstructed line of sight to target tile
    public static bool TileIsVisibile(int x, int y, int x2, int y2)
    {
        // bresenham line algorithm
        int w = x2 - x;
        int h = y2 - y;
        int absW = System.Math.Abs(w);
        int absH = System.Math.Abs(h);

        // Is neighbouring tile
        if (absW <= 1 && absH <= 1)
        {
            return true;
        }

        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0)
        {
            dx1 = -1;
            dx2 = -1;
        }
        else if (w > 0)
        {
            dx1 = 1;
            dx2 = 1;
        }
        if (h < 0)
        {
            dy1 = -1;
        }
        else if (h > 0)
        {
            dy1 = 1;
        }

        int longest = absW;
        int shortest = absH;
        if (longest <= shortest)
        {
            longest = absH;
            shortest = absW;
            if (h < 0)
            {
                dy2 = -1;
            }
            else if (h > 0)
            {
                dy2 = 1;
            }
            dx2 = 0;
        }

        int numerator = longest >> 1;
        for (int i = 1; i < longest; i++)
        {
            numerator += shortest;
            if (numerator >= longest)
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }

            if (!Environment.walkable[x, y])
            {
                return false;
            }
        }
        return true;
    }

    // returns coords of tiles from given tile up to and including the target tile (null if path is obstructed)
    public static Coord[] GetPath(int startX, int startY, int endX, int endY)
    {
        // Implémentez A* pour trouver le chemin dans une grille hexagonale
        var path = new List<Coord>();
        var frontier = new PriorityQueue<Coord>();
        var cameFrom = new Dictionary<Coord, Coord>();
        var costSoFar = new Dictionary<Coord, int>();

        var start = new Coord(startX, startY);
        var goal = new Coord(endX, endY);

        frontier.Enqueue(start, 0);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == goal)
            {
                break;
            }

            foreach (var next in GetNeighbours(current))
            {

                int newCost = costSoFar[current] + 1; // assume cost between each tile is 1
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }

            }
        }

        var temp = goal;
        while (temp != start)
        {
            path.Add(temp);
            temp = cameFrom[temp];
        }
        path.Reverse();
        return path.ToArray();
    }

    static IEnumerable<Coord> GetNeighbours(Coord coord)
    {
        yield return new Coord(coord.x + 1, coord.y);
        yield return new Coord(coord.x - 1, coord.y);
        yield return new Coord(coord.x, coord.y + 1);
        yield return new Coord(coord.x, coord.y - 1);
        if (coord.y % 2 == 0)
        {
            yield return new Coord(coord.x - 1, coord.y + 1);
            yield return new Coord(coord.x - 1, coord.y - 1);
        }
        else
        {
            yield return new Coord(coord.x + 1, coord.y + 1);
            yield return new Coord(coord.x + 1, coord.y - 1);
        }
    }

    static int Heuristic(Coord a, Coord b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}

public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T item, int priority)>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}