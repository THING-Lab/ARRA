using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class RoomScanSender : MonoBehaviour
{
    // Boundary vis
    private bool boundaryVisualizationFound = false;
    private GameObject boundaryVis;
    private bool shouldShowScan = false;
    private bool isSendingScan = false;

    // Gesture stuff
    GestureRecognizer recognizer;

    // Scan timer and instructions
    public float scanWindow = 10f;
    private float scanTime = 0f;
    private bool isScanning = false;
    private MessageManager sender;

    private List<ScanMesh> scanMeshes = new List<ScanMesh>();

    // Start is called before the first frame update
    void Awake()
    {
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.Tapped += (args) => {
            if (boundaryVisualizationFound && !isScanning && !isSendingScan) {
                shouldShowScan = true;
                isScanning = true;
                scanTime = 0f;
            }
        };
        recognizer.StartCapturingGestures();

        sender = GetComponent<MessageManager>();
    }

    IEnumerator SendScan() {
        Debug.Log("Begin sending scan");
        foreach(ScanMesh sm in scanMeshes) {
            // Debug.Log(JsonUtility.ToJson(sm));
            sender.SendJSON(JsonUtility.ToJson(new JSONPacket("SCAN_MESH", JsonUtility.ToJson(sm))));
            // I'm doin this to limit the frequency we send meshes and potentially make it smoother
            yield return new WaitForSeconds(0.1f);
        }
        isSendingScan = false;
        Debug.Log("End sending scan");
    }

    // Update is called once per frame
    void Update()
    {
        if (!boundaryVisualizationFound) {
            if (GameObject.Find("Spatial Awareness System") != null) {
                boundaryVisualizationFound = true;
                boundaryVis = GameObject.Find("Spatial Awareness System");
                Debug.Log("Boundary Vis Located and Hidden");
            }
        } else {
            if (boundaryVis != null) {
                boundaryVis.SetActive(shouldShowScan);
            } else {
                boundaryVisualizationFound = false;
                isScanning = false;
                shouldShowScan = false;
            }

            if (isScanning) {
                if (scanTime < scanWindow) {
                    scanTime += Time.deltaTime;
                } else {
                    // generate mesh store and queue sending coroutine
                    foreach(MeshFilter mf in boundaryVis.transform.GetChild(0).GetComponentsInChildren<MeshFilter>()) {
                        scanMeshes.Add(new ScanMesh(
                            mf.transform.position,
                            mf.transform.rotation,
                            mf.mesh.vertices,
                            mf.mesh.triangles,
                            mf.mesh.uv
                        ));
                    }
                    StartCoroutine("SendScan");

                    isSendingScan = true;
                    isScanning = false;
                    shouldShowScan = false;
                }
            }
        }
    }
}
