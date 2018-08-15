namespace Sitecore.Support.Data.Locking
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.SqlClient;
  using Sitecore.Abstractions;
  using Sitecore.Data;
  using Sitecore.Data.Locking;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;

  public class DirectSqlQuery : UserLockProvider
  {
    private readonly string sqlBody = $"SELECT [ItemId], [Version], [Language] FROM [VersionedFields] WHERE [FieldId]='{FieldIDs.Lock}' AND CHARINDEX(@lockOwner, [Value])=1 ORDER BY [ItemId]";

    private readonly BaseItemManager manager;

    public DirectSqlQuery([NotNull]BaseItemManager manager)
    {
      Assert.ArgumentNotNull(manager, nameof(manager));
      this.manager = manager;
    }

    public override void GetItemsLockedByUser(UserLockedItemArgs args)
    {
      var db = args.ContentDatabase;
      var connectionStringName = db.ConnectionStringName;

      var connectionString = Assert.ResultNotNull(ConfigurationManager.ConnectionStrings[connectionStringName]).ConnectionString;

      var lockedItemUris = new List<DataUri>();
      using (SqlConnection connection = new SqlConnection(
           connectionString))
      {
        SqlCommand command = new SqlCommand(sqlBody, connection);
        command.Parameters.AddWithValue("@lockOwner", $@"<r owner=""{args.User.Name}""");
        connection.Open();
        SqlDataReader reader = command.ExecuteReader();
        try
        {
          while (reader.Read())
          {
            var id = new ID((Guid)reader[0]);
            var ver = Sitecore.Data.Version.Parse(reader[1]);
            var lan = Language.Parse((string)reader[2]);

            lockedItemUris.Add(new DataUri(id, lan, ver));
          }
        }
        finally
        {
          // Always call Close when done reading.
          reader.Close();
        }
      }


      foreach (var lockedMoniker in lockedItemUris)
      {
        var lockedItem = this.manager.GetItem(lockedMoniker.ItemID, lockedMoniker.Language, lockedMoniker.Version, db);

        if (lockedItem != null)
        {
          args.Result.Add(lockedItem);
        }
      }
    }
  }
}