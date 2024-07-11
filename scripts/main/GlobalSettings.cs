using System;
using System.Xml.Linq;
using Godot;

public partial class GlobalSettings : Node2D
{
    // Events
    public static event EventHandler Timeout;

    // Public Fields
    public static int levelCount = 10;

    // Private Fields
    private Sprite2D cusor;
    private Timer timer;
    private Vector2 window;
    private Label score, countdown;
    private Camera2D camera;
    private Control control;
    private int playerScore = 0;
    private int aiScore = 0;

    public override void _Ready()
    {
        window = GetViewportRect().Size;

        timer = GetNode<Timer>("%Timer");
        cusor = GetNode<Sprite2D>("%Cusor");
        score = GetNode<Label>("%Score"); score.Text = $"{playerScore} - {aiScore}";
        countdown = GetNode<Label>("%Countdown");
        camera = GetNode<Camera2D>("%Camera2D");
        control = GetNode<Control>("%UI");

        WindowLayout();
        timer.Start();
        GameLogic.ScoreTracker += UpdateScore;

        timer.Timeout += () => Timeout?.Invoke(this, EventArgs.Empty);
    }

    public override void _Process(double delta)
    {
        countdown.Text = Convert.ToString(Mathf.Ceil(timer.TimeLeft));
        cusor.Position = GetGlobalMousePosition();
    }

    private void UpdateScore(int tracker)
    {
        playerScore = tracker == 0 ? playerScore += 1 : playerScore;
        aiScore = tracker == 1 ? aiScore += 1 : aiScore;

        score.Text = $"{playerScore} - {aiScore}";
    }

    private void WindowLayout()
    {
        float gridWidth = CardManager.cellSize.X * CardManager.gridSize.X;
        float gridHeight = CardManager.cellSize.Y * CardManager.gridSize.Y;
        Vector2 gridMidpoint = new Vector2(gridWidth, gridHeight) / 2;

        float zoomX = window.X / (gridWidth + 100f);
        Vector2 currentWindow = new Vector2(window.X / zoomX, window.Y / zoomX);
        Vector2 centerPosition = new Vector2(gridMidpoint.X - (currentWindow.X / 2), gridMidpoint.Y - (currentWindow.Y / 2));
        centerPosition.Y += 50f;

        camera.Zoom = new Vector2(zoomX, zoomX);
        camera.Position = centerPosition;
        control.Size = currentWindow;

        GD.Print(currentWindow);
    }
}
