using Godot;
using System;

// klasa bazowa dla postaci w grze
public partial class Character : CharacterBody2D
{
    [Export] public int Health { get; set; }
    [Export] public int AttackDamage { get; set; }
    [Export] public double MovementSpeed { get; set; }

}
