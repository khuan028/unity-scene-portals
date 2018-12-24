using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A cache to accelerate lookup of Portal Gameobjects
/// </summary>
public class PortalComponentCache {

    private Dictionary<int, PortalComponent> portalDict = new Dictionary<int, PortalComponent> ();
    private string scenePath;

    private static PortalComponentCache _ins;

    private static PortalComponentCache Instance {
        get {
            if (_ins == null) {
                _ins = new PortalComponentCache ();
            }
            return _ins;
        }
    }

    public static string ScenePath {
        get {
            return Instance.scenePath;
        }
    }

    /// <summary>
    /// Finds the PortalComponent in the active scene
    /// </summary>
    /// <param name="id">ID of the portal</param>
    /// <returns></returns>
    public static PortalComponent GetPortal (int id) {
        PortalComponent portal;
        if (Instance.portalDict.TryGetValue (id, out portal) && portal != null && portal.id == id) {
            return portal;
        }
        Instance.RefreshCache ();
        if (Instance.portalDict.TryGetValue (id, out portal) && portal != null && portal.id == id) {
            return portal;
        }
        return null;
    }

    /// <summary>
    /// Finds all PortalComponents in the active scene and adds them to a dictionary
    /// </summary>
    public void RefreshCache () {
        Scene activeScene = SceneManager.GetActiveScene ();
        scenePath = activeScene.path;
        List<PortalComponent> portals = Finder.FindObjectsOfTypeAll<PortalComponent> ();
        portalDict.Clear ();
        foreach (PortalComponent portal in portals) {
            if(!portalDict.ContainsKey(portal.id)) {
                portalDict.Add (portal.id, portal);
            }
        }
    }
}