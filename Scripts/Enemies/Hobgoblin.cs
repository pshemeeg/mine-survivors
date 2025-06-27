using Godot;

namespace MineSurvivors.scripts.enemies;

/// <summary>
/// Hobgoblin - drugi konkretny typ przeciwnika, większy i silniejszy brat Goblina.
/// Doskonały przykład polimorfizmu - wykorzystuje tę samą architekturę co Goblin,
/// ale implementuje całkowicie inną strategię walki.
/// 
/// Charakterystyka Hobgoblina:
/// - Taktyczny, bardziej inteligentny przeciwnik
/// - Wolniejszy ale potężniejszy niż Goblin
/// - Używa "charge attack" - atakuje z dystansu po nabraniu prędkości
/// - Idealny mid-game enemy dla bardziej doświadczonych graczy
/// 
/// Zasady OOP w działaniu:
/// - Dziedziczenie: dziedziczy całą funkcjonalność z Enemy
/// - Polimorfizm: nadpisuje CalculateMovement dla unikalnego zachowania
/// - Hermetyzacja: wykorzystuje protected/public interface klasy bazowej
/// - Kompozycja: używa tej samej struktury komponentów co Enemy
/// 
/// POLIMORFIZM W AKCJI:
/// Hobgoblin i Goblin mogą być traktowane identycznie przez system spawning,
/// collision detection, death handling itp. - ale zachowują się zupełnie inaczej!
/// </summary>
public partial class Hobgoblin : Enemy
{
    #region Hobgoblin-specific Properties
        
    /// <summary>
    /// Zasięg "charge attack" - Hobgoblin przyspiesza gdy jest w tym zasięgu
    /// To jest dodatkowa właściwość specyficzna dla Hobgoblina,
    /// która nie istnieje w klasie bazowej ani u Goblina
    /// </summary>
    [Export] private float _chargeRange = 150f;
        
    /// <summary>
    /// Mnożnik prędkości podczas charge attack
    /// Hobgoblin może być normalnie wolny, ale podczas ataku staje się bardzo szybki
    /// </summary>
    [Export] private float _chargeSpeedMultiplier = 2.5f;
        
    /// <summary>
    /// Prywatna zmienna do śledzenia czy Hobgoblin jest w trybie charge
    /// Hermetyzacja - stan wewnętrzny jest ukryty przed światem zewnętrznym
    /// </summary>
    private bool _isCharging = false;
        
    #endregion

    #region Core Polymorphic Behavior
        
    /// <summary>
    /// KLUCZOWA METODA POLIMORFICZNA!
    /// 
    /// To jest serce polimorfizmu - ta sama sygnatura metody co u Goblina,
    /// ale zupełnie inna implementacja. System gry może traktować wszystkich
    /// wrogów identycznie, ale każdy zachowuje się unikalnie.
    /// 
    /// Strategia Hobgoblina:
    /// 1. Jeśli daleko od gracza - powolny ruch w jego kierunku
    /// 2. Jeśli w zasięgu charge - przyspiesz dramatycznie i zaatakowaj
    /// 3. Po ataku - wróć do powolnego ruchu
    /// 
    /// To pokazuje jak polimorfizm pozwala na sophisticated behavior
    /// przy zachowaniu prostoty w systemach nadrzędnych.
    /// </summary>
    /// <param name="targetPosition">Pozycja gracza (przekazana z klasy bazowej)</param>
    /// <returns>Wektor prędkości - kierunek i szybkość ruchu Hobgoblina</returns>
    protected override Vector2 CalculateMovement(Vector2 targetPosition)
    {
        // Oblicz kierunek do gracza - fundamentalna matematyka wektorowa
        Vector2 direction = (targetPosition - GlobalPosition).Normalized();
        float distanceToPlayer = GlobalPosition.DistanceTo(targetPosition);
            
        // TAKTYKA HOBGOBLINA: Charge attack w odpowiednim zasięgu
        if (distanceToPlayer <= _chargeRange && !_isCharging)
        {
            // Rozpocznij charge attack
            _isCharging = true;
            GD.Print($"Hobgoblin at {GlobalPosition} rozpoczyna charge attack!");
        }
        else if (distanceToPlayer > _chargeRange * 1.5f)
        {
            // Zakończ charge jeśli gracz uciekł za daleko
            _isCharging = false;
        }
            
        // Oblicz finalną prędkość na podstawie stanu
        float finalSpeed;
        if (_isCharging)
        {
            // Szybki charge attack
            finalSpeed = MoveSpeed * _chargeSpeedMultiplier;
        }
        else
        {
            // Normalny, powolny ruch - Hobgoblin jest naturalnie wolniejszy niż Goblin
            finalSpeed = MoveSpeed * 0.7f; // 30% wolniejszy niż base speed
        }
            
        // Zwróć wektor prędkości - Godot aplikuje to w systemie fizyki
        return direction * finalSpeed;
    }
        
    #endregion

    #region Enhanced Initialization
        
    public override void _Ready()
    {
        base._Ready();
        // Hobgoblin-specific setup
        _isCharging = false;

        // Debug message z bardziej detailed info
        GD.Print($"Hobgoblin spawned at {GlobalPosition}");
        GD.Print($"  - Charge range: {_chargeRange}");
        GD.Print($"  - Charge speed multiplier: {_chargeSpeedMultiplier}");
        GD.Print($"  - Base speed (70% of normal): {MoveSpeed * 0.7f}");
        GD.Print($"  - Charge speed: {MoveSpeed * _chargeSpeedMultiplier}");
            
    }
        
    #endregion

    #region Advanced Behavior Extensions

    /// <summary>
    /// Enhanced death behavior dla Hobgoblina.
    /// Demonstration jak można extend cleanup logic dla specific enemy types.
    /// 
    /// Polimorfizm: Ta sama metoda co u innych enemies, ale unique implementation.
    /// </summary>
    public override void _ExitTree()
    {
        // Custom death message z więcej informacji
        string chargeStatus = _isCharging ? "podczas charge attack" : "w normalnym stanie";
        GD.Print($"Hobgoblin at {GlobalPosition} has been defeated {chargeStatus}!");
            
        // Reset charge state przed death (defensive programming)
        _isCharging = false;

        // ZAWSZE wywołuj base implementation dla proper cleanup
        base._ExitTree();
            
    }
        
    #endregion

    #region Public Debug/Utility Methods
        
    public bool IsCharging()
    {
        return _isCharging;
    }
        
    #endregion
}