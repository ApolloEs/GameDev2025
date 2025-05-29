/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using UnityEditor;
using AdvancedShooterKit;

namespace AdvancedShooterKitEditor
{
    [CustomEditor( typeof( FirstPersonController ) )]
    public class FirstPersonControllerEditor : Editor
    {
        private SerializedProperty
            canWalkProp, walkSpeedProp, backwardsSpeedProp, sidewaysSpeedProp, inAirSpeedProp,
            canRunProp, runSpeedProp,
            canCrouchProp, crouchSpeedProp, crouchHeightProp,
            canJumpProp, jumpForceProp,
            canClimbProp, climbingSpeedProp,
            useHeadBobProp, posForceProp, tiltForceProp,
            gravityMultiplierProp, fallingDistanceToDamageProp, fallingDamageMultiplierProp,
            stepIntervalProp,
            lookSmoothProp, maxLookAngleYProp, cameraOffsetProp;


        // OnEnable
        void OnEnable()
        {
            canWalkProp = serializedObject.FindProperty( "canWalk" );
            walkSpeedProp = serializedObject.FindProperty( "walkSpeed" );
            backwardsSpeedProp = serializedObject.FindProperty( "backwardsSpeed" );
            sidewaysSpeedProp = serializedObject.FindProperty( "sidewaysSpeed" );
            inAirSpeedProp = serializedObject.FindProperty( "inAirSpeed" );

            canRunProp = serializedObject.FindProperty( "canRun" );
            runSpeedProp = serializedObject.FindProperty( "runSpeed" );

            canCrouchProp = serializedObject.FindProperty( "canCrouch" );
            crouchSpeedProp = serializedObject.FindProperty( "crouchSpeed" );
            crouchHeightProp = serializedObject.FindProperty( "crouchHeight" );

            canJumpProp = serializedObject.FindProperty( "canJump" );
            jumpForceProp = serializedObject.FindProperty( "jumpForce" );

            canClimbProp = serializedObject.FindProperty( "canClimb" );
            climbingSpeedProp = serializedObject.FindProperty( "climbingSpeed" );

            useHeadBobProp = serializedObject.FindProperty( "useHeadBob" );
            posForceProp = serializedObject.FindProperty( "posForce" );
            tiltForceProp = serializedObject.FindProperty( "tiltForce" );

            gravityMultiplierProp = serializedObject.FindProperty( "gravityMultiplier" );
            fallingDistanceToDamageProp = serializedObject.FindProperty( "fallingDistanceToDamage" );
            fallingDamageMultiplierProp = serializedObject.FindProperty( "fallingDamageMultiplier" );

            stepIntervalProp = serializedObject.FindProperty( "stepInterval" );

            lookSmoothProp = serializedObject.FindProperty( "lookSmooth" );
            maxLookAngleYProp = serializedObject.FindProperty( "maxLookAngleY" );
            cameraOffsetProp = serializedObject.FindProperty( "cameraOffset" );
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
            const float SPACE = 15f;

            ASKEditorHelper.DrawBoolField( canWalkProp );
            ASKEditorHelper.DrawPropertyField( walkSpeedProp, "Normal Speed", SPACE );
            ASKEditorHelper.DrawPropertyField( backwardsSpeedProp, SPACE );
            ASKEditorHelper.DrawPropertyField( sidewaysSpeedProp, SPACE );
            ASKEditorHelper.DrawPropertyField( inAirSpeedProp, "InAir Speed", SPACE );
            GUI.enabled = true;

            ASKEditorHelper.DrawBoolField( canRunProp );
            ASKEditorHelper.DrawPropertyField( runSpeedProp, "Move Speed", SPACE );
            GUI.enabled = true;

            ASKEditorHelper.DrawBoolField( canCrouchProp );
            ASKEditorHelper.DrawPropertyField( crouchSpeedProp, "Move Speed", SPACE );
            ASKEditorHelper.DrawPropertyField( crouchHeightProp, "Capsule Height", SPACE );
            GUI.enabled = true;

            ASKEditorHelper.DrawBoolField( canJumpProp );
            ASKEditorHelper.DrawPropertyField( jumpForceProp, "Force", SPACE );
            GUI.enabled = true;

            ASKEditorHelper.DrawBoolField( canClimbProp );
            ASKEditorHelper.DrawPropertyField( climbingSpeedProp, "Move Speed", SPACE );
            GUI.enabled = true;

            GUILayout.Space( 5f );
            ASKEditorHelper.DrawBoolField( useHeadBobProp );
            ASKEditorHelper.DrawPropertyField( posForceProp, "Pos Force", SPACE );
            ASKEditorHelper.DrawPropertyField( tiltForceProp, "Tilt Force", SPACE );
            GUI.enabled = true;

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( gravityMultiplierProp );
            EditorGUILayout.PropertyField( fallingDistanceToDamageProp );
            EditorGUILayout.PropertyField( fallingDamageMultiplierProp );            

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( stepIntervalProp );

            GUILayout.Space( 5f );
            EditorGUILayout.PropertyField( lookSmoothProp );
            EditorGUILayout.PropertyField( maxLookAngleYProp );
            EditorGUILayout.PropertyField( cameraOffsetProp );
        }
    };
}