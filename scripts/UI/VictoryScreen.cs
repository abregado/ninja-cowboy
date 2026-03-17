using Godot;

namespace NinjaCowboy;

/// <summary>Victory overlay scene.</summary>
public partial class VictoryScreen : Control
{
    public override void _Ready()
    {
        var bg = new ColorRect();
        bg.Color           = new Color(0f, 0.1f, 0f, 0.85f);
        bg.Size            = new Vector2(1920, 1080);
        bg.MouseFilter     = Control.MouseFilterEnum.Stop;
        AddChild(bg);

        var vbox = new VBoxContainer();
        vbox.Position = new Vector2(660, 380);
        vbox.Size     = new Vector2(600, 300);
        AddChild(vbox);

        var title = new Label();
        title.Text            = "MISSION COMPLETE!";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 0.4f));
        title.AddThemeFontSizeOverride("font_size", 48);
        vbox.AddChild(title);

        var sub = new Label();
        sub.Text              = "The vaccine is secure.";
        sub.HorizontalAlignment = HorizontalAlignment.Center;
        sub.AddThemeColorOverride("font_color", Colors.White);
        sub.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(sub);

        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 40);
        vbox.AddChild(spacer);

        var btn = new Button();
        btn.Text              = "Play Again";
        btn.CustomMinimumSize = new Vector2(200, 50);
        btn.Pressed          += () => GameManager.Instance.RestartMission();
        vbox.AddChild(btn);
    }
}
