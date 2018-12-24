using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor (typeof (PortalComponent))]
public class PortalComponentInspector : Editor {

	public static GUIStyle red;

	void OnEnable() {
		red = new GUIStyle();
		red.normal.textColor = Color.red * 0.7f;
		red.fontStyle = FontStyle.Bold;
	}

	public override void OnInspectorGUI () {
		PortalComponent tar = target as PortalComponent;

		SerializedProperty id = serializedObject.FindProperty ("id");
		SerializedProperty destinationScene = serializedObject.FindProperty ("destinationScene");
		SerializedProperty destinationId = serializedObject.FindProperty ("destinationId");

		bool wideLayout = EditorGUIUtility.currentViewWidth > 450f;

		bool wantViewDestinationPortal = false;

		EditorGUI.BeginChangeCheck ();
		GUILayout.Space (10);
		Horizontal (wideLayout, () => {
			LabelWidth (40f, wideLayout, () => {
				Vertical (() => {
					GUILayout.Label ("My Properties", EditorStyles.boldLabel);
					EditorGUILayout.PropertyField (id);
				});
			});

			GUILayout.Space (15);

			LabelWidth (130f, wideLayout, () => {
				Vertical (() => {
					GUILayout.Label ("Destination", EditorStyles.boldLabel);
					EditorGUILayout.PropertyField (destinationScene);
					Enabled (tar.destinationScene.ScenePath != string.Empty, () => {
						EditorGUILayout.PropertyField (destinationId);
						if (tar.destinationScene.ScenePath == tar.gameObject.scene.path && tar.destinationId == tar.id) {
							GUILayout.Label ("This portal connects to itself", red);
						}
					});
					Enabled (tar.destinationScene.ScenePath != string.Empty, () => {
						GUILayout.Space (10);
						if (GUILayout.Button ("View destination portal", GUILayout.Height (25))) {
							// Call to ViewDestinationPortal must be deferred until after all GUI operations
							wantViewDestinationPortal = true;
						}
					});
				});
			});
		});
		GUILayout.Space (10);
		if (EditorGUI.EndChangeCheck ()) {
			serializedObject.ApplyModifiedProperties ();
		}

		if (wantViewDestinationPortal) {
			ViewDestinationPortal (tar.destinationScene.ScenePath, tar.destinationId);
		}

	}

	public static void ViewDestinationPortal (string scenePath, int destinationId) {
		// Check if we are already in the scene
		bool inSceneAlready = SceneManager.GetActiveScene ().path == scenePath ||
			SceneManager.GetSceneByPath (scenePath).IsValid ();

		if (!inSceneAlready) {
			// Require that modified scenes be saved before proceeding
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) {
				return;
			}

			Scene nextScene = EditorSceneManager.OpenScene (scenePath, OpenSceneMode.Single);
			if (!nextScene.IsValid ()) {
				Debug.LogError ("Could not find scene asset \"" + scenePath + "\"");
				return;
			}
		}

		PortalComponent destination = PortalComponentCache.GetPortal (destinationId);

		if (destination == null) {
			Debug.LogError ("Could not find the portal with id = " + destinationId + " in \"" + scenePath + "\"");
			return;
		}

		Selection.activeGameObject = destination.gameObject;
		SceneView.FrameLastActiveSceneView ();

	}

	/// <summary>
	/// Wrapper for GUI.enabled
	/// </summary>
	/// <param name="enabled">Should GUI be enabled?</param>
	/// <param name="body">Code within</param>
	void Enabled (bool enabled, System.Action body) {
		bool cachedEnable = GUI.enabled;
		GUI.enabled = enabled;
		body ();
		GUI.enabled = cachedEnable;
	}

	/// <summary>
	/// Wrapper for EditorGUIUtility.labelWidth
	/// </summary>
	/// <param name="width">Label width</param>
	/// <param name="condition">Enable changes to label width?</param>
	/// <param name="body">Code widthin</param>
	void LabelWidth (float width, bool condition, System.Action body) {
		float cachedWidth = EditorGUIUtility.labelWidth;
		if (condition) {
			EditorGUIUtility.labelWidth = width;
		}
		body ();
		if (condition) {
			EditorGUIUtility.labelWidth = cachedWidth;
		}
	}

	/// <summary>
	/// Wrapper for EditorGUILayout.BeginHorizontal() and EditorGUILayout.EndHorizontal()
	/// </summary>
	/// <param name="condition">Enable horizontal layout?</param>
	/// <param name="body">Code within</param>
	void Horizontal (bool condition, System.Action body) {
		if (condition) {
			EditorGUILayout.BeginHorizontal ();
		}
		body ();
		if (condition) {
			EditorGUILayout.EndHorizontal ();
		}
	}

	/// <summary>
	/// Wrapper for EditorGUILayout.BeginHorizontal() and EditorGUILayout.EndHorizontal()
	/// </summary>
	/// <param name="body">Code within</param>
	void Horizontal (System.Action body) {
		EditorGUILayout.BeginHorizontal ();
		body ();
		EditorGUILayout.EndHorizontal ();
	}

	/// <summary>
	/// Wrapper for EditorGUILayout.BeginVertical() and EditorGUILayout.EndVertical()
	/// </summary>
	/// <param name="condition">Enable vertical layout?</param>
	/// <param name="body">Code within</param>
	void Vertical (bool condition, System.Action body) {
		if (condition) {
			EditorGUILayout.BeginVertical ();
		}
		body ();
		if (condition) {
			EditorGUILayout.EndVertical ();
		}
	}

	/// <summary>
	/// Wrapper for EditorGUIlayout.BeginVertical() and EditorGUILayout.EndVertical()
	/// </summary>
	/// <param name="body">Code within</param>
	void Vertical (System.Action body) {
		EditorGUILayout.BeginVertical ();
		body ();
		EditorGUILayout.EndVertical ();
	}

	void Colorize (Color color, System.Action body) {
		Color cachedColor = GUI.color;
		GUI.color = color;
		body ();
		GUI.color = cachedColor;
	}

}