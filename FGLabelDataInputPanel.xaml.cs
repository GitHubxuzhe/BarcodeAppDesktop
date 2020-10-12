using System.Windows;
using System.Windows.Controls;
using BarcodeAppDesktop.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace BarcodeAppDesktop.UserControls
{
    /// <summary>
    ///     Interaction logic for LabelDataInputPanel.xaml
    /// </summary>
    public partial class FGLabelDataInputPanel : UserControl
    {
        public static readonly DependencyProperty LabelDataProperty =
            DependencyProperty.Register("LabelData", typeof (ILabelData), typeof (FGLabelDataInputPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PartNumbersProperty =
            DependencyProperty.Register("PartNumbers", typeof(string[]), typeof(FGLabelDataInputPanel),
                new PropertyMetadata(null));

        public FGLabelDataInputPanel()
        {
            InitializeComponent();
        }

        #region properties

        public string[] PartNumbers
        {
            get { return (string[])GetValue(PartNumbersProperty); }
            set { SetValue(PartNumbersProperty, value); }
        }

        public ILabelData LabelData
        {
            get { return (ILabelData)GetValue(LabelDataProperty); }
            set { SetValue(LabelDataProperty, value); }
        }


        #endregion

        // Using a DependencyProperty as the backing store for LabelData.  This enables animation, styling, binding, etc...
    }
}