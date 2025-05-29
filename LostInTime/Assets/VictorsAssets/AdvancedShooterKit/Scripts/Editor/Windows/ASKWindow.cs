/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AdvancedShooterKitEditor
{
    public sealed class ASKWindow : EditorWindow
    {
        const string TITLE = "ShooterKit";
        const string LOGO_NAME = "ASKLogoIcon";

        public const string MENU_ITEM_PATH = "Tools/Victor's Assets/Advanced Shooter Kit/";

        static ASKWindow window;

        static readonly Dictionary<string, Texture2D> m_Images = new Dictionary<string, Texture2D>();

        public static string editorDirectory { get; private set; }

        public static string imagesPath { get { return editorDirectory + "/Images/"; } }
        public static string iconsPath { get { return imagesPath + "Icons/"; } }


        internal static string mainDirectory { get; private set; }
        internal static float width { get; private set; }


        static bool dirty, needReinit, needSave;

        static GUIContent[] subWindowTabs;
        int m_SelectedTab;

                
        // GetImage
        public static Texture2D GetImage( string imgPath )
        {
            Texture2D image;

            if( m_Images.TryGetValue( imgPath, out image ) == false )
            {
                image = AssetDatabase.LoadAssetAtPath<Texture2D>( imgPath + ".png" );

                if( image != null ) {
                    m_Images.Add( imgPath, image );
                }
            }

            return image;
        }


        // On ScriptsRecompiled
        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsRecompiled()
        {
            needReinit = true;
        }


        // OnInspectorUpdate
        void OnInspectorUpdate()
        {
            if( needReinit )
            {
                needReinit = false;
                Init();
            }

            Repaint();
        }


        // SetDirty Data
        public static void SetDirtyData()
        {
            needSave = true;
        }

        // Clear DirtyData
        static void ClearDirtyData()
        {
            needSave = false;
        }


        // SetDirty Data
        public static void MarkDirty()
        {
            dirty = true;
        }


        // OnDestroy
        void OnDestroy()
        {
            if( needSave )
            {
                int closeId = EditorUtility.DisplayDialogComplex( "Save changes", "Warning: You have not saved changes! Save?", "Save", "No", "Cancel" );

                if( closeId == 0 )
                {
                    SaveSettings();
                }
                else if( closeId == 2 )
                {
                    window = CreateInstance<ASKWindow>();
                    Init();
                    return;
                }

                ClearDirtyData();
            }
            
            FullReset();
            AssetDatabase.DeleteAsset( mainDirectory + "/tmp" );
        }


        // Show Settings
        [MenuItem( MENU_ITEM_PATH + "Settings", false, 101 )]
        public static void ShowSettings()
        {
            Init( 0 );
        }

        // Show About
        [MenuItem( MENU_ITEM_PATH + "About", false, 151 )]
        static void ShowAbout()
        {
            Init( 3 );
        }

        // Init
        static void Init()
        {
            Init( -1 );
        }
        // Init  
        static void Init( int tabIndex )
        {
            window = GetWindow<ASKWindow>();
            window.minSize = new Vector2( 725f, 535f );
            window.Focus();

            if( tabIndex > -1 ) {
                window.m_SelectedTab = tabIndex;
            }

            SetupIt();

            window.titleContent = new GUIContent( TITLE, GetImage( imagesPath + LOGO_NAME ) );
            
            subWindowTabs = new[]
            {
                new GUIContent( "Surfaces" )
                , new GUIContent( "Input" )
                , new GUIContent( "Game" )
                , new GUIContent( "About" )
            };

            string imgPrefix = iconsPath + "ASK";

            for( int i = 0; i < subWindowTabs.Length; i++ )
            {
                subWindowTabs[ i ].image = GetImage( imgPrefix + subWindowTabs[ i ].text );
            }
        }

        // SetupIt
        static void SetupIt()
        {
            var monoScript = MonoScript.FromScriptableObject( window );

            mainDirectory = GetResourcesPath( monoScript );
            editorDirectory = GetEditorPath( monoScript );

            SurfaceDetectorTab.SetupTab();
            InputSettingsTab.SetupTab();
            GameSettingsTab.SetupTab();
        }


        // OnGUI
        void OnGUI()
        {
            if( dirty )
            {
                dirty = false;
                Repaint();
            }

            width = position.width;

            GUILayout.Space( 10f );

            bool save, load;
            using( ASKEditorLayout.Horizontal() )
            {
                m_SelectedTab = GUILayout.Toolbar( m_SelectedTab, subWindowTabs, GUILayout.Width( 320f ), GUILayout.Height( 24f ) );
                
                GUILayout.FlexibleSpace();

                GUI.enabled = needSave;
                Vector2 btnSize = new Vector2( 110f, 24f );
                load = GUILayout.Button( new GUIContent( "Reset", GetImage( iconsPath + "ASKReset" ) ), ASKEditorStyle.Get.buttonLeft, GUILayout.Width( btnSize.x ), GUILayout.Height( btnSize.y ) );
                save = GUILayout.Button( new GUIContent( "Save", GetImage( iconsPath + "ASKSave" ) ), ASKEditorStyle.Get.buttonRight, GUILayout.Width( btnSize.x ), GUILayout.Height( btnSize.y ) );
                GUI.enabled = true;

                GUILayout.Space( 10f );
            }

            GUILayout.Space( 5f );

            using( ASKEditorLayout.Horizontal() )
            {
                switch( m_SelectedTab )
                {
                    case 0:
                        SurfaceDetectorTab.OnWindowGUI();
                        break;
                    case 1:
                        InputSettingsTab.OnWindowGUI();
                        break;
                    case 2:
                        GameSettingsTab.OnWindowGUI();
                        break;
                    case 3:
                        ASKAboutTab.OnWindowGUI();
                        break;

                    default:
                        break;
                }
            }        

            if( save )
            {
                SaveSettings();
            }

            if( load && EditorUtility.DisplayDialog( "Warning!", "Warning: All changes will be reset! Сontinue?", "Yes", "No" ) )
            {
                ReloadSettings();
            }
        }


        // ReloadSettings
        static void ReloadSettings()
        {
            ClearDirtyData();

            /*FullReset();
            SetupIt();*/

            SurfaceDetectorTab.ReloadSettings();
            InputSettingsTab.ReloadSettings();
            GameSettingsTab.ReloadSettings();            
        }

        
        // Save Settings
        static void SaveSettings()
        {
            SurfaceDetectorTab.SaveSettings();
            InputSettingsTab.SaveSettings();
            GameSettingsTab.SaveSettings();
            ClearDirtyData();
        }

        
        // Get Names
        internal static string[] GetNames( SerializedProperty array )
        {
            int arraySize = array.arraySize;
            string[] names = new string[ arraySize ];

            for( int i = 0; i < arraySize; i++ )
            {
                names[ i ] = array.GetArrayElementAtIndex( i ).FindPropertyRelative( "name" ).stringValue;
            }                

            return names;
        }

        
        // Not Begin
        internal static bool NotBegin( int index )
        {
            return ( index - 1 >= 0 );
        }
        // Not End
        internal static bool NotEnd( int index, int size )
        {
            return ( index + 1 < size );
        }


        // Get ResourcesPath
        private static string GetResourcesPath( MonoScript monoScript )
        {
            string assetPath = AssetDatabase.GetAssetPath( monoScript );
            const string startFolder = "Assets";
            const string endFolder = "/Scripts";
            const string resFolder = "Resources";

            if( assetPath.Contains( startFolder ) && assetPath.Contains( endFolder ) )
            {
                int startIndex = assetPath.IndexOf( startFolder, 0 ) + startFolder.Length;
                int endIndex = assetPath.IndexOf( endFolder, startIndex );

                string between = assetPath.Substring( startIndex, endIndex - startIndex );
                string projectFolder = startFolder + between;
                string resPath = projectFolder + "/" + resFolder;

                bool refresh = false;

                if( AssetDatabase.IsValidFolder( resPath ) == false )
                {
                    AssetDatabase.CreateFolder( projectFolder, resFolder );
                    refresh = true;
                }

                if( AssetDatabase.IsValidFolder( resPath + "/tmp" ) == false )
                {
                    AssetDatabase.CreateFolder( resPath, "tmp" );
                    refresh = true;
                }

                if( refresh )
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                return resPath;
            }

            return string.Empty;
        }

        // Get EditorPath
        private static string GetEditorPath( MonoScript monoScript )
        {
            string assetPath = AssetDatabase.GetAssetPath( monoScript );
            const string endFolder = "/Editor";

            if( assetPath.Contains( endFolder ) )
            {
                int endIndex = assetPath.IndexOf( endFolder, 0 );
                string between = assetPath.Substring( 0, endIndex );
                return between + endFolder;
            }

            return string.Empty;
        }


        // Full Reset
        static void FullReset()
        {
            SurfaceDetectorTab.FullReset();
            InputSettingsTab.FullReset();
            GameSettingsTab.FullReset();
        }
    };
}