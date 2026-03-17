using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NinjaCowboy;

/// <summary>
/// Plays the boarding cutscene at the start of BattleScene:
///  1. Two spaceships slide into view.
///  2. Four ninja sprites tween to boarding tiles.
///  3. Explosion flash on each entry tile.
/// Frees itself when done.
/// </summary>
public partial class BoardingSequence : CanvasLayer
{
    private readonly List<Vector2I> _boardingTiles;
    private readonly GridManager    _grid;

    public BoardingSequence(List<Vector2I> boardingTiles, GridManager grid)
    {
        _boardingTiles = boardingTiles;
        _grid          = grid;
        Layer          = 30;
    }

    public async Task PlayAsync()
    {
        // Node2D container owns all visuals — CanvasLayer has no modulate property
        var root = new Node2D();
        AddChild(root);

        // ── 1. Two ships slide in ─────────────────────────────────────────────
        var cowboyShip = MakeShip(new Color(0.05f, 0.09f, 0.15f), new Vector2(1380, 440));
        var ninjaShip  = MakeShip(new Color(0.07f, 0.04f, 0.10f), new Vector2(-300, 440));
        root.AddChild(cowboyShip);
        root.AddChild(ninjaShip);

        var shipTween = CreateTween();
        shipTween.TweenProperty(ninjaShip, "position", new Vector2(260, 440), 1.0f);
        await ToSignal(shipTween, "finished");
        await ToSignal(GetTree().CreateTimer(0.3), "timeout");

        // ── 2. Ninjas jump one by one ─────────────────────────────────────────
        var ninjaTex = SpriteLoader.Instance.Get(SpriteType.Ninja);

        for (int i = 0; i < _boardingTiles.Count; i++)
        {
            var ns = new Sprite2D
            {
                Texture  = ninjaTex,
                Position = new Vector2(300, 500 + i * 30)
            };
            root.AddChild(ns);

            var targetPos = _grid.CellToWorld(_boardingTiles[i]);
            var jt = CreateTween();
            jt.TweenProperty(ns, "position", targetPos, 0.40f);
            await ToSignal(jt, "finished");

            await Flash(_boardingTiles[i], root);
            ns.Visible = false; // ninja placed by BattleScene

            await ToSignal(GetTree().CreateTimer(0.08), "timeout");
        }

        await ToSignal(GetTree().CreateTimer(0.35), "timeout");

        // ── 3. Fade out ───────────────────────────────────────────────────────
        var fade = CreateTween();
        fade.TweenProperty(root, "modulate:a", 0f, 0.4f);
        await ToSignal(fade, "finished");

        QueueFree();
    }

    private ColorRect MakeShip(Color color, Vector2 position)
    {
        return new ColorRect
        {
            Color    = color,
            Size     = new Vector2(280, 180),
            Position = position
        };
    }

    private async Task Flash(Vector2I cell, Node2D parent)
    {
        int ts = GridManager.TileSize;
        var worldPos = _grid.CellToWorld(cell);
        var flash = new ColorRect
        {
            Color    = new Color(1f, 0.55f, 0.1f, 0.92f),
            Size     = new Vector2(ts, ts),
            Position = worldPos - new Vector2(ts * 0.5f, ts * 0.5f)
        };
        parent.AddChild(flash);

        var ft = CreateTween();
        ft.TweenProperty(flash, "modulate:a", 0f, 0.4f);
        await ToSignal(ft, "finished");
        flash.QueueFree();
    }
}
