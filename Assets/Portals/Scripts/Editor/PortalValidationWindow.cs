using System.Collections;
using System.Collections.Generic;
using Swing.Editor;
using UnityEditor;
using UnityEngine;

public class PortalValidationWindow : EditorWindow {

	public struct PError {
		public SceneAsset scene;
		public List<string> msg;

		public PError (SceneAsset sc) {
			scene = sc;
			msg = new List<string>();
		}

		public string Msg {
			get {
				return string.Join("\n", msg.ToArray());
			}
		}
	}

	public List<PError> portalErrors = new List<PError> ();
	public bool showEverythingIsGood = false;
	public bool checkForDisconnectedPortals = false;

	[MenuItem ("Tools/Portals/Validation")]
	public static void ShowWindow () {
		PortalValidationWindow window = EditorWindow.GetWindow (typeof (PortalValidationWindow), false, "Validate Portal", true) as PortalValidationWindow;
		window.showEverythingIsGood = false;
	}

	void OnGUI () {
		EditorStyles.label.wordWrap = true;
		GUILayout.Space (10);
		if (GUILayout.Button ("Validate all portals", GUILayout.Height (30))) {
			Validate ();
		}
		GUILayout.Space (5);
		checkForDisconnectedPortals = EditorGUILayout.ToggleLeft (new GUIContent("Warn me of disconnected portals", "Raise an error message if a portal has no destination scene"), checkForDisconnectedPortals);
		GUILayout.Space (5);
		if (portalErrors.Count == 0) {
			if (showEverythingIsGood) {
				EditorGUILayout.HelpBox ("Everything's good! No errors found.", MessageType.Info);
			}
		} else {
			EditorGUILayout.HelpBox ("A few errors need to be fixed", MessageType.Warning);
			foreach (PError p in portalErrors) {
				Enabled (false, () => {
					EditorGUILayout.ObjectField (p.scene, typeof (SceneAsset), false);
				});
				Indented (2, () => {
					EditorGUILayout.LabelField (p.Msg);
				});
			}
		}
	}

	private struct PortalComponentData {
		public int id;
		public string destinationScenePath;
		public int destinationId;

		public PortalComponentData (int _id, string _ds, int _did) {
			id = _id;
			destinationScenePath = _ds;
			destinationId = _did;
		}
	}

	public void Validate () {
		string[] scenePaths = SceneUtility.findAllScenePaths ();
		var errors = new Dictionary<string, PError> ();
		var allPortalData = new Dictionary<string, Dictionary<int, PortalComponentData>> ();
		string errMsg = string.Empty;

		SceneUtility.processAllScenes (
			"Please wait",
			"Validating portals in {2}\nScenes processed: {1}/{0}",
			(scenePath, sceneObj, sceneAsset) => {
				// Add an entry for this scene
				if (!allPortalData.ContainsKey (scenePath)) {
					allPortalData.Add (scenePath, new Dictionary<int, PortalComponentData> ());
				}

				var idCount = new Dictionary<int, int> ();

				// Find all portals in this scene
				var portals = Finder.FindObjectsOfTypeAll<PortalComponent> ();

				foreach (PortalComponent portal in portals) {
					// Count # of times an id occurs in one scene
					if (idCount.ContainsKey (portal.id)) {
						idCount[portal.id] = idCount[portal.id] + 1;
					} else {
						idCount.Add (portal.id, 1);
					}

					// Check that id != destinationId
					if (portal.id == portal.destinationId && portal.destinationScene.ScenePath != string.Empty && scenePath == portal.destinationScene.ScenePath) {
						if (!errors.ContainsKey (scenePath)) {
							errors.Add (scenePath, new PError(sceneAsset));
						}
						errors[scenePath].msg.Add("- Portal " + portal.id + " connects to itself");
					}

					if ((portal.destinationScene == null || portal.destinationScene.ScenePath == string.Empty) && checkForDisconnectedPortals) {
						if (!errors.ContainsKey (scenePath)) {
							errors.Add (scenePath, new PError(sceneAsset));
						}
						errors[scenePath].msg.Add("- Portal " + portal.id + " has no destination");
					}

					// Add portal data to a cache so we can do cross-scene processing later
					var cachedPortalData = new PortalComponentData (portal.id, portal.destinationScene.ScenePath, portal.destinationId);
					if (!allPortalData[scenePath].ContainsKey (portal.id)) {
						allPortalData[scenePath].Add (portal.id, cachedPortalData);
					}
				}

				// Generate error message for duplicate id's
				errMsg = string.Empty;
				foreach (var pair in idCount) {
					if (pair.Value > 1) {
						errMsg += "- Found " + pair.Value + " portals with identical id = " + pair.Key + "\n";
					}
				}

				if (errMsg != string.Empty) {
					if (!errors.ContainsKey (scenePath)) {
						errors.Add (scenePath, new PError(sceneAsset));
					}
					errors[scenePath].msg.Add(errMsg);
				}
			},
			() => {
				// Perform cross scene processing
				// Find invalid destination portals
				foreach (string scenePath in scenePaths) {
					SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath (scenePath, typeof (SceneAsset)) as SceneAsset;
					var sceneData = allPortalData[scenePath];
					foreach (var pair in sceneData) {
						if (pair.Value.destinationScenePath != string.Empty && !allPortalData[pair.Value.destinationScenePath].ContainsKey (pair.Value.destinationId)) {
							if (!errors.ContainsKey (scenePath)) {
								errors.Add (scenePath, new PError(sceneAsset));
							}
							errors[scenePath].msg.Add("- Portal " + pair.Key + " is connected to a nonexistent destination portal " +
								pair.Value.destinationId + " in " + pair.Value.destinationScenePath);
						}
					}
				}

				// Generate list of errors
				this.portalErrors.Clear ();
				foreach (string scenePath in scenePaths) {
					if (errors.ContainsKey (scenePath)) {
						this.portalErrors.Add (errors[scenePath]);
					}
				}
				showEverythingIsGood = true;
			});
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

	void Indented (int indent, System.Action body) {
		EditorGUI.indentLevel += indent;
		body ();
		EditorGUI.indentLevel -= indent;
	}
}