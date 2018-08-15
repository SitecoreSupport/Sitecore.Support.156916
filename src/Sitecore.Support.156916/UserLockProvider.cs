namespace Sitecore.Data.Locking
{
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore.Abstractions;
  using Sitecore.DependencyInjection;
  using Sitecore.Support.Data.Locking;

  /// <summary>
  /// Defines a logic to find items
  /// </summary>  
  public abstract class UserLockProvider
  {
    /// <summary>
    /// Gets or sets the instance of the provider responsible for locating locked items solution-wide.
    /// </summary>
    /// <value>
    /// The instance.
    /// </value>
    [NotNull]
    public static UserLockProvider Instance { get; set; } = new DirectSqlQuery(ServiceLocator.ServiceProvider.GetRequiredService<BaseItemManager>());

    /// <summary>
    /// Gets the items locked by user.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public abstract void GetItemsLockedByUser([NotNull] UserLockedItemArgs args);
  }
}
