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
    public class WeaponsManager : MonoBehaviour
    {
        public enum EWeaponType : byte
        {
            Standart,
            Keep,
            Thrown
        };


        [System.Serializable]
        struct WeaponSlot
        {
            //public string name;
            public bool available;
            public EWeaponType type;
            public WeaponBase weapon, subWeapon;
            public Rigidbody dropout;
        }

        [SerializeField]
        [Range( 1, 10 )]
        private int maxWeapons = 2;

        [SerializeField]
        private AudioClip
            switchWeapon = null,
            switchShootingMode = null,
            subWeaponOnOff = null,
            switchProjectile = null;

        [SerializeField]
        private EIronsightingMode ironsightingMode = EIronsightingMode.Mixed;
        internal EIronsightingMode IronsightingMode { get { return ironsightingMode; } }

        [SerializeField]
        private WeaponSlot[] weapons = new WeaponSlot[ 1 ];


        GameObject weaponGo;
        WeaponBase m_Weapon;
        Firearms m_Firearm;

        int weaponIndex = -1, weaponsCount;
        bool isSubweapon;
        private bool isMelee { get { return ( m_Firearm == null ); } }
        AudioSource m_Audio;

        PlayerCharacter m_Player;
        HudElements m_Hud { get { return m_Player.hud; } }
        AmmoBackpack m_AmmoBackpack { get { return m_Player.ammoBackpack; } }

        FirstPersonWeaponSway m_FPWeapSway;


        // Awake
        void Awake()
        {
            weaponIndex = -1;
            weaponsCount = 0;
            isSubweapon = false;

            maxWeapons = Mathf.Clamp( 1, maxWeapons, weapons.Length );

            m_Audio = GetComponent<AudioSource>();
            m_Audio.outputAudioMixerGroup = GameSettings.SFXOutput;

            m_Player = GetComponentInParent<PlayerCharacter>();
        }

        // Start
        void Start()
        {
            UpdateRefs( 0 );

            InitWeapons();

            UpdateHud();
            UpdateInformerAndCrosshair();
        }

        // OnDisable
        void OnDisable()
        {
            StopCoroutine( "ChangeWeapon" );

            if( m_FPWeapSway != null )
            {
                m_FPWeapSway.FullReset();
            }
        }


        // UpdateRefs
        private void UpdateRefs( int weapIndex )
        {
            m_Weapon = weapons[ weapIndex ].weapon;
            weaponGo = m_Weapon.gameObject;
            m_Firearm = m_Weapon as Firearms;
            m_FPWeapSway = weaponGo.GetComponentInChildren<FirstPersonWeaponSway>( true );

            m_Player.getCamera.SetCurrentFPWeapSway( m_FPWeapSway );
            m_Player.fpController.SetCurrentFPWeapSway( m_FPWeapSway );
        }


        // Init Weapons
        private void InitWeapons()
        {
            for( int i = 0; i < weapons.Length; i++ )
            {
                GameObject currentGO = weapons[ i ].weapon.gameObject;

                currentGO.SetActive( true );

                if( weapons[ i ].type == EWeaponType.Thrown )
                {
                    Firearms tmpFA = weapons[ i ].weapon as Firearms;

                    if( tmpFA.isEmpty == false || m_AmmoBackpack.IsEmpty( tmpFA.currentSlot.ammoType ) == false )
                    {
                        weapons[ i ].available = true;
                    }
                    else if( m_AmmoBackpack.IsEmpty( tmpFA.currentSlot.ammoType ) && tmpFA.isEmpty )
                    {
                        weapons[ i ].available = false;
                    }
                }

                if( weaponsCount >= maxWeapons && weapons[ i ].type == EWeaponType.Standart )
                {
                    DropWeapon( i );
                }
                else if( weapons[ i ].available && weapons[ i ].type == EWeaponType.Standart )
                {
                    weaponsCount++;
                }

                currentGO.SetActive( false );
            }

            for( int i = weaponIndex; i < weapons.Length; i++ )
            {
                if( i != weaponIndex && weapons[ i ].available )
                {
                    weaponIndex = i;
                    StartCoroutine( ChangeWeapon() );
                    break;
                }
            }
        }




        // bespoken
        private bool bespoken
        {
            get
            {
                bool result = ( ( m_Weapon == null ) || ( isMelee == false && m_Firearm.isReloading ) || m_Player.isAlive == false );

                result |= ( m_FPWeapSway != null && ( m_FPWeapSway.ironsightZooming || m_FPWeapSway.isChanging ) );

                return result;
            }
        }


        // weapons array length
        public int size { get { return weapons.Length; } }


        public bool haveAnyWeapon { get { return ( weaponIndex >= 0 ); } }

        // Get CurrentWeapon PointMode
        public ECrosshair.EPointMode GetCurrentWeaponPointMode( out ECrosshair.EView view )
        {
            var mode = ECrosshair.EPointMode.BigPoint;

            if( haveAnyWeapon && m_FPWeapSway != null )
            {
                view = m_FPWeapSway.crosshairView;
                if( view == ECrosshair.EView.ALL )
                    mode = ECrosshair.EPointMode.SmallPoint;
            }
            else
            {
                view = ECrosshair.EView.Point;
            }

            return mode;
        }



        // Weapon Fire
        internal void WeaponFire()
        {
            if( bespoken || weaponIndex < 0 )
                return;

            m_Weapon.StartShooting();
        }

        // Weapon Reset
        internal void WeaponReset()
        {
            if( bespoken || weaponIndex < 0 )
                return;

            m_Weapon.WeaponReset();
        }


        // Drop Current Weapon
        internal void DropCurrentWeapon()
        {
            if( bespoken )
            {
                return;
            }

            int newInd = -1;

            for( int i = 0; i < size; i++ )
            {
                if( i != weaponIndex && weapons[ i ].available )
                {
                    newInd = i;
                    break;
                }
            }

            DropWeapon( weaponIndex, newInd );
        }

        // Reload Weapon
        internal void ReloadWeapon()
        {
            if( bespoken || weaponIndex < 0 || isMelee )
                return;

            m_Firearm.StartReloading();
        }

        // Switch To SubWeapon
        internal void SwitchToSubWeapon()
        {
            if( bespoken || weaponIndex < 0 || isMelee || weapons[ weaponIndex ].subWeapon == null )
                return;

            if( isSubweapon )
            {
                m_Weapon = weapons[ weaponIndex ].weapon;
                isSubweapon = false;
            }
            else
            {
                m_Weapon = weapons[ weaponIndex ].subWeapon;
                isSubweapon = true;
            }

            m_Firearm = m_Weapon as Firearms;
            m_Audio.PlayOneShot( subWeaponOnOff );
            UpdateHud();
        }

        // Switch to Next Firemode
        internal void SwitchFiremode()
        {
            if( !bespoken && haveAnyWeapon && !isMelee && m_Weapon.SwitchShootingMode() )
            {
                m_Audio.PlayOneShot( switchShootingMode );
            }
        }

        // Switch to Next AmmoType
        internal void SwitchAmmotype()
        {
            if( !bespoken && haveAnyWeapon && !isMelee && m_Firearm.SwitchProjectileType() )
                m_Audio.PlayOneShot( switchProjectile );
        }


        // Select Weapon ByIndex
        internal void SelectWeaponByIndex( int index )
        {
            if( bespoken
                || ( weaponIndex == index )
                || m_Weapon.weaponReady == false
                || ( index < 0 )
                || ( index > size - 1 ) )
                return;

            if( weapons[ index ].available )
            {
                weaponIndex = index;
                StartCoroutine( ChangeWeapon() );
            }
        }

        // Select Previous Weapon
        internal void SelectPreviousWeapon()
        {
            if( bespoken || m_Weapon.weaponReady == false )
                return;

            // Find Previous Weapon
            for( int i = weaponIndex; i > -1; i-- )
                if( i != weaponIndex && weapons[ i ].available )
                {
                    weaponIndex = i;
                    StartCoroutine( ChangeWeapon() );
                    return;
                }

            // Find Last Weapon
            for( int i = size - 1; i > -1; i-- )
                if( i != weaponIndex && weapons[ i ].available )
                {
                    weaponIndex = i;
                    StartCoroutine( ChangeWeapon() );
                    break;
                }
        }

        // Select Next Weapon
        internal void SelectNextWeapon()
        {
            if( bespoken || !m_Weapon.weaponReady )
                return;

            // Find Next Weapon
            for( int i = weaponIndex; i < size; i++ )
                if( i != weaponIndex && weapons[ i ].available )
                {
                    weaponIndex = i;
                    StartCoroutine( ChangeWeapon() );
                    return;
                }

            // Find First Weapon
            for( int i = 0; i < size; i++ )
                if( i != weaponIndex && weapons[ i ].available )
                {
                    weaponIndex = i;
                    StartCoroutine( ChangeWeapon() );
                    break;
                }
        }

        // ChangeEmpty to FirstAvaliable
        internal void ChangeEmptyToFirstAvaliable()
        {
            if( m_FPWeapSway.isChanging == false && m_AmmoBackpack.IsEmpty( m_Firearm.currentSlot.ammoType ) )
            {
                if( weapons[ weaponIndex ].type == EWeaponType.Thrown )
                {
                    weapons[ weaponIndex ].available = false;
                    FindFirstAvailableWeapon();
                    CalculateWeaponsCount();
                    StartCoroutine( ChangeWeapon() );
                }
                else
                {
                    if( FindFirstAvailableWeapon() )
                        StartCoroutine( ChangeWeapon() );
                }
            }
        }

        // Change Weapon
        private IEnumerator ChangeWeapon()
        {
            while( m_FPWeapSway.isPlaying )
                yield return null;

            if( m_FPWeapSway.ironsightZoomed )
            {
                m_FPWeapSway.IronsightUnzoom();

                while( m_FPWeapSway.ironsightZooming )
                    yield return null;
            }

            if( weaponGo.activeSelf )
            {
                if( weaponsCount > 0 )
                    m_Audio.PlayOneShot( switchWeapon );

                m_FPWeapSway.DropoutAnimation();
            }

            while( m_FPWeapSway.isChanging )
                yield return null;

            weaponGo.SetActive( false );

            if( haveAnyWeapon )
            {
                isSubweapon = false;

                UpdateRefs( weaponIndex );

                weaponGo.SetActive( true );
                m_FPWeapSway.DropinAnimation();
            }

            UpdateHud( true );
        }


        // Pickup Weapon
        internal bool PickupWeapon( string weapName, bool isFArms = false, int ammoAmount = 0 )
        {
            int index = FindIndex( weapName );

            if( index < 0 || weapons[ index ].available )
            {
                return false;
            }

            weapons[ index ].available = true;

            if( isFArms )
            {
                Firearms faWeap = weapons[ index ].weapon as Firearms;
                faWeap.currentSlot.SetAmmo( Mathf.Min( faWeap.maxAmmo, ammoAmount ) );
            }

            if( weaponsCount < maxWeapons )
            {
                if( weaponIndex < 0 )
                {
                    weaponIndex = index;
                    StartCoroutine( ChangeWeapon() );
                }

                CalculateWeaponsCount();
            }
            else
            {
                if( weapons[ index ].type == EWeaponType.Standart )
                {
                    DropWeapon( weaponIndex, index );
                }
            }

            return true;
        }

        // Drop Weapon
        private void DropWeapon( int index, int newIndex = -1 )
        {
            if( index < 0 )
            {
                for( int i = 0; i < size; i++ )
                {
                    if( newIndex != i && weapons[ i ].available && weapons[ i ].type == EWeaponType.Standart )
                    {
                        DropWeapon( i, newIndex );
                        return;
                    }
                }

                for( int i = 0; i < size; i++ )
                {
                    if( weapons[ i ].available && weapons[ i ].type == EWeaponType.Standart )
                    {
                        DropWeapon( i );
                        return;
                    }
                }

                return;
            }


            if( weaponsCount < 1 || weapons[ index ].available == false )
                return;

            if( weapons[ index ].type != EWeaponType.Standart )
            {
                for( int i = 0; i < size; i++ )
                {
                    if( newIndex != i && weapons[ i ].available && weapons[ i ].type == EWeaponType.Standart )
                    {
                        DropWeapon( i );
                        return;
                    }
                }

                for( int i = 0; i < size; i++ )
                {
                    if( weapons[ i ].available && weapons[ i ].type == EWeaponType.Standart )
                    {
                        DropWeapon( i );
                        return;
                    }
                }
            }

            if( weapons[ index ].dropout != null )
            {
                Transform playerTR = m_Player.getTransform;
                Rigidbody droppedWeapon = weapons[ index ].dropout.SpawnCopy( playerTR.forward * .2f + m_Player.getCamera.getTransform.position + Vector3.up * .1f, Random.rotation );
                droppedWeapon.AddForce( playerTR.forward * ( 4f * droppedWeapon.mass ), ForceMode.Impulse );
                droppedWeapon.AddRelativeTorque( Vector3.up * ( Random.value > .5f ? 15f : -15f ), ForceMode.Impulse );

                Firearms faWeap = weapons[ index ].weapon as Firearms;
                if( faWeap != null )
                {
                    Pickup dropPickup = droppedWeapon.GetComponent<Pickup>();
                    if( dropPickup != null )
                    {
                        dropPickup.Amount = faWeap.currentSlot.currentAmmo;
                    }
                    else
                    {
                        dropPickup = droppedWeapon.gameObject.AddComponent<Pickup>();
                        dropPickup.Amount = faWeap.currentSlot.currentAmmo;
                        dropPickup.WeaponType = weapons[ index ].weapon.name;
                        dropPickup.AmmoType = faWeap.currentSlot.ammoType;
                        dropPickup.PickupType = Pickup.EType.Firearms;
                    }
                }
            }
            else
            {
                Debug.LogError( "Dropout item is null! Item index in weapons: " + index );
            }


            weapons[ index ].weapon.gameObject.SetActive( false );
            weapons[ index ].available = false;

            if( newIndex > -1 )
            {
                weaponIndex = newIndex;
                StartCoroutine( ChangeWeapon() );
            }

            CalculateWeaponsCount();

            if( weaponsCount < maxWeapons )
                m_Audio.PlayOneShot( switchWeapon );

            UpdateHud( true );
        }

        // Find FirstAvailable Weapon
        private bool FindFirstAvailableWeapon()
        {
            for( int i = 0; i < size; i++ )
            {
                if( i != weaponIndex && weapons[ i ].available )
                {
                    weaponIndex = i;
                    return true;
                }
            }

            return false;
        }


        // Calculate Weapons Count
        private void CalculateWeaponsCount()
        {
            weaponsCount = 0;

            for( int i = 0; i < size; i++ )
            {
                if( weapons[ i ].available && weapons[ i ].type == EWeaponType.Standart )
                    weaponsCount++;
            }

            if( weaponsCount < 1 && !weapons[ weaponIndex ].available )
                weaponIndex = -1;

            UpdateHud();
        }


        // Weapon isAvailable
        internal bool WeaponIsAvailable( string weapName )
        {
            int index = FindIndex( weapName );

            if( index < 0 )
                return false;

            return weapons[ index ].available;
        }

        // Crowded
        internal bool crowded
        {
            get { return weaponsCount >= maxWeapons; }
        }

        // WeaponType isStandart
        internal bool WeaponTypeIsStandart( string weapName )
        {
            int index = FindIndex( weapName );

            if( index < 0 )
                return false;

            return weapons[ index ].type == EWeaponType.Standart;
        }


        // Update Hud
        public void UpdateHud( bool updateAll = false )
        {
            if( m_Player.isAlive == false )
            {
                return;
            }

            if( isMelee == false )
            {
                m_Hud.crosshair.UpdatePosition( m_Firearm.dispersion );

                m_Hud.weaponInformer.UpdateAmmoIcon( m_AmmoBackpack.GetHudIcon( m_Firearm.currentSlot.ammoType ) );
                m_Hud.weaponInformer.UpdateCurrentAmmoInfo( m_Firearm.currentSlot.currentAmmo );
                m_Hud.weaponInformer.UpdateAllAmmoInfo( m_AmmoBackpack.GetCurrentAmmo( m_Firearm.currentSlot.ammoType ) );
                m_Hud.weaponInformer.UpdateShootingModeIcon( m_Weapon.firingMode );
            }

            if( updateAll )
            {
                UpdateInformerAndCrosshair();
            }
        }

        // Update InformerAndCrosshair
        private void UpdateInformerAndCrosshair()
        {
            ECrosshair.EView view;
            m_Hud.crosshair.SetPointSprite( GetCurrentWeaponPointMode( out view ) );

            if( m_FPWeapSway != null && haveAnyWeapon == false )
            {
                m_FPWeapSway.moveSpeed = 1f;
            }

            m_Hud.crosshair.SetActive( view );
            m_Hud.weaponInformer.SetActive( weaponIndex >= 0 && isMelee == false );
        }



        // FindIndex
        int FindIndex( string weapName )
        {
            string[] names = GetNames();

            for( int i = 0; i < names.Length; i++ )
            {
                if( names[ i ] == weapName )
                {
                    return i;
                }
            }

            Debug.LogError( "ERR: incorrect weapon name: " + weapName );
            return -1;
        }


        // GetNames
        public string[] GetNames()
        {
            string[] names = new string[size];

            for (int i = 0; i < names.Length; i++)
            {
                var weap = weapons[i].weapon;
                names[i] = (weap != null) ? weap.name : string.Empty;
            }

            return names;
        }
    };
}