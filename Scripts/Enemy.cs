using Godot;

// Klasa reprezentująca przeciwnika, dziedzicząca po Character
public partial class Enemy : CharacterBody2D, IDamageable
{


    [Export] protected float maxHealth = 100f;
    protected float currentHealth;
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;

    [Export] public float MovementSpeed { get; set; } = 150;
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

    // Zmienna przechowująca referencję do węzła gracza
    private Player _player;

    // Metoda wywoływana przy inicjalizacji węzła
    public override void _Ready()
    {
        // Próba znalezienia węzła gracza w drzewie sceny.
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;

        if (_player == null)
        {
            GD.PrintErr("Enemy: Nie znaleziono węzła gracza w grupie 'Player'.");
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (_player != null)
        {
            // Obliczanie kierunku do gracza
            Vector2 directionToPlayer = (_player.GlobalPosition - GlobalPosition).Normalized();

            // Wywołanie metody ruchu z klasy bazowej
            ProcessMovement(directionToPlayer, delta);
        }
        else
        {
            // Jeśli gracz nie został znaleziony, przeciwnik stoi w miejscu
            ProcessMovement(Vector2.Zero, delta);
        }
    }
    public void TakeDamage(float damage)
    {
        currentHealth = -damage;
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
    }
}