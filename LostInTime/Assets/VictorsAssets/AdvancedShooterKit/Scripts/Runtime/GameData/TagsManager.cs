/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/

using UnityEngine;

namespace AdvancedShooterKit
{
    public static class TagsManager
    {
        public const string MainCamera = "MainCamera";
        //
        public const string Player = "Player";
        public const string NPC = "ASK/NonPlayerCharacter";
        //
        public const string Pickup = "ASK/Pickup";


        // IsMainCamera
        public static bool IsMainCamera( this GameObject obj )
        {
            return obj.tag == MainCamera;
        }
        public static bool IsMainCamera( this Component comp )
        {
            return comp.tag == MainCamera;
        }

        // IsPlayer
        public static bool IsPlayer( this GameObject obj )
        {
            return obj.tag == Player;
        }
        public static bool IsPlayer( this Component comp )
        {
            return comp.tag == Player;
        }
        // IsNPC
        public static bool IsNPC( this GameObject obj )
        {
            return obj.tag == NPC;
        }
        public static bool IsNPC( this Component comp )
        {
            return comp.tag == NPC;
        }

        // IsPickup
        public static bool IsPickup( this GameObject obj )
        {
            return obj.tag == Pickup;
        }
        public static bool IsPickup( this Component comp )
        {
            return comp.tag == Pickup;
        }
    }
}