using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export] public int TotalCoins = 10;
	[Export] public float GameTime = 60.0f;

	public int Score { get; private set; } = 0;
	public float TimeRemaining { get; private set; }
	public bool IsGameOver { get; private set; } = false;
	public bool IsGameWon { get; private set; } = false;

	[Signal] public delegate void ScoreChangedEventHandler(int score, int total);
	[Signal] public delegate void TimerUpdatedEventHandler(float timeRemaining);
	[Signal] public delegate void GameOverEventHandler();
	[Signal] public delegate void GameWonEventHandler();

	private RandomNumberGenerator rng = new RandomNumberGenerator();
	private AudioStreamPlayer coinSfx;
	private AudioStreamPlayer gameOverSfx;

	public override void _Ready()
	{
		Instance = this;
		TimeRemaining = GameTime;
		rng.Randomize();

		// Load SFX
		coinSfx = new AudioStreamPlayer();
		coinSfx.Stream = GD.Load<AudioStream>("res://smb_coin.mp3");
		coinSfx.VolumeDb = -5.0f;
		AddChild(coinSfx);

		gameOverSfx = new AudioStreamPlayer();
		gameOverSfx.Stream = GD.Load<AudioStream>("res://smb_gameover.mp3");
		gameOverSfx.VolumeDb = -3.0f;
		AddChild(gameOverSfx);

		CallDeferred(nameof(SpawnCoins));
	}

	private void SpawnCoins()
	{
		var coinScene = GD.Load<PackedScene>("res://coin.tscn");
		float platformHalfSize = 42.0f;

		for (int i = 0; i < TotalCoins; i++)
		{
			var coin = coinScene.Instantiate<Node3D>();
			float x = rng.RandfRange(-platformHalfSize, platformHalfSize);
			float z = rng.RandfRange(-platformHalfSize, platformHalfSize);
			coin.Position = new Vector3(x, 0.5f, z);
			GetTree().CurrentScene.AddChild(coin);
		}

		EmitSignal(SignalName.ScoreChanged, Score, TotalCoins);
		EmitSignal(SignalName.TimerUpdated, TimeRemaining);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsGameOver || IsGameWon) return;

		TimeRemaining -= (float)delta;
		EmitSignal(SignalName.TimerUpdated, TimeRemaining);

		if (TimeRemaining <= 0)
		{
			TimeRemaining = 0;
			TriggerGameOver();
		}
	}

	public void AddScore()
	{
		if (IsGameOver || IsGameWon) return;

		Score++;
		coinSfx?.Play();
		EmitSignal(SignalName.ScoreChanged, Score, TotalCoins);

		if (Score >= TotalCoins)
		{
			IsGameWon = true;
			EmitSignal(SignalName.GameWon);
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public void TriggerGameOver()
	{
		if (IsGameOver || IsGameWon) return;
		IsGameOver = true;
		gameOverSfx?.Play();
		EmitSignal(SignalName.GameOver);
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public void RestartGame()
	{
		Instance = null;
		GetTree().ReloadCurrentScene();
	}
}
