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
    public static class InputSettingsTab
    {
        private static string MAIN_DATABASE_PATH { get { return ASKWindow.mainDirectory + "/InputSettings.asset"; } }
        private static string TMP_DATABASE_PATH { get { return ASKWindow.mainDirectory + "/tmp/InputSettingsTMP.asset"; } }
        
        //
        private static SerializedObject serializedObject = null;
        private static SerializedProperty actionDatabaseArray, axesDatabaseArray;

        private static ReorderableList[]
            actionAxesList = new ReorderableList[ 0 ],
            keysList = new ReorderableList[ 0 ],
            unityAxesList = new ReorderableList[ 0 ],
            customKeysList = new ReorderableList[ 0 ];

        private static int actionSel, axesSel, currentTab;
        private static Vector2 leftScroll, rightScroll;

        private static readonly string[] tabs = { "Actions", "Axes" }; 


        // Load CurrentAssetFile
        private static InputSettings LoadAssetFile( string path )
        {
            InputSettings currentFile = AssetDatabase.LoadAssetAtPath( path, typeof( InputSettings ) ) as InputSettings;

            if( currentFile == null )
            {
                currentFile = ScriptableObject.CreateInstance<InputSettings>();
                AssetDatabase.CreateAsset( currentFile, path );
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return currentFile;
        }

        // Save CopyAssetFile
        private static void SaveCopyAssetFile( string copyFrom, string copyTo )
        {
            if( copyFrom == MAIN_DATABASE_PATH )
                LoadAssetFile( copyFrom );

            AssetDatabase.DeleteAsset( copyTo );
            AssetDatabase.CopyAsset( copyFrom, copyTo );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        

        // Setup Tab
        internal static void SetupTab()
        {
            if( serializedObject == null )
                SaveCopyAssetFile( MAIN_DATABASE_PATH, TMP_DATABASE_PATH );

            serializedObject = new SerializedObject( LoadAssetFile( TMP_DATABASE_PATH ) );

            //
            actionDatabaseArray = serializedObject.FindProperty( "actionDatabase" );
            axesDatabaseArray = serializedObject.FindProperty( "axesDatabase" );
        }

        // Reload Settings
        internal static void ReloadSettings()
        {
            SaveCopyAssetFile( MAIN_DATABASE_PATH, TMP_DATABASE_PATH );
            FullReset();
            SetupTab();
        }

        // Save Settings
        internal static void SaveSettings()
        {
            SaveCopyAssetFile( TMP_DATABASE_PATH, MAIN_DATABASE_PATH );
        }

        
        // OnWindowGUI
        internal static void OnWindowGUI()
        {
            // BEGIN
            serializedObject.Update();
            // BEGIN

            using( new ASKEditorChangeCheck( ASKWindow.SetDirtyData ) )
            {
                switch( currentTab )
                {
                    case 0: //Actions
                        ShowLeftActionsSide();
                        break;
                    case 1: //Axes
                        ShowLeftAxesSide();
                        break;
                }

                ShowRightSide();
            }            

            // END
            serializedObject.ApplyModifiedProperties();
            // END
        }


        // Show LeftActionsSide
        private static void ShowLeftActionsSide()
        {
            int actionDatabaseSize = actionDatabaseArray.arraySize;

            leftScroll = EditorGUILayout.BeginScrollView( leftScroll, "box", GUILayout.Width( 200f ), GUILayout.ExpandHeight( true ) );

            GUILayout.Space( 5f );
            EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
            GUILayout.Space( 25f );
            bool add = GUILayout.Button( "Add Action", GUILayout.Height( 35f ) );
            GUI.enabled = true;
            GUILayout.Space( 25f );
            EditorGUILayout.EndHorizontal();
            GUILayout.Space( 5f );

            EditorGUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
            if( actionDatabaseSize > 0 )
                actionSel = GUILayout.SelectionGrid( actionSel, ASKWindow.GetNames( actionDatabaseArray ), 1 );
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical( GUILayout.Width( 25f ) );
            GUILayout.Space( 5f );
            GUI.enabled = ( actionDatabaseSize > 0 );
            bool delete = GUILayout.Button( "X" );
            GUI.enabled = true;
            GUI.enabled = ASKWindow.NotBegin( actionSel );
            GUILayout.Space( 15f );
            bool moveUp = GUILayout.Button( "▲", GUILayout.Height( 30f ) );
            GUI.enabled = true;
            GUI.enabled = ASKWindow.NotEnd( actionSel, actionDatabaseSize );
            bool moveDown = GUILayout.Button( "▼", GUILayout.Height( 30f ) );
            GUI.enabled = true;
            EditorGUILayout.EndVertical();


            if( add )
            {
                actionDatabaseArray.InsertArrayElementAtIndex( actionDatabaseSize );
                SerializedProperty actionDatabaseElement = actionDatabaseArray.GetArrayElementAtIndex( actionDatabaseSize );

                actionDatabaseSize = actionDatabaseArray.arraySize;
                actionSel = ( actionDatabaseSize > 1 ) ? actionSel : 0;

                actionDatabaseElement.FindPropertyRelative( "name" ).stringValue = "New Action " + actionDatabaseSize;
                actionDatabaseElement.FindPropertyRelative( "type" ).enumValueIndex = 0;
                actionDatabaseElement.FindPropertyRelative( "keys" ).ClearArray();
                actionDatabaseElement.FindPropertyRelative( "actionAxes" ).ClearArray();
            }
            else if( moveUp )
            {
                actionDatabaseArray.MoveArrayElement( actionSel - 1, actionSel-- );
            }
            else if( moveDown )
            {
                actionDatabaseArray.MoveArrayElement( actionSel + 1, actionSel++ );
            }
            else if( delete )
            {
                actionDatabaseArray.DeleteArrayElementAtIndex( actionSel );
                actionDatabaseSize = actionDatabaseArray.arraySize;
                actionSel = ASKWindow.NotEnd( actionSel, actionDatabaseSize ) ? actionSel : actionDatabaseSize - 1;
            }
        }

        // Show LeftAxesSide
        private static void ShowLeftAxesSide()
        {
            int axesDatabaseSize = axesDatabaseArray.arraySize;

            leftScroll = EditorGUILayout.BeginScrollView( leftScroll, "box", GUILayout.Width( 200f ), GUILayout.ExpandHeight( true ) );

            GUILayout.Space( 5f );
            EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
            GUILayout.Space( 25f );
            bool add = GUILayout.Button( "Add Axis", GUILayout.Height( 35f ) );
            GUI.enabled = true;
            GUILayout.Space( 25f );
            EditorGUILayout.EndHorizontal();
            GUILayout.Space( 5f );

            EditorGUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );
            if( axesDatabaseSize > 0 )
                axesSel = GUILayout.SelectionGrid( axesSel, ASKWindow.GetNames( axesDatabaseArray ), 1 );
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical( GUILayout.Width( 25f ) );
            GUILayout.Space( 5f );
            GUI.enabled = ( axesDatabaseSize > 0 );
            bool delete = GUILayout.Button( "X" );
            GUI.enabled = true;
            GUI.enabled = ASKWindow.NotBegin( axesSel );
            GUILayout.Space( 15f );
            bool moveUp = GUILayout.Button( "▲", GUILayout.Height( 30f ) );
            GUI.enabled = true;
            GUI.enabled = ASKWindow.NotEnd( axesSel, axesDatabaseSize );
            bool moveDown = GUILayout.Button( "▼", GUILayout.Height( 30f ) );
            GUI.enabled = true;
            EditorGUILayout.EndVertical();


            if( add )
            {
                axesDatabaseArray.InsertArrayElementAtIndex( axesDatabaseSize );
                SerializedProperty axesDatabaseElement = axesDatabaseArray.GetArrayElementAtIndex( axesDatabaseSize );

                axesDatabaseSize = axesDatabaseArray.arraySize;
                axesSel = ( axesDatabaseSize > 1 ) ? axesSel : 0;

                axesDatabaseElement.FindPropertyRelative( "name" ).stringValue = "New Axis " + axesDatabaseSize;
                axesDatabaseElement.FindPropertyRelative( "type" ).enumValueIndex = 0;
                axesDatabaseElement.FindPropertyRelative( "unityAxes" ).ClearArray();
                axesDatabaseElement.FindPropertyRelative( "customKeys" ).ClearArray();
                axesDatabaseElement.FindPropertyRelative( "normalize" ).boolValue = false;
            }
            else if( moveUp )
            {
                axesDatabaseArray.MoveArrayElement( axesSel - 1, axesSel-- );
            }
            else if( moveDown )
            {
                axesDatabaseArray.MoveArrayElement( axesSel + 1, axesSel++ );
            }
            else if( delete )
            {
                axesDatabaseArray.DeleteArrayElementAtIndex( axesSel );
                axesDatabaseSize = axesDatabaseArray.arraySize;
                axesSel = ASKWindow.NotEnd( axesSel, axesDatabaseSize ) ? axesSel : axesDatabaseSize - 1;
            }
        }


        // Show RightSide
        private static void ShowRightSide()
        {
            rightScroll = EditorGUILayout.BeginScrollView( rightScroll, "box", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );

            GUILayout.Space( 7f );
            currentTab = GUILayout.Toolbar( currentTab, tabs, GUILayout.ExpandWidth( true ), GUILayout.Height( 25f ) );
            GUILayout.Space( 15f );

            EditorGUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );

            const float width = 90f;
            const float space = 10f;

            switch( currentTab )
            {
                case 0: //Actions
                    int actionSize = actionDatabaseArray.arraySize;
                    if( actionSize > 0 )
                    {
                        SerializedProperty actionDatabaseElement = actionDatabaseArray.GetArrayElementAtIndex( actionSel );
                        SerializedProperty actionName = actionDatabaseElement.FindPropertyRelative( "name" );
                        SerializedProperty actionTypeProp = actionDatabaseElement.FindPropertyRelative( "type" );
                        SerializedProperty keysArray = actionDatabaseElement.FindPropertyRelative( "keys" );
                        SerializedProperty actionAxesArray = actionDatabaseElement.FindPropertyRelative( "actionAxes" );

                        if( actionSize != keysList.Length )
                            keysList = new ReorderableList[ actionSize ];

                        if( actionSize != actionAxesList.Length )
                            actionAxesList = new ReorderableList[ actionSize ];

                        if( keysList[ actionSel ] == null )
                            keysList[ actionSel ] = new ReorderableList( serializedObject, keysArray );

                        if( actionAxesList[ actionSel ] == null )
                            actionAxesList[ actionSel ] = new ReorderableList( serializedObject, actionAxesArray );

                        GUILayout.Space( 5f );
                        ASKEditorHelper.ShowFixedPropertyField( ref actionName, "Name", space, width );

                        GUILayout.Space( 5f );
                        ASKEditorHelper.ShowFixedPropertyField( ref actionTypeProp, "Type", space, width );

                        GUILayout.Space( 5f );
                        int enumValueIndex = actionTypeProp.enumValueIndex;
                        //
                        if( enumValueIndex == 0 || enumValueIndex == 2 )
                        {
                            DrawSimpleList( keysList[ actionSel ], "Keys", space );
                        }
                        GUILayout.Space( 5f );
                        if( enumValueIndex == 1 || enumValueIndex == 2 )
                        {
                            ReorderableList actionsListElement = actionAxesList[ actionSel ];

                            GUILayout.BeginHorizontal();
                            GUILayout.Space( space );
                            GUILayout.BeginVertical();
                            actionsListElement.drawHeaderCallback = ( Rect rect ) =>
                            {
                                EditorGUI.LabelField( rect, "Action Axes" );
                            };

                            float pHeight = EditorGUIUtility.singleLineHeight;
                            if( actionsListElement.count > 0 )
                                actionsListElement.elementHeight = pHeight * 3f;
                            else
                                actionsListElement.elementHeight = pHeight;

                            actionsListElement.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
                            {
                                SerializedProperty listElement = actionsListElement.serializedProperty.GetArrayElementAtIndex( index );
                                SerializedProperty axisName = listElement.FindPropertyRelative( "axisName" );
                                SerializedProperty axisSource = listElement.FindPropertyRelative( "axisSource" );
                                SerializedProperty threshold = listElement.FindPropertyRelative( "threshold" );
                                SerializedProperty axisStateClamp = listElement.FindPropertyRelative( "axisStateClamp" );

                                const float pSpace = 5f;
                                float pWidth = EditorGUIUtility.currentViewWidth;                                
                                float startX = rect.x;

                                rect.y += 2f;
                                rect.x = startX / 2f;
                                rect.width = pWidth / 1.58f;
                                rect.height = pHeight * 2.8f;
                                EditorGUI.HelpBox( rect, string.Empty, MessageType.None );

                                // Axis + Source

                                rect.x = startX;
                                rect.y += 4f;
                                rect.height = pHeight;

                                rect.width = pWidth / 10f;
                                EditorGUI.LabelField( rect, "Axis Name" );

                                rect.x += rect.width + pSpace;
                                rect.width = pWidth / 6f;
                                EditorGUI.PropertyField( rect, axisName, GUIContent.none );


                                rect.x += rect.width + pSpace * 3f;
                                rect.width = pWidth / 16f;
                                EditorGUI.LabelField( rect, "Source" );

                                rect.x += rect.width + space;
                                rect.width = pWidth / 4.4f;
                                EditorGUI.PropertyField( rect, axisSource, GUIContent.none );


                                // Threshold + Clamp

                                rect.x = startX;
                                rect.y += pHeight + pSpace;
                                rect.width = pWidth / 10f;
                                EditorGUI.LabelField( rect, "Threshold" );

                                rect.x += rect.width + pSpace;
                                rect.width = pWidth / 6f;
                                EditorGUI.PropertyField( rect, threshold, GUIContent.none );

                                rect.x += rect.width + pSpace * 3f;                                
                                rect.width = pWidth / 16f;
                                EditorGUI.LabelField( rect, "Clamp" );

                                rect.x += rect.width + space;
                                rect.width = pWidth / 4.4f;
                                EditorGUI.PropertyField( rect, axisStateClamp, GUIContent.none );
                            };

                            ASKReorderableListDrawer.DoLayoutList( actionsListElement, ASKWindow.SetDirtyData );
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }                        
                    }
                    break;

                case 1: //Axes
                    int axesSize = axesDatabaseArray.arraySize;
                    if( axesSize > 0 )
                    {
                        SerializedProperty axesDatabaseElement = axesDatabaseArray.GetArrayElementAtIndex( axesSel );
                        SerializedProperty axesName = axesDatabaseElement.FindPropertyRelative( "name" );
                        SerializedProperty axisTypeProp = axesDatabaseElement.FindPropertyRelative( "type" );
                        SerializedProperty normalizeProp = axesDatabaseElement.FindPropertyRelative( "normalize" );
                        SerializedProperty unityAxesArray = axesDatabaseElement.FindPropertyRelative( "unityAxes" );
                        SerializedProperty customKeysArray = axesDatabaseElement.FindPropertyRelative( "customKeys" );

                        if( axesSize != unityAxesList.Length )
                            unityAxesList = new ReorderableList[ axesSize ];

                        if( axesSize != customKeysList.Length )
                            customKeysList = new ReorderableList[ axesSize ];

                        if( unityAxesList[ axesSel ] == null )
                            unityAxesList[ axesSel ] = new ReorderableList( serializedObject, unityAxesArray );

                        if( customKeysList[ axesSel ] == null )
                            customKeysList[ axesSel ] = new ReorderableList( serializedObject, customKeysArray );

                        GUILayout.Space( 5f );
                        ASKEditorHelper.ShowFixedPropertyField( ref axesName, "Name", space, width );

                        GUILayout.Space( 5f );
                        ASKEditorHelper.ShowFixedPropertyField( ref axisTypeProp, "Type", space, width );

                        GUILayout.Space( 5f );
                        ASKEditorHelper.ShowFixedPropertyField( ref normalizeProp, "Normalize", space, width );

                        GUILayout.Space( 10f );
                        int enumValueIndex = axisTypeProp.enumValueIndex;
                        //
                        if( enumValueIndex == 1 || enumValueIndex == 2 )
                        {
                            ReorderableList keysListElement = customKeysList[ axesSel ];

                            GUILayout.BeginHorizontal();
                            GUILayout.Space( space );
                            GUILayout.BeginVertical();
                            keysListElement.drawHeaderCallback = ( Rect rect ) =>
                            {
                                EditorGUI.LabelField( rect, "Positive & Negative Keys" );
                            };

                            keysListElement.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
                            {                                
                                SerializedProperty listElement = keysListElement.serializedProperty.GetArrayElementAtIndex( index );
                                SerializedProperty positiveKey = listElement.FindPropertyRelative( "positiveKey" );
                                SerializedProperty negativeKey = listElement.FindPropertyRelative( "negativeKey" );

                                const float pSpace = 5f;
                                float pWidth = EditorGUIUtility.currentViewWidth;
                                float pHeight = EditorGUIUtility.singleLineHeight;
                                float startX = rect.x;

                                rect.x = startX / 2f;
                                rect.width = pWidth / 1.58f;
                                rect.height = pHeight * 1.25f;
                                EditorGUI.HelpBox( rect, string.Empty, MessageType.None );

                                rect.x = startX;
                                rect.y += 2f;
                                rect.height = pHeight;

                                rect.width = pWidth / 8.75f;
                                EditorGUI.LabelField( rect, "Positive Key" );

                                rect.x += rect.width + pSpace;
                                rect.width = pWidth / 6f;
                                EditorGUI.PropertyField( rect, positiveKey, GUIContent.none );

                                rect.x += rect.width + pSpace * 2f;
                                rect.width = pWidth / 8.75f;
                                EditorGUI.LabelField( rect, "Negative Key" );

                                rect.x += rect.width + pSpace;
                                rect.width = pWidth / 6f;
                                EditorGUI.PropertyField( rect, negativeKey, GUIContent.none );

                                rect.x += rect.width + 10f;
                                rect.width = 22f;
                            };
                            ASKReorderableListDrawer.DoLayoutList( keysListElement, ASKWindow.SetDirtyData );
                            GUILayout.EndVertical();
                            GUILayout.EndHorizontal();
                        }

                        if( enumValueIndex == 0 || enumValueIndex == 2 )
                        {
                            DrawSimpleList( unityAxesList[ axesSel ], "Unity Axes Names", space );
                        }
                    }
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        // DrawSimpleList
        static void DrawSimpleList( ReorderableList list, string label, float space )
        {
            var rld = new ASKReorderableListDrawer( list, label, ASKWindow.SetDirtyData );
            rld.offset = space;
            rld.HideFoldout();            
            rld.DoDraw();
        }



        // FullReset
        internal static void FullReset()
        {
            serializedObject = null;
            axesDatabaseArray = actionDatabaseArray = null;

            actionAxesList = new ReorderableList[ 0 ];
            keysList = new ReorderableList[ 0 ];
            unityAxesList = new ReorderableList[ 0 ];
            customKeysList = new ReorderableList[ 0 ];

            actionSel = axesSel = currentTab = 0;
            leftScroll = rightScroll = Vector2.zero;
        }
    };
}