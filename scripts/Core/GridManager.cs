using Godot;
using System;
using System.Collections.Generic;

namespace NinjaCowboy;

/// <summary>
/// Battle-scene node. Owns the 30×15 wall/door map, A* pathfinding, and Bresenham LOS.
/// </summary>
public partial class GridManager : Node
{
    public const int GridWidth  = 30;
    public const int GridHeight = 15;
    public const int TileSize   = 64;

    // Top-left pixel of cell (0,0). Grid is 1920×960; centre vertically in 1080p.
    public static readonly Vector2 GridOrigin = new(0f, 60f);

    private bool[,] _wallMap = new bool[GridWidth, GridHeight];
    private bool[,] _doorMap = new bool[GridWidth, GridHeight];

    public void InitGrid(bool[,] wallMap, bool[,] doorMap)
    {
        _wallMap = wallMap;
        _doorMap = doorMap;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public bool IsInBounds(Vector2I c) =>
        c.X >= 0 && c.X < GridWidth && c.Y >= 0 && c.Y < GridHeight;

    public bool IsWall(Vector2I c) => !IsInBounds(c) || _wallMap[c.X, c.Y];
    public bool IsDoor(Vector2I c) => IsInBounds(c) && _doorMap[c.X, c.Y];
    public bool IsWalkable(Vector2I c) => IsInBounds(c) && !_wallMap[c.X, c.Y];

    // World position = centre of cell
    public Vector2 CellToWorld(Vector2I c) =>
        GridOrigin + new Vector2(c.X * TileSize + TileSize * 0.5f, c.Y * TileSize + TileSize * 0.5f);

    public Vector2I WorldToCell(Vector2 world)
    {
        var local = world - GridOrigin;
        return new Vector2I((int)Math.Floor(local.X / TileSize), (int)Math.Floor(local.Y / TileSize));
    }

    public int ChebyshevDistance(Vector2I a, Vector2I b) =>
        Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

    // ── A* Pathfinding ────────────────────────────────────────────────────────

    private static readonly Vector2I[] Dirs =
    {
        new(-1,-1), new(0,-1), new(1,-1),
        new(-1, 0),            new(1, 0),
        new(-1, 1), new(0, 1), new(1, 1)
    };

    /// <summary>
    /// Returns path from 'from' to 'to' (excluding 'from', including 'to').
    /// blockedEnds: cells occupied by units (passable but cannot END on them).
    /// Returns null if no path.
    /// </summary>
    public List<Vector2I> FindPath(Vector2I from, Vector2I to, HashSet<Vector2I> blockedEnds = null)
    {
        if (!IsWalkable(to)) return null;
        if (blockedEnds != null && blockedEnds.Contains(to)) return null;

        var g = new Dictionary<Vector2I, float> { [from] = 0f };
        var parent = new Dictionary<Vector2I, Vector2I>();
        var closed = new HashSet<Vector2I>();

        // Simple priority queue via sorted list
        var open = new List<(float f, Vector2I cell)> { (Heuristic(from, to), from) };

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.f.CompareTo(b.f));
            var (_, cur) = open[0];
            open.RemoveAt(0);

            if (cur == to) return Reconstruct(parent, from, to);
            if (closed.Contains(cur)) continue;
            closed.Add(cur);

            foreach (var dir in Dirs)
            {
                var nb = cur + dir;
                if (closed.Contains(nb)) continue;
                if (!IsWalkable(nb)) continue;
                if (blockedEnds != null && blockedEnds.Contains(nb) && nb != to) continue;

                float cost = (dir.X != 0 && dir.Y != 0) ? 1.414f : 1f;
                float ng = g[cur] + cost;

                if (!g.TryGetValue(nb, out float existingG) || ng < existingG)
                {
                    g[nb] = ng;
                    parent[nb] = cur;
                    open.Add((ng + Heuristic(nb, to), nb));
                }
            }
        }
        return null;
    }

    private static float Heuristic(Vector2I a, Vector2I b) =>
        Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

    private static List<Vector2I> Reconstruct(Dictionary<Vector2I, Vector2I> parent, Vector2I from, Vector2I to)
    {
        var path = new List<Vector2I>();
        var cur = to;
        while (cur != from) { path.Add(cur); cur = parent[cur]; }
        path.Reverse();
        return path;
    }

    // ── Bresenham Line-Of-Sight ───────────────────────────────────────────────

    /// <summary>Returns true if no wall blocks the straight line between 'from' and 'to'.</summary>
    public bool GetLOS(Vector2I from, Vector2I to)
    {
        int x0 = from.X, y0 = from.Y, x1 = to.X, y1 = to.Y;
        int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 == x1 && y0 == y1) return true;
            // Block on wall (not start cell)
            if (!(x0 == from.X && y0 == from.Y) && IsInBounds(new Vector2I(x0, y0)) && _wallMap[x0, y0])
                return false;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 <  dx) { err += dx; y0 += sy; }
        }
    }
}
