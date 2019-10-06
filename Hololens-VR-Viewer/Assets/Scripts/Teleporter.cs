using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private int targetLayer = 1 << 9; // Layer 9 (Floor)
    public LineRenderer previewLine;
    public GameObject previewSpot;

    public GameObject player;

    private bool wasHeld = false;
    private bool hasTarget = false;
    private Vector3 tpTarget = new Vector3();

    void ShowRaycast() {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, targetLayer)) {
            tpTarget = hit.point;
            previewSpot.transform.position = new Vector3(tpTarget.x, -0.9f, tpTarget.z);
            hasTarget = true;

            previewLine.SetPosition(0, transform.position);
            previewLine.SetPosition(1, tpTarget);
        }
        else
        {
            previewLine.SetPosition(0, transform.position);
            previewLine.SetPosition(1, transform.position + transform.TransformDirection(Vector3.forward) * 100);
            hasTarget = false;
        }
    }

    void Update()
    {
        if (!wasHeld) {
            Vector3 hideVec = new Vector3(0, -100, 0);
            previewLine.SetPosition(0, hideVec);
            previewLine.SetPosition(1, hideVec);
            previewSpot.transform.position = hideVec;
        }

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.4f) {
            wasHeld = true;
            ShowRaycast();
        } else if (wasHeld && hasTarget) {
            // Teleport Player
            player.transform.position = new Vector3(tpTarget.x, player.transform.position.y, tpTarget.z);
            wasHeld = false;
        }
    }
}
