using Godot;
using MineSurvivors.scripts.enemies;
using MineSurvivors.scripts.player;

namespace minesurvivors.Scripts.Spawner
{
    /// <summary>
    /// Prosty ale skuteczny system spawnowania przeciwników.
    /// KISS: Jedna klasa, proste zasady, łatwe do zrozumienia i modyfikacji.
    /// 
    /// Zasady OOP:
    /// - Hermetyzacja: Logika spawnowania ukryta w tej klasie
    /// - Enkapsulacja: Zewnętrzne systemy nie muszą znać szczegółów spawning logic
    /// - Separacja odpowiedzialności: Tylko spawning, nic więcej
    /// - Kompozycja: Używa PackedScene dla różnych typów przeciwników
    /// </summary>
    public partial class EnemySpawner : Node2D
    {
        #region Enemy Scenes - Konfiguracja w edytorze
        
        [ExportGroup("Enemy Types")]
        [Export] private PackedScene _goblinScene;
        [Export] private PackedScene _hobgoblinScene;
        [Export] private PackedScene _golemScene;
        
        #endregion

        #region Spawn Settings - Proste parametry do tweakowania
        
        [ExportGroup("Basic Spawn Settings")]
        [Export] private float _baseSpawnInterval = 2.0f; // Podstawowy interwał między spawnami
        [Export] private float _spawnRadius = 600f; // Jak daleko od gracza spawnować
        [Export] private int _maxEnemiesOnScreen = 50; // Limit przeciwników na ekranie
        
        [ExportGroup("Difficulty Scaling")]
        [Export] private float _timeScaleRate = 0.95f; // Co sekundę spawning przyspiesza o 5%
        [Export] private float _minSpawnInterval = 0.3f; // Minimum interval (cap)
        
        #endregion

        #region Private State - Hermetyzacja
        
        private Timer _spawnTimer;
        private Player _player; // Cache gracza
        private float _currentGameTime = 0f;
        private int _difficultyLevel = 1; // 0=Easy, 1=Normal, 2=Hard, 3=Extreme
        
        // Mnożniki trudności - KISS approach
        private readonly float[] _difficultyMultipliers = { 0.7f, 1.0f, 1.3f, 1.8f };
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja EnemySpawner...");
            
            // Znajdź gracza
            CachePlayer();
            
            // Setup timer
            SetupSpawnTimer();
            
            // Wczytaj poziom trudności z ustawień
            LoadDifficultyFromSettings();
            
            // Wczytaj sceny jeśli nie ustawione w edytorze
            LoadEnemyScenesIfNeeded();
            
            GD.Print($"EnemySpawner gotowy! Trudność: {GetDifficultyName(_difficultyLevel)}");
        }

