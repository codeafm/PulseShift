using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using TMPro;

public static class MenuRewardsBuild {
 const string Art="Assets/Resources/Lobby/SkinMenuButton.png";
 public static void ApplyOnce(){
  var importer=(TextureImporter)AssetImporter.GetAtPath(Art);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
  var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art);if(!sprite)throw new System.Exception("Skin menu sprite did not import.");
  var scene=EditorSceneManager.OpenScene(SeparateMenuBuild.MenuPath);var lobby=Object.FindFirstObjectByType<PulseLobby>();if(!lobby||!lobby.safeRoot)throw new System.Exception("Menu scene is incomplete.");
  if(!lobby.popupRoot){var pages=new GameObject("Pages • native Canvas modals",typeof(RectTransform)).GetComponent<RectTransform>();pages.SetParent(lobby.safeRoot,false);pages.anchorMin=pages.anchorMax=pages.pivot=new Vector2(.5f,.5f);pages.sizeDelta=new Vector2(540,1170);lobby.popupRoot=pages;}
  foreach(var button in lobby.safeRoot.GetComponentsInChildren<LobbyButton>(true))if(button.action==LobbyAction.Collection||button.action==LobbyAction.Stats)button.gameObject.SetActive(false);
  foreach(var label in lobby.safeRoot.GetComponentsInChildren<TMP_Text>(true)){label.raycastTarget=false;EditorUtility.SetDirty(label);}
  var profile=lobby.safeRoot.Find("Profile");if(profile){var oldAction=profile.GetComponent<LobbyButton>();if(oldAction)Object.DestroyImmediate(oldAction);var oldPlate=profile.GetComponent<LobbyPlate>();if(oldPlate)oldPlate.raycastTarget=false;var hit=lobby.safeRoot.Find("Profile • hit area") as RectTransform;if(!hit){hit=new GameObject("Profile • hit area",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LobbyButton)).GetComponent<RectTransform>();hit.SetParent(lobby.safeRoot,false);}hit.anchorMin=hit.anchorMax=hit.pivot=new Vector2(0,1);hit.anchoredPosition=new Vector2(92,-45);hit.sizeDelta=new Vector2(184,82);var image=hit.GetComponent<Image>();image.color=Color.clear;image.raycastTarget=true;var button=hit.GetComponent<Button>();button.targetGraphic=image;button.transition=Selectable.Transition.None;var action=hit.GetComponent<LobbyButton>();action.lobby=lobby;action.action=LobbyAction.Profile;hit.SetSiblingIndex(lobby.popupRoot.GetSiblingIndex());EditorUtility.SetDirty(hit);}
  var skin=lobby.safeRoot.GetComponentsInChildren<LobbyButton>(true).FirstOrDefault(b=>b.action==LobbyAction.Skins);if(!skin)throw new System.Exception("Skin button not found.");
  var rect=(RectTransform)skin.transform;rect.anchoredPosition=new Vector2(0,rect.anchoredPosition.y);rect.sizeDelta=new Vector2(86,86);var plate=skin.GetComponent<LobbyPlate>();plate.sprite=sprite;plate.preserveAspect=true;plate.type=Image.Type.Simple;plate.color=Color.white;
  foreach(var glyph in skin.GetComponentsInChildren<LobbyGlyph>(true))glyph.gameObject.SetActive(false);foreach(var label in skin.GetComponentsInChildren<Text>(true)){label.text="СКИНЫ";label.fontSize=11;label.rectTransform.anchoredPosition=new Vector2(0,-29);label.rectTransform.sizeDelta=new Vector2(74,20);label.color=Color.white;}
  EditorUtility.SetDirty(lobby);EditorUtility.SetDirty(rect);EditorUtility.SetDirty(plate);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("MENU_REWARDS_SETTINGS_AND_SKIN_APPLIED");SeparateMenuBuild.BuildExisting();
 }
}
