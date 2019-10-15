using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuggestionManager : MonoBehaviour
{
    private List<GameObject> suggestions = new List<GameObject>();
    public GameObject PingPrefab;

    public void AddPing(Vector3 pos) {
        GameObject newPing = Instantiate(PingPrefab);
        suggestions.Add(newPing);
        newPing.transform.parent = transform;
        newPing.transform.position = pos;
    }

    public void Clear() {
        foreach (GameObject go in suggestions) {
            Destroy(go);
        }
        suggestions.Clear();
    }
}
