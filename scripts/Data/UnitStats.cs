using Godot;

namespace NinjaCowboy;

/// <summary>
/// Exported Resource holding all tuneable numeric attributes for a unit type.
/// Create instances via New() in code or in the Godot editor inspector.
/// </summary>
[GlobalClass]
public partial class UnitStats : Resource
{
    // ── Base ──────────────────────────────────────────────────────────────────
    [Export] public int MaxHP      { get; set; } = 12;
    [Export] public int MaxAP      { get; set; } = 10;
    [Export] public int MaxShuriken{ get; set; } = 3;

    // ── Ninja weapons ─────────────────────────────────────────────────────────
    [Export] public int   ShurikenAPCost    { get; set; } = 3;
    [Export] public int   KatanaAPCostBase  { get; set; } = 2;   // +1 per sq moved
    [Export] public int   AmbushAPCost      { get; set; } = 5;
    [Export] public int   ConcealAPCost     { get; set; } = 3;

    [Export] public float ShurikenBaseHit   { get; set; } = 0.80f;
    [Export] public float ShurikenFalloff   { get; set; } = 0.05f; // per square
    [Export] public int   ShurikenMinDmg    { get; set; } = 2;
    [Export] public int   ShurikenMaxDmg    { get; set; } = 4;

    [Export] public float KatanaBaseHit     { get; set; } = 0.80f;
    [Export] public int   KatanaMinDmg      { get; set; } = 3;
    [Export] public int   KatanaMaxDmg      { get; set; } = 5;

    [Export] public float ConcealMissChance { get; set; } = 0.85f;

    // ── Cowboy weapons ────────────────────────────────────────────────────────
    [Export] public int   GunAPCost         { get; set; } = 4;
    [Export] public float GunBaseHit        { get; set; } = 0.80f;
    [Export] public float GunFalloff        { get; set; } = 0.10f; // per square
    [Export] public float GunAdjacentHit    { get; set; } = 0.50f;
    [Export] public int   GunMinDmg         { get; set; } = 3;
    [Export] public int   GunMaxDmg         { get; set; } = 5;

    // ── Factory helpers ───────────────────────────────────────────────────────
    public static UnitStats DefaultNinja()
    {
        return new UnitStats
        {
            MaxHP = 12, MaxAP = 10, MaxShuriken = 3
        };
    }

    public static UnitStats DefaultCowboy()
    {
        return new UnitStats
        {
            MaxHP = 12, MaxAP = 10, MaxShuriken = 0,
            ShurikenAPCost = 99, KatanaAPCostBase = 99,
            AmbushAPCost = 99, ConcealAPCost = 99
        };
    }
}
