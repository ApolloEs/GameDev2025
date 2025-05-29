/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEditor;

namespace AdvancedShooterKitEditor
{
    public static class ASKAboutTab
    {
        const string PABLISHER_URL = "http://u3d.as/5Fb";
        const string SUPPORT_URL = "http://bit.ly/vk-SupportNew";

        const string VERSION = "1.6.5";
        const string LOGO_NAME = "ASKLogoBig";

        const string
             MANUAL_URL = "https://goo.gl/EoF1gC"
            , FORUM_URL = "http://forum.unity.com/threads/212234"
            , CHANGELOG_URL = "http://smart-assets.org/index/0-10"
            , ASSET_URL = "http://u3d.as/bkq";


        static Texture2D m_Logo;
        private static Texture2D logo
        {
            get
            {
                if( m_Logo == null ) {
                    m_Logo = ASKWindow.GetImage( ASKWindow.imagesPath + LOGO_NAME );
                }

                return m_Logo;
            }
        }


        // OnWindowGUI
        public static void OnWindowGUI()
        {
            var style = ASKEditorStyle.Get;

            // LINK's
            using( ASKEditorLayout.Vertical( "box", GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) ) )
            {
                GUILayout.Space( 5f );

                using( ASKEditorLayout.Vertical( style.area ) )
                {
                    GUILayout.Label( "Documentation", style.headLabel );

                    GUILayout.BeginVertical( style.area );
                    ASKEditorHelper.DrawLink( "Online Manual", MANUAL_URL );
                    GUILayout.EndVertical();
                }

                using( ASKEditorLayout.Vertical( style.area ) )
                {
                    GUILayout.Label( "Support, News, More Assets", style.headLabel );

                    GUILayout.BeginVertical( style.area );
                    ASKEditorHelper.DrawLink( "Support", SUPPORT_URL );
                    GUILayout.Space( 10f );
                    ASKEditorHelper.DrawLink( "Forum", FORUM_URL );
                    GUILayout.Space( 25f );
                    ASKEditorHelper.DrawLink( "More Assets", PABLISHER_URL );
                    GUILayout.Space( 15f );
                    /*ASKEditorHelper.DrawLink( "Get \"Save Game Kit\"", "http://u3d.as/Z6E" );
                    GUILayout.Space( 10f );*/
                    ASKEditorHelper.DrawLink( "Get \"Touch Controls Kit\"", "http://u3d.as/5NP" );
                    GUILayout.EndVertical();
                }

                using( ASKEditorLayout.Vertical( style.area ) )
                {
                    GUILayout.Label( "Release Notes", style.headLabel );

                    GUILayout.BeginVertical( style.area );
                    ASKEditorHelper.DrawLink( "Full Changelog", CHANGELOG_URL );
                    GUILayout.EndVertical();
                }
            }

            // LOGO
            using( ASKEditorLayout.Vertical( "box", GUILayout.Width( 280f ), GUILayout.ExpandHeight( true ) ) )
            {
                GUILayout.Space( 5f );


                GUILayout.Label( "<size=18>Advanced Shooter Kit</size>", style.centeredLabel );

                GUILayout.Space( 5f );
                GUILayout.Label( "<size=16> Developed by Victor Klepikov\n" +
                                 "Version <b>" + VERSION + "</b> </size>", style.centeredLabel );

                EditorGUILayout.Space();
                ASKEditorHelper.Separator();

                if( logo != null )
                {
                    GUILayout.FlexibleSpace();

                    using( ASKEditorLayout.Horizontal() )
                    {
                        GUILayout.FlexibleSpace();

                        Rect logoRect = EditorGUILayout.GetControlRect( GUILayout.Width( logo.width ), GUILayout.Height( logo.height ) );

                        if( GUI.Button( logoRect, new GUIContent( logo, "Open AssetStore Page" ), EditorStyles.label ) )
                        {
                            Application.OpenURL( ASSET_URL );
                        }

                        EditorGUIUtility.AddCursorRect( logoRect, MouseCursor.Link );

                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.FlexibleSpace();
                }
                else
                {
                    GUILayout.Label( "<size=15>Logo not found</size> \n" + LOGO_NAME, style.centeredLabel );
                }
            }
        }
    };
}
