using ComponentArt.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.Controls;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Locking;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Ajax;
using Sitecore.Web.UI.XamlSharp.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Sitecore.Support.Shell.Applications.WebEdit.Dialogs.LockedItems
{
  public class LockedItemsPage : ModalDialogPage
  {
    protected const string UnlockedItemsIDsKey = "UnlockedItemIDs";
    private readonly BaseTranslate translate;
    private readonly UserLockProvider userLockProvider;
    protected Grid Items;

    public LockedItemsPage()
      : this(ServiceLocator.ServiceProvider.GetRequiredService<BaseTranslate>(), UserLockProvider.Instance)
    {
    }

    internal LockedItemsPage(BaseTranslate translate, UserLockProvider userLockProvider)
    {
      Assert.ArgumentNotNull((object)translate, nameof(translate));
      this.translate = translate;
      this.userLockProvider = userLockProvider;
    }

    protected string[] UnlockedItemIds
    {
      get
      {
        return WebUtil.GetSessionValue("UnlockedItemIDs") as string[];
      }
      set
      {
        WebUtil.SetSessionValue("UnlockedItemIDs", (object)value);
      }
    }

    protected override void ExecuteAjaxCommand(AjaxCommandEventArgs e)
    {
      Assert.ArgumentNotNull((object)e, nameof(e));
      if (e.Name == "lockeditems:refresh")
        SheerResponse.Eval("Items.callback()");
      else
        base.ExecuteAjaxCommand(e);
    }

    protected override void OnLoad(EventArgs e)
    {
      Assert.CanRunApplication("/sitecore/content/Applications/Content Editor/Ribbons/Chunks/Locks/My Items");
      Assert.ArgumentNotNull((object)e, nameof(e));
      base.OnLoad(e);
      if (XamlControl.AjaxScriptManager.IsEvent)
        return;
      string[] unlockedItemIds = this.UnlockedItemIds;
      WebUtil.RemoveSessionValue("UnlockedItemIDs");
      Item[] objArray = this.GetLockedItems(Sitecore.Context.User.Identity, Sitecore.Context.ContentDatabase) ?? new Item[0];
      if (unlockedItemIds != null)
        objArray = unlockedItemIds.Length == 0 ? new Item[0] : ((IEnumerable<Item>)objArray).Where<Item>((Func<Item, bool>)(x => !((IEnumerable<string>)unlockedItemIds).Contains<string>(x.ID.ToString()))).ToArray<Item>();
      ComponentArtGridHandler<Item>.Manage(this.Items, (IGridSource<Item>)new GridSource<Item>((IEnumerable<Item>)objArray), true);
      this.Items.GroupingNotificationText = this.translate.Text("To group your items by column, drag and drop the column here.");
      this.Items.LocalizeGrid();
    }

    private Item[] GetLockedItems(IIdentity user, Database contentDatabase)
    {
      UserLockedItemArgs args = new UserLockedItemArgs(user, contentDatabase);
      this.userLockProvider.GetItemsLockedByUser(args);
      return args.Result.ToArray<Item>();
    }

    protected void OnUnlock()
    {
      string[] selectedKeys = this.Items.SelectedKeys;
      if (selectedKeys.Length != 0)
        this.UnlockedItemIds = selectedKeys;
      CommandManager.GetCommand("webedit:unlock").Execute(new CommandContext());
    }

    protected void OnUnlockAll()
    {
      this.UnlockedItemIds = new string[0];
      CommandManager.GetCommand("webedit:unlockall").Execute(new CommandContext());
    }
  }
}