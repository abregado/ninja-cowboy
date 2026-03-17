using Godot;

namespace NinjaCowboy;

public enum CowboyState { Patrolling, Alerted }

/// <summary>
/// AI-controlled cowboy. Extends Unit with patrol/alert state.
/// AI logic lives in BattleScene.RunCowboyTurn() to keep it centralised.
/// </summary>
public partial class Cowboy : Unit
{
    public CowboyState AIState    { get; set; } = CowboyState.Patrolling;
    public Vector2I    LastKnownNinjaPos { get; set; }

    public override void Initialize(UnitStats stats, Vector2I cell, GridManager grid)
    {
        Faction = Faction.Cowboy;
        base.Initialize(stats, cell, grid);
    }
}
