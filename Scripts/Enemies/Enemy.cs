using Godot;
using MineSurvivors.scripts.interfaces;
using MineSurvivors.scripts.player;
using MineSurvivors.scripts.managers;

namespace MineSurvivors.scripts.enemies;

/// <summary>
/// Zoptymalizowana klasa Enemy z cachowaniem gracza.
/// Wprowadza prostą optymalizację wydajności bez zwiększania złożoności.
/// 
/// OPTYMALIZACJA: Cache'owanie referencji do gracza
/// - Zamiast szukać gracza w każdej klatce (expensive operation)
/// - Znajdź raz w _Ready() i przechowuj referencję
/// - Sprawdzaj tylko czy gracz nadal istnieje i żyje
/// 
/// Zasady OOP: Dziedziczenie, Polimorfizm, Hermetyzacja, Interfejsy
/// Zasada KISS: Jedna prosta zmiana, duży zysk wydajnościowy
/// </summary>
public partial class Enemy : CharacterBody2D, IDamageable, IAttack
{
    #region Exported Stats - Łatwe zarządzanie w edytorze
    [ExportGroup("Combat Stats")]
    [Export] public float MaxHealth { get; set; } = 50f;
    [Export] public float Damage { get; set; } = 10f;
    [Export] public float AttackCooldown { get; set; } = 1.0f;

    [ExportGroup("Movement")]
    [Export] public float MoveSpeed { get; set; } = 100f;

    [ExportGroup("Rewards")]
    [Export] public float ExperienceReward { get; set; } = 10f;
    #endregion

    #region Simple State - Tylko to co konieczne
    public float CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool CanAttack { get; private set; } = true;
    public float Cooldown => AttackCooldown;

    // Komponenty - znajdowane raz w _Ready
    private AnimatedSprite2D _sprite;
    private Area2D _attackArea;
    private Timer _attackTimer;
        
    // NOWE: Cache gracza - kluczowa optymalizacja wydajności
    private Player _player;
    #endregion

    #region Core Functionality - Zoptymalizowane
    public override void _Ready()
    {
        // Ustaw zdrowie
        CurrentHealth = MaxHealth;

        // Znajdź komponenty (bez extensive error handling - jeśli nie ma, to błąd w setup)
        _sprite = GetNode<AnimatedSprite2D>("VisualRepresentation");
        _attackArea = GetNode<Area2D>("AttackDetectionArea");
        _attackTimer = GetNode<Timer>("AttackCooldownTimer");

        // KLUCZOWA OPTYMALIZACJA: Cache gracza raz, zamiast szukać w każdej klatce
        // To dramatic improvement wydajności - z O(n) searches per frame do O(1) lookup
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
        // Proste sprawdzenie czy gracz istnieje - bez tego gra nie może działać
        if (_player == null)
        {
            GD.PrintErr("BŁĄD: Nie znaleziono gracza! Enemy potrzebuje gracza do funkcjonowania.");
            // Opcjonalnie: QueueFree(); - usuń przeciwnika jeśli nie ma gracza
        }

        // Skonfiguruj timer
        _attackTimer.WaitTime = AttackCooldown;
        _attackTimer.Timeout += () => CanAttack = true;

        AddToGroup("enemies");
            
        // Debug info - pomocne podczas development
        GD.Print($"Enemy ready. Player cached: {_player != null}");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsAlive) return;

        // OPTYMALIZACJA: Użyj cached gracza zamiast expensive search
        // Jedyne co musimy sprawdzić to czy gracz nadal istnieje i żyje
        if (_player == null || !IsNodeInsideTree(_player) || !_player.IsAlive) 
        {
            // Gracz nie istnieje lub umarł - przeciwnik może:
            // 1. Stać w miejscu (obecne rozwiązanie)
            // 2. Wandrować losowo 
            // 3. Atakować innych wrogów
            // 4. Usunąć się z gry
                
            // Dla prostoty: zatrzymaj się i czekaj
            Velocity = Vector2.Zero;
            _sprite.Play("idle");
            return;
        }

        // Logika: jeśli możesz atakować - atakuj, jeśli nie - idź do gracza
        if (CanAttack && IsPlayerInRange())
        {
            PerformAttack(_player);
            _sprite.Play("attack");
        }
        else
        {
            // Polimorfizm - jedyna wirtualna metoda którą naprawdę potrzebujemy
            // Teraz używamy cached player position zamiast parameter
            Velocity = CalculateMovement(_player.GlobalPosition);
            _sprite.Play("walk");
        }

        MoveAndSlide();
    }
    #endregion

    #region Interfaces Implementation - Czyste i proste
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        CurrentHealth -= damage;
        _sprite.Play("hurt");

        if (!IsAlive)
        {
            _sprite.Play("death");
            SetPhysicsProcess(false);
            _sprite.AnimationFinished += QueueFree;
            // Loose coupling - Enemy nie musi znać szczegółów GameManager
            GameManager.Instance?.RegisterEnemyKill();
            GameManager.Instance?.AddExperience(ExperienceReward);
            EmitSignal(SignalName.Died, this);
        }
    }

    public float PerformAttack(IDamageable target)
    {
        if (!CanAttack) return 0f;

        target.TakeDamage(Damage);
        CanAttack = false;
        _attackTimer.Start();

        return Damage;
    }

    [Signal] public delegate void DiedEventHandler(Enemy enemy);
    #endregion

    #region Single Virtual Method - Jedyny punkt polimorfizmu
    /// <summary>
    /// Jedyna metoda wirtualna - definiuje jak przeciwnik się porusza.
    /// Wszystkie różnice między typami przeciwników wyrażane przez tę jedną metodę.
    /// 
    /// Domyślnie: prosty ruch liniowy w kierunku gracza.
    /// Klasy pochodne mogą to zmienić dla różnych zachowań.
    /// </summary>
    protected virtual Vector2 CalculateMovement(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - GlobalPosition).Normalized();
        return direction * MoveSpeed;
    }
    #endregion

    #region Simple Helpers - Bez skomplikowanej logiki
    private bool IsPlayerInRange()
    {
        // Prosta sprawdzenia - bez cache'owania czy skomplikowanych warunków
        return _attackArea.HasOverlappingBodies();
    }
        
    /// <summary>
    /// Pomocnicza metoda do sprawdzania czy Node nadal istnieje w drzewie sceny.
    /// Godot automatycznie może usuwać obiekty, więc warto to sprawdzić.
    /// </summary>
    private bool IsNodeInsideTree(Node node)
    {
        return node != null && IsInstanceValid(node) && node.IsInsideTree();
    }
    #endregion

    #region Optional: Advanced Cache Management (dla przyszłości)
    /// <summary>
    /// Opcjonalna metoda do odświeżenia cache gracza.
    /// Użyteczna jeśli gracz może się zmieniać podczas gry (multiplayer, respawn, etc.)
    /// 
    /// Na razie nie jest potrzebna, ale pokazuje jak można rozszerzyć system
    /// bez breaking existing code.
    /// </summary>
    public void RefreshPlayerCache()
    {
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        GD.Print($"Player cache refreshed. New player: {_player != null}");
    }
        
    /// <summary>
    /// Debug method - sprawdź status cached gracza.
    /// Przydatne podczas development i testing.
    /// </summary>
    public string GetPlayerCacheStatus()
    {
        if (_player == null) return "Player cache: NULL";
        if (!IsNodeInsideTree(_player)) return "Player cache: INVALID (not in tree)";
        if (!_player.IsAlive) return "Player cache: DEAD";
        return "Player cache: VALID";
    }
    #endregion
}