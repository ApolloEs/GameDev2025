/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using System.Collections;

#pragma warning disable 169

namespace AdvancedShooterKit
{
    using Utils;

    public class FirstPersonWeaponSway : MonoBehaviour
    {
        // Sway
        [SerializeField]
        private bool useSway = true;
        [SerializeField]
        private float swaySmoothing = 2.2f;
        [SerializeField]
        private float borderSize = 25f;
        [SerializeField]
        private float fpMoveSpeed = 1f;
        internal float moveSpeed { get; set; }

        // Animation
        [SerializeField, Range( 1f, 10f )]
        private float dropoutSmoothing = 4.5f;
        [SerializeField]
        private Vector3 dropoutRotation;
        [SerializeField]
        private Vector3 runOffset;
        [SerializeField]
        private Vector3 outWallOffset;
        internal bool isChanging { get; private set; }
        internal bool isPlaying { get; private set; }

        // Ironsighting
        [SerializeField]
        private bool useIronsighting = true;        
        [SerializeField]
        private float ironsightSmoothing = 3.5f;
        [SerializeField]
        private float ironsightDispersion = .5f;
        [SerializeField]
        private float addMove = .85f;
        [SerializeField]
        private float addLook = .4f;
        [SerializeField]
        private float addRunAndInAir = 1.15f;
        [SerializeField]
        private float crouched = .55f;
        [SerializeField]
        private float zoomFOV = 20f;
        [SerializeField]
        private float ironsightMoveSpeed = 1f;
        [SerializeField]
        private Vector3 zoomPos = Vector3.zero;        
        internal bool ironsightZoomed { get; private set; }
        internal bool ironsightZooming { get; private set; }

        // Crosshair
        public ECrosshair.EView crosshairView = ECrosshair.EView.ALL;

        // Pivot
        public Vector3 pivotPosition = Vector3.zero;
        

        // Sway  
        Transform m_Parent, m_Transform, m_СameraTransform;
        Vector3 nativePosition, noisePosition;
        Vector3 prevRotation, prevVelocity;
        float halfBorderSize, fullBorderSize;
        float divider = 400f;
        float vel01, vel02, vel03, magVel;
        float idleX, idleY;
        float dispersionMultiplier = 1f;
        float bodyYaw, prevBodyYaw;

        internal bool outOfWall { get; private set; }

        // Animation
        Animator m_Animator;
        Vector3 dropinRotation, velRotation;
        
        // Ironsighting
        bool meleeWeapon;
        float mainCamNativeFOV;
        Vector3 zoomVel;
        float camFovVel;
        Firearms m_Firearm;
        float nativeDispersion;
        const int ironsightingDigits = 4;

        // Pivot
        Vector3 parentPosition, prevPivotPosition;        

        // other
        PlayerCharacter m_Player;
        ASKInputManager m_Input { get { return m_Player.inputManager; } }
        HudElements m_Hud { get { return m_Player.hud; } }
        CameraHeadBob m_HeadBob;
        PlayerCamera m_PlayerCamera { get { return m_Player.getCamera; } }
        FirstPersonController m_Controller { get { return m_Player.fpController; } }

        [ContextMenuItem( "Reset", "ResetBounds" )]
        public Bounds bounds = new Bounds( Vector3.zero, Vector3.one );
        void ResetBounds() { bounds = new Bounds( Vector3.zero, Vector3.one ); }

                
        // Awake
        void Awake()
        {
            InitComponents();
            InitValues();
        }

        // Start
        void Start()
        {
            m_СameraTransform = m_PlayerCamera.getTransform;
            mainCamNativeFOV = m_PlayerCamera.mainCamera.fieldOfView;          
        }

        // OnEnable
        void OnEnable()
        {
            moveSpeed = useSway ? fpMoveSpeed : 1f;
        }


        // InitComponents
        private void InitComponents()
        {
            m_Transform = transform;
            m_Parent = m_Transform.parent;

            m_Animator = GetComponentInChildren<Animator>();
            m_Firearm = m_Parent.GetComponent<Firearms>();

            Transform root = m_Transform.root;
            m_HeadBob = root.GetComponent<CameraHeadBob>();
            m_Player = root.GetComponent<PlayerCharacter>();
        }