        /// <summary>
        /// Cache gracza - optymalizacja wydajności
        /// </summary>
        private void CachePlayer()
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (_player == null)
            {
                GD.PrintErr("BŁĄD: EnemySpawner nie znalazł gracza!");
            }
        }

        /// <summary>
        /// Setup Godot Timer - proper way
        /// </summary>
        private void SetupSpawnTimer()
        {
            _spawnTimer = new Timer();
            _spawnTimer.WaitTime = _baseSpawnInterval;
            _spawnTimer.Timeout += OnSpawnTimerTimeout;
            AddChild(_spawnTimer);
            _spawnTimer.Start();
        }

        /// <summary>
        /// Wczytaj poziom trudności z ConfigFile
        /// </summary>
        private void LoadDifficultyFromSettings()
        {
            var config = new ConfigFile();
            var error = config.Load("user://settings.cfg");
            
            if (error == Error.Ok)
            {
                _difficultyLevel = (int)config.GetValue("gameplay", "difficulty", 1);
                _difficultyLevel = Mathf.Clamp(_difficultyLevel, 0, 3); // Safety clamp
            }
            
            GD.Print($"Difficulty loaded: {GetDifficultyName(_difficultyLevel)}");
        }

        /// <summary>
        /// Wczytaj sceny przeciwników jeśli nie ustawione
        /// </summary>
        private void LoadEnemyScenesIfNeeded()
        {
            if (_goblinScene == null)
                _goblinScene = GD.Load<PackedScene>("res://scenes/enemies/Goblin.tscn");
                
            if (_hobgoblinScene == null)
                _hobgoblinScene = GD.Load<PackedScene>("res://scenes/enemies/Hobgoblin.tscn");
                
            if (_golemScene == null)
                _golemScene = GD.Load<PackedScene>("res://scenes/enemies/Golem.tscn");
        }

        #endregion

        #region Main Spawning Logic - Serce systemu
        
        public override void _Process(double delta)
        {
            // Aktualizuj czas gry
            _currentGameTime += (float)delta;
            
            // Co sekundę aktualizuj spawn rate (progressive difficulty)
            if (Mathf.FloorToInt(_currentGameTime) % 1 == 0) // Co sekundę
            {
                UpdateSpawnRate();
            }
        }

        /// <summary>
        /// Event Handler: Timer timeout - czas na spawn!
        /// </summary>
        private void OnSpawnTimerTimeout()
        {
            // Sprawdź czy możemy spawnować
            if (!CanSpawn())
            {
                return;
            }
            
            // Wybierz typ przeciwnika
            PackedScene enemyScene = ChooseEnemyType();
            if (enemyScene == null)
            {
                GD.PrintErr("Brak sceny przeciwnika do spawnu!");
                return;
            }
            
            // Znajdź pozycję spawnu
            Vector2 spawnPosition = GetSpawnPosition();
            
            // Spawnuj przeciwnika
            SpawnEnemy(enemyScene, spawnPosition);
        }

        /// <summary>
        /// Sprawdź czy można spawnować przeciwnika
        /// </summary>
        private bool CanSpawn()
        {
            // Sprawdź czy gracz istnieje i żyje
            if (_player == null || !_player.IsAlive)
                return false;
            
            // Sprawdź limit przeciwników na ekranie
            int currentEnemyCount = GetTree().GetNodesInGroup("enemies").Count;
            if (currentEnemyCount >= _maxEnemiesOnScreen)
            {
                GD.Print($"Za dużo przeciwników na ekranie: {currentEnemyCount}/{_maxEnemiesOnScreen}");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Wybierz typ przeciwnika na podstawie czasu gry i trudności
        /// KISS: Proste zasady progresji
        /// </summary>
        private PackedScene ChooseEnemyType()
        {
            // Progressywne wprowadzanie przeciwników
            float gameMinutes = _currentGameTime / 60f;
            
            // Bazowe prawdopodobieństwa
            float goblinChance = 70f;
            float hobgoblinChance = 20f;
            float golemChance = 10f;
            
            // Modyfikacja na podstawie czasu gry
            if (gameMinutes < 1f)
            {
                // Pierwsze minuty - tylko gobliny
                return _goblinScene;
            }
            else if (gameMinutes < 3f)
            {
                // 1-3 minuty - gobliny i trochę hobgoblinów
                hobgoblinChance = 30f;
                golemChance = 0f;
            }
            else if (gameMinutes < 5f)
            {
                // 3-5 minut - wszystkie typy, ale więcej hobgoblinów
                goblinChance = 50f;
                hobgoblinChance = 40f;
                golemChance = 10f;
            }
            else
            {
                // Po 5 minutach - więcej trudnych przeciwników
                goblinChance = 40f;
                hobgoblinChance = 35f;
                golemChance = 25f;
            }
            
            // Modyfikacja na podstawie trudności
            float difficultyMultiplier = _difficultyMultipliers[_difficultyLevel];
            if (difficultyMultiplier > 1.0f)
            {
                // Wyższa trudność = więcej trudnych przeciwników
                hobgoblinChance *= difficultyMultiplier;
                golemChance *= difficultyMultiplier;
                goblinChance = 100f - hobgoblinChance - golemChance;
            }
            
            // Losuj na podstawie prawdopodobieństw
            float roll = GD.Randf() * 100f;
            
            if (roll < goblinChance)
                return _goblinScene;
            else if (roll < goblinChance + hobgoblinChance)
                return _hobgoblinScene;
            else
                return _golemScene;
        }

        /// <summary>
        /// Znajdź pozycję spawnu wokół gracza
        /// </summary>
        private Vector2 GetSpawnPosition()
        {
            if (_player == null) return Vector2.Zero;
            
            // Losowy kąt wokół gracza
            float angle = GD.Randf() * Mathf.Tau; // 0 to 2π
            
            // Pozycja w promieniu spawnu od gracza
            Vector2 offset = Vector2.FromAngle(angle) * _spawnRadius;
            Vector2 spawnPos = _player.GlobalPosition + offset;
            
            return spawnPos;
        }

        /// <summary>
        /// Spawnuj przeciwnika w określonej pozycji
        /// </summary>
        private void SpawnEnemy(PackedScene enemyScene, Vector2 position)
        {
            var enemy = enemyScene.Instantiate<Enemy>();
            enemy.GlobalPosition = position;
            
            // Dodaj do sceny (parent node)
            GetTree().CurrentScene.AddChild(enemy);
            
            GD.Print($"Spawned {enemy.GetType().Name} at {position}");
        }

        #endregion

        #region Difficulty Scaling - Progresywna trudność
        
        /// <summary>
        /// Aktualizuj tempo spawnu na podstawie czasu gry
        /// </summary>
        private void UpdateSpawnRate()
        {
            // Oblicz nowy interwał spawnu
            float timeScale = Mathf.Pow(_timeScaleRate, _currentGameTime);
            float newInterval = _baseSpawnInterval * timeScale;
            
            // Zastosuj mnożnik trudności
            newInterval /= _difficultyMultipliers[_difficultyLevel];
            
            // Ogranicz do minimum
            newInterval = Mathf.Max(newInterval, _minSpawnInterval);
            
            // Ustaw nowy interwał
            _spawnTimer.WaitTime = newInterval;
        }

        #endregion

        #region Public API - Interface dla innych systemów
        
        /// <summary>
        /// Ustaw poziom trudności (np. z GameManager)
        /// </summary>
        public void SetDifficulty(int difficulty)
        {
            _difficultyLevel = Mathf.Clamp(difficulty, 0, 3);
            GD.Print($"Difficulty changed to: {GetDifficultyName(_difficultyLevel)}");
        }

        /// <summary>
        /// Zatrzymaj spawning (np. podczas pauzy lub game over)
        /// </summary>
        public void StopSpawning()
        {
            _spawnTimer?.Stop();
        }

        /// <summary>
        /// Wznów spawning
        /// </summary>
        public void ResumeSpawning()
        {
            _spawnTimer?.Start();
        }

        /// <summary>
        /// Wyczyść wszystkich przeciwników (np. restart gry)
        /// </summary>
        public void ClearAllEnemies()
        {
            var enemies = GetTree().GetNodesInGroup("enemies");
            foreach (Node enemy in enemies)
            {
                enemy.QueueFree();
            }
            GD.Print($"Cleared {enemies.Count} enemies");
        }

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Helper: Nazwa poziomu trudności
        /// </summary>
        private string GetDifficultyName(int level)
        {
            return level switch
            {
                0 => "Łatwy",
                1 => "Normalny", 
                2 => "Trudny",
                3 => "Ekstremalny",
                _ => "Nieznany"
            };
        }

        /// <summary>
        /// Debug: Pobierz statystyki spawning
        /// </summary>
        public (float currentInterval, int enemiesOnScreen, float gameTime) GetSpawnStats()
        {
            int enemyCount = GetTree().GetNodesInGroup("enemies").Count;
            return ((float currentInterval, int enemiesOnScreen, float gameTime))(_spawnTimer?.WaitTime ?? 0f, enemyCount, _currentGameTime);
        }

        #endregion

        #region Cleanup
        
        public override void _ExitTree()
        {
            GD.Print("EnemySpawner cleanup...");
            base._ExitTree();
        }

        #endregion
    }
}