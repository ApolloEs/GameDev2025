/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;
using UnityEditor;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{
    [CanEditMultipleObjects]
    [CustomEditor( typeof( Projectile ) )]    
    public class ProjectileEditor : Editor
    {
        private SerializedProperty
            typeProp, surfacesArray, genericProp, maxImpactsProp,
            damageProp, speedProp, lifetimeProp, gravityPowerProp,
            decalObjectProp, shellObjectProp, explosionObjectProp,
            soundProp, impactAfterHitProp, noiseProp, resetAnglesProp;


        bool surfacesSFo = true, surfacesSubFo;
        int selection;
        static readonly string[] stateNames = { "Generic", "Special" };
        static readonly string[] surParams = { "maxPenetration", "outDispersion", "ricochetChance", "ricochetAngle", "ricochetDispersion" };


        // OnEnable
        void OnEnable()
        {
            typeProp = serializedObject.FindProperty( "type" );

            damageProp = serializedObject.FindProperty( "damage" );
            speedProp = serializedObject.FindProperty( "speed" );
            lifetimeProp = serializedObject.FindProperty( "lifetime" );
            gravityPowerProp = serializedObject.FindProperty( "gravityPower" );

            explosionObjectProp = serializedObject.FindProperty( "explosionObject" );
            decalObjectProp = serializedObject.FindProperty( "decalObject" );
            shellObjectProp = serializedObject.FindProperty( "shellObject" );

            soundProp = serializedObject.FindProperty( "sound" );
            impactAfterHitProp = serializedObject.FindProperty( "impactAfterHit" );
            noiseProp = serializedObject.FindProperty( "noise" );
            resetAnglesProp = serializedObject.FindProperty( "resetAngles" );

            genericProp = serializedObject.FindProperty( "generic" );
            surfacesArray = serializedObject.FindProperty( "surfaces" );

            maxImpactsProp = serializedObject.FindProperty( "maxImpacts" );
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
            bool isExplosionObject = ( explosionObjectProp.objectReferenceValue != null );

            var projType = ( Projectile.EType )typeProp.enumValueIndex;

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( typeProp );            

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( lifetimeProp );
            EditorGUILayout.PropertyField( speedProp );
            EditorGUILayout.PropertyField( gravityPowerProp );
            
            if( isExplosionObject == false )
            {
                GUILayout.Space( 5f );
                EditorGUILayout.PropertyField( damageProp );
            }
            
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( explosionObjectProp );
            GUILayout.Space( 5f );

            if( ( projType == Projectile.EType.Bullet || projType == Projectile.EType.Arrow ) && isExplosionObject == false )
            {
                EditorGUILayout.PropertyField( decalObjectProp );

                if( projType == Projectile.EType.Bullet )
                {
                    EditorGUILayout.PropertyField( shellObjectProp );
                    ShowSurfaces();
                }                
            }

            switch( projType )
            {
                case Projectile.EType.Throw:
                    ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, soundProp, "Pin SFX" );
                    EditorGUILayout.PropertyField( impactAfterHitProp, new GUIContent( "Explode After Hit" ) );                    
                    break;

                case Projectile.EType.Rocket:
                    ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, soundProp, "Fly SFX" );
                    EditorGUILayout.PropertyField( noiseProp );
                    EditorGUILayout.PropertyField( resetAnglesProp );
                    break;

                default: break;
            }
        }


        // ShowSurfaces
        private void ShowSurfaces()
        {
            GUILayout.Space( 5f );
            EditorGUILayout.BeginVertical( "box" );

            GUILayout.BeginHorizontal();
            GUILayout.Space( 15f );
            surfacesSFo = EditorGUILayout.Foldout( surfacesSFo, "    Surfaces", ASKEditorStyle.Get.largeFoldout );
            GUILayout.EndHorizontal();

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( maxImpactsProp );

            if( surfacesSFo == false )
            {
                GUILayout.Space( 3f );
                EditorGUILayout.EndVertical();
                return;
            }

            GUILayout.Space( 5f );
            surfacesSubFo = Convert.ToBoolean( GUILayout.Toolbar( Convert.ToInt32( surfacesSubFo ), stateNames, GUILayout.Height( 20f ) ) );
            GUILayout.Space( 5f );

            if( surfacesSubFo )
            {
                // Surface index & Minimum decrement to show pain screen
                string[] surfacesNames = SurfaceDetector.allNames;
                var arrCmd = ASKEditorHelper.DrawArrayControls( surfacesArray, surfacesNames.Length, ref selection );
                int surfacesSize = surfacesArray.arraySize;

                if( surfacesSize > 0 )
                {
                    SerializedProperty surfacesElement = surfacesArray.GetArrayElementAtIndex( selection );

                    GUILayout.Space( 5f );
                    ASKEditorHelper.DrawStringPopup( surfacesElement.FindPropertyRelative( "name" ), surfacesNames, "Surface Type" );
                    GUILayout.Space( 10f );

                    DrawSurfaceProp( surfacesElement );
                }

                // Actions
                if( arrCmd.type == ASKEditorHelper.ArrayCmd.EType.Add )
                {
                    foreach( string n in surParams )
                    {
                        SerializedProperty targetProp = arrCmd.newElement.FindPropertyRelative( n );
                        SerializedProperty sourceProp = genericProp.FindPropertyRelative( n );

                        if( targetProp.propertyType == SerializedPropertyType.Float ) {
                            targetProp.floatValue = sourceProp.floatValue;
                        }
                    }

                    arrCmd.newElement.FindPropertyRelative( "name" ).stringValue = surfacesNames[ selection ];
                }
            }
            else
            {
                DrawSurfaceProp( genericProp );
            }

            GUILayout.Space( 2f );
            EditorGUILayout.EndVertical();
        }


        // Draw SurfaceProp
        static void DrawSurfaceProp( SerializedProperty property )
        {            
            foreach( string n in surParams )
            {
                SerializedProperty subProp = property.FindPropertyRelative( n );
                EditorGUILayout.PropertyField( subProp );

                if( n == "ricochetChance" ) {
                    GUI.enabled = ( subProp.floatValue > 0f );
                }
            }

            GUI.enabled = true;
        }
    };
}