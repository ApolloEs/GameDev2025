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
    [CustomEditor( typeof( Pickup ) ), CanEditMultipleObjects]
    public class PickupEditor : Editor
    {
        private SerializedProperty
            dropChanceProp,
            pickupTypeProp, pickupDistanceProp, pickupSoundProp,
            amountProp,ammoTypeProp, weaponTypeProp,
            PickupedProp;

        static string[] weaponsNames, ammoNames;

        // OnEnable
        void OnEnable()
        {
            dropChanceProp = serializedObject.FindProperty( "dropChance" );
            pickupTypeProp = serializedObject.FindProperty( "pickupType" );
            pickupDistanceProp = serializedObject.FindProperty( "pickupDistance" );
            pickupSoundProp = serializedObject.FindProperty( "pickupSound" );
            amountProp = serializedObject.FindProperty( "amount" );
            ammoTypeProp = serializedObject.FindProperty( "ammoType" );
            weaponTypeProp = serializedObject.FindProperty( "weaponType" );
            PickupedProp = serializedObject.FindProperty( "Pickuped" );

            weaponsNames = FindObjectOfType<WeaponsManager>().GetNames();
            ammoNames = FindObjectOfType<AmmoBackpack>().GetNames();
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
            EditorGUILayout.PropertyField( dropChanceProp );

            GUILayout.Space( 5f );
            ASKEditorHelper.DrawEnumAsToolbar( pickupTypeProp );
            EditorGUILayout.PropertyField( pickupDistanceProp );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, pickupSoundProp, "Pickup SFX" );

            GUILayout.Space( 5f );

            var type = ( Pickup.EType )pickupTypeProp.enumValueIndex;

            //Health = 0, Melee = 1, Firearms = 2, Ammo = 3, Thrown = 4
            switch( type )
            {
                case Pickup.EType.Health:
                    EditorGUILayout.IntSlider( amountProp, 1, 100, new GUIContent( "Health Amount" ) );
                    break;

                case Pickup.EType.Melee:
                    ASKEditorHelper.DrawStringPopup( weaponTypeProp, weaponsNames, weaponTypeProp.displayName );
                    break;

                case Pickup.EType.Firearms:
                    EditorGUILayout.IntSlider( amountProp, 1, 100, new GUIContent( "Ammo Amount" ) );
                    ASKEditorHelper.DrawStringPopup( ammoTypeProp, ammoNames, ammoTypeProp.displayName );
                    ASKEditorHelper.DrawStringPopup( weaponTypeProp, weaponsNames, weaponTypeProp.displayName );
                    break;

                case Pickup.EType.Ammo:
                    EditorGUILayout.IntSlider( amountProp, 1, 100, new GUIContent( "Ammo Amount" ) );
                    ASKEditorHelper.DrawStringPopup( ammoTypeProp, ammoNames, ammoTypeProp.displayName );
                    break;

                case Pickup.EType.Thrown:
                    EditorGUILayout.IntSlider( amountProp, 1, 10, new GUIContent( "Ammo Amount" ) );
                    ASKEditorHelper.DrawStringPopup( ammoTypeProp, ammoNames, ammoTypeProp.displayName );
                    ASKEditorHelper.DrawStringPopup( weaponTypeProp, weaponsNames, weaponTypeProp.displayName );
                    break;
            }

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( PickupedProp, false, null );
        }
    };
}