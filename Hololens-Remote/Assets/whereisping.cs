using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whereisping : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject pingTarget;
    public GameObject Cube;
    public Camera maincam;
    public Vector3 RotateAmount;

    Vector3 point = new Vector3();
    
    Vector2 mousePos = new Vector2();

    float smooth = 5.0f;
    float tiltAngle = 60.0f;
    void Start()
    {
        maincam = Camera.main;
    }

void OnGUI()
    {
        Event currentEvent = Event.current;

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        //mousePos.x = currentEvent.mousePosition.x;
        //mousePos.y = maincam.pixelHeight - currentEvent.mousePosition.y;
        if(currentEvent.button == 0 && currentEvent.isMouse){
        point = maincam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, maincam.nearClipPlane));

         mousePos =
                 new Vector2(maincam.ScreenToWorldPoint(Input.mousePosition).x,
                 maincam.ScreenToWorldPoint(Input.mousePosition).y);

        Debug.Log("Screen pixels: " + maincam.pixelWidth + ":" + maincam.pixelHeight);
        Debug.Log("Mouse position: " + mousePos);
        Debug.Log("World position: " + point.ToString("F3"));
        }
    }
    // Update is called once per frame
    void Update()
    {
        //Cube.transform.Rotate(0, 50 * Time.deltaTime, 0);
        Cube.transform.position = new Vector3(point.x, point.y, point.z+0.5f);
    }
}
