using Godot;

namespace NinjaCowboy;

/// <summary>
/// Stateless helpers for hit rolls and damage rolls.
/// Extend by adding new weapon methods without modifying existing ones.
/// </summary>
public static class CombatSystem
{
    // ── Roll helpers ──────────────────────────────────────────────────────────

    public static bool RollHit(float chance)  => GD.Randf() < Mathf.Clamp(chance, 0f, 1f);
    public static int  RollDamage(int min, int max) => min + (int)(GD.Randi() % (uint)(max - min + 1));

    // ── Ninja attacks ─────────────────────────────────────────────────────────

    public static (bool hit, int damage) ShurikenAttack(UnitStats stats, int distanceSq)
    {
        float chance = stats.ShurikenBaseHit - stats.ShurikenFalloff * distanceSq;
        bool hit = RollHit(chance);
        int dmg  = hit ? RollDamage(stats.ShurikenMinDmg, stats.ShurikenMaxDmg) : 0;
        return (hit, dmg);
    }

    public static (bool hit, int damage) KatanaAttack(UnitStats stats)
    {
        bool hit = RollHit(stats.KatanaBaseHit);
        int dmg  = hit ? RollDamage(stats.KatanaMinDmg, stats.KatanaMaxDmg) : 0;
        return (hit, dmg);
    }

    // ── Cowboy attacks ────────────────────────────────────────────────────────

    public static (bool hit, int damage) GunAttack(UnitStats stats, int distanceSq)
    {
        float chance = distanceSq <= 1
            ? stats.GunAdjacentHit
            : stats.GunBaseHit - stats.GunFalloff * distanceSq;
        bool hit = RollHit(chance);
        int dmg  = hit ? RollDamage(stats.GunMinDmg, stats.GunMaxDmg) : 0;
        return (hit, dmg);
    }
}
