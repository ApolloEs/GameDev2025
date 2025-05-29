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

    public class CameraHeadBob : MonoBehaviour
    {
        [SerializeField, Range( 1f, 3f )]
        private float headBobFrequency = 1.5f;

        [SerializeField, Range( .1f, 2f )]
        private float headBobHeight = .35f;

        [SerializeField, Range( .1f, 2f )]
        private float headBobSwayAngle = .5f;

        [SerializeField, Range( .01f, .1f )]
        private float headBobSideMovement = .075f;

        [SerializeField, Range( .1f, 2f )]
        private float bobHeightSpeedMultiplier = .35f;

        [SerializeField, Range( .1f, 2f )]
        private float bobStrideSpeedLengthen = .35f;

        [SerializeField, Range( .1f, 5f )]
        private float jumpLandMove = 2f;

        [SerializeField, Range( 10f, 100f )]
        private float jumpLandTilt = 35f;

        [SerializeField, Range( .1f, 4f )]
        private float springElastic = 1.25f;

        [SerializeField, Range( .1f, 2f )]
        private float springDampen = .77f;


        // Fields for calculation
        float springPos, springVelocity, headBobFade;
        Vector3 prevVelocity, prevPosition;
        FirstPersonController m_Controller;
        Transform m_Transform;


        // Fields for internal access
        public float headBobCycle { get; private set; }
        public float xPos { get; private set; }
        public float yPos { get; private set; }
        public float xTilt { get; private set; }
        public float yTilt { get; private set; }


        // Awake
        void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<FirstPersonController>();
        }

        // FixedUpdate
        void FixedUpdate()
        {
            UpdateValues( Time.fixedDeltaTime );
        }

        // UpdateValues
        private void UpdateValues( float deltaTime )
        {
            Vector3 velocity = ( m_Transform.position - prevPosition ) / deltaTime;
            Vector3 deltaVelocity = velocity - prevVelocity;
            prevPosition = m_Transform.position;
            prevVelocity = velocity;

            if( m_Controller.isClimbing == false ) {
                velocity.y = 0f;
            }

            springVelocity -= deltaVelocity.y;
            springVelocity -= springPos * springElastic;
            springVelocity *= springDampen;
            springPos += springVelocity * deltaTime;
            springPos = Mathf.Clamp( springPos, -.32f, .32f );

            if( Mathf.Abs( springVelocity ) < .05f && Mathf.Abs( springPos ) < .05f ) {
                springVelocity = springPos = 0f;
            }

            float velocitySize = velocity.magnitude;
            float flatVelocity = velocitySize;

            if( m_Controller.isClimbing ) {
                flatVelocity *= 4f;
            }
            else if( m_Controller.isClimbing == false && m_Controller.isGrounded == false ) {
                flatVelocity /= 4f;
            }

            float strideLengthen = 1f + flatVelocity * bobStrideSpeedLengthen;
            headBobCycle += ( flatVelocity / strideLengthen ) * ( deltaTime / headBobFrequency );

            float headBobCyclePi = headBobCycle * ASKMath.DOUBLE_PI;
            float bobFactor = Mathf.Sin( headBobCyclePi );
            float bobSwayFactor = Mathf.Sin( headBobCyclePi + ASKMath.HALF_PI );
            bobFactor = 1f - ( bobFactor * .5f + 1f );
            bobFactor *= bobFactor;

            headBobFade = Mathf.Lerp( headBobFade, ( velocitySize < .1f ) ? 0f : 1f, deltaTime );
            headBobFade = ASKMath.SnapToZero( headBobFade );

            float speedHeightFactor = 1f + ( flatVelocity * bobHeightSpeedMultiplier );

            xPos = -headBobSideMovement * bobSwayFactor * headBobFade;
            yPos = springPos * jumpLandMove + bobFactor * headBobHeight * headBobFade * speedHeightFactor;
            xTilt = springPos * jumpLandTilt;
            yTilt = bobSwayFactor * headBobSwayAngle * headBobFade;
        }
    };
}