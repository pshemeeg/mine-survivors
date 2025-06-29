using Godot;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Proste menu high scores - używa Autoload ScoreManager.
    /// KISS: VBoxContainer + Labels, nothing more.
    /// </summary>
    public partial class HighScoresMenu : Control
    {
        #region UI References
        
        private VBoxContainer _scoresContainer;
        private Button _backButton;
        private Button _clearButton;
        private Label _noScoresLabel;
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            // Znajdź komponenty
            _scoresContainer = GetNode<VBoxContainer>("%ScoresContainer");
            _backButton = GetNode<Button>("%BackButton");
            _clearButton = GetNode<Button>("%ClearButton");
            _noScoresLabel = GetNode<Label>("%NoScoresLabel");
            
            // Podłącz sygnały
            _backButton.Pressed += OnBackPressed;
            _clearButton.Pressed += OnClearPressed;
            
            // Wyświetl wyniki
            DisplayScores();
        }
        
        #endregion

        #region Display Logic - Proste i skuteczne
        
        private void DisplayScores()
        {
            // Wyczyść poprzednie wpisy
            foreach (Node child in _scoresContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Autoload access - prostsze niż Instance pattern!
            var scoreManager = GetNode<MineSurvivors.scripts.managers.ScoreManager>("/root/ScoreManager");
            var scores = scoreManager?.GetTopScores();
            
            if (scores == null || scores.Count == 0)
            {
                _noScoresLabel.Show();
                return;
            }
            
            _noScoresLabel.Hide();
            
            // Stwórz prosty wpis dla każdego wyniku
            for (int i = 0; i < scores.Count; i++)
            {
                var score = scores[i];
                var position = i + 1;
                
                // Jeden label na wynik - KISS!
                var label = new RichTextLabel();
                label.FitContent = true;
                label.BbcodeEnabled = true;
                
                // Format z emoji dla czytelności
                string text = $"[b]#{position}[/b] - [color=gold]{score.FinalScore:N0}[/color] punktów\n";
                text += $"   ⏱️ {score.GetFormattedTime()} | 💀 {score.EnemiesKilled} | 📈 Lv.{score.LevelReached}\n";
                text += $"   📅 {score.GetShortDate()}";
                
                label.Text = text;
                _scoresContainer.AddChild(label);
                
                // Separator między wpisami
                if (i < scores.Count - 1)
                {
                    var separator = new HSeparator();
                    _scoresContainer.AddChild(separator);
                }
            }
        }
        
        #endregion

        #region Event Handlers
        
        private void OnBackPressed()
        {
            GetTree().ChangeSceneToFile("res://scenes/UI/MainMenu.tscn");
        }
        
        private void OnClearPressed()
        {
            // Proste potwierdzenie
            var dialog = new AcceptDialog();
            dialog.DialogText = "Czy na pewno chcesz wyczyścić wszystkie wyniki?";
            dialog.Title = "Potwierdzenie";
            
            // Dodaj drugi przycisk dla Cancel
            dialog.AddCancelButton("Anuluj");
            
            AddChild(dialog);
            dialog.PopupCentered();
            
            // Obsłuż potwierdzenie
            dialog.Confirmed += () => {
                var scoreManager = GetNode<MineSurvivors.scripts.managers.ScoreManager>("/root/ScoreManager");
                scoreManager?.ClearAllScores();
                DisplayScores();
                dialog.QueueFree();
            };
            
            dialog.Canceled += () => dialog.QueueFree();
        }
        
        #endregion

        #region Input
        
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnBackPressed();
            }
        }
        
        #endregion
    }
}
