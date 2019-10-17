using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuggestionTool : MonoBehaviour
{
    private int targetLayer = 1 << 8; // Layer 8 (environment)
    public GameObject preview;
    public MessageServer sender;
    public LineRenderer previewLine;
    public SuggestionManager manager;

    private bool isDown = false;
    private bool isDrawing = false;

    void Update()
    {
        if (!isDown) {
            bool currentDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
            if (!isDown && currentDown) manager.CreatePing(new Vector3(0, -100, 0));
            isDown = currentDown;
        }

        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch)) {
            manager.Clear();
            sender.SendClear();
        }

        if (isDown) {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetLayer)) {

                // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                // Debug.Log("Did Hit");
                // preview.transform.position = hit.point;
                manager.SetPing(hit.point);
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

        // Stroke Logic
        if (!isDrawing && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.5f) {
            manager.CreateStroke(transform.position); // maybe add a pen point for this
            isDrawing = true;
        }

        if (isDrawing && OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) < 0.4f) {
            isDrawing = false;
            sender.SendStroke(manager.CurrentStroke);
        }

        // While the pen is drawing
        if (isDrawing) {
            // compute a vector that points from the last position to the current position
            Vector3 distanceVec = transform.position - manager.GetLastStrokePoint();
            
            // Add a point if the current position is far enough away
            if (distanceVec.sqrMagnitude > 0.01 * 0.01) manager.AddStrokePoint(transform.position);
        }
    }
}
