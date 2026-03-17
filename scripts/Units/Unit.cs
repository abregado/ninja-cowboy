using Godot;
using System;

namespace NinjaCowboy;

public enum Faction { Ninja, Cowboy }

/// <summary>
/// Base unit: handles HP/AP, sprite, stats label, tween movement, and vaccine carrying.
/// Call Initialize(stats, cell, grid) BEFORE adding to the scene tree.
/// </summary>
public partial class Unit : Node2D
{
    // ── Data ──────────────────────────────────────────────────────────────────
    public UnitStats Stats    { get; protected set; }
    public Faction   Faction  { get; set; }

    public int HP    { get; protected set; }
    public int MaxHP { get; protected set; }
    public int AP    { get; protected set; }
    public int MaxAP { get; protected set; }

    public Vector2I GridCell  { get; set; }
    public bool     IsAlive   => HP > 0;
    public bool     HasVaccine{ get; set; }

    // ── Events ────────────────────────────────────────────────────────────────
    public event Action<Vector2I> VaccineDropped;
    public event Action<Unit>     UnitDied;

    // ── Visual children ───────────────────────────────────────────────────────
    protected Sprite2D _sprite;
    protected Label    _statsLabel;
    protected Sprite2D _vaccineIcon;

    protected GridManager _grid;

    // ── Init ──────────────────────────────────────────────────────────────────

    public virtual void Initialize(UnitStats stats, Vector2I cell, GridManager grid)
    {
        Stats   = stats;
        GridCell = cell;
        _grid   = grid;

        MaxHP = stats.MaxHP;
        HP    = MaxHP;
        MaxAP = stats.MaxAP;
        AP    = MaxAP;

        BuildVisuals();
        Position = grid.CellToWorld(cell);  // absolute position set directly; no scene tree needed yet
    }

    protected virtual void BuildVisuals()
    {
        _sprite = new Sprite2D();
        _sprite.Texture = GetLiveTexture();
        AddChild(_sprite);

        _statsLabel = new Label();
        _statsLabel.Position = new Vector2(36f, -40f);
        _statsLabel.AddThemeColorOverride("font_color", Colors.White);
        _statsLabel.AddThemeFontSizeOverride("font_size", 11);
        _statsLabel.ZIndex = 5;
        AddChild(_statsLabel);

        _vaccineIcon = new Sprite2D();
        _vaccineIcon.Texture = SpriteLoader.Instance.Get(SpriteType.Vaccine);
        _vaccineIcon.Scale    = new Vector2(0.5f, 0.5f);
        _vaccineIcon.Position = new Vector2(-36f, -36f);
        _vaccineIcon.Visible  = false;
        _vaccineIcon.ZIndex   = 5;
        AddChild(_vaccineIcon);

        UpdateStatsLabel();
    }

    protected virtual ImageTexture GetLiveTexture() =>
        SpriteLoader.Instance.Get(Faction == Faction.Ninja ? SpriteType.Ninja : SpriteType.Cowboy);

    protected ImageTexture GetDeadTexture() =>
        SpriteLoader.Instance.Get(Faction == Faction.Ninja ? SpriteType.NinjaDead : SpriteType.CowboyDead);

    // ── AP / HP ───────────────────────────────────────────────────────────────

    public bool UseAP(int cost)
    {
        if (AP < cost) return false;
        AP -= cost;
        UpdateStatsLabel();
        return true;
    }

    public void RestoreAP()
    {
        AP = MaxAP;
        UpdateStatsLabel();
    }

    public virtual void TakeDamage(int damage)
    {
        HP = Mathf.Max(0, HP - damage);
        UpdateStatsLabel();
        if (HP <= 0) Die();
    }

    protected virtual void Die()
    {
        _sprite.Texture = GetDeadTexture();
        ZIndex = -1;

        if (HasVaccine)
        {
            HasVaccine = false;
            _vaccineIcon.Visible = false;
            VaccineDropped?.Invoke(GridCell);
        }
        UnitDied?.Invoke(this);
    }

    // ── Vaccine ───────────────────────────────────────────────────────────────

    public void PickupVaccine()
    {
        HasVaccine = true;
        _vaccineIcon.Visible = true;
        UpdateStatsLabel();
    }

    // ── Movement ──────────────────────────────────────────────────────────────

    public void SnapToCell(Vector2I cell)
    {
        GridCell = cell;
        GlobalPosition = _grid.CellToWorld(cell);
    }

    public async System.Threading.Tasks.Task TweenToCell(Vector2I cell, float duration = 0.18f)
    {
        GridCell = cell;
        var target = _grid.CellToWorld(cell);
        var tween  = CreateTween();
        tween.TweenProperty(this, "global_position", target, duration);
        await ToSignal(tween, "finished");
    }

    // ── Label ─────────────────────────────────────────────────────────────────

    public virtual void UpdateStatsLabel()
    {
        if (_statsLabel == null) return;
        _statsLabel.Text = $"{HP}/{MaxHP}HP\n{AP}/{MaxAP}AP";
    }
}
