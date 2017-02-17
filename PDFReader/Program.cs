using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Patagames.Ocr;
using Patagames.Ocr.Enums;
using PDFReader.Models;
namespace PDFReader
{
    class Program
    {
        private static string invoice1 = @"C:\data\Franzke\fergInvoice.pdf";
        private static string invoice2 = @"C:\data\Franzke\invoice2.pdf";
        private static string invoice3 = @"C:\data\Franzke\invoice3.pdf";
        private static string invoice4 = @"C:\data\Franzke\invoice4.pdf";
        private static string invoice55 = @"C:\data\Franzke\image.png";
        private static string dataFiles = @"C:\data\Franzke\development\eng.traineddata";
        private static int maximumMissing = 3;
        private static string fileName = invoice1;
        static void Main(string[] args)
        {
            
            StringBuilder text = new StringBuilder();
            var invoiceLines = new List<string[]>();
            //ImageText();
            if (File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (var page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ImageText();
                    var strategy = new LocationTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page);
                    
                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                    var strings = currentText.Split('\n');
                    var invoiceItems = GetInvoiceItems(strings);
                    WriteExcel(page.ToString(), invoiceItems);
                    if (invoiceItems.Count == 0)
                    {
                        
                    }
                }
                pdfReader.Close();
            }  
        }

        public static List<string[]> GetInvoiceItems(string[] invoiceData)
        {
            var columns = new ColumnType().columns;
            var sections = columns.Keys.ToList();
            var headerOrder = new List<string>();
            var invoiceItems = new List<string[]>();
            var lineItemNum = invoiceData[18].ToLower().Contains("description");
            foreach (var item in invoiceData)
            {
                var lineItem = item.ToLower();
                var addedCustomHeader = false;
                if (lineItem.ToLower().Contains("description"))
                {
                    var columnHeaders = item.Split(' ');

                    for (var i = 0; i < columnHeaders.Length; i++)
                    {
                        foreach (var section in sections)
                        {
                            var headerOne = columnHeaders[i].ToLower();
                            var headerTwo = i + 1 == columnHeaders.Length?"11111":columnHeaders[i + 1].ToLower();
                            if (section.ToLower().Contains(headerOne + " " + headerTwo))
                            {
                                headerOrder.Add(headerOne + " " + headerTwo);
                                addedCustomHeader = true;
                                i++;
                                break;
                            }

                        }
                        if (addedCustomHeader)
                        {
                            addedCustomHeader = false;
                        }
                        else
                        {
                            headerOrder.Add(columnHeaders[i].ToLower());
                        }
                    }
                }
                if (headerOrder.Count > 0)
                {
                    var inventoryLineArray = item.Split(' ');
                    var inventoryLineItem = GetAllInvoices(headerOrder, inventoryLineArray);
                    if (!CleanUpCheck(inventoryLineItem, maximumMissing))
                        invoiceItems.Add(inventoryLineItem);
                }
            }

            return invoiceItems;
        }

        public static string FindNextInventoryItem(string[] inventoryLineItem, int index, string freeFormString, string type)
        {
            int itemInt;
            decimal itemDecimal;
            if (type == "string")
            {
                
            }
            if (int.TryParse(inventoryLineItem[index], out itemInt))
            {
                return itemInt.ToString();
            }
            if (decimal.TryParse(inventoryLineItem[index], out itemDecimal))
            {
                return itemDecimal.ToString();
            }
            else
            {
                freeFormString += inventoryLineItem[index];
                index++;

            }
            return "";
        }

