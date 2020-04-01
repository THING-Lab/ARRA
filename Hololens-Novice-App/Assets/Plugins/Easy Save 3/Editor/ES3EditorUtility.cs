using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using ES3Internal;

public class ES3EditorUtility : Editor 
{
    public static string pathToEasySaveFolder = null;

	public static string PathToEasySaveFolder()
	{
        // If the path has not yet been cached, get the path and cache it.
        if (string.IsNullOrEmpty(pathToEasySaveFolder))
        {
            string[] guids = AssetDatabase.FindAssets("ES3Window");
            if (guids.Length == 0)
                Debug.LogError("Could not locate the Easy Save 3 folder because the ES3Window script has been moved or removed.");
            if (guids.Length > 1)
                Debug.LogError("Could not locate the Easy Save 3 folder because more than one ES3Window script exists in the project, but this needs to be unique to locate the folder.");

            pathToEasySaveFolder = AssetDatabase.GUIDToAssetPath(guids[0]).Split(new string[] { "Editor" }, System.StringSplitOptions.RemoveEmptyEntries)[0];
        }
        return pathToEasySaveFolder;
    }

    public static string PathToDefaultSettings()
    {
        return ES3EditorUtility.PathToEasySaveFolder() + "Resources/ES3/ES3 Default Settings.prefab";
    }

	public static void DisplayLink(string label, string url)
	{
		var style = ES3Editor.EditorStyle.Get;
		if(GUILayout.Button(label, style.link))
			Application.OpenURL(url);

		var buttonRect = GUILayoutUtility.GetLastRect();
		buttonRect.width = style.link.CalcSize(new GUIContent(label)).x;

		EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);	
	}

	public static bool IsPrefabInAssets(UnityEngine.Object obj)
	{
		#if UNITY_2018_3_OR_NEWER
		return PrefabUtility.IsPartOfPrefabAsset(obj);
		#else
		return (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab);
		#endif
	}

    /* 
     * Gets all children and components from a GameObject or GameObjects.
     * We create our own method for this because EditorUtility.CollectDeepHierarchy isn't thread safe in the Editor.
     */
    public static IEnumerable<UnityEngine.Object> CollectDeepHierarchy(IEnumerable<GameObject> gos)
    {
        var deepHierarchy = new HashSet<UnityEngine.Object>();
        foreach (var go in gos)
        {
            deepHierarchy.Add(go);
            deepHierarchy.UnionWith(go.GetComponents<Component>());
            foreach (Transform t in go.transform)
                deepHierarchy.UnionWith( CollectDeepHierarchy( new GameObject[] { t.gameObject } ) );
        }
        return deepHierarchy;
    }
}
