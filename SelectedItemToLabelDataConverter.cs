using System;
using System.Globalization;
using System.Windows.Data;
using BarcodeAppDesktop.Model;
using BarcodeAppDesktop.Properties;

namespace BarcodeAppDesktop.Converter
{
    class SelectedItemToLabelDataConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public static readonly ILabelData LabelData = new LabelData(false);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var lot = value as ActiveLot;
            if(lot == null)
                throw new ArgumentException("Target type should be ActiveLot");

            LabelData.Plant = lot.Plant;
            LabelData.PartNumber = lot.Material;
            LabelData.Vendor = lot.Vendor;
            LabelData.DeliveryNoteOrAsn = lot.ASN;
            LabelData.DeliveryDate = lot.IncomingDate.ToString();
            LabelData.Quantity = lot.PackageQty.ToString();
            LabelData.Description = lot.MaterialDescription;
            LabelData.IsConsignment = lot.IsConsignment;
            LabelData.DeliveryNote = lot.DeliveryNote;
            LabelData.SAPStorageBin = lot.SAPStorageBin;
            LabelData.LotQuantity = (int)lot.Qty;

            if (Settings.Default.EnableLotQCStatePrint)
                LabelData.IsPQA = lot.QCState == 2;

            return LabelData;
        }

        #endregion
    }
}
