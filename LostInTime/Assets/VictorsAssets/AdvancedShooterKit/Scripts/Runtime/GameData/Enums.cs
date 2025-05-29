/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


namespace AdvancedShooterKit
{
    // Difficulty Levels for gaymplay setups.
    public enum EDifficultyLevel : byte
    {
        Easy,
        Normal,
        Hard,
        Delta,
        Extreme
    };
    

    // Show Damege Indicator mode.
    public enum EDamageIndication : byte
    {
        OnlyCharacters,
        ForAll,
        OFF
    };

    
        
    // Using for sets ironsighting modes of first person
    public enum EIronsightingMode : byte
    {
        Press,
        Click,
        Mixed
    };

    // Using for sets Armor hardnes in "Damager.cs"
    public enum EArmorType : byte
    { 
        None, 
        Lite, 
        Medium, 
        Heavy, 
        Ultra 
    };
    
    
    // Setter VolumeType
    public enum EVolumeType : byte
    {
        Master,
        Music,
        SFX,
        Voice
    };
}
