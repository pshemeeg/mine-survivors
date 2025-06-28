using Godot;
using MineSurvivors.scripts.player;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Enhanced HUD - zarządzanie wszystkimi elementami interfejsu gry.
    /// Demonstracja hermetyzacji UI i loose coupling z Player.
    /// 
    /// Zasady OOP:
    /// - Hermetyzacja: Wszystkie UI elementy są prywatne
    /// - Loose Coupling: HUD obserwuje Player przez sygnały, nie bezpośrednio
    /// - Separacja odpowiedzialności: HUD tylko wyświetla, nie przetwarza logiki
    /// </summary>
    public partial class Hud : Control
    {
        #region Private UI Components - Hermetyzacja
        
        // Labels
        private Label _timeLabel;
        private Label _killCountLabel;
        private Label _levelLabel;
        
        // Progress Bars
        private ProgressBar _healthBar;
        private ProgressBar _expBar;
        
        // Health Bar Labels
        private Label _healthValueLabel; // "100/100"
        private Label _expValueLabel; // "XP: 450/1000"
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja Enhanced HUD...");
            
            // Znajdź wszystkie komponenty
            FindHudComponents();
            
            // Połącz z Player jeśli istnieje
            ConnectToPlayer();
            
            GD.Print("Enhanced HUD gotowy!");
        }

        /// <summary>
        /// Hermetyzacja: Znajdowanie komponentów HUD
        /// </summary>
        private void FindHudComponents()
        {
            // Top info (czas, zabójstwa, level)
            var topContainer = GetNode("MarginContainer/VBoxContainer/TopInfo");
            _timeLabel = topContainer.GetNode<Label>("TimeLabel");
            _killCountLabel = topContainer.GetNode<Label>("KillCountLabel");
            _levelLabel = topContainer.GetNode<Label>("LevelLabel");
            
            // Bottom bars (HP, EXP)
            var bottomContainer = GetNode("MarginContainer/VBoxContainer/BottomBars");
            
            // Health Bar setup
            var healthContainer = bottomContainer.GetNode("HealthContainer");
            _healthBar = healthContainer.GetNode<ProgressBar>("HealthBar");
            _healthValueLabel = healthContainer.GetNode<Label>("HealthValue");
            
            // Experience Bar setup
            var expContainer = bottomContainer.GetNode("ExpContainer");
            _expBar = expContainer.GetNode<ProgressBar>("ExpBar");
            _expValueLabel = expContainer.GetNode<Label>("ExpValue");
            
            // Walidacja
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_timeLabel == null) GD.PrintErr("BŁĄD: Brak TimeLabel w HUD!");
            if (_healthBar == null) GD.PrintErr("BŁĄD: Brak HealthBar w HUD!");
            if (_expBar == null) GD.PrintErr("BŁĄD: Brak ExpBar w HUD!");
        }

        /// <summary>
        /// Loose Coupling: Połączenie z Player przez sygnały
        /// </summary>
        private void ConnectToPlayer()
        {
            var player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (player != null)
            {
                // Podłącz sygnały Player'a
                player.HealthChanged += OnPlayerHealthChanged;
                player.ExperienceGained += OnPlayerExperienceChanged;
                player.LevelUp += OnPlayerLevelUp;
                
                // Inicjalne wartości
                InitializePlayerStats(player);
                
                GD.Print("HUD połączony z Player przez sygnały");
            }
            else
            {
                GD.PrintErr("HUD nie znalazł Player w grupie 'player'!");
            }
        }

        /// <summary>
        /// Inicjalizacja początkowych wartości z Player
        /// </summary>
        private void InitializePlayerStats(Player player)
        {
            // Health
            OnPlayerHealthChanged(player.CurrentHealth, player.MaxHealth);
            
            // Experience (jeśli ma property)
            OnPlayerExperienceChanged(0f, player.Experience);
            
            // Level
            OnPlayerLevelUp(player.Level);
        }

        #endregion

        #region Public Update Methods - Interface dla GameManager
        
        /// <summary>
        /// Publiczny interfejs: Aktualizuj czas przeżycia
        /// </summary>
        public void UpdateSurvivalTime(float survivalTime)
        {
            if (_timeLabel != null)
            {
                var minutes = (int)(survivalTime / 60);
                var seconds = (int)(survivalTime % 60);
                _timeLabel.Text = $"Czas: {minutes:D2}:{seconds:D2}";
            }
        }

        /// <summary>
        /// Publiczny interfejs: Aktualizuj licznik zabójstw
        /// </summary>
        public void UpdateKillCount(int killCount)
        {
            if (_killCountLabel != null)
            {
                _killCountLabel.Text = $"Wrogów: {killCount}";
            }
        }

        #endregion

        #region Signal Handlers - Loose Coupling z Player
        
        /// <summary>
        /// Event Handler: Zdrowie gracza się zmieniło
        /// </summary>
        private void OnPlayerHealthChanged(float currentHealth, float maxHealth)
        {
            if (_healthBar != null)
            {
                _healthBar.MaxValue = maxHealth;
                _healthBar.Value = currentHealth;
                
                // Zmiana koloru w zależności od HP
                UpdateHealthBarColor(currentHealth / maxHealth);
            }
            
            if (_healthValueLabel != null)
            {
                _healthValueLabel.Text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
        }

        /// <summary>
        /// Event Handler: Doświadczenie gracza się zmieniło
        /// </summary>
        private void OnPlayerExperienceChanged(float gainedExp, float totalExp)
        {
            if (_expBar != null)
            {
                // Oblicz wymagane XP dla obecnego poziomu
                var currentLevel = (int)(totalExp / 100f) + 1;
                var expInCurrentLevel = totalExp % 100f;
                var expNeededForLevel = 100f;
                
                _expBar.MaxValue = expNeededForLevel;
                _expBar.Value = expInCurrentLevel;
            }
            
            if (_expValueLabel != null)
            {
                var expInLevel = totalExp % 100f;
                _expValueLabel.Text = $"XP: {expInLevel:F0}/100";
            }
        }

        /// <summary>
        /// Event Handler: Gracz awansował na poziom
        /// </summary>
        private void OnPlayerLevelUp(int newLevel)
        {
            if (_levelLabel != null)
            {
                _levelLabel.Text = $"Poziom: {newLevel}";
            }
            
            // Reset exp bar dla nowego poziomu
            if (_expBar != null)
            {
                _expBar.Value = 0;
            }
            
            // Animacja level up (opcjonalnie)
            AnimateLevelUp();
        }

        #endregion

        #region Visual Effects - Hermetyzacja efektów wizualnych
        
        /// <summary>
        /// Hermetyzacja: Zmiana koloru paska zdrowia
        /// </summary>
        private void UpdateHealthBarColor(float healthPercent)
        {
            if (_healthBar == null) return;
            
            // Zmiana koloru w zależności od procentu HP
            Color barColor;
            if (healthPercent > 0.6f)
                barColor = Colors.Green;
            else if (healthPercent > 0.3f)
                barColor = Colors.Yellow;
            else
                barColor = Colors.Red;
            
            // Aplikuj kolor do progress bar
            _healthBar.Modulate = barColor;
        }

        /// <summary>
        /// Hermetyzacja: Animacja level up
        /// </summary>
        private void AnimateLevelUp()
        {
            if (_levelLabel == null) return;
            
            // Prosta animacja scale
            var tween = CreateTween();
            tween.TweenProperty(_levelLabel, "scale", Vector2.One * 1.2f, 0.2f);
            tween.TweenProperty(_levelLabel, "scale", Vector2.One, 0.2f);
            
            // Efekt kolorowy
            var colorTween = CreateTween();
            colorTween.TweenProperty(_levelLabel, "modulate", Colors.Gold, 0.3f);
            colorTween.TweenProperty(_levelLabel, "modulate", Colors.White, 0.3f);
        }

        #endregion

        #region Cleanup
        
        public override void _ExitTree()
        {
            // Rozłącz sygnały
            var player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (player != null)
            {
                // Godot automatycznie rozłącza sygnały, ale dobra praktyka to explicit disconnect
                if (player.IsConnected(Player.SignalName.HealthChanged, Callable.From<float, float>(OnPlayerHealthChanged)))
                    player.HealthChanged -= OnPlayerHealthChanged;
                    
                if (player.IsConnected(Player.SignalName.ExperienceGained, Callable.From<float, float>(OnPlayerExperienceChanged)))
                    player.ExperienceGained -= OnPlayerExperienceChanged;
                    
                if (player.IsConnected(Player.SignalName.LevelUp, Callable.From<int>(OnPlayerLevelUp)))
                    player.LevelUp -= OnPlayerLevelUp;
            }
            
            base._ExitTree();
        }

        #endregion
    }
}