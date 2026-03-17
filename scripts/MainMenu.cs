using Godot;

namespace NinjaCowboy;

/// <summary>
/// Main menu scene. Tiled starfield background + title + start button.
/// </summary>
public partial class MainMenu : Control
{
    public override void _Ready()
    {
        // Tiled starfield background
        var bg = new TextureRect();
        bg.Texture           = SpriteLoader.Instance.Get(SpriteType.Starfield);
        bg.TextureRepeat     = CanvasItem.TextureRepeatEnum.Enabled;
        bg.StretchMode       = TextureRect.StretchModeEnum.Tile;
        bg.Size              = new Vector2(1920, 1080);
        bg.MouseFilter       = Control.MouseFilterEnum.Ignore;
        AddChild(bg);

        // Dark overlay for readability
        var overlay = new ColorRect();
        overlay.Color      = new Color(0f, 0f, 0f, 0.55f);
        overlay.Size       = new Vector2(1920, 1080);
        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(overlay);

        // Centre vbox
        var vbox = new VBoxContainer();
        vbox.Position = new Vector2(760, 300);
        vbox.Size     = new Vector2(400, 480);
        AddChild(vbox);

        var title = new Label();
        title.Text            = "SPACE NINJA:\nTACTICS";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", new Color(0.2f, 0.9f, 1f));
        title.AddThemeFontSizeOverride("font_size", 52);
        vbox.AddChild(title);

        var subtitle = new Label();
        subtitle.Text         = "Mission 01: Steal the Vaccine";
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        subtitle.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        subtitle.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(subtitle);

        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 60);
        vbox.AddChild(spacer);

        var startBtn = new Button();
        startBtn.Text             = "Start Game";
        startBtn.CustomMinimumSize = new Vector2(200, 54);
        startBtn.Pressed          += () => GameManager.Instance.GoToBoardingSelect();
        vbox.AddChild(startBtn);

        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(0, 16);
        vbox.AddChild(spacer2);

        var howToBtn = new Button();
        howToBtn.Text             = "How to Play";
        howToBtn.CustomMinimumSize = new Vector2(200, 44);
        howToBtn.Pressed          += OnHowToPlay;
        vbox.AddChild(howToBtn);

        // Instructions label (hidden until How To Play)
        var instr = new Label();
        instr.Name   = "InstructionsLabel";
        instr.Visible = false;
        instr.Position = new Vector2(480, 780);
        instr.Size     = new Vector2(960, 240);
        instr.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        instr.AddThemeColorOverride("font_color", Colors.White);
        instr.AddThemeFontSizeOverride("font_size", 14);
        instr.Text = "OBJECTIVE: Pick up the vaccine and return a ninja to any boarding tile.\n\n" +
                     "SELECT NINJA: Click on a ninja to select. Click again for Ambush/Conceal.\n" +
                     "MOVE: Click an empty cell. Cost: 1 AP per square.\n" +
                     "ATTACK: Click a cowboy. Close range: choose SHURIKEN or KATANA. Long range: SHURIKEN only.\n" +
                     "END TURN: Click End Turn when ninjas are ready (or set Ambush/Conceal).\n" +
                     "WIN: Ninja carrying vaccine steps onto a boarding tile (marked gold).";
        AddChild(instr);
    }

    private void OnHowToPlay()
    {
        var lbl = GetNodeOrNull<Label>("InstructionsLabel");
        if (lbl != null) lbl.Visible = !lbl.Visible;
    }
}
