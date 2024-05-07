using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for LayerEditor.xaml
/// </summary>
public partial class LayerEditor
{
    private static Canvas _staticCanvas = new();
    private static Style _style = new();

    public LayerEditor()
    {
        InitializeComponent();

        _staticCanvas = editor_canvas;
        _style = FindResource("DesignerItemStyle") as Style;
    }

    public static void AddKeySequenceElement(FreeFormObject freeForm, Color elementColor, string elementName)
    {
        var existingControl = FindElementByTag(freeForm);

        if (existingControl != null) return;

        var transform = new RotateTransform
        {
            Angle = freeForm.Angle
        };

        var label = new Label();
        label.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        label.Content = elementName;
        label.IsHitTestVisible = false;

        Shape content =  new Rectangle();
        content.Fill = new SolidColorBrush(elementColor);
        content.IsHitTestVisible = false;
        content.Opacity = 0.50D;

        var contentGrid = new Grid();
        contentGrid.IsHitTestVisible = false;
        contentGrid.Children.Add(content);
        contentGrid.Children.Add(label);

        var transformControl = new ContentControl();
        transformControl.Width = freeForm.Width;
        transformControl.Height = freeForm.Height;
        transformControl.Style = _style;
        transformControl.Tag = freeForm;
        transformControl.Content = contentGrid;
        transformControl.SizeChanged += NewcontrolSizeChanged;

        transformControl.SetValue(Selector.IsSelectedProperty, true);
        transformControl.SetValue(Canvas.TopProperty, (double)(freeForm.Y + Effects.Canvas.GridBaselineY));
        transformControl.SetValue(Canvas.LeftProperty, (double)(freeForm.X + Effects.Canvas.GridBaselineX));
        transformControl.SetValue(RenderTransformProperty, transform);

        var descriptor = DependencyPropertyDescriptor.FromProperty(
            Canvas.LeftProperty, typeof(ContentControl)
        );
        descriptor.AddValueChanged(transformControl, OnCanvasLeftChanged);
        var descriptorTop = DependencyPropertyDescriptor.FromProperty(
            Canvas.TopProperty, typeof(ContentControl)
        );
        descriptorTop.AddValueChanged(transformControl, OnCanvasTopChanged);
        var descriptorAngle = DependencyPropertyDescriptor.FromProperty(
            RenderTransformProperty, typeof(ContentControl)
        );
        descriptorAngle.AddValueChanged(transformControl, OnAngleChanged);

        _staticCanvas.Children.Add(transformControl);

        void OnAngleChanged(object? sender, EventArgs e)
        {
            var item = transformControl.GetValue(RenderTransformProperty) as RotateTransform;

            freeForm.Angle = (float)item.Angle;
        }

        void OnCanvasTopChanged(object? sender, EventArgs e)
        {
            var item = transformControl.GetValue(Canvas.TopProperty);
            freeForm.Y = (float)(double)item - Effects.Canvas.GridBaselineY;
        }

        void OnCanvasLeftChanged(object? sender, EventArgs e)
        {
            var item = transformControl.GetValue(Canvas.LeftProperty);
            freeForm.X = (float)(double)item - Effects.Canvas.GridBaselineX;
        }

        void NewcontrolSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            freeForm.Width = (float)transformControl.ActualWidth;
            freeForm.Height = (float)transformControl.ActualHeight;
        }
    }

    public static void RemoveKeySequenceElement(FreeFormObject element)
    {
        var existingControl = FindElementByTag(element);

        if (existingControl != null)
        {
            _staticCanvas.Children.Remove(existingControl);
        }
    }

    private static ContentControl? FindElementByTag(FreeFormObject tag)
    {
        foreach (var child in _staticCanvas.Children)
        {
            if (child is not ContentControl control || !ReferenceEquals(tag, control.Tag)) continue;
            return control;
        }

        return null;
    }
}