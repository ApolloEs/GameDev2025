/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using System;
using UnityEngine;

namespace AdvancedShooterKit
{
    public abstract class DamageHandler : MonoBehaviour
    {
        [SerializeField]
        private EArmorType armorType = EArmorType.None;
        public EArmorType ArmorType
        {
            get { return armorType; }
            set { armorType = value; }
        }

        [SerializeField]
        protected string surfaceType = string.Empty;
        public string SurfaceType { get { return surfaceType; } }

        public virtual DamageInfo lastDamage { get; protected set; }

        public abstract bool isAlive { get; }

        public virtual bool isPlayer { get { return false; } }
        public virtual bool isNPC { get { return false; } }

        public PlayerCharacter castToPlayerCharacter { get { return this as PlayerCharacter; } }


        // Take Damage
        public virtual void TakeDamage( DamageInfo damageInfo )
        {
            lastDamage = damageInfo;

            if( damageInfo.owner.isPlayer == false )
            {
                return;
            }

            Action OnShowDmg = () => damageInfo.owner.castToPlayerCharacter.hud.ShowDamegeIndicator();

            switch( GameSettings.DamageIndication )
            {
                case EDamageIndication.OnlyCharacters:
                    if( isNPC )
                    {
                        OnShowDmg.Invoke();
                    }
                    break;
                case EDamageIndication.ForAll:
                    OnShowDmg.Invoke();
                    break;

                default:
                    break;
            }
        }

        // Calc Damage
        protected int CalcDamage( float damage )
        {
            return Mathf.RoundToInt( damage * getHardness * damageModifierByDifficulty );
        }

        // DamageModifier ByDifficulty
        protected virtual float damageModifierByDifficulty { get { return 1f; } }


        // Get Hardness
        private float getHardness
        {
            get
            {
                switch( armorType )
                {
                    case EArmorType.None: return    1f;
                    case EArmorType.Lite: return   .8f;
                    case EArmorType.Medium: return .65f;
                    case EArmorType.Heavy: return  .5f;
                    case EArmorType.Ultra: return  .35f;

                    default: Debug.LogError( "Invalid ArmorType in " + this.name ); return 1f;
                }
            }
        }
    };
}