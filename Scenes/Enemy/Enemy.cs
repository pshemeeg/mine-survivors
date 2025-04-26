using Godot;

namespace minesurvivors.Scenes.Enemy;
// Klasa reprezentująca przeciwnika, dziedzicząca po Character
public partial class Enemy : Character
{
    // Zmienna przechowująca referencję do węzła gracza
    private Player _player;

    // Metoda wywoływana przy inicjalizacji węzła
    public override void _Ready()
    {
        // Próba znalezienia węzła gracza w drzewie sceny.
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;

        if (_player == null)
        {
            GD.PrintErr("Enemy: Nie znaleziono węzła gracza w grupie 'Player'.");
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (_player != null)
        {
            // Obliczanie kierunku do gracza
            Vector2 directionToPlayer = (_player.GlobalPosition - GlobalPosition).Normalized();

            // Wywołanie metody ruchu z klasy bazowej
            ProcessMovement(directionToPlayer, delta);
        }
        else
        {
            // Jeśli gracz nie został znaleziony, przeciwnik stoi w miejscu
            ProcessMovement(Vector2.Zero, delta);
        }
    }
}