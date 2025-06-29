using Godot;
using MineSurvivors.scripts.player;
using MineSurvivors.scripts.ui;
using MineSurvivors.scripts.managers;

namespace MineSurvivors.scripts.managers
{
    /// <summary>
    /// GameManager - centralny punkt zarządzania grą.
    /// Demonstracja wzorca Singleton i Mediator.
    /// 
    /// Zasady OOP:
    /// - Singleton: Jedna instancja w całej grze
    /// - Mediator: Pośredniczy między systemami
    /// - Hermetyzacja: Stan gry jest ukryty, dostęp przez publiczne metody
    /// - Separacja odpowiedzialności: Tylko orkiestracja, nie implementacja szczegółów
    /// </summary>
    public partial class GameManager : Node2D
    {
        #region Singleton Pattern
        
        public static GameManager Instance { get; private set; }
        
        #endregion

        #region Game State
        
        public enum GameState
        {
            Playing,
            Paused,
            GameOver,
            MainMenu
        }
        
        private GameState _currentState = GameState.Playing;
        
        // Statystyki gry
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

        #region Component References
        
        private Player _player;
        private PauseMenu _pauseMenu;
        private GameOverMenu _gameOverMenu;
        private Hud _hud;
        private LevelUpManager _levelUpManager;
        
        #endregion

        #region Initialization
        
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

        private void FindGameComponents()
        {
            // Znajdź gracza
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (_player == null)
                GD.PrintErr("UWAGA: Nie znaleziono gracza w grupie 'player'");
            
            // Znajdź UI komponenty
            _pauseMenu = GetNodeOrNull<PauseMenu>("UI/PauseMenu");
            _gameOverMenu = GetNodeOrNull<GameOverMenu>("UI/GameOverMenu");
            _hud = GetNodeOrNull<Hud>("UI/HUD");
            
            // Znajdź LevelUpManager
            _levelUpManager = GetNodeOrNull<LevelUpManager>("LevelUpManager");
            if (_levelUpManager == null)
            {
                GD.PrintErr("UWAGA: Nie znaleziono LevelUpManager - system level up nie będzie działać");
            }
            else
            {
                GD.Print("LevelUpManager znaleziony i połączony");
            }
            
            // Walidacja kluczowych komponentów
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_player == null) GD.PrintErr("BŁĄD: GameManager nie znalazł gracza!");
            if (_pauseMenu == null) GD.PrintErr("UWAGA: Brak PauseMenu - pauza nie będzie działać");
            if (_gameOverMenu == null) GD.PrintErr("UWAGA: Brak GameOverMenu - koniec gry nie będzie działać");
            if (_hud == null) GD.PrintErr("UWAGA: Brak HUD - statystyki nie będą się aktualizować");
        }

        private void ConnectSignals()
        {
            if (_player != null)
            {
                _player.Died += OnPlayerDied;
                _player.ExperienceGained += OnExperienceGained;
                GD.Print("Sygnały gracza podłączone do GameManager");
            }
        }

        #endregion

        #region Game Flow Control
        
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

        public void TogglePause()
        {
            if (_currentState == GameState.GameOver)
                return;
                
            if (_currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (_currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

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
                GetTree().Paused = true;
            }
            
            GD.Print("Gra zapauzowana przez GameManager");
        }

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
                GetTree().Paused = false;
            }
            
            GD.Print("Gra wznowiona przez GameManager");
        }

        public void EndGame()
        {
            GD.Print("Kończenie gry...");
            
            ChangeState(GameState.GameOver);
            
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

        #region Statistics Management
        
        public void RegisterEnemyKill()
        {
            _enemiesKilled++;
            UpdateUI();
            GD.Print($"Wrogów zabitych: {_enemiesKilled}");
        }

        public void AddExperience(float amount)
        {
            _totalExperience += amount;
            
            if (_player != null)
            {
                _player.GainExperience(amount);
            }
        }

        #endregion

        #region Private State Management
        
        private void ChangeState(GameState newState)
        {
            var oldState = _currentState;
            _currentState = newState;
            
            GD.Print($"Stan gry zmieniony: {oldState} → {newState}");
            
            OnStateChanged(oldState, newState);
        }

        private void OnStateChanged(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    GetTree().Paused = false;
                    break;
                    
                case GameState.Paused:
                    GetTree().Paused = true;
                    break;
                    
                case GameState.GameOver:
                    GetTree().Paused = true;
                    break;
            }
        }

        private void HideAllMenus()
        {
            _pauseMenu?.Hide();
            _gameOverMenu?.Hide();
        }

        #endregion

        #region Signal Handlers
        
        private void OnPlayerDied()
        {
            GD.Print("GameManager otrzymał sygnał: gracz umarł");
            EndGame();
        }

        private void OnExperienceGained(float amount, float total)
        {
            _totalExperience = total;
            UpdateUI();
        }

        #endregion

        #region UI Management
        
        private void UpdateUI()
        {
            if (_hud != null)
            {
                _hud.UpdateSurvivalTime(_survivalTime);
                _hud.UpdateKillCount(_enemiesKilled);
            }
        }

        #endregion

        #region Game Loop
        
        public override void _Process(double delta)
        {
            if (_currentState == GameState.Playing)
            {
                _survivalTime += (float)delta;
                
                if (Engine.GetProcessFrames() % 60 == 0)
                {
                    UpdateUI();
                }
            }
        }

        #endregion

        #region Input Handling
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("pause") || @event.IsActionPressed("ui_cancel"))
            {
                TogglePause();
            }
            
            // Debug - usuń w produkcji
            if (@event.IsActionPressed("ui_accept") && Input.IsKeyPressed(Key.F1) && OS.IsDebugBuild())
            {
                TriggerTestLevelUp();
            }
        }

        #endregion

        #region Public Utility Methods
        
        public bool IsGameActive()
        {
            // Sprawdź czy nie ma otwartego menu level up
            bool levelUpMenuOpen = _levelUpManager?.IsUpgradeMenuOpen ?? false;
            
            return _currentState == GameState.Playing && !levelUpMenuOpen;
        }

        public bool IsGamePaused()
        {
            return _currentState == GameState.Paused;
        }

        public (float survivalTime, int enemiesKilled, float experience) GetGameStats()
        {
            return (_survivalTime, _enemiesKilled, _totalExperience);
        }

        #endregion

        #region Debug Methods - Usuń w produkcji
        
        public void TriggerTestLevelUp()
        {
            if (OS.IsDebugBuild() && _levelUpManager != null)
            {
                _levelUpManager.TriggerLevelUp();
                GD.Print("Debug: Level up triggered manually");
            }
        }

        #endregion

        #region Cleanup
        
        public override void _ExitTree()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            
            GD.Print("GameManager cleanup completed");
            base._ExitTree();
        }

        #endregion
    }
}