/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit.Utils
{
    public static class LevelCache
    {
        static Transform m_Cache;
        private static Transform cache
        {
            get
            {
                if( m_Cache == null ) {
                    m_Cache = new GameObject( "ASK_LevelCache" ).transform;
                }                    

                return m_Cache;
            }
        }


        // Move ToCache
        public static T MoveToCache<T>( this T obj ) where T : Component
        {
            obj.transform.SetParent( cache );
            return obj;
        }

        // Move ToCache
        public static GameObject MoveToCache( this GameObject obj )
        {
            obj.transform.SetParent( cache );
            return obj;
        }
    };
}