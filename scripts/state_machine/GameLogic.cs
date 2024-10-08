using Godot;
using GodotStateCharts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class GameLogic : Node2D
{
    // Events
    public static event Action<int> ScoreTracker;

    // Private Fields
    private TextureRect playerNode, npcNode;
    private CardManager cardManager;
    private StateChart stateChart;
    private bool isInputEnabled = true;
    private List<CardManager.CardData> playerSelectedCards = new(), aiSelectedCards = new();
    private Queue<CardManager.CardData> selectedCardsHistory = new();
    private int historyCount = 2;

    public override void _Ready()
    {
        cardManager = GetNode<CardManager>("%CardTileMap");
        stateChart = StateChart.Of(GetNode("%StateChart"));
        playerNode = GetNode<TextureRect>("%Player");
        npcNode = GetNode<TextureRect>("%Npc");
        GlobalSettings.Timeout += (object sender, EventArgs e) =>
        {
            ResetCardState(playerSelectedCards);
            ResetCardState(aiSelectedCards);
            GetNode<Node>("%StateChart").ProcessMode = ProcessModeEnum.Disabled;
        };
    }

    public override void _Input(InputEvent @event)
    {
        if (!isInputEnabled) return;

        if (@event is InputEventScreenTouch touch && touch.IsPressed())
        {
            Vector2I cellPosition = cardManager.LocalToMap(GetGlobalMousePosition());

            if (cardManager.GetCellSourceId(0, cellPosition) >= 0)
            {
                if (playerSelectedCards.Any(p => p.CellPosition == cellPosition)) return;

                var cardData = cardManager.ReturnCardData(cellPosition);
                cardManager.SetCardState(cellPosition, 1, cardData.FrontCardId);
                playerSelectedCards.Add(cardData);
                UpdateSelectedCardsHistory(cardData);

                if (cardManager.touchCount == 2)
                {
                    isInputEnabled = false;
                    stateChart.SendEvent("ToOutcome");
                }
            }
        }
    }
    private void OnPlayStateEntered()
    {
        isInputEnabled = true;
        playerSelectedCards.Clear();
        cardManager.touchCount = 0;
        playerNode.Modulate = Colors.White;
        npcNode.Modulate = Colors.Gray;
    }

    private void OnOutcomeStateEntered() => ProcessOutcomeState(0, playerSelectedCards, "ToPlay", "ToAIPlay");

    private async void OnAIPlayStateEntered()
    {
        playerNode.Modulate = Colors.Gray;
        npcNode.Modulate = Colors.White;

        int iteration = 0;
        aiSelectedCards.Clear();

        while (aiSelectedCards.Count < 2)
        {
            Vector2I randomPosition = cardManager.GetRandomTile();

            if (aiSelectedCards.Any(p => p.CellPosition == randomPosition)) continue;

            var cardData = cardManager.ReturnCardData(randomPosition);
            var matchData = selectedCardsHistory.FirstOrDefault(p => p.FrontCardId == cardData.FrontCardId
            && p.CellPosition != cardData.CellPosition);

            if (matchData != null && iteration == 0)
            {
                aiSelectedCards.AddRange(new[] { matchData, cardData });

                foreach (var p in aiSelectedCards)
                {
                    cardManager.SetCardState(p.CellPosition, 1, p.FrontCardId);

                    await ToSignal(GetTree().CreateTimer(0.5), "timeout");
                }
                break;
            }

            cardManager.SetCardState(randomPosition, 1, cardData.FrontCardId);
            aiSelectedCards.Add(cardData);
            UpdateSelectedCardsHistory(cardData);
            iteration++;

            await ToSignal(GetTree().CreateTimer(0.5), "timeout");
        }
        stateChart.SendEvent("ToAIOutcome");
    }

    private void OnAIOutcomeStateEntered() => ProcessOutcomeState(1, aiSelectedCards, "ToAIPlay", "ToPlay");

    private void ProcessOutcomeState(int layer, List<CardManager.CardData> selectedCards, string previousState, string nextState)
    {

        if (selectedCards[0].FrontCardId == selectedCards[1].FrontCardId)
        {
            selectedCards.ForEach(p => cardManager.EraseCell(0, p.CellPosition));
            ScoreTracker?.Invoke(layer);

            stateChart.SendEvent(previousState);
        }
        else
        {
            selectedCards.ForEach(p => cardManager.SetCardState(p.CellPosition, 0, p.BackCardId));
            stateChart.SendEvent(nextState);
        }
    }

    private void UpdateSelectedCardsHistory(CardManager.CardData cardData)
    {
        if (selectedCardsHistory.Count == historyCount)
            selectedCardsHistory.Dequeue();

        selectedCardsHistory.Enqueue(cardData);
    }

    private void ResetCardState(List<CardManager.CardData> selectedCards)
    {
        if (selectedCards.Count == 0) return;

        selectedCards.ForEach(p => cardManager.SetCardState(p.CellPosition, 0, p.BackCardId));
    }
}