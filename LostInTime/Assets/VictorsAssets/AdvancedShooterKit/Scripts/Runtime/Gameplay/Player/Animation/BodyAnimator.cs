/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    using Utils;

    [RequireComponent( typeof( Animator ) )]
    public class BodyAnimator : MonoBehaviour
    {
        public bool
            isMoving
            , isMovedForward
            , isCrouched
            , isClimbing
            , isFalling;

        public float 
            normalizedSpeed
            , radians
            , floorDistance
            , turn;
        

        Transform m_Root;
        Animator m_Animator;
        FirstPersonController m_Controller;

        float bodyYaw, prevBodyYaw;

        // bools
        int m_IsMovingHash, m_IsCrouchedHash, m_IsClimbingHash, m_IsFallingHash;
        // floats
        int m_RadiansHash, m_NormalizedSpeedHash, m_FloorDistanceHash, m_TurnHash;


#if UNITY_EDITOR
        [HideInInspector]
        public bool boolFoldout, floatFoldout;
#endif


        // Start
        void Start()
        {
            m_Root = transform.root;
            m_Controller = m_Root.GetComponent<FirstPersonController>();

            InitAnimator();
            InitHashIDs();
        }

        // LateUpdate
        void LateUpdate()
        {
            UpdateAnimationValues();
            UpdateRadiansAndSpeed();

            UpdateAnimator();
        }


        // Init Animator
        private void InitAnimator()
        {
            m_Animator = GetComponent<Animator>();

            m_Animator.applyRootMotion = false;
            m_Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            m_Animator.updateMode = AnimatorUpdateMode.Normal;

            m_Animator.GetBoneTransform( HumanBodyBones.RightUpperArm ).localScale = Vector3.zero;
            m_Animator.GetBoneTransform( HumanBodyBones.LeftUpperArm ).localScale = Vector3.zero;
        }

        // Init HashIDs
        private void InitHashIDs()
        {
            // bools
            m_IsMovingHash = Animator.StringToHash( "IsMoving" );
            m_IsCrouchedHash = Animator.StringToHash( "IsCrouched" );
            m_IsClimbingHash = Animator.StringToHash( "IsClimbing" );
            m_IsFallingHash = Animator.StringToHash( "IsFalling" );            

            // floats
            m_TurnHash = Animator.StringToHash( "Turn" );
            m_RadiansHash = Animator.StringToHash( "Radians" );
            m_NormalizedSpeedHash = Animator.StringToHash( "NormalizedSpeed" );
            m_FloorDistanceHash = Animator.StringToHash( "FloorDistance" );            
        }        


        // Update AnimationData
        private void UpdateAnimationValues()
        {
            isMoving = m_Controller.isMoving;
            isMovedForward = m_Controller.isMoveForward;
            
            isCrouched = m_Controller.isCrouched;
            isClimbing = m_Controller.isClimbing;
            isFalling = m_Controller.isFalling;
            floorDistance = m_Controller.floorDistance;

            if( isMoving )
            {
                prevBodyYaw = bodyYaw = turn = 0f;
            }
            else
            {
                prevBodyYaw = bodyYaw;
                bodyYaw = m_Root.eulerAngles.y;

                float targetTurn = Mathf.DeltaAngle( prevBodyYaw, bodyYaw );
                turn = Mathf.Lerp( turn, targetTurn, Time.smoothDeltaTime * 10f );
                turn = Mathf.Clamp( ASKMath.SnapToZero( turn, .01f ), -2f, 2f );
            }
        }

        // Update RadiansAndSpeed
        private void UpdateRadiansAndSpeed()
        {
            float speed = 0f;
            float smoothTime = Time.smoothDeltaTime * 10f;

            if( m_Controller.isGrounded && m_Controller.isClimbing == false )
            {
                speed = m_Controller.speedPercent;
                speed = m_Controller.isRunning ? speed * 2f : speed;
                speed = isMovedForward ? speed : -speed;

                normalizedSpeed = Mathf.Lerp( normalizedSpeed, speed, smoothTime );
            }

            if( speed != 0f )
            {
                // PI = 180 degrees
                const float HALF_PI = Mathf.PI * .5f;

                float angle = m_Controller.deltaAngle;

                if( isMovedForward )
                {
                    angle = Mathf.Clamp( angle, -HALF_PI, HALF_PI );
                }
                else
                {
                    if( angle > HALF_PI ) {
                        angle -= Mathf.PI;
                    }
                    else if( angle < -HALF_PI ) {
                        angle += Mathf.PI;
                    }

                    angle = -angle;
                }

                radians = Mathf.Lerp( radians, angle, smoothTime );
            }
            else
            {
                radians = Mathf.Lerp( radians, 0f, smoothTime );
                normalizedSpeed = Mathf.Lerp( normalizedSpeed, 0f, smoothTime );
            }

            radians = ASKMath.SnapToZero( radians );
            normalizedSpeed = ASKMath.SnapToZero( normalizedSpeed );
        }


        // Update Animator
        private void UpdateAnimator()
        {
            // bools
            m_Animator.SetBool( m_IsMovingHash, isMoving );
            m_Animator.SetBool( m_IsCrouchedHash, isCrouched );
            m_Animator.SetBool( m_IsClimbingHash, isClimbing );
            m_Animator.SetBool( m_IsFallingHash, isFalling );

            // floats
            m_Animator.SetFloat( m_TurnHash, turn );
            m_Animator.SetFloat( m_RadiansHash, radians );
            m_Animator.SetFloat( m_FloorDistanceHash, floorDistance );
            m_Animator.SetFloat( m_NormalizedSpeedHash, normalizedSpeed );            
        }
    };
}
