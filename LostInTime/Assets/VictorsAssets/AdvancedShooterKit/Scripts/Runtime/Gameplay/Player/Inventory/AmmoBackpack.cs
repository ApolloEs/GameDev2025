/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    public class AmmoBackpack : MonoBehaviour
    {
        [System.Serializable]
        public struct Ammunition
        {
            public string name;
            public Sprite hudIcon;
            public int currentAmmo, maxAmmo;
        };

        public Ammunition[] ammunition = new Ammunition[ 1 ];


        public bool infiniteAmmo = false;


        // Ammunition array length
        public int size { get { return ammunition.Length; } }

        // IsFull
        public bool IsFull( string ammoName )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 )
            {
                return ammunition[ index ].currentAmmo >= ammunition[ index ].maxAmmo;
            }

            return true;
        }
        // IsEmpty
        public bool IsEmpty( string ammoName )
        {
            if( infiniteAmmo )
            {
                return false;
            }

            int index = FindIndex( ammoName );

            if( index >= 0 )
            {
                return ammunition[ index ].currentAmmo <= 0;
            }

            return false;
        }

        // Add Ammo
        public bool AddAmmo( string ammoName, ref int addАmount )
        {
            if( FindIndex( ammoName ) < 0 )
            {
                return false;
            }

            int currentAmmo = GetCurrentAmmo( ammoName );
            int maxAmmo = GetMaxAmmo( ammoName );

            if( currentAmmo >= maxAmmo )
                return false;

            int missingAmmo = Mathf.Max( 0, maxAmmo - currentAmmo );
            currentAmmo += Mathf.Min( Mathf.Max( 0, addАmount ), missingAmmo );
            addАmount = Mathf.Max( 0, addАmount -= missingAmmo );
            SetCurrentAmmo( ammoName, currentAmmo );
            return true;
        }

        // Get CurrentAmmo
        public int GetCurrentAmmo( string ammoName )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 )
            {
                return ammunition[ index ].currentAmmo;
            }

            return 0;
        }
        // Set CurrentAmmo
        public void SetCurrentAmmo( string ammoName, int count )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 )
            {
                count = Mathf.Min( GetMaxAmmo( ammoName ), Mathf.Max( 0, count ) );
                ammunition[ index ].currentAmmo = count;
            }            
        }

        // Get MaxAmmo
        public int GetMaxAmmo( string ammoName )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 ) {
                return ammunition[ index ].maxAmmo;
            }

            return 0;
        }
        // Set MaxAmmo
        public void SetMaxAmmo( string ammoName, int count )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 ) {
                ammunition[ index ].maxAmmo = Mathf.Max( 0, count );
            }         
        }

        // Get HudIcon
        public Sprite GetHudIcon( string ammoName )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 )
            {
                return ammunition[ index ].hudIcon;
            }

            return null;
        }
        // Set HudIcon
        public void SetHudIcon( string ammoName, Sprite icon )
        {
            int index = FindIndex( ammoName );

            if( index >= 0 )
                ammunition[ index ].hudIcon = icon;            
        }


        // FindIndex
        int FindIndex( string ammoName )
        {
            string[] names = GetNames();

            for( int i = 0; i < names.Length; i++ )
            {
                if( names[ i ] == ammoName ) return i;
            }

            Debug.LogError( "Incorrect ammo name: " + ammoName );
            return -1;
        }        

        // GetNames
        public string[] GetNames()
        {
            string[] names = new string[ ammunition.Length ];

            for( int i = 0; i < names.Length; i++ )
            {
                names[ i ] = ammunition[ i ].name;
            }

            return names;
        }
    };
}