using Godot;
using MineSurvivors.scripts.player;
using MineSurvivors.scripts.ui;

namespace MineSurvivors.scripts.managers
{
    /// <summary>
    /// Rozszerzony GameManager - centralny punkt zarządzania grą i UI.
    /// Demonstracja wzorca Singleton i Mediator.
    /// 
    /// Zasady OOP w działaniu:
    /// - Singleton: Jedna instancja w całej grze
    /// - Mediator: Pośredniczy między systemami (Player ↔ UI ↔ Game Logic)
    /// - Loose Coupling: Systemy nie znają się nawzajem, komunikują przez GameManager
    /// - Hermetyzacja: Stan gry jest ukryty, dostęp przez publiczne metody
    /// - Separacja odpowiedzialności: Tylko orkiestracja, nie implementacja szczegółów
    /// </summary>
    public partial class GameManager : Node2D
    {
        #region Singleton Pattern - Jedna instancja w grze
        
        /// <summary>
        /// Singleton instance - zapewnia globalny dostęp do GameManager
        /// </summary>
        public static GameManager Instance { get; private set; }
        
        #endregion

        #region Game State - Hermetyzacja stanu gry
        
        /// <summary>
        /// Enum dla stanów gry - czytelne zarządzanie stanem
        /// </summary>
        public enum GameState
        {
            Playing,
            Paused,
            GameOver,
            MainMenu
        }
        
        // Prywatny stan gry - hermetyzacja
        private GameState _currentState = GameState.Playing;
        
        // Statystyki gry - hermetyzacja danych
        private float _gameStartTime;
        private float _survivalTime;
        private int _enemiesKilled;
        private float _totalExperience;
        
        // Publiczny dostęp tylko do odczytu
        public GameState CurrentState => _currentState;
        public float SurvivalTime => _survivalTime;
        public int EnemiesKilled => _enemiesKilled;
        public float TotalExperience => _totalExperience;
        
        #endregion

        #region Component References - Loose Coupling
        
        // Referencje do kluczowych komponentów - znajdowane dynamicznie
        private Player _player;
        private PauseMenu _pauseMenu;
        private GameOverMenu _gameOverMenu;
        
        // UI Elements (HUD)
        private Label _timeLabel;
        private Label _killCountLabel;
        
        #endregion

        #region Initialization - Setup Singleton i referencji
        
        public override void _Ready()
        {
            // Setup Singleton
            if (Instance != null)
            {
                GD.PrintErr("BŁĄD: Próba stworzenia drugiego GameManager! Usuwam duplikat.");
                QueueFree();
                return;
            }
            
            Instance = this;
            GD.Print("GameManager zainicjalizowany jako Singleton");
            
            // Znajdź komponenty
            FindGameComponents();
            
            // Połącz sygnały
            ConnectSignals();
            
            // Inicjalizuj grę
            StartNewGame();
            
            GD.Print("Mine Survivors - Gra gotowa!");
        }

        /// <summary>
        /// Hermetyzacja: Znajdowanie komponentów gry
        /// Używa GetNodeOrNull dla bezpiecznego dostępu
        /// </summary>
        private void FindGameComponents()
        {
            // Znajdź gracza w grupie "player"
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (_player == null)
                GD.PrintErr("UWAGA: Nie znaleziono gracza w grupie 'player'");
            
            // Znajdź UI komponenty
            _pauseMenu = GetNodeOrNull<PauseMenu>("UI/PauseMenu");
            _gameOverMenu = GetNodeOrNull<GameOverMenu>("UI/GameOverMenu");
            
            // Znajdź HUD elementy  
            _timeLabel = GetNodeOrNull<Label>("UI/HUD/TimeLabel");
            _killCountLabel = GetNodeOrNull<Label>("UI/HUD/KillCountLabel");
            
            // Walidacja kluczowych komponentów
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_player == null) GD.PrintErr("BŁĄD: GameManager nie znalazł gracza!");
            if (_pauseMenu == null) GD.PrintErr("UWAGA: Brak PauseMenu - pauza nie będzie działać");
            if (_gameOverMenu == null) GD.PrintErr("UWAGA: Brak GameOverMenu - koniec gry nie będzie działać");
        }

