using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapCreator))]
public class MapCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapCreator mapCreator = (MapCreator)target;

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
