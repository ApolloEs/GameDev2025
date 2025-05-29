/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;
using AdvancedShooterKit.Events;

namespace AdvancedShooterKit
{
    public class Pickup : MonoBehaviour
    {
        public enum EType : byte
        {
            Health,
            Melee,
            Firearms,
            Ammo,
            Thrown
        };


        [SerializeField, Range( 1, 100 )]
        private int dropChance = 75;
        

        [SerializeField]
        private EType pickupType = EType.Health;
        public EType PickupType
        {
            get { return pickupType; }
            set { pickupType = value; }
        }

        [SerializeField]
        [Range( .1f, 3f )]
        private float pickupDistance = 1.5f;
        [SerializeField]
        private AudioClip pickupSound = null;

        [SerializeField]
        private int amount = 10;
        public int Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        [SerializeField]
        private string ammoType = string.Empty;
        public string AmmoType
        {
            get { return ammoType; }
            set { ammoType = value; }
        }

        [SerializeField]
        private string weaponType = string.Empty;
        public string WeaponType
        {
            get { return weaponType; }
            set { weaponType = value; }
        }


        [SerializeField]
        private ASKEvent Pickuped = new ASKEvent();

        private Transform m_Transform = null;
        private int nativeAmount = 0;


        // Awake
        void Awake()
        {
            m_Transform = transform;
            nativeAmount = amount;
        }

        // OnRespawn
        void OnRespawn()
        {
            amount = nativeAmount;
        }


        void OnSpawn()
        {
            bool isLuck = ( Random.Range( 1, 101 ) <= dropChance );

            if( isLuck == false )
                Destroy( gameObject );
        }



        // Check Distance
        public bool CheckDistance( Transform playerTransform )
        {
            return ( Vector3.Distance( playerTransform.position, m_Transform.position ) <= pickupDistance );
        }

        // Pickup Item
        public void PickupItem( PlayerCharacter player )
        {
            Pickuped.Invoke();

            switch( pickupType )
            {
                case EType.Health:
                    if( player.IncrementHealth( amount ) )
                        DestroyIt();
                    break;

                case EType.Melee:
                case EType.Firearms:
                    if( player.weaponsManager.PickupWeapon( weaponType, ( pickupType == EType.Firearms ), amount ) )
                        DestroyIt();
                    else
                        PickupAmmo( player );
                    break;

                case EType.Ammo:
                    PickupAmmo( player );   
                    break;

                case EType.Thrown:
                    if( player.weaponsManager.PickupWeapon( weaponType, true, amount ) )
                    {
                        amount--;

                        if( amount > 0 )
                            PickupAmmo( player );
                        else
                            DestroyIt();
                    }
                    else
                    {
                        PickupAmmo( player );
                    }
                    break;
            }
        }

        // Ammo Pickup
        private void PickupAmmo( PlayerCharacter player )
        {
            if( player.ammoBackpack.AddAmmo( ammoType, ref amount ) )
            {
                player.weaponsManager.UpdateHud();

                Utils.ASKAudio.PlayClipAtPoint( pickupSound, m_Transform.position );

                if( pickupType == EType.Ammo || pickupType == EType.Thrown )
                    DestroyIt( false );
            }
        }


        // Destroy It
        private void DestroyIt( bool playSound = true )
        {
            if( playSound )
                Utils.ASKAudio.PlayClipAtPoint( pickupSound, m_Transform.position );
            
            Respawner.StartRespawn( gameObject );
        }
    }
}