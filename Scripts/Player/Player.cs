using Godot;
using MineSurvivors.scripts.interfaces;

namespace MineSurvivors.scripts.player
{
    public partial class Player : CharacterBody2D, IDamageable
    {
        // Statystyki bazowe - widoczne w Godot Inspector dla łatwego balansowania
        [ExportGroup("Base Stats")]
        [Export] public float BaseSpeed = 200f;
        [Export] public float BaseDamage = 10f;
        [Export] public float BaseDefense = 1f;
        [Export] public float BaseMaxHealth = 100f;
        
        // Bonusy z levelowania - kontrolowane przez kod, nie przez edytor
        public float SpeedBonus { get; private set; }
        public float DamageBonus { get; private set; }
        public float DefenseBonus { get; private set; }
        public float HealthBonus { get; private set; }
        
        // Finalne wartości - automatycznie obliczane na podstawie bazowych i bonusów
        public float TotalSpeed => BaseSpeed + SpeedBonus;
        public float TotalDamage => BaseDamage + DamageBonus;
        public float TotalDefense => BaseDefense + DefenseBonus;
        
        // Implementacja IDamageable
        public float MaxHealth => BaseMaxHealth + HealthBonus;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        
        // System doświadczenia i poziomów
        public float Experience { get; private set; }
        public int Level { get; private set; } = 1;
        
        // Komponenty Godot
        private AnimatedSprite2D _sprite;
        
        // System uników (dodge roll)
        [ExportGroup("Combat")]
        [Export] private float _rollSpeed = 400f;
        [Export] private float _rollCooldown = 1.0f;
        [Export] private bool _invulnerableDuringRoll = true;
        // NOWE: Eksportowane wartości dla czasów animacji/stanów
        [Export] private float _rollAnimationDuration = 0.9f; // Przykład: ustal czas animacji rolla
        [Export] private float _hurtAnimationDuration = 0.2f; // Przykład: ustal czas animacji hurt
        
        // Stany ruchu - KLUCZOWA ZMIANA: enum dla lepszego zarządzania stanami
        private enum PlayerState
        {
            Normal,
            Rolling,
            Hurt,
            Dead
        }
        
        private PlayerState _currentState = PlayerState.Normal;
        private float _rollCooldownTimer; // Zmieniona nazwa dla jasności
        private Vector2 _rollDirection;
        private float _stateExitTimer; // Timer do wymuszonego wyjścia ze stanu (jako fallback)
        
        // Sygnały
        [Signal] public delegate void DiedEventHandler();
        [Signal] public delegate void HealthChangedEventHandler(float currentHealth, float maxHealth);
        [Signal] public delegate void ExperienceGainedEventHandler(float experience, float totalExperience);
        [Signal] public delegate void LevelUpEventHandler(int newLevel);
        
        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            AddToGroup("player");
            _sprite.AnimationFinished += OnAnimationFinished;
        }
        
        public override void _PhysicsProcess(double delta)
        {
            // Aktualizacja timerów
            if (_rollCooldownTimer > 0) _rollCooldownTimer -= (float)delta;
            if (_stateExitTimer > 0) _stateExitTimer -= (float)delta;
            
            // Obsługa stanów
            HandleStateLogic();
            
            // Input i ruch tylko gdy gracz żyje i nie jest w stanie blokującym
            if (IsAlive && CanMove())
            {
                HandleInput();
                HandleMovement();
            }
            
            MoveAndSlide();
        }
        
        // NOWA METODA: Centralne zarządzanie logiką stanów
        private void HandleStateLogic()
        {
            switch (_currentState)
            {
                case PlayerState.Rolling:
                    // Roll powinien zakończyć się głównie przez AnimationFinished
                    // stateExitTimer to bezpiecznik, gdyby animacja się zacięła
                    if (_stateExitTimer <= 0)
                    {
                        EndRoll(); // Wywołaj EndRoll nawet jeśli animacja się nie skończyła
                    }
                    break;
                    
                case PlayerState.Hurt:
                    // Stan hurt powinien zakończyć się głównie przez AnimationFinished
                    // stateExitTimer to bezpiecznik
                    if (_stateExitTimer <= 0)
                    {
                        ChangeState(PlayerState.Normal);
                    }
                    break;
                    
                case PlayerState.Dead:
                    // W stanie śmierci gracz nie może się ruszać
                    Velocity = Vector2.Zero;
                    break;
            }
        }
        
        // NOWA METODA: Sprawdzenie czy gracz może się poruszać
        private bool CanMove()
        {
            return _currentState == PlayerState.Normal || _currentState == PlayerState.Rolling;
        }
        
        // NOWA METODA: Bezpieczna zmiana stanu
        private void ChangeState(PlayerState newState)
        {
            // Jeśli gracz nie żyje, może przejść tylko do stanu Dead
            if (!IsAlive && newState != PlayerState.Dead)
                return;
                
            // Zapobieganie zmianie na ten sam stan, chyba że to konieczne (np. reset)
            if (_currentState == newState && newState != PlayerState.Dead) // Pozwól na wielokrotne ustawienie Dead
                return;
            
            _currentState = newState;
            
            // Reset timerów przy zmianie stanu
            _stateExitTimer = 0f; // Zawsze resetuj przy zmianie stanu
            
            GD.Print($"Player state changed to: {newState}");
        }
        
        private void HandleInput()
        {
            // Dodge roll - tylko w normalnym stanie i gdy cooldown minął
            if (Input.IsActionJustPressed("dodge") && 
                _currentState == PlayerState.Normal && 
                _rollCooldownTimer <= 0)
            {
                StartRoll();
            }
        }
        
