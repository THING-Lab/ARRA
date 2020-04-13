using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanIndicator : MonoBehaviour
{
    public GameObject red;
    public GameObject green;
    public GameObject white;
    // Start is called before the first frame update
    void Start()
    {
        red.SetActive(true);
        green.SetActive(false);
        white.SetActive(false);
    }

    public void SetCanScan() {
        red.SetActive(false);
        green.SetActive(true);
        white.SetActive(false);
    }

    public void SetCannotScan() {
        red.SetActive(true);
        green.SetActive(false);
        white.SetActive(false);
    }

    public void SetIsScanning() {
        red.SetActive(false);
        green.SetActive(false);
        white.SetActive(true);
    }
}
