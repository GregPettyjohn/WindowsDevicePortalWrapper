﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceLab
{
    /// <summary>
    /// Interaction logic for DeviceCollectionView.xaml
    /// </summary>
    public partial class DeviceCollectionView : UserControl
    {
        public DeviceCollectionView()
        {
            InitializeComponent();
        }

        private void EatMouseClicks(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}