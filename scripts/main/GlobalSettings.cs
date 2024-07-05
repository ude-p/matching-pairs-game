using Godot;
using System;

public partial class GlobalSettings : Node2D
{
    public static int levelCount { get; private set; } = 5;

    private Label label;
    private int playerScore = 0;
    private int aiScore = 0;

    public override void _Ready()
    {
        label = GetNode<Label>("%Score");
        label.Text = $"{playerScore} - {aiScore}";

        GameLogic.ScoreTracker += UpdateScore;
    }

    private void UpdateScore(int tracker)
    {
        playerScore = tracker == 0 ? playerScore += 1 : playerScore;
        aiScore = tracker == 1 ? aiScore += 1 : aiScore;

        label.Text = $"{playerScore} - {aiScore}";
    }
}
