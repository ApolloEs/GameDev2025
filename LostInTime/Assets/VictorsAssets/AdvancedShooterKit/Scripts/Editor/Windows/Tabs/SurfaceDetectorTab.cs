/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using AdvancedShooterKit;
using Object = UnityEngine.Object;

namespace AdvancedShooterKitEditor
{
    public static class SurfaceDetectorTab
    {
        private static string MAIN_DATABASE_PATH { get { return ASKWindow.mainDirectory + "/SurfaceDetector.asset"; } }
        private static string TMP_DATABASE_PATH { get { return ASKWindow.mainDirectory + "/tmp/SurfaceDetectorTMP.asset"; } }


        private static SerializedObject serializedObject;
        private static SerializedProperty surfacesArray;
        private static ReorderableList surfacesList;

        private static int currentTab, oldTab;
        private static Vector2 leftScroll, rightScroll;

        private static bool searchOrFilter;
        private static string searchText = "", filterElementsInfo = "ALL";

        private static readonly string[] tabs = { "Meshes Materials", "Terrain Textures" };
        private static readonly Type[] tabsTypes = { typeof( Material ), typeof( Texture ) };
        private static readonly ReorderableList[] tabsLists = new ReorderableList[ tabs.Length ];

        static bool haveNullItems;

        static bool dirtyPreviews = true;
        static void SetDirtyPreviews() { dirtyPreviews = true; }

        static Texture[] cachedPreviews = new Texture[ 0 ];

        // Refresh PreviewsCache
        private static void RefreshPreviewsCache( SerializedProperty arrayProperty )
        {
            int arraySize = arrayProperty.arraySize;

            AssetPreview.SetPreviewTextureCacheSize( arraySize + 1 );
            cachedPreviews = new Texture[ arraySize ];

            for( int i = 0; i < arraySize; i++ )
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex( i );
                SerializedProperty targetProp = element.FindPropertyRelative( "targetObject" );

                cachedPreviews[ i ] = AssetPreview.GetAssetPreview( targetProp.objectReferenceValue );
            }

            ASKWindow.MarkDirty();
        }

        // CheckPreviewsCache
        private static void CheckPreviewsCache()
        {
            for( int i = 0; i < cachedPreviews.Length; i++ )
            {
                if( cachedPreviews[ i ] == null )
                {
                    SetDirtyPreviews();
                    break;
                }
            }
        }


        static string defaultName = string.Empty;
        static string[] cachedNames = new string[ 0 ], moreNames = new string[ 0 ];
        private static void RefreshNames()
        {
            int arraySize = surfacesArray.arraySize;
            string[] names = new string[ arraySize ];

            for( int i = 0; i < arraySize; i++ )
            {
                names[ i ] = surfacesArray.GetArrayElementAtIndex( i ).stringValue;
            }

            moreNames = new string[ names.Length + 1 ];
            moreNames[ 0 ] = "GENERIC";

            for( int i = 0; i < names.Length; i++ )
            {
                moreNames[ i + 1 ] = names[ i ];
            }

            cachedNames = names;
        }


        // Register Undo
        private static void RegisterUndo( string name )
        {
            Undo.RecordObject( serializedObject.targetObject, name );
        }
        // Clear Undo
        private static void ClearUndo()
        {
            if( serializedObject != null )
            {
                Undo.ClearUndo( serializedObject.targetObject );
            }
        }


