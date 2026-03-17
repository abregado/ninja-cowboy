using Godot;
using System;
using System.Collections.Generic;

namespace NinjaCowboy;

/// <summary>
/// CanvasLayer popup showing contextual action buttons for the selected ninja.
/// Call Show(options) to display; each option raises ActionChosen with its label.
/// </summary>
public partial class ActionDialogue : CanvasLayer
{
    public event Action<string> ActionChosen;

    private Panel         _panel;
    private VBoxContainer _vbox;

    public override void _Ready()
    {
        Layer   = 20;
        Visible = false;

        _panel = new Panel();
        _panel.Size = new Vector2(160, 220);
        AddChild(_panel);

        _vbox = new VBoxContainer();
        _vbox.Position = new Vector2(8, 8);
        _vbox.Size     = new Vector2(144, 204);
        _panel.AddChild(_vbox);
    }

    /// <summary>Show the dialogue near a world position.</summary>
    public void Show(Vector2 worldPos, List<string> options, Camera2D cam = null)
    {
        // Clear previous buttons
        foreach (Node child in _vbox.GetChildren()) child.QueueFree();

        float panelH = options.Count * 42 + 16;
        _panel.Size = new Vector2(160, panelH);

        // Position panel near the unit, nudged onto screen
        var screenPos = worldPos + new Vector2(40, -80);
        screenPos.X = Mathf.Clamp(screenPos.X, 4, 1920 - 164);
        screenPos.Y = Mathf.Clamp(screenPos.Y, 4, 1080 - panelH - 4);
        _panel.Position = screenPos;

        foreach (var option in options)
        {
            var btn = new Button();
            btn.Text    = option;
            btn.Size    = new Vector2(144, 36);
            string captured = option;
            btn.Pressed += () =>
            {
                Visible = false;
                ActionChosen?.Invoke(captured);
            };
            _vbox.AddChild(btn);
        }
        Visible = true;
    }

    public new void Hide() => Visible = false;
}
