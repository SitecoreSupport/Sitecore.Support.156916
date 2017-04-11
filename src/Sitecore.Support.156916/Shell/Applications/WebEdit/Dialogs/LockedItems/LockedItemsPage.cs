using ComponentArt.Web.UI;
using Sitecore.Controls;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Globalization;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Ajax;
using Sitecore.Web.UI.XamlSharp.Xaml;
using System;

namespace Sitecore.Support.Shell.Applications.WebEdit.Dialogs.LockedItems
{
  public class LockedItemsPage : ModalDialogPage
  {
    // Fields
    protected Grid Items;

    // Methods
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

    protected override void OnLoad(EventArgs e)
    {
      Assert.CanRunApplication("/sitecore/content/Applications/Content Editor/Ribbons/Chunks/Locks/My Items");
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);
      if (!XamlControl.AjaxScriptManager.IsEvent)
      {
        Item[] elements = Client.ContentDatabase.SelectItems("search://*[@__lock='%\"" + Sitecore.Context.User.Name + "\"']") ?? new Item[0];
        ComponentArtGridHandler<Item>.Manage(this.Items, new GridSource<Item>(elements), true);
        this.Items.GroupingNotificationText = Translate.Text("To group your items by column, drag and drop the column here.");
        this.Items.LocalizeGrid();
      }
    }
  }

}