using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DFSCell : BaseCell
{

    //Grid
    public bool Visited { get; private set; }
    public List<DFSCell> AdjacentCells { get; private set; }

    //Visualization
    [field:SerializeField]public SpriteRenderer CellVisualization { get; private set; }
    public Color GoingThroughCol { get; private set; }
    [SerializeField]private Color unvisitedCol;
    [SerializeField]private Color visitedCol;


    //-------------------------------------------------SETUP VARIABLES-------------------------------------

    /// <summary>
    /// Visualizes and sets the visited cell
    /// </summary>
    public void SetVisited(bool visit)
    {
        Visited = visit;
        CellVisualization.color = Visited ? visitedCol : unvisitedCol;
    }

    //-------------------------------------------------NEIGHBOURING CELLS-------------------------------------

    /// <summary>
    /// Check if a cell's position is within the bounds of the grid
    /// </summary>
    private bool IsCellWithinGridBounds(int x, int y, DFSCell[,] grid)
    {
        return x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1);
    }

    /// <summary>
    /// Calculate adjacent cells for this cell within the grid and add them to the adjacentCells list
    /// </summary>
    public void CalculateAdjacentCells(DFSCell[,] grid)
    {
        AdjacentCells = new List<DFSCell>();

        // Check and add the top cell
        if (IsCellWithinGridBounds(gridX, gridY + 1, grid))
        {
            AdjacentCells.Add(grid[gridX, gridY + 1]);
        }

        // Check and add the bottom cell 
        if (IsCellWithinGridBounds(gridX, gridY - 1, grid))
        {
            AdjacentCells.Add(grid[gridX, gridY - 1]);
        }

        // Check and add the left cell
        if (IsCellWithinGridBounds(gridX - 1, gridY, grid))
        {
            AdjacentCells.Add(grid[gridX - 1, gridY]);
        }

        // Check and add the right cell
        if (IsCellWithinGridBounds(gridX + 1, gridY, grid))
        {
            AdjacentCells.Add(grid[gridX + 1, gridY]);
        }
    }

    /// <summary>
    /// Chooses a random unvisitedCell from the adjacent cells
    /// </summary>
    public DFSCell ChooseRandomUnvisitedCell()
    {
        List<DFSCell> unvisistedCells = new List<DFSCell>();

        foreach (DFSCell cell in AdjacentCells)
        {
            if (!cell.Visited)
            {
                unvisistedCells.Add(cell);
            }
        }
        
        int randomNr = Random.Range(0,unvisistedCells.Count);
        return unvisistedCells[randomNr];
    }

    /// <summary>
    /// Destroy the wall between the current cell and the neighbouring cell
    /// </summary>
    public void DestroyWalls(DFSCell neighbouringDfsCell)
    {
        // Calculate the relative positions of the two cells
        int dx = neighbouringDfsCell.gridX - gridX;
        int dy = neighbouringDfsCell.gridY - gridY;

        // Check if the neighboring cell is to the right
        if (dx == 1)
        {
            right.SetActive(false);
            neighbouringDfsCell.left.SetActive(false);
        }
        // Check if the neighboring cell is to the left
        else if (dx == -1)
        {
            left.SetActive(false);
            neighbouringDfsCell.right.SetActive(false);
        }
        // Check if the neighboring cell is above
        else if (dy == 1)
        {
            up.SetActive(false);
            neighbouringDfsCell.down.SetActive(false);
        }
        // Check if the neighboring cell is below
        else if (dy == -1)
        {
            down.SetActive(false);
            neighbouringDfsCell.up.SetActive(false);
        }
    }

}
