using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Camera Zoom
    [SerializeField]private Camera cam;
    [SerializeField] private Scrollbar camZoomScrollbar;
    private float maxZoom = 5.5f;
    private float minZoom;

    //Camera Panning
    [SerializeField] private float camPanSpeed = 2f;
    private Vector3 lastMousePosition;
    private bool isPanning;

    private int maxSize = 50; //max width and height of the grid


    //--------------------------------Camera---------------------------------------------
    void Update()
    {
        MoveCamera();
    }
    
    /// <summary>
    /// Setting up camera position and size to always be in the middle of the maze and always be able to see the whole maze
    /// </summary>
    public void SetUpCamera(int width,int height)
    {
        
        int biggestNr = height > width ? height : width; //get the biggest nr between height and width
        
        minZoom = height >= width? 5 + biggestNr / 2 : biggestNr / 2;  
        
        if (minZoom <= maxZoom) minZoom = maxZoom; // size can't be smaller then 5.5
        
        cam.orthographicSize = minZoom; //set the size

        if(camZoomScrollbar.value > 0)
            camZoomScrollbar.value = 0; //reset the scrollbar in case it's been used
        
        if(cam.transform.position != new Vector3(0,0,-10))
            cam.transform.position = new Vector3(0,0,-10); //reset position if it has been changed
    }

    /// <summary>
    /// Using the scrollbar - it zooms the camera in and out
    /// </summary>
    public void ZoomCamera(float scrollValue)
    {
        float currentZoom = cam.orthographicSize;

        currentZoom = minZoom + (maxZoom - minZoom) * scrollValue;

        cam.orthographicSize = currentZoom;
    }

    /// <summary>
    /// Moves the camera in the direction the user drags the mouse across the screen
    /// </summary>
    private void MoveCamera()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Capture the initial mouse position when the button is pressed.
            lastMousePosition = Input.mousePosition;
            isPanning = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Stop panning when the button is released.
            isPanning = false;
        }

        if (isPanning)
        {
            // Calculate the change in mouse position and move the camera accordingly.
            Vector3 offset = Input.mousePosition - lastMousePosition;
            Camera.main.transform.Translate(-offset * camPanSpeed * Time.deltaTime);

            // Update the last mouse position.
            lastMousePosition = Input.mousePosition;
        }
    }

    //--------------------------------Grid Values---------------------------------------------

    
    /// <summary>
    /// Change Width of the grid in the input field
    /// </summary>
    public void OnWidthValueChanged(TMP_InputField textField)
    {
        int parseW = 0;
        if (int.TryParse(textField.text, out parseW))
        {
            parseW = Int32.Parse(textField.text);
        }
        if (parseW <= 2 && textField.text.Length > 0) //width can't be smaller then 2
        {
            parseW = 2;
            textField.text = parseW.ToString();
        }
        if (parseW >= maxSize) //width can't be bigger then max size
        {
            parseW = maxSize;
            textField.text = parseW.ToString();
        }

        DFSMaze.Width = parseW;
        WilsonMaze.Width = parseW;
        SidewinderMaze.Width = parseW;
        RecursiveDivisionMaze.Width = parseW;
    }
    
    /// <summary>
    /// Change Height of the grid in the input field
    /// </summary>
    public void OnHeightValueChanged(TMP_InputField textField)
    {
        int parseH = 0;
        if (int.TryParse(textField.text, out parseH))
        {
            parseH = Int32.Parse(textField.text);
        }
        if (parseH <= 2 && textField.text.Length > 0) //height can't be smaller then 2
        {
            parseH = 2;
            textField.text = parseH.ToString();
        }
        if (parseH >= maxSize) //height can't be bigger then max size
        {
            parseH = maxSize;
            textField.text = parseH.ToString();
        }

        DFSMaze.Height = parseH;
        WilsonMaze.Height = parseH;
        SidewinderMaze.Height = parseH;
        RecursiveDivisionMaze.Height = parseH;
    }
    
}
