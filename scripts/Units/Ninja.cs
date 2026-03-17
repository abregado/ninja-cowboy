using Godot;

namespace NinjaCowboy;

/// <summary>
/// Player-controlled ninja. Adds shuriken count and Ambush/Conceal state.
/// </summary>
public partial class Ninja : Unit
{
    public int  Shuriken       { get; set; }
    public bool IsAmbush       { get; set; }
    public bool IsConceal      { get; set; }
    public bool HasActedThisTurn { get; set; }

    public override void Initialize(UnitStats stats, Vector2I cell, GridManager grid)
    {
        Faction  = Faction.Ninja;
        Shuriken = stats.MaxShuriken;
        base.Initialize(stats, cell, grid);
    }

    public override void UpdateStatsLabel()
    {
        if (_statsLabel == null) return;
        string state = IsAmbush ? " [AMB]" : IsConceal ? " [CON]" : "";
        _statsLabel.Text = $"{HP}/{MaxHP}HP\n{AP}/{MaxAP}AP\n★×{Shuriken}{state}";
    }

    public void SetAmbush()
    {
        IsAmbush  = true;
        IsConceal = false;
        HasActedThisTurn = true;
        UpdateStatsLabel();
    }

    public void SetConceal()
    {
        IsConceal = true;
        IsAmbush  = false;
        HasActedThisTurn = true;
        UpdateStatsLabel();
    }

    public void NewTurn()
    {
        IsAmbush       = false;
        IsConceal      = false;
        HasActedThisTurn = false;
        RestoreAP();
    }
}