        // Load CurrentAssetFile
        private static SurfaceDetector LoadAssetFile( string path )
        {
            SurfaceDetector currentFile = AssetDatabase.LoadAssetAtPath<SurfaceDetector>( path );

            if( currentFile == null )
            {
                currentFile = ScriptableObject.CreateInstance<SurfaceDetector>();
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
            {
                LoadAssetFile( copyFrom );
            }

            AssetDatabase.DeleteAsset( copyTo );
            AssetDatabase.CopyAsset( copyFrom, copyTo );
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Setup Tab
        internal static void SetupTab()
        {
            if( serializedObject == null )
            {
                SaveCopyAssetFile( MAIN_DATABASE_PATH, TMP_DATABASE_PATH );
            }

            serializedObject = new SerializedObject( LoadAssetFile( TMP_DATABASE_PATH ) );

            surfacesArray = serializedObject.FindProperty( "m_Names" );
            SerializedProperty materialsArray = serializedObject.FindProperty( "m_Materials" );
            SerializedProperty texturesArray = serializedObject.FindProperty( "m_Textures" );

            surfacesList = new ReorderableList( serializedObject, surfacesArray );
            tabsLists[ 0 ] = new ReorderableList( serializedObject, materialsArray, true, true, false, false );
            tabsLists[ 1 ] = new ReorderableList( serializedObject, texturesArray, true, true, false, false );

            surfacesList.onAddCallback = list =>
            {
                int index = list.count;
                surfacesArray.InsertArrayElementAtIndex( index );
                surfacesArray.GetArrayElementAtIndex( index ).stringValue = "New Surface " + list.count;
            };

            SetDirtyPreviews();
        }

        // Reload Settings
        internal static void ReloadSettings()
        {
            SaveCopyAssetFile( MAIN_DATABASE_PATH, TMP_DATABASE_PATH );
            FullReset();
            SetupTab();
            ClearUndo();
        }

        // Save Settings
        internal static void SaveSettings()
        {
            SaveCopyAssetFile( TMP_DATABASE_PATH, MAIN_DATABASE_PATH );
        }


        // OnWindowGUI
        internal static void OnWindowGUI()
        {
            RefreshNames();

            // BEGIN
            serializedObject.Update();
            // BEGIN

            ShowLeftSide();
            ShowRightSide();

            // END
            serializedObject.ApplyModifiedProperties();
            // END

            CheckPreviewsCache();
        }


        // Show LeftSide
        private static void ShowLeftSide()
        {
            using( ASKEditorLayout.ScrollView( ref leftScroll, "box", GUILayout.Width( 215f ), GUILayout.ExpandHeight( true ) ) )
            {
                var rld = new ASKReorderableListDrawer( surfacesList, "Surfaces", ASKWindow.SetDirtyData );
                rld.HideFoldout();
                rld.DoDraw();
            }
        }


        // Show RightSide
        private static void ShowRightSide()
        {
            int surfacesSize = surfacesArray.arraySize;

            GUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) );

            if( surfacesSize > 0 )
            {
                using( ASKEditorLayout.Horizontal( "Toolbar" ) )
                {
                    searchText = EditorGUILayout.TextField( searchText, ( GUIStyle )"ToolbarSeachTextField" );

                    GUIStyle toolbarSearchCancelButton = "ToolbarSeachCancelButton" + ( string.IsNullOrEmpty( searchText ) ? "Empty" : string.Empty );
                    if( GUILayout.Button( string.Empty, toolbarSearchCancelButton ) )
                    {
                        searchText = string.Empty;
                        GUIUtility.keyboardControl = 0;
                    }

                    Rect filterBtnRect;
                    GUIContent filterBtnLabel = new GUIContent( string.IsNullOrEmpty( filterElementsInfo ) ? "GENERIC" : filterElementsInfo );
                    if( ASKEditorHelper.DrawStaticButton( filterBtnLabel, EditorStyles.toolbarDropDown, out filterBtnRect
                        , GUILayout.ExpandWidth( true ), GUILayout.MaxWidth( 125f ) ) )
                    {
                        GenericMenu.MenuFunction2 OnItemClick = obj =>
                        {
                            int index = ( int )obj;

                            if( index >= 0 )
                            {
                                filterElementsInfo = surfacesArray.GetArrayElementAtIndex( index ).stringValue;
                            }
                            else if( index == -1 )
                            {
                                filterElementsInfo = string.Empty;
                            }
                            else if( index == -2 )
                            {
                                filterElementsInfo = "ALL";
                            }

                            GUIUtility.keyboardControl = 0;
                        };

                        GenericMenu menu = new GenericMenu();

                        GUIContent content = new GUIContent( "ALL" );
                        menu.AddItem( content, ( content.text == filterElementsInfo ), OnItemClick, -2 );
                        menu.AddSeparator( string.Empty );

                        content = new GUIContent( "GENERIC" );
                        menu.AddItem( content, ( content.text == filterElementsInfo ), OnItemClick, -1 );
                        menu.AddSeparator( string.Empty );

                        for( int i = 0; i < surfacesSize; i++ )
                        {
                            content = new GUIContent( surfacesArray.GetArrayElementAtIndex( i ).stringValue );
                            menu.AddItem( content, ( content.text == filterElementsInfo ), OnItemClick, i );
                        }

                        menu.DropDown( filterBtnRect );
                    }

                    currentTab = GUILayout.Toolbar( currentTab, tabs, EditorStyles.toolbarButton, GUILayout.ExpandWidth( true ), GUILayout.MinWidth( 250f ) );
                }

                DrawTabList( tabsLists[ currentTab ], tabsTypes[ currentTab ] );
            }
            GUILayout.EndVertical();
        }

