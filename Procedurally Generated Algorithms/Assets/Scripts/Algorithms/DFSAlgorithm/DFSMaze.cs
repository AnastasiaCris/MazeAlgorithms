using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DFSMaze : MonoBehaviour
{
    [SerializeField] private UIManager uiMan;
    
    //Grid
    public static int Width = 5;
    public static int Height = 5;
    private float startX, startY;
    private DFSCell[,] gridArray;

    //Generation
    [SerializeField] private DFSCell cellObject;
    [SerializeField] private float secUntilNextCell = 0.1f;
    private DFSCell currentDfsCell;
    private Stack<DFSCell> cellStack = new Stack<DFSCell>();
    
    //Object Pooling
    private Queue<DFSCell> pooledCells = new Queue<DFSCell>();
    private Queue<DFSCell> activeCells = new Queue<DFSCell>();
    private int amountToPool = 25;

    //--------------------------------Object Pooling---------------------------------------------

    private void Awake()
    {
        PoolCells();
    }

    /// <summary>
    /// Instantiates new cells and adds them to the queue
    /// </summary>
    private void PoolCells()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            DFSCell cellClone = Instantiate(cellObject, Vector3.zero, Quaternion.identity, transform);
            cellClone.gameObject.SetActive(false);
            pooledCells.Enqueue(cellClone);
        }
    }
    
    /// <summary>
    /// Returns an inactive cell object
    /// </summary>
    private DFSCell ReturnPooledObject()
    {
        if (pooledCells.Count == 0)
        {
            PoolCells();
        }
        DFSCell newCell = pooledCells.Dequeue();
        activeCells.Enqueue(newCell);
        newCell.gameObject.SetActive(true);
        return newCell;
    }
    
    /// <summary>
    /// Deactivates all active cells and makes them unvisited
    /// </summary>
    private void DeactivateAllCells()
    {
        List<DFSCell> activatedCells = activeCells.ToList();
        
        foreach (var cell in activatedCells)
        {
            cell.gameObject.SetActive(false);
            cell.SetVisited(false);
            pooledCells.Enqueue(cell);
            activeCells.Dequeue();
        }
    }

    //--------------------------------Generation---------------------------------------------

    /// <summary>
    /// Activate all cells in the grid
    /// Setup camera after
    /// </summary>
    private void ActivateAllCells()
    {
        //create the grid
        gridArray = new DFSCell[Width, Height];

        startX = -(float)Width/2;
        startY = -(float)Height/2;
        
        // Activate all cells and set their gridX and gridY properties
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                DFSCell dfsCell = ReturnPooledObject();
                dfsCell.transform.position = new Vector3(startX + x, startY + y);
                dfsCell.SetGridProperties(x,y);
                gridArray[x, y] = dfsCell;
            }
        }

        foreach (DFSCell dfsCell in activeCells)
        {
            dfsCell.CalculateAdjacentCells(gridArray);
        }
        
        uiMan.SetUpCamera(Width, Height);
    }

    /// depth-first search algorithm:
    /// 1. select a random cell to start from - and make it visited
    ///   a. if current cell has adjacent cells that are not visited => choose one at random (break the wall between)
    ///   b. else if all adjacent cells have been visited => go back to the previous cell
    /// 2. do this until all cells have been visited
    private IEnumerator Generate()
    {
        WaitForSeconds wait = new WaitForSeconds(secUntilNextCell);
        List<DFSCell> allCells = activeCells.ToList();

        //from the list of cells select a random one
        int randomFirstCell = Random.Range(0, allCells.Count);
        int cellsVisited = 0;
        
        currentDfsCell = allCells[randomFirstCell];
        currentDfsCell.SetVisited(true);
        cellStack.Push(currentDfsCell);
        cellsVisited++;
        
        yield return wait;

        while (cellsVisited < Width * Height || cellStack.Count > 0) //while there are still unvisited cells and the stack is empty
        {
            if (HasUnvisitedNeighbourCells(currentDfsCell))
            {
                DFSCell nextDfsCell = currentDfsCell.ChooseRandomUnvisitedCell();
                cellStack.Push(nextDfsCell);

                //get rid of the walls in between the current and next cell
                currentDfsCell.DestroyWalls(nextDfsCell);
                currentDfsCell = nextDfsCell;
                currentDfsCell.SetVisited(true);

                cellsVisited++;
            }
            else // if all neighbouring cells have been visited go to the previous cell
            {
                currentDfsCell = cellStack.Pop();
                currentDfsCell.CellVisualization.color = currentDfsCell.GoingThroughCol;
            }
            yield return wait;
        }
    }

    /// <summary>
    /// Check if the current cell has any unvisited cells
    /// </summary>
    private bool HasUnvisitedNeighbourCells(DFSCell currDfsCell)
    {
        foreach (var cell in currDfsCell.AdjacentCells)
        {
            if (!cell.Visited) return true;
        }

        return false;
    }
    
    //--------------------------------Regeneration---------------------------------------------

    /// <summary>
    /// Start a new DFS maze generation
    /// </summary>
    public void Regenerate()
    {
        DeactivateMaze();

        ActivateAllCells();
        StartCoroutine(Generate());

    }

    /// <summary>
    /// Deactivates and resets the mazes properties
    /// </summary>
    public void DeactivateMaze()
    {
        DeactivateAllCells();

        cellStack.Clear();
    }

}


