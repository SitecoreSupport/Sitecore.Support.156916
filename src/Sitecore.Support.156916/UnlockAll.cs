using Sitecore.Data.Items;
using Sitecore.Data.Locking;
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

namespace Sitecore.Support.sitecore.shell.Application.WebEdit
{
  [Obsolete("This method is obsolete and will be removed in the next product version. Please use SPEAK JS approach instead.")]
  [Serializable]
  public class UnlockAll : WebEditCommand, ISupportsContinuation
  {
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull((object)context, nameof(context));
      ContinuationManager.Current.Start((ISupportsContinuation)this, "Run");
    }

    protected static void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      UserLockedItemArgs args1 = new UserLockedItemArgs(Context.User.Identity, Context.ContentDatabase);
      UserLockProvider.Instance.GetItemsLockedByUser(args1);
      IList<Item> result = args1.Result;
      if (result == null || !result.Any<Item>())
      {
        SheerResponse.Alert("You have no locked items.", Array.Empty<string>());
      }
      else
      {
        List<Item> objList = new List<Item>(result.Count);
        if (args.IsPostBack)
        {
          if (!(args.Result == "yes"))
            return;
          foreach (Item obj in (IEnumerable<Item>)result)
          {
            foreach (Item version in obj.Versions.GetVersions(true))
            {
              if (string.Compare(version.Locking.GetOwner(), Context.User.Name, StringComparison.OrdinalIgnoreCase) == 0)
                objList.Add(version);
            }
          }
          ProgressBox.Execute(nameof(UnlockAll), "Unlocking items", "Network/16x16/lock.png", new ProgressBoxMethod(UnlockAll.UnlockAllItems), "lockeditems:refresh", Context.User, (object)objList);
        }
        else
        {
          string text;
          if (result.Count<Item>() != 1)
            text = Translate.Text("Are you sure you want to unlock these {0} items?", (object)result.Count<Item>());
          else
            text = "Are you sure you want to unlock this item?";
          SheerResponse.Confirm(text);
          args.WaitForPostBack();
        }
      }
    }

    private static void UnlockAllItems(params object[] parameters)
    {
      Assert.ArgumentNotNull((object)parameters, nameof(parameters));
      List<Item> parameter = parameters[0] as List<Item>;
      if (parameter == null)
        return;
      Job job = Context.Job;
      if (job != null)
        job.Status.Total = (long)parameter.Count;
      foreach (Item obj in parameter)
      {
        job?.Status.Messages.Add(Translate.Text("Unlocking {0}", (object)obj.Paths.ContentPath));
        obj.Locking.Unlock();
        if (job != null)
          ++job.Status.Processed;
      }
    }
  }
}