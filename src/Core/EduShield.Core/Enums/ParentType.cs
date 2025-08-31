namespace EduShield.Core.Enums;

/// <summary>
/// Defines the type of parent relationship
/// </summary>
public enum ParentType
{
    /// <summary>
    /// Primary parent (usually the main contact)
    /// </summary>
    Primary = 1,
    
    /// <summary>
    /// Secondary parent (spouse, partner)
    /// </summary>
    Secondary = 2,
    
    /// <summary>
    /// Guardian (legal guardian, not biological parent)
    /// </summary>
    Guardian = 3,
    
    /// <summary>
    /// Step parent
    /// </summary>
    StepParent = 4,
    
    /// <summary>
    /// Foster parent
    /// </summary>
    FosterParent = 5,
    
    /// <summary>
    /// Other type of relationship
    /// </summary>
    Other = 6
}


