using Godot;
using System.Collections.Generic;
using System.Linq;
using MineSurvivors.scripts.data;

namespace MineSurvivors.scripts.managers
{
    /// <summary>
    /// ScoreManager jako Autoload Singleton - najlepsze rozwiązanie dla Godot 4.
    /// SETUP: Project Settings → Autoload → Add ScoreManager.cs jako "ScoreManager"
    /// KISS: Używa Godot's ConfigFile, automatycznie dostępny w całym projekcie.
    /// </summary>
    public partial class ScoreManager : Node
    {
        #region Autoload Singleton - Najlepszy pattern dla Godot 4
        
        // Godot automatycznie tworzy Instance przez Autoload
        // Nie potrzebujemy manual singleton setup!
        
        public override void _Ready()
        {
            LoadScores();
            GD.Print("ScoreManager ready as Autoload Singleton");
        }
        
        #endregion

        #region Simple Data Storage
        
        private const string SavePath = "user://high_scores.cfg";
        private const int MaxScores = 10;
        
        // Prosta lista wyników - enkapsulacja
        private List<GameResult> _scores = new();
        
        #endregion

        #region Public API - KISS Design
        
        /// <summary>
        /// Dodaj nowy wynik. Zwraca pozycję (1-10) lub 0 jeśli nie weszło.
        /// </summary>
        public int AddScore(float survivalTime, int enemiesKilled, int levelReached)
        {
            var newScore = new GameResult(survivalTime, enemiesKilled, levelReached);
            _scores.Add(newScore);
            
            // Sort i trim
            _scores = _scores.OrderByDescending(s => s.FinalScore).Take(MaxScores).ToList();
            
            // Znajdź pozycję
            int position = _scores.FindIndex(s => s == newScore) + 1;
            
            SaveScores();
            GD.Print($"New score: {newScore.FinalScore} (position: {position})");
            
            return position > 0 && position <= MaxScores ? position : 0;
        }
        
        /// <summary>
        /// Pobierz top wyniki
        /// </summary>
        public List<GameResult> GetTopScores() => new(_scores);
        
        /// <summary>
        /// Sprawdź czy wynik kwalifikuje się do top 10
        /// </summary>
        public bool IsHighScore(int score)
        {
            return _scores.Count < MaxScores || score > _scores.LastOrDefault()?.FinalScore;
        }
        
        /// <summary>
        /// Wyczyść wszystkie wyniki
        /// </summary>
        public void ClearAllScores()
        {
            _scores.Clear();
            SaveScores();
            GD.Print("All scores cleared");
        }
        
        #endregion

        #region Godot 4 File I/O - Proste i niezawodne
        
        /// <summary>
        /// Godot 4 ConfigFile - prostsze niż JSON, built-in serialization
        /// </summary>
        private void SaveScores()
        {
            var config = new ConfigFile();
            
            // Zapisz każdy wynik jako sekcję
            for (int i = 0; i < _scores.Count; i++)
            {
                var score = _scores[i];
                config.SetValue($"score_{i}", "timestamp", score.Timestamp);
                config.SetValue($"score_{i}", "survival_time", score.SurvivalTime);
                config.SetValue($"score_{i}", "enemies_killed", score.EnemiesKilled);
                config.SetValue($"score_{i}", "level_reached", score.LevelReached);
                config.SetValue($"score_{i}", "final_score", score.FinalScore);
            }
            
            // Zapisz liczbę wyników
            config.SetValue("meta", "count", _scores.Count);
            
            // Save do pliku
            var error = config.Save(SavePath);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to save scores: {error}");
            }
        }
        
        /// <summary>
        /// Wczytaj wyniki z ConfigFile
        /// </summary>
        private void LoadScores()
        {
            var config = new ConfigFile();
            var error = config.Load(SavePath);
            
            if (error != Error.Ok)
            {
                GD.Print("No scores file found, starting fresh");
                return;
            }
            
            int count = config.GetValue("meta", "count", 0).AsInt32();
            _scores.Clear();
            
            for (int i = 0; i < count; i++)
            {
                var section = $"score_{i}";
                if (!config.HasSection(section)) continue;
                
                var score = new GameResult
                {
                    Timestamp = config.GetValue(section, "timestamp", "").AsString(),
                    SurvivalTime = config.GetValue(section, "survival_time", 0f).AsSingle(),
                    EnemiesKilled = config.GetValue(section, "enemies_killed", 0).AsInt32(),
                    LevelReached = config.GetValue(section, "level_reached", 1).AsInt32(),
                    FinalScore = config.GetValue(section, "final_score", 0).AsInt32()
                };
                
                _scores.Add(score);
            }
            
            // Sort dla pewności
            _scores = _scores.OrderByDescending(s => s.FinalScore).ToList();
            GD.Print($"Loaded {_scores.Count} scores");
        }
        
        #endregion

        #region Cleanup
        
        public override void _ExitTree()
        {
            SaveScores(); // Final save
        }
        
        #endregion
    }
}