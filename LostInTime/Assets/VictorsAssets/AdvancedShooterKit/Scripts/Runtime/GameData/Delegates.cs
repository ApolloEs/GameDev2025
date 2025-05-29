/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine.Events;

namespace AdvancedShooterKit.Events
{
    public delegate void ActionHandler();
    public delegate void AxisHandler( float value );
    //
    [System.Serializable] public class ASKEvent : UnityEvent { }
}