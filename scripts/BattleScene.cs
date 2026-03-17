using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NinjaCowboy;

// ─────────────────────────────────────────────────────────────────────────────
// CursorNode – draws the hover-cell outline and dotted line to selected ninja.
// Lives inside a CanvasLayer so it renders above all Node2D world content.
// ─────────────────────────────────────────────────────────────────────────────
public partial class CursorNode : Node2D
{
    public Vector2I  HoverCell         = Vector2I.Zero;
    public Vector2I? SelectedNinjaCell = null;
    public bool      CanReach          = true;
    public bool      ShowLine          = false;

    private float _flashTimer;
    private bool  _flashOn = true;

    public override void _Process(double delta)
    {
        _flashTimer += (float)delta;
        if (_flashTimer >= 0.35f) { _flashTimer = 0; _flashOn = !_flashOn; QueueRedraw(); }
    }

    public override void _Draw()
    {
        if (!ShowLine) return;

        int ts   = GridManager.TileSize;
        var orig = GridManager.GridOrigin;
        var hcol = CanReach
            ? new Color(0.0f, 1.0f, 0.3f, _flashOn ? 0.85f : 0.40f)
            : new Color(1.0f, 0.2f, 0.2f, _flashOn ? 0.85f : 0.40f);

        // Hover-cell rectangle
        var hPos = orig + new Vector2(HoverCell.X * ts, HoverCell.Y * ts);
        DrawRect(new Rect2(hPos, new Vector2(ts, ts)), hcol, false, 3f);

        // Dotted line from selected ninja to hover cell
        if (SelectedNinjaCell.HasValue)
        {
            float half = ts * 0.5f;
            var from = orig + new Vector2(SelectedNinjaCell.Value.X * ts + half,
                                         SelectedNinjaCell.Value.Y * ts + half);
            var to   = orig + new Vector2(HoverCell.X * ts + half, HoverCell.Y * ts + half);
            DrawDashedLine(from, to, hcol, 2f, 12f);
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// BattleScene – orchestrates grid, units, turns, and player input
// ─────────────────────────────────────────────────────────────────────────────
public partial class BattleScene : Node2D
{
    // ── Children / services ───────────────────────────────────────────────────
    private GridManager     _grid;
    private TurnManager     _turns;
    private HUD             _hud;
    private ActionDialogue  _actionDlg;
    private EndTurnDialogue _endTurnDlg;
    private CursorNode      _cursor;

    // ── Game objects ──────────────────────────────────────────────────────────
    private readonly List<Ninja>  _ninjas  = new();
    private readonly List<Cowboy> _cowboys = new();
    private HashSet<Vector2I>     _boardingTileSet = new();

    private Ninja  _selectedNinja;
    private Cowboy _pendingCowboyTarget;

    private Vector2I _vaccineCell;
    private bool     _vaccineOnGround = true;
    private Sprite2D _vaccineSprite;

    // Cache wall / door maps so _Draw() doesn't rebuild them every frame
    private bool[,] _wallMap;
    private bool[,] _doorMap;

    // ── Input states ──────────────────────────────────────────────────────────
    private enum IS { Idle, NinjaSelected, DialogueOpen, Animating, AITurn }
    private IS _state = IS.Animating;

    // ═════════════════════════════════════════════════════════════════════════
    // _Ready
    // ═════════════════════════════════════════════════════════════════════════
    public override async void _Ready()
    {
        _wallMap = Mission01.BuildWallMap();
        _doorMap = Mission01.BuildDoorMap();

        // ── Grid ──────────────────────────────────────────────────────────────
        _grid = new GridManager();
        _grid.Name = "GridManager";
        _grid.InitGrid(_wallMap, _doorMap);
        AddChild(_grid);

        // ── Turn manager ──────────────────────────────────────────────────────
        _turns = new TurnManager();
        _turns.Name = "TurnManager";
        AddChild(_turns);

        // ── Boarding tiles ────────────────────────────────────────────────────
        foreach (var bt in GameManager.Instance.BoardingTiles)
            _boardingTileSet.Add(bt);

        if (_boardingTileSet.Count == 0) // direct-launch fallback
        {
            var fallbacks = new[] { new Vector2I(1,1), new Vector2I(28,1),
                                    new Vector2I(1,13), new Vector2I(28,13) };
            foreach (var f in fallbacks) _boardingTileSet.Add(f);
            GameManager.Instance.BoardingTiles = _boardingTileSet.ToArray();
        }

        // ── Vaccine ───────────────────────────────────────────────────────────
        SpawnVaccine();

        // ── Units ─────────────────────────────────────────────────────────────
        SpawnNinjas();
        SpawnCowboys();

        // ── HUD ───────────────────────────────────────────────────────────────
        _hud = new HUD();
        _hud.EndTurnRequested += OnEndTurnRequested;
        AddChild(_hud);
        _hud.SetTurnNumber(_turns.TurnNumber);
        _hud.SetPhase(TurnPhase.PlayerTurn);
        _hud.SetEndTurnEnabled(false);

        // ── Dialogues ─────────────────────────────────────────────────────────
        _actionDlg = new ActionDialogue();
        _actionDlg.ActionChosen += OnActionChosen;
        AddChild(_actionDlg);

        _endTurnDlg = new EndTurnDialogue();
        _endTurnDlg.Confirmed += OnEndTurnConfirmed;
        _endTurnDlg.Cancelled += () => _state = IS.Idle;
        AddChild(_endTurnDlg);

        // ── Cursor overlay (CanvasLayer renders above all Node2D content) ─────
        var cursorLayer = new CanvasLayer { Layer = 5 };
        AddChild(cursorLayer);
        _cursor = new CursorNode();
        cursorLayer.AddChild(_cursor);

        // ── Boarding sequence animation ───────────────────────────────────────
        var anim = new BoardingSequence(_boardingTileSet.ToList(), _grid);
        AddChild(anim);
        await anim.PlayAsync();

        // ── Start player turn ─────────────────────────────────────────────────
        _hud.SetEndTurnEnabled(true);
        _state = IS.Idle;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Spawn helpers
    // ═════════════════════════════════════════════════════════════════════════

    private void SpawnVaccine()
    {
        var candidates = Mission01.GetVaccineRoomCells(_wallMap);
        _vaccineCell = candidates.Count > 0
            ? candidates[(int)(GD.Randi() % (uint)candidates.Count)]
            : new Vector2I(14, 7);

        _vaccineSprite = new Sprite2D
        {
            Texture  = SpriteLoader.Instance.Get(SpriteType.Vaccine),
            Position = _grid.CellToWorld(_vaccineCell),
            ZIndex   = 2
        };
        AddChild(_vaccineSprite);
    }

    private void SpawnNinjas()
    {
        var stats       = UnitStats.DefaultNinja();
        var boardList   = _boardingTileSet.ToList();

        for (int i = 0; i < boardList.Count; i++)
        {
            var ninja = new Ninja();
            ninja.ZIndex = 3;
            ninja.Initialize(stats, boardList[i], _grid);
            ninja.VaccineDropped += DropVaccine;
            AddChild(ninja);
            _ninjas.Add(ninja);
        }
    }

    private void SpawnCowboys()
    {
        var stats    = UnitStats.DefaultCowboy();
        var occupied = new HashSet<Vector2I>(_boardingTileSet) { _vaccineCell };

        for (int i = 0; i < 10; i++)
        {
            Vector2I cell;
            int safety = 0;
            do
            {
                int rx = 1 + (int)(GD.Randi() % 28u);
                int ry = 1 + (int)(GD.Randi() % 13u);
                cell = new Vector2I(rx, ry);
                if (++safety > 500) return;
            }
            while (!_grid.IsWalkable(cell) || occupied.Contains(cell));

            var cowboy = new Cowboy();
            cowboy.ZIndex = 3;
            cowboy.Initialize(stats, cell, _grid);
            AddChild(cowboy);
            _cowboys.Add(cowboy);
            occupied.Add(cell);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Tile rendering
    // ═════════════════════════════════════════════════════════════════════════

    public override void _Draw()
    {
        var floorTex    = SpriteLoader.Instance.Get(SpriteType.TileFloor);
        var wallTex     = SpriteLoader.Instance.Get(SpriteType.TileWall);
        var doorTex     = SpriteLoader.Instance.Get(SpriteType.TileDoor);
        var boardingTex = SpriteLoader.Instance.Get(SpriteType.TileBoarding);
        int ts = GridManager.TileSize;

        for (int x = 0; x < GridManager.GridWidth; x++)
        for (int y = 0; y < GridManager.GridHeight; y++)
        {
            var rect = new Rect2(
                GridManager.GridOrigin + new Vector2(x * ts, y * ts),
                new Vector2(ts, ts));

            var cell = new Vector2I(x, y);
            ImageTexture tex;
            if (_wallMap[x, y])                       tex = wallTex;
            else if (_doorMap[x, y])                  tex = doorTex;
            else if (_boardingTileSet.Contains(cell))  tex = boardingTex;
            else                                       tex = floorTex;

            DrawTextureRect(tex, rect, false);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Input
    // ═════════════════════════════════════════════════════════════════════════

    public override void _Input(InputEvent ev)
    {
        if (_state is IS.Animating or IS.AITurn or IS.DialogueOpen) return;

        if (ev is InputEventMouseMotion mm)
        {
            var cell = _grid.WorldToCell(mm.Position);
            if (cell != _cursor.HoverCell)
            {
                _cursor.HoverCell = cell;
                if (_state == IS.NinjaSelected && _selectedNinja != null)
                    _cursor.CanReach = CanNinjaReachCell(_selectedNinja, cell);
                _cursor.QueueRedraw();
            }
        }

        if (ev is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            HandleCellClick(_grid.WorldToCell(mb.Position));
        }
    }

    private void HandleCellClick(Vector2I cell)
    {
        if (!_grid.IsInBounds(cell)) return;

        var clickedNinja  = _ninjas.FirstOrDefault(n => n.IsAlive && n.GridCell == cell);
        var clickedCowboy = _cowboys.FirstOrDefault(c => c.IsAlive && c.GridCell == cell);

        if (clickedNinja != null)  { HandleNinjaClick(clickedNinja);   return; }
        if (clickedCowboy != null && _state == IS.NinjaSelected) { HandleAttackClick(clickedCowboy); return; }
        if (_state == IS.NinjaSelected) HandleMoveClick(cell);
    }

    // ── Ninja selection ───────────────────────────────────────────────────────

    private void HandleNinjaClick(Ninja ninja)
    {
        if (_state == IS.NinjaSelected && _selectedNinja == ninja)
        {
            // Re-click same ninja → action dialogue
            if (!ninja.HasActedThisTurn) ShowNinjaActionDialogue(ninja);
            return;
        }
        if (ninja.HasActedThisTurn) return;

        _selectedNinja = ninja;
        _state = IS.NinjaSelected;
        _cursor.SelectedNinjaCell = ninja.GridCell;
        _cursor.ShowLine  = true;
        _cursor.CanReach  = true;
        _cursor.QueueRedraw();
    }

    // ── Attack ────────────────────────────────────────────────────────────────

    private void HandleAttackClick(Cowboy cowboy)
    {
        var ninja = _selectedNinja;
        if (ninja == null || ninja.HasActedThisTurn) return;

        int  dist = _grid.ChebyshevDistance(ninja.GridCell, cowboy.GridCell);
        bool los  = _grid.GetLOS(ninja.GridCell, cowboy.GridCell);
        if (!los) return;

        var options = new List<string>();
        if (dist <= 3 && ninja.Shuriken > 0) options.Add($"SHURIKEN ({ninja.Stats.ShurikenAPCost}AP)");
        if (dist <= 3)                        options.Add($"KATANA ({ninja.Stats.KatanaAPCostBase}+AP)");
        if (dist > 3  && ninja.Shuriken > 0)  options.Add($"SHURIKEN ({ninja.Stats.ShurikenAPCost}AP)");
        options.Add("CANCEL");

        if (options.Count <= 1) return;

        _pendingCowboyTarget = cowboy;
        _state = IS.DialogueOpen;
        _actionDlg.Show(_grid.CellToWorld(ninja.GridCell), options);
    }

    // ── Move ──────────────────────────────────────────────────────────────────

    private void HandleMoveClick(Vector2I cell)
    {
        var ninja = _selectedNinja;
        if (ninja == null || ninja.HasActedThisTurn || !_grid.IsWalkable(cell)) return;

        var path = _grid.FindPath(ninja.GridCell, cell, GetOccupiedCells(ninja));
        if (path == null || path.Count == 0 || ninja.AP < path.Count) return;

        _ = ExecuteNinjaMove(ninja, path);
    }

    private bool CanNinjaReachCell(Ninja ninja, Vector2I cell)
    {
        if (!_grid.IsWalkable(cell)) return false;
        // Approximation: Chebyshev distance ≤ AP remaining
        return _grid.ChebyshevDistance(ninja.GridCell, cell) <= ninja.AP;
    }

    // ── Ninja action dialogue ─────────────────────────────────────────────────

    private void ShowNinjaActionDialogue(Ninja ninja)
    {
        var options = new List<string>();
        if (ninja.AP >= ninja.Stats.AmbushAPCost)  options.Add($"AMBUSH ({ninja.Stats.AmbushAPCost}AP)");
        if (ninja.AP >= ninja.Stats.ConcealAPCost) options.Add($"CONCEAL ({ninja.Stats.ConcealAPCost}AP)");
        options.Add("CANCEL");

        _pendingCowboyTarget = null;
        _state = IS.DialogueOpen;
        _actionDlg.Show(_grid.CellToWorld(ninja.GridCell), options);
    }

    private async void OnActionChosen(string action)
    {
        if (action.StartsWith("CANCEL")) { _state = IS.NinjaSelected; return; }

        var ninja = _selectedNinja;
        if (ninja == null) { _state = IS.Idle; return; }

        _state = IS.Animating;

        if (action.StartsWith("AMBUSH"))
        {
            ninja.UseAP(ninja.Stats.AmbushAPCost);
            ninja.SetAmbush();
            DeselectNinja();
        }
        else if (action.StartsWith("CONCEAL"))
        {
            ninja.UseAP(ninja.Stats.ConcealAPCost);
            ninja.SetConceal();
            DeselectNinja();
        }
        else if (action.StartsWith("SHURIKEN"))
        {
            await ExecuteShurikenAttack(ninja, _pendingCowboyTarget);
        }
        else if (action.StartsWith("KATANA"))
        {
            await ExecuteKatanaAttack(ninja, _pendingCowboyTarget);
        }

        _state = IS.Idle;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Ninja attacks
    // ═════════════════════════════════════════════════════════════════════════

    private async Task ExecuteShurikenAttack(Ninja ninja, Cowboy cowboy)
    {
        if (cowboy == null || !cowboy.IsAlive) return;
        int dist = _grid.ChebyshevDistance(ninja.GridCell, cowboy.GridCell);
        if (!ninja.UseAP(ninja.Stats.ShurikenAPCost)) return;
        ninja.Shuriken--;
        ninja.HasActedThisTurn = true;

        await TweenProjectile(_grid.CellToWorld(ninja.GridCell),
                              _grid.CellToWorld(cowboy.GridCell), 0.28f);

        var (hit, dmg) = CombatSystem.ShurikenAttack(ninja.Stats, dist);
        if (hit && cowboy.IsAlive) cowboy.TakeDamage(dmg);
        await FlashUnit(cowboy, hit);
        ninja.UpdateStatsLabel();
        DeselectNinja();
    }

    private async Task ExecuteKatanaAttack(Ninja ninja, Cowboy cowboy)
    {
        if (cowboy == null || !cowboy.IsAlive) return;

        var adjCell  = FindAdjacentCell(cowboy.GridCell, ninja.GridCell);
        int moveCost = _grid.ChebyshevDistance(ninja.GridCell, adjCell);
        int totalAP  = ninja.Stats.KatanaAPCostBase + moveCost;
        if (!ninja.UseAP(totalAP)) return;
        ninja.HasActedThisTurn = true;

        await ninja.TweenToCell(adjCell);
        _cursor.SelectedNinjaCell = ninja.GridCell;

        var (hit, dmg) = CombatSystem.KatanaAttack(ninja.Stats);
        if (hit && cowboy.IsAlive) cowboy.TakeDamage(dmg);
        await FlashUnit(cowboy, hit);
        ninja.UpdateStatsLabel();
        DeselectNinja();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Ninja movement
    // ═════════════════════════════════════════════════════════════════════════

    private async Task ExecuteNinjaMove(Ninja ninja, List<Vector2I> path)
    {
        _state = IS.Animating;
        ninja.UseAP(path.Count);

        foreach (var cell in path)
            await ninja.TweenToCell(cell, 0.12f);

        _cursor.SelectedNinjaCell = ninja.GridCell;
        _cursor.QueueRedraw();

        // Vaccine auto-pickup
        if (_vaccineOnGround && ninja.GridCell == _vaccineCell)
        {
            ninja.PickupVaccine();
            _vaccineOnGround    = false;
            _vaccineSprite.Visible = false;
        }

        // Win condition
        if (ninja.HasVaccine && _boardingTileSet.Contains(ninja.GridCell))
        {
            await ToSignal(GetTree().CreateTimer(0.5), "timeout");
            GameManager.Instance.GoToVictory();
            return;
        }

        _state = IS.NinjaSelected;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // End turn
    // ═════════════════════════════════════════════════════════════════════════

    private void OnEndTurnRequested()
    {
        if (_state != IS.Idle && _state != IS.NinjaSelected) return;

        var warnings = _ninjas
            .Where(n => n.IsAlive && !n.HasActedThisTurn && n.AP > 0 && !n.IsAmbush && !n.IsConceal)
            .Select(n => $"Ninja at ({n.GridCell.X},{n.GridCell.Y}) has {n.AP}AP unused")
            .ToList();

        _state = IS.DialogueOpen;
        _endTurnDlg.Show(warnings);
    }

    private async void OnEndTurnConfirmed()
    {
        DeselectNinja();
        _state = IS.AITurn;
        _hud.SetEndTurnEnabled(false);
        _hud.SetPhase(TurnPhase.AITurn);

        // Restore cowboy AP at start of their turn
        foreach (var c in _cowboys.Where(c => c.IsAlive)) c.RestoreAP();

        await RunAITurn();

        if (!IsInsideTree()) return; // scene may have changed (game over)

        // Player turn begins
        foreach (var n in _ninjas.Where(n => n.IsAlive)) n.NewTurn();
        _turns.EndAITurn();
        _hud.SetTurnNumber(_turns.TurnNumber);
        _hud.SetPhase(TurnPhase.PlayerTurn);
        _hud.SetEndTurnEnabled(true);
        _state = IS.Idle;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // AI turn
    // ═════════════════════════════════════════════════════════════════════════

    private async Task RunAITurn()
    {
        foreach (var cowboy in _cowboys.ToList())
        {
            if (!cowboy.IsAlive || !IsInsideTree()) continue;
            await ToSignal(GetTree().CreateTimer(0.12), "timeout");
            await RunCowboyTurn(cowboy);

            if (!_ninjas.Any(n => n.IsAlive))
            {
                await ToSignal(GetTree().CreateTimer(0.5), "timeout");
                GameManager.Instance.GoToGameOver();
                return;
            }
        }
    }

    private async Task RunCowboyTurn(Cowboy cowboy)
    {
        // Shoot first if can see a ninja
        var target = FindVisibleNinja(cowboy);
        if (target != null && cowboy.AP >= cowboy.Stats.GunAPCost)
        {
            cowboy.AIState            = CowboyState.Alerted;
            cowboy.LastKnownNinjaPos  = target.GridCell;
            await ShootCowboy(cowboy, target);
            return;
        }

        // Move
        if (cowboy.AP > 0)
        {
            var dest = GetCowboyMoveTarget(cowboy);
            if (dest.HasValue) await MoveCowboyToward(cowboy, dest.Value);
        }
    }

    private async Task MoveCowboyToward(Cowboy cowboy, Vector2I target)
    {
        var blocked = GetOccupiedCells(cowboy);
        var path    = _grid.FindPath(cowboy.GridCell, target, blocked)
                   ?? GetRandomAdjacentPath(cowboy);
        if (path == null) return;

        foreach (var cell in path)
        {
            if (cowboy.AP <= 0 || !cowboy.IsAlive || !IsInsideTree()) break;

            cowboy.UseAP(1);
            await cowboy.TweenToCell(cell);
            cowboy.UpdateStatsLabel();

            if (await CheckAmbushTrigger(cowboy)) return;
            if (await CheckConcealReveal(cowboy))  return;

            var spotted = FindVisibleNinja(cowboy);
            if (spotted != null)
            {
                cowboy.AIState           = CowboyState.Alerted;
                cowboy.LastKnownNinjaPos = spotted.GridCell;
                if (cowboy.AP >= cowboy.Stats.GunAPCost)
                    await ShootCowboy(cowboy, spotted);
                return;
            }
        }
    }

    private async Task<bool> CheckAmbushTrigger(Cowboy cowboy)
    {
        foreach (var ninja in _ninjas.ToList())
        {
            if (!ninja.IsAlive || !ninja.IsAmbush) continue;
            if (_grid.ChebyshevDistance(cowboy.GridCell, ninja.GridCell) > 4) continue;
            if (!_grid.GetLOS(cowboy.GridCell, ninja.GridCell)) continue;

            ninja.IsAmbush       = false;
            ninja.HasActedThisTurn = true;

            var adj = FindAdjacentCell(cowboy.GridCell, ninja.GridCell);
            await ninja.TweenToCell(adj);

            var (hit, dmg) = CombatSystem.KatanaAttack(ninja.Stats);
            if (hit && cowboy.IsAlive) cowboy.TakeDamage(dmg);
            await FlashUnit(cowboy, hit);
            ninja.UpdateStatsLabel();

            if (!cowboy.IsAlive) return true;
        }
        return false;
    }

    private async Task<bool> CheckConcealReveal(Cowboy cowboy)
    {
        foreach (var ninja in _ninjas.ToList())
        {
            if (!ninja.IsAlive || !ninja.IsConceal) continue;
            if (!_grid.GetLOS(cowboy.GridCell, ninja.GridCell)) continue;
            if (GD.Randf() < ninja.Stats.ConcealMissChance) continue;

            // Conceal broken
            ninja.IsConceal = false;
            ninja.UpdateStatsLabel();
            cowboy.AIState           = CowboyState.Alerted;
            cowboy.LastKnownNinjaPos = ninja.GridCell;

            if (cowboy.AP >= cowboy.Stats.GunAPCost)
                await ShootCowboy(cowboy, ninja);
            return true;
        }
        return false;
    }

    private async Task ShootCowboy(Cowboy cowboy, Ninja target)
    {
        if (!target.IsAlive) return;
        cowboy.UseAP(cowboy.Stats.GunAPCost);

        await TweenProjectile(_grid.CellToWorld(cowboy.GridCell),
                              _grid.CellToWorld(target.GridCell), 0.22f);

        int dist = _grid.ChebyshevDistance(cowboy.GridCell, target.GridCell);
        var (hit, dmg) = CombatSystem.GunAttack(cowboy.Stats, dist);
        if (hit && target.IsAlive) target.TakeDamage(dmg);
        await FlashUnit(target, hit);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Helpers
    // ═════════════════════════════════════════════════════════════════════════

    private Ninja FindVisibleNinja(Cowboy cowboy)
    {
        Ninja best = null; int bestDist = int.MaxValue;
        foreach (var n in _ninjas.Where(n => n.IsAlive && !n.IsConceal))
        {
            if (!_grid.GetLOS(cowboy.GridCell, n.GridCell)) continue;
            int d = _grid.ChebyshevDistance(cowboy.GridCell, n.GridCell);
            if (d < bestDist) { bestDist = d; best = n; }
        }
        return best;
    }

    private Vector2I? GetCowboyMoveTarget(Cowboy cowboy)
    {
        if (cowboy.AIState == CowboyState.Alerted &&
            cowboy.LastKnownNinjaPos != Vector2I.Zero &&
            cowboy.LastKnownNinjaPos != cowboy.GridCell)
            return cowboy.LastKnownNinjaPos;

        // Patrol: random walkable cell
        for (int i = 0; i < 40; i++)
        {
            var cell = new Vector2I(1 + (int)(GD.Randi() % 28u), 1 + (int)(GD.Randi() % 13u));
            if (_grid.IsWalkable(cell) && cell != cowboy.GridCell) return cell;
        }
        return null;
    }

    private HashSet<Vector2I> GetOccupiedCells(Unit exclude = null)
    {
        var set = new HashSet<Vector2I>();
        foreach (var n in _ninjas)  if (n.IsAlive && n != exclude) set.Add(n.GridCell);
        foreach (var c in _cowboys) if (c.IsAlive && c != exclude) set.Add(c.GridCell);
        return set;
    }

    private bool IsOccupied(Vector2I cell)
    {
        foreach (var n in _ninjas)  if (n.IsAlive && n.GridCell == cell) return true;
        foreach (var c in _cowboys) if (c.IsAlive && c.GridCell == cell) return true;
        return false;
    }

    private Vector2I FindAdjacentCell(Vector2I target, Vector2I preferredFrom)
    {
        Vector2I[] dirs = { new(0,1), new(0,-1), new(1,0), new(-1,0),
                            new(1,1), new(-1,1), new(1,-1), new(-1,-1) };
        Vector2I best  = target + dirs[0];
        float bestDist = float.MaxValue;
        foreach (var dir in dirs)
        {
            var cell = target + dir;
            if (!_grid.IsWalkable(cell) || IsOccupied(cell)) continue;
            float d = ((Vector2)(cell - preferredFrom)).Length();
            if (d < bestDist) { bestDist = d; best = cell; }
        }
        return best;
    }

    private List<Vector2I> GetRandomAdjacentPath(Cowboy cowboy)
    {
        var dirs = new[] { new Vector2I(1,0), new Vector2I(-1,0),
                           new Vector2I(0,1), new Vector2I(0,-1) };
        foreach (var dir in dirs)
        {
            var cell = cowboy.GridCell + dir;
            if (_grid.IsWalkable(cell) && !IsOccupied(cell))
                return new List<Vector2I> { cell };
        }
        return null;
    }

    private void DeselectNinja()
    {
        _selectedNinja        = null;
        _cursor.ShowLine      = false;
        _cursor.SelectedNinjaCell = null;
        _cursor.QueueRedraw();
    }

    private void DropVaccine(Vector2I cell)
    {
        _vaccineOnGround        = true;
        _vaccineCell            = cell;
        _vaccineSprite.Position = _grid.CellToWorld(cell);
        _vaccineSprite.Visible  = true;
        _vaccineSprite.ZIndex   = 4; // show above dead unit
    }

    // ── Visual effects ────────────────────────────────────────────────────────

    private async Task TweenProjectile(Vector2 from, Vector2 to, float duration)
    {
        var dot = new ColorRect
        {
            Color    = new Color(1f, 0.9f, 0.2f),
            Size     = new Vector2(10, 10),
            Position = from - new Vector2(5, 5),
            ZIndex   = 10
        };
        AddChild(dot);
        var t = CreateTween();
        t.TweenProperty(dot, "position", to - new Vector2(5, 5), duration);
        await ToSignal(t, "finished");
        dot.QueueFree();
    }

    private async Task FlashUnit(Unit unit, bool hit)
    {
        if (!IsInstanceValid(unit) || !unit.IsAlive) return;
        unit.Modulate = hit ? new Color(1f, 0.2f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
        await ToSignal(GetTree().CreateTimer(0.25), "timeout");
        if (IsInstanceValid(unit)) unit.Modulate = Colors.White;
    }
}
