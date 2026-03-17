using Godot;
using System;

namespace NinjaCowboy;

/// <summary>
/// CanvasLayer overlay: turn counter (top-left) and End Turn button (bottom-right).
/// </summary>
public partial class HUD : CanvasLayer
{
    public event Action EndTurnRequested;

    private Label  _turnLabel;
    private Button _endTurnButton;

    public override void _Ready()
    {
        Layer = 10;

        // Turn counter
        _turnLabel = new Label();
        _turnLabel.Text = "Turn 1";
        _turnLabel.Position = new Vector2(12, 8);
        _turnLabel.AddThemeColorOverride("font_color", Colors.White);
        _turnLabel.AddThemeFontSizeOverride("font_size", 20);
        AddChild(_turnLabel);

        // Phase label
        var phaseLabel = new Label();
        phaseLabel.Name = "PhaseLabel";
        phaseLabel.Text = "PLAYER TURN";
        phaseLabel.Position = new Vector2(12, 36);
        phaseLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 1f));
        phaseLabel.AddThemeFontSizeOverride("font_size", 16);
        AddChild(phaseLabel);

        // End Turn button
        _endTurnButton = new Button();
        _endTurnButton.Text = "End Turn";
        _endTurnButton.Size = new Vector2(120, 40);
        _endTurnButton.Position = new Vector2(1920 - 140, 1080 - 56);
        _endTurnButton.Pressed += () => EndTurnRequested?.Invoke();
        AddChild(_endTurnButton);
    }

    public void SetTurnNumber(int turn)  => _turnLabel.Text = $"Turn {turn}";

    public void SetPhase(TurnPhase phase)
    {
        var lbl = GetNodeOrNull<Label>("PhaseLabel");
        if (lbl == null) return;
        lbl.Text = phase == TurnPhase.PlayerTurn ? "PLAYER TURN" : "COWBOY TURN";
        lbl.AddThemeColorOverride("font_color",
            phase == TurnPhase.PlayerTurn ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.5f, 0.2f));
    }

    public void SetEndTurnEnabled(bool enabled) => _endTurnButton.Disabled = !enabled;
}
