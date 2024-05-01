using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Common;
using Common.Devices;
using Xceed.Wpf.Toolkit;
using MediaColor = System.Windows.Media.Color;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_VariableRegistryItem.xaml
/// </summary>
public partial class Control_VariableRegistryItem
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VariableNameProperty = DependencyProperty.Register(nameof(VariableName),
        typeof(string), typeof(Control_VariableRegistryItem), new PropertyMetadata(VariableNameChanged));

    private static void VariableNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue.Equals(e.OldValue))
            return;

        var self = (Control_VariableRegistryItem)d;
        if (self.IsLoaded)
            self.UpdateControls();
    }

    public string VariableName
    {
        get => (string)GetValue(VariableNameProperty);
        set => SetValue(VariableNameProperty, value);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public static readonly DependencyProperty VarRegistryProperty = DependencyProperty.Register(nameof(VarRegistry),
        typeof(VariableRegistry), typeof(Control_VariableRegistryItem));

    public VariableRegistry VarRegistry
    {
        get => (VariableRegistry)GetValue(VarRegistryProperty);
        set
        {
            SetValue(VarRegistryProperty, value);

            if (IsLoaded)
                UpdateControls();
        }
    }

    public Control_VariableRegistryItem()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            Dispatcher.BeginInvoke(UpdateControls, DispatcherPriority.Loaded);
        };
    }

    private void UpdateControls()
    {
        var varTitle = VarRegistry.GetTitle(VariableName);

        txtBlk_name.Text = string.IsNullOrWhiteSpace(varTitle) ? VariableName : varTitle;

        var varRemark = VarRegistry.GetRemark(VariableName);

        if (string.IsNullOrWhiteSpace(varRemark))
            txtBlk_remark.Visibility = Visibility.Collapsed;
        else
            txtBlk_remark.Text = varRemark;

        //Create a control here...
        var varType = VarRegistry.GetVariableType(VariableName);

        grd_control.Children.Clear();

        Control control;
        if (varType == typeof(bool))
        {
            control = CreateBoolControl();
        }
        else if (varType == typeof(string))
        {
            control = CreateStringControl();
        }
        else if (varType == typeof(int))
        {
            control = CreateIntControl();
        }
        else if (varType == typeof(long))
        {
            control = CreateLongControl();
        }
        else if (varType == typeof(double))
        {
            control = CreateDoubleControl();
        }
        else if (varType == typeof(float))
        {
            control = CreateFloatControl();
        }
        else if (varType == typeof(Settings.KeySequence))
        {
            control = CreateKeySequenceControl(varTitle);
        }
        else if (varType == typeof(SimpleColor))
        {
            control = CreateColorControl();
        }
        else if (varType == typeof(DeviceKeys))
        {
            control = CreateDeviceKeyControl();
        }
        else if (varType.IsEnum)
        {
            control = CreateEnumControl(varType);
        }
        else
        {
            control = new TextBox{ Text = "Unsupported type: " + varType, IsEnabled = false};
        }

        grd_control.Children.Add(control);
    }

    private CheckBox CreateBoolControl()
    {
        var chkbxControl = new CheckBox();
        chkbxControl.Content = "";
        chkbxControl.IsChecked = VarRegistry.GetVariable<bool>(VariableName);
        chkbxControl.Checked += ChkbxControl_VarChanged;
        chkbxControl.Unchecked += ChkbxControl_VarChanged;
        return chkbxControl;
    }

    private TextBox CreateStringControl()
    {
        var txtbxControl = new TextBox();
        txtbxControl.Text = VarRegistry.GetString(VariableName);
        txtbxControl.TextChanged += Txtbx_control_TextChanged;
        return txtbxControl;
    }

    private Control CreateIntControl()
    {
        if (VarRegistry.GetFlags(VariableName).HasFlag(IntVariableDisplay.UseHex))
        {
            var hexBox = new TextBox();
            hexBox.PreviewTextInput += HexBoxOnPreviewTextInput;
            hexBox.Text = $"{VarRegistry.GetVariable<int>(VariableName):X}";
            hexBox.TextChanged += HexBox_TextChanged;
            return hexBox;
        }

        var intUpDownControl = new IntegerUpDown();
        intUpDownControl.Value = VarRegistry.GetVariable<int>(VariableName);
        if (VarRegistry.GetVariableMax<int>(VariableName, out var maxVal))
            intUpDownControl.Maximum = maxVal;
        if (VarRegistry.GetVariableMin<int>(VariableName, out var minVal))
            intUpDownControl.Minimum = minVal;

        intUpDownControl.ValueChanged += VariableChanged;

        return intUpDownControl;
    }

    private LongUpDown CreateLongControl()
    {
        var longUpDownControl = new LongUpDown();
        longUpDownControl.Value = VarRegistry.GetVariable<long>(VariableName);
        if (VarRegistry.GetVariableMax<long>(VariableName, out var maxVal))
            longUpDownControl.Maximum = maxVal;
        if (VarRegistry.GetVariableMin<long>(VariableName, out var minVal))
            longUpDownControl.Minimum = minVal;

        longUpDownControl.ValueChanged += VariableChanged;
        return longUpDownControl;
    }

    private DoubleUpDown CreateDoubleControl()
    {
        var doubleUpDownControl = new DoubleUpDown();
        doubleUpDownControl.Value = VarRegistry.GetVariable<double>(VariableName);
        if (VarRegistry.GetVariableMax<double>(VariableName, out var maxVal))
            doubleUpDownControl.Maximum = maxVal;
        if (VarRegistry.GetVariableMax<double>(VariableName, out var minVal))
            doubleUpDownControl.Minimum = minVal;
        doubleUpDownControl.ValueChanged += VariableChanged;
        return doubleUpDownControl;
    }

    private DoubleUpDown CreateFloatControl()
    {
        var doubleUpDownControl = new DoubleUpDown();
        doubleUpDownControl.Value = VarRegistry.GetVariable<float>(VariableName);
        if (VarRegistry.GetVariableMax<float>(VariableName, out var maxVal))
            doubleUpDownControl.Maximum = maxVal;
        if (VarRegistry.GetVariableMax<float>(VariableName, out var minVal))
            doubleUpDownControl.Minimum = minVal;
        doubleUpDownControl.ValueChanged += VariableChanged;
        return doubleUpDownControl;
    }

    private KeySequence CreateKeySequenceControl(string varTitle)
    {
        var ctrl = new KeySequence
        {
            RecordingTag = varTitle,
            Sequence = VarRegistry.GetVariable<Settings.KeySequence>(VariableName)
        };

        ctrl.SequenceUpdated += VariableChanged;
        return ctrl;
    }

    private ColorPicker CreateColorControl()
    {
        var clr = VarRegistry.GetVariable<SimpleColor>(VariableName);
        var mediaColor = MediaColor.FromArgb(clr.A, clr.R, clr.G, clr.B);

        var ctrl = new ColorPicker
        {
            ColorMode = ColorMode.ColorCanvas,
            SelectedColor = mediaColor
        };
        ctrl.SelectedColorChanged += ColorPickerControlValueChanged;
        return ctrl;
    }

    private ComboBox CreateDeviceKeyControl()
    {
        var ctrl = new ComboBox
        {
            ItemsSource = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToList(),
            SelectedValue = VarRegistry.GetVariable<DeviceKeys>(VariableName)
        };
        ctrl.SelectionChanged += CmbbxEnum_control_SelectionChanged;
        return ctrl;
    }

    private ComboBox CreateEnumControl(Type varType)
    {
        var cmbbxEnumControl = new ComboBox
        {
            ItemsSource = Enum.GetValues(varType),
            SelectedValue = VarRegistry.GetVariable<object>(VariableName)
        };
        cmbbxEnumControl.SelectionChanged += CmbbxEnum_control_SelectionChanged;
        return cmbbxEnumControl;
    }


    private void CmbbxEnum_control_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        VarRegistry.SetVariable(VariableName, e.AddedItems[0]);
    }

    private void ColorPickerControlValueChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        var ctrlClr = e.NewValue;
        if (!ctrlClr.HasValue)
        {
            return;
        }
        var clr = ctrlClr.Value;
        VarRegistry.SetVariable(VariableName, new SimpleColor(clr.R, clr.G, clr.B, clr.A));
    }

    private void VariableChanged<T>(object? sender, RoutedPropertyChangedEventArgs<T> e) where T : notnull
    {
        VarRegistry.SetVariable(VariableName, e.NewValue);
    }

    private void Txtbx_control_TextChanged(object sender, TextChangedEventArgs e)
    {
        VarRegistry.SetVariable(VariableName, ((TextBox)sender).Text);
    }

    private void HexBoxOnPreviewTextInput(object? sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out _);
    }

    private void HexBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = ((TextBox)sender).Text;
        //Hacky fix to stop error when nothing is entered
        if (string.IsNullOrWhiteSpace(text))
            text = "0";
        VarRegistry.SetVariable(VariableName, int.Parse(text, NumberStyles.HexNumber, CultureInfo.CurrentCulture));
    }

    private void ChkbxControl_VarChanged(object sender, RoutedEventArgs e)
    {
        VarRegistry.SetVariable(VariableName, ((CheckBox)sender).IsChecked.GetValueOrDefault());
    }

    private void btn_reset_Click(object? sender, RoutedEventArgs e)
    {
        VarRegistry.ResetVariable(VariableName);

        UpdateControls();
    }
}