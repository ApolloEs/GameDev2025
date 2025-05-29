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
    [CustomEditor( typeof( WeaponsManager ) )]
    public class WeaponsManagerEditor : Editor
    {
        private SerializedProperty 
            maxWeaponsProp, ironsightingModeProp,
            switchWeaponProp, switchShootingModeProp, subWeaponOnOffProp, switchProjectileProp,
            listElement, availableProp, typeProp, weaponProp, subWeaponProp, dropoutProp;

        private ReorderableList weaponsList = null;            


        // OnEnable
        void OnEnable()
        {
            maxWeaponsProp = serializedObject.FindProperty( "maxWeapons" );
            switchWeaponProp = serializedObject.FindProperty( "switchWeapon" );
            switchShootingModeProp = serializedObject.FindProperty( "switchShootingMode" );
            subWeaponOnOffProp = serializedObject.FindProperty( "subWeaponOnOff" );
            switchProjectileProp = serializedObject.FindProperty( "switchProjectile" );
            ironsightingModeProp = serializedObject.FindProperty( "ironsightingMode" );

            weaponsList = new ReorderableList( serializedObject, serializedObject.FindProperty( "weapons" ), true, true, true, true );
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
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( maxWeaponsProp );

            GUILayout.Space( 5f );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, switchWeaponProp );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, switchShootingModeProp, "Switch Fire Mode" );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, subWeaponOnOffProp, "SubWeapon On/Off" );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, switchProjectileProp );
            GUILayout.Space( 5f );

            ASKEditorHelper.DrawEnumAsToolbar( ironsightingModeProp );

            GUILayout.Space( 5f );
            weaponsList.drawHeaderCallback = ( Rect rect ) =>
            {
                EditorGUI.LabelField( rect, "Player Weapons" );
            };

            if( weaponsList.count > 0 )
                weaponsList.elementHeight = EditorGUIUtility.singleLineHeight * 4.45f;
            else
                weaponsList.elementHeight = EditorGUIUtility.singleLineHeight;

            weaponsList.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
            {
                listElement = weaponsList.serializedProperty.GetArrayElementAtIndex( index );
                availableProp = listElement.FindPropertyRelative( "available" );
                typeProp = listElement.FindPropertyRelative( "type" );
                weaponProp = listElement.FindPropertyRelative( "weapon" );
                subWeaponProp = listElement.FindPropertyRelative( "subWeapon" );
                dropoutProp = listElement.FindPropertyRelative( "dropout" );                

                const float space = 5f;
                float width = EditorGUIUtility.currentViewWidth;
                float height = EditorGUIUtility.singleLineHeight;
                float startX = rect.x;

                rect.y += 2f;

                rect.x = startX / 2f;
                rect.width = width - 40f;
                rect.height = height * 4.15f;
                EditorGUI.HelpBox( rect, string.Empty, MessageType.None );

                // available + type

                rect.x = startX;
                rect.y += 4f;
                rect.height = height;

                
                rect.width = width / 35f;
                EditorGUI.PropertyField( rect, availableProp, GUIContent.none );
                //
                rect.x += rect.width + space;
                rect.width = width / 7f;
                EditorGUI.LabelField( rect, "Available |" );


                rect.x += rect.width;// + space * 4f;
                rect.width = width / 15f;
                EditorGUI.LabelField( rect, "Type" );
                //
                rect.x += rect.width + space;
                rect.width = width / 4f;
                EditorGUI.PropertyField( rect, typeProp, GUIContent.none );

                rect.x += rect.width + space * 2f;
                rect.width = width / 3f;
                var weap = weaponProp.objectReferenceValue;
                EditorGUI.LabelField( rect, string.Empty, EditorStyles.helpBox );
                rect.x += space;
                EditorGUI.LabelField( rect, ( weap != null ) ? weap.name : "NULL", EditorStyles.boldLabel );

                // weapon + subWeapon

                rect.x = startX;
                rect.y += height + space;
                rect.width = width / 10f;
                EditorGUI.LabelField( rect, "Main" );

                rect.x += rect.width + space;
                rect.width = width / 3.3f;
                EditorGUI.PropertyField( rect, weaponProp, GUIContent.none );

                GUI.enabled = typeProp.enumValueIndex != 2;                
                
                rect.x += rect.width + space * 2f;
                rect.width = width / 15f;
                EditorGUI.LabelField( rect, "Sub" );

                rect.x += rect.width + space;
                rect.width = width / 2.85f;
                EditorGUI.PropertyField( rect, subWeaponProp, GUIContent.none );

                GUI.enabled = true;

                // dropout

                GUI.enabled = typeProp.enumValueIndex == 0;

                rect.x = startX;
                rect.y += height + space;
                rect.width = width / 10f;
                EditorGUI.LabelField( rect, "Drop" );

                rect.x += rect.width + space;
                rect.width = width / 1.33f;
                EditorGUI.PropertyField( rect, dropoutProp, GUIContent.none );

                GUI.enabled = true;
            };

            weaponsList.DoLayoutList();
            GUILayout.Space( 5f );
        }
    };
}