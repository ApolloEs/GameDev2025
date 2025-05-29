/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{
    [CustomEditor( typeof( BodyAnimator ) )]
    public class BodyAnimatorEditor : Editor
    {
        List<SerializedProperty>
            boolFields = new List<SerializedProperty>()
            , floatFields = new List<SerializedProperty>();

        SerializedProperty boolFoldoutProp, floatFoldoutProp;

        
        // OnEnable
        void OnEnable()
        {
            boolFields.Add( serializedObject.FindProperty( "isMoving" ) );
            boolFields.Add( serializedObject.FindProperty( "isMovedForward" ) );
            boolFields.Add( serializedObject.FindProperty( "isCrouched" ) );
            boolFields.Add( serializedObject.FindProperty( "isClimbing" ) );
            boolFields.Add( serializedObject.FindProperty( "isFalling" ) );

            floatFields.Add( serializedObject.FindProperty( "normalizedSpeed" ) );
            floatFields.Add( serializedObject.FindProperty( "radians" ) );
            floatFields.Add( serializedObject.FindProperty( "floorDistance" ) );
            floatFields.Add( serializedObject.FindProperty( "turn" ) );
            

            boolFoldoutProp = serializedObject.FindProperty( "boolFoldout" );
            floatFoldoutProp = serializedObject.FindProperty( "floatFoldout" );
        }


        // OnInspectorGUI
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShowParameters();
            serializedObject.ApplyModifiedProperties();
        }

        // ShowParameters
        private void ShowParameters()
        {
            ASKEditorHelper.LargeFoldout( boolFoldoutProp, "Bool Values", () => DrawReadOnlyList( boolFields ) );
            ASKEditorHelper.LargeFoldout( floatFoldoutProp, "Float Values", () => DrawReadOnlyList( floatFields ) );
        }

        // Draw List
        private void DrawReadOnlyList( List<SerializedProperty> list )
        {
            GUI.enabled = false;
            list.ForEach( prop => EditorGUILayout.PropertyField( prop ) );
            GUI.enabled = true;
        }
    };
}
