using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Swing.Editor {
	public static class SceneUtility {

		private class SceneUtilityRunner { }

		private static SceneUtilityRunner runner = null;

		public static string[] findAllScenePaths () {
			var guids = AssetDatabase.FindAssets ("t:Scene");
			var paths = Array.ConvertAll<string, string> (guids, AssetDatabase.GUIDToAssetPath);
			paths = Array.FindAll (paths, File.Exists); // Unity erroneously considers folders named something.unity as scenes, remove them
			return paths;
		}

		public delegate void ProcessAllScenesDelegate (string _scenePath, Scene _sceneObject, SceneAsset _sceneAsset);

		public static void processAllScenes (ProcessAllScenesDelegate _callback) {
			processAllScenes ("Processing {0} Scenes", "Processing scene {1}/{0} : {2}", _callback);
		}

		/// <summary>
		/// Format {0} : scene count
		/// Format {1} : scene index
		/// Format {2} : scene path
		/// </summary>
		public static void processAllScenes (
			string _titleFormat,
			string _messageFormat,
			ProcessAllScenesDelegate _callback,
			System.Action _postProcess = null) {
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ()) {
				return;
			}

			if(runner == null) {
				runner = new SceneUtilityRunner();
			}
			runner.StartCoroutine (
				processAllScenesCoroutine (_titleFormat, _messageFormat, _callback, _postProcess));
		}


		static IEnumerator processAllScenesCoroutine (string _titleFormat, string _messageFormat, ProcessAllScenesDelegate _callback, System.Action _postProcess) {

			SceneSetup[] beforeSetup = EditorSceneManager.GetSceneManagerSetup();

			var scenePaths = findAllScenePaths ();
			var sceneCount = scenePaths.Length;
			Debug.Log (string.Format ("Processing {0} scenes", sceneCount));

			for (int i = 0; i < sceneCount; i++) {
				var scenePath = scenePaths[i];

				EditorUtility.DisplayProgressBar (
					string.Format (_titleFormat, sceneCount, i, scenePath),
					string.Format (_messageFormat, sceneCount, i, scenePath),
					(float) i / sceneCount);

				SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset)) as SceneAsset;
				Scene sceneObject = EditorSceneManager.OpenScene (scenePath);

				if (sceneObject.IsValid()) {
					// delay one frame to give a chance for all Awake/Start/OnEnable callbacks to trigger
					yield return null;

					try {
						_callback (scenePath, sceneObject, sceneAsset);
					} catch (Exception e) {
						Debug.LogError (string.Format ("Error while processing scene  '{0}'", scenePath), sceneAsset);
						Debug.LogException (e);
					}
				} else {
					Debug.LogError (string.Format ("Failed to open scene '{0}'", scenePath), sceneAsset);
				}
			}

			EditorUtility.ClearProgressBar ();
			EditorSceneManager.RestoreSceneManagerSetup(beforeSetup);

			if(_postProcess != null) {
				_postProcess();
			}
			
		}

		
	}
}