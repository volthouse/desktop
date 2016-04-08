using ACast.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ACast.UI
{
    public enum PivotView {
        Feeds,
        FeedItems,
        Player
    }

    public class ViewManager
    {
        private Pivot pivot;

        public event EventHandler<PivotView> ActiveViewChanged;

        public PivotView CurrentView { get; set; }

        public Pivot Pivot {
            get { return pivot; }
            set
            {
                if (pivot != null)
                {
                    pivot.SelectionChanged -= pivot_SelectionChanged;
                }
                pivot = value;
                pivot.SelectionChanged += pivot_SelectionChanged;
            }
        }


        public void SwitchTo(PivotView view)
        {
            pivot.SelectionChanged -= pivot_SelectionChanged;

            switch (view)
            {
                case PivotView.Feeds:
                    Pivot.SelectedIndex = 0;
                    OnViewChanged(PivotView.Feeds);
                    CurrentView = view;
                    break;
                case PivotView.FeedItems:
                    Pivot.SelectedIndex = 1;
                    OnViewChanged(PivotView.FeedItems);
                    CurrentView = view;
                    break;
                case PivotView.Player:
                    Pivot.SelectedIndex = 2;
                    OnViewChanged(PivotView.Player);
                    CurrentView = view;
                    break;
                default:
                    break;
            }

            pivot.SelectionChanged += pivot_SelectionChanged;
        }

        public void ActivateView(object sender, PivotView view)
        {
            SwitchTo(view);
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    if(CurrentView != PivotView.Feeds)
                        SwitchTo(PivotView.Feeds);
                    break;
                case 1:
                    if (CurrentView != PivotView.FeedItems)
                        SwitchTo(PivotView.FeedItems);
                    break;
                case 2:
                    if (CurrentView != PivotView.Player)
                        SwitchTo(PivotView.Player);
                    break;
                default:
                    break;
            }
        }

        private void OnViewChanged(PivotView view)
        {
            if (ActiveViewChanged != null)
            {
                ActiveViewChanged(this, view);
            }
        }
    }

    [Flags]
    public enum ButtonFlags
    {
        Add =           0x01,
        Remove =        0x02,
        Cancel =        0x04,
        Multiselect =   0x08,
        Search =        0x10
    }

    public class CommandBarManager
    {
        private CommandBar commandBar;
        private Dictionary<ButtonFlags, AppBarButton> buttons;

        public event EventHandler<ButtonFlags> ButtonClick;

        public CommandBarManager()
        {
            buttons = new Dictionary<ButtonFlags, AppBarButton>();

            buttons.Add(ButtonFlags.Add, new AddButton());
            buttons.Add(ButtonFlags.Remove, new RemoveButton());
            buttons.Add(ButtonFlags.Cancel, new CancelButton());
            buttons.Add(ButtonFlags.Multiselect, new MultiSelectButton());
            buttons.Add(ButtonFlags.Search, new SearchButton());

            foreach (var button in buttons.Values)
            {
                button.Click += button_Click;
            }
        }

        public CommandBar CommandBar
        {
            get { return commandBar;  }
            set {
                commandBar = value;                
            }
        }

        public void EnableButtons(object sender, ButtonFlags buttonFlags)
        {
            commandBar.PrimaryCommands.Clear();

            foreach (ButtonFlags flag in Enum.GetValues(typeof(ButtonFlags)))
            {
                if((buttonFlags & flag) != 0)
                {
                    commandBar.PrimaryCommands.Add(buttons[flag]);
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var keys = from item in buttons where item.Value.Equals(sender) select item.Key;

            if (ButtonClick != null && keys.Any())
            {
                ButtonClick(this, keys.First());
            }
        }
    }

    public class AppBarButtonBase : AppBarButton
    {
        public AppBarButtonBase()
        {
        }
    }

    public class AddButton : AppBarButtonBase
    {
        public AddButton()
        {
            Icon = new SymbolIcon(Symbol.Add);
        }
    }

    public class RemoveButton : AppBarButtonBase
    {
        public RemoveButton()
        {
            Icon = new SymbolIcon(Symbol.Delete);
        }
    }

    public class RefreshButton : AppBarButtonBase
    {
        public RefreshButton()
        {
            Icon = new SymbolIcon(Symbol.Refresh);
        }
    }

    public class MultiSelectButton : AppBarButtonBase
    {
        public MultiSelectButton()
        {
            Icon = new SymbolIcon(Symbol.Bullets);
        }
    }

    public class CancelButton : AppBarButtonBase
    {
        public CancelButton()
        {
            Icon = new SymbolIcon(Symbol.Cancel);
        }
    }

    public class SearchButton : AppBarButtonBase
    {
        public SearchButton()
        {
            Icon = new SymbolIcon(Symbol.Zoom);
        }
    }    
}
