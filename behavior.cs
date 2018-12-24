using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class behavior : MonoBehaviour
{
    public bool toggleInefficientMethod;
    public GameObject maskPrefab;
    private bool isPressed = false;
    public GameObject topCover;
    public GameObject bonusCover;
    private Texture2D maskTex;
    public int circleRadius;
    private Bounds topRenderBounds;
    private Vector3 boundsMinScreen, boundsMaxScreen;

    private void Start()
    {
        //Set up Scree-Space bounds
        topRenderBounds = topCover.GetComponent<Renderer>().bounds;
        Vector3 boundsMin = topRenderBounds.min;
        Vector3 boundsMax = topRenderBounds.max;
        boundsMinScreen = Camera.main.WorldToScreenPoint(boundsMin);
        boundsMaxScreen = Camera.main.WorldToScreenPoint(boundsMax);

        //Create and set material's mask texture to blank white
        float minX = boundsMinScreen.x;
        float maxX = boundsMaxScreen.x;
        int coverX = (int)(maxX-minX);
        float minY = boundsMinScreen.y;
        float maxY = boundsMaxScreen.y;
        int coverY = (int)(maxY - minY);
        maskTex = new Texture2D(coverX, coverY);
        int pix;
        Color32[] pixels = new Color32[coverX * coverY];
        for (pix = 0; pix < coverY * coverX; pix++)
        {
            pixels[pix] = Color.white;
        }
        maskTex.SetPixels32(pixels);
        maskTex.Apply();
        topCover.GetComponent<Renderer>().material.SetTexture("_MaskTex", maskTex);
    }
    // Update is called once per frame
    void Update()
    {
        //Get mouse position in Screen-Space
        var mousePos0 = Input.mousePosition;
        
        //Inefficient method of spawning an object used as a mask for the sprite every frame
        if (toggleInefficientMethod)
        {
            //Screen-Space -> World-Space mouse position
            var mousePos1 = Camera.main.ScreenToWorldPoint(mousePos0);
            mousePos1.z = 0;

            if (isPressed)
            {
                //Make a new mask sprite at the mouse's world-space position
                GameObject maskSprite = Instantiate(maskPrefab, mousePos1, Quaternion.identity);
                maskSprite.transform.parent = gameObject.transform;
            }

            //Mouse toggle
            if (Input.GetMouseButtonDown(0))
            {
                isPressed = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isPressed = false;
            }
        }
        //Now we get the efficient shader magic
        else
        {
            if (isPressed)
            {
                //Screen-space coordinates for both the mouse click and top-left of the cover
                Vector3 screenPos = new Vector3(boundsMinScreen.x, boundsMaxScreen.y, 0);
                Vector3 clickPos = Input.mousePosition;

                //Check to see if the mouse click is in the cover's bounds
                if ((clickPos.x >= boundsMinScreen.x && clickPos.x <= boundsMaxScreen.x) && (clickPos.y >= boundsMinScreen.y && clickPos.y <= boundsMaxScreen.y))
                {
                    //Grab relative coordinates of click in cover
                    Vector3 posInCover = calcClickInBounds(screenPos, clickPos);
                    
                    //Append a black circle to the mask, centered at the mouse click
                    drawTest(maskTex, posInCover);
                }
            }

            //Mouse toggle
            if (Input.GetMouseButtonDown(0))
            {
                isPressed = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isPressed = false;
            }
            
        }
    }

    private Vector3 calcClickInBounds(Vector3 boundTopLeftScreen, Vector3 clickScreen)
    {
        //Calculate relative values
        float x = clickScreen.x - boundTopLeftScreen.x;
        float y = boundTopLeftScreen.y - clickScreen.y;
        float z = boundTopLeftScreen.z;

        return new Vector3(x, y, z);
    }

    private void drawTest(Texture2D maskTexture, Vector3 relativePoint)
    {
        //Center of click
        int cx = (int)relativePoint.x;
        int cy = (int)-relativePoint.y; //Negative for some reason or else y-axis is drawn inverted

        //Mask is drawn on black for transparent sections
        Color col = Color.black;

        //Initialize temporary values
        int x, y, px, nx, py, ny, d;

        //Draw a circle (not gonna lie, copied the code from a random forum post from like 2012)
        for (x = 0; x <= circleRadius; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(circleRadius * circleRadius - x * x));
            for (y = 0; y <= d; y++)
            {
                //I wish I knew what these meant...
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                //Set the pixel at these locations to black
                maskTexture.SetPixel(px, py, col);
                maskTexture.SetPixel(nx, py, col);
                maskTexture.SetPixel(px, ny, col);
                maskTexture.SetPixel(nx, ny, col);

            }
        }
        //Better apply the changes to the texture or you'll wonder why it wasn't updating for 30 minutes
        maskTexture.Apply();
    }

}
