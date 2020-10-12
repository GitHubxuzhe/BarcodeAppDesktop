using System.Windows;
using System.Windows.Controls;
using BarcodeAppDesktop.Model;

namespace BarcodeAppDesktop.UserControls
{
    /// <summary>
    ///     Interaction logic for LabelDataInputPanel.xaml
    /// </summary>
    public partial class LabelDataInputPanel : UserControl
    {
        public static readonly DependencyProperty LabelDataProperty =
            DependencyProperty.Register("LabelData", typeof (ILabelData), typeof (LabelDataInputPanel),
                new PropertyMetadata(null));

        public LabelDataInputPanel()
        {
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