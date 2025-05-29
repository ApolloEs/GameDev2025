/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using System.Collections;

namespace AdvancedShooterKit
{
    using Utils;

    [RequireComponent( typeof( CameraHeadBob )
                     , typeof( FootstepSFXManager )
                     , typeof( CharacterController ) )]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField]
        private bool canWalk = true;
        [SerializeField, Range( 1f, 7f )]
        private float walkSpeed = 4.25f;
        [SerializeField, Range( 0f, 1f )]
        private float backwardsSpeed = .6f;
        [SerializeField, Range( 0f, 1f )]
        private float sidewaysSpeed = .7f;
        [SerializeField, Range( 0f, 1f )]
        private float inAirSpeed = .35f;

        [SerializeField]
        private bool canRun = true;
        [SerializeField, Range( 0f, 1f )]
        private float runSpeed = 8.75f;

        [SerializeField]
        private bool canCrouch = true;
        [SerializeField, Range( 0f, 1f )]
        private float crouchSpeed = .45f;
        [SerializeField, Range( 1f, 1.8f )]
        private float crouchHeight = 1.25f;

        [SerializeField]
        private bool canJump = true;
        [SerializeField, Range( 1f, 10f )]
        private float jumpForce = 5f;

        [SerializeField]
        private bool canClimb = true;
        [SerializeField, Range( 0f, 1f )]
        private float climbingSpeed = .8f;

        [SerializeField]
        private bool useHeadBob = true;
        [SerializeField, Range( 0f, 1f )]
        private float posForce = .65f;
        [SerializeField, Range( 0f, 1f )]
        private float tiltForce = .85f;

        [SerializeField, Range( 1f, 5f )]
        private float gravityMultiplier = 2f;
        [SerializeField, Range( 1f, 5f )]
        private float fallingDistanceToDamage = 3f;
        [SerializeField, Range( 1f, 10f )]
        private float fallingDamageMultiplier = 3.5f;

        [SerializeField, Range( .1f, 1.5f )]
        private float stepInterval = .5f;

        [SerializeField, Range( .1f, 1f )]
        private float lookSmooth = 1f;
        [SerializeField, Range( 25f, 90f )]
        private float maxLookAngleY = 65f;
        [SerializeField]
        private Vector3 cameraOffset = Vector3.up;


        public Transform getTransform { get { return m_Transform; } }


        public bool isGrounded { get; private set; }
        public bool isClimbing { get; private set; }
        public bool isMoving { get; private set; }
        public bool isMoveForward { get; private set; }
        public bool isRunning { get; private set; }
        public bool isCrouched { get; private set; }
        public bool isJumping { get; private set; }
        public bool isFalling { get; private set; }

        public Vector3 velocity { get { return m_Velocity; } }
        Vector3 m_Velocity;

        public float velocitySize { get; private set; }

        public Vector2 deltaPosition { get { return m_DeltaPosition; } }
        Vector2 m_DeltaPosition;

        public float deltaAngle { get; private set; }
        public float speedPercent { get; private set; }


        public RaycastHit floorHit { get { return m_FloorHit; } }
        RaycastHit m_FloorHit;

        public float floorDistance { get; private set; }


        // Fields for move calculation
        bool prevGrounded, jump, crouching;
        CharacterController m_Controller;
        CollisionFlags collisionFlags;
        Transform m_Transform, cameraTransform;
        Vector3 moveDirection, crouchVelVec;
        float nextStep, nativeCapsuleHeight, crouchVel, fallingStartPos, fallingDist;
        float deltaSin, deltaCos;
        Ladder currentLadder;
        Vector3 nativeCapsuleCenter;
        FootstepSFXManager m_FootstepSFXManager;
        CameraHeadBob m_HeadBob;

        // Fields for look calculation
        Vector2 lookRotation = Vector2.zero;
        Quaternion nativeRotation = Quaternion.identity;

        PlayerCharacter m_Player;
        FirstPersonWeaponSway m_CurrentFPWeapSway;
        ASKInputManager m_Input { get { return m_Player.inputManager; } }


        // Awake
        void Awake()
        {
            m_Transform = transform;

            m_Controller = GetComponent<CharacterController>();

            nativeCapsuleHeight = m_Controller.height;
            nativeCapsuleCenter = m_Controller.center;
            nativeCapsuleCenter.y = nativeCapsuleHeight * .5f;
            m_Controller.center = nativeCapsuleCenter;

            m_FootstepSFXManager = GetComponent<FootstepSFXManager>();
            m_HeadBob = GetComponent<CameraHeadBob>();
            m_Player = GetComponent<PlayerCharacter>();
        }

        // Start
        void Start()
        {
            cameraTransform = m_Player.getCamera.getTransform;

            lookRotation.x = m_Transform.eulerAngles.y;
            lookRotation.y = cameraTransform.localEulerAngles.y;

            nativeRotation.eulerAngles = new Vector3( 0f, lookRotation.y, 0f );
        }

        
        // FixedUpdate
        void FixedUpdate()
        {
            UpdateStates();
        }

        // Update
        void Update()
        {
            CameraLook();
        }

        // Late Update
        void LateUpdate()
        {
            UpdateMoveAngle();
        }


        // SetCurrent FPWeapSway
        internal void SetCurrentFPWeapSway( FirstPersonWeaponSway fpWeapSway )
        {
            m_CurrentFPWeapSway = fpWeapSway;
        }


        // UpdateStates
        private void UpdateStates()
        {
            isGrounded = m_Controller.isGrounded;
            if( isGrounded )
            {
                if( isFalling )
                {
                    isFalling = false;

                    if( fallingDist > fallingDistanceToDamage )
                    {
                        int damage = Mathf.RoundToInt( fallingDist * fallingDamageMultiplier );
                        m_Player.hud.healthBar.SetDamageTargetPosition( Vector3.zero );
                        m_Player.DecrementHealth( damage );
                    }

                    fallingDist = 0f;
                }
            }
            else
            {
                if( isFalling )
                {
                    fallingDist = fallingStartPos - m_Transform.position.y;
                }
                else
                {
                    if( isClimbing == false )
                    {
                        isFalling = true;
                        fallingStartPos = m_Transform.position.y;
                    }
                }
            }

            Movement();
            PlayFootStepAudio();

            if( isClimbing == false && isGrounded == false && isJumping == false && prevGrounded )
            {
                moveDirection.y = 0f;
            }

            prevGrounded = isGrounded;
        }


        // Update MoveAngle
        private void UpdateMoveAngle()
        {
            m_DeltaPosition.x = Vector3.Dot( m_Transform.right, m_Velocity );
            m_DeltaPosition.y = Vector3.Dot( m_Transform.forward, m_Velocity );

            deltaAngle = Mathf.Atan2( m_DeltaPosition.x, m_DeltaPosition.y );

            deltaSin = ASKMath.SnapToZero( Mathf.Sin( deltaAngle ) );
            deltaCos = ASKMath.SnapToZero( Mathf.Cos( deltaAngle ) );
        }


        // Jump
        internal void Jump()
        {
            if( isClimbing || crouching || isGrounded == false )
                return;

            if( isCrouched )
            {
                Crouch();
                return;
            }

            if( canJump && canWalk && jump == false && isJumping == false )
                jump = true;
        }

        // Crouch
        internal void Crouch()
        {
            if( canCrouch == false || isClimbing || crouching || isGrounded == false )
                return;

            crouching = true;

            if( isCrouched )
            {
                if( Physics.SphereCast( m_Transform.position + Vector3.up * .75f, m_Controller.radius, Vector3.up, out m_FloorHit, nativeCapsuleHeight * .25f ) )
                {
                    //Debug.Log( "StandUp return" );
                    crouching = false;
                    return;
                }

                //Debug.Log( "StandUp RUN" );
                StartCoroutine( StandUp() );
            }
            else
            {
                StartCoroutine( SitDown() );
            }
        }

        // Movement
        private void Movement()
        {
            float horizontal = m_Input.moveHorizontal * Time.timeScale; // move Left/Right
            float vertical = m_Input.moveVertical * Time.timeScale;     // move Forward/Backward            

            bool moveForward = ( vertical > -.1f );
            isMoveForward = moveForward;

            bool runReady = ( canRun && isCrouched == false && moveForward );
            runReady &= ( m_CurrentFPWeapSway == null || ( m_CurrentFPWeapSway.ironsightZoomed == false && m_CurrentFPWeapSway.ironsightZooming == false ) );

            isRunning = ( m_Input.runAction && runReady );

            float maxSpeed = GetMaxSpeed();

            Quaternion screenMovementSpace = Quaternion.Euler( 0f, cameraTransform.eulerAngles.y, 0f );
            Vector3 forwardVector = screenMovementSpace * new Vector3( 0f, 0f, vertical ); //forward;
            Vector3 rightVector = screenMovementSpace * new Vector3( horizontal, 0f, 0f ); //right;
            Vector3 moveVector = forwardVector + rightVector;
            moveVector = moveVector.normalized * Mathf.Clamp01( moveVector.magnitude );

            if( isClimbing )
            {
                bool lookUp = cameraTransform.forward.y > -.4f;

                if( moveForward )
                {
                    forwardVector = currentLadder.m_Transform.up * ( lookUp ? vertical : -vertical );

                    moveVector = forwardVector + rightVector;

                    if( isGrounded && lookUp == false )
                    {
                        moveVector += screenMovementSpace * Vector3.forward;
                    }
                    else if( isGrounded == false && lookUp )
                    {
                        moveVector += screenMovementSpace * Vector3.forward;
                    }
                }

                moveDirection = moveVector * maxSpeed;
            }
            else
            {
                if( isGrounded )
                {
                    Physics.SphereCast( m_Transform.position + m_Controller.center, m_Controller.radius, Vector3.down, out m_FloorHit, m_Controller.height * .5f );

                    moveDirection = moveVector * maxSpeed;
                    moveDirection.y = -10f;

                    if( jump )
                    {
                        m_FootstepSFXManager.PlayJumpingSound( m_FloorHit );
                        isJumping = true;
                        jump = false;
                        moveDirection.y = jumpForce;
                    }

                    floorDistance = 0f;
                }
                else
                {
                    moveDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;

                    if( Physics.Raycast( m_Transform.position, Vector3.down, out m_FloorHit ) )
                    {
                        floorDistance = ASKMath.SnapToZero( m_FloorHit.distance );
                    }
                }
            }

            if( canWalk )
            {
                collisionFlags = m_Controller.Move( moveDirection * Time.fixedDeltaTime );
            }

            m_Velocity = m_Controller.velocity;
            m_Velocity.y = isClimbing ? m_Velocity.y : 0f;

            velocitySize = m_Velocity.magnitude;
            speedPercent = velocitySize / maxSpeed;

            bool accelerated = ( velocitySize > .01f );
            isMoving = isClimbing ? accelerated : isGrounded && accelerated;
        }
        // Get Speed
        private float GetMaxSpeed()
        {
            float speed = isRunning ? runSpeed : walkSpeed;

            if( isClimbing )
            {
                speed *= climbingSpeed;
            }
            else
            {
                if( isCrouched )
                {
                    speed *= crouchSpeed;
                }
                else if( isFalling && isJumping == false )
                {
                    speed *= inAirSpeed;
                }
            }

            if( deltaCos < 0f )
            {
                speed *= ASKMath.Persent01( backwardsSpeed, deltaCos );
            }

            speed *= ASKMath.Persent01( sidewaysSpeed, deltaSin );
            speed *= ( m_CurrentFPWeapSway != null ) ? m_CurrentFPWeapSway.moveSpeed : 1f;

            return speed;
        }


        // Camera Look
        private void CameraLook()
        {
            float nextHorizontal = m_Input.lookHorizontal * Time.timeScale;
            float nextVertical = m_Input.lookVertical * Time.timeScale;

            lookRotation.x += nextHorizontal;
            lookRotation.y += nextVertical;

            lookRotation.y = Mathf.Clamp( lookRotation.y, -maxLookAngleY, maxLookAngleY );

            Quaternion camTargetRotation = nativeRotation * Quaternion.AngleAxis( lookRotation.y + ( useHeadBob ? m_HeadBob.xTilt * tiltForce : 0f ), Vector3.left );
            Quaternion bodyTargetRotation = nativeRotation * Quaternion.AngleAxis( lookRotation.x + ( useHeadBob ? m_HeadBob.yTilt * tiltForce : 0f ), Vector3.up );

            cameraTransform.localRotation = Quaternion.Slerp( cameraTransform.localRotation, camTargetRotation, lookSmooth );
            m_Transform.localRotation = Quaternion.Slerp( m_Transform.localRotation, bodyTargetRotation, lookSmooth );

            cameraTransform.localPosition = new Vector3
            {
                x = m_Controller.center.x + cameraOffset.x + ( useHeadBob ? m_HeadBob.xPos * posForce : 0f ),
                y = ( m_Controller.center.y * 2f ) + cameraOffset.y + ( useHeadBob ? m_HeadBob.yPos * posForce : 0f ),
                z = m_Controller.center.z + cameraOffset.z
            };
        }


        // StandUp
        private IEnumerator StandUp()
        {
            Vector3 targetCenter = nativeCapsuleCenter;

            isCrouched = false;

            while( PlayCrouchAnimation( targetCenter, nativeCapsuleHeight ) )
                yield return null;

            m_Controller.height = nativeCapsuleHeight;
            m_Controller.center = targetCenter;

            crouching = false;
        }
        // SitDown
        private IEnumerator SitDown()
        {
            Vector3 targetCenter = nativeCapsuleCenter;
            targetCenter.y = crouchHeight * .5f;

            isCrouched = true;

            while( PlayCrouchAnimation( targetCenter, crouchHeight ) )
                yield return null;

            m_Controller.height = crouchHeight;
            m_Controller.center = targetCenter;

            crouching = false;
        }
        // Play CrouchAnimation
        private bool PlayCrouchAnimation( Vector3 targetCenter, float targetHeight )
        {
            float deltaStep = Time.fixedDeltaTime * 5f;

            m_Controller.height = Mathf.SmoothDamp( m_Controller.height, targetHeight, ref crouchVel, deltaStep );
            m_Controller.center = Vector3.SmoothDamp( m_Controller.center, targetCenter, ref crouchVelVec, deltaStep );

            const int digits = 3;
            float cMag = ASKMath.Round( m_Controller.center.magnitude, digits );
            float tMag = ASKMath.Round( targetCenter.magnitude, digits );

            return ( cMag != tMag );
        }


        // Play FootStepAudio 
        private void PlayFootStepAudio()
        {
            if( prevGrounded == false && isGrounded )
            {
                m_FootstepSFXManager.PlayLandingSound( m_FloorHit );
                nextStep = m_HeadBob.headBobCycle + stepInterval;
                isJumping = false;
                moveDirection.y = 0f;
                return;
            }

            if( m_HeadBob.headBobCycle > nextStep )
            {
                nextStep = m_HeadBob.headBobCycle + stepInterval;

                if( isGrounded )
                {
                    m_FootstepSFXManager.PlayFootStepSound( m_FloorHit );
                }
                else if( isClimbing )
                {
                    currentLadder.PlayLadderFootstepSound();
                }
            }
        }


        // OnController ColliderHit            
        void OnControllerColliderHit( ControllerColliderHit hit )
        {
            PushObject( hit );
        }

        // OnTrigger Enter
        void OnTriggerEnter( Collider collider )
        {
            OnLadderEnter( collider );
        }

        // OnTrigger Exit
        void OnTriggerExit( Collider collider )
        {
            FromLadderExit();
        }


        // PushObject
        private void PushObject( ControllerColliderHit hit )
        {
            if( collisionFlags == CollisionFlags.Below )
            {
                return;
            }

            Rigidbody hitRb = hit.collider.attachedRigidbody;

            if( hitRb != null && hitRb.isKinematic == false )
            {
                hitRb.AddForceAtPosition( hit.moveDirection * ( m_Controller.velocity.magnitude * 1.25f / hitRb.mass ), hit.point, ForceMode.VelocityChange );
            }
        }

        // OnLadderEnter
        private void OnLadderEnter( Collider collider )
        {
            if( canClimb == false )
            {
                return;
            }

            currentLadder = collider.GetComponent<Ladder>();

            if( currentLadder == null )
            {
                return;
            }

            if( isCrouched )
            {
                Crouch();
            }

            currentLadder.AssignAudioSource( GetComponent<AudioSource>() );
            moveDirection = Vector3.zero;
            isClimbing = true;

            isFalling = false;
            fallingDist = 0f;
        }

        // FromLadder Exit
        private void FromLadderExit()
        {
            if( isClimbing )
            {
                isClimbing = false;
                currentLadder = null;
            }
        }


        // Player Die
        internal void PlayerDie()
        {
            if( m_CurrentFPWeapSway != null ) {
                m_CurrentFPWeapSway.gameObject.SetActive( false );
            }

            enabled = false;
            m_Controller.height = .1f;
            m_Controller.radius = .1f;
        }
    };
}