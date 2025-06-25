using Godot;
using System;

public interface IAttack
{
    float AttackDamage { get; }
    float AttackRange { get; }
    float AttackCooldown { get; }
   
    void Attack(Vector2 targetPosition, IDamageable target = null);
    bool CanAttack();
}