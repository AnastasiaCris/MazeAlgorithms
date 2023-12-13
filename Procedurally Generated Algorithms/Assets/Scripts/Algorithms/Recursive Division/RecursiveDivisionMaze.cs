using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RecursiveDivisionMaze : MonoBehaviour
{
    
    [SerializeField] private UIManager uiMan;
    
    //gen visualization
    [SerializeField] private float secUntilNextCell = 1;
    
    //walls
    [SerializeField] private GameObject wallPrefab;
    private List<GameObject> wallPathing = new List<GameObject>();//a series of walls made before a passage is carved through one of them
    
    //Grid
    public static int Width = 5;
    public static int Height = 5;
    private float startX, startY;
    
    //Object Pooling
    private Queue<GameObject> pooledWalls = new Queue<GameObject>();
    private Queue<GameObject> activeWalls = new Queue<GameObject>();
    private int amountToPool = 25;

    //--------------------------------Object Pooling---------------------------------------------

    private void Awake()
    {
        PoolCells();
    }

    /// <summary>
    /// Instantiates new walls and adds them to the queue
    /// </summary>
    private void PoolCells()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject wall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity, transform);
            wall.SetActive(false);
            pooledWalls.Enqueue(wall);
        }
    }
    
    /// <summary>
    /// Returns a pooled wall object
    /// </summary>
    private GameObject ReturnPooledObject(bool horizontal)
    {
        if (pooledWalls.Count == 0)
        {
            PoolCells();
        }
        GameObject newWall = pooledWalls.Dequeue();
        activeWalls.Enqueue(newWall);

        newWall.transform.rotation = Quaternion.Euler(0, 0, horizontal ? 0 : 90);

        newWall.gameObject.SetActive(true);
        return newWall;
    }
    
    /// <summary>
    /// Deactivates all active walls
    /// </summary>
    private void DeactivateAllWalls()
    {
        List<GameObject> activatedCells = activeWalls.ToList();
        
        foreach (var cell in activatedCells)
        {
            cell.SetActive(false);
            pooledWalls.Enqueue(cell);
            activeWalls.Dequeue();
        }
    }

    //--------------------------------GENERATION---------------------------------------------

    /// <summary>
    /// Creates outside walls
    /// </summary>
    private void SetUpGrid()
    {
        float offset = 0.5f;
        
        startX = -(float)Width / 2;
        startY = -(float)Height / 2;

        
        //create outside walls
        for (int x = 0; x < Width; x++)
        {
            GameObject bottomWall = ReturnPooledObject(true);
            bottomWall.transform.position = new Vector3(startX + x, startY - offset);
            GameObject topWall = ReturnPooledObject(true);
            topWall.transform.position = new Vector3(startX + x, startY + Height - offset);
        }

        for (int y = 0; y < Height; y++)
        {
            GameObject leftWall = ReturnPooledObject(false);
            leftWall.transform.position = new Vector3(startX - offset, startY + y);
            GameObject rightWall = ReturnPooledObject(false);
            rightWall.transform.position = new Vector3(startX + Width - offset, startY + y);
        }
        
        uiMan.SetUpCamera(Width, Height);
        
        WaitForSeconds waitSec = new WaitForSeconds(secUntilNextCell);
        StartCoroutine(GenerateMaze(0,0, Width, Height, waitSec));
    }

    /// <summary>
    ///  A recursive coroutine that creates the core logic of the Recursive Division Algorithm
    /// </summary>
    /// <param name="x"> The x-coordinate of the top-left corner of the current chamber within the maze. </param>
    /// <param name="y"> The x-coordinate of the top-left corner of the current chamber within the maze. </param>
    /// <param name="width"> The width of the current chamber within the maze. </param>
    /// <param name="height"> The width of the current chamber within the maze. </param>
    /// <param name="waitSec"> How many sec to wait in between wall generations </param>
    /// <returns></returns>
    private IEnumerator GenerateMaze(int x, int y, int width, int height, WaitForSeconds waitSec)
    {
        if (width < 2 || height < 2) // if the size of the chamber is too small stop the generation
        {
            yield break;
        }
        
        yield return waitSec;
        
        bool divideVertically = Random.Range(0, 2) == 0; //choose a random direction for creating a wall

        
        if (divideVertically)
        {
            int wallX = Random.Range(x + 1, x + width);
            CreateWall(wallX, y, wallX, y + height); // Create a wall along the division line
            yield return waitSec;
            StartCoroutine(GenerateMaze(x, y, wallX - x, height, waitSec)); //generate another wall for the first chamber created
            StartCoroutine(GenerateMaze(wallX, y, x + width - wallX, height, waitSec)); //generate another wall for the second chamber created
        }
        else
        {
            int wallY = Random.Range(y + 1, y + height);
            CreateWall(x, wallY, x + width, wallY); // Create a wall along the division line
            yield return waitSec;
            StartCoroutine(GenerateMaze(x, y, width, wallY - y, waitSec)); //generate another wall for the first chamber created
            StartCoroutine(GenerateMaze(x, wallY, width, y + height - wallY, waitSec)); //generate another wall for the second chamber created
        }
    }

    /// <summary>
    /// Creates a series of walls from the first pos (x1, y1) to the second pos(x2, y2) and creates a passage through the wall
    /// </summary>
    /// <param name="x1"> the x of the start position of the wall </param>
    /// <param name="y1"> the y of the start position of the wall </param>
    /// <param name="x2"> the x of the end position of the wall </param>
    /// <param name="y2"> the y of the end position of the wall </param>
    private void CreateWall(int x1, int y1, int x2, int y2)
    {
        bool horizontal = x1 == x2 ? false : true;

        Vector2 position;
        float length = Vector3.Distance(new Vector3(x1, 0, y1), new Vector3(x2, 0, y2));

        //for as many walls as there are
        for (int i = 0; i < length; i++)
        {
            position = new Vector2(horizontal ? startX + x1 + i : startX + x1  - 0.5f,horizontal ? startY + y1 - 0.5f  : startY + y1 + i );
            GameObject wall = ReturnPooledObject(horizontal);
            wall.transform.position = position;
            wallPathing.Add(wall);
        }
        
        int randInt = Random.Range(0, wallPathing.Count);
        wallPathing[randInt].SetActive(false);//make a passage
        wallPathing.Clear();
    }


    //--------------------------------Regeneration---------------------------------------------


    /// <summary>
    /// Start a new Wilson maze generation
    /// </summary>
    public void Regenerate()
    {
        DestroyMaze();

        SetUpGrid();
    }
   
    /// <summary>
    /// Destroys and resets the mazes properties
    /// </summary>
    public void DestroyMaze()
    {
        DeactivateAllWalls();
    }
}