using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class RoomScanSender : MonoBehaviour
{
    // Boundary vis
    public bool boundaryVisualizationFound = false;
    public GameObject boundaryVis;
    private bool shouldShowScan = false;
    private bool isSendingScan = false;
    public ScanIndicator indicator;

    // Gesture stuff
    GestureRecognizer recognizer;

    // Scan timer and instructions
    public float scanWindow = 10f;
    private float scanTime = 0f;
    private bool isScanning = false;
    private MessageManager sender;

    private List<ScanMesh> scanMeshes = new List<ScanMesh>();
    private List<ScanTexture> scanTextures = new List<ScanTexture>();

    // Start is called before the first frame update
    void Awake()
    {
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        indicator.SetCannotScan();
        recognizer.Tapped += (args) => {
            if (boundaryVisualizationFound && !isScanning && !isSendingScan) {
                shouldShowScan = true;
                isScanning = true;
                scanTime = 0f;
                indicator.SetIsScanning();
            } else if (isScanning) {
                // generate mesh store and queue sending coroutine
                scanMeshes.Clear();
                foreach(MeshFilter mf in boundaryVis.transform.GetChild(0).GetComponentsInChildren<MeshFilter>()) {
                    scanMeshes.Add(new ScanMesh(
                        mf.transform.position,
                        mf.transform.rotation,
                        mf.mesh.vertices,
                        mf.mesh.triangles,
                        mf.mesh.uv
                    ));
                }
                /*
                foreach(MeshRenderer mr in GameObject.Find("Model Generator").transform.GetChild(0).GetComponentsInChildren<MeshRenderer>()) {
                    scanTextures.Add(new ScanTexture(
                        mr.sharedMaterial.GetTexture("_MyArr"),
                        mr.sharedMaterial.GetMatrixArray("_WorldToCameraMatrixArray"),
                        mr.sharedMaterial.GetMatrixArray("_CameraProjectionMatrixArray"),
                        mr.sharedMaterial.GetMatrixArray("_MyObjectToWorld")
                    ));
                }
                */
                StartCoroutine("SendScan");

                isSendingScan = true;
                isScanning = false;
                shouldShowScan = false;
            }
        };
        recognizer.StartCapturingGestures();

        sender = GetComponent<MessageManager>();
    }

    IEnumerator SendScan() {
        Debug.Log("Begin sending scan");
        int scanCount = 0;
        foreach(ScanMesh sm in scanMeshes) {
            // Debug.Log(JsonUtility.ToJson(sm));
            scanCount += 1;
            int chunkSize = 4000;
            // Save all mesh data to a strin
            string meshString = JsonUtility.ToJson(sm);

            // While mesh size exceeds chunk
            while (meshString.Length > chunkSize) {
                // create a mesh chunk from the mesh string
                string chunkPacket = JsonUtility.ToJson(new MeshChunk(meshString.Substring(0, chunkSize), false));
                // send the chunk
                sender.SendJSON(JsonUtility.ToJson(new JSONPacket("MESH_CHUNK", chunkPacket)));

                // remove the sent chunk from the string
                meshString = meshString.Substring(chunkSize);
                // give our coroutine some breathing room
                yield return new WaitForSeconds(0.001f);
            }

            // send final chunk
            string finalPacket = JsonUtility.ToJson(new MeshChunk(meshString.Substring(0), true));
            // Debug.Log(scanCount);
            sender.SendJSON(JsonUtility.ToJson(new JSONPacket("MESH_CHUNK", finalPacket)));

            // HOW TO RECIEVE THIS
            // Parse json packet
            // See that it's mesh chunk
            // if no current mesh to build, create a new one (it's just a string)
            // add chunk's data to string
            // if chunk is last chunk, parse scan mesh from json and add to scene. reset scan mesh build string

            // I'm doin this to limit the frequency we send meshes and potentially make it smoother
            yield return new WaitForSeconds(0.1f);
        }
        indicator.SetCanScan();
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
                indicator.SetCanScan();
            }
        } else {
            if (boundaryVis != null) {
                boundaryVis.SetActive(shouldShowScan);
            } else {
                boundaryVisualizationFound = false;
                isScanning = false;
                shouldShowScan = false;
            }
        }
    }
}
