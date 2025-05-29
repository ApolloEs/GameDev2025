/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    public class CustomSurface : MonoBehaviour
    {
        [SerializeField]
        private string m_SurfaceName;

        public string surfaceName { get { return m_SurfaceName; } }
    };
}
