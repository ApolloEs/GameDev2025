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
    [CustomEditor( typeof( Health ) )]
    [CanEditMultipleObjects]
    public class HealthEditor : Editor
    {
        private SerializedProperty
            immortalProp, armorTypeProp,
            maxHealthProp, currentHealthProp,
            regenerationProp, amountProp, delayProp, intervalProp;

        protected SerializedProperty
            spawnObjectsDelayProp, destroyBodyDelayProp,
            onlyOneDropProp;

        protected SerializedProperty mainFoProp, eventsFoProp;

        protected ReorderableList deathDropsList, deathObjectList;

        
        protected List<SerializedProperty> eventsList = new List<SerializedProperty>();



        // OnEnable
        protected virtual void OnEnable()
        {
            immortalProp = serializedObject.FindProperty( "immortal" );
            armorTypeProp = serializedObject.FindProperty( "armorType" );
            maxHealthProp = serializedObject.FindProperty( "maxHealth" );
            currentHealthProp = serializedObject.FindProperty( "currentHealth" );

            regenerationProp = serializedObject.FindProperty( "regeneration" );
            amountProp = serializedObject.FindProperty( "regAmount" );
            delayProp = serializedObject.FindProperty( "regDelay" );
            intervalProp = serializedObject.FindProperty( "regInterval" );


            spawnObjectsDelayProp = serializedObject.FindProperty( "spawnObjectsDelay" );
            destroyBodyDelayProp = serializedObject.FindProperty( "destroyBodyDelay" );

            deathObjectList = new ReorderableList( serializedObject, serializedObject.FindProperty( "deathObjects" ) );

            onlyOneDropProp = serializedObject.FindProperty( "dropOnlyOnePickup" );
            deathDropsList = new ReorderableList( serializedObject, serializedObject.FindProperty( "deathDrops" ) );

            eventsList.Add( serializedObject.FindProperty( "OnDamage" ) );
            eventsList.Add( serializedObject.FindProperty( "OnDead" ) );

            mainFoProp = serializedObject.FindProperty( "mainFo" );
            eventsFoProp = serializedObject.FindProperty( "eventsFo" );
        }

        // OnInspectorGUI
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShowParameters();
            serializedObject.ApplyModifiedProperties();
        }

        // Show Parameters
        protected virtual void ShowParameters()
        {
            ASKEditorHelper.LargeFoldout( mainFoProp, "Main", () => ShowMainParams( true ) );
            ASKEditorHelper.LargeFoldout( eventsFoProp, "Events", () => eventsList.ForEach( evnt => EditorGUILayout.PropertyField( evnt ) ) );
        }
        

        // ShowMain Params
        protected virtual void ShowMainParams( bool showDeathDrops )
        {
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( immortalProp );
            GUILayout.Space( 5f );

            GUI.enabled = !immortalProp.boolValue;
            EditorGUILayout.PropertyField( armorTypeProp );
            ASKEditorHelper.ShowMinMaxSlider( currentHealthProp, maxHealthProp, 1f, 200f, "Health Level", true );
            GUI.enabled = true;

            // Regeneration
            EditorGUILayout.PropertyField( regenerationProp );
            GUI.enabled = regenerationProp.boolValue;
            GUILayout.BeginHorizontal();
            GUILayout.Space( 15 );
            EditorGUILayout.IntSlider( amountProp, 1, 10 );
            GUILayout.EndHorizontal();
            ASKEditorHelper.ShowSubSlider( ref delayProp, .1f, 5f, "Delay", 15f );
            ASKEditorHelper.ShowSubSlider( ref intervalProp, .01f, 5f, "Interval", 15f );
            GUI.enabled = true;

            GUILayout.Space( 5f );
            float currentHealth = currentHealthProp.intValue;
            float maxHealth = maxHealthProp.intValue;
            float percent = currentHealth / maxHealth;
            ASKEditorHelper.ShowProgressBar( percent, "Percent: " + Mathf.RoundToInt( percent * 100f ) );

            if( showDeathDrops ) {
                ShowDeathDrops( "Spawn objects delay", "Dropped objects after destroy", "Dropped pickups after destroy" );
            }                
        }

        // ShowDeathDrops
        protected void ShowDeathDrops( string sliderLabel, string deathObjectLabel, string deathDropsLabel )
        {
            GUILayout.Space( 5f );
            ASKEditorHelper.ShowMinMaxSlider( destroyBodyDelayProp, spawnObjectsDelayProp, 0f, 15f, sliderLabel );

            new ASKReorderableListDrawer( deathObjectList, deathObjectLabel ).DoDraw();

            EditorGUILayout.PropertyField( onlyOneDropProp );
            using( var rld = new ASKReorderableListDrawer( deathDropsList, deathDropsLabel ) )
            {
                rld.topSpace = rld.bottomSpace = 0f;
            }                
        }        
    };
}