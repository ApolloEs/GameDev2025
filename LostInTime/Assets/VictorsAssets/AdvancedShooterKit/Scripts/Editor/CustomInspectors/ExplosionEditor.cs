/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{    
    [CustomEditor( typeof( Explosion ) )]
    [CanEditMultipleObjects]
    public class ExplosionEditor : Editor
    {
        List<SerializedProperty> properties = new List<SerializedProperty>();
        ReorderableList explosionSoundsList;

        bool isProjectileObject;


        // OnEnable
        void OnEnable()
        {
            properties.Add( serializedObject.FindProperty( "hitMask" ) );
            properties.Add( serializedObject.FindProperty( "damage" ) );
            properties.Add( serializedObject.FindProperty( "radius" ) );
            properties.Add( serializedObject.FindProperty( "force" ) );
            properties.Add( serializedObject.FindProperty( "upwardsModifier" ) );
            properties.Add( serializedObject.FindProperty( "fragments" ) );
            //
            explosionSoundsList = new ReorderableList( serializedObject, serializedObject.FindProperty( "explosionSounds" ), true, true, true, true );

            isProjectileObject = ( ( target as Component ).GetComponent<Projectile>() != null );
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
            if( isProjectileObject == false )
                GUILayout.Space( 5f );

            for( int i = 0; i < properties.Count; i++ )
            {
                if( isProjectileObject && i == 0 )
                    continue;

                if( i == 1 || i == 5 )
                    GUILayout.Space( 5f );              

                EditorGUILayout.PropertyField( properties[ i ] );
            }

            GUILayout.Space( 5f );
            ASKEditorHelper.ShowSFXListAndPlayButton( serializedObject, explosionSoundsList, "Explosion Sounds" );
        }
    };
}