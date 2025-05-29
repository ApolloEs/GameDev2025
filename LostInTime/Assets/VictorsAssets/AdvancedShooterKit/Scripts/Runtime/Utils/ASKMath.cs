/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;

namespace AdvancedShooterKit.Utils
{
    public static class ASKMath
    {
        public const float HALF_PI = Mathf.PI * .5f;
        public const float DOUBLE_PI = Mathf.PI * 2f;


        // SnapToZero
        public static float SnapToZero( float value, float epsilon = .0001f )
        {
            return ( Mathf.Abs( value ) < epsilon ) ? 0f : value;
        }

        // Round
        public static double Round( double value, int digits )
        {
            return Math.Round( value, digits );
        }
        // Round
        public static float Round( float value, int digits )
        {
            return ( float )Math.Round( value, digits );
        }

        // Persent01
        public static float Persent01( float src, float percent )
        {
            return 1f - ( 1f - src ) * Math.Abs( percent );
        }
    };
}
