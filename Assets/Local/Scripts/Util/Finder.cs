using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class Finder {	
	/// <summary>
	/// Find all loaded objects of a specific type, including inactive objects
	/// </summary>
	/// <typeparam name="T">Type of object to search for</typeparam>
	/// <returns></returns>
	public static List<T> FindObjectsOfTypeAll<T> () {
		List<T> results = new List<T> ();
		for (int i = 0; i < SceneManager.sceneCount; i++) {
			var s = SceneManager.GetSceneAt (i);
			if (s.isLoaded) {
				var allGameObjects = s.GetRootGameObjects ();
				for (int j = 0; j < allGameObjects.Length; j++) {
					var go = allGameObjects[j];
					results.AddRange (go.GetComponentsInChildren<T> (true));
				}
			}
		}
		return results;
	}
}