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
    [CustomEditor( typeof( HitEffect ) )]
    [CanEditMultipleObjects]
    public class HitEffectEditor : Editor
    {
        private SerializedProperty
            lifetimeProp, 
            framesXProp, framesYProp,
            surfacesArray;

        static bool surfacesSFToolbar;
        static bool mainFo, surfacesFo;
        int selection;
        static readonly string[] stateNames = { "Generic", "Special" };


        // OnEnable
        void OnEnable()
        {
            lifetimeProp = serializedObject.FindProperty( "lifetime" );
            framesXProp = serializedObject.FindProperty( "framesX" );
            framesYProp = serializedObject.FindProperty( "framesY" );
            //
            surfacesArray = serializedObject.FindProperty( "surfaces" );
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
            ShowFoldout( ref mainFo, "    Main", ShowMainParams );
            ShowFoldout( ref surfacesFo, "    Surfaces", ShowSurfaces );
        }

        // Call NamedMethod ( Refletions no-no )
        private void ShowFoldout( ref bool spFoldout, string dataName, System.Action methodRef )
        {
            GUILayout.BeginVertical( "Box", GUILayout.ExpandWidth( true ) );

            GUILayout.BeginHorizontal();
            GUILayout.Space( 15f );
            spFoldout = EditorGUILayout.Foldout( spFoldout, dataName, ASKEditorStyle.Get.largeFoldout );
            GUILayout.EndHorizontal();


            if( spFoldout )
            {
                GUILayout.Space( 5f );
                methodRef.Invoke();
                GUILayout.Space( 5f );
            }
            else
            {
                GUILayout.Space( 2f );
            }

            GUILayout.EndVertical();
        }


        // Show MainParams
        private void ShowMainParams()
        {
            EditorGUILayout.Slider( lifetimeProp, 1f, 120f, new GUIContent( "Lifetime" ) );
            //
            EditorGUILayout.IntSlider( framesXProp, 1, 10, new GUIContent( "Frames X" ) );
            EditorGUILayout.IntSlider( framesYProp, 1, 10, new GUIContent( "Frames Y" ) );
        }

        // Show Surfaces
        private void ShowSurfaces()
        {
            // Surface index & Minimum decrement to show pain screen
            string[] surfacesNames = SurfaceDetector.allNames;
            int surfacesLength = surfacesNames.Length;

            surfacesSFToolbar = System.Convert.ToBoolean( GUILayout.Toolbar( System.Convert.ToInt32( surfacesSFToolbar ), stateNames, GUILayout.Height( 20f ) ) );

            GUILayout.Space( 5f );

            if( surfacesSFToolbar )
            {
                var arrCmd = ASKEditorHelper.DrawArrayControls( surfacesArray, surfacesLength, ref selection );

                int surfacesSize = surfacesArray.arraySize;
                if( surfacesSize > 0 )
                {
                    SerializedProperty surfacesElement = surfacesArray.GetArrayElementAtIndex( selection );
                    
                    SerializedProperty hitTextureProp = surfacesElement.FindPropertyRelative( "hitTexture" );
                    SerializedProperty hitSoundProp = surfacesElement.FindPropertyRelative( "hitSound" );
                    SerializedProperty hitParticleProp = surfacesElement.FindPropertyRelative( "hitParticle" );

                    GUILayout.Space( 5f );
                    ASKEditorHelper.DrawStringPopup( surfacesElement.FindPropertyRelative( "name" ), surfacesNames, "Surface Type" );
                    GUILayout.Space( 10f );

                    EditorGUILayout.PropertyField( hitTextureProp, new GUIContent( hitTextureProp.displayName ) );
                    ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, hitSoundProp, "Hit SFX" );
                    EditorGUILayout.PropertyField( hitParticleProp, new GUIContent( hitParticleProp.displayName ) );
                }

                // Actions
                if( arrCmd.type == ASKEditorHelper.ArrayCmd.EType.Add )
                {
                    arrCmd.newElement.FindPropertyRelative( "name" ).stringValue = surfacesNames[ selection ];

                    arrCmd.newElement.FindPropertyRelative( "hitTexture" ).objectReferenceValue = null;
                    arrCmd.newElement.FindPropertyRelative( "hitSound" ).objectReferenceValue = null;
                    arrCmd.newElement.FindPropertyRelative( "hitParticle" ).objectReferenceValue = null;
                }
            }
            else
            {
                SerializedProperty surfacesElement = serializedObject.FindProperty( "generic" );
                SerializedProperty hitTextureProp = surfacesElement.FindPropertyRelative( "hitTexture" );
                SerializedProperty hitSoundProp = surfacesElement.FindPropertyRelative( "hitSound" );
                SerializedProperty hitParticleProp = surfacesElement.FindPropertyRelative( "hitParticle" );
                          
                EditorGUILayout.PropertyField( hitTextureProp, new GUIContent( "Hit Texture" ) );
                ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, hitSoundProp, "Hit SFX" );
                EditorGUILayout.PropertyField( hitParticleProp, new GUIContent( "Hit Particle" ) );
            }
        }
    };
}