using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuggestionManager : MonoBehaviour
{
    private List<GameObject> suggestions = new List<GameObject>();
    public GameObject PingPrefab;
    public GameObject StrokePrefab;

    private GameObject currentPing;
    private LineRenderer currentStroke;

    public LineRenderer CurrentStroke {
        get { return currentStroke; }
    }
    
    public void SetPing(Vector3 pos) {
        if (currentPing != null) currentPing.transform.position = pos;
    }
    public GameObject CreatePing(Vector3 pos) {
        GameObject newPing = Instantiate(PingPrefab);
        suggestions.Add(newPing);
        newPing.transform.parent = transform;
        newPing.transform.position = pos;
        return newPing;
    }

    public Vector3 GetLastStrokePoint() {
        return currentStroke.GetPosition(currentStroke.positionCount - 1);
    }

    public void AddStrokePoint(Vector3 point) {
        currentStroke.positionCount += 1;
        currentStroke.SetPosition(currentStroke.positionCount - 1, point);
    }

    public GameObject CreateStroke(Vector3 firstPos) {
        GameObject newStroke = Instantiate(StrokePrefab);
        suggestions.Add(newStroke);
        newStroke.transform.parent = transform;
        currentStroke = newStroke.GetComponent<LineRenderer>();
        currentStroke.positionCount += 1;
        currentStroke.SetPosition(currentStroke.positionCount - 1, firstPos);
        return newStroke;
    }

    public void Clear() {
        foreach (GameObject go in suggestions) {
            Destroy(go);
        }
        suggestions.Clear();
    }
}
