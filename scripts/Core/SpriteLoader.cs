using Godot;
using System.Collections.Generic;

namespace NinjaCowboy;

public enum SpriteType
{
    Ninja, NinjaDead, Cowboy, CowboyDead,
    TileFloor, TileWall, TileDoor, TileBoarding,
    Vaccine, Shuriken, Starfield, ShipExterior, MenuBg
}

/// <summary>
/// Autoload singleton. Generates all placeholder solid-colour textures at startup.
/// Swap in real art by replacing the ImageTexture in _textures[type].
/// </summary>
public partial class SpriteLoader : Node
{
    public static SpriteLoader Instance { get; private set; }

    private readonly Dictionary<SpriteType, ImageTexture> _textures = new();

    public override void _Ready()
    {
        Instance = this;
        GenerateAll();
    }

    private void GenerateAll()
    {
        Make(SpriteType.Ninja,        64, 64, new Color(0.07f, 0.07f, 0.07f));   // near black
        Make(SpriteType.NinjaDead,    64, 64, new Color(0.26f, 0.20f, 0.20f));   // dark grey-red
        Make(SpriteType.Cowboy,       64, 64, new Color(0.55f, 0.27f, 0.07f));   // saddle brown
        Make(SpriteType.CowboyDead,   64, 64, new Color(0.29f, 0.15f, 0.05f));   // dark brown
        Make(SpriteType.TileFloor,    64, 64, new Color(0.16f, 0.19f, 0.25f));   // dark blue-grey
        Make(SpriteType.TileWall,     64, 64, new Color(0.50f, 0.50f, 0.50f));   // mid grey
        Make(SpriteType.TileDoor,     64, 64, new Color(0.80f, 0.40f, 0.00f));   // orange
        Make(SpriteType.TileBoarding, 64, 64, new Color(0.85f, 0.75f, 0.00f));   // gold
        Make(SpriteType.Vaccine,      32, 32, new Color(0.00f, 0.90f, 0.30f));   // bright green
        Make(SpriteType.Shuriken,     16, 16, new Color(0.75f, 0.75f, 0.80f));   // silver
        Make(SpriteType.Starfield,    64, 64, new Color(0.02f, 0.02f, 0.08f));   // near black
        Make(SpriteType.ShipExterior, 64, 64, new Color(0.05f, 0.09f, 0.13f));   // dark blue
        Make(SpriteType.MenuBg,       64, 64, new Color(0.04f, 0.04f, 0.10f));   // very dark blue
    }

    private void Make(SpriteType type, int w, int h, Color fill)
    {
        var img = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        img.Fill(fill);

        // Darker border for visual separation
        var border = new Color(fill.R * 0.4f, fill.G * 0.4f, fill.B * 0.4f, 1f);
        for (int x = 0; x < w; x++) { img.SetPixel(x, 0, border); img.SetPixel(x, h - 1, border); }
        for (int y = 0; y < h; y++) { img.SetPixel(0, y, border); img.SetPixel(w - 1, y, border); }

        _textures[type] = ImageTexture.CreateFromImage(img);
    }

    public ImageTexture Get(SpriteType type) => _textures[type];
}
