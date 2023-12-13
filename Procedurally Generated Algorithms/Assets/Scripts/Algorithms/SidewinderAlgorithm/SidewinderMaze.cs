using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SidewinderMaze : MonoBehaviour
{
    [SerializeField] private UIManager uiMan;

   //Grid
   public static int Width = 5;
   public static int Height = 5;
   private float startX, startY; 
   private SidewinderCell[,] gridArray;
   
   //Generation
   [SerializeField] private SidewinderCell cellObject;
   [SerializeField] private float secUntilNextCell;
   private SidewinderCell currentCell;
   private List<SidewinderCell> unvisitedCells = new List<SidewinderCell>();
   private List<SidewinderCell> cellCarvingPath = new List<SidewinderCell>();
   
   //Object Pooling
   private Queue<SidewinderCell> pooledCells = new Queue<SidewinderCell>();
   private Queue<SidewinderCell> activeCells = new Queue<SidewinderCell>();
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
         SidewinderCell cellClone = Instantiate(cellObject, Vector3.zero, Quaternion.identity, transform);
         cellClone.gameObject.SetActive(false);
         pooledCells.Enqueue(cellClone);
      }
   }
    
   /// <summary>
   /// Returns an inactive cell object
   /// </summary>
   private SidewinderCell ReturnPooledObject()
   {
      if (pooledCells.Count == 0)
      {
         PoolCells();
      }
      SidewinderCell newCell = pooledCells.Dequeue();
      activeCells.Enqueue(newCell);
      newCell.gameObject.SetActive(true);
      return newCell;
   }
    
   /// <summary>
   /// Deactivates all active cells and makes them unvisited
   /// </summary>
   private void DeactivateAllCells()
   {
      List<SidewinderCell> activatedCells = activeCells.ToList();
        
      foreach (var cell in activatedCells)
      {
         cell.gameObject.SetActive(false);
         cell.SetCellCol(cell.UnvisitedCol);
         pooledCells.Enqueue(cell);
         activeCells.Dequeue();
      }
   }

   //--------------------------------Generation---------------------------------------------

   
   /// <summary>
   /// Activate and place all cells in the grid
   /// Setup camera after
   /// </summary>
   private void ActivateAllCells()
   {
      //create the grid
      gridArray = new SidewinderCell[Width, Height];

      startX = -(float)Width/2;
      startY = -(float)Height/2;
        
      // Activate all cells and set their gridX and gridY properties
      for (int x = 0; x < Width; x++)
      {
         for (int y = 0; y < Height; y++)
         {
            SidewinderCell cellClone = ReturnPooledObject();
            cellClone.transform.position = new Vector3(startX + x, startY + y);
            cellClone.SetGridProperties(x, y);
            gridArray[x, y] = cellClone;
            
            unvisitedCells.Add(cellClone);
         }
      }

      foreach (SidewinderCell stackItem in activeCells)
      {
         stackItem.CalculateAdjacentCells(gridArray);
      }
        
      uiMan.SetUpCamera(Width, Height);
   }
   
   /// <summary>
   /// 1. First row is a single passage
   /// 2. Second row start with the current cell choosing to go right or up and add the cell to the cellCarvingPath list
   /// 3. if it chooses to go right destroy the wall in-between and add the cell to the cellCarvingPath list
   /// 4. if it chooses to go up make the next cell the current cell > if it was the furthest cell east go to a new row
   ///      a. choose one of the cells in the list to make a passage up
   /// 5. do this until there are no unvisited cells
   /// </summary>
   private IEnumerator Generation() //Made it a coroutine for visualization purposes
   {
      WaitForSeconds waitSec = new WaitForSeconds(secUntilNextCell);
      int x = 0; //value to keep track in which column the current cell is
      
      //1. make a passage in the first row
      for (int i = 0; i < Width; i++) 
      {
         currentCell = gridArray[i, Height - 1];
         unvisitedCells.Remove(currentCell);
         
         //visualization
         currentCell.SetCellCol(currentCell.VisitedCol);
         
         if(i < Width - 1)
            currentCell.DestroyWalls(Vector2.right, gridArray);
      }
      
      while (unvisitedCells.Count > 0) //until all cells have been visited
      {
         for (int h = 2; h <= Height; h++) //whenever a new row starts
         {
            currentCell = gridArray[0, Height - h];
            cellCarvingPath.Add(currentCell);
            unvisitedCells.Remove(currentCell);
            
            x = currentCell.GridX;
            
            //visualization
            currentCell.SetCellCol(currentCell.MakingPathCol);
            
            yield return waitSec;
            
            while (x < Width) //do this until a row is finished
            {
               //2. choose the next 'direction' of the current cell
               SidewinderCell nextDirCell = currentCell.ChooseRandomNeighbouringCell();
               
               //visualization
               nextDirCell.SetCellCol(nextDirCell.NextCellCol);
               
               yield return waitSec;

               if (currentCell.DirectionBetweenCells(nextDirCell) == Vector2.right) //3. if next cell is to the right
               {
                  //add to list and make it the next current cell
                  cellCarvingPath.Add(nextDirCell);
                  currentCell = nextDirCell;
                  x = currentCell.GridX;
                  currentCell.SetCellCol(currentCell.MakingPathCol);
               }
               else if (currentCell.DirectionBetweenCells(nextDirCell) == Vector2.up) //4. if the next cell is on top
               {
                  //visualization
                  nextDirCell.SetCellCol(nextDirCell.VisitedCol);
                     
                  //a. carve a passage up from a random cell in the list and empty the list
                  int randomInt = Random.Range(0, cellCarvingPath.Count - 1);
                  cellCarvingPath[randomInt].DestroyWalls(Vector2.up, gridArray);
                  
                  for (int i = 0; i < cellCarvingPath.Count; i++)
                  {
                     if(i<cellCarvingPath.Count - 1)
                        cellCarvingPath[i].DestroyWalls(Vector2.right, gridArray); //destroy the walls in the list
                     cellCarvingPath[i].SetCellCol(cellCarvingPath[i].VisitedCol);
                     unvisitedCells.Remove(cellCarvingPath[i]);
                  }
                  cellCarvingPath.Clear();

                  // make the next cell the current cell if it's not the end of the row
                  if (currentCell.GridX + 1 < Width)
                  {
                     currentCell = gridArray[currentCell.GridX + 1, Height - h];
                     cellCarvingPath.Add(currentCell);
                     x = currentCell.GridX;
                     
                     //visualization
                     currentCell.SetCellCol(currentCell.MakingPathCol);
                  }
                  else
                  {
                     x++;
                  }
               }
            }
            yield return waitSec;
         }
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
   /// Destroys and resets the mazes properties
   /// </summary>
   public void DeactivateMaze()
   {
      DeactivateAllCells();

      unvisitedCells.Clear();
      cellCarvingPath.Clear();
   }

}
