/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{
    [CustomEditor( typeof( FirstPersonWeaponSway ) )]
    public class FirstPersonWeaponSwayEditor : Editor
    {
        // Sway
        private SerializedProperty 
            useSwayProp, swaySmoothingProp, borderSizeProp, fpMoveSpeedProp, runOffsetProp, outWallOffsetProp;

        // Animation
        private SerializedProperty
            dropoutSmoothingProp, dropoutRotationProp;       

        // Ironsighting 
        private SerializedProperty 
            useIronsightingProp,
            ironsightSmoothingProp, ironsightDispersionProp,
            addMoveProp, addLookProp, addRunAndInAirProp,
            crouchedProp, zoomFOVProp, ironsightMoveSpeedProp, zoomPosProp;


        // Crosshair
        private SerializedProperty crosshairView;

        // Pivot
        private SerializedProperty pivotPositionProp;


        readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle( typeof( FirstPersonWeaponSwayEditor ).Name.GetHashCode() );
        
        FirstPersonWeaponSway m_Target;
        Transform m_CameraTransform;

        static string nicifyName;
        

        // OnEnable
        void OnEnable()
        {
            useSwayProp = serializedObject.FindProperty( "useSway" );
            swaySmoothingProp = serializedObject.FindProperty( "swaySmoothing" );
            borderSizeProp = serializedObject.FindProperty( "borderSize" );
            fpMoveSpeedProp = serializedObject.FindProperty( "fpMoveSpeed" );
            runOffsetProp = serializedObject.FindProperty( "runOffset" );
            outWallOffsetProp = serializedObject.FindProperty( "outWallOffset" );

            dropoutSmoothingProp = serializedObject.FindProperty( "dropoutSmoothing" );
            dropoutRotationProp = serializedObject.FindProperty( "dropoutRotation" );            

            useIronsightingProp = serializedObject.FindProperty( "useIronsighting" );
            ironsightSmoothingProp = serializedObject.FindProperty( "ironsightSmoothing" );
            ironsightDispersionProp = serializedObject.FindProperty( "ironsightDispersion" );
            addMoveProp = serializedObject.FindProperty( "addMove" );
            addLookProp = serializedObject.FindProperty( "addLook" );
            addRunAndInAirProp = serializedObject.FindProperty( "addRunAndInAir" );
            crouchedProp = serializedObject.FindProperty( "crouched" );
            zoomFOVProp = serializedObject.FindProperty( "zoomFOV" );
            ironsightMoveSpeedProp = serializedObject.FindProperty( "ironsightMoveSpeed" );
            zoomPosProp = serializedObject.FindProperty( "zoomPos" );

            crosshairView = serializedObject.FindProperty( "crosshairView" );
            pivotPositionProp = serializedObject.FindProperty( "pivotPosition" );

            m_Target = target as FirstPersonWeaponSway;
            m_CameraTransform = m_Target.GetComponentInParent<Camera>().transform;

            nicifyName = ObjectNames.NicifyVariableName( m_Target.GetType().Name );
        }


        // OnInspectorGUI
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShowParameters();
            serializedObject.ApplyModifiedProperties();
        }

        // OnSceneGUI
        void OnSceneGUI()
        {
            using( new Handles.DrawingScope( Color.yellow, m_CameraTransform.localToWorldMatrix ) )
            {
                FromBoundsToHandle();

                using( var ecc = new ASKEditorChangeCheck() )
                {
                    m_BoundsHandle.center = Handles.PositionHandle( m_BoundsHandle.center, Quaternion.identity );
                    m_BoundsHandle.DrawHandle();

                    ecc.OnChangeCheck = () =>
                    {
                        Undo.RecordObject( m_Target, string.Format( "Modify {0}", nicifyName ) );
                        FromHandleToBounds();
                    };
                }
            };

            using( new Handles.DrawingScope( m_CameraTransform.localToWorldMatrix ) )
            {
                using( var ecc = new ASKEditorChangeCheck() )
                {
                    Vector3 pos = m_Target.pivotPosition;
                    pos = Handles.PositionHandle( pos, Quaternion.identity );

                    ecc.OnChangeCheck = () =>
                    {
                        Undo.RecordObject( m_Target, string.Format( "Modify Pivot On {0}", nicifyName ) );
                        m_Target.pivotPosition = pos;
                    };
                }
            };
        }

        // From Bounds ToHandle
        void FromBoundsToHandle()
        {
            m_BoundsHandle.center = Handles.inverseMatrix * m_CameraTransform.localToWorldMatrix * m_Target.bounds.center;            
            m_BoundsHandle.size = Vector3.Scale( m_Target.bounds.size, m_CameraTransform.lossyScale );
        }

        // From Handle ToBounds
        void FromHandleToBounds()
        {
            // Invert ScaleVector
            System.Func<Vector3, Vector3> InvertScaleVector = vec =>
            {
                for( int i = 0; i < 3; i++ ) {
                    vec[ i ] = vec[ i ] != 0f ? 1f / vec[ i ] : 0f;
                }

                return vec;
            };

            // Copy HandleProperties
            m_Target.bounds.center = Handles.matrix * m_CameraTransform.worldToLocalMatrix * m_BoundsHandle.center;
            Vector3 size = Vector3.Scale( m_BoundsHandle.size, InvertScaleVector( m_CameraTransform.lossyScale ) );
            m_Target.bounds.size = new Vector3( Mathf.Abs( size.x ), Mathf.Abs( size.y ), Mathf.Abs( size.z ) );
        }


        // ShowParameters
        private void ShowParameters()
        {
            const float SPACE = 15f;

            GUILayout.Space( 5f );

            using( ASKEditorLayout.Vertical( "box" ) )
            {
                GUILayout.Space( 5f );
                if( ASKEditorHelper.ToggleFoldout( useSwayProp, "Sway" ) )
                {
                    GUI.enabled = useSwayProp.boolValue;
                    ASKEditorHelper.ShowSubSlider( ref swaySmoothingProp, .1f, 5f, "Smoothing", SPACE );
                    ASKEditorHelper.ShowSubSlider( ref borderSizeProp, 1f, 35f, "Border Size", SPACE );
                    ASKEditorHelper.ShowSubSlider( ref fpMoveSpeedProp, 0f, 1f, "Move Speed", SPACE );
                    ASKEditorHelper.DrawPropertyField( runOffsetProp, SPACE );
                    ASKEditorHelper.DrawPropertyField( outWallOffsetProp, SPACE );
                    GUI.enabled = true;
                }
                
                GUILayout.Space( 5f );
            }            

            using( ASKEditorLayout.Vertical( "box" ) )
            {
                GUILayout.Space( 5f );
                if( ASKEditorHelper.ToggleFoldout( useIronsightingProp, "Ironsighting" ) )
                {
                    GUI.enabled = useIronsightingProp.boolValue;
                    const float DSPACE = SPACE * 2f;
                    ASKEditorHelper.ShowSubSlider( ref ironsightSmoothingProp, 1f, 10f, "Smoothing", SPACE );
                    ASKEditorHelper.ShowSubSlider( ref ironsightDispersionProp, .1f, 1f, "Dispersion", SPACE );
                    ASKEditorHelper.ShowSubSlider( ref addMoveProp, 0f, 2f, "+ Move", DSPACE );
                    ASKEditorHelper.ShowSubSlider( ref addLookProp, 0f, 2f, "+ Look", DSPACE );
                    ASKEditorHelper.ShowSubSlider( ref addRunAndInAirProp, 0f, 3f, "+ Run & InAir", DSPACE );
                    ASKEditorHelper.ShowSubSlider( ref crouchedProp, .1f, 1f, "х Crouched", DSPACE );

                    GUILayout.Space( 2f );
                    ASKEditorHelper.ShowSubSlider( ref zoomFOVProp, 10f, 50f, "Cameras FOV", SPACE );
                    ASKEditorHelper.ShowSubSlider( ref ironsightMoveSpeedProp, .1f, 1f, "Move Speed", SPACE );
                    ASKEditorHelper.DrawPropertyField( zoomPosProp, "Parent Position", SPACE );
                    GUI.enabled = true;
                }
                
                GUILayout.Space( 5f );
            }

            using( ASKEditorLayout.Vertical( "box" ) )
            {
                GUILayout.Space( 5f );
                EditorGUILayout.PropertyField( dropoutRotationProp );
                EditorGUILayout.PropertyField( dropoutSmoothingProp );

                GUILayout.Space( 5f );
                ASKEditorHelper.DrawEnumAsToolbar( crosshairView );

                GUILayout.Space( 5f );
                EditorGUILayout.PropertyField( pivotPositionProp );
                GUILayout.Space( 5f );
            }

            GUILayout.Space( 5f );
        }
    };
}