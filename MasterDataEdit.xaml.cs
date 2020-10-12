using BarcodeAppDesktop.Helper;
using GalaSoft.MvvmLight.Messaging;
using System;
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
using System.Windows.Shapes;

namespace BarcodeAppDesktop.View
{
    /// <summary>
    /// Interaction logic for PartNumberEdit.xaml
    /// </summary>
    public partial class MasterDataEdit : Window
    {
        public MasterDataEdit()
        {
            InitializeComponent();

            Messenger.Default.Register<NotificationMessage>(this, (nm) =>
            {
                if (nm.Notification == Consts.MESSAGE_TYPE_CLOSE_WINDOW)
                {
                    if (nm.Sender == this.DataContext)
                        this.Close();
                }
            });
        }
    }
}
