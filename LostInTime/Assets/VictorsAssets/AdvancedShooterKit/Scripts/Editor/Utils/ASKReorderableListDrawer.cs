/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AdvancedShooterKitEditor
{
    public sealed class ASKReorderableListDrawer : IDisposable
    {
        public delegate Rect ElementCallbackDelegate( Rect rect, int index, bool isActive, bool isFocused );        
        public enum ERemoveBtnMode { Bottom, Middle, OFF }

        public readonly ReorderableList list;
        public ElementCallbackDelegate OnDrawElement;

        public bool displayAdd = true;

        public ERemoveBtnMode removeBtnMode = ERemoveBtnMode.Middle;

        public float elementHeight = 21f;
        public float topSpace = 5f, bottomSpace = 5f
            , offset = 0f;

        public int  rows = 1, minItems;


        public float singleLineHeight { get { return EditorGUIUtility.singleLineHeight; } }
        public SerializedProperty getArray { get { return list.serializedProperty; } }


        readonly string m_Label;

        readonly Action m_OnDirty = () => { }
                      , m_OnRemove = () => { };        

        bool m_DrawBox, m_DrawFoldout = true
            , m_RemoveBtnIsMiddle;

        bool m_IsExpanded { get { return getArray.isExpanded; } set { getArray.isExpanded = value; } }


        static bool m_IsDragging;

        static readonly ReorderableList emptyList = new ReorderableList( new object[] { }, typeof( object ), false, true, false, false );


        // Constructor
        public ASKReorderableListDrawer( ReorderableList list, string label, Action OnDirty, Action OnRemove )
        {
            this.list = list;
            m_OnDirty = OnDirty;
            m_OnRemove = OnRemove;
            m_Label = label;
        }
        // Constructor
        public ASKReorderableListDrawer( ReorderableList list, string label, Action OnDirty )
        {
            this.list = list;
            m_OnDirty = OnDirty;
            m_Label = label;
        }
        // Constructor
        public ASKReorderableListDrawer( ReorderableList list, string label )
        {
            this.list = list;
            m_Label = label;
        }        
        

        // GetArrayElement AtIndex
        public SerializedProperty GetArrayElementAtIndex( int index )
        {
            return list.serializedProperty.GetArrayElementAtIndex( index );
        }

        // DrawBox
        public void DrawBox()
        {
            m_DrawBox = true;
        }

        // HideFoldout
        public void HideFoldout()
        {
            m_DrawFoldout = false;
            m_IsExpanded = true;
        }


        // Init
        void Init()
        {
            if( list.count <= minItems )
            {
                removeBtnMode = ERemoveBtnMode.OFF;
                HideFoldout();
            }

            if( m_IsExpanded == false )
            {                
                return;
            }

            if( list.count > 0 )
            {
                if( rows > 1 ) {
                    elementHeight *= rows;
                }

                if( m_DrawBox ) {
                    elementHeight += 7f;
                }
            }
            else
            {
                elementHeight = singleLineHeight;
            }

            m_RemoveBtnIsMiddle = ( removeBtnMode == ERemoveBtnMode.Middle );

            list.displayAdd = displayAdd;
            list.displayRemove = ( removeBtnMode == ERemoveBtnMode.Bottom );
            list.elementHeight = elementHeight;

            list.drawHeaderCallback = DrawHeader;
        }

                
        // Dispose
        void IDisposable.Dispose()
        {
            DoDraw();
        }


        // DrawEmptyList
        private void DrawEmptyList()
        {
            emptyList.drawHeaderCallback = DrawHeader;
            emptyList.footerHeight = emptyList.elementHeight = 0f;
            DoLayoutList( emptyList, () => { }, topSpace, bottomSpace, offset );
        }


        // DrawHeader
        private void DrawHeader( Rect rect )
        {
            if( m_DrawFoldout )
            {
                rect.x += 10f;
                m_IsExpanded = EditorGUI.Foldout( rect, m_IsExpanded, m_Label.Insert( 0, " " ), true );
            }
            else
            {
                EditorGUI.LabelField( rect, m_Label );
            }
        }


        // Do Draw
        public void DoDraw()
        {
            Init();

            if( m_IsExpanded == false )
            {
                DrawEmptyList();
                return;
            }

            CheckDrawDelegate();

            int removeIndex = -1;

            list.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
            {
                rect = DrawTop( rect );
                rect = OnDrawElement.Invoke( rect, index, isActive, isFocused );

                if( m_RemoveBtnIsMiddle && removeIndex < 0 ) {
                    removeIndex = DrawBottom( rect, index );
                } 
            };

            DoLayoutList( list, m_OnDirty, topSpace, bottomSpace, offset );

            if( m_RemoveBtnIsMiddle ) {
                list.DoRemoveElementFromList( removeIndex, m_OnRemove );
            }            
        }


        // Check DrawDelegate
        private void CheckDrawDelegate()
        {
            if( OnDrawElement != null )
            {
                return;
            }

            OnDrawElement = ( Rect rect, int index, bool isActive, bool isFocused ) =>
            {
                SerializedProperty property = list.serializedProperty.GetArrayElementAtIndex( index );
                EditorGUI.PropertyField( rect, property, GUIContent.none );
                return rect;
            };
        }


        // DrawUp
        private Rect DrawTop( Rect rect )
        {
            rect.y += 2f;

            if( m_DrawBox )
            {
                float startWidth = rect.width;                
                float startX = rect.x;
                rect.x = startX - 15f;
                rect.width = startWidth + 15f;
                rect.height = list.elementHeight - 4f;
                EditorGUI.HelpBox( rect, string.Empty, MessageType.None );
                rect.x = startX;
                rect.y += 4f;
                rect.width = startWidth - 5f;
            }

            rect.height = singleLineHeight;
            rect.width = ( m_RemoveBtnIsMiddle ? rect.width - 25f : rect.width );

            return rect;
        }

        // DrawBottom
        private int DrawBottom( Rect rect, int index )
        {
            rect.x = rect.xMax + 7f;
            rect.width = rect.height;

            var style = ASKEditorStyle.Get;
            if( GUI.Button( rect, style.iconToolbarMinus, style.rlFooterButton ) )
            {
                return index;
            }

            return -1;
        }


        // Do LayoutList
        public static void DoLayoutList( ReorderableList list, Action OnDirty, float topSpace = 5f, float bottomSpace = 5f, float offset = 0f )
        {
            using( new ASKEditorChangeCheck( OnDirty ) )
            {
                Event ev = Event.current;

                if( ev.type == EventType.MouseDrag )
                {
                    m_IsDragging = true;
                }

                if( m_IsDragging && ev.type == EventType.MouseUp )
                {
                    m_IsDragging = false;
                    OnDirty.Invoke();
                }

                GUILayout.Space( topSpace );

                Rect rect = EditorGUILayout.GetControlRect( GUILayout.Height( list.GetHeight() ) );
                rect.x += offset;
                rect.width -= offset;
                list.DoList( rect );

                //list.DoLayoutList();

                GUILayout.Space( bottomSpace );

                if( ev.type == EventType.MouseUp )
                {
                    m_IsDragging = false;
                }
            }
        }
    };
}