        public static string[] GetAllInvoices(List<string> columnHeaders, string[] inventoryLineItem)
        {
            var sections = new ColumnType().columns;
            var enums = new ColumnType().enums;
            var itemList = new string[columnHeaders.Count];
            for (int i = 0, j = 0; i < columnHeaders.Count && j < inventoryLineItem.Length; i++, j++)
            {
                var headerInfo = sections.FirstOrDefault(k => k.Key == columnHeaders[i]);
                switch (headerInfo.Value)
                {
                    case "number":
                        double itemValue;
                        if (double.TryParse(inventoryLineItem[j], out itemValue))
                        {
                            itemList[i] = itemValue.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    case "enum":
                        var columnEnum = enums.FirstOrDefault(k => k.Key == headerInfo.Key).Value;
                        if (columnEnum != null)
                        {
                            if (columnEnum.Contains(inventoryLineItem[j].Trim().ToLower()))
                            {
                                itemList[i] = inventoryLineItem[j];
                            }
                        }
                        break;
                    case "single":
                        itemList[i] = inventoryLineItem[j];
                        break;
                    case "decimal":
                        int itemDecimalValue;
                        int.TryParse(inventoryLineItem[j], out itemDecimalValue);
                        if (itemDecimalValue > 0)
                        {
                            itemList[i] = itemDecimalValue.ToString();
                        }
                        break;
                    case "freeform":
                        var remainingHeaders = columnHeaders.GetRange(i, columnHeaders.Count - i);
                        var remainingInvoiceItems = inventoryLineItem.ToList().GetRange(j, inventoryLineItem.Length - j);
                        if (!remainingHeaders.Contains("freeform"))
                        {
                            if (remainingHeaders.Count != remainingInvoiceItems.Count)
                            {
                                itemList[i] += " " + inventoryLineItem[j];
                                i--;
                            }
                        }
                        break;
                }
            }
            return itemList;

        }

        public static List<Invoice> GetInvoices(string[] invoiceData)
        {
            var sections = new [] {"none", "description", "item number", "shipped", "ordered", "sub-total", "um", "amount", "unit price"};
            var section = "none";
            var index = 0;
            var totalItems = 0;
            var invoiceItems = new List<Invoice>();

            foreach (var item in invoiceData)
            {
                var itemName = item.ToLower().Trim();
                if (sections.Contains(item.ToLower().Trim()))
                {
                    section = item.ToLower().Trim();
                    index = -1;
                }
                switch (section)
                {
                    case "ordered":
                        if (index >= 0)
                        {
                            var invoice = new Invoice();
                            int ordered = -1;
                            int.TryParse(item, out ordered);
                            invoice.Ordered = ordered;
                            invoiceItems.Add(invoice);
                        }
                        index++;
                        totalItems++;
                        break;
                    case "description":
                        if (index >= 0 && index < totalItems - 1)
                        {
                            invoiceItems[index].Description = item;
                        }
                        index++;
                        break;
                    case "item number":
                        if (index >= 0 && index < totalItems - 1)
                        {
                            invoiceItems[index].ItemNumber = item;
                        }
                        index++;
                        break;
                    case "shipped":
                        if (index >= 0 && index < totalItems - 1)
                        {
                            var shipped = -1;
                            int.TryParse(item, out shipped);
                            invoiceItems[index].Shipped = shipped;
                        }
                        index++;
                        break;
                    case "um":
                        if (index >= 0 && index < totalItems - 1)
                        {
                            invoiceItems[index].UnitOfMeasure = item;
                        }
                        index++;
                        break;
                    case "amount":
                        if (index >= 0 && index < totalItems - 1)
                        {
                            double amount;
                            double.TryParse(item, out amount);
                            invoiceItems[index].Amount = amount;
                        }
                        index++;
                        break;
                    case "unit price":
                        if (index >= 0 && index < totalItems - 1)
                        {
                            double amount;
                            double.TryParse(item, out amount);
                            invoiceItems[index].UnitPrice = amount;
                        }
                        index++;
                        break;
                }
                
            }
            return invoiceItems;
        }

        public static bool CleanUpCheck(string[] items, int maximumMissingRecords)
        {
            return items.Count(i => i == null) >= maximumMissingRecords;
        }

        public static void WriteExcel(string name, List<string[]> invoiceItems)
        {
            var stringBuilder = new StringBuilder();
            foreach (var line in invoiceItems)
            {
                foreach (var item in line)
                {
                    stringBuilder.Append(item);
                    stringBuilder.Append(",");
                }
                stringBuilder.Append("\n");
            }
            File.WriteAllText(@"C:\data\Franzke\"+name+".csv", stringBuilder.ToString());
        }

        public static void ImageText()
        {
            try
            {
                using (var api = OcrApi.Create())
                {
                    api.Init(Languages.English);
                    string plaintext = api.GetTextFromImage(invoice55);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Error: " + e.Message);
            }
        }
    }
}
