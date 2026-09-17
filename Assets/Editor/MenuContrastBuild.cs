using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// One-time visual tuning for the saved editable menu material.
public static class MenuContrastBuild {
 public static void Apply(){
  var shader=Shader.Find("Pulse/MenuBackdrop");if(!shader)throw new System.Exception("Menu backdrop shader was not imported.");
  var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/UI/MenuCosmosBackdrop.mat");if(!material)throw new System.Exception("Menu background material is missing.");
  material.shader=shader;material.SetFloat("_Exposure",.62f);material.SetFloat("_Saturation",.9f);material.SetFloat("_Vignette",.34f);material.SetFloat("_TopShade",.22f);material.SetFloat("_BottomShade",.30f);EditorUtility.SetDirty(material);
  var scene=EditorSceneManager.OpenScene(SeparateMenuBuild.MenuPath);var lobby=Object.FindFirstObjectByType<PulseLobby>();lobby.stage.backdrop.GetComponent<Renderer>().sharedMaterial=material;EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  Debug.Log("MENU_CONTRAST_APPLIED");SeparateMenuBuild.BuildExisting();
 }
}
