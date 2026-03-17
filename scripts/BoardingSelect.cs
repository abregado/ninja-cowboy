using Godot;
using System.Collections.Generic;

namespace NinjaCowboy;

/// <summary>
/// Pre-battle scene: player selects 4 outer-edge cells for ninja boarding points.
/// Confirms → stores in GameManager → plays brief boarding anim → loads BattleScene.
/// </summary>
public partial class BoardingSelect : Node2D
{
    private const int GridWidth  = GridManager.GridWidth;
    private const int GridHeight = GridManager.GridHeight;
    private const int TileSize   = GridManager.TileSize;

    private readonly bool[,] _walls = Mission01.BuildWallMap();
    private readonly bool[,] _doors = Mission01.BuildDoorMap();
    private readonly HashSet<Vector2I> _validCells = new();
    private readonly List<Vector2I>    _selected   = new();
    private Label   _statusLabel;
    private Button  _confirmBtn;

    private bool _animating = false;

    public override void _Ready()
    {
        BuildValidCells();
        BuildUI();
    }

    private void BuildValidCells()
    {
        // Valid = inner-edge walkable cells (x==1, x==28, y==1, y==13)
        for (int x = 1; x < GridWidth - 1; x++)
        {
            if (!_walls[x, 1])  _validCells.Add(new Vector2I(x, 1));
            if (!_walls[x, 13]) _validCells.Add(new Vector2I(x, 13));
        }
        for (int y = 2; y < GridHeight - 2; y++)
        {
            if (!_walls[1,  y]) _validCells.Add(new Vector2I(1,  y));
            if (!_walls[28, y]) _validCells.Add(new Vector2I(28, y));
        }
    }

    private void BuildUI()
    {
        // CanvasLayer for UI controls
        var canvas = new CanvasLayer();
        canvas.Layer = 10;
        AddChild(canvas);

        var title = new Label();
        title.Text     = "BOARDING SELECTION";
        title.Position = new Vector2(700, 8);
        title.AddThemeColorOverride("font_color", new Color(0.2f, 0.9f, 1f));
        title.AddThemeFontSizeOverride("font_size", 28);
        canvas.AddChild(title);

        var instr = new Label();
        instr.Text     = "Click 4 outer-edge tiles to place your ninjas.\nGold tiles = boarding points (return here to win).";
        instr.Position = new Vector2(580, 44);
        instr.AddThemeColorOverride("font_color", Colors.White);
        instr.AddThemeFontSizeOverride("font_size", 16);
        canvas.AddChild(instr);

        _statusLabel = new Label();
        _statusLabel.Text     = "Selected: 0 / 4";
        _statusLabel.Position = new Vector2(820, 1020);
        _statusLabel.AddThemeColorOverride("font_color", Colors.White);
        _statusLabel.AddThemeFontSizeOverride("font_size", 18);
        canvas.AddChild(_statusLabel);

        _confirmBtn = new Button();
        _confirmBtn.Text     = "Confirm Boarding";
        _confirmBtn.Size     = new Vector2(200, 44);
        _confirmBtn.Position = new Vector2(1700, 1020);
        _confirmBtn.Disabled = true;
        _confirmBtn.Pressed  += OnConfirm;
        canvas.AddChild(_confirmBtn);

        var backBtn = new Button();
        backBtn.Text     = "Back";
        backBtn.Size     = new Vector2(100, 44);
        backBtn.Position = new Vector2(20, 1020);
        backBtn.Pressed  += () => GameManager.Instance.GoToMainMenu();
        canvas.AddChild(backBtn);
    }

    public override void _Draw()
    {
        DrawBackground();
        DrawGrid();
        DrawSelections();
    }

    private void DrawBackground()
    {
        var starTex = SpriteLoader.Instance.Get(SpriteType.Starfield);
        // Tile starfield
        for (int x = 0; x < 30; x++)
        for (int y = 0; y < 15; y++)
        {
            var rect = GetCellRect(new Vector2I(x, y));
            DrawTextureRect(starTex, rect, false);
        }
    }

    private void DrawGrid()
    {
        var floorTex    = SpriteLoader.Instance.Get(SpriteType.TileFloor);
        var wallTex     = SpriteLoader.Instance.Get(SpriteType.TileWall);
        var doorTex     = SpriteLoader.Instance.Get(SpriteType.TileDoor);
        var boardingTex = SpriteLoader.Instance.Get(SpriteType.TileBoarding);

        for (int x = 0; x < GridWidth; x++)
        for (int y = 0; y < GridHeight; y++)
        {
            var cell = new Vector2I(x, y);
            var rect = GetCellRect(cell);
            ImageTexture tex;

            if (_walls[x, y])         tex = wallTex;
            else if (_doors[x, y])    tex = doorTex;
            else if (_validCells.Contains(cell)) tex = boardingTex;
            else                      tex = floorTex;

            DrawTextureRect(tex, rect, false);
        }
    }

    private void DrawSelections()
    {
        foreach (var cell in _selected)
        {
            var rect = GetCellRect(cell);
            DrawRect(rect, new Color(0f, 1f, 0.3f, 0.6f));
            DrawRect(rect, new Color(0f, 1f, 0.3f), false, 3f);
        }
    }

    private Rect2 GetCellRect(Vector2I cell) =>
        new Rect2(GridManager.GridOrigin + new Vector2(cell.X * TileSize, cell.Y * TileSize),
                  new Vector2(TileSize, TileSize));

    public override void _Input(InputEvent ev)
    {
        if (_animating) return;
        if (ev is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            var cell = WorldToCell(mb.Position);

            if (_validCells.Contains(cell))
            {
                if (_selected.Contains(cell))
                {
                    _selected.Remove(cell);
                }
                else if (_selected.Count < 4)
                {
                    _selected.Add(cell);
                }
                _statusLabel.Text   = $"Selected: {_selected.Count} / 4";
                _confirmBtn.Disabled = _selected.Count < 4;
                QueueRedraw();
            }
        }
    }

    private Vector2I WorldToCell(Vector2 pos)
    {
        var local = pos - GridManager.GridOrigin;
        return new Vector2I((int)Mathf.Floor(local.X / TileSize), (int)Mathf.Floor(local.Y / TileSize));
    }

    private async void OnConfirm()
    {
        _animating = true;
        _confirmBtn.Disabled = true;

        GameManager.Instance.BoardingTiles = _selected.ToArray();

        // Brief boarding animation: flash selected tiles then load battle
        for (int i = 0; i < 4; i++)
        {
            await ToSignal(GetTree().CreateTimer(0.15), "timeout");
            QueueRedraw();
        }
        await ToSignal(GetTree().CreateTimer(0.2), "timeout");
        GameManager.Instance.GoToBattle();
    }
}
