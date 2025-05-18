using Godot;
using System;

public partial class Player : Character
{
    public Vector2 GetInput()
    {

        Vector2 inputDirection = Input.GetVector("left", "right", "up", "down");

        return inputDirection;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = GetInput();

        ProcessMovement(direction,delta);
    }

}
