using System;
using System.Windows;
using System.Windows.Controls;
using BarcodeAppDesktop.Model;

namespace BarcodeAppDesktop.UserControls
{
    /// <summary>
    ///     Interaction logic for AIAGLabelView.xaml
    /// </summary>
    [Serializable]
    public partial class AIAGLabelView : UserControl
    {
        public static readonly DependencyProperty LabelDataProperty =
            DependencyProperty.Register("LabelData", typeof (ILabelData), typeof (AIAGLabelView),
                new PropertyMetadata(null));

        public AIAGLabelView()
        {
            // Required to initialize variables
            InitializeComponent();
        }

        public ILabelData LabelData
        {
            get { return (ILabelData) GetValue(LabelDataProperty); }
            set { SetValue(LabelDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelData.  This enables animation, styling, binding, etc...
    }
}