using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class whereisping : MonoBehaviour
{

    public GameObject Cube;
    
    public GameObject PingTarget;

    private Camera maincam;
    Vector3 objcoord, topleft, topright, bottomleft, bottomright = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        maincam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newpos;
        float tempx, tempy;
        
        tempx = PingTarget.transform.position.x;
        tempy = PingTarget.transform.position.y;

        objcoord = maincam.transform.InverseTransformPoint(PingTarget.transform.position);

        if(objcoord.x > 0.45f || (objcoord.z < 0 && objcoord.x > 0))
            tempx = 0.45f;
        else if(objcoord.x < -0.45f || (objcoord.z < 0 && objcoord.x < 0))
            tempx = -0.45f;
        else tempx = objcoord.x;

        if((objcoord.y > 0.25f && objcoord.z > 0))
            tempy = 0.25f;
        else if(objcoord.y < -0.25f)
            tempy = -0.25f;
        else tempy = objcoord.y;
        
        if(objcoord.z < 0)
            tempy = 0;

        if(objcoord.x < 0.5f && objcoord.x > -0.5f && objcoord.y < 0.3f && objcoord.y > -0.3f)
            Cube.transform.GetComponent<Renderer>().enabled = false;
        else
            Cube.transform.GetComponent<Renderer>().enabled = true;
        
        newpos = new Vector3(tempx, tempy, 2); 
        Cube.transform.localPosition = newpos;

        Debug.DrawRay(maincam.transform.position, PingTarget.transform.position, Color.yellow);

        Cube.transform.LookAt(new Vector3(PingTarget.transform.position.x, maincam.transform.position.y, PingTarget.transform.position.z));
    }
}
