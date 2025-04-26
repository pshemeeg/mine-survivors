using Godot;
using System;

// klasa bazowa dla postaci w grze
public partial class Character : CharacterBody2D
{
    [Export] public int Health { get; set; }
    [Export] public int AttackDamage { get; set; }
    [Export] public float MovementSpeed { get; set; }
    // metoda do obliczania movemnetu
    protected void ProcessMovement(Vector2 direction, float delta)
    {
        if (direction == Vector2.Zero)
        {
            Velocity = Vector2.Zero;
        }
        else
        {
            Velocity = direction.Normalized() * MovementSpeed;
            MoveAndSlide();
        }
    }

    // metoda do obliczania przyjetych obrazen 
    public virtual void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Death();
        }
    }
    // metoda śmierci
    public virtual void Death()
    {
        QueueFree();
    }

    // metoda wykonywania ataku
    public virtual void Attack(Character target)
    {
        target.TakeDamage(AttackDamage);
    }


}
