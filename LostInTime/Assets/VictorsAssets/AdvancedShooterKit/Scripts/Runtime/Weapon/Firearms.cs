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
    public class Firearms : WeaponBase
    {
        [System.Serializable]
        public sealed class ProjectileSlot
        {
            public string ammoType;
            public int currentAmmo;
            public GameObject projectile, shell;

            public ProjectileSlot( int ammo )
            {
                ammoType = string.Empty;
                currentAmmo = ammo;
                projectile = null;
            }


            public bool isEmpty { get { return currentAmmo < 1; } }


            // AddAmmo
            public void AddAmmo( int amount )
            {
                currentAmmo += amount;
            }

            // AddAmmo
            public void SetAmmo( int ammo )
            {
                currentAmmo = ammo;
            }

            // Decrement Ammo
            public void DecrementAmmo()
            {
                currentAmmo--;
            }
        }
        

        [Range( 1, 100 )]
        public int maxAmmo = 32;
        public bool autoReload = true;

        [Range( -10f, 35f )]
        public float addDamage = 0f;
        [Range( -5f, 15f )]
        public float addSpeed = 0f;
        [Range( 5f, 100f )]
        public float dispersion = 50f;

        [SerializeField]
        private int projectilesPerShot = 1;

        [SerializeField]
        private AudioClip emptySFX = null, reloadSFX = null;

        public float shelloutForce = 25f;
        public Transform shellOuter;

        public ParticleSystem muzzleFlash;

        public ProjectileSlot[] projectiles = new ProjectileSlot[]
        {
            new ProjectileSlot( 15 )
        };

        public ProjectileSlot currentSlot { get { return projectiles[ projectileIndex ]; } }

        public bool isEmpty { get { return currentSlot.isEmpty; } }
        public bool isReloading { get; private set; }

        protected override bool checkAmmo { get { return isEmpty == false; } }


        int projectileIndex;
        Vector3 nativeOuterAngles;
        Light shotLight;

        AmmoBackpack m_AmmoBackpack { get { return m_Player.ammoBackpack; } }


        // Awake
        protected override void Awake()
        {
            base.Awake();

            projectileIndex = 0;

            if( muzzleFlash != null )
            {
                var main = muzzleFlash.main;
                main.loop = main.playOnAwake = false;
            }

            shotLight = GetComponentInChildren<Light>();

            if( shotLight != null ) {
                shotLight.enabled = false;
            }                

            if( projectileOuter != null ) {
                nativeOuterAngles = projectileOuter.localEulerAngles;
            }                
        }

        
        // OnEnable
        protected override void OnEnable()
        {
            base.OnEnable();
            isReloading = false;
        }


        // FirstStage Shooting
        protected override void FirstStageShooting()
        {
            if( owner.isPlayer )
            {
                if( isEmpty )
                {
                    if( autoReload == false || m_AmmoBackpack.IsEmpty( currentSlot.ammoType ) )
                    {
                        m_Audio.pitch = Time.timeScale;
                        m_Audio.PlayOneShot( emptySFX );
                        StartCoroutine( FireRate( .75f ) );
                    }
                    else
                    {
                        StartReloading();
                    }
                }
                else
                {
                    if( isReloading == false ) base.FirstStageShooting();
                }
            }
            else if( owner.isNPC )
            {
                SecondStageShooting();
            }
        }
        // SecondStage Shooting
        protected override void SecondStageShooting()
        {
            if( owner.isPlayer )
            {
                currentSlot.DecrementAmmo();
                m_Player.hud.weaponInformer.UpdateCurrentAmmoInfo( currentSlot.currentAmmo );
            }

            if( muzzleFlash != null && shotLight != null ) {
                StartCoroutine( PlayVisualEffects() );
            }                

            base.SecondStageShooting();
        }        
        // FinalStage Shooting
        protected override void FinalStageShooting()
        {
            if( currentSlot.projectile == null )
            {
                Debug.LogError( "Projectile is not setup in: " + name );
                return;
            }

            if( projectilesPerShot > 1 )
            {
                for( int i = 0; i < projectilesPerShot; i++ )
                {
                    OnSpawnProjectile();
                    MuzzleToCameraPoint();
                }
            }
            else
            {
                OnSpawnProjectile();
            }

            OnSpawnShell();

            base.FinalStageShooting();

            if( autoReload && owner.isPlayer && isEmpty && m_AmmoBackpack.IsEmpty( currentSlot.ammoType ) == false )
            {
                StartReloading();
            }  
        }


        // OnSpawn Projectile
        protected virtual void OnSpawnProjectile()
        {
            GameObject copyGO = Instantiate( currentSlot.projectile, projectileOuter.position, projectileOuter.rotation );

            IProjectile copy = copyGO.GetComponent<IProjectile>();

            if( copy != null ) {
                copy.OnSpawn( hitMask, owner, addDamage, addSpeed );
            }

            projectileOuter.localEulerAngles = nativeOuterAngles;
        }

        // OnSpawn Shell
        protected virtual void OnSpawnShell()
        {
            if( currentSlot.shell == null )
            {
                return;
            }

            if( shellOuter == null )
            {
                Debug.LogError( "Shell Outer is not setup! Error in: " + name );
                return;
            }

            GameObject copyGO = Instantiate( currentSlot.shell, shellOuter.position, Random.rotation );

            IShell copy = copyGO.GetComponent<IShell>();

            if( copy != null ) {
                copy.OnSpawn( shellOuter.forward * Random.Range( shelloutForce * .75f, shelloutForce * 1.25f ) / 45f );
            }            
        }


        // Muzzle ToCameraPoint
        protected override void MuzzleToCameraPoint()
        {
            base.MuzzleToCameraPoint();
            projectileOuter.Rotate( CalcDispersion( dispersion ) );
        }

        // Calc Dispersion
        public static Vector2 CalcDispersion( float dispersion )
        {
            return new Vector2
            {
                x = Random.Range( -dispersion, dispersion ) / 25f,
                y = Random.Range( -dispersion, dispersion ) / 20f
            };
        }


        // Play VisualEffects
        private IEnumerator PlayVisualEffects()
        {
            muzzleFlash.Stop();
            muzzleFlash.Play();

            shotLight.enabled = true;

            float lifetime = muzzleFlash.main.startLifetime.constant;

            for( float el = 0f; el < lifetime; el += Time.deltaTime ) {
                yield return null;
            }                

            shotLight.enabled = false;
        }

        // Reloading
        internal virtual void StartReloading()
        {
            if( isReloading == false && currentSlot.currentAmmo < maxAmmo && m_AmmoBackpack.IsEmpty( currentSlot.ammoType ) == false )
            {
                StartCoroutine( PlayReload() );
            }            
        }

        // Play Reload
        private IEnumerator PlayReload()
        {
            isReloading = true;

            if( m_FPWeaponSway != null )
            {
                m_FPWeaponSway.IronsightUnzoom();

                while( m_FPWeaponSway.ironsightZooming || m_FPWeaponSway.isPlaying )
                    yield return null;
            }                                    

            m_Audio.pitch = Time.timeScale;
            m_Audio.PlayOneShot( reloadSFX );

            if( m_FPWeaponSway != null )
            {
                m_FPWeaponSway.PlayReloadAnimation();                

                do yield return null;                
                while( m_FPWeaponSway.isPlaying );
            }

            int invAmmo = m_AmmoBackpack.GetCurrentAmmo( currentSlot.ammoType );

            if( m_AmmoBackpack.infiniteAmmo )
            {
                currentSlot.SetAmmo( maxAmmo );
            }
            else
            {
                int missingAmmo = Mathf.Max( 0, maxAmmo - currentSlot.currentAmmo );
                currentSlot.AddAmmo( Mathf.Min( Mathf.Max( 0, invAmmo ), missingAmmo ) );
                invAmmo = Mathf.Max( 0, invAmmo -= missingAmmo );

                m_AmmoBackpack.SetCurrentAmmo( currentSlot.ammoType, invAmmo );
            }

            m_Player.hud.weaponInformer.UpdateCurrentAmmoInfo( currentSlot.currentAmmo );
            m_Player.hud.weaponInformer.UpdateAllAmmoInfo( invAmmo );

            isReloading = false;
            m_SingleShotReady = false;
            m_ShotsNumber = 0;
        }


        // Switch ProjectileType
        internal bool SwitchProjectileType()
        {
            if( projectiles.Length < 2 )
            {
                return false;
            }                

            projectileIndex++;

            if( projectileIndex >= projectiles.Length ) {
                projectileIndex = 0;
            }

            m_Player.weaponsManager.UpdateHud();
            return true;
        } 
    };
}