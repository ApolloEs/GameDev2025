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
    [CustomEditor( typeof( Respawner ) )]
    [CanEditMultipleObjects]
    public class RespawnerEditor : Editor
    {
        private SerializedProperty
            spawnModeProp,
            minRespawnTimeProp, maxRespawnTimeProp,
            spawnSFXProp,
            smoothScaleProp, scaleSpeedProp,
            RespawnStartedProp, RespawnEndedProp, 
            progress, seconds;

        // OnEnable
        void OnEnable()
        {
            spawnModeProp = serializedObject.FindProperty( "spawnMode" );
            minRespawnTimeProp = serializedObject.FindProperty( "minRespawnTime" );
            maxRespawnTimeProp = serializedObject.FindProperty( "maxRespawnTime" );
            spawnSFXProp = serializedObject.FindProperty( "spawnSFX" );
            smoothScaleProp = serializedObject.FindProperty( "smoothScale" );
            scaleSpeedProp = serializedObject.FindProperty( "scaleSpeed" );
            RespawnStartedProp = serializedObject.FindProperty( "RespawnStarted" );
            RespawnEndedProp = serializedObject.FindProperty( "RespawnEnded" );

            progress = serializedObject.FindProperty( "progress" );
            seconds = serializedObject.FindProperty( "seconds" );
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
            EditorGUILayout.PropertyField( spawnModeProp );
            ASKEditorHelper.ShowMinMaxSlider( minRespawnTimeProp, maxRespawnTimeProp, 1f, 120f, "Respawn Range" );

            GUILayout.Space( 5f );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, spawnSFXProp );

            ASKEditorHelper.DrawBoolField( smoothScaleProp );
            ASKEditorHelper.DrawPropertyField( scaleSpeedProp, 22f );
            GUI.enabled = true;

            GUILayout.Space( 10f );
            ASKEditorHelper.ShowProgressBar( progress.floatValue, string.Format( "Time : {0}", System.TimeSpan.FromSeconds( seconds.intValue ) ) ); 
            GUILayout.Space( 5f );

            EditorGUILayout.PropertyField( RespawnStartedProp, false, null );
            EditorGUILayout.PropertyField( RespawnEndedProp, false, null );            
        }
    };
}