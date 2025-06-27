namespace MineSurvivors.scripts.interfaces;
public interface IAttack
{
    float Damage { get; }
    float Cooldown { get; }
    bool CanAttack { get; }
    float PerformAttack(IDamageable target);
}