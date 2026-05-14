using Godot;
using System;

public partial class Coin : Area3D
{
	[Export] public float RotationSpeed = 2.0f;
	[Export] public float BobSpeed = 2.0f;
	[Export] public float BobHeight = 0.2f;

	private float initialY;
	private float timeElapsed = 0;

	public override void _Ready()
	{
		initialY = Position.Y;
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// Spin the coin
		RotateY((float)(RotationSpeed * delta));

		// Bob up and down
		timeElapsed += (float)delta;
		var pos = Position;
		pos.Y = initialY + Mathf.Sin(timeElapsed * BobSpeed) * BobHeight;
		Position = pos;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player)
		{
			GameManager.Instance?.AddScore();
			QueueFree();
		}
	}
}
