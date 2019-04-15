using ComponentArt.Web.UI;
using Sitecore;
using Sitecore.Controls;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Ajax;
using Sitecore.Web.UI.XamlSharp.Xaml;
using System;
using System.Linq;
namespace Sitecore.Support.Shell.Applications.WebEdit.Dialogs.LockedItems
{
  /// <summary>
  /// The locked items page.
  /// </summary>
  public class LockedItemsPage : ModalDialogPage
  {
    /// <summary>
    /// The unlocked item IDs list session key
    /// </summary>
    protected const string UnlockedItemsIDsKey = "UnlockedItemIDs";

    /// <summary>
    /// The items.
    /// </summary>
    protected Grid Items;

    /// <summary>
    /// Gets or sets unlocked item IDs
    /// </summary>
    protected string[] UnlockedItemIds
    {
      get
      {
        return WebUtil.GetSessionValue("UnlockedItemIDs") as string[];
      }
      set
      {
        WebUtil.SetSessionValue("UnlockedItemIDs", value);
      }
    }

    /// <summary>
    /// Executes the ajax command.
    /// </summary>
    /// <param name="e">
    /// The <see cref="T:Sitecore.Web.UI.XamlSharp.Ajax.AjaxCommandEventArgs" /> instance containing the event data.
    /// </param>
    protected override void ExecuteAjaxCommand(AjaxCommandEventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      if (e.Name == "lockeditems:refresh")
      {
        SheerResponse.Eval("Items.callback()");
      }
      else
      {
        base.ExecuteAjaxCommand(e);
      }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
    /// </summary>
    /// <param name="e">
    /// The <see cref="T:System.EventArgs"></see> object that contains the event data.
    /// </param>
    protected override void OnLoad(EventArgs e)
    {
      Assert.CanRunApplication("/sitecore/content/Applications/Content Editor/Ribbons/Chunks/Locks/My Items");
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);
      if (!XamlControl.AjaxScriptManager.IsEvent)
      {
        string[] unlockedItemIds = UnlockedItemIds;
        WebUtil.RemoveSessionValue("UnlockedItemIDs");
        //Item[] array = Client.ContentDatabase.SelectItems("search://*[@__lock='%\"" + Sitecore.Context.User.Name + "\"']") ?? new Item[0];
        Item[] array = Client.ContentDatabase.SelectItems("fast:/sitecore//*[@__lock='%" + Sitecore.Context.User.Name + "%']") ?? new Item[0];
        if (unlockedItemIds != null)
        {
          array = ((unlockedItemIds.Length == 0) ? new Item[0] : (from x in array
                                                                  where !unlockedItemIds.Contains(x.ID.ToString())
                                                                  select x).ToArray());
        }
        ComponentArtGridHandler<Item>.Manage(Items, new GridSource<Item>(array), true);
        Items.GroupingNotificationText = Translate.Text("To group your items by column, drag and drop the column here.");
        Items.LocalizeGrid();
      }
    }

    /// <summary>
    /// Hanldes Unlock button click.
    /// </summary>
    protected void OnUnlock()
    {
      string[] selectedKeys = Items.SelectedKeys;
      if (selectedKeys.Length > 0)
      {
        UnlockedItemIds = selectedKeys;
      }
      Command command = CommandManager.GetCommand("webedit:unlock");
      CommandContext context = new CommandContext();
      command.Execute(context);
    }

    /// <summary>
    /// Hanldes UnlockAll button click.
    /// </summary>
    protected void OnUnlockAll()
    {
      UnlockedItemIds = new string[0];
      Command command = CommandManager.GetCommand("webedit:unlockall");
      CommandContext context = new CommandContext();
      command.Execute(context);
    }
  }

}