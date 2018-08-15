namespace Sitecore.Data.Locking
{
  using System.Collections.Generic;
  using System.Security.Principal;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines;

  /// <summary>
  /// Carries arguments to resolve items locked by <see cref="IIdentity"/> in specific <see cref="Database"/>.  
  /// </summary>
  /// <seealso cref="Sitecore.Pipelines.PipelineArgs" />
  public class UserLockedItemArgs : PipelineArgs
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserLockedItemArgs"/> class.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="contentDatabase">The content database.</param>
    public UserLockedItemArgs([NotNull] IIdentity user, [NotNull] Database contentDatabase)
    {
      Assert.ArgumentNotNull(user, nameof(user));

      Assert.ArgumentNotNull(contentDatabase, nameof(contentDatabase));
      this.User = user;
      this.ContentDatabase = contentDatabase;

      Result = new List<Item>();
    }

    /// <summary>
    /// Gets the user to find locked items for.
    /// </summary>
    /// <value>
    /// The user.
    /// </value>
    [NotNull]
    public IIdentity User { get; private set; }

    /// <summary>
    /// Gets the content database to find locked items in.
    /// </summary>
    /// <value>
    /// The content database.
    /// </value>
    [NotNull]
    public Database ContentDatabase { get; private set; }

    /// <summary>
    /// Gets the result.
    /// </summary>
    /// <value>
    /// The result.
    /// </value>
    [NotNull]
    public IList<Item> Result { get; private set; }
  }
}
