using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WilsonMaze : MonoBehaviour
{
   [SerializeField] private UIManager uiMan;

   //Grid
   public static int Width = 5;
   public static int Height = 5;
   private float startX, startY; 
   private WilsonsCell[,] gridArray;
   
   //Generation
   [SerializeField] private WilsonsCell cellObject;
   [SerializeField] private float secUntilNextCell = 0.1f;
   private WilsonsCell currentCell;
   private List<WilsonsCell> unvisitedCells = new List<WilsonsCell>();
   private List<WilsonsCell> cellPath = new List<WilsonsCell>();
   
   //Object Pooling
   private Queue<WilsonsCell> pooledCells = new Queue<WilsonsCell>();
   private Queue<WilsonsCell> activeCells = new Queue<WilsonsCell>();
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
         WilsonsCell cellClone = Instantiate(cellObject, Vector3.zero, Quaternion.identity, transform);
         cellClone.gameObject.SetActive(false);
         pooledCells.Enqueue(cellClone);
      }
   }
    
   /// <summary>
   /// Returns an inactive cell object
   /// </summary>
   private WilsonsCell ReturnPooledObject()
   {
      if (pooledCells.Count == 0)
      {
         PoolCells();
      }
      WilsonsCell newCell = pooledCells.Dequeue();
      activeCells.Enqueue(newCell);
      newCell.gameObject.SetActive(true);
      return newCell;
   }
    
   /// <summary>
   /// Deactivates all active cells and makes them unvisited
   /// </summary>
   private void DeactivateAllCells()
   {
      List<WilsonsCell> activatedCells = activeCells.ToList();
        
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
   /// Activates and sets all cells in the grid
   /// Setup camera after
   /// </summary>
   private void ActivateAllCells()
   {
      //create the grid
      gridArray = new WilsonsCell[Width, Height];

      startX = -(float)Width/2;
      startY = -(float)Height/2;
        
      // Activate all cells and set their gridX and gridY properties
      for (int x = 0; x < Width; x++)
      {
         for (int y = 0; y < Height; y++)
         {
            WilsonsCell wilsonCell = ReturnPooledObject();
            wilsonCell.transform.position = new Vector3(startX + x, startY + y);
            wilsonCell.SetGridProperties(x,y);
            gridArray[x, y] = wilsonCell;
            
            unvisitedCells.Add(wilsonCell);
         }
      }

      foreach (WilsonsCell stackItem in activeCells)
      {
         stackItem.CalculateAdjacentCells(gridArray);
      }
        
      uiMan.SetUpCamera(Width, Height);
   }
   
   /// <summary>
   /// 1. Choose a random cell and add it to the visited list
   /// 2. Choose another random cell, which is now the current cell
   /// 3. Choose a random cell that is adjacent to the current cell and save the direction traveled in the last cell.
   /// This is your new current cell. -> if the new random cell is part of the path => delete all cells until the current cell and continue the path
   /// 4. If the current cell is not in the visited cells list -> Go to 3
   /// 5. Else -> Starting from the first cell in 2. remove all walls between the cells to complete the path and make the cells visited
   /// 6. If all cells have not been visited -> Go to 2
   /// </summary>
   private IEnumerator Generation() //Made it a coroutine for visualization purposes
   {
      WaitForSecondsRealtime waitSec = new WaitForSecondsRealtime(secUntilNextCell);
      
      // SELECT one random cell and mark it as visited
      int randomInt = Random.Range(0, unvisitedCells.Count);
      
      unvisitedCells[randomInt].SetVisited(true);

      unvisitedCells.Remove(unvisitedCells[randomInt]);

      yield return waitSec; 
      
      //SELECT another random cell which is now the current cell and add it to the path finding list
      randomInt = Random.Range(0, unvisitedCells.Count);
      currentCell = unvisitedCells[randomInt]; 
      cellPath.Add(currentCell);
      
      //visualization
      currentCell.SetCurrentPathfinding(true);

      yield return waitSec;

      while (unvisitedCells.Count > 0) //while there's still unvisited cells
      {
         if (!currentCell.Visited) //if current cell is not visited
         {
            //SELECT random cell adjacent to current cell
            WilsonsCell nextCell = currentCell.ChooseRandomNeighbouringCell(); 

            if (nextCell.CurrentlyInPathFinding) // if the next cell would end up looping with a cell from the path - delete the loop and start from next cell
            {
               int until = cellPath.IndexOf(nextCell);
               for (int i = cellPath.Count - 1; i > until;)
               {
                  cellPath[i].SetCurrentPathfinding(false);
                  cellPath[i].SetVisited(false);
                  cellPath[i].SetDirection(Vector2.zero);
                  cellPath.Remove(cellPath[i]);
                  i--;
               }
            }
            
            //save direction between cells
            currentCell.SetDirectionBetweenCells(nextCell); 
            
            //visualization
            if (cellPath.Contains(currentCell))
            {
               currentCell.SetFormingPathCol();
            }

            //set the new current cell
            currentCell = nextCell; 

            if (!currentCell.Visited) //check if the next/current cell is not visited
            {
               cellPath.Add(currentCell);

               //visualization
               currentCell.SetCurrentPathfinding(true);
            }

         }
         else // If the current cell is visited -> remove all walls in the path
         {
            for (int i = 0; i < cellPath.Count; i++)
            {
               cellPath[i].DestroyWalls(gridArray);
               cellPath[i].SetVisited(true);
               cellPath[i].SetCurrentPathfinding(false);
               unvisitedCells.Remove(cellPath[i]);
            }
            cellPath.Clear();
            
            if (unvisitedCells.Count > 0) //if there's still unvisited cells -> select another random cell 
            {
               randomInt = Random.Range(0, unvisitedCells.Count);
               currentCell = unvisitedCells[randomInt];
               cellPath.Add(currentCell);

               //visualization
               currentCell.SetCurrentPathfinding(true);
            }
         }
         yield return waitSec;
      }
   }
   
   //--------------------------------Regeneration---------------------------------------------


   /// <summary>
   /// Start a new Wilson maze generation
   /// </summary>
   public void Regenerate()
   {
      DeactivateMaze();

      ActivateAllCells();
      StartCoroutine(Generation());
   }

   /// <summary>
   /// Deactivates and resets the mazes properties
   /// </summary>
   public void DeactivateMaze()
   {
      DeactivateAllCells();

      unvisitedCells.Clear();
      cellPath.Clear();
   }
}

