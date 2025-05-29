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
    [CustomEditor( typeof( Melee ) )]
    [CanEditMultipleObjects]
    public class MeleeEditor : Editor
    {
        private SerializedProperty
            damageProp, shotDelayProp,swayRangeProp, swayRadiusProp, hitForceProp, rateOfFireProp,
            shotSFXProp, hitMaskProp, hitEffectProp, minCameraShakeProp, maxCameraShakeProp;        

        private bool isPlayerWeapon = false;


        // OnEnable
        void OnEnable()
        {
            damageProp = serializedObject.FindProperty( "damage" );
            shotDelayProp = serializedObject.FindProperty( "shotDelay" );
            swayRangeProp = serializedObject.FindProperty( "swayRange" );
            swayRadiusProp = serializedObject.FindProperty( "swayRadius" );
            hitForceProp = serializedObject.FindProperty( "hitForce" );

            rateOfFireProp = serializedObject.FindProperty( "rateOfFire" );
            shotSFXProp = serializedObject.FindProperty( "shotSFX" );

            hitMaskProp = serializedObject.FindProperty( "hitMask" );
            hitEffectProp = serializedObject.FindProperty( "hitEffect" );

            minCameraShakeProp = serializedObject.FindProperty( "minCameraShake" );
            maxCameraShakeProp = serializedObject.FindProperty( "maxCameraShake" );

            isPlayerWeapon = ( ( target as Component ).GetComponentInParent<PlayerCharacter>() != null );
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
            EditorGUILayout.PropertyField( hitMaskProp );

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( damageProp );
            EditorGUILayout.PropertyField( hitForceProp );
            EditorGUILayout.PropertyField( swayRangeProp );
            EditorGUILayout.PropertyField( swayRadiusProp );

            GUILayout.Space( 5f );
            EditorGUILayout.Slider( rateOfFireProp, 1f, 125f );
            EditorGUILayout.Slider( shotDelayProp, 0f, 2f, new GUIContent( "Hit Delay" ) );

            GUILayout.Space( 5f );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, shotSFXProp, "Whoosh SFX" );

            EditorGUILayout.PropertyField( hitEffectProp );            

            if( isPlayerWeapon )
            {
                // camera shake
                GUILayout.Space( 5f );
                ASKEditorHelper.ShowMinMaxSlider( minCameraShakeProp, maxCameraShakeProp, .01f, 2f, "Camera Shake Range" );
            }
        }
    }
}