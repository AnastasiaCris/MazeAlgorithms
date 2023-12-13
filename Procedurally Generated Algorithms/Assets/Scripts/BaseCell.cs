using UnityEngine;

public class BaseCell : MonoBehaviour
{
    //Walls
    [SerializeField]protected internal GameObject up;
    [SerializeField]protected internal GameObject down;
    [SerializeField]protected internal GameObject left;
    [SerializeField]protected internal GameObject right;
    
    //Grid
    protected int gridX;
    protected int gridY;

    public virtual void OnEnable()
    {
        //make sure all walls are active
        if(!up.activeSelf) up.SetActive(true);
        if(!down.activeSelf) down.SetActive(true);
        if(!left.activeSelf) left.SetActive(true);
        if(!right.activeSelf) right.SetActive(true);
    }


    /// <summary>
    /// Sets the X and Y position in the grid
    /// </summary>
    public void SetGridProperties(int gridX, int gridY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
