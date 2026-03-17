using Godot;

namespace NinjaCowboy;

/// <summary>
/// Exported Resource describing a mission's high-level configuration.
/// Extend per-mission by subclassing or by overriding the layout helpers.
/// </summary>
[GlobalClass]
public partial class MissionData : Resource
{
    [Export] public string MissionName    { get; set; } = "Mission 01";
    [Export] public string ObjectiveText  { get; set; } = "Steal the vaccine and return to the boarding points.";
    [Export] public int    GridWidth      { get; set; } = 30;
    [Export] public int    GridHeight     { get; set; } = 15;
    [Export] public int    CowboyCount    { get; set; } = 10;
    [Export] public int    NinjaCount     { get; set; } = 4;
}
