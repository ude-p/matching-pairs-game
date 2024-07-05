using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardManager : TileMap
{
    // Sub class
    public record CardData(int FrontCardId, int BackCardId, Vector2I CellPosition)
    {
        public CardData(int FrontCardId, int BackCardId) : this(FrontCardId, BackCardId, Vector2I.One * -1)
        {
        }
    }

    // Public Fields
    public int touchCount { get; set; } = 0;
    private Dictionary<int, CardData> cellData { get; set; } = new();

    //Private
    private Vector2I gridSize;

    // Built-in Functions
    public override void _Ready() => SetupCards();

    // Public Funtions
    public void SetCardState(Vector2I cellPosition, int source, int tileID)
    {
        Vector2I tileCoord = TileSet.GetSource(source).GetTileId(tileID);
        SetCell(0, cellPosition, source, tileCoord);
        ++touchCount;
    }
    public Vector2I GetRandomTile()
    {
        List<Vector2I> totalCells = new(GetUsedCells(0));
        Random rnd = new Random();
        return totalCells[rnd.Next(totalCells.Count)];
    }

    public CardData ReturnCardData(Vector2I cellPosition)
    {
        if (cellData.TryGetValue(GetCellId(cellPosition), out var data))
        {
            return new CardData(data.FrontCardId, data.BackCardId, cellPosition);
        }
        else
            return null;
    }

    //Private Functions
    private void SetupCards()
    {
        List<int> frontCardList = new();
        List<int> backCardList = new();

        int levelCount = GlobalSettings.levelCount;
        gridSize = FindFactors(levelCount * 2);

        AddLayer(0); SetLayerName(0, "Cards");

        TileSetSource source0 = TileSet.GetSource(0);
        int totalBackCardTiles = source0.GetTilesCount();

        for (int row = 0; row < gridSize.X; row++)
        {
            for (int col = 0; col < gridSize.Y; col++)
            {
                int cellId = row * gridSize.Y + col;
                int frontCardIndex = RandomizeIndex(levelCount, frontCardList);
                int backCardIndex = RandomizeIndex(totalBackCardTiles, backCardList);

                cellData[cellId] = new CardData(frontCardIndex, backCardIndex);
                Vector2I tileCoord = source0.GetTileId(backCardIndex);
                SetCell(0, new Vector2I(row, col), 0, tileCoord);
            }
        }
    }

    private int GetCellId(Vector2I cellPosition) => cellPosition.X * gridSize.Y + cellPosition.Y;

    private int RandomizeIndex(int totalTiles, List<int> usedIndices)
    {
        if (!usedIndices.Any())
        {
            usedIndices.AddRange(Enumerable.Range(0, totalTiles));
        }

        Random random = new Random();
        int index = usedIndices[random.Next(usedIndices.Count)];
        usedIndices.Remove(index);

        return index;
    }

    private Vector2I FindFactors(int totalTiles)
    {
        int factor1 = (int)Math.Sqrt(totalTiles);
        int factor2 = 1;

        for (int i = factor1; i > 0; i--)
        {
            if (totalTiles % i == 0)
            {
                factor1 = i;
                factor2 = totalTiles / i;
                break;
            }
        }
        return new Vector2I(factor2, factor1);
    }
}
