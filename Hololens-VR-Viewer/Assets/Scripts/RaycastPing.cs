using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastPing : MonoBehaviour
{
    private int targetLayer = 1 << 8; // Layer 8 (environment)
    public GameObject preview;
    public MessageServer sender;
    public LineRenderer previewLine;

    private bool isDown = false;

    void Start()
    {
    }

    void Update()
    {
        if (!isDown) isDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);

        if (isDown) {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetLayer)) {

                // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                // Debug.Log("Did Hit");
                preview.transform.position = hit.point;
                previewLine.SetPosition(0, transform.position);
                previewLine.SetPosition(1, hit.point);
                sender.SendRay(transform.position, hit.point);
                sender.SendPing(hit.point);
            }
            else
            {
                sender.SendRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000);
                sender.SendPing(new Vector3(0, -100, 0));
            }

        }

        if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch)) {
            isDown = false;
            RaycastHit hit;
            sender.SendRay(Vector3.zero, Vector3.zero);
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetLayer)) {
                preview.transform.position = hit.point;
                sender.SetPing(hit.point);
                sender.SendRay(Vector3.zero, Vector3.zero); // gets rid of ray
                Vector3 hideVec = new Vector3(0, -100, 0);
                previewLine.SetPosition(0, hideVec);
                previewLine.SetPosition(1, hideVec);
            }
        }
    }
}
