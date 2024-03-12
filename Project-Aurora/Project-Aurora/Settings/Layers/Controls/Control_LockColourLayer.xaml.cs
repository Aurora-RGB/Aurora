﻿using System.Windows.Controls;

namespace AuroraRgb.Settings.Layers.Controls {

    public partial class Control_LockColourLayer : UserControl {
        public Control_LockColourLayer() {
            InitializeComponent();
            cmbKey.Items.Add(System.Windows.Forms.Keys.CapsLock);
            cmbKey.Items.Add(System.Windows.Forms.Keys.NumLock);
            cmbKey.Items.Add(System.Windows.Forms.Keys.Scroll);
        }

        public Control_LockColourLayer(LockColourLayerHandler datacontext) : this() {
            DataContext = datacontext;
        }
    }
}