        private void HandleMovement()
        {
            switch (_currentState)
            {
                case PlayerState.Normal:
                    // Normalny ruch na podstawie input'u
                    Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
                    Velocity = input * TotalSpeed;
                    UpdateAnimation(input);
                    break;
                    
                case PlayerState.Rolling:
                    // Kontynuuj ruch roll'a
                    Velocity = _rollDirection * _rollSpeed;
                    break;
                    
                case PlayerState.Hurt:
                    // Podczas hurt gracza lekko spowalnia (można też całkowicie zatrzymać)
                    Velocity = Velocity * 0.5f; 
                    break;
                    
                default:
                    Velocity = Vector2.Zero;
                    break;
            }
        }
        
        private void UpdateAnimation(Vector2 input)
        {
            // Nie zmieniaj animacji jeśli w trakcie specjalnego stanu
            if (_currentState != PlayerState.Normal)
                return;
                
            if (input != Vector2.Zero)
            {
                _sprite.Play("run");
                _sprite.FlipH = input.X < 0;
            }
            else
            {
                _sprite.Play("idle");
            }
        }
        
        // POPRAWIONA METODA: Rozpoczęcie roll'a
        private void StartRoll()
        {
            Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            _rollDirection = input != Vector2.Zero ? input.Normalized() : Vector2.Down;
            
            ChangeState(PlayerState.Rolling);
            _rollCooldownTimer = _rollCooldown;
            _stateExitTimer = _rollAnimationDuration; // Ustaw timer na czas animacji (jako bezpiecznik)
            _sprite.Play("roll");
        }
        
        // NOWA METODA: Zakończenie roll'a
        private void EndRoll()
        {
            if (_currentState == PlayerState.Rolling) // Upewnij się, że nadal jesteśmy w stanie Rolling
            {
                ChangeState(PlayerState.Normal);
            }
        }
        
        private void OnAnimationFinished()
        {
            string animName = _sprite.Animation;
            
            if (animName == "roll")
            {
                // Zakończ rolla po zakończeniu animacji
                EndRoll();
            }
            else if (animName == "hurt")
            {
                // Zakończ stan hurt po zakończeniu animacji
                if (_currentState == PlayerState.Hurt)
                {
                    ChangeState(PlayerState.Normal);
                }
            }
            // Możesz dodać inne animacje, które po zakończeniu zmieniają stan
            else if (animName == "death")
            {
                // Po animacji śmierci gracz pozostaje w stanie Dead
                // Możesz dodać logikę np. ukrycia gracza po dłuższym czasie
            }
        }
        
        // POPRAWIONA METODA: TakeDamage z lepszym zarządzaniem stanami
        public void TakeDamage(float damage)
        {
            // Sprawdzenie warunków otrzymania obrażeń
            if (!IsAlive)
                return;
                
            // Sprawdzenie nietykalności podczas roll'a
            if (_currentState == PlayerState.Rolling && _invulnerableDuringRoll)
                return;
            
            // Zastosowanie systemu obrony gracza
            float actualDamage = Mathf.Max(0, damage - TotalDefense);
            CurrentHealth = Mathf.Max(0, CurrentHealth - actualDamage);
            
            // Powiadomienie o zmianie zdrowia
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            
            // Sprawdzenie śmierci
            if (!IsAlive)
            {
                Die();
                return;
            }
            
            // KLUCZOWA ZMIANA: Hurt tylko jeśli nie w trakcie roll'a i nie jesteśmy już Hurt
            if (_currentState != PlayerState.Rolling && _currentState != PlayerState.Hurt)
            {
                ChangeState(PlayerState.Hurt);
                _stateExitTimer = _hurtAnimationDuration; // Ustaw timer na czas animacji hurt (jako bezpiecznik)
                _sprite.Play("hurt");
            }
        }
        
        // POPRAWIONA METODA: Die z lepszym zarządzaniem stanu
        private void Die()
        {
            CurrentHealth = 0;
            ChangeState(PlayerState.Dead);
            _sprite.Play("death");
            EmitSignal(SignalName.Died);
        }
        
        // Metody bonusów (bez zmian)
        public void AddSpeedBonus(float bonus) 
        {
            SpeedBonus += bonus;
            GD.Print($"Speed bonus: +{bonus}. Total speed: {TotalSpeed}");
        }
        
        public void AddDamageBonus(float bonus) 
        {
            DamageBonus += bonus;
            GD.Print($"Damage bonus: +{bonus}. Total damage: {TotalDamage}");
        }
        
        public void AddDefenseBonus(float bonus) 
        {
            DefenseBonus += bonus;
            GD.Print($"Defense bonus: +{bonus}. Total defense: {TotalDefense}");
        }
        
        public void AddHealthBonus(float bonus) 
        {
            HealthBonus += bonus;
            float healAmount = bonus;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healAmount);
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            GD.Print($"Health bonus: +{bonus}. Total max health: {MaxHealth}");
        }
        
        public void ResetBonuses()
        {
            SpeedBonus = DamageBonus = DefenseBonus = HealthBonus = 0f;
            CurrentHealth = MaxHealth;
            Experience = 0f;
            Level = 1;
            ChangeState(PlayerState.Normal); // Reset stanu przy nowej grze
        }
        
        public void GainExperience(float amount)
        {
            Experience += amount;
            EmitSignal(SignalName.ExperienceGained, amount, Experience);
            CheckLevelUp();
        }
        
        private void CheckLevelUp()
        {
            int newLevel = (int)(Experience / 100f) + 1;
            if (newLevel > Level)
            {
                Level = newLevel;
                EmitSignal(SignalName.LevelUp, Level);
                GD.Print($"Level up! New level: {Level}");
            }
        }
        
        // NOWA METODA: Debugging — sprawdzenie aktualnego stanu
        public string GetCurrentState()
        {
            return _currentState.ToString();
        }
    }
}