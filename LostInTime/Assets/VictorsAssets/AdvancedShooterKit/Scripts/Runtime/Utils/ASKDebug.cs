/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AdvancedShooterKit.Utils
{
    public static class ASKDebug
    {
        static List<double> SW_Examples = new List<double>();


        // Create Primitive
        public static void CreatePrimitive( Vector3 position, PrimitiveType type, float lifetime, float size, Color color )
        {
            Transform objTr = GameObject.CreatePrimitive( type ).transform;
            objTr.localScale *= size;
            objTr.position = position;
            objTr.GetComponent<Renderer>().material.color = color;

            Object.Destroy( objTr.GetComponent<Collider>() );
            Object.Destroy( objTr.gameObject, lifetime );
        }


        // Start NewStopwatch
        public static Stopwatch StartNewStopwatch()
        {
            return Stopwatch.StartNew();
        }
        // StopAndPrint
        public static void StopAndPrint( this Stopwatch sw, string prefix = "", string postfix = "" )
        {
            sw.Stop();
            Debug.Log( prefix + sw.Elapsed.TotalMilliseconds.ToString( "f4" ) + postfix );
        }

        // StopAndPrintAverage
        public static void StopAndPrintAverage( this Stopwatch sw, string prefix = "", string postfix = "" )
        {
            sw.Stop();
            SW_Examples.Add( sw.Elapsed.TotalMilliseconds );
            double total = 0;
            SW_Examples.ForEach( v => total += v );
            Debug.Log( ( total / ( double )SW_Examples.Count ).ToString( "f4" ) + " | " + SW_Examples.Count );
        }

        // Clear StopwatchExamples
        public static void ClearStopwatchExamples()
        {
            SW_Examples.Clear();
        }



    };
}

