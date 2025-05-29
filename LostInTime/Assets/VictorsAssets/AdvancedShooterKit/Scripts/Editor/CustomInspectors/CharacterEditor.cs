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
    [CustomEditor( typeof( Character ) )]
    [CanEditMultipleObjects]
    public class CharacterEditor : HealthEditor
    {
        protected SerializedProperty
            damageToPainProp, percentToPainProp, surfaceNameProp,
            deathSoundProp, deathLayerProp;

        
        private ReorderableList painSoundsList = null;        
        private bool damageHandlersFound = false;


        // OnEnable
        protected override void OnEnable()
        {
            base.OnEnable();            
            
            surfaceNameProp = serializedObject.FindProperty( "surfaceType" );
            damageToPainProp = serializedObject.FindProperty( "damageToPain" );
            percentToPainProp = serializedObject.FindProperty( "percentToPain" );
            deathSoundProp = serializedObject.FindProperty( "deathSound" );
            deathLayerProp = serializedObject.FindProperty( "deathLayer" );

            painSoundsList = new ReorderableList( serializedObject, serializedObject.FindProperty( "painSounds" ) );

            eventsList.Add( serializedObject.FindProperty( "OnPain" ) );

            damageHandlersFound = ( ( target as Component ).GetComponentInChildren<DamagePoint>() != null );
        }
        

        // Show MainParams
        protected override void ShowMainParams( bool showDeathDrops )
        {
            GUI.enabled = !damageHandlersFound;
            ASKEditorHelper.DrawStringPopup( surfaceNameProp, SurfaceDetector.allNames, "Hit Surface" );
            GUI.enabled = true;

            base.ShowMainParams( false );            
            
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( damageToPainProp );
            EditorGUILayout.PropertyField( percentToPainProp );
            ASKEditorHelper.ShowSFXListAndPlayButton( serializedObject, painSoundsList, "Pain Sounds" );

            GUILayout.Space( 5f );            
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, deathSoundProp );
            int layerValue = deathLayerProp.intValue;
            layerValue = EditorGUILayout.LayerField( "Death Layer", layerValue );
            deathLayerProp.intValue = layerValue;            

            if( showDeathDrops ) {
                ShowDeathDrops( "Drop objects delay", "Dropped objects after death", "Dropped pickups after death" );
            }            
        }
    };
}