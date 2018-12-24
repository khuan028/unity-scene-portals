using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTransitionManager : MonoBehaviour {

	private bool teleporting = false;
	public bool allowSceneActivation = true;

	/// <summary>
	/// Is a teleport operation occuring?
	/// </summary>
	/// <value></value>
	public bool Teleporting {
		get {
			return teleporting;
		}
	}

	private static PortalTransitionManager _ins;

	/// <summary>
	/// The singleton instance of PortalTransitionManager
	/// </summary>
	/// <value></value>
	public static PortalTransitionManager Instance {
		get {
			return _ins;
		}
	}

	/// <summary>
	/// An event that is raised when a teleportation operation starts
	/// </summary>
	public System.Action OnTeleportStart;

	/// <summary>
	/// An event that is raised when a teleportation operation finishes
	/// </summary>
	public System.Action OnTeleportFinish;

	void Awake () {
		if (_ins == null) {
			_ins = this;
			//DontDestroyOnLoad (this.gameObject);
		} else {
			Destroy (this.gameObject);
		}
	}

	/// <summary>
	/// Start a teleport operation
	/// </summary>
	/// <param name="scenePath">Path of the destination scene</param>
	/// <param name="id">ID of the destination portal</param>
	public void Teleport (string scenePath, int id) {
		if (teleporting) {
			Debug.LogError ("Cannot start a new teleport operation amidst an ongoing teleport operation.");
		} else if (scenePath == string.Empty) {
			Debug.LogWarning ("Path is empty");
		} else {
			teleporting = true;
			Scene[] scenesToUnload = { SceneManager.GetActiveScene () };
			StartCoroutine (TeleportOperation (scenePath, id, scenesToUnload));
		}
	}

	private IEnumerator TeleportOperation (string scenePath, int id, Scene[] scenesToUnload) {

		// Exit early if scene cannot be found in build settings
		if(SceneUtility.GetBuildIndexByScenePath(scenePath) == -1) {
			Debug.LogError("The scene " + scenePath + " could not be found in the build settings. This error happens fairly often and can be fixed easily. Just clear all scenes from your build setting, and drag the scenes back into the build settings.");
			teleporting = false;
			yield break;
		}

		if (OnTeleportStart != null) {
			OnTeleportStart ();
		}

		// Change scenes if necessary
		if (SceneManager.GetActiveScene ().path != scenePath) {
			// Start scene operations for loading the next scene and unloading the specified scenes
			List<AsyncOperation> sceneOperations = new List<AsyncOperation> ();
			if (scenesToUnload != null) {
				foreach (Scene s in scenesToUnload) {
					sceneOperations.Add (SceneManager.UnloadSceneAsync (s));
				}
			}

			// Load next scene
			AsyncOperation nextSceneLoad = SceneManager.LoadSceneAsync (scenePath, LoadSceneMode.Additive);
			nextSceneLoad.allowSceneActivation = false;

			// Wait for all scene operations to finish
			foreach (AsyncOperation s in sceneOperations) {
				yield return new WaitUntil (() => s.isDone);
			}

			// Wait for next scene to finish loading
			yield return new WaitUntil (() => nextSceneLoad.progress >= 0.9f);

			// Wait until scene activation is allowed
			yield return new WaitUntil(() => allowSceneActivation);

			// Activate next scene
			nextSceneLoad.allowSceneActivation = true;
			yield return new WaitUntil (() => nextSceneLoad.isDone);
			SceneManager.SetActiveScene (SceneManager.GetSceneByPath (scenePath));
		}

		PortalComponent exitPortal = PortalComponentCache.GetPortal (id);
		if (exitPortal == null) {
			Debug.LogError ("Could not find destination portal with id = " + id + " in scene \"" + SceneManager.GetActiveScene ().path + "\"");
		} else {
			exitPortal.Exit ();
		}

		teleporting = false;

		if (OnTeleportFinish != null) {
			OnTeleportFinish ();
		}
	}

}