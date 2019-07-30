using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPing : MonoBehaviour
{
    private int targetLayer = 1 << 8; // Layer 8 (environment)
    public GameObject preview;
    public MessageServer sender;

    void Start()
    {
    }

    void Update()
    {
        // Debug.Log(OVRInput.GetDown(OVRInput.Button.One));
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetLayer)) {
            
                // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                // Debug.Log("Did Hit");
                preview.transform.position = hit.point;
                sender.SendPing(hit.point);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
        }
    }
}
