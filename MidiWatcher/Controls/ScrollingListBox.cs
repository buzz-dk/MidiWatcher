using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MidiWatcher.Models;

// https://stackoverflow.com/questions/16866309/listbox-scroll-into-view-with-mvvm

namespace MidiWatcher.Controls;

public class ScrollingListBox : ListBox
{
    protected override void OnItemsChanged(object e)
    {
        var newItemCount= Items.Count;
        if (newItemCount > 0)
        {
            ScrollIntoView(Items[newItemCount - 1]);
        }
        base.OnItemsChanged(e);
    }
}
