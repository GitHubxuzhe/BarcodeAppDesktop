using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BarcodeAppDesktop.Enumrations;

namespace BarcodeAppDesktop.Helper
{
    public class MasterDataHelper
    {
 
        public static string LoadContent(MasterDataEditTypeEnum t)
        {
            var fileName = GetFileName(t);

            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, fileName);
            if (!File.Exists(filePath))
                return string.Empty;

            return File.ReadAllText(filePath);
        }

        public static void SaveContent(MasterDataEditTypeEnum t, string content)
        {
            var fileName = GetFileName(t);

            string filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, fileName);

            File.WriteAllText(filePath, content);
        }

        private static string GetFileName(MasterDataEditTypeEnum t)
        {
            switch (t)
            {
                case MasterDataEditTypeEnum.ProductNumber:
                    return "PartNumber.txt";
                case MasterDataEditTypeEnum.Description:
                    return "Description.txt";
                    
                case MasterDataEditTypeEnum.Vendor:
                    return "Vendor.txt";
            }

            return "Default.txt";
        }
    }
}