        // DrawTabList
        private static void DrawTabList( ReorderableList list, Type targetType )
        {
            SerializedProperty arrayProperty = list.serializedProperty;

            //DrawDragArea( arrayProperty, targetType );

            if( dirtyPreviews )
            {
                dirtyPreviews = false;
                RefreshPreviewsCache( arrayProperty );
            }

            Event currentEvent = Event.current;

            if( currentEvent.type == EventType.MouseUp )
            {
                SetDirtyPreviews();
            }

            if( oldTab != currentTab )
            {
                oldTab = currentTab;
                SetDirtyPreviews();
            }

            int arraySize = arrayProperty.arraySize;

            if( arraySize != cachedPreviews.Length )
            {
                RefreshPreviewsCache( arrayProperty );
            }


            bool removeNulls = false;
            int removedIndex = -1;
            Texture preview = null;

            using( ASKEditorLayout.ScrollView( ref rightScroll, GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) ) )
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Space( 10f );

                list.elementHeight = EditorGUIUtility.singleLineHeight * 3f;

                Rect headRect = new Rect( 0f, 0f, 0f, 0f );
                string dragDropLabel = "Drop new \"" + ( currentTab == 0 ? "materials" : "textures" ) + "\" here";                

                list.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField( rect, dragDropLabel );
                    headRect = rect;

