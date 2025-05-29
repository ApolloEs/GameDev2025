/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


namespace AdvancedShooterKit
{
    public interface IProjectile
    {
        void OnSpawn( UnityEngine.LayerMask hitMask, Character owner, float addDamage, float addSpeed );
    };
}
