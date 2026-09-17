using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PulseSceneMenu {
 [MenuItem("PULSESHIFT/Open editable scene")]
 public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()){PlayerSettings.colorSpace=ColorSpace.Linear;EditorSceneManager.OpenScene("Assets/Scenes/PULSESHIFT_Cinematic.unity");var root=GameObject.Find("LEVEL • editable models");if(root){Selection.activeGameObject=root;if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.FrameSelected();}}}
 [MenuItem("PULSESHIFT/Open editable scene",true)]public static bool CanOpen()=>!EditorApplication.isPlayingOrWillChangePlaymode;
}
