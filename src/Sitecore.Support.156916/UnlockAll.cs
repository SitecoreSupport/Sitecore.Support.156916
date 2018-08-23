using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  [Serializable]
  public class UnlockAll : WebEditCommand, ISupportsContinuation
  {
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      ContinuationManager.Current.Start(this, "Run");
    }

    protected static void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      List<Item> list = new List<Item>();
      Item[] array = Client.ContentDatabase.SelectItems("search://*[@__lock='%\"" + Context.User.Name + "\"']");
      if (array == null || !array.Any())
      {
        SheerResponse.Alert("You have no locked items.");
      }
      else if (args.IsPostBack)
      {
        if (args.Result == "yes")
        {
          Item[] array2 = array;
          foreach (Item item in array2)
          {
            Item[] versions = item.Versions.GetVersions(true);
            foreach (Item item2 in versions)
            {
              if (string.Compare(item2.Locking.GetOwner(), Context.User.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
              {
                list.Add(item2);
              }
            }
          }
          ProgressBox.Execute("UnlockAll", "Unlocking items", "Network/16x16/lock.png", UnlockAllItems, "lockeditems:refresh", Context.User, list);
        }
      }
      else
      {
        if (array.Count() == 1)
        {
          SheerResponse.Confirm("Are you sure you want to unlock this item?");
        }
        else
        {
          SheerResponse.Confirm(Translate.Text("Are you sure you want to unlock these {0} items?", array.Count()));
        }
        args.WaitForPostBack();
      }
    }

    private static void UnlockAllItems(params object[] parameters)
    {
      Assert.ArgumentNotNull(parameters, "parameters");
      List<Item> list = parameters[0] as List<Item>;
      if (list != null)
      {
        Job job = Context.Job;
        if (job != null)
        {
          job.Status.Total = list.Count;
        }
        foreach (Item item in list)
        {
          job?.Status.Messages.Add(Translate.Text("Unlocking {0}", item.Paths.ContentPath));
          item.Locking.Unlock();
          if (job != null)
          {
            job.Status.Processed += 1L;
          }
        }
      }
    }
  }
}