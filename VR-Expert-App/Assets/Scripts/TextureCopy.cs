using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureCopy : MonoBehaviour
{
    public GameObject copySource;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.mainTexture = copySource.GetComponent<Renderer>().material.mainTexture;
    }
}
