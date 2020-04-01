using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrokeParser : MonoBehaviour
{
    private List<GameObject> suggestions = new List<GameObject>();
    public GameObject strokePrefab;

    public void AddStroke(List<Vector3> points) {
        GameObject newStroke = Instantiate(strokePrefab);
        LineRenderer line = newStroke.GetComponent<LineRenderer>();
        suggestions.Add(newStroke);
        foreach (Vector3 point in points) {
            line.positionCount += 1;
            line.SetPosition(line.positionCount - 1, point);
        }
    }

    public void Clear() {
        foreach (GameObject go in suggestions) {
            Destroy(go);
        }
        suggestions.Clear();
    }
}
