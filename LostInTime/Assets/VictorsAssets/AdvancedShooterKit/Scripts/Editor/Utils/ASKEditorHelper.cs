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
    public static class ASKEditorHelper
    {
        // Draw PropertyField
        public static void DrawPropertyField( SerializedProperty property, string label )
        {
            DrawPropertyField( property, label, 0f );
        }
        // Draw PropertyField
        public static void DrawPropertyField( SerializedProperty property, float space )
        {
            DrawPropertyField( property, property.displayName, space );
        }
        // Draw PropertyField
        public static void DrawPropertyField( SerializedProperty property, string label, float space )
        {
            Rect rect = EditorGUILayout.GetControlRect();           
            rect.x += space;
            rect.width -= space;
            EditorGUI.PropertyField( rect, property, new GUIContent( label ) );
        }

        // Show FixedPropertyField
        public static void ShowFixedPropertyField( ref SerializedProperty property, string label, float space, float width )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( space );
            GUILayout.Label( label, GUILayout.Width( width ) );
            EditorGUILayout.PropertyField( property, GUIContent.none );
            GUILayout.EndHorizontal();
        }

        // Draw BoolField
        public static void DrawBoolField( SerializedProperty property )
        {
            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( property );
            GUI.enabled = property.boolValue;
        }

        // Show SubSlider
        public static void ShowSubSlider( ref SerializedProperty property, float minVal, float maxVal, string label, float space )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( space );
            EditorGUILayout.Slider( property, minVal, maxVal, new GUIContent( label ) );
            GUILayout.EndHorizontal();
        }

        // Show int SubSlider
        public static void ShowIntSubSlider( ref SerializedProperty property, int minVal, int maxVal, string label, float space )
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( space );
            EditorGUILayout.IntSlider( property, minVal, maxVal, new GUIContent( label ) );
            GUILayout.EndHorizontal();
        }

                
        // Show CameraShakeSlider
        public static void ShowMinMaxSlider( SerializedProperty minProperty, SerializedProperty maxProperty, float minLimit, float maxLimit, string label, bool intValues = false )
        {
            float minValue = intValues ? ( float )minProperty.intValue : minProperty.floatValue;
            float maxValue = intValues ? ( float )maxProperty.intValue : maxProperty.floatValue;

            using( ASKEditorLayout.Horizontal() )
            {
                GUILayout.Label( label );
                GUILayout.Space( 15f );
                EditorGUILayout.LabelField( minValue.ToString( intValues ? "f0" : "f1" ), GUILayout.Width( 30f ) );
                EditorGUILayout.MinMaxSlider( ref minValue, ref maxValue, minLimit, maxLimit );
                EditorGUILayout.LabelField( maxValue.ToString( intValues ? "f0" : "f1" ), GUILayout.Width( 30f ) );
            }

            if( intValues )
            {
                minProperty.intValue = Mathf.RoundToInt( minValue );
                maxProperty.intValue = Mathf.RoundToInt( maxValue );
            }
            else
            {
                minProperty.floatValue = minValue;
                maxProperty.floatValue = maxValue;
            }
        }


        // Show SFXProperty AndPlayButton
        public static void ShowSFXPropertyAndPlayButton( SerializedObject serializedObject, SerializedProperty property, string label = "" )
        {
            GUILayout.BeginHorizontal();

            if( label == "" )
                EditorGUILayout.PropertyField( property );
            else
                EditorGUILayout.PropertyField( property, new GUIContent( label ) );

            GUI.enabled = ( property.objectReferenceValue != null );
            if( GUILayout.Button( "►", GUILayout.Width( 22f ), GUILayout.Height( 15f ) ) )
            {
                try
                {
                    AudioSource tmpAudio = ( serializedObject.targetObject as Component ).transform.root.GetComponentInChildren<AudioSource>();
                    
                    if( tmpAudio == null )
                        tmpAudio = Camera.main.GetComponentInChildren<AudioSource>();

                    if( tmpAudio != null )
                        tmpAudio.PlayOneShot( property.objectReferenceValue as AudioClip );
                }
                catch
                {
                    Debug.LogError( "This prefab must be placed on scene." );
                }                
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        // Show SFXList AndPlayButton
        public static void ShowSFXListAndPlayButton( SerializedObject serializedObject, ReorderableList list, string label, float space = 0f )
        {
            GUILayout.Space( 5f );

            if( space != 0f )
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space( space );
                GUILayout.BeginVertical();
            }

            list.drawHeaderCallback = rect => EditorGUI.LabelField( rect, label );

            list.drawElementCallback = ( Rect rect, int index, bool isActive, bool isFocused ) =>
            {
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.width = EditorGUIUtility.currentViewWidth - 100f;

                SerializedProperty property = list.serializedProperty.GetArrayElementAtIndex( index );                
                EditorGUI.PropertyField( rect, property, GUIContent.none );

                rect.x += rect.width + 10f;
                rect.width = 22f;

                GUI.enabled = ( property.objectReferenceValue != null );
                if( GUI.Button( rect, "►" ) )
                {
                    try
                    {
                        AudioSource tmpAudio = ( serializedObject.targetObject as Component ).transform.root.GetComponentInChildren<AudioSource>();

                        if( tmpAudio == null )
                            tmpAudio = Camera.main.GetComponentInChildren<AudioSource>();
                        
                        if( tmpAudio != null )
                            tmpAudio.PlayOneShot( property.objectReferenceValue as AudioClip );
                    }
                    catch
                    {
                        Debug.LogError( "This prefab must be placed on scene." );
                    }
                }
                GUI.enabled = true;

            };
            list.DoLayoutList();

            if( space != 0f )
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }


                
        // Show ProgressBar
        public static void ShowProgressBar( float percent, string label )
        {
            Rect rect = GUILayoutUtility.GetRect( 18f, 18f, "TextField" );
            EditorGUI.ProgressBar( rect, percent, label );
            EditorGUILayout.Space();
        }
        
        
        // ArrayCmd struct
        public struct ArrayCmd
        {
            public enum EType : byte 
            {
                NONE, Add, Delete, MoveUp, MoveDown
            };

            public ArrayCmd( EType type, SerializedProperty newElement )
            {
                this.type = type;
                this.newElement = newElement;
            }

            public readonly EType type;
            public readonly SerializedProperty newElement;
        };

        // Draw ArrayControls
        public static ArrayCmd DrawArrayControls( SerializedProperty array, int maxElements, ref int selection )
        {
            int arraySize = array.arraySize;

            EditorGUILayout.BeginVertical( "box" );
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical( GUILayout.Width( 30f ) );
            GUILayout.Space( 4f );
            GUI.enabled = ASKWindow.NotBegin( selection );
            bool moveUp = GUILayout.Button( "▲", GUILayout.Height( 16f ) );
            GUI.enabled = true;
            GUI.enabled = ASKWindow.NotEnd( selection, arraySize );
            bool moveDown = GUILayout.Button( "▼", GUILayout.Height( 16f ) );
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            //
            GUI.enabled = ( arraySize < maxElements );
            bool add = GUILayout.Button( "Add Surface", GUILayout.Height( 35f ) );
            GUI.enabled = ( arraySize > 0 );
            bool delete = GUILayout.Button( "Remove it", GUILayout.Height( 35f ) );
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if( arraySize > 0 )
            {
                GUILayout.Space( 5f );
                selection = GUILayout.SelectionGrid( selection, ASKWindow.GetNames( array ), 4 );
                GUILayout.Space( 5f );
            }            

            EditorGUILayout.EndVertical();

            ArrayCmd.EType cmdType = ArrayCmd.EType.NONE;
            SerializedProperty newElement = null;

            if( add )
            {
                cmdType = ArrayCmd.EType.Add;

                selection = arraySize;
                array.InsertArrayElementAtIndex( selection );
                newElement = array.GetArrayElementAtIndex( selection );
            }
            else if( delete )
            {
                cmdType = ArrayCmd.EType.Delete;
                array.DeleteArrayElementAtIndex( selection );
                arraySize = array.arraySize;
                selection = ASKWindow.NotEnd( selection, arraySize ) ? selection : arraySize - 1;
            }
            else if( moveUp )
            {
                cmdType = ArrayCmd.EType.MoveUp;
                array.MoveArrayElement( selection, --selection );                
            }
            else if( moveDown )
            {
                cmdType = ArrayCmd.EType.MoveDown;
                array.MoveArrayElement( selection, ++selection );                
            }            

            return new ArrayCmd( cmdType, newElement );
        }

        
        // Draw StringPopup
        public static void DrawStringPopup( SerializedProperty property, string[] names, string label, params GUILayoutOption[] options )
        {
            int id = GetStringId( property, names );
            id = EditorGUILayout.Popup( label, id, names, options );
            property.stringValue = ( id > -1 ) ? names[ id ] : string.Empty;
        }

        // Draw StringPopup
        public static void DrawStringPopup( Rect rect, SerializedProperty property, string[] names )
        {
            int id = GetStringId( property, names );
            id = EditorGUI.Popup( rect, id, names );
            property.stringValue = ( id > -1 ) ? names[ id ] : string.Empty;
        }
        // GetStringId
        static int GetStringId( SerializedProperty property, string[] names )
        {
            string propValue = property.stringValue;
            return ArrayUtility.FindIndex( names, n => n == propValue );
        }

        
        // DrawLink
        public static void DrawLink( string label, string url, string tooltip = null )
        {
            label = label.Insert( 0, "• " );

            var style = ASKEditorStyle.Get;

            Rect btnRect = GUILayoutUtility.GetRect( GUIContent.none, GUIStyle.none );

            btnRect.width = style.link.CalcSize( new GUIContent( label ) ).x;

            if( btnRect.Contains( Event.current.mousePosition ) )
            {
                label = "<color=#0CB4CCFF>" + label + "</color>";
            }

            if( GUI.Button( btnRect, new GUIContent( label, string.IsNullOrEmpty( tooltip ) ? url : tooltip ), style.link ) )
            {
                Application.OpenURL( url );
            }

            EditorGUIUtility.AddCursorRect( btnRect, MouseCursor.Link );
        }


        // Separator
        public static void Separator()
        {
            GUILayout.Box( GUIContent.none, ASKEditorStyle.Get.line, GUILayout.ExpandWidth( true ), GUILayout.Height( 1f ) );
        }


        // LargeFoldout
        public static void LargeFoldout( SerializedProperty foldoutProp, string label, Action showMethod )
        {
            GUILayout.BeginVertical( "box", GUILayout.ExpandWidth( true ) );

            const float SPACE = 15f;
            Rect foldoutRect = EditorGUILayout.GetControlRect();
            foldoutRect.x += SPACE;
            foldoutRect.width -= SPACE;

            bool foldout = foldoutProp.isExpanded;
            foldout = EditorGUI.Foldout( foldoutRect, foldout, label.Insert( 0, " " ), true, ASKEditorStyle.Get.largeFoldout );
            foldoutProp.isExpanded = foldout;

            if( foldout )
            {
                GUILayout.Space( 5f );
                showMethod.Invoke();
                GUILayout.Space( 5f );
            }
            else
            {
                GUILayout.Space( 2f );
            }

            GUILayout.EndVertical();
        }

        // ToggleFoldout
        public static bool ToggleFoldout( SerializedProperty property, string label, bool bold = true )
        {
            GUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 2f;
            EditorGUILayout.PropertyField( property, GUIContent.none, GUILayout.ExpandWidth( false ) );
            EditorGUIUtility.labelWidth = 0f;

            GUILayout.Space( 10f );
            property.isExpanded = EditorGUI.Foldout( EditorGUILayout.GetControlRect(), property.isExpanded, label, true, bold ? ASKEditorStyle.Get.richBoldFoldout : EditorStyles.foldout );
            GUILayout.EndHorizontal();

            return property.isExpanded;
        }


        // DoRemove ElementFromList
        public static void DoRemoveElementFromList( this ReorderableList list, int index )
        {
            list.DoRemoveElementFromList( index, () => { } );
        }

        // DoRemove ElementFromList
        public static void DoRemoveElementFromList( this ReorderableList list, int index, Action OnRemove )
        {
            if( index < 0 )
            {
                return;
            }

            SerializedProperty scenesArray = list.serializedProperty;
            scenesArray.DeleteArrayElementAtIndex( index );

            if( list.index >= scenesArray.arraySize )
            {
                list.index = scenesArray.arraySize - 1;
            }

            OnRemove.Invoke();
        }



        // Draw StaticButton
        public static bool DrawStaticButton( GUIContent content, GUIStyle style, out Rect rect, params GUILayoutOption[] options )
        {
            rect = GUILayoutUtility.GetRect( content, style, options );
            Event ev = Event.current;

            if( ev.type == EventType.Repaint )
            {
                style.Draw( rect, content, false, false, false, false );
            }

            if( ev.type == EventType.MouseDown && rect.Contains( ev.mousePosition ) )
            {
                ev.Use();
                return true;
            }

            return false;
        }


        // DrawEnum AsToolbar
        public static void DrawEnumAsToolbar( SerializedProperty property, bool withLabel = true )
        {
            DrawEnumAsToolbar( EditorGUILayout.GetControlRect(), property, withLabel );
        }
        // DrawEnum AsToolbar
        public static void DrawEnumAsToolbar( Rect position, SerializedProperty property, bool withLabel = true )
        {
            if( withLabel ) {
                position = EditorGUI.PrefixLabel( position, new GUIContent( property.displayName, property.tooltip ) );
            }            

            property.enumValueIndex = GUI.Toolbar( position, property.enumValueIndex, property.enumDisplayNames, EditorStyles.miniButton );
        }


        // DrawBool AsButton
        public static void DrawBoolAsButton( SerializedProperty property )
        {
            Rect btnRect = EditorGUILayout.GetControlRect();

            bool boolValue = property.boolValue;
            boolValue = EditorGUI.Toggle( btnRect, boolValue, EditorStyles.toolbarButton );

            GUIStyle labelStyle = new GUIStyle( GUI.skin.label );
            labelStyle.fontStyle = boolValue ? FontStyle.Bold : FontStyle.Normal;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUI.LabelField( btnRect, property.displayName + ( boolValue ? " ON" : " OFF" ), labelStyle );

            property.boolValue = boolValue;
        }
    };
}