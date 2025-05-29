/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEditor;

namespace AdvancedShooterKitEditor
{
    public sealed class ASKEditorChangeCheck : IDisposable
    {
        public Action OnChangeCheck = () => { };


        // Constructor
        public ASKEditorChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }

        // Constructor
        public ASKEditorChangeCheck( Action OnChange )
        {
            OnChangeCheck = OnChange;
            EditorGUI.BeginChangeCheck();
        }



        // Dispose
        public void Dispose()
        {
            if( EditorGUI.EndChangeCheck() ) {
                OnChangeCheck.Invoke();
            }                
        }
    };
}
