using UnityEngine;

public static class GizmosUtils {
    /// <summary>
    /// Draw text in the Scene View using Gizmos
    /// </summary>
    /// <param name="guiSkin">GUI skin</param>
    /// <param name="text">Text to display</param>
    /// <param name="position">World-space position of text</param>
    /// <param name="color">Text color</param>
    /// <param name="fontSize">Size of the characters</param>
    /// <param name="yOffset">Vertical offset of text in screen-space coordinates</param>
    public static void DrawText (GUISkin guiSkin, string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0) {
#if UNITY_EDITOR
        GUISkin prevSkin = GUI.skin;
        if (guiSkin == null)
            Debug.LogWarning ("editor warning: guiSkin parameter is null");
        else
            GUI.skin = guiSkin;

        GUIContent textContent = new GUIContent (text);

        GUIStyle style = (guiSkin != null) ? new GUIStyle (guiSkin.GetStyle ("Label")) : new GUIStyle ();
        if (color != null)
            style.normal.textColor = (Color) color;
        if (fontSize > 0)
            style.fontSize = fontSize;

        Vector2 textSize = style.CalcSize (textContent);
        Vector3 screenPoint = Camera.current.WorldToScreenPoint (position);

        if (screenPoint.z > 0) // checks necessary to the text is not visible when the camera is pointed in the opposite direction relative to the object
        {
            var worldPosition = Camera.current.ScreenToWorldPoint (new Vector3 (screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
            UnityEditor.Handles.Label (worldPosition, textContent, style);
        }
        GUI.skin = prevSkin;
#endif
    }

    public static void DrawArrow (Vector3 start, Vector3 end, Color? color = null, float size = 1) {
#if UNITY_EDITOR
        Color cachedColor = Gizmos.color;
        if(color != null) {
            Gizmos.color = (Color) color;
        }
        else {
            Gizmos.color = Color.white;
        }
        Vector3 dir = (end - start).normalized;
        Vector3 reverseDir = -dir;
        Gizmos.DrawLine (start, end);
        Vector3 p = Vector3.Cross (dir, Camera.current.transform.forward);
        float arrowHeadSize =Mathf.Clamp(size, 0, 0.5f * (end - start).magnitude);
        Gizmos.DrawLine (end, end + arrowHeadSize * (reverseDir + 0.5f * p));
        Gizmos.DrawLine (end, end + arrowHeadSize * (reverseDir - 0.5f * p));
        Gizmos.color = cachedColor;
#endif
    }
}