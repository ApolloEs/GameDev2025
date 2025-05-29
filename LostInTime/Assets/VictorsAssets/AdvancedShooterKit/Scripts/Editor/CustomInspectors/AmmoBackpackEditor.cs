/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{
    [CustomEditor( typeof( AmmoBackpack ) )]
    public class AmmoBackpackEditor : Editor
    {
        private SerializedProperty infiniteAmmoProp;
        private ReorderableList ammoList;


        // OnEnable
        void OnEnable()
        {
            infiniteAmmoProp = serializedObject.FindProperty( "infiniteAmmo" );
            ammoList = new ReorderableList( serializedObject, serializedObject.FindProperty( "ammunition" ), true, true, true, true );
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
            EditorGUILayout.Space();
            ASKEditorHelper.DrawBoolAsButton( infiniteAmmoProp );
            EditorGUILayout.Space();

            ammoList.drawHeaderCallback = ( Rect rect ) =>
            {
                EditorGUI.LabelField( rect, "Player Ammunition" );
            };

            float height = EditorGUIUtility.singleLineHeight;
            ammoList.elementHeight = height;

            if( ammoList.count > 0 ) {
                ammoList.elementHeight *= 3f;
            }
                

            ammoList.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
            {
                SerializedProperty listElement = ammoList.serializedProperty.GetArrayElementAtIndex( index );
                SerializedProperty currentAmmoProp = listElement.FindPropertyRelative( "currentAmmo" );
                SerializedProperty maxAmmoProp = listElement.FindPropertyRelative( "maxAmmo" );
                SerializedProperty hudIconProp = listElement.FindPropertyRelative( "hudIcon" );
                SerializedProperty nameProp = listElement.FindPropertyRelative( "name" );

                const float SPACE = 5f;
                float width = EditorGUIUtility.currentViewWidth;                
                float startX = rect.x;

                rect.y += 2f;

                rect.x = startX / 2f;
                rect.width = width - 40f;
                rect.height = height * 2.8f;
                EditorGUI.HelpBox( rect, string.Empty, MessageType.None );

                // current + max

                rect.x = startX;
                rect.y += 4f;
                rect.height = height;

                rect.width = width / 4.5f;
                EditorGUI.LabelField( rect, "Current Ammo" );

                rect.x += rect.width;
                rect.width = width / 6f;
                EditorGUI.PropertyField( rect, currentAmmoProp, GUIContent.none );

                rect.x += rect.width + SPACE * 2f;
                rect.width = width / 6f;
                EditorGUI.LabelField( rect, "Max Ammo" );

                rect.x += rect.width + SPACE;
                rect.width = width / 3.75f;
                EditorGUI.PropertyField( rect, maxAmmoProp, GUIContent.none );

                // hud icon

                rect.x = startX;
                rect.y += height + SPACE;
                rect.width = width / 15f;
                EditorGUI.LabelField( rect, "Icon" );

                rect.x += rect.width + SPACE;
                rect.width = width / 3.1f;
                EditorGUI.PropertyField( rect, hudIconProp, GUIContent.none );

                // name

                rect.x += rect.width + SPACE;
                rect.width = width / 10f;
                EditorGUI.LabelField( rect, "Name" );

                rect.x += rect.width + SPACE;
                rect.width = width / 3f;
                EditorGUI.PropertyField( rect, nameProp, GUIContent.none );
            };

            ammoList.DoLayoutList();
            GUILayout.Space( 5f );
        }
    };
}