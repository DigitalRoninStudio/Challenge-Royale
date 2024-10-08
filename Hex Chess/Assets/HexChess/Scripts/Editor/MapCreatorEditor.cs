using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapEditor))]
public class MapCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapEditor mapCreator = (MapEditor)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Create"))
        {
            mapCreator.CreateMap();
        }
        if (GUILayout.Button("Save"))
        {
            mapCreator.SaveMap();
        }
        if (GUILayout.Button("Load"))
        {
            mapCreator.LoadMap();
        }
        if (GUILayout.Button("Clear"))
        {
            mapCreator.ClearMap();
        }
        if (GUILayout.Button("Delete"))
        {
            mapCreator.DeleteMap();
        }
    }
}
