/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    // Unsing for read/write damege data in combat
    public enum EDamageType : byte
    {
        Unknown,
        Impact,
        Melee,
        Explosion
    };

    public struct DamageInfo
    {
        public readonly float value;
        public readonly Transform source;
        public readonly Character owner;
        public readonly EDamageType type;


        // Constructor
        public DamageInfo( float value, Transform source, Character owner )
        {
            this.value = value;
            this.source = source;
            this.owner = owner;
            type = EDamageType.Unknown;
        }
        // Constructor
        public DamageInfo( float value, Transform source, Character owner, EDamageType type )
        {
            this.value = value;
            this.source = source;
            this.owner = owner;
            this.type = type;
        }
    };
}