using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(LobbyPlate)),CanEditMultipleObjects]
public class LobbyPlateInspector:ImageEditor {
 public override void OnInspectorGUI(){
  base.OnInspectorGUI();serializedObject.Update();
  EditorGUILayout.PropertyField(serializedObject.FindProperty("hitAreaOnly"));
  EditorGUILayout.PropertyField(serializedObject.FindProperty("primary"));
  if(!((LobbyPlate)target).sprite){EditorGUILayout.PropertyField(serializedObject.FindProperty("accent"));EditorGUILayout.PropertyField(serializedObject.FindProperty("top"));EditorGUILayout.PropertyField(serializedObject.FindProperty("bottom"));}
  serializedObject.ApplyModifiedProperties();
 }
}