                    if( haveNullItems )
                    {
                        rect.y += 1.5f;
                        rect.x = rect.xMax - 100f;
                        rect.width = 100f;
                        rect.height -= 3f;

                        if( GUI.Button( rect, "Remove NULL's", EditorStyles.miniButton ) )
                        {
                            removeNulls = true;
                        }

                        haveNullItems = false;
                    }
                };

                list.drawFooterCallback = rect =>
                {
                    if( list.count == 0 )
                    {
                        const float space = 5f;
                        float startY = rect.y;

                        rect.x += space;
                        rect.y = headRect.y + headRect.height + space;
                        rect.width -= space * 2f;
                        rect.height = startY - 40f;

                        GUIStyle centredStyle = GUI.skin.box;
                        centredStyle.alignment = TextAnchor.MiddleCenter;
                        centredStyle.normal.textColor = GUI.skin.button.normal.textColor;

                        GUI.Box( rect, dragDropLabel, centredStyle );

                        DraggingAndDropping( rect, arrayProperty, -1, targetType );
                    }
                };                

                list.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
                {
                    float startWidth = rect.width;
                    float singleLineHeight = EditorGUIUtility.singleLineHeight;

                    rect.y += 2f;
                    float startX = rect.x;

                    rect.x = startX - 15f;
                    rect.width = startWidth + 15f;
                    rect.height = list.elementHeight - 2f;
                    EditorGUI.HelpBox( rect, string.Empty, MessageType.None );

                    DraggingAndDropping( rect, arrayProperty, index, targetType );

                    SerializedProperty element = arrayProperty.GetArrayElementAtIndex( index );
                    SerializedProperty surfaceName = element.FindPropertyRelative( "surfaceName" );
                    SerializedProperty targetProp = element.FindPropertyRelative( "targetObject" );

                    Object target = targetProp.objectReferenceValue;

                    rect.y += 4f;
                    rect.x = startX;
                    rect.width = rect.height = singleLineHeight * 2.5f;
                    GUI.Box( rect, cachedPreviews[ index ] );

                    if( target != null )
                    {
                        if( rect.Contains( currentEvent.mousePosition ) )
                        {
                            if( currentEvent.button == 0 && currentEvent.type == EventType.MouseDown )
                            {
                                EditorGUIUtility.PingObject( target );
                            }

                            preview = cachedPreviews[ index ];
                        }
                    }
                    else
                    {
                        haveNullItems = true;
                    }


                    float afterImgX = rect.x += rect.width + 10f;

                    // name

                    rect.x = afterImgX;
                    rect.width = startWidth - 55f;
                    rect.height = singleLineHeight + 1f;
                    EditorGUI.LabelField( rect, ( target != null ) ? target.name : "NULL", EditorStyles.helpBox );


                    // popup

                    rect.x = afterImgX;
                    rect.y += singleLineHeight * 1.35f;
                    rect.width = startWidth - 150f;
                    rect.height = singleLineHeight;
                    ASKEditorHelper.DrawStringPopup( rect, surfaceName, cachedNames );

                    // remove

                    rect.height = singleLineHeight;
                    rect.width = 50f;
                    rect.x = startWidth - rect.width / 1.4f;
                    if( GUI.Button( rect, "X", EditorStyles.miniButton ) )
                    {
                        removedIndex = index;
                    }
                };


                bool isSearch = !string.IsNullOrEmpty( searchText );
                bool isFilter = ( filterElementsInfo != "ALL" );

                searchOrFilter = ( isSearch || isFilter );

                using( new ASKEditorChangeCheck( ASKWindow.SetDirtyData ) )
                {
                    if( searchOrFilter )
                    {
                        for( int i = 0; i < arraySize; i++ )
                        {
                            SerializedProperty element = arrayProperty.GetArrayElementAtIndex( i );
                            SerializedProperty surfaceName = element.FindPropertyRelative( "surfaceName" );
                            SerializedProperty targetProp = element.FindPropertyRelative( "targetObject" );

                            Object target = targetProp.objectReferenceValue;

                            if( target == null )
                            {
                                continue;
                            }

                            bool result = false;
                            bool foundByName = false;
                            bool foundByType = false;

                            if( isSearch )
                            {
                                result = foundByName = target.name.ToLower().Contains( searchText.ToLower() );
                            }

                            if( isFilter )
                            {
                                result = foundByType = ( surfaceName.stringValue == filterElementsInfo );
                            }

                            if( isSearch && isFilter )
                            {
                                result = foundByName && foundByType;
                            }

                            if( result )
                            {
                                Rect rect = GUILayoutUtility.GetRect( 0f, list.elementHeight + 2f );
                                rect.x += 20f;
                                rect.width -= rect.x + 5f;
                                list.drawElementCallback.Invoke( rect, i, false, false );
                            }
                        }
                    }
                    else
                    {
                        ASKReorderableListDrawer.DoLayoutList( list, ASKWindow.SetDirtyData );
                    }
                }                

                EditorGUILayout.EndVertical();

                GUILayout.Space( 5f );
            }

            
            if( preview != null )
            {
                Rect viewRect = new Rect( currentEvent.mousePosition, new Vector2( 128f, 128f ) );
                viewRect.x += 10f;
                viewRect.y += 15f;
                GUI.Box( viewRect, preview );

                ASKWindow.MarkDirty();
            }

            if( removedIndex > -1 )
            {
                RegisterUndo( "[SurfaceDetector] Delete element of index (" + removedIndex + ")" );
                arrayProperty.DeleteArrayElementAtIndex( removedIndex );
                ASKWindow.SetDirtyData();
            }

            if( removeNulls )
            {
                RegisterUndo( "[SurfaceDetector] Remove null items" );

                bool ditryNulls = true;

                while( ditryNulls )
                {
                    ditryNulls = false;

                    for( int i = 0; i < arrayProperty.arraySize; i++ )
                    {
                        SerializedProperty element = arrayProperty.GetArrayElementAtIndex( i );
                        SerializedProperty targetProp = element.FindPropertyRelative( "targetObject" );
                        if( targetProp.objectReferenceValue == null )
                        {
                            arrayProperty.DeleteArrayElementAtIndex( i );
                            ditryNulls = true;
                        }
                    }
                }

                ASKWindow.SetDirtyData();
            }
        }


        // Dragging AndDropping
        private static void DraggingAndDropping( Rect dropArea, SerializedProperty arrayProperty, int desiredIndex, Type targetType )
        {
            if( searchOrFilter )
            {
                return;
            }

            Event currentEvent = Event.current;

            if( dropArea.Contains( currentEvent.mousePosition ) == false )
            {
                return;
            }


            if( currentEvent.type == EventType.DragUpdated )
            {
                DragAndDrop.visualMode = IsDragValid( targetType ) ? DragAndDropVisualMode.Link : DragAndDropVisualMode.Rejected;
                currentEvent.Use();
            }
            else if( currentEvent.type == EventType.DragPerform )
            {
                DragAndDrop.AcceptDrag();

                GenericMenu.MenuFunction2 menuFunc = item =>
                {
                    defaultName = ( string )item;

                    serializedObject.Update();

                    bool needRegUndo = false;

                    for( int i = 0; i < DragAndDrop.objectReferences.Length; i++ )
                    {
                        string dragPath = DragAndDrop.paths[ i ];
                        string dragFolderPath;

                        if( IsDragFolder( dragPath, out dragFolderPath ) )
                        {
                            string searchPattern = ( currentTab == 0 ) ? "*.mat" : "*.*";
                            string[] allFiles = Directory.GetFiles( dragFolderPath, searchPattern, SearchOption.AllDirectories );

                            for( int fileNum = 0; fileNum < allFiles.Length; fileNum++ )
                            {
                                float progress = ( float )fileNum / ( float )allFiles.Length;
                                string progressInfo = Mathf.RoundToInt( progress * 100f ) + "% | " + dragPath;

                                EditorUtility.DisplayProgressBar( "Scanning...", progressInfo, progress );

                                string filePath = GetRightPartOfPath( allFiles[ fileNum ] );
                                var asset = AssetDatabase.LoadAssetAtPath( filePath, targetType );

                                if( asset != null && FindFreeOrAddCell( arrayProperty, asset, ref desiredIndex ) )
                                {
                                    needRegUndo = true;
                                }
                            }
                        }
                        else
                        {
                            if( FindFreeOrAddCell( arrayProperty, DragAndDrop.objectReferences[ i ], ref desiredIndex ) )
                            {
                                needRegUndo = true;
                            }
                        }
                    }

                    if( needRegUndo )
                    {
                        RegisterUndo( "[SurfaceDetector] Added new items" );
                        ASKWindow.SetDirtyData();
                    }

                    serializedObject.ApplyModifiedProperties();
                    currentEvent.Use();
                    SetDirtyPreviews();

                    EditorUtility.ClearProgressBar();
                };

                GenericMenu conMenu = new GenericMenu();

                for( int i = 0; i < moreNames.Length; i++ )
                {
                    conMenu.AddItem( new GUIContent( moreNames[ i ] ), false, menuFunc, moreNames[ i ] );
                }

                conMenu.ShowAsContext();
            }
        }

        // FindFree Or AddCell
        private static bool FindFreeOrAddCell( SerializedProperty arrayProperty, Object elementToAdd, ref int desiredIndex )
        {
            if( elementToAdd == null )
            {
                return false;
            }


            const string objFldName = "targetObject";

            for( int i = 0; i < arrayProperty.arraySize; i++ )
            {
                SerializedProperty element = arrayProperty.GetArrayElementAtIndex( i );
                SerializedProperty targetObject = element.FindPropertyRelative( objFldName );

                if( elementToAdd.Equals( targetObject.objectReferenceValue ) )
                {
                    Debug.LogError( "ERR: \"" + elementToAdd.name + "\" is already there." );
                    return false;
                }


                if( targetObject.objectReferenceValue == null )
                {
                    targetObject.objectReferenceValue = elementToAdd;
                    Debug.Log( "\"" + elementToAdd.name + "\" added in empty cell." );
                    return true;
                }
            }

            int index = 0;

            if( desiredIndex > 0 && desiredIndex < arrayProperty.arraySize )
            {
                index = ++desiredIndex;
            }

            arrayProperty.InsertArrayElementAtIndex( index );
            arrayProperty.GetArrayElementAtIndex( index ).FindPropertyRelative( objFldName ).objectReferenceValue = elementToAdd;
            arrayProperty.GetArrayElementAtIndex( index ).FindPropertyRelative( "surfaceName" ).stringValue = defaultName;
            Debug.Log( "\"" + elementToAdd.name + "\" addted as new." );

            return true;
        }



        // IsDragValid
        private static bool IsDragValid( Type targetType )
        {
            for( int i = 0; i < DragAndDrop.objectReferences.Length; i++ )
            {
                Type dragType = DragAndDrop.objectReferences[ i ].GetType();

                if( dragType == targetType || dragType.IsSubclassOf( targetType ) )
                {
                    return true;
                }

                if( IsDragFolder( DragAndDrop.paths[ i ] ) )
                {
                    return true;
                }
            }

            return false;
        }

        // GetRightPart OfPath
        static string GetRightPartOfPath( string assetPath )
        {
            const string startFolder = "Assets";

            int startIndex = assetPath.IndexOf( startFolder, 0 );
            int endIndex = assetPath.Length - startIndex;

            return assetPath.Substring( startIndex, endIndex );
        }

        // Get Drag FolderPath
        private static string GetDragFolderPath( string path )
        {
            return Application.dataPath + path.Remove( 0, path.IndexOf( "/" ) );
        }

        // IsDragFolder
        private static bool IsDragFolder( string path )
        {
            return Directory.Exists( GetDragFolderPath( path ) );
        }

        // IsDragFolder
        private static bool IsDragFolder( string path, out string folderPath )
        {
            folderPath = GetDragFolderPath( path );
            return Directory.Exists( folderPath );
        }



        // FullReset
        internal static void FullReset()
        {
            ClearUndo();

            serializedObject = null;

            searchOrFilter = false;
            filterElementsInfo = "ALL";
            searchText = string.Empty;

            leftScroll = rightScroll = Vector2.zero;
            currentTab = oldTab = 0;
            SetDirtyPreviews();
        }
    };
}