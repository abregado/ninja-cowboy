using Godot;

namespace NinjaCowboy;

public enum TurnPhase { PlayerTurn, AITurn }

/// <summary>
/// Battle-scene node. Tracks whose turn it is and the turn counter.
/// </summary>
public partial class TurnManager : Node
{
    [Signal] public delegate void PlayerTurnStartedEventHandler();
    [Signal] public delegate void AITurnStartedEventHandler();

    public TurnPhase CurrentPhase { get; private set; } = TurnPhase.PlayerTurn;
    public int TurnNumber { get; private set; } = 1;

    public void StartPlayerTurn()
    {
        CurrentPhase = TurnPhase.PlayerTurn;
        EmitSignal(SignalName.PlayerTurnStarted);
    }

    public void EndPlayerTurn()
    {
        CurrentPhase = TurnPhase.AITurn;
        EmitSignal(SignalName.AITurnStarted);
    }

    public void EndAITurn()
    {
        TurnNumber++;
        StartPlayerTurn();
    }
}
