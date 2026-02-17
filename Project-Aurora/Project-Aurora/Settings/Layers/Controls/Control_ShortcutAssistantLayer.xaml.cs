using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AuroraRgb.Controls;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MenuItem = System.Windows.Controls.MenuItem;

namespace AuroraRgb.Settings.Layers.Controls;

/// <summary>
/// Interaction logic for Control_ShortcutAssistantLayer.xaml
/// </summary>
public partial class ControlShortcutAssistantLayer
{
    private readonly List<IShortcut> _predefinedShortcuts = [];

    private readonly Button _buttonAddNewShortcut = new() {
        Content = "New shortcut",
        HorizontalAlignment = HorizontalAlignment.Center
    };

    protected ControlShortcutAssistantLayer()
    {
        InitializeComponent();
        CreateDefaults();

        PopulatePresets();

        _buttonAddNewShortcut.Click += ButtonAddNewShortcut_Click;

        Loaded += (_, _) => { SetSettings(); };
    }

    public ControlShortcutAssistantLayer(ShortcutAssistantLayerHandler dataContext) : this()
    {
        DataContext = dataContext;
    }

    private void SetSettings()
    {
        comboboxPresentationType.SelectedValue = ((ShortcutAssistantLayerHandler)DataContext).Properties.PresentationType;

        stackPanelShortcuts.Children.Clear();
        foreach(var keyb in ((ShortcutAssistantLayerHandler)DataContext).Properties.ShortcutKeys)
        {
            AddKeybind(keyb);
        }
        AddStackPanelButton();
    }
    private void PopulatePresets(MenuItem? item = null, ShortcutNode? node = null)
    {
        var currentItem = item ?? menuPresets;
        foreach (var shortcut in node?.Children ?? _predefinedShortcuts)
        {
            var newItem = new MenuItem { Header = shortcut.Title, Tag = shortcut };
            newItem.Click += ShortcutPresetClick;
            if (shortcut is ShortcutNode shortcutNode)
                PopulatePresets(newItem, shortcutNode);
            currentItem.Items.Add(newItem);
        }
    }

    private void ShortcutPresetClick(object? sender, RoutedEventArgs e)
    {
        IShortcut? shortcut;
        if ((shortcut = (e.OriginalSource as MenuItem)?.Tag as IShortcut) == null) return;
        var layer = (ShortcutAssistantLayerHandler)DataContext;
        layer.Properties.ShortcutKeys = shortcut switch
        {
            ShortcutNode shortcutNode => shortcutNode.GetShortcuts(),
            ShortcutGroup shortcutGroup => shortcutGroup.Shortcuts ?? [],
            _ => layer.Properties.ShortcutKeys
        };
        SetSettings();
        e.Handled = true;
    }

