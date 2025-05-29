/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdvancedShooterKit
{
    //using Utils;

    public sealed class SurfaceDetector : ScriptableObject
    {
        [Serializable]
        struct SData
        {
            public string surfaceName;
            public Object targetObject;

            public int GetHash()
            {
                return ( targetObject != null ) ? targetObject.GetHashCode() : 0;
            }
        };

        [SerializeField]
        private SData[]
            m_Materials = new SData[ 0 ]
            , m_Textures = new SData[ 0 ];


        struct SInfo
        {
            public SInfo( string surfaceName, int hashCode )
            {
                this.surfaceName = surfaceName;
                this.hashCode = hashCode;
            }

            public readonly string surfaceName;
            public readonly int hashCode;
        };

        [NonSerialized] bool inited;
        static SInfo[] materialsInfo, texturesInfo;



        [SerializeField]
        private string[] m_Names = new string[ 0 ];

        public static string[] allNames { get { return instance.m_Names; } }
        public static int count { get { return instance.m_Names.Length; } }


        public const string UNKNOWN = "UNKNOWN";


        // instance
        static SurfaceDetector rawInstance;
        // m_Instance
        private static SurfaceDetector instance
        {
            get
            {
                if( rawInstance == null )
                {
                    rawInstance = Resources.Load<SurfaceDetector>( typeof( SurfaceDetector ).Name );
                }

                return rawInstance;
            }
        }

        // isValid
        private static bool isValid { get { return instance != null; } }
        private static void PrintInvalidMessage()
        {
            Debug.LogError( "SurfaceDetector instance not found. Please create it from [Tools->VictorsAssets->AdvancedShooterKit->Settings]" );
        }


        // CheckInit
        private static void CheckInit()
        {
            if( instance.inited == false )
            {
                instance.inited = true;
                WhriteInfo( instance.m_Materials, out materialsInfo );
                WhriteInfo( instance.m_Textures, out texturesInfo );
            }
        }

        // Whrite Hashes
        private static void WhriteInfo( SData[] source, out SInfo[] target )
        {
            target = new SInfo[ source.Length ];

            for( int i = 0; i < source.Length; i++ )
            {
                target[ i ] = new SInfo( source[ i ].surfaceName, source[ i ].GetHash() );
            }

            Array.Sort( target, ( x, y ) => x.hashCode.CompareTo( y.hashCode ) );
        }


        // Get SurfaceName ByHit
        public static string GetSurface( RaycastHit hit )
        {
            if( isValid == false )
            {
                PrintInvalidMessage();
                return UNKNOWN;
            }


            if( hit.collider == null || hit.collider.isTrigger )
            {
                return UNKNOWN;
            }

            CheckInit();

            int surIndex = FindIndex( materialsInfo, hit.GetMaterial() ) ?? -1;
            if( surIndex >= 0 )
            {
                return materialsInfo[ surIndex ].surfaceName;
            }

            surIndex = FindIndex( texturesInfo, hit.GetTerrainTexture() ) ?? -1;
            if( surIndex >= 0 )
            {
                return texturesInfo[ surIndex ].surfaceName;
            }
            
            CustomSurface custom = hit.collider.GetComponent<CustomSurface>();
            if( custom != null ) return custom.surfaceName;

            return UNKNOWN;
        }

        // Get SurfaceName ByMaterial
        public static string GetSurface( Material meshMaterial )
        {
            if( isValid == false )
            {
                PrintInvalidMessage();
                return UNKNOWN;
            }

            CheckInit();

            int surIndex = FindIndex( materialsInfo, meshMaterial ) ?? -1;
            if( surIndex >= 0 )
            {
                return materialsInfo[ surIndex ].surfaceName;
            }

            return UNKNOWN;
        }

        // Get SurfaceName ByTexture
        public static string GetSurface( Texture terrainTexture )
        {
            if( isValid == false )
            {
                PrintInvalidMessage();
                return UNKNOWN;
            }

            CheckInit();

            int surIndex = FindIndex( texturesInfo, terrainTexture ) ?? -1;
            if( surIndex >= 0 )
            {
                return texturesInfo[ surIndex ].surfaceName;
            }

            return UNKNOWN;
        }


        // Try GetSurface ByHit
        public static bool TryGetSurface( RaycastHit hit, out string surface )
        {
            surface = GetSurface( hit );
            return ( surface != UNKNOWN );
        }

        // Try GetSurface ByHit
        public static bool TryGetSurface( Material meshMaterial, out string surface )
        {
            surface = GetSurface( meshMaterial );
            return ( surface != UNKNOWN );
        }

        // Try GetSurface ByHit
        public static bool TryGetSurface( Texture terrainTexture, out string surface )
        {
            surface = GetSurface( terrainTexture );
            return ( surface != UNKNOWN );
        }



        // FindIndex
        private static int? FindIndex( SInfo[] array, Object item )
        {
            if( item == null || array.Length == 0 )
            {
                return null;
            }

            int first = 0;
            int last = array.Length;
            int itemHash = item.GetHashCode();

            if( itemHash >= array[ first ].hashCode && itemHash <= array[ last - 1 ].hashCode )
            {
                while( first < last )
                {
                    int mid = first + ( last - first ) / 2;

                    if( itemHash <= array[ mid ].hashCode )
                    {
                        last = mid;
                    }
                    else
                    {
                        first = mid + 1;
                    }
                }

                if( array[ last ].hashCode == itemHash )
                {
                    return last;
                }
            }

            return null;
        }
    };
}