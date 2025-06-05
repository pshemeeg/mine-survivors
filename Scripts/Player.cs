using Godot;
using System;

public partial class Player : CharacterBody2D, IDamageable
{

    [Export] public float MovementSpeed { get; set; } = 200;

    [Export] protected float maxHealth = 100f;
    protected float currentHealth;
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;

    

    // metoda do obliczania movemnetu
    protected void ProcessMovement(Vector2 direction, double delta)
    {
        AnimatedSprite2D animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (direction == Vector2.Zero)
        {
            Velocity = Vector2.Zero;
            animatedSprite.Animation = "idle";
        }
        else
        {
            animatedSprite.Animation = "run";
            if (direction.X < 0)
            {
                animatedSprite.FlipH = true;
            }
            else
            {
                animatedSprite.FlipH = false;
            }
            Velocity = direction.Normalized() * MovementSpeed;
            MoveAndSlide();
        }
    }
    public Vector2 GetInput()
    {

        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");

        return inputDirection;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = GetInput();

        ProcessMovement(direction,delta);
    }

    public void TakeDamage(float damage)
    {
       currentHealth =- damage;
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;    
    }
}


