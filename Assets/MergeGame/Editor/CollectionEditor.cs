using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(MergeCollection))]
public class CollectionEditor : Editor
{
    MergeCollection collection;
    ReorderableList reorderableList;
 
    void OnEnable() {
        
        reorderableList = new ReorderableList(serializedObject, 
            serializedObject.FindProperty("objects"), 
            true, true, true, true);
        
        reorderableList.drawElementCallback =  
            (rect, index, isActive, isFocused) => {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Icon"), GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(rect.x + 110, rect.y, rect.width - 110 - 80, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Title"), GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width - 80, rect.y, 80, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("RawPrice"), GUIContent.none);
            };
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}