using Godot;
using System;
public partial class Player: CharacterBody3D
{
	[Export] public float Speed = 7.0f;
	[Export] public float JumpVelocity = 7.5f;
	[Export] public float Sens = 0.5f;
	[Export] public float Smoothness = 15.0f;
	[Export] public float Acceleration = 2.0f;  // NEW: Controls how quickly velocity ramps up
	
	private Node3D pivot;
	private MeshInstance3D mesh;
	private float gravity;
	private float targetYaw;
	private float targetPitch;
	private AudioStreamPlayer jumpSfx;
	
	public override void _Ready()
	{
		pivot = GetNodeOrNull<Node3D>("CameraOrigin");
		mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
		if (pivot == null)
		{
			GD.PrintErr("CameraOrigin not found!");
			return;
		}
		targetYaw = RotationDegrees.Y;
		targetPitch = pivot.RotationDegrees.X;
		
		gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// Load jump SFX
		jumpSfx = new AudioStreamPlayer();
		jumpSfx.Stream = GD.Load<AudioStream>("res://smb_jump-small.mp3");
		jumpSfx.VolumeDb = -5.0f;
		AddChild(jumpSfx);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameWon))
			return;

		if (@event is InputEventMouseMotion mouseMotion)
		{
			targetYaw -= mouseMotion.Relative.X * Sens;
			targetPitch -= mouseMotion.Relative.Y * Sens;
			targetPitch = Mathf.Clamp(targetPitch, -40.0f, 80.0f);
		}
	}
	
	public override void _Process(double delta)
	{
		float fDelta = (float)delta;
		Vector3 playerRot = RotationDegrees;
		playerRot.Y = Mathf.RadToDeg(Mathf.LerpAngle(
			Mathf.DegToRad(RotationDegrees.Y), 
			Mathf.DegToRad(targetYaw), 
			Smoothness * fDelta
		));
		RotationDegrees = playerRot;
		
		Vector3 pivotRot = pivot.RotationDegrees;
		pivotRot.X = Mathf.Lerp(pivotRot.X, targetPitch, Smoothness * fDelta);
		pivot.RotationDegrees = pivotRot;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		float fDelta = (float)delta;

		// Fall detection - if the ball falls off the platform
		if (Position.Y < -10)
		{
			GameManager.Instance?.TriggerGameOver();
		}

		// If game is over, only apply gravity (no input)
		if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.IsGameWon))
		{
			if (!IsOnFloor())
			{
				Velocity = new Vector3(
					Velocity.X * 0.95f,
					Velocity.Y - gravity * fDelta,
					Velocity.Z * 0.95f
				);
			}
			else
			{
				Velocity = new Vector3(0, 0, 0);
			}
			MoveAndSlide();
			return;
		}

		// Apply gravity
		if (!IsOnFloor())
		{
			Velocity = new Vector3(
				Velocity.X,
				Velocity.Y - gravity * fDelta,
				Velocity.Z
			);
		}
		
		// Jump
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			Velocity = new Vector3(
				Velocity.X,
				JumpVelocity,
				Velocity.Z
			);
			jumpSfx?.Play();
		}
		
		// Quit game
		if (Input.IsActionJustPressed("quit"))
		{
			GetTree().Quit();
		}
		
		// Get input direction
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		
		// Calculate target velocity
		Vector3 targetVelocity = direction * Speed;
		
		// Smoothly accelerate/decelerate horizontal velocity
		Vector3 currentVelocity = Velocity;
		currentVelocity.X = Mathf.Lerp(currentVelocity.X, targetVelocity.X, Acceleration * fDelta);
		currentVelocity.Z = Mathf.Lerp(currentVelocity.Z, targetVelocity.Z, Acceleration * fDelta);
		Velocity = currentVelocity;
		
		// Mesh rolling
		Vector3 horizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
		if (horizontalVelocity.Length() > 0.1f)
		{
			Vector3 rollAxis = horizontalVelocity.Cross(Vector3.Up).Normalized();
			float rollAmount = horizontalVelocity.Length() * fDelta;
			Basis rotationStep = new Basis(rollAxis, -rollAmount);
			mesh.GlobalBasis = rotationStep * mesh.GlobalBasis;
		}
		
		MoveAndSlide();
	}
}