        /// <summary>
        /// Loose Coupling: Podłączenie sygnałów bez tight coupling
        /// </summary>
        private void ConnectSignals()
        {
            if (_player != null)
            {
                // Słuchaj śmierci gracza
                _player.Died += OnPlayerDied;
                
                // Słuchaj zmian w doświadczeniu
                _player.ExperienceGained += OnExperienceGained;
                
                GD.Print("Sygnały gracza podłączone do GameManager");
            }
        }

        #endregion

        #region Game Flow Control - Publiczne API do zarządzania grą
        
        /// <summary>
        /// Publiczny interfejs: Rozpocznij nową grę
        /// </summary>
        public void StartNewGame()
        {
            GD.Print("Rozpoczynanie nowej gry...");
            
            // Reset statystyk
            _gameStartTime = (float)Time.GetUnixTimeFromSystem();
            _survivalTime = 0f;
            _enemiesKilled = 0;
            _totalExperience = 0f;
            
            // Ustaw stan
            ChangeState(GameState.Playing);
            
            // Reset gracza jeśli istnieje
            if (_player != null)
            {
                _player.ResetBonuses();
                GD.Print("Gracz zresetowany");
            }
            
            // Ukryj wszystkie menu
            HideAllMenus();
            
            GD.Print("Nowa gra rozpoczęta!");
        }

