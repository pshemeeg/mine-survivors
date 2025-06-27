using Godot;

namespace MineSurvivors.scripts.managers;

/// Podstawowa klasa GameManager — będzie rozwijana w dalszych fazach jako Singleton
public partial class GameManager : Node2D
{
    public override void _Ready()
    {
        GD.Print("Mine Survivors - Projekt uruchomiony!");
        GD.Print("Godot 4.4.1 z obsługą C# działa poprawnie.");
    }

    public override void _Process(double delta)
    {
        // Główna pętla gry — na razie pusta
    }
}