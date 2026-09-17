using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PulseLobby))]
public class PulseMenuInspector:Editor {
 public override void OnInspectorGUI(){
  var menu=(PulseLobby)target;
  if(menu.standaloneScene){
   EditorGUILayout.HelpBox("Предпросмотр использует ту же адаптацию, что и игра. Героя перемещай и масштабируй внутри Hero framing • automatic. Не меняй сам автоматический контейнер. Фон: LobbyStage → Background Offset / Zoom. Кнопки: RectTransform. Имя и баланс в игре берутся из сохранений.",MessageType.Info);
   if(GUILayout.Button("Выбрать Canvas — кнопки и надписи")){Selection.activeGameObject=menu.safeRoot.gameObject;SceneView.lastActiveSceneView?.FrameSelected();}
   if(GUILayout.Button("Выбрать героя — ручное положение и размер")){Selection.activeGameObject=menu.stage.spirit.gameObject;SceneView.lastActiveSceneView?.FrameSelected();}
   if(GUILayout.Button("Обновить предпросмотр")){var preview=menu.GetComponent<MenuScenePreview>();if(preview)preview.Refresh();SceneView.RepaintAll();}
   if(GUILayout.Button("Выбрать камеру меню"))Selection.activeGameObject=menu.stage.view.gameObject;
   EditorGUILayout.Space();
  }
  DrawDefaultInspector();
 }
}
