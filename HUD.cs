using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private Label scoreLabel;
	private Label timerLabel;
	private Control gameOverPanel;
	private Label messageLabel;
	private Label restartLabel;

	public override void _Ready()
	{
		BuildUI();
		CallDeferred(nameof(ConnectSignals));
	}

	private void ConnectSignals()
	{
		var gm = GameManager.Instance;
		if (gm != null)
		{
			gm.ScoreChanged += OnScoreChanged;
			gm.TimerUpdated += OnTimerUpdated;
			gm.GameOver += OnGameOver;
			gm.GameWon += OnGameWon;
		}
	}

	private void BuildUI()
	{
		// Root control - full screen
		var root = new Control();
		root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		root.MouseFilter = Control.MouseFilterEnum.Ignore;
		AddChild(root);

		// Top bar
		var topMargin = new MarginContainer();
		topMargin.SetAnchorsPreset(Control.LayoutPreset.TopWide);
		topMargin.AddThemeConstantOverride("margin_left", 20);
		topMargin.AddThemeConstantOverride("margin_right", 20);
		topMargin.AddThemeConstantOverride("margin_top", 15);
		topMargin.MouseFilter = Control.MouseFilterEnum.Ignore;
		root.AddChild(topMargin);

		var topBar = new HBoxContainer();
		topBar.MouseFilter = Control.MouseFilterEnum.Ignore;
		topMargin.AddChild(topBar);

		// Score label
		scoreLabel = new Label();
		scoreLabel.Text = "Coins: 0/10";
		scoreLabel.AddThemeFontSizeOverride("font_size", 28);
		scoreLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		scoreLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.2f));
		scoreLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.7f));
		scoreLabel.AddThemeConstantOverride("shadow_offset_x", 2);
		scoreLabel.AddThemeConstantOverride("shadow_offset_y", 2);
		topBar.AddChild(scoreLabel);

		// Timer label
		timerLabel = new Label();
		timerLabel.Text = "Time: 60s";
		timerLabel.AddThemeFontSizeOverride("font_size", 28);
		timerLabel.HorizontalAlignment = HorizontalAlignment.Right;
		timerLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		timerLabel.AddThemeColorOverride("font_color", Colors.White);
		timerLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.7f));
		timerLabel.AddThemeConstantOverride("shadow_offset_x", 2);
		timerLabel.AddThemeConstantOverride("shadow_offset_y", 2);
		topBar.AddChild(timerLabel);

		// Game over / win overlay (hidden)
		gameOverPanel = new Control();
		gameOverPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		gameOverPanel.Visible = false;
		gameOverPanel.MouseFilter = Control.MouseFilterEnum.Ignore;
		root.AddChild(gameOverPanel);

		var overlay = new ColorRect();
		overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		overlay.Color = new Color(0, 0, 0, 0.6f);
		overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
		gameOverPanel.AddChild(overlay);

		var center = new CenterContainer();
		center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		center.MouseFilter = Control.MouseFilterEnum.Ignore;
		gameOverPanel.AddChild(center);

		var vbox = new VBoxContainer();
		vbox.MouseFilter = Control.MouseFilterEnum.Ignore;
		vbox.AddThemeConstantOverride("separation", 20);
		center.AddChild(vbox);

		messageLabel = new Label();
		messageLabel.Text = "GAME OVER";
		messageLabel.AddThemeFontSizeOverride("font_size", 64);
		messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
		messageLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		messageLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
		messageLabel.AddThemeConstantOverride("shadow_offset_x", 3);
		messageLabel.AddThemeConstantOverride("shadow_offset_y", 3);
		vbox.AddChild(messageLabel);

		restartLabel = new Label();
		restartLabel.Text = "Press R to Restart";
		restartLabel.AddThemeFontSizeOverride("font_size", 24);
		restartLabel.HorizontalAlignment = HorizontalAlignment.Center;
		restartLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		vbox.AddChild(restartLabel);
	}

	private void OnScoreChanged(int score, int total)
	{
		scoreLabel.Text = $"Coins: {score}/{total}";
	}

	private void OnTimerUpdated(float timeRemaining)
	{
		int seconds = Mathf.Max(0, Mathf.CeilToInt(timeRemaining));
		timerLabel.Text = $"Time: {seconds}s";

		if (timeRemaining <= 10)
			timerLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		else
			timerLabel.AddThemeColorOverride("font_color", Colors.White);
	}

	private void OnGameOver()
	{
		messageLabel.Text = "GAME OVER";
		messageLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		gameOverPanel.Visible = true;
	}

	private void OnGameWon()
	{
		messageLabel.Text = "YOU WIN!";
		messageLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1, 0.4f));
		float timeTaken = GameManager.Instance.GameTime - GameManager.Instance.TimeRemaining;
		restartLabel.Text = $"Time: {timeTaken:F1}s — Press R to Restart";
		gameOverPanel.Visible = true;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.R)
		{
			var gm = GameManager.Instance;
			if (gm != null && (gm.IsGameOver || gm.IsGameWon))
			{
				gm.RestartGame();
			}
		}
	}
}
