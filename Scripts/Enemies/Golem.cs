using Godot;

namespace MineSurvivors.scripts.enemies;

/// <summary>
/// Golem - trzeci konkretny typ przeciwnika, wolny ale bardzo wytrzymały.
/// POPRAWIONA IMPLEMENTACJA z proper Godot timers i animacjami.
/// 
/// Charakterystyka Golema:
/// - Wolny ale bardzo wytrzymały przeciwnik
/// - Porusza się w prostych liniach z krótkimi przystankami
/// - Używa proper Godot Timer nodes zamiast manual timing
/// - Korzysta z animacji idle podczas odpoczynku
/// 
/// Zasady OOP w działaniu:
/// - Dziedziczenie: dziedziczy całą funkcjonalność z Enemy
/// - Polimorfizm: nadpisuje CalculateMovement dla step-based movement
/// - Hermetyzacja: wykorzystuje protected/public interface klasy bazowej
/// - Kompozycja: używa Godot Timer nodes jako komponenty
/// </summary>
public partial class Golem : Enemy
{
    #region Golem-specific Properties
    
    /// <summary>
    /// Czas ruchu przed następnym przystankiem
    /// </summary>
    [Export] private float _moveTime = 2.0f;
    
    /// <summary>
    /// Czas postoju między ruchami
    /// </summary>
    [Export] private float _restTime = 1.0f;
    
    /// <summary>
    /// Czy Golem aktualnie się porusza czy odpoczywa
    /// </summary>
    private bool _isMoving = true;
    
    /// <summary>
    /// Godot Timer dla fazy ruchu
    /// </summary>
    private Timer _moveTimer;
    
    /// <summary>
    /// Godot Timer dla fazy odpoczynku
    /// </summary>
    private Timer _restTimer;
    
    #endregion

    #region Initialization - Proper Godot way
    
    public override void _Ready()
    {
        base._Ready();
        
        // Stwórz i skonfiguruj timery - proper Godot approach
        SetupTimers();
        
        // Rozpocznij od fazy ruchu
        StartMovementPhase();
        
        // Debug info
        GD.Print($"Golem awakened at {GlobalPosition}");
        GD.Print($"  - Move time: {_moveTime}s");
        GD.Print($"  - Rest time: {_restTime}s");
        GD.Print($"  - Movement speed: {MoveSpeed * 0.5f} (50% of base)");
    }
    
    /// <summary>
    /// Hermetyzacja: Setup timerów w jednym miejscu
    /// </summary>
    private void SetupTimers()
    {
        // Movement Timer
        _moveTimer = new Timer();
        _moveTimer.WaitTime = (float)_moveTime;
        _moveTimer.OneShot = true;
        _moveTimer.Timeout += OnMovementPhaseEnd;
        AddChild(_moveTimer);
        
        // Rest Timer  
        _restTimer = new Timer();
        _restTimer.WaitTime = (float)_restTime;
        _restTimer.OneShot = true;
        _restTimer.Timeout += OnRestPhaseEnd;
        AddChild(_restTimer);
    }
    
    #endregion

    #region Core Polymorphic Behavior - Simplified
    
    /// <summary>
    /// KLUCZOWA METODA POLIMORFICZNA!
    /// 
    /// Teraz super prosta - tylko sprawdza stan i zwraca odpowiedni ruch.
    /// Cała logika timing jest obsługiwana przez Godot timers!
    /// </summary>
    /// <param name="targetPosition">Pozycja gracza</param>
    /// <returns>Wektor prędkości</returns>
    protected override Vector2 CalculateMovement(Vector2 targetPosition)
    {
        // Jeśli odpoczywamy - nie ruszaj się (animacja idle jest już ustawiona)
        if (!_isMoving)
        {
            return Vector2.Zero;
        }
        
        // Jeśli się poruszamy - powolny ruch w kierunku gracza
        Vector2 direction = (targetPosition - GlobalPosition).Normalized();
        float golemSpeed = MoveSpeed * 0.5f; // 50% podstawowej prędkości
        
        return direction * golemSpeed;
    }
    
    #endregion

    #region Timer Event Handlers - Clean separation
    
    /// <summary>
    /// Koniec fazy ruchu - przejdź do odpoczynku
    /// </summary>
    private void OnMovementPhaseEnd()
    {
        StartRestPhase();
    }
    
    /// <summary>
    /// Koniec fazy odpoczynku - przejdź do ruchu  
    /// </summary>
    private void OnRestPhaseEnd()
    {
        StartMovementPhase();
    }
    
    #endregion

    #region Phase Management - Hermetyzacja stanów
    
    /// <summary>
    /// Rozpocznij fazę ruchu
    /// </summary>
    private void StartMovementPhase()
    {
        _isMoving = true;
        _moveTimer.Start();
        
        // Animacja chodzenia będzie ustawiona automatycznie w base Enemy
        // gdy velocity != Vector2.Zero
        
        GD.Print($"Golem starts moving for {_moveTime}s");
    }
    
    /// <summary>
    /// Rozpocznij fazę odpoczynku
    /// </summary>
    private void StartRestPhase()
    {
        _isMoving = false;
        _restTimer.Start();
        
        // NIE RUSZAMY ANIMACJI - klasa bazowa Enemy już to zarządza!
        // Gdy velocity == Vector2.Zero, Enemy automatycznie ustawi "idle"
        
        GD.Print($"Golem starts resting for {_restTime}s");
    }
    
    #endregion

    #region Public Debug/Utility Methods
    
    /// <summary>
    /// Publiczny getter dla stanu ruchu
    /// </summary>
    public bool IsCurrentlyMoving()
    {
        return _isMoving;
    }
    
    /// <summary>
    /// Ile czasu zostało w obecnej fazie
    /// </summary>
    public float GetTimeLeftInCurrentPhase()
    {
        if (_isMoving && _moveTimer != null)
            return (float)_moveTimer.TimeLeft;
        else if (!_isMoving && _restTimer != null)
            return (float)_restTimer.TimeLeft;
        
        return 0f;
    }
    
    #endregion

    #region Cleanup
    
    public override void _ExitTree()
    {
        string currentState = _isMoving ? "during movement phase" : "during rest phase";
        float timeLeft = GetTimeLeftInCurrentPhase();
        
        GD.Print($"Golem crumbled to dust {currentState}!");
        GD.Print($"  - Time left in phase: {timeLeft:F1}s");
        
        // Godot automatycznie wyczyści timery gdy node zostanie usunięty
        base._ExitTree();
    }
    
    #endregion
}