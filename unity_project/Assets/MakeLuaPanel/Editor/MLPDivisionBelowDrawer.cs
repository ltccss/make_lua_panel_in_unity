using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MLPDivisionBelow))]
public class MLPDivisionBelowDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var propertyPos = position;
        propertyPos.size = new Vector2(propertyPos.size.x, base.GetPropertyHeight(property, label));
        EditorGUI.PropertyField(propertyPos, property);

        var divisionPos = propertyPos;
        divisionPos.y += base.GetPropertyHeight(property, label) + 3;
        EditorGUI.DrawRect(new Rect(0, divisionPos.y, EditorGUIUtility.currentViewWidth, 2), Color.yellow);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 3 + 2;
    }
}
