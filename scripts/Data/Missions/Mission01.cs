using Godot;
using System.Collections.Generic;

namespace NinjaCowboy;

/// <summary>
/// Static layout data for Mission 01.
/// Returns wall map, door map, and candidate vaccine cells.
/// Modular: add Mission02.cs etc. following the same pattern.
/// </summary>
public static class Mission01
{
    public static MissionData Data() => new MissionData
    {
        MissionName   = "Mission 01",
        ObjectiveText = "Steal the vaccine and return to the boarding points.",
        GridWidth     = 30,
        GridHeight    = 15,
        CowboyCount   = 10,
        NinjaCount    = 4
    };

    /// <summary>true = wall (impassable)</summary>
    public static bool[,] BuildWallMap()
    {
        var w = new bool[30, 15];

        // Outer hull
        for (int x = 0; x < 30; x++) { w[x, 0] = true; w[x, 14] = true; }
        for (int y = 0; y < 15; y++) { w[0, y] = true; w[29, y] = true; }

        // ── Horizontal interior walls ─────────────────────────────────────────
        // Top corridor wall  y=5 : x=1..9   gap at x=5
        for (int x = 1; x <= 9;  x++) if (x != 5)  w[x, 5]  = true;
        // Top corridor wall  y=5 : x=11..18  gap at x=15
        for (int x = 11; x <= 18; x++) if (x != 15) w[x, 5]  = true;
        // Bottom room wall   y=10: x=1..12  gap at x=7
        for (int x = 1; x <= 12; x++) if (x != 7)  w[x, 10] = true;
        // Bottom room wall   y=10: x=14..28 gap at x=21
        for (int x = 14; x <= 28; x++) if (x != 21) w[x, 10] = true;

        // ── Vertical interior walls ───────────────────────────────────────────
        // Central divider left section x=10 y=1..4 and y=6..9
        for (int y = 1; y <= 4; y++)  w[10, y] = true;
        for (int y = 6; y <= 9; y++)  w[10, y] = true;
        // Right section     x=20 y=1..4
        for (int y = 1; y <= 4; y++)  w[20, y] = true;
        // Bottom room split  x=13 y=11..13
        for (int y = 11; y <= 13; y++) w[13, y] = true;

        return w;
    }

    /// <summary>true = door (walkable, rendered as door tile)</summary>
    public static bool[,] BuildDoorMap()
    {
        var d = new bool[30, 15];
        d[5,  5]  = true;
        d[15, 5]  = true;
        d[7,  10] = true;
        d[21, 10] = true;
        return d;
    }

    /// <summary>Valid cells for vaccine placement (inside rooms, not walls or outer ring).</summary>
    public static List<Vector2I> GetVaccineRoomCells(bool[,] walls)
    {
        var candidates = new[]
        {
            new Vector2I(5, 2),  new Vector2I(5, 3),
            new Vector2I(15, 2), new Vector2I(15, 3),
            new Vector2I(5, 8),  new Vector2I(6, 7),
            new Vector2I(25, 7), new Vector2I(25, 8),
            new Vector2I(20,12), new Vector2I(21,11),
            new Vector2I(3, 2),  new Vector2I(17, 7),
        };

        var valid = new List<Vector2I>();
        foreach (var c in candidates)
            if (c.X > 0 && c.X < 29 && c.Y > 0 && c.Y < 14 && !walls[c.X, c.Y])
                valid.Add(c);
        return valid;
    }
}
