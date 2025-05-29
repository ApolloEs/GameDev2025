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
    [CustomEditor( typeof( Firearms ) ), CanEditMultipleObjects]
    public class FirearmsEditor : Editor
    {
        private SerializedProperty
            autoReloadProp,
            maxAmmoProp,
            addDamageProp, addSpeedProp, rateOfFireProp, dispersionProp,
            shotDelayProp, shotSFXProp, emptySFXProp, reloadSFXProp,
            shellOuterProp, shelloutForceProp, projectileOuterProp,
            muzzleFlashProp,
            projectilesPerShotProp, hitMaskProp,
            minCameraShakeProp, maxCameraShakeProp;

        private ReorderableList firingModesList, projectilesList;
        private bool isPlayerWeapon;

        static string[] ammoNames;



        // OnEnable
        void OnEnable()
        {
            maxAmmoProp = serializedObject.FindProperty( "maxAmmo" );

            autoReloadProp = serializedObject.FindProperty( "autoReload" );

            addDamageProp = serializedObject.FindProperty( "addDamage" );
            addSpeedProp = serializedObject.FindProperty( "addSpeed" );

            rateOfFireProp = serializedObject.FindProperty( "rateOfFire" );
            dispersionProp = serializedObject.FindProperty( "dispersion" );

            shotSFXProp = serializedObject.FindProperty( "shotSFX" );
            emptySFXProp = serializedObject.FindProperty( "emptySFX" );
            reloadSFXProp = serializedObject.FindProperty( "reloadSFX" );

            shotDelayProp = serializedObject.FindProperty( "shotDelay" );
            projectileOuterProp = serializedObject.FindProperty( "projectileOuter" );

            shellOuterProp = serializedObject.FindProperty( "shellOuter" );
            shelloutForceProp = serializedObject.FindProperty( "shelloutForce" );

            muzzleFlashProp = serializedObject.FindProperty( "muzzleFlash" );

            projectilesPerShotProp = serializedObject.FindProperty( "projectilesPerShot" );
            hitMaskProp = serializedObject.FindProperty( "hitMask" );

            firingModesList = new ReorderableList( serializedObject, serializedObject.FindProperty( "firingModes" ) );
            projectilesList = new ReorderableList( serializedObject, serializedObject.FindProperty( "projectiles" ) );

            minCameraShakeProp = serializedObject.FindProperty( "minCameraShake" );
            maxCameraShakeProp = serializedObject.FindProperty( "maxCameraShake" );

            Component comp = ( target as Component );
            isPlayerWeapon = ( comp.GetComponentInParent<PlayerCharacter>() != null );

            if( isPlayerWeapon ) {
                ammoNames = comp.GetComponentInParent<AmmoBackpack>().GetNames();
            }            
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

            if( isPlayerWeapon )
            {
                GUILayout.Space( 5f );
                EditorGUILayout.PropertyField( maxAmmoProp );
                EditorGUILayout.PropertyField( autoReloadProp );
            }

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( addDamageProp );
            EditorGUILayout.PropertyField( addSpeedProp );            

            EditorGUILayout.Slider( rateOfFireProp, 1f, 2500f );
            EditorGUILayout.PropertyField( dispersionProp );
                        
            GUILayout.Space( 5f );
            ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, shotSFXProp );

            if( isPlayerWeapon )
            {
                ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, emptySFXProp );
                ASKEditorHelper.ShowSFXPropertyAndPlayButton( serializedObject, reloadSFXProp );
            }
            
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( projectileOuterProp );

            GUI.enabled = ( projectileOuterProp.objectReferenceValue != null );
            ASKEditorHelper.ShowSubSlider( ref shotDelayProp, 0f, 2f, "Spawn Delay", 20f );
            ASKEditorHelper.ShowIntSubSlider( ref projectilesPerShotProp, 1, 12, "Projectiles Per Shot", 20f );
            GUI.enabled = true;            

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( shellOuterProp );

            GUI.enabled = ( shellOuterProp.objectReferenceValue != null );
            ASKEditorHelper.ShowSubSlider( ref shelloutForceProp, 10f, 50f, "Shellout Force", 20f );
            GUI.enabled = true;

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( muzzleFlashProp );

            if( isPlayerWeapon )
            {
                // camera shake
                GUILayout.Space( 5f );
                ASKEditorHelper.ShowMinMaxSlider( minCameraShakeProp, maxCameraShakeProp, .01f, 2f, "Camera Shake Range" );

                // firing modes
                using( var listDrawer = new ASKReorderableListDrawer( firingModesList, "Firing Modes" ) )
                {
                    listDrawer.minItems = 1;

                    listDrawer.OnDrawElement = ( Rect rect, int index, bool isActive, bool isFocused ) =>
                    {
                        SerializedProperty property = listDrawer.GetArrayElementAtIndex( index );
                        ASKEditorHelper.DrawEnumAsToolbar( rect, property, false );
                        return rect;
                    };
                }
            }

            // used projectiles
            using( var listDrawer = new ASKReorderableListDrawer( projectilesList, "Used Projectiles" ) )
            {
                listDrawer.DrawBox();
                listDrawer.minItems = 1;
                listDrawer.rows = isPlayerWeapon ? 2 : 1;
                listDrawer.removeBtnMode = ASKReorderableListDrawer.ERemoveBtnMode.Bottom;

                listDrawer.OnDrawElement = ( Rect rect, int index, bool isActive, bool isFocused ) =>
                {
                    SerializedProperty listElement = projectilesList.serializedProperty.GetArrayElementAtIndex( index );
                    SerializedProperty ammoTypeProp = listElement.FindPropertyRelative( "ammoType" );
                    SerializedProperty currentAmmoProp = listElement.FindPropertyRelative( "currentAmmo" );
                    SerializedProperty projectileProp = listElement.FindPropertyRelative( "projectile" );
                    SerializedProperty shellProp = listElement.FindPropertyRelative( "shell" );

                    const float SPACE = 5f;
                    const float DSPACE = 10f;
                    const float QSPACE = 20f;

                    float startX = rect.x;
                    float startWidth = rect.width;

                    const float MIN_WIDTH = 20f;
                    const float LABEL_WIDTH = 60f;

                    if( isPlayerWeapon )
                    {
                        rect.width = LABEL_WIDTH;
                        EditorGUI.LabelField( rect, "Type" );

                        rect.x += rect.width + SPACE;
                        rect.width = startWidth * .4f;
                        ASKEditorHelper.DrawStringPopup( rect, ammoTypeProp, ammoNames );

                        // current Ammo

                        rect.x += rect.width + QSPACE;
                        rect.width = LABEL_WIDTH - DSPACE;
                        EditorGUI.LabelField( rect, "Amount" );

                        rect.x += rect.width + SPACE;
                        rect.width = startWidth - rect.x + QSPACE + DSPACE;

                        if( rect.width > MIN_WIDTH )
                        {
                            EditorGUI.PropertyField( rect, currentAmmoProp, GUIContent.none );
                            currentAmmoProp.intValue = Mathf.Clamp( currentAmmoProp.intValue, 0, maxAmmoProp.intValue );
                        }

                        rect.x = startX;
                        rect.y += listDrawer.singleLineHeight + SPACE;
                    }

                    // Projectile                
                    rect.width = LABEL_WIDTH;
                    EditorGUI.LabelField( rect, projectileProp.displayName );

                    rect.x += rect.width + SPACE;
                    rect.width = startWidth * .4f;
                    EditorGUI.PropertyField( rect, projectileProp, GUIContent.none );


                    // Shell
                    rect.x += rect.width + QSPACE;
                    rect.width = LABEL_WIDTH - DSPACE;
                    EditorGUI.LabelField( rect, "Shell" );

                    rect.x += rect.width + SPACE;
                    rect.width = startWidth - rect.x + QSPACE + DSPACE;
                    if( rect.width > MIN_WIDTH ) EditorGUI.PropertyField( rect, shellProp, GUIContent.none );

                    return rect;
                };
            }
        }
    };
}