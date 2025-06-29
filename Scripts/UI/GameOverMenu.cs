using Godot;

using MineSurvivors.scripts.managers;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Menu końca gry - demonstracja hermetyzacji statystyk i zarządzania wynikami.
    /// 
    /// Zasady OOP:
    /// - Hermetyzacja: Statystyki gry są prywatne, dostęp przez publiczne metody
    /// - Enkapsulacja: Logika wyników jest oddzielona od logiki gry
    /// - Kompozycja: Używa GameStats jako osobny obiekt do zarządzania danymi
    /// - Separacja odpowiedzialności: Menu zajmuje się tylko prezentacją wyników
    /// </summary>
    public partial class GameOverMenu : Control
    {
        #region Private UI Components - Hermetyzacja
        
        private Label _titleLabel;
        private Label _survivalTimeLabel;
        private Label _enemiesKilledLabel;
        private Label _experienceGainedLabel;
        private Label _levelReachedLabel;
        private Label _scoreLabel;
        
        private Button _restartButton;
        private Button _mainMenuButton;
        private Button _quitButton;
        
        // Panel dla animacji
        private AnimationPlayer _animationPlayer;
        
        // Ścieżki scen
        private const string GameScenePath = "res://scenes/Game/GameScene.tscn";
        private const string MainMenuPath = "res://scenes/UI/MainMenu.tscn";
        
        #endregion

        #region Game Statistics - Hermetyzacja danych
        
        /// <summary>
        /// Prywatna struktura do przechowywania statystyk gry
        /// Hermetyzacja: Dane są ukryte, dostęp tylko przez metody publiczne
        /// </summary>
        private struct GameStats
        {
            public float SurvivalTime;
            public int EnemiesKilled;
            public float ExperienceGained;
            public int LevelReached;
            public int FinalScore;
        }
        
        private GameStats _currentStats;
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja menu końca gry...");
            
            // Znajdź komponenty UI
            FindUiComponents();
            
            // Skonfiguruj przyciski
            SetupButtons();
            
            // Ukryj menu na start
            Hide();
            
            GD.Print("Menu końca gry gotowe!");
        }

        /// <summary>
        /// Hermetyzacja: Znajdowanie komponentów UI
        /// </summary>
        private void FindUiComponents()
        {
            var container = GetNode("PanelContainer/VBoxContainer");
            
            _titleLabel = container.GetNode<Label>("TitleLabel");
            
            var statsContainer = container.GetNode("StatsContainer");
            _survivalTimeLabel = statsContainer.GetNode<Label>("SurvivalTimeLabel");
            _enemiesKilledLabel = statsContainer.GetNode<Label>("EnemiesKilledLabel");
            _experienceGainedLabel = statsContainer.GetNode<Label>("ExperienceGainedLabel");
            _levelReachedLabel = statsContainer.GetNode<Label>("LevelReachedLabel");
            _scoreLabel = statsContainer.GetNode<Label>("ScoreLabel");
            
            var buttonContainer = container.GetNode("ButtonContainer");
            _restartButton = buttonContainer.GetNode<Button>("RestartButton");
            _mainMenuButton = buttonContainer.GetNode<Button>("MainMenuButton");
            _quitButton = buttonContainer.GetNode<Button>("QuitButton");
            
            // Animacja (opcjonalna)
            _animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_restartButton == null) GD.PrintErr("BŁĄD: Brak RestartButton!");
            if (_mainMenuButton == null) GD.PrintErr("BŁĄD: Brak MainMenuButton!");
            if (_scoreLabel == null) GD.PrintErr("BŁĄD: Brak ScoreLabel!");
        }

        /// <summary>
        /// Hermetyzacja: Konfiguracja przycisków
        /// </summary>
        private void SetupButtons()
        {
            if (_restartButton != null)
                _restartButton.Pressed += OnRestartPressed;
                
            if (_mainMenuButton != null)
                _mainMenuButton.Pressed += OnMainMenuPressed;
                
            if (_quitButton != null)
                _quitButton.Pressed += OnQuitPressed;
        }

        #endregion

        #region Public API - Interface do pokazywania wyników
        
        /// <summary>
        /// Publiczny interfejs: Pokaż menu końca gry z statystykami
        /// Enkapsulacja: Zewnętrzne systemy przekazują dane, ale nie znają implementacji
        /// </summary>
        /// <param name="survivalTime">Czas przeżycia w sekundach</param>
        /// <param name="enemiesKilled">Liczba zabitych wrogów</param>
        /// <param name="experienceGained">Zdobyte doświadczenie</param>
        /// <param name="levelReached">Osiągnięty poziom</param>
        public void ShowGameOver(float survivalTime, int enemiesKilled, float experienceGained, int levelReached)
        {
            // Zapisz statystyki
            _currentStats = new GameStats
            {
                SurvivalTime = survivalTime,
                EnemiesKilled = enemiesKilled,
                ExperienceGained = experienceGained,
                LevelReached = levelReached,
                FinalScore = CalculateScore(survivalTime, enemiesKilled, experienceGained, levelReached)
            };
            
            // Zaktualizuj UI
            UpdateStatsDisplay();
            
            // Pokaż menu
            Show();
            
            // Focus na restart dla łatwiejszej nawigacji
            _restartButton?.GrabFocus();
            
            GD.Print($"Game Over! Score: {_currentStats.FinalScore}");
            
            //Zapis wyników do pliku
            SaveScoreAndUpdateTitle();
        }

        /// <summary>
        /// Publiczny interfejs: Szybkie pokazanie game over bez szczegółów
        /// Przydatne dla debug lub prostszych przypadków
        /// </summary>
        public void ShowGameOver()
        {
            ShowGameOver(0f, 0, 0f, 1);
        }

        #endregion

        #region Private Methods - Hermetyzacja logiki
        
        /// <summary>
        /// Hermetyzacja: Obliczanie wyniku na podstawie statystyk
        /// Algorytm jest ukryty przed zewnętrznymi systemami
        /// </summary>
        private int CalculateScore(float survivalTime, int enemiesKilled, float experience, int level)
        {
            // Prosty algorytm scoringu - można łatwo modyfikować
            float timePoints = survivalTime * 10f; // 10 punktów za sekundę
            float killPoints = enemiesKilled * 50f; // 50 punktów za wroga
            float expPoints = experience * 2f; // 2 punkty za XP
            float levelBonus = (level - 1) * 200f; // 200 punktów za poziom
            
            return Mathf.RoundToInt(timePoints + killPoints + expPoints + levelBonus);
        }

        /// <summary>
        /// Hermetyzacja: Aktualizacja wyświetlanych statystyk
        /// </summary>
        private void UpdateStatsDisplay()
        {
            // Format czasu przeżycia
            string timeString = FormatTime(_currentStats.SurvivalTime);
            
            // Aktualizuj etykiety
            if (_survivalTimeLabel != null)
                _survivalTimeLabel.Text = $"Czas przeżycia: {timeString}";
                
            if (_enemiesKilledLabel != null)
                _enemiesKilledLabel.Text = $"Wrogów pokonanych: {_currentStats.EnemiesKilled}";
                
            if (_experienceGainedLabel != null)
                _experienceGainedLabel.Text = $"Doświadczenie: {_currentStats.ExperienceGained:F0} XP";
                
            if (_levelReachedLabel != null)
                _levelReachedLabel.Text = $"Osiągnięty poziom: {_currentStats.LevelReached}";
                
            if (_scoreLabel != null)
                _scoreLabel.Text = $"WYNIK: {_currentStats.FinalScore:N0}";
        }

        /// <summary>
        /// Helper: Formatowanie czasu do czytelnej formy
        /// </summary>
        private string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60);
            int remainingSeconds = (int)(seconds % 60);
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }

        #endregion

        #region Event Handlers - Obsługa przycisków
        
        /// <summary>
        /// Restart gry - powrót do gry z czystym stanem
        /// </summary>
        private void OnRestartPressed()
        {
            GD.Print("Restartowanie gry...");
            
            // Reset pauzy (gdyby była aktywna)
            GetTree().Paused = false;
            
            // Przeładuj scenę gry
            GetTree().ChangeSceneToFile(GameScenePath);
        }

        /// <summary>
        /// Powrót do głównego menu
        /// </summary>
        private void OnMainMenuPressed()
        {
            GD.Print("Powrót do głównego menu...");
            
            // Reset pauzy
            GetTree().Paused = false;
            
            // Przejdź do głównego menu
            GetTree().ChangeSceneToFile(MainMenuPath);
        }

        /// <summary>
        /// Zamknij grę
        /// </summary>
        private void OnQuitPressed()
        {
            GD.Print("Zamykanie gry...");
            
            // Reset pauzy przed zamknięciem
            GetTree().Paused = false;
            
            // Zamknij grę
            GetTree().Quit();
        }

        #endregion

        #region Data Persistence - Opcjonalne zarządzanie high scores
        
        private void SaveScoreAndUpdateTitle()
        {
            // Pobierz ScoreManager przez Autoload
            var scoreManager = GetNode<MineSurvivors.scripts.managers.ScoreManager>("/root/ScoreManager");
            if (scoreManager == null) return;
    
            // Sprawdź czy to high score
            bool isHighScore = scoreManager.IsHighScore(_currentStats.FinalScore);
    
            if (isHighScore)
            {
                // Dodaj do rankingu
                int position = scoreManager.AddScore(
                    _currentStats.SurvivalTime, 
                    _currentStats.EnemiesKilled, 
                    _currentStats.LevelReached);
        
                if (position > 0)
                {
                    // AKTUALIZUJ TITLE LABEL!
                    UpdateTitleForHighScore(position);
                }
            }
        }

        private void UpdateTitleForHighScore(int position)
        {
            if (_titleLabel == null) return;
    
            // Określ message na podstawie pozycji
            string congratsMessage = position switch
            {
                1 => "🏆 NOWY REKORD! 🏆",
                2 => "🥈 2. MIEJSCE! 🥈", 
                3 => "🥉 3. MIEJSCE! 🥉",
                <= 10 => $"🎉 TOP 10! ({position}. miejsce) 🎉",
                _ => "Game Over" // Fallback
            };
    
            // Ustaw nowy tekst w title label
            _titleLabel.Text = congratsMessage;
        }

        #endregion

        #region Input Handling
        
        /// <summary>
        /// Obsługa klawiatury - szybki restart lub quit
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("restart"))
            {
                OnRestartPressed();
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                OnMainMenuPressed();
            }
        }

        #endregion
    }
}