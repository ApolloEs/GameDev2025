/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;

namespace AdvancedShooterKitEditor
{
    public struct ASKEditorLayout : IDisposable
    {
        enum ELayoutMode : byte
        {
            Horizontal,
            Vertical,
            ScrollView
        }

        readonly ELayoutMode m_LayoutMode;


        // Constructor
        private ASKEditorLayout( ELayoutMode mode, GUIStyle style, params GUILayoutOption[] options )
        {
            m_LayoutMode = mode;

            switch( mode )
            {
                case ELayoutMode.Horizontal:
                    GUILayout.BeginHorizontal( style, options );
                    break;
                case ELayoutMode.Vertical:
                    GUILayout.BeginVertical( style, options );
                    break;

                default: break;
            }
        }
        // Constructor
        private ASKEditorLayout( ref Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options )
        {
            m_LayoutMode = ELayoutMode.ScrollView;
            scrollPosition = GUILayout.BeginScrollView( scrollPosition, style, options );
        }


        // Horizontal
        public static ASKEditorLayout Horizontal( params GUILayoutOption[] options )
        {
            return Horizontal( GUIStyle.none, options );
        }
        // Horizontal
        public static ASKEditorLayout Horizontal( GUIStyle style, params GUILayoutOption[] options )
        {
            return new ASKEditorLayout( ELayoutMode.Horizontal, style, options );
        }

        // Vertical
        public static ASKEditorLayout Vertical( params GUILayoutOption[] options )
        {
            return Vertical( GUIStyle.none, options );
        }
        // Vertical
        public static ASKEditorLayout Vertical( GUIStyle style, params GUILayoutOption[] options )
        {
            return new ASKEditorLayout( ELayoutMode.Vertical, style, options );
        }

        // ScrollView
        public static ASKEditorLayout ScrollView( ref Vector2 scrollPosition, params GUILayoutOption[] options )
        {
            return ScrollView( ref scrollPosition, GUIStyle.none, options );
        }
        // ScrollView
        public static ASKEditorLayout ScrollView( ref Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options )
        {
            return new ASKEditorLayout( ref scrollPosition, style, options );
        }


        // Dispose
        void IDisposable.Dispose()
        {
            switch( m_LayoutMode )
            {
                case ELayoutMode.Horizontal:
                    GUILayout.EndHorizontal();
                    break;
                case ELayoutMode.Vertical:
                    GUILayout.EndVertical();
                    break;
                case ELayoutMode.ScrollView:
                    GUILayout.EndScrollView();
                    break;

                default: break;
            }
        }
    };
}
