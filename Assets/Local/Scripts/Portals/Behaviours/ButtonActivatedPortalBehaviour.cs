using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonActivatedPortalBehaviour : MonoBehaviour {

    public PortalComponent portalComponent;

    public string activationKey;

    void Awake() {
        portalComponent.OnExitEvent += this.HandleExitEvent;
    }

    void OnDestroy() {
        portalComponent.OnExitEvent -= this.HandleExitEvent;
    }

    void Update() {
        if(Input.GetKeyDown(activationKey)) {
            portalComponent.Enter();
        }
    }

    void HandleExitEvent() {
        Debug.Log("You have successfully exited at portal id = " + portalComponent.id + " in scene " + this.gameObject.scene.path);
    }

}