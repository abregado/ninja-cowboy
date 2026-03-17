using Godot;

namespace NinjaCowboy;

/// <summary>
/// Autoload singleton. Owns scene transitions and cross-scene mission state.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    // Set by BoardingSelect before switching to BattleScene
    public Vector2I[] BoardingTiles { get; set; } = new Vector2I[4];

    public int CurrentMission { get; private set; } = 1;

    public override void _Ready()
    {
        Instance = this;
    }

    public void GoToMainMenu()   => GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
    public void GoToBoardingSelect() => GetTree().ChangeSceneToFile("res://scenes/BoardingSelect.tscn");
    public void GoToBattle()     => GetTree().ChangeSceneToFile("res://scenes/BattleScene.tscn");
    public void GoToVictory()    => GetTree().ChangeSceneToFile("res://scenes/VictoryScreen.tscn");
    public void GoToGameOver()   => GetTree().ChangeSceneToFile("res://scenes/GameOverScreen.tscn");
    public void RestartMission() => GoToBoardingSelect();
}
