using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PortalComponent : MonoBehaviour {
	/// <summary>
	/// The ID of this portal
	/// </summary>
	public int id;

	/// <summary>
	/// The scene in which the destination portal is located
	/// </summary>
	public SerializableSceneReference destinationScene;
	
	/// <summary>
	/// The ID of the destination portal
	/// </summary>
	public int destinationId;

	/// <summary>
	/// An event raised when the player arrives at this portal
	/// </summary>
	public event System.Action OnExitEvent;

	/// <summary>
	/// Raises the OnExitEvent
	/// (Should only be called by PortalTransitionManager) 
	/// </summary>
	public void Exit() {
		if(OnExitEvent != null) {
			OnExitEvent();
		}
	}

	/// <summary>
	/// Teleport to the destination portal
	/// </summary>
	public void Enter() {
		if(destinationScene.ScenePath == string.Empty) {
			Debug.LogWarning("Portal " + id + " is not connected to a destination portal.");
			return;
		}
		PortalTransitionManager.Instance.Teleport(destinationScene.ScenePath, destinationId);
	}


	private void Update () {
#if UNITY_EDITOR
		this.gameObject.name = id.ToString ();
#endif
	}

#if UNITY_EDITOR
	private void OnDrawGizmos () {
		GizmosUtils.DrawText (GUI.skin, " " + id, transform.position + Vector3.down * 1.5f, Color.yellow, 16, 0);
		if(destinationScene.ScenePath == this.gameObject.scene.path) {
			PortalComponent destinationPortalRef = PortalComponentCache.GetPortal(destinationId);
			if(destinationPortalRef != null) {
				GizmosUtils.DrawArrow(transform.position, destinationPortalRef.transform.position, Color.yellow, 2.0f);
			}
		}
		else if(destinationScene.ScenePath != string.Empty) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, 0.7f);
			Gizmos.DrawWireSphere(transform.position, 0.5f);
		}
	}
#endif




}