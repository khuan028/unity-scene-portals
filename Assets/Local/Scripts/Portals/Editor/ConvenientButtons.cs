using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConvenientButtonsWindow : EditorWindow {

	private static string key_sceneToLoad = "ConvenientButtonsWindow.sceneToLoad";

	[MenuItem ("Tools/Convenient Buttons")]
	public static void ShowWindow () {
		ConvenientButtonsWindow window = EditorWindow.GetWindow (typeof (ConvenientButtonsWindow), false, "Convenient", true) as ConvenientButtonsWindow;
		if(EditorPrefs.HasKey(key_sceneToLoad)) {
			string sceneToLoadPath = EditorPrefs.GetString(key_sceneToLoad);
			window.sceneToLoad = (SceneAsset) AssetDatabase.LoadAssetAtPath(sceneToLoadPath, typeof(SceneAsset));
		}
		
	}

	public SceneAsset sceneToLoad;

	void OnGUI() {
		EditorGUI.BeginChangeCheck();
		sceneToLoad = (SceneAsset) EditorGUILayout.ObjectField(sceneToLoad, typeof(SceneAsset), false);
		if(EditorGUI.EndChangeCheck()) {
			string sceneToLoadPath = string.Empty;
			if(sceneToLoad != null) {
				sceneToLoadPath = AssetDatabase.GetAssetPath(sceneToLoad);
			}
			EditorPrefs.SetString(key_sceneToLoad, sceneToLoadPath);
		}

		GUI.enabled = (sceneToLoad != null);
		if(GUILayout.Button(new GUIContent("Open Scene Additively", "Load this scene in the editor without unloading existing scenes"), GUILayout.Height(30))) {
			string path = AssetDatabase.GetAssetPath(sceneToLoad);
			EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
		}
	}

}