    private void ButtonRemoveKeybind_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: DockPanel buttonTag }) return;
        stackPanelShortcuts.Children.Remove(buttonTag);
        ApplySettings();
    }

    private void ButtonAddNewShortcut_Click(object? sender, RoutedEventArgs e)
    {
        stackPanelShortcuts.Children.Remove(_buttonAddNewShortcut);

        AddKeybind(new Keybind());

        AddStackPanelButton();
    }

    private void AddKeybind(Keybind keyb)
    {
        var keybindEditor = new Control_Keybind
        {
            ContextKeybind = keyb,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        keybindEditor.KeybindUpdated += KeybindEditor_KeybindUpdated;

        var dp = new DockPanel
        {
            LastChildFill = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Tag = keybindEditor
        };

        var buttonRemoveKeybind = new Button
        {
            Content = "X",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Tag = dp
        };
        DockPanel.SetDock(buttonRemoveKeybind, Dock.Right);
        buttonRemoveKeybind.Click += ButtonRemoveKeybind_Click;


        dp.Children.Add(buttonRemoveKeybind);
        dp.Children.Add(keybindEditor);

        stackPanelShortcuts.Children.Add(dp);
    }

    private void AddStackPanelButton()
    {
        stackPanelShortcuts.Children.Add(_buttonAddNewShortcut);
    }

    private void KeybindEditor_KeybindUpdated(object? sender, Keybind newKeybind)
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        ((ShortcutAssistantLayerHandler)DataContext).Properties.ShortcutKeys = (
            from object child in stackPanelShortcuts.Children
            where (child as DockPanel)?.Tag is Control_Keybind
            select ((Control_Keybind) (child as DockPanel).Tag).ContextKeybind).ToArray();
    }

    private void comboboxPresentationType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && DataContext is ShortcutAssistantLayerHandler && sender is ComboBox comboBox)
            ((ShortcutAssistantLayerHandler)DataContext).Properties.PresentationType = (ShortcutAssistantPresentationType)comboBox.SelectedValue;
    }

    private void CreateDefaults()
    {
        _predefinedShortcuts.Add(
            new ShortcutNode("Windows") {
                Children =
                [
                    new ShortcutGroup("Ctrl")
                    {
                        Shortcuts =
                        [
                            new Keybind(new[] { Keys.LControlKey, Keys.X }),
                            new Keybind(new[] { Keys.LControlKey, Keys.C }),
                            new Keybind(new[] { Keys.LControlKey, Keys.V }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Z }),
                            new Keybind(new[] { Keys.LControlKey, Keys.F4 }),
                            new Keybind(new[] { Keys.LControlKey, Keys.A }),
                            new Keybind(new[] { Keys.LControlKey, Keys.D }),
                            new Keybind(new[] { Keys.LControlKey, Keys.R }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Y }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Right }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Left }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Down }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Up }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LMenu, Keys.Tab }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LMenu, Keys.LShiftKey, Keys.Tab }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Up }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Down }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Left }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Right }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Escape }),
                            new Keybind(new[] { Keys.LControlKey, Keys.LShiftKey, Keys.Escape }),
                            new Keybind(new[] { Keys.LControlKey, Keys.Escape }),
                            new Keybind(new[] { Keys.LControlKey, Keys.F })
                        ]
                    },

                    new ShortcutGroup("Win")
                    {
                        Shortcuts =
                        [
                            new Keybind(new[] { Keys.LWin, Keys.L }),
                            new Keybind(new[] { Keys.LWin, Keys.D }),
                            new Keybind(new[] { Keys.LWin, Keys.B }),
                            new Keybind(new[] { Keys.LWin, Keys.A }),
                            new Keybind(new[] { Keys.LWin, Keys.LMenu, Keys.D }),
                            new Keybind(new[] { Keys.LWin, Keys.E }),
                            new Keybind(new[] { Keys.LWin, Keys.G }),
                            new Keybind(new[] { Keys.LWin, Keys.I }),
                            new Keybind(new[] { Keys.LWin, Keys.M }),
                            new Keybind(new[] { Keys.LWin, Keys.P }),
                            new Keybind(new[] { Keys.LWin, Keys.R }),
                            new Keybind(new[] { Keys.LWin, Keys.S }),
                            new Keybind(new[] { Keys.LWin, Keys.Up }),
                            new Keybind(new[] { Keys.LWin, Keys.Down }),
                            new Keybind(new[] { Keys.LWin, Keys.Left }),
                            new Keybind(new[] { Keys.LWin, Keys.Right }),
                            new Keybind(new[] { Keys.LWin, Keys.Home }),
                            new Keybind(new[] { Keys.LWin, Keys.D }),
                            new Keybind(new[] { Keys.LWin, Keys.LShiftKey, Keys.Left }),
                            new Keybind(new[] { Keys.LWin, Keys.LShiftKey, Keys.Up }),
                            new Keybind(new[] { Keys.LWin, Keys.LShiftKey, Keys.Right }),
                            new Keybind(new[] { Keys.LWin, Keys.LShiftKey, Keys.Down })
                        ]
                    },

                    new ShortcutGroup("Alt")
                    {
                        Shortcuts =
                        [
                            new Keybind(new[] { Keys.LMenu, Keys.Tab }),
                            new Keybind(new[] { Keys.LMenu, Keys.F4 }),
                            new Keybind(new[] { Keys.LMenu, Keys.Space }),
                            new Keybind(new[] { Keys.LMenu, Keys.Left }),
                            new Keybind(new[] { Keys.LMenu, Keys.Right }),
                            new Keybind(new[] { Keys.LMenu, Keys.PageUp }),
                            new Keybind(new[] { Keys.LMenu, Keys.PageDown }),
                            new Keybind(new[] { Keys.LMenu, Keys.Tab })
                        ]
                    }
                ]
            }
        );
    }
}