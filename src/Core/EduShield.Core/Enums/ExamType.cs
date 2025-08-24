namespace EduShield.Core.Enums;

/// <summary>
/// Defines the different types of exams in the EduShield system
/// </summary>
public enum ExamType
{
    /// <summary>
    /// Unit test or quiz
    /// </summary>
    UnitTest = 0,

    /// <summary>
    /// Mid-term examination
    /// </summary>
    MidTerm = 1,

    /// <summary>
    /// Final examination
    /// </summary>
    Final = 2,

    /// <summary>
    /// Assignment or project
    /// </summary>
    Assignment = 3,

    /// <summary>
    /// Laboratory work
    /// </summary>
    Laboratory = 4,

    /// <summary>
    /// Presentation or oral exam
    /// </summary>
    Presentation = 5,

    /// <summary>
    /// Continuous assessment
    /// </summary>
    ContinuousAssessment = 6,

    /// <summary>
    /// Other type of assessment
    /// </summary>
    Other = 7
}