        /// <summary>
        /// Publiczny interfejs: Przełącz pauzę
        /// Mediator Pattern - GameManager pośredniczy między input a UI
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.GameOver)
                return; // Nie można pauzować po śmierci
                
            if (_currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (_currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        /// <summary>
        /// Publiczny interfejs: Zapauzuj grę
        /// </summary>
        public void PauseGame()
        {
            if (_currentState != GameState.Playing) return;
            
            ChangeState(GameState.Paused);
            
            if (_pauseMenu != null)
            {
                _pauseMenu.Pause();
            }
            else
            {
                // Fallback jeśli nie ma UI
                GetTree().Paused = true;
            }
            
            GD.Print("Gra zapauzowana przez GameManager");
        }

        /// <summary>
        /// Publiczny interfejs: Wznów grę
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;
            
            ChangeState(GameState.Playing);
            
            if (_pauseMenu != null)
            {
                _pauseMenu.Resume();
            }
            else
            {
                // Fallback
                GetTree().Paused = false;
            }
            
            GD.Print("Gra wznowiona przez GameManager");
        }

        /// <summary>
        /// Publiczny interfejs: Zakończ grę (Game Over)
        /// </summary>
        public void EndGame()
        {
            GD.Print("Kończenie gry...");
            
            ChangeState(GameState.GameOver);
            
            // Pokaż ekran Game Over z statystykami
            if (_gameOverMenu != null)
            {
                var playerLevel = _player?.Level ?? 1;
                _gameOverMenu.ShowGameOver(_survivalTime, _enemiesKilled, _totalExperience, playerLevel);
            }
            else
            {
                GD.Print("Brak GameOverMenu - restart ręcznie!");
            }
        }

        #endregion

        #region Statistics Management - Hermetyzacja statystyk
        
        /// <summary>
        /// Publiczny interfejs: Zarejestruj zabicie wroga
        /// Loose Coupling - Enemy nie musi znać GameManager, może wywołać przez sygnały
        /// </summary>
        public void RegisterEnemyKill()
        {
            _enemiesKilled++;
            UpdateUI();
            GD.Print($"Wrogów zabitych: {_enemiesKilled}");
        }

        /// <summary>
        /// Publiczny interfejs: Dodaj doświadczenie
        /// </summary>
        public void AddExperience(float amount)
        {
            _totalExperience += amount;
            
            // Przekaż do gracza jeśli istnieje
            if (_player != null)
            {
                _player.GainExperience(amount);
            }
        }

        #endregion

        #region Private State Management - Hermetyzacja
        
        /// <summary>
        /// Hermetyzacja: Bezpieczna zmiana stanu gry
        /// </summary>
        private void ChangeState(GameState newState)
        {
            var oldState = _currentState;
            _currentState = newState;
            
            GD.Print($"Stan gry zmieniony: {oldState} → {newState}");
            
            // Można dodać logikę dla transitions między stanami
            OnStateChanged(oldState, newState);
        }

        /// <summary>
        /// Hermetyzacja: Obsługa zmian stanu
        /// </summary>
        private void OnStateChanged(GameState oldState, GameState newState)
        {
            // Logika specific dla transitions
            switch (newState)
            {
                case GameState.Playing:
                    GetTree().Paused = false;
                    break;
                    
                case GameState.Paused:
                    GetTree().Paused = true;
                    break;
                    
                case GameState.GameOver:
                    GetTree().Paused = true; // Zatrzymaj grę
                    break;
            }
        }

        /// <summary>
        /// Hermetyzacja: Ukryj wszystkie menu
        /// </summary>
        private void HideAllMenus()
        {
            _pauseMenu?.Hide();
            _gameOverMenu?.Hide();
        }

        #endregion

        #region Signal Handlers - Loose Coupling przez sygnały
        
        /// <summary>
        /// Event Handler: Gracz umarł
        /// Loose Coupling - Player nie musi znać GameManager bezpośrednio
        /// </summary>
        private void OnPlayerDied()
        {
            GD.Print("GameManager otrzymał sygnał: gracz umarł");
            EndGame();
        }

        /// <summary>
        /// Event Handler: Gracz zdobył doświadczenie
        /// </summary>
        private void OnExperienceGained(float amount, float total)
        {
            // Zaktualizuj wewnętrzne statystyki
            _totalExperience = total;
            UpdateUI();
        }

        #endregion

        #region UI Management - Mediator między logiką a UI
        
        /// <summary>
        /// Hermetyzacja: Aktualizacja UI na podstawie stanu gry
        /// </summary>
        private void UpdateUI()
        {
            // Aktualizuj czas
            if (_timeLabel != null)
            {
                var minutes = (int)(_survivalTime / 60);
                var seconds = (int)(_survivalTime % 60);
                _timeLabel.Text = $"Czas: {minutes:D2}:{seconds:D2}";
            }
            
            // Aktualizuj licznik zabójstw
            if (_killCountLabel != null)
            {
                _killCountLabel.Text = $"Wrogów: {_enemiesKilled}";
            }
        }

        #endregion

        #region Game Loop - Aktualizacja statystyk
        
        public override void _Process(double delta)
        {
            // Aktualizuj czas tylko podczas gry
            if (_currentState == GameState.Playing)
            {
                _survivalTime += (float)delta;
                
                // Aktualizuj UI co jakiś czas (nie co klatkę dla wydajności)
                if (Engine.GetProcessFrames() % 60 == 0) // Co sekundę przy 60 FPS
                {
                    UpdateUI();
                }
            }
        }

        #endregion

        #region Input Handling - Centralna obsługa input'u
        
        /// <summary>
        /// Centralna obsługa input'u - Mediator Pattern
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            // Pauza - tylko podczas gry lub pauzy
            if (@event.IsActionPressed("pause") || @event.IsActionPressed("ui_cancel"))
            {
                TogglePause();
            }
            
            // Debug restart (tylko podczas developmentu)
            if (@event.IsActionPressed("restart") && OS.IsDebugBuild())
            {
                GD.Print("Debug restart!");
                StartNewGame();
            }
        }

        #endregion

        #region Cleanup - Zarządzanie zasobami
        
        public override void _ExitTree()
        {
            // Wyczyść Singleton
            if (Instance == this)
            {
                Instance = null;
            }
            
            GD.Print("GameManager cleanup completed");
            base._ExitTree();
        }

        #endregion

        #region Public Utility Methods - Zewnętrzne API
        
        /// <summary>
        /// Publiczny interfejs: Czy gra jest w trakcie?
        /// </summary>
        public bool IsGameActive()
        {
            return _currentState == GameState.Playing;
        }

        /// <summary>
        /// Publiczny interfejs: Czy gra jest zapauzowana?
        /// </summary>
        public bool IsGamePaused()
        {
            return _currentState == GameState.Paused;
        }

        /// <summary>
        /// Publiczny interfejs: Otrzymaj pełne statystyki
        /// Enkapsulacja - zwraca kopię, nie referencję do prywatnych danych
        /// </summary>
        public (float survivalTime, int enemiesKilled, float experience) GetGameStats()
        {
            return (_survivalTime, _enemiesKilled, _totalExperience);
        }

        #endregion
    }
}