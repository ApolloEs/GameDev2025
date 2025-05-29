/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEditor;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{
    [CanEditMultipleObjects]
    [CustomEditor( typeof( DamagePoint ) )]    
    public class DamagePointEditor : Editor
    {
        private SerializedProperty
            damageModifierProp,
            armorTypeProp, surfaceTypeProp;


        // OnEnable
        void OnEnable()
        {
            armorTypeProp = serializedObject.FindProperty( "armorType" );
            surfaceTypeProp = serializedObject.FindProperty( "surfaceType" );
            damageModifierProp = serializedObject.FindProperty( "damageModifier" );
        }

        // OnInspectorGUI
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShowParameters();
            serializedObject.ApplyModifiedProperties();
        }


        // Show Parameters
        private void ShowParameters()
        {
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( damageModifierProp );

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( armorTypeProp );

            ASKEditorHelper.DrawStringPopup( surfaceTypeProp, SurfaceDetector.allNames, surfaceTypeProp.displayName );
        }
    };
}