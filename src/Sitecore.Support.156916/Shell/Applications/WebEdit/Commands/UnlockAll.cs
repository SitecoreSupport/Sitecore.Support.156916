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
      Item[] source = Client.ContentDatabase.SelectItems("search://*[@__lock='%\"" + Context.User.Name + "\"']");
      if ((source == null) || !source.Any<Item>())
      {
        SheerResponse.Alert("You have no locked items.", new string[0]);
      }
      else if (args.IsPostBack)
      {
        if (args.Result == "yes")
        {
          foreach (Item item in source)
          {
            foreach (Item item2 in item.Versions.GetVersions(true))
            {
              if (string.Compare(item2.Locking.GetOwner(), Context.User.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
              {
                list.Add(item2);
              }
            }
          }
          ProgressBox.Execute("UnlockAll", "Unlocking items", "Network/16x16/lock.png", new ProgressBoxMethod(UnlockAll.UnlockAllItems), "lockeditems:refresh", Context.User, new object[] { list });
        }
      }
      else
      {
        if (source.Count<Item>() == 1)
        {
          SheerResponse.Confirm("Are you sure you want to unlock this item?");
        }
        else
        {
          SheerResponse.Confirm(Translate.Text("Are you sure you want to unlock these {0} items?", new object[] { source.Count<Item>() }));
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
          if (job != null)
          {
            job.Status.Messages.Add(Translate.Text("Unlocking {0}", new object[] { item.Paths.ContentPath }));
          }
          item.Locking.Unlock();
          if (job != null)
          {
            JobStatus status = job.Status;
            status.Processed += 1L;
          }
        }
      }
    }

  }
}