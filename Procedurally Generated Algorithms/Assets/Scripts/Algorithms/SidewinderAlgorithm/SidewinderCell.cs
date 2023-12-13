using System.Collections.Generic;
using UnityEngine;

public class SidewinderCell : BaseCell
{
    //Grid
    private List<SidewinderCell> adjacentCells;
    public int GridX { get { return gridX;}}
    
    //Visualization
    [SerializeField] private SpriteRenderer CellVisualization;
    [field:SerializeField]public Color UnvisitedCol { get; private set; }
    [field:SerializeField]public Color VisitedCol { get; private set; }
    [field:SerializeField]public Color MakingPathCol { get; private set; }
    [field:SerializeField]public Color NextCellCol { get; private set; }


    //-------------------------------------------------SETUP VARIABLES-------------------------------------

    public void SetCellCol(Color col)
    {
        CellVisualization.color = col;
    }
    
    //-------------------------------------------------NEIGHBOURING CELLS-------------------------------------

    
    /// <summary>
    /// Check if a cell's position is within the bounds of the grid
    /// </summary>
    private bool IsCellWithinGridBounds(int x, int y, SidewinderCell[,] grid)
    {
        return x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1);
    }

    /// <summary>
    /// Calculate adjacent cells ONLY on the top and right side of the cell
    /// </summary>
    public void CalculateAdjacentCells(SidewinderCell[,] grid)
    {
        adjacentCells = new List<SidewinderCell>();

        // Check and add the top cell
        if (IsCellWithinGridBounds(gridX, gridY + 1, grid))
        {
            adjacentCells.Add(grid[gridX, gridY + 1]);
        }

        // Check and add the right cell
        if (IsCellWithinGridBounds(gridX + 1, gridY, grid))
        {
            adjacentCells.Add(grid[gridX + 1, gridY]);
        }
    }

    /// <summary>
    /// Chooses a random cell from the adjacent cells 
    /// </summary>
    public SidewinderCell ChooseRandomNeighbouringCell()
    {
        int randomNr = Random.Range(0,adjacentCells.Count);
        return adjacentCells[randomNr];
    }
    
    /// <summary>
    /// Gets the direction where the neighbouring cell is situated in relation to the current cell
    /// </summary>
    public Vector2 DirectionBetweenCells(SidewinderCell neighbouringCell)
    {
        Vector2 dir = new Vector2(neighbouringCell.gridX - gridX, neighbouringCell.gridY - gridY);
        return dir;
    }

    /// <summary>
    /// Destroy the wall between the current and neighbouring cell based on the direction
    /// </summary>
    public void DestroyWalls(Vector2 dir, SidewinderCell[,] grid)
    {
        SidewinderCell neighbouringCell = grid[gridX + (int)dir.x, gridY + (int)dir.y];
        
        //The algorithm checks only for cells to the right or on top, so do that
        
        // Check if the neighboring cell is to the right
        if (dir == Vector2.right)
        {
            right.SetActive(false);
            neighbouringCell.left.SetActive(false);
        }
        // Check if the neighboring cell is on top
        else if (dir == Vector2.up)
        {
            up.SetActive(false);
            neighbouringCell.down.SetActive(false);
        }
    }

}
