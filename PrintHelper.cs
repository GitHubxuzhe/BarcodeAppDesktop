using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using BarcodeAppDesktop.Converter;
using BarcodeAppDesktop.Enumrations;
using BarcodeAppDesktop.Model;
using BarcodeAppDesktop.Properties;
using Honeywell.MHS.ClientLib;
using Honeywell.MHS.ClientLib.Services.NewContainerId;

namespace BarcodeAppDesktop.Helper
{
    public static class PrintHelper
    {
        public static string LastPrinter { get; set; }

        public static void ExecutePrintCommand(UIElement labelView, ILabelData labelData)
        {
            if (labelData == null)
            {
                MessageBox.Show("Please query and select a lot first!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;  
            }

            if (string.IsNullOrWhiteSpace(labelData.NumOfPCs))
            {
                MessageBox.Show("Please enter number of labels!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!labelData.IsValid)
                return;

            if (Settings.Default.EnableContainerGenerationOption && Settings.Default.EnableContainerGeneration)
            {
                PrintWithContainerGeneration(labelView, labelData);
            }
            else
            {
                int iLabels = Convert.ToInt32(labelData.NumOfPCs);

                Settings.Default.LastUsedVendor = labelData.Vendor;
                switch (Settings.Default.PrinterType)
                {
                    case PrinterTypeEnum.Normal:
                        {
                            PrintQueue printQueue;
                            try
                            {
                                printQueue = new LocalPrintServer().GetPrintQueue(Settings.Default.LastPrinterQueue);
                            }
                            catch (Exception)
                            {
                                printQueue = null;
                            }

                            if (Settings.Default.AutoSelectPrinter && !string.IsNullOrWhiteSpace(Settings.Default.LastPrinterQueue) && printQueue != null
                            )
                            {
                                //   printQueue = new LocalPrintServer().GetPrintQueue(Settings.Default.LastPrinterQueue);
                            }
                            else
                            {
                                var pd = new PrintDialog();
                                if (pd.ShowDialog() != true)
                                    return;

                                printQueue = pd.PrintQueue;
                                Settings.Default.LastPrinterQueue = printQueue.Name;
                            }

                            DrawingVisual dvA4 = BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA4, 610, 400),
                                labelView, 2);

                            DrawingVisual dvA5 = BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA5, 610, 400),
                                labelView, 1);

                            var stream = new MemoryStream();
                            const string pack = "pack://temp.xps";
                            var uri = new Uri(pack);

                            using (Package container = Package.Open(stream, FileMode.Create))
                            {
                                PackageStore.AddPackage(uri, container);
                                using (var xpsDoc = new XpsDocument(container, CompressionOption.Fast, pack))
                                {
                                    XpsDocumentWriter xpsDocWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                                    var visualToXpsDoc = (VisualsToXpsDocument)xpsDocWriter.CreateVisualsCollator();
                                    if (visualToXpsDoc != null)
                                    {
                                        visualToXpsDoc.BeginBatchWrite();
                                        if (Settings.Default.OneLabelPerPage)
                                        {
                                            for (int i = 0; i < iLabels; ++i)
                                            {
                                                visualToXpsDoc.Write(dvA5);
                                            }
                                        }
                                        else
                                        {
                                            for (int i = 0; i < iLabels / 2; ++i)
                                            {
                                                visualToXpsDoc.Write(dvA4);
                                            }

                                            int iRemain = iLabels % 2;
                                            if (iRemain != 0)
                                            {
                                                visualToXpsDoc.Write(BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA5, 610, 400),
             labelView, iRemain));
                                            }
                                        }
                                        visualToXpsDoc.EndBatchWrite();

                                        FixedDocumentSequence fixedDocumentSequence = xpsDoc.GetFixedDocumentSequence();

                                        if (fixedDocumentSequence != null)
                                        {
                                            XpsDocumentWriter xpsWriter = PrintQueue.CreateXpsDocumentWriter(printQueue);

                                            xpsWriter.Write(fixedDocumentSequence);
                                            //pd.PrintDocument(fixedDocumentSequence.DocumentPaginator, "label");
                                        }
                                        else
                                        {
                                            throw new Exception("fixedDocumentSequence is null");
                                        }
                                    }
                                }
                                PackageStore.RemovePackage(uri);
                            }
                        }
                        break;
                    case PrinterTypeEnum.Zebra:
                        {
                            string stringZPL = BuildZPLString(labelData);
                            ZebraPrinter zebraPrinter = ZebraPrinter.Get();
                            if (Settings.Default.ZebraConnectionType == ZebraConnectionTypeEnum.USB)
                            {
                                PrintQueue printQueue;
                                if (Settings.Default.AutoSelectPrinter && !string.IsNullOrWhiteSpace(Settings.Default.LastPrinterQueue))
                                {
                                    printQueue = new LocalPrintServer().GetPrintQueue(Settings.Default.LastPrinterQueue);
                                }
                                else
                                {
                                    var pd = new PrintDialog();
                                    if (pd.ShowDialog() != true)
                                        return;

                                    printQueue = pd.PrintQueue;
                                    Settings.Default.LastPrinterQueue = printQueue.Name;
                                }

                                zebraPrinter.Connection = new USBConnection(printQueue.Name);
                            }
                            else
                            {
                                zebraPrinter.Connection = new SerialConnection(Settings.Default.ZebraComPort);
                            }

                            if (Settings.Default.EnableAutoLabelQtyCalculation)
                            {
                                AutoCalculateLabelQty(labelView, zebraPrinter, labelData, iLabels);
                            }
                            else
                            {
                                for (int j = 0; j < iLabels; ++j)
                                    zebraPrinter.Print(stringZPL);
                            }
                        }
                        break;
                }
            }
        }

        public static DrawingVisual BuildGraphVisual(PageMediaSize pageSize, Visual visual, int iNumOfLabels)
        {
            if (iNumOfLabels < 1)
                return null;

            var drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                Visual visualContent = visual;
                Debug.Assert(pageSize.Width != null, "pageSize.Width != null");
                Debug.Assert(pageSize.Height != null, "pageSize.Height != null");
                var rect = new Rect
                {
                    X = 0,
                    Y = 0,
                    Width = pageSize.Width.Value,
                    Height = pageSize.Height.Value,
                };

                const Stretch stretch = Stretch.None;
                var transform = new TranslateTransform(Settings.Default.NormalOffsetX, Settings.Default.NormalOffsetY);
                var visualBrush = new VisualBrush(visualContent) { Stretch = stretch };

                if (Settings.Default.NormalPrintDirection == ZebraPrintDirectionEnum.Landscape)
                {
                    RotateTransform rotate = new RotateTransform(90, rect.Width / 2, rect.Height);
                    rotate.Angle = 90;
                    drawingContext.PushTransform(rotate);
                }

                drawingContext.PushTransform(new ScaleTransform(Settings.Default.PrintZoomRatio, Settings.Default.PrintZoomRatio));
                drawingContext.PushTransform(transform);
                drawingContext.DrawRectangle(visualBrush, null, rect);

                for (int i = 0; i < iNumOfLabels - 1; ++i)
                {
                   drawingContext.PushTransform(new TranslateTransform(0, 460));
                   drawingContext.DrawRectangle(visualBrush, null, rect);
                }
            }
            return drawingVisual;
        }

        public static string BuildZPLString(ILabelData labelData)
        {
            bool prefix = Settings.Default.EnableLabelPrefix;

            var QRConverter = new QrStringCatConverter();
            var QRString =
                QRConverter.Convert(
                    new object[]
                    {
                        (prefix ? "P" : string.Empty) + labelData.PartNumber,
                        (prefix ? "Q" : string.Empty) + labelData.Quantity, 
                        (prefix ? "V" : string.Empty) + labelData.Vendor,
                        (prefix ? "5K" : string.Empty) + labelData.DeliveryNoteOrAsn,
                        (prefix ? "N" : string.Empty) + labelData.DeliveryNote,
                        labelData.DeliveryDate,
                        labelData.IsConsignment ? "K" : ""
                    }, typeof(string), null, null) as string;

            if (Settings.Default.ZebraPrintDirection == ZebraPrintDirectionEnum.Landscape)
            {
                var strToPrint = ZPLTemplate.str200DPIWithQRSmallAndKQ;

                switch (Settings.Default.ZebraDPI)
                {
                        case DPITypeEnum.DPI200:
                            strToPrint = ZPLTemplate.str200DPIWithQRSmallAndKQ;
                            break;
                        case DPITypeEnum.DPI300:
                            strToPrint = ZoomTo300DPI(ZPLTemplate.str200DPIWithQRSmallAndKQ);
                            break;
                        case DPITypeEnum.DPI400:
                            strToPrint = ZoomTo400DPI(ZPLTemplate.str200DPIWithQRSmallAndKQ);
                            break;
                }

                return ProcessTemplate(strToPrint, 
                                        labelData.Plant,
                                        labelData.Vendor, 
                                        labelData.Quantity, 
                                        labelData.PartNumber,
                                        labelData.DeliveryNoteOrAsn, 
                                        labelData.Description, 
                                        labelData.DeliveryDate, 
                                        QRString,
                                        Settings.Default.ZebraOffsetX, 
                                        Settings.Default.ZebraOffsetY, 
                                        labelData.IsConsignment ? "K" : "", 
                                        labelData.IsPQA ? "Q" : "",
                                        labelData.SN,
                                        labelData.IdContainer,
                                        labelData.DeliveryNote,
                                        labelData.SAPStorageBin);         
            }
            else
            {
                var strToPrint = ZPLTemplate.str200DPIWithQRSmallAndKQ_Rotate90;

                switch (Settings.Default.ZebraDPI)
                {
                    case DPITypeEnum.DPI200:
                        strToPrint = ZPLTemplate.str200DPIWithQRSmallAndKQ_Rotate90;
                        break;
                    case DPITypeEnum.DPI300:
                        strToPrint = ZoomTo300DPI(ZPLTemplate.str200DPIWithQRSmallAndKQ_Rotate90);
                        break;
                    case DPITypeEnum.DPI400:
                        strToPrint = ZoomTo400DPI(ZPLTemplate.str200DPIWithQRSmallAndKQ_Rotate90);
                        break;
                }

                return ProcessTemplate(strToPrint, 
                                        labelData.Plant,
                                        labelData.Vendor, 
                                        labelData.Quantity, 
                                        labelData.PartNumber,
                                        labelData.DeliveryNoteOrAsn, 
                                        labelData.Description, 
                                        labelData.DeliveryDate, 
                                        QRString,
                                        Settings.Default.ZebraOffsetX, 
                                        Settings.Default.ZebraOffsetY, 
                                        labelData.IsConsignment ? "K" : "", 
                                        labelData.IsPQA ? "Q" : "",
                                        labelData.SN,
                                        labelData.IdContainer,
                                        labelData.DeliveryNote,
                                        labelData.SAPStorageBin);                  
            }


        }

        private static string ProcessTemplate(  string template,
                                                string plant,
                                                string vendor,
                                                string quantity,
                                                string partNumber,
                                                string deliveryNoteOrAsn,
                                                string description,
                                                string deliveryDate,
                                                string qrString,
                                                int zebraOffsetX,
                                                int zebraOffsetY,
                                                string isConsignment,
                                                string isPQA,
                                                string sn,
                                                string idContainer,
                                                string deliveryNote,
                                                string sapStorageBin)
        {

            var containerLine = string.Empty;
            if (Settings.Default.ZebraPrintDirection == ZebraPrintDirectionEnum.Landscape)
            {
                var generateContainerTemplate = Settings.Default.ContainerGenerationTemplate;
                if (!string.IsNullOrWhiteSpace(generateContainerTemplate) && !string.IsNullOrWhiteSpace(idContainer))
                {
                    containerLine = generateContainerTemplate.Replace("{{IdContainer}}",
                        string.IsNullOrWhiteSpace(idContainer) ? "" : idContainer);
                }
            }
            else
            {
                var generateContainerTemplateRotate90 = Settings.Default.ContainerGenerationTemplateRotate90;
                if (!string.IsNullOrWhiteSpace(generateContainerTemplateRotate90) && !string.IsNullOrWhiteSpace(idContainer))
                {
                    containerLine = generateContainerTemplateRotate90.Replace("{{IdContainer}}",
                        string.IsNullOrWhiteSpace(idContainer) ? "" : idContainer);
                }
            }
            

            return template.Replace("{{PLANT}}", plant)
                            .Replace("{{VENDOR}}", vendor)
                            .Replace("{{QUANTITY}}", quantity)
                            .Replace("{{PART_NUMBER}}", partNumber)
                            .Replace("{{DELIVERY_NOTE_OR_ASN}}", deliveryNoteOrAsn)
                            .Replace("{{DESCRIPTION}}", description)
                            .Replace("{{DELIVERY_DATE}}", deliveryDate)
                            .Replace("{{QR_STRING}}", qrString)
                            .Replace("{{ZEBRA_OFFSET_X}}", zebraOffsetX.ToString())
                            .Replace("{{ZEBRA_OFFSET_Y}}", zebraOffsetY.ToString())
                            .Replace("{{IS_CONSIGNMENT}}", isConsignment)
                            .Replace("{{IS_PQA}}", isPQA)
                            .Replace("{{SN}}", string.IsNullOrWhiteSpace(sn) ? "" : sn)
                            .Replace("{{IdContainer}}", string.IsNullOrWhiteSpace(containerLine) ? "" : containerLine)
                            .Replace("{{DELIVERY_NOTE}}", deliveryNote)
                            .Replace("{{SAP_STORAGE_BIN}}", string.IsNullOrWhiteSpace(sapStorageBin) ? "" : sapStorageBin);
        }

        private static string ZoomTo300DPI(string ZPLString)
        {
            var regex = new Regex(@"(?<pre>[^{])(?<num>\d+)");
            return regex.Replace(ZPLString, To300DPI, 200);
        }

        private static string ZoomTo400DPI(string ZPLString)
        {
            var regex = new Regex(@"(?<pre>[^{])(?<num>\d+)");
            return regex.Replace(ZPLString, To400DPI, 200);
        }

        private static string To300DPI(Match match)
        {
            return match.Groups["pre"].Value + ((int)Math.Round((Convert.ToInt32(match.Groups["num"].Value) * 1.5f))).ToString();
        }

        private static string To400DPI(Match match)
        {
            return match.Groups["pre"].Value + ((int)Math.Round((Convert.ToInt32(match.Groups["num"].Value) * 2f))).ToString();
        }

        #region Container Generation


        private static void PrintWithContainerGeneration(UIElement labelView, ILabelData labelData)
        {
            int iLabels = Convert.ToInt32(labelData.NumOfPCs);
            
            Settings.Default.LastUsedVendor = labelData.Vendor;
            switch (Settings.Default.PrinterType)
            {
                case PrinterTypeEnum.Normal:
                    {
                        PrintQueue printQueue;
                        try
                        {
                            printQueue = new LocalPrintServer().GetPrintQueue(Settings.Default.LastPrinterQueue);
                        }
                        catch (Exception)
                        {
                            printQueue = null;
                        }

                        if (Settings.Default.AutoSelectPrinter && !string.IsNullOrWhiteSpace(Settings.Default.LastPrinterQueue) && printQueue != null
                        )
                        {
                            //   printQueue = new LocalPrintServer().GetPrintQueue(Settings.Default.LastPrinterQueue);
                        }
                        else
                        {
                            var pd = new PrintDialog();
                            if (pd.ShowDialog() != true)
                                return;

                            printQueue = pd.PrintQueue;
                            Settings.Default.LastPrinterQueue = printQueue.Name;
                        }



                        var stream = new MemoryStream();
                        const string pack = "pack://temp.xps";
                        var uri = new Uri(pack);

                        using (Package container = Package.Open(stream, FileMode.Create))
                        {
                            PackageStore.AddPackage(uri, container);
                            using (var xpsDoc = new XpsDocument(container, CompressionOption.Fast, pack))
                            {
                                XpsDocumentWriter xpsDocWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                                var visualToXpsDoc = (VisualsToXpsDocument)xpsDocWriter.CreateVisualsCollator();
                                if (visualToXpsDoc != null)
                                {
                                    #region due to the container id cannot be retrieve during labelview calucaltion, only support 1 label 1 page.

                                    for (int i = 0; i < iLabels; ++i)
                                    {
                                        
                                        AssignNewContainerToLabel(labelView, labelData);
                                        
                                        DrawingVisual dvA5 = BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA5, 610, 400),
                                            labelView, 1);
                                        
                                        visualToXpsDoc.Write(dvA5);
                                    }

                                    #region comment out code

                                    //                                    if (Settings.Default.OneLabelPerPage)
                                    //                                    {
                                    //                                        for (int i = 0; i < iLabels; ++i)
                                    //                                        {
                                    //
                                    //                                            AssignNewContainerToLabel(labelView, labelData);
                                    //
                                    //                                            DrawingVisual dvA5 = BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA5, 610, 400),
                                    //                                                labelView, 1);
                                    //
                                    //                                            visualToXpsDoc.Write(dvA5);
                                    //                                        }
                                    //                                    }
                                    //                                    else
                                    //                                    {
                                    //                                        for (int i = 0; i < iLabels / 2; ++i)
                                    //                                        {
                                    //                                            AssignNewContainerToLabel(labelView, labelData);
                                    //
                                    //                                            DrawingVisual dvA4 = BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA4, 610, 400),
                                    //                                                labelView, 2);
                                    //
                                    //                                            visualToXpsDoc.Write(dvA4);
                                    //                                        }
                                    //
                                    //                                        int iRemain = iLabels % 2;
                                    //                                        if (iRemain != 0)
                                    //                                        {
                                    //                                            AssignNewContainerToLabel(labelView, labelData);
                                    //
                                    //                                            visualToXpsDoc.Write(BuildGraphVisual(new PageMediaSize(PageMediaSizeName.ISOA5, 610, 400),
                                    //         labelView, iRemain));
                                    //                                        }
                                    //                                    }

                                    #endregion

                                    #endregion

                                    visualToXpsDoc.BeginBatchWrite();


                                    visualToXpsDoc.EndBatchWrite();

                                    FixedDocumentSequence fixedDocumentSequence = xpsDoc.GetFixedDocumentSequence();

                                    if (fixedDocumentSequence != null)
                                    {
                                        XpsDocumentWriter xpsWriter = PrintQueue.CreateXpsDocumentWriter(printQueue);

                                        xpsWriter.Write(fixedDocumentSequence);
                                        //pd.PrintDocument(fixedDocumentSequence.DocumentPaginator, "label");
                                    }
                                    else
                                    {
                                        throw new Exception("fixedDocumentSequence is null");
                                    }
                                }
                            }
                            PackageStore.RemovePackage(uri);
                        }
                    }
                    break;
                case PrinterTypeEnum.Zebra:
                    {
                        ZebraPrinter zebraPrinter = ZebraPrinter.Get();
                        if (Settings.Default.ZebraConnectionType == ZebraConnectionTypeEnum.USB)
                        {
                            PrintQueue printQueue;
                            if (Settings.Default.AutoSelectPrinter && !string.IsNullOrWhiteSpace(Settings.Default.LastPrinterQueue))
                            {
                                printQueue = new LocalPrintServer().GetPrintQueue(Settings.Default.LastPrinterQueue);
                            }
                            else
                            {
                                var pd = new PrintDialog();
                                if (pd.ShowDialog() != true)
                                    return;

                                printQueue = pd.PrintQueue;
                                Settings.Default.LastPrinterQueue = printQueue.Name;
                            }

                            zebraPrinter.Connection = new USBConnection(printQueue.Name);
                        }
                        else
                        {
                            zebraPrinter.Connection = new SerialConnection(Settings.Default.ZebraComPort);
                        }

                        if (Settings.Default.EnableAutoLabelQtyCalculation)
                        {
                            AutoCalculateLabelQty(labelView, zebraPrinter, labelData, iLabels);
                        }
                        else
                        {
                            for (int j = 0; j < iLabels; ++j)
                            {
                                AssignNewContainerToLabel(labelView, labelData);

                                string stringZPL = BuildZPLString(labelData);
                                zebraPrinter.Print(stringZPL);
                            }
                        }

                    }
                    break;
            }

            labelData.IdContainer = string.Empty;
            labelView.UpdateLayout();
        }

        private static void AutoCalculateLabelQty(UIElement labelView, ZebraPrinter zebraPrinter, ILabelData labelData, int labelCount)
        {
            var unitQty = int.Parse(labelData.Quantity);
            var numOfLabels = labelCount;
            var totalQty = (int)labelData.LotQuantity;

            var quantityExceedError = "Printing quantity exceeds lot quantity, please correct the quantity.";

            if (numOfLabels == 1)
            {
                if (unitQty > totalQty && Settings.Default.DataInputType == DataInputSourceEnum.Import)
                {
                    MessageBox.Show(quantityExceedError, "Quantity Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AssignNewContainerToLabel(labelView, labelData);

                string stringZPL = BuildZPLString(labelData);
                zebraPrinter.Print(stringZPL);
            }
            else if (numOfLabels > 1)
            {

                if (((unitQty*numOfLabels) >= (totalQty + unitQty)) && Settings.Default.DataInputType == DataInputSourceEnum.Import) //add an additional label quantity
                {
                    MessageBox.Show(quantityExceedError, "Quantity Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var originalLabelQuantity = labelData.Quantity; //need to restore label quantity later.

                var remainQty = totalQty;
                var printedCount = 0;
                while (remainQty > 0)
                {
                    if (printedCount >= numOfLabels)
                        break;

                    var currentQty = unitQty;
                    if (remainQty < currentQty)
                        currentQty = remainQty;

                    labelData.Quantity = currentQty.ToString();

                    AssignNewContainerToLabel(labelView, labelData);

                    string stringZPL = BuildZPLString(labelData);
                    zebraPrinter.Print(stringZPL);

                    remainQty -= (unitQty);
                    printedCount++;
                }

                labelData.Quantity = originalLabelQuantity;
            }
        }

        private static void AssignNewContainerToLabel(UIElement labelView, ILabelData labelData)
        {
            if (Settings.Default.EnableContainerGeneration)
            {
                string newContainerId = GenerateNewContainerId();
                labelData.IdContainer = newContainerId;
                labelView.UpdateLayout();
            }
        }

        private static string GenerateNewContainerId()
        {
            try
            {
                var setting = new MHSSetting
                {
                    ManagerServer = Settings.Default.MHSServer,
                    ManagerPort = Settings.Default.MHSPort,
                    ModuleName = Settings.Default.MHSModule
                };

                NewContainerId svc = new NewContainerId(setting);

                string msg = string.Empty;
                var result = svc.Call(new NewContainerRequest
                {
                    NumberOfContainers = 1,
                    Type = NewContainerType.Create,
                    IdPrinter = string.Empty,
                    IdContainerToReprint = string.Empty,
                    Format = NewContainerFormat.Normal
                }, out msg);

                if (result.Count == 0)
                {
                    MessageBox.Show("Got empty container.");
                    return string.Empty;
                }

                return result[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Generate Container Id Error: " + ex.Message);
                return String.Empty;
            }

        }

        #endregion
    }
}
