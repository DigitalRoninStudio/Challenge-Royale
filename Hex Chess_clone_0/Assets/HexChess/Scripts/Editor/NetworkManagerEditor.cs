#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkManager networkManager = (NetworkManager)target;

       /* if (GUILayout.Button("Start Server"))
        {
            networkManager.StartServer();
        }

        if (GUILayout.Button("Connect to Server"))
        {
            networkManager.ConnectToServer();
        }*/
    }
}
#endif