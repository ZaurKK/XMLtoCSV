using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;


namespace XMLtoCSVconvertor
{
    public partial class Form1 : Form
    {
        public Form1() => InitializeComponent();

        public static Dictionary<int, String> LPUs = new Dictionary<int, string>()
        {
            { 70510, "ГП 1 г.Нальчик" },
            { 70570, "ГП 2 г.Нальчик" },
            { 70571, "ГП 3 г.Нальчик" },
            { 70103, "Баксанской ЦРБ" },
            { 70105, "РБ с.Заюково" },
            { 70201, "Зольской ЦРБ" },
            { 70405, "Майской ЦРБ" },
            { 70616, "Прохладненской ЦРБ" },
            { 70714, "Терской ЦРБ" },
            { 70807, "ММБ г.Нарткала" },
            { 70906, "Чегемской ЦРБ" },
            { 71001, "Черекской ЦРБ" },
            { 71103, "Эльбрусской ЦРБ" },
            { 71106, "УБ с.Эльбрус" }
        };

        public Form1(FolderBrowserDialog folderBrowserDialog)
        {
            this.folderBrowserDialog = folderBrowserDialog;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                processFiles();
        }

        private void processFiles()
        {
            if (String.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
                folderBrowserDialog.SelectedPath = textBox1.Text;

            string extr_path = textBox2.Text;

            String outputFilePath = Path.Combine(extr_path, "M_All.csv");
            if (File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            StreamWriter swAll = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));

            var archives = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.zip");           //переменна по свойствам диспансеризации
            foreach (var archive in archives)
            {
                ZipFile.ExtractToDirectory(archive, extr_path);
                string[] dplFiles = Directory.GetFiles(extr_path, "DPLM*");           //переменная по свойствам диспансеризации
                string[] lFiles = Directory.GetFiles(extr_path, "LM*");               //переменная по данным пациентов

                String dplFileFirst = dplFiles.First();
                String lFileFirst = dplFiles.First();
                if (String.IsNullOrEmpty(dplFileFirst) || String.IsNullOrEmpty(lFileFirst))
                    return;

                String dplFileName = Path.GetFileName(dplFileFirst);
                String lpuMO = dplFileName.Substring(3, 14);

                int lpuId = 0;
                if (!int.TryParse(lpuMO.Substring(1, 6), out lpuId))
                    return;

                //int lpuId = int.Parse(Mid(fname2, 0, 6));

                //объединяем 2 XML в 10
                XDocument xdoc1 = XDocument.Load(dplFiles.First());
                XDocument xdoc2 = XDocument.Load(lFiles.First());
                //Берем данные из первого XML               
                var data1 = xdoc1.Root.Elements("ZAP")
                .Select(x => new
                {
                    Disp = (string)x.Element("DISP"),
                    PacientId = (string)x.Element("PACIENT").Element("ID_PAC"),
                    Quarter = (string)x.Element("SLUCH").Element("QUARTER")
                });

                //Берем данные из второго XML
                var data2 = xdoc2.Root.Elements("PERS")
                .Select(x => new
                {
                    PacientId = (string)x.Element("ID_PAC"),
                    LastName = (string)x.Element("FAM"),
                    FirstName = (string)x.Element("IM"),
                    MiddleName = (string)x.Element("OT"),
                    Birthdate = (string)x.Element("DR")//,
                    //Sex = (int)x.Element("W")
                });

                //Сохраняем данные из первых двух XML в третий
                var data3 = data2.Join(data1, outer => outer.PacientId, inner => inner.PacientId, (outer, inner) => new { Data1 = inner, Data2 = outer });
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Data", data3.Select(x => new XElement("Item",
                                new XElement("ID_PAC", x.Data2.PacientId),
                                new XElement("DISP", x.Data1.Disp),
                                new XElement("QUARTER", x.Data1.Quarter),
                                new XElement("FAM", x.Data2.LastName),
                                new XElement("IM", x.Data2.FirstName),
                                new XElement("OT", x.Data2.MiddleName),
                                new XElement("DR", x.Data2.Birthdate)//,
                                //new XElement("W", x.Data2.Sex)
                            )
                        )
                    )
                );
                String resultXmlFilePath = Path.Combine(extr_path, String.Concat(lpuMO, ".xml"));
                doc.Save(resultXmlFilePath);

                //конвертируем объединенный XML в CSV
                StringBuilder sb = new StringBuilder();
                string delimiter = ";";                                                                 //разделитель
                string resultCsvFilePath = Path.Combine(extr_path, String.Concat(lpuMO, ".csv"));
                XDocument.Load(resultXmlFilePath).Descendants("Item").ToList().ForEach(element => sb.Append(
                    element.Element("FAM").Value + delimiter +
                    element.Element("IM").Value + delimiter +
                    element.Element("OT").Value + delimiter +
                    element.Element("DR").Value + delimiter +
                    //element.Element("W").Value + delimiter +
                    element.Element("QUARTER").Value + delimiter +
                    element.Element("DISP").Value + delimiter +
                    LPUs[lpuId] + "\r\n"));
                StreamWriter sw = new StreamWriter(resultCsvFilePath, false, Encoding.GetEncoding("Windows-1251"));
                sw.Write(sb.ToString());
                sw.Close();

                String input = sb.ToString();
                String[] inputArray = input.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                String[] distinctInputArray = inputArray.Distinct().ToArray();
                StringBuilder result = new StringBuilder();
                foreach (String s in distinctInputArray)
                {
                    result.Append(s);
                    result.Append("\r\n");
                }
                swAll.Write(result.ToString());

                //Удаляем xml-файлы в папке
                string mask = ".xml";
                string[] fileNames = Directory.GetFiles(extr_path, "*" + mask, SearchOption.AllDirectories);
                string fileErrors = string.Empty;
                for (int j = 0; j != fileNames.Length; j++)
                {
                    try
                    {
                        File.Delete(fileNames[j]);
                    }
                    catch
                    {
                        fileErrors = fileErrors + "\n" + fileNames[j];
                    }
                }
            }
            swAll.Close();
            DialogResult dialogResult = MessageBox.Show("Файлы сформированы!\r\n\r\nВыйти из программы?", "Операция завершена", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
                Application.Exit();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ofd.InitialDirectory = @"\\192.168.0.221\vipnetprocess\disp";
            //ofd.Filter = "ZIP Files |*.zip";
            //ofd.RestoreDirectory = true;
            //if (ofd.ShowDialog() == DialogResult.OK)
            //{
            //    string zip_fnam = ofd.FileName;
            //    string extr_path2 = @"\\192.168.0.221\vipnetprocess\disp\disp2\"; //папка для выгрузки файлов и работы с ними
            //    ZipFile.ExtractToDirectory(zip_fnam, extr_path2);
            //    string[] files1 = Directory.GetFiles(extr_path2, "DPLM*"); //переменна по свойствам диспансеризации
            //    string[] files2 = Directory.GetFiles(extr_path2, "LM*"); //переменная по данным пациентов
            //    string fname = Right(files1.First(), 22); //маска для объединенного XML
            //    string fname2 = Mid(fname, 4, 14);


            //    //объединяем 2 XML в 1
            //    XDocument xdoc1 = XDocument.Load(files1.First());
            //    XDocument xdoc2 = XDocument.Load(files2.First());

            //    //Берем данные из первого XML               
            //    var data1 = xdoc1.Root.Elements("ZAP")
            //        .Select(x => new
            //        {
            //            Disp = (string)x.Element("DISP"),
            //            PacientId = (string)x.Element("PACIENT").Element("ID_PAC"),
            //            Quarter = (string)x.Element("SLUCH").Element("QUARTER"),
            //            Enp = (string)x.Element("PACIENT").Element("ENP"),
            //            Smo = (string)x.Element("PACIENT").Element("SMO"),
            //            Lpu = (string)x.Element("SLUCH").Element("LPU"),
            //        });

            //    //Берем данные из второго XML
            //    var data2 = xdoc2.Root.Elements("PERS")
            //        .Select(x => new
            //        {
            //            PacientId = (string)x.Element("ID_PAC"),
            //            LastName = (string)x.Element("FAM"),
            //            FirstName = (string)x.Element("IM"),
            //            MiddleName = (string)x.Element("OT"),
            //            Birthdate = (string)x.Element("DR"),
            //            Sex = (string)x.Element("W"),
            //            Snils = (string)x.Element("SNILS"),
            //        });

            //    //Сохраняем данные из первых двух XML в третий
            //    var data3 = data2.Join(data1, outer => outer.PacientId, inner => inner.PacientId,
            //        (outer, inner) => new { Data1 = inner, Data2 = outer });
            //    XDocument doc = new XDocument(
            //        new XDeclaration("1.0", "utf-8", "yes"),
            //        new XElement("Data",
            //            data3.Select(x => new XElement("Item",
            //                new XElement("ID_PAC", x.Data2.PacientId),
            //                new XElement("FAM", x.Data2.LastName),
            //                new XElement("IM", x.Data2.FirstName),
            //                new XElement("OT", x.Data2.MiddleName),
            //                new XElement("DR", x.Data2.Birthdate),
            //                new XElement("W", x.Data2.Sex),
            //                new XElement("ENP", x.Data1.Enp),
            //                new XElement("SMO", x.Data1.Smo),
            //                new XElement("LPU", x.Data1.Lpu),
            //                new XElement("DISP", x.Data1.Disp),
            //                new XElement("QUARTER", x.Data1.Quarter),
            //                new XElement("SNILS", x.Data2.Snils)
            //            ))));
            //    doc.Save(extr_path2 + "W" + fname);

            //    try
            //    {
            //        XDocument XDoc = XDocument.Load(extr_path2 + "W" + fname);
            //    }
            //    catch
            //    {
            //        MessageBox.Show("Не загружен");
            //    }
            //}
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            processFiles();
        }
    }

    class ProductComparer : EqualityComparer<XElement>
    {
        // XElements are equal if their names and product numbers are equal.
        public override bool Equals(XElement x, XElement y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the xelements' properties are equal.
            return x.Value == y.Value;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode(XElement element)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(element, null)) return 0;

            //Calculate the hash code for the product.
            return element.Value.GetHashCode();
        }
    }
}