        // InitValues
        private void InitValues()
        {
            halfBorderSize = borderSize / 2.75f;
            fullBorderSize = borderSize;

            dropinRotation = m_Transform.localEulerAngles;

            parentPosition = m_Parent.localPosition;

            if( m_Firearm != null )
            {
                nativeDispersion = m_Firearm.dispersion;
            }
            else
            {
                meleeWeapon = true;
                useIronsighting = false;
            }

            SetPivot();

            m_Parent.localPosition = noisePosition = nativePosition;
        }
        


        // Update
        void Update()
        {
            if( useIronsighting && meleeWeapon == false && isPlaying == false && isChanging == false ) {
                Ironsighting();
            }                

            if( prevPivotPosition != pivotPosition ) {
                SetPivot();
            }
        }

        // Late Update
        void LateUpdate()
        {
            Sway();
            UpdadeDispersion();
            UpdateAnimatorData();
        }

        // Fixed Update
        void FixedUpdate()
        {
            Matrix4x4 cameraMatrix = m_СameraTransform.localToWorldMatrix;
            Vector3 camCenter = cameraMatrix * bounds.center;
            camCenter += m_СameraTransform.position;

            if( ironsightZoomed || ironsightZooming )
            {
                Vector3 offset = m_Parent.localPosition - pivotPosition;
                offset = cameraMatrix * offset;
                camCenter += offset;
            }

            outOfWall = Physics.CheckBox( camCenter, bounds.extents, m_СameraTransform.rotation, m_PlayerCamera.hitMask, QueryTriggerInteraction.Ignore );
        }

        
        // Update AnimatorData
        private void UpdateAnimatorData()
        {
            if( m_Animator == null )
            {
                isPlaying = false;
                return;
            }
            
            AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo( 0 );
            isPlaying = ( stateInfo.normalizedTime < 1f || m_Animator.IsInTransition( 0 ) );
        }

        //Updade Dispersion
        private void UpdadeDispersion()
        {
            if( meleeWeapon || useIronsighting == false )
            {
                return;
            }

            float newMultiplier = 1f;

            if( m_Controller.isMoving ) // AddMove
                newMultiplier += addMove;

            if( ( m_Input.lookHorizontal != 0 ) || ( m_Input.lookVertical != 0 ) ) // AddLook
                newMultiplier += addLook;

            if( m_Controller.isRunning || m_Controller.isGrounded == false ) // AddRunningAndInAir
                newMultiplier += addRunAndInAir;

            newMultiplier = m_Controller.isCrouched ? newMultiplier * crouched : newMultiplier; // Crouched

            dispersionMultiplier = Mathf.SmoothDamp( dispersionMultiplier, newMultiplier, ref magVel, Time.smoothDeltaTime * 5f );

            if( ironsightZoomed )
                m_Firearm.dispersion = nativeDispersion * ironsightDispersion * dispersionMultiplier;
            else
                m_Firearm.dispersion = nativeDispersion * dispersionMultiplier;

            m_Hud.crosshair.UpdatePosition( m_Firearm.dispersion );
        }

        
        // Sway
        private void Sway()
        {
            if( useSway )
            {
                idleX = Mathf.Sin( Time.time * 1.25f ) + m_HeadBob.xPos * 50f;
                idleY = Mathf.Cos( Time.time * 1.5f ) + m_HeadBob.yPos * 50f;

                float dividerDeltaTime = Time.smoothDeltaTime * 1000f;

                if( ironsightZoomed )
                {
                    if( divider < 1600f ) {
                        divider += dividerDeltaTime;
                    }                        

                    idleX /= divider;
                    idleY /= divider;

                    if( ironsightZooming == false ) {
                        noisePosition = zoomPos;
                    }                        
                }
                else
                {
                    if( m_Controller.isMoving )
                    {
                        if( divider < 800f ) {
                            divider += dividerDeltaTime;
                        }                            
                        else if( divider > 800f ) {
                            divider -= dividerDeltaTime;
                        }                            

                        idleX /= divider;
                        idleY /= divider;
                    }
                    else
                    {
                        if( divider > 400f ) {
                            divider -= dividerDeltaTime;
                        }                            

                        idleX /= divider;
                        idleY /= divider;
                    }

                    if( ironsightZooming == false ) {
                        noisePosition = nativePosition;
                    }                        
                }
            }
            else
            {
                idleX = idleY = 0f;
            }

            if( ironsightZooming == false )
            {
                noisePosition.x += idleX;
                noisePosition.y += idleY;
                m_Parent.localPosition = noisePosition;
            }

            if( useSway == false ) 
            {
                return;
            }

            Vector3 localEulerAngles = m_Player.getTransform.localEulerAngles + m_СameraTransform.localEulerAngles;
            Vector3 velocity = ( localEulerAngles - prevRotation ) / Time.fixedDeltaTime;
            Vector3 velocityChange = velocity + prevVelocity;
            prevRotation = localEulerAngles;
            prevVelocity = velocity;
            velocityChange *= -Time.fixedDeltaTime;

            float smoothTime = Time.smoothDeltaTime;
            smoothTime = ( smoothTime == 0f ) ? Time.deltaTime : smoothTime;

            float smoothing = swaySmoothing * Time.smoothDeltaTime;

            Vector3 eulerAngles = new Vector3
            {
                x = Mathf.SmoothDampAngle( m_Parent.localEulerAngles.x, velocityChange.x, ref vel01, smoothing, Mathf.Infinity, smoothTime ),
                y = Mathf.SmoothDampAngle( m_Parent.localEulerAngles.y, velocityChange.y, ref vel02, smoothing, Mathf.Infinity, smoothTime ),
                z = Mathf.SmoothDampAngle( m_Parent.localEulerAngles.z, velocityChange.z, ref vel03, smoothing, Mathf.Infinity, smoothTime )
            };

            float smoothStep = smoothTime * 25f;

            if( outOfWall )
            {
                IronsightUnzoom();                
                m_Parent.localRotation = Quaternion.Slerp( m_Parent.localRotation, GetSwayOffset( eulerAngles, outWallOffset ), smoothStep );
                prevBodyYaw = bodyYaw = 0f;
                return;
            }

            if( m_Controller.isRunning )
            {
                IronsightUnzoom();
                m_Parent.localRotation = Quaternion.Slerp( m_Parent.localRotation, GetSwayOffset( eulerAngles, runOffset ), smoothStep );
                prevBodyYaw = bodyYaw = 0f;
            }
            else
            {
                m_Parent.localRotation = Quaternion.Slerp( m_Parent.localRotation, Quaternion.Euler( eulerAngles ), smoothStep );

                prevBodyYaw = bodyYaw;
                bodyYaw = m_СameraTransform.eulerAngles.y;

                float turn = Mathf.Abs( Mathf.DeltaAngle( prevBodyYaw, bodyYaw ) );
                if( turn > borderSize )
                {
                    float angle = Mathf.Sign( m_Input.lookHorizontal ) * -borderSize;
                    Quaternion lookRotation = Quaternion.LookRotation( Quaternion.AngleAxis( angle, Vector3.up ) * m_СameraTransform.forward );
                    m_Parent.rotation = Quaternion.Slerp( m_Parent.rotation, lookRotation, smoothStep * .8f );
                }
            }
        }

        // GetOffset
        private Quaternion GetSwayOffset( Vector3 eulerAngles, Vector3 targetOffset )
        {
            float x = eulerAngles.x - targetOffset.x - m_HeadBob.yPos * 10f;
            float y = eulerAngles.y - targetOffset.y + m_HeadBob.xPos * 15f;
            float z = eulerAngles.z + targetOffset.z;

            return Quaternion.Euler( x, y, z );
        }

                
        // Play FireAnimation
        internal void PlayFireAnimation()
        {
            m_Animator.SetTrigger( "Fire" );
        }

        // Play ReloadAnimation
        internal void PlayReloadAnimation()
        {
            m_Animator.SetTrigger( "Reload" );
        }

        // Dropin Animation
        internal void DropinAnimation()
        {
            isChanging = true;
            StartCoroutine( StartDropinAnimation() );
        }

        // Dropout Animation
        internal void DropoutAnimation()
        {
            isChanging = true;
            StartCoroutine( StartDropoutAnimation() );
        }

        // Start DropinAnimation
        private IEnumerator StartDropinAnimation()
        {
            m_Transform.localEulerAngles = dropoutRotation;

            while( DropAnimationPlay( dropinRotation ) )
                yield return null;            

            m_Transform.localEulerAngles = dropinRotation;
            isChanging = false;
        }

        // Start DropoutAnimation
        private IEnumerator StartDropoutAnimation()
        {
            m_Transform.localEulerAngles = dropinRotation;

            while( DropAnimationPlay( dropoutRotation ) )
                yield return null;            

            m_Transform.localEulerAngles = dropoutRotation;
            isChanging = false;
        }

        // DropAnimation Play
        private bool DropAnimationPlay( Vector3 targetRotation )
        {
            m_Transform.localEulerAngles = Vector3.SmoothDamp( m_Transform.localEulerAngles, targetRotation, ref velRotation, dropoutSmoothing * Time.smoothDeltaTime );
            
            const int digits = 2;
            float eaMag = ASKMath.Round( m_Transform.localEulerAngles.magnitude, digits );
            float drMag = ASKMath.Round( targetRotation.magnitude, digits );
            
            return ( eaMag != drMag );
        }



        // Ironsighting
        private void Ironsighting()
        {
            if( m_Controller.isRunning || ironsightZooming || outOfWall )
            {
                return;
            }            

            bool zoomActionDown = m_Input.zoomActionDown;
            bool zoomAction = m_Input.zoomAction;

            switch( m_Player.weaponsManager.IronsightingMode )
            {
                case EIronsightingMode.Click:
                    if( zoomActionDown && ironsightZoomed )
                        StartUnzoom();
                    else if( zoomActionDown && ironsightZoomed == false )
                        StartZoom();
                    break;
                case EIronsightingMode.Press:
                    if( zoomAction && ironsightZoomed == false )
                        StartZoom();                    
                    else if( zoomAction == false && ironsightZoomed )
                        StartUnzoom();                    
                    break;
                case EIronsightingMode.Mixed:
                    if( zoomActionDown && ironsightZoomed == false )
                        StartZoom();
                    else if( ( zoomActionDown || m_Input.zoomActionUp ) && ironsightZoomed )
                        StartUnzoom();
                    break;
            }
        }

        // Start Zoom
        private void StartZoom()
        {
            m_Hud.crosshair.SetActive( ECrosshair.EView.OFF );

            StopCoroutine( "IronsightUnzoomming" );
            StartCoroutine( "IronsightZoomming" );
        }
        // Start Unzoom
        private void StartUnzoom()
        {
            StopCoroutine( "IronsightZoomming" );
            StartCoroutine( "IronsightUnzoomming" );
        }

        // Ironsight Zoomming
        private IEnumerator IronsightZoomming()
        {
            ironsightZooming = true;

            while( ironsightZooming )
            {
                float smoothing = ironsightSmoothing * Time.smoothDeltaTime;

                noisePosition = zoomPos;
                noisePosition.x += idleX;
                noisePosition.y += idleY;
                m_PlayerCamera.mainCamera.fieldOfView = Mathf.SmoothDamp( m_PlayerCamera.mainCamera.fieldOfView, mainCamNativeFOV - zoomFOV, ref camFovVel, smoothing );
                m_Parent.localPosition = Vector3.SmoothDamp( m_Parent.localPosition, noisePosition, ref zoomVel, smoothing );

                float lpMag = ASKMath.Round( m_Parent.localPosition.magnitude, ironsightingDigits );
                float npMag = ASKMath.Round( noisePosition.magnitude, ironsightingDigits );

                if( lpMag == npMag )
                {
                    ironsightZooming = false;
                    ironsightZoomed = true;

                    m_Parent.localPosition = noisePosition;
                    m_PlayerCamera.mainCamera.fieldOfView = mainCamNativeFOV - zoomFOV;
                    borderSize = halfBorderSize;
                    m_Firearm.dispersion = ironsightDispersion;
                    moveSpeed = fpMoveSpeed * ironsightMoveSpeed;
                }

                yield return null;
            }
        }
        // Ironsight Unzoomming
        private IEnumerator IronsightUnzoomming()
        {
            ironsightZooming = true;

            while( ironsightZooming )
            {
                float smoothing = ironsightSmoothing * Time.smoothDeltaTime;

                noisePosition = nativePosition;
                noisePosition.x += idleX;
                noisePosition.y += idleY;
                m_PlayerCamera.mainCamera.fieldOfView = Mathf.SmoothDamp( m_PlayerCamera.mainCamera.fieldOfView, mainCamNativeFOV, ref camFovVel, smoothing );
                m_Parent.localPosition = Vector3.SmoothDamp( m_Parent.localPosition, noisePosition, ref zoomVel, smoothing );

                float lpMag = ASKMath.Round( m_Parent.localPosition.magnitude, ironsightingDigits );
                float npMag = ASKMath.Round( noisePosition.magnitude, ironsightingDigits );

                if( lpMag == npMag )
                {
                    ironsightZooming = false;
                    ironsightZoomed = false;

                    m_Parent.localPosition = noisePosition;
                    m_PlayerCamera.mainCamera.fieldOfView = mainCamNativeFOV;
                    borderSize = fullBorderSize;
                    m_Firearm.dispersion = nativeDispersion;
                    moveSpeed = fpMoveSpeed;
                    m_Hud.crosshair.SetActive( crosshairView );
                    m_Hud.crosshair.SetPointSprite( crosshairView == ECrosshair.EView.Point ? ECrosshair.EPointMode.BigPoint : ECrosshair.EPointMode.SmallPoint );
                }

                yield return null;
            }
        }
                

        // Ironsight Unzoom
        internal void IronsightUnzoom()
        {
            if( ironsightZoomed && ironsightZooming == false )
            {
                StopCoroutine( "IronsightZoomming" );
                StartCoroutine( "IronsightUnzoomming" );
            }
        }


        // Full Reset
        internal void FullReset()
        {
            FullResetInternal();
            ironsightZoomed = ironsightZooming = isChanging = false;
        }

        // FullReset Internal
        private void FullResetInternal()
        {
            StopAllCoroutines();

            if( ironsightZoomed || ironsightZooming ) {
                m_Parent.localPosition = nativePosition;
            }                

            if( isChanging ) {
                m_Transform.localEulerAngles = dropoutRotation;
            }                

            if( isPlaying ) {
                m_Animator.StopPlayback();
            } 
        }


        // Set Pivot
        private void SetPivot()
        {
            m_Transform.localPosition = parentPosition - pivotPosition;
            m_Parent.localPosition = nativePosition = pivotPosition;
            prevPivotPosition = pivotPosition;            
        }


#if UNITY_EDITOR
        // OnDrawGizmos Selected
        void OnDrawGizmosSelected()
        {
            if( m_Parent == null || m_СameraTransform == null )
            {
                m_Parent = transform.parent;
                m_СameraTransform = GetComponentInParent<Camera>().transform;
            }

            Gizmos.matrix = m_СameraTransform.localToWorldMatrix;
            Gizmos.color = new Color32( 242, 242, 242, 192 );

            Vector3 cubeCenter = bounds.center;

            if( ironsightZoomed || ironsightZooming ) {
                cubeCenter += m_Parent.localPosition - pivotPosition;
            }

            Gizmos.DrawCube( cubeCenter, bounds.size );
            Gizmos.DrawWireCube( cubeCenter, bounds.size );
            
            const float radius = .05f;
            Gizmos.color = Color.red * .8f;
            Gizmos.DrawSphere( pivotPosition, radius );
            Gizmos.DrawWireSphere( pivotPosition, radius );
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    };
}