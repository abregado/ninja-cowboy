using Godot;
using System;
using System.Collections.Generic;

namespace NinjaCowboy;

/// <summary>
/// CanvasLayer confirmation panel before ending the player turn.
/// Warns about ninjas with remaining AP that haven't set Ambush/Conceal.
/// </summary>
public partial class EndTurnDialogue : CanvasLayer
{
    public event Action Confirmed;
    public event Action Cancelled;

    private Panel _panel;
    private Label _warningLabel;

    public override void _Ready()
    {
        Layer   = 20;
        Visible = false;

        _panel = new Panel();
        _panel.Size     = new Vector2(360, 200);
        _panel.Position = new Vector2(780, 440);
        AddChild(_panel);

        var vbox = new VBoxContainer();
        vbox.Position = new Vector2(12, 12);
        vbox.Size     = new Vector2(336, 176);
        _panel.AddChild(vbox);

        var title = new Label();
        title.Text = "End Turn?";
        title.AddThemeColorOverride("font_color", Colors.White);
        title.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(title);

        _warningLabel = new Label();
        _warningLabel.Text = "";
        _warningLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
        _warningLabel.AddThemeFontSizeOverride("font_size", 13);
        _warningLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(_warningLabel);

        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, 8);
        vbox.AddChild(spacer);

        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);

        var confirmBtn = new Button();
        confirmBtn.Text      = "End Turn";
        confirmBtn.Size      = new Vector2(120, 36);
        confirmBtn.Pressed  += () => { Visible = false; Confirmed?.Invoke(); };
        hbox.AddChild(confirmBtn);

        var spacer2 = new Control();
        spacer2.CustomMinimumSize = new Vector2(12, 0);
        hbox.AddChild(spacer2);

        var cancelBtn = new Button();
        cancelBtn.Text     = "Cancel";
        cancelBtn.Size     = new Vector2(100, 36);
        cancelBtn.Pressed += () => { Visible = false; Cancelled?.Invoke(); };
        hbox.AddChild(cancelBtn);
    }

    public void Show(List<string> warnings)
    {
        _warningLabel.Text = warnings.Count > 0
            ? "Warning:\n" + string.Join("\n", warnings)
            : "All ninjas ready.";
        Visible = true;
    }
}
