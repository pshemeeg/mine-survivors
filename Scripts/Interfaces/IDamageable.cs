namespace MineSurvivors.scripts.interfaces;
public interface IDamageable
{
    float MaxHealth { get; }
    float CurrentHealth { get; }
    bool IsAlive { get; }
    void TakeDamage(float damage);
}