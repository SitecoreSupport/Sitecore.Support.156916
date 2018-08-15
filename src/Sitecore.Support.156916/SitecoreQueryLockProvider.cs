namespace Sitecore.Data.Locking
{  
  using Sitecore.Data.Items;

  /// <summary>
  /// Locates items locked by user using Sitecore Query.
  /// </summary>
  /// <seealso cref="Sitecore.Data.Locking.UserLockProvider" />
  public class SitecoreQueryLockProvider : UserLockProvider
  {
    /// <summary>
    /// Gets the items locked by user.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public override void GetItemsLockedByUser(UserLockedItemArgs args)
    {
      var items = args.ContentDatabase.SelectItems(@"search://*[@__lock='%""" + args.User.Name + @"""%']");

      foreach (var item in items ?? new Item[0])
      {
        args.Result.Add(item);
      }      
    }
  }
}
