namespace Firebend.AutoCrud.ChangeTracking.Models;

/// <summary>
/// Encapsulates data pertaining to a entity change.
/// </summary>
public class ChangeTrackingContext
{
    /// <summary>
    /// Gets or sets a value indicating the user's email address that made the change.
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating where the change originated from.
    /// </summary>
    public string Source { get; set; }
}
