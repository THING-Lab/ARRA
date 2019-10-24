using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanController : MonoBehaviour
{
    public GameObject[] environments;
    private int currentEnv = 0;
    private bool wasTriggerPress = false;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < environments.Length; i++) {
            environments[i].SetActive(false);
        }

        environments[currentEnv].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        bool triggerVal = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch);
        if (triggerVal)
        {
            environments[currentEnv].SetActive(false);

            currentEnv += 1;
            if (currentEnv >= environments.Length) currentEnv = 0;

            environments[currentEnv].SetActive(true);
        }
        /*if (triggerVal > 0.4f && !wasTriggerPress) {
           
        }

        if (triggerVal > 0.4f) {
            wasTriggerPress = true;
        } else {
            wasTriggerPress = false;
        }*/
    }
}
