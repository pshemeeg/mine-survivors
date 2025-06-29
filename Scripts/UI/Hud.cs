using Godot;
using MineSurvivors.scripts.player;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Enhanced HUD z poprawionym paskiem XP.
    /// Pasek XP teraz ma tekst bezpośrednio na sobie i zapełnia się na fioletowo.
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
        
        // Health Bar Label (zachowujemy)
        private Label _healthValueLabel; // "100/100"
        
        // USUNIĘTO: _expValueLabel - tekst będzie bezpośrednio na pasku
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja Enhanced HUD z fioletowym paskiem XP...");
            
            // Znajdź wszystkie komponenty
            FindHudComponents();
            
            // Skonfiguruj pasek XP
            SetupExpBar();
            
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
            
            // Health Bar setup (bez zmian)
            var healthContainer = bottomContainer.GetNode("HealthContainer");
            _healthBar = healthContainer.GetNode<ProgressBar>("HealthBar");
            _healthValueLabel = healthContainer.GetNode<Label>("HealthValue");
            
            // Experience Bar setup - ZMIANA: nie szukamy osobnego labela
            var expContainer = GetNode("MarginContainer/VBoxContainer/ExpContainer");
            _expBar = expContainer.GetNode<ProgressBar>("ExpBar");
            
            // Walidacja
            ValidateComponents();
        }

        /// <summary>
        /// NOWA METODA: Konfiguracja paska XP
        /// </summary>
        private void SetupExpBar()
        {
            if (_expBar == null) return;

            // KLUCZOWA ZMIANA: Ustaw kolor tła i wypełnienia paska XP
            
            // Fioletowy kolor wypełnienia (Tokyo Night accent)
            var fillStyleBox = new StyleBoxFlat();
            fillStyleBox.BgColor = new Color("#BB9AF7"); // Tokyo Night violet
            fillStyleBox.CornerRadiusTopLeft = 4;
            fillStyleBox.CornerRadiusTopRight = 4;
            fillStyleBox.CornerRadiusBottomLeft = 4;
            fillStyleBox.CornerRadiusBottomRight = 4;
            
            // Ciemne tło paska
            var backgroundStyleBox = new StyleBoxFlat();
            backgroundStyleBox.BgColor = new Color("#1A1B26"); // Tokyo Night dark
            backgroundStyleBox.CornerRadiusTopLeft = 4;
            backgroundStyleBox.CornerRadiusTopRight = 4;
            backgroundStyleBox.CornerRadiusBottomLeft = 4;
            backgroundStyleBox.CornerRadiusBottomRight = 4;
            
            // Zastosuj style do paska
            _expBar.AddThemeStyleboxOverride("fill", fillStyleBox);
            _expBar.AddThemeStyleboxOverride("background", backgroundStyleBox);
            
            // WAŻNE: Ustaw sposób wyświetlania tekstu na pasku
            _expBar.ShowPercentage = false; // Wyłącz domyślny procent
            
            GD.Print("Pasek XP skonfigurowany z fioletowym wypełnieniem");
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
            
            // Experience
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
        /// Event Handler: Zdrowie gracza się zmieniło (bez zmian)
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
        /// Event Handler: Doświadczenie gracza się zmieniło - POPRAWIONA WERSJA
        /// </summary>
        private void OnPlayerExperienceChanged(float gainedExp, float totalExp)
        {
            if (_expBar == null) return;

            // Oblicz wymagane XP dla obecnego poziomu
            var currentLevel = (int)(totalExp / 100f) + 1;
            var expInCurrentLevel = totalExp % 100f;
            var expNeededForLevel = 100f;
            
            _expBar.MaxValue = expNeededForLevel;
            _expBar.Value = expInCurrentLevel;
            
            // KLUCZOWA ZMIANA: Ustaw tekst bezpośrednio na pasku XP
            UpdateExpBarText(expInCurrentLevel, expNeededForLevel);
        }

        /// <summary>
        /// Event Handler: Gracz awansował na poziom (bez zmian)
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
                UpdateExpBarText(0f, 100f); // Reset tekstu na pasku
            }
            
            // Animacja level up (opcjonalnie)
            AnimateLevelUp();
        }

        #endregion

        #region XP Bar Text Management - NOWA SEKCJA
        
        /// <summary>
        /// NOWA METODA: Aktualizacja tekstu bezpośrednio na pasku XP
        /// Używa override tekstu w ProgressBar
        /// </summary>
        private void UpdateExpBarText(float currentExp, float maxExp)
        {
            if (_expBar == null) return;
            
            // Stwórz tekst do wyświetlenia na pasku
            string expText = $"XP: {currentExp:F0}/{maxExp:F0}";
            
            // METODA 1: Godot 4 pozwala na custom text w ProgressBar
            // Ale musimy to zrobić przez theme override
            
            // Stwórz label jako child paska XP jeśli nie istnieje
            var existingLabel = _expBar.GetNodeOrNull<Label>("ExpText");
            if (existingLabel == null)
            {
                CreateExpBarLabel();
                existingLabel = _expBar.GetNodeOrNull<Label>("ExpText");
            }
            
            // Zaktualizuj tekst
            if (existingLabel != null)
            {
                existingLabel.Text = expText;
            }
        }
        
        /// <summary>
        /// NOWA METODA: Tworzenie labela na pasku XP
        /// </summary>
        private void CreateExpBarLabel()
        {
            if (_expBar == null) return;
            
            var expLabel = new Label();
            expLabel.Name = "ExpText";
            expLabel.Text = "XP: 0/100";
            
            // Pozycjonowanie: wyśrodkowany na pasku
            expLabel.AnchorLeft = 0f;
            expLabel.AnchorTop = 0f;
            expLabel.AnchorRight = 1f;
            expLabel.AnchorBottom = 1f;
            expLabel.HorizontalAlignment = HorizontalAlignment.Center;
            expLabel.VerticalAlignment = VerticalAlignment.Center;
            
            // Styl tekstu - biały z cieniem dla czytelności
            expLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1)); // Biały
            expLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0)); // Czarny
            expLabel.AddThemeConstantOverride("shadow_offset_x", 1);
            expLabel.AddThemeConstantOverride("shadow_offset_y", 1);
            
            // Dodaj jako child paska XP
            _expBar.AddChild(expLabel);
            
            GD.Print("Label tekstu XP utworzony na pasku");
        }
        
        #endregion

        #region Visual Effects - Hermetyzacja efektów wizualnych
        
        /// <summary>
        /// Hermetyzacja: Zmiana koloru paska zdrowia (bez zmian)
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
        /// Hermetyzacja: Animacja level up (bez zmian)
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

        #region Process - Dla dodatkowych efektów XP bara
        
        public override void _Process(double delta)
        {
            // Opcjonalnie: Subtelna animacja fioletowego paska XP
            if (_expBar != null && _expBar.Value > 0)
            {
                AnimateExpBarGlow(delta);
            }
        }
        
        /// <summary>
        /// NOWA METODA: Subtelna animacja świecenia paska XP
        /// </summary>
        private void AnimateExpBarGlow(double delta)
        {
            // Subtelne pulsowanie fioletowego koloru
            float pulseSpeed = 2.0f;
            float pulseIntensity = 0.1f;
            
            float currentTime = (float)Time.GetUnixTimeFromSystem() * pulseSpeed;
            float pulse = Mathf.Sin(currentTime) * pulseIntensity;
            
            // Modyfikuj odcień fioletowego
            Color baseColor = new Color("#BB9AF7");
            Color glowColor = baseColor.Lightened(pulse);
            
            // Zastosuj do modulate paska (subtelnie)
            _expBar.Modulate = new Color(1, 1, 1).Lerp(glowColor, 0.3f);
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