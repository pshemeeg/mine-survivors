using Godot;

namespace MineSurvivors.scripts.data
{
    /// <summary>
    /// Prosty model wyniku gry - demonstracja enkapsulacji.
    /// KISS: Tylko niezbędne dane, proste metody.
    /// </summary>
    [System.Serializable]
    public partial class GameResult : GodotObject
    {
        // Godot 4 wymaga [Export] dla auto-serialization
        [Export] public string Timestamp { get; set; } = "";
        [Export] public float SurvivalTime { get; set; }
        [Export] public int EnemiesKilled { get; set; }
        [Export] public int LevelReached { get; set; }
        [Export] public int FinalScore { get; set; }

        // Konstruktor bezparametrowy dla Godot
        public GameResult() { }

        // Główny konstruktor
        public GameResult(float survivalTime, int enemiesKilled, int levelReached)
        {
            Timestamp = Time.GetDatetimeStringFromSystem();
            SurvivalTime = survivalTime;
            EnemiesKilled = enemiesKilled;
            LevelReached = levelReached;
            FinalScore = CalculateScore();
        }

        // Enkapsulacja: Prosta logika obliczania wyniku
        private int CalculateScore()
        {
            return Mathf.RoundToInt(SurvivalTime * 10 + EnemiesKilled * 50 + LevelReached * 200);
        }

        // Helper methods - enkapsulacja prezentacji
        public string GetFormattedTime()
        {
            int minutes = (int)(SurvivalTime / 60);
            int seconds = (int)(SurvivalTime % 60);
            return $"{minutes:D2}:{seconds:D2}";
        }

        public string GetShortDate()
        {
            return Timestamp.Split(' ')[0]; // Tylko data bez czasu
        }
    }
}