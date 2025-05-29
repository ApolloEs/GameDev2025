/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using System.Collections;
using AdvancedShooterKit.Utils;

namespace AdvancedShooterKit
{
    // Using for sets firing modes in weapon
    public enum EFiringMode : byte
    {
        Automatic,
        Single,
        Double,
        Triple
    };


    public abstract class WeaponBase : MonoBehaviour
    {
        public LayerMask hitMask = 1;

        [SerializeField]
        protected EFiringMode[] firingModes = new EFiringMode[ 1 ];

        [SerializeField]
        protected float rateOfFire = 250f;

        [SerializeField]
        protected AudioClip shotSFX = null;

        [SerializeField]
        protected float shotDelay = 0f;

        [SerializeField]
        protected float minCameraShake = .05f, maxCameraShake = .15f;

        public bool weaponReady { get; private set; }

        public EFiringMode firingMode { get; protected set; }
        private int firingModeIndex = 0;

        public Transform projectileOuter = null;

        public Character owner { get; private set; }

        protected bool m_SingleShotReady = true;
        protected byte m_ShotsNumber;

        protected PlayerCharacter m_Player;
        protected PlayerCamera m_Camera;
        protected AudioSource m_Audio;
        protected FirstPersonWeaponSway m_FPWeaponSway;


        // Awake
        protected virtual void Awake()
        {
            if( projectileOuter == null )
                Debug.LogError( "Projectile Outer is not setup in: " + name );            

            weaponReady = true;
        }


        // Start
        protected virtual void Start()
        {
            owner = GetComponentInParent<Character>();
            if( owner == null )
            {
                Debug.LogError( "Weapon Owner is not found in: " + name );
                return;
            }

            if( owner.isPlayer )
            {
                m_Player = owner.castToPlayerCharacter;
                m_Camera = m_Player.getCamera;
                m_Audio = GetComponentInParent<AudioSource>();
                m_FPWeaponSway = GetComponentInChildren<FirstPersonWeaponSway>( true );

                if( firingModes.Length >= 1 )
                {
                    firingModeIndex = 0;
                    firingMode = firingModes[ 0 ];
                }

                m_Player.weaponsManager.UpdateHud();
            }
            else if( owner.isNPC )
            {
                firingMode = EFiringMode.Automatic;
                m_Audio = GetComponentInChildren<AudioSource>( true );

                if( m_Audio == null )
                {
                    m_Audio = owner.getAudio;

                    if( m_Audio == null )
                    {
                        Debug.LogWarning( "AudioSource is not found in: " + name );
                        m_Audio = gameObject.AddComponent<AudioSource>();
                    }                    
                }
            }

            m_Audio.SetupForSFX();
        }


        // OnEnable
        protected virtual void OnEnable()
        {
            weaponReady = true;
        }
                
                
        // Switch ShootingMode
        internal bool SwitchShootingMode()
        {
            if( firingModes.Length < 2 )
            {
                return false;
            }                

            firingModeIndex++;

            if( firingModeIndex >= firingModes.Length ) {
                firingModeIndex = 0;
            }                

            firingMode = firingModes[ firingModeIndex ];
            m_Player.weaponsManager.UpdateHud();
            return true;
        }


        // Shooting
        public virtual void StartShooting()
        {
            if( weaponReady == false || ( owner.isPlayer && m_FPWeaponSway != null && m_FPWeaponSway.outOfWall ) )
            {
                return;
            }

            if( projectileOuter != null ) {
                FirstStageShooting();
            }                
            else {
                Debug.LogError( "Projectile Outer is not setup in: " + name );
            }
        }

        // FirstStage Shooting
        protected virtual void FirstStageShooting()
        {
            if( firingMode == EFiringMode.Automatic )
            {
                SecondStageShooting();
            }
            else if( firingMode != EFiringMode.Automatic && m_SingleShotReady )
            {
                m_SingleShotReady = false;
                m_ShotsNumber = ( byte )firingMode;
                m_ShotsNumber--;
                SecondStageShooting();
            }
        }
        // SecondStage Shooting
        protected virtual void SecondStageShooting()
        {
            m_Audio.pitch = Time.timeScale;
            m_Audio.PlayOneShot( shotSFX );

            if( owner.isPlayer && m_FPWeaponSway != null ) {
                m_FPWeaponSway.PlayFireAnimation();
            }                

            MuzzleToCameraPoint();

            if( shotDelay > 0f && firingMode == EFiringMode.Single ) {
                StartCoroutine( ShotWithDelay() );
            }                
            else {
                FinalStageShooting();
            }                

            StartCoroutine( FireRate( 60f / rateOfFire ) );           
        }
        // FinalStage Shooting
        protected virtual void FinalStageShooting()
        {
            if( owner.isPlayer ) {
                m_Camera.ShakeOneShot( Random.Range( minCameraShake, maxCameraShake ) );
            }  
        }

        // Rate Of Fire
        protected IEnumerator FireRate( float rateTime )
        {
            weaponReady = false;

            for( float el = 0f; el < rateTime; el += Time.deltaTime )
                yield return null;

            weaponReady = true;

            if( owner.isPlayer )
            {
                if( checkAmmo && m_ShotsNumber > 0 )
                {
                    SecondStageShooting();
                    m_ShotsNumber--;
                }

                if( checkAmmo == false ) {
                    m_Player.weaponsManager.ChangeEmptyToFirstAvaliable();
                }                    
            }
        }

        // Check Ammo
        protected virtual bool checkAmmo { get { return true; } } 

        // Muzzle ToCameraPoint
        protected virtual void MuzzleToCameraPoint()
        {
            if( owner.isPlayer )
            {
                m_Camera.UpdateHit();

                if( m_Camera.isHitted )
                {
                    projectileOuter.LookAt( m_Camera.hitInfo.point );
                }
                else
                {
                    Transform camTr = m_Camera.getTransform;
                    projectileOuter.LookAt( camTr.position + camTr.forward * 250f );
                }
            }
        }

        // Shot WithDelay
        private IEnumerator ShotWithDelay()
        {
            for( float el = 0f; el < shotDelay; el += Time.deltaTime )
                yield return null;

            FinalStageShooting();
        }
        
        
        // Weapon Reset
        internal void WeaponReset()
        {
            if( m_SingleShotReady == false && m_ShotsNumber == 0 ) {
                m_SingleShotReady = true;
            }                
        }
    };
}