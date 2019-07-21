using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;


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

        private enum XmlFileType
        {
            D = 0,
            DN = 1
        };

        private static string rootFileMask = "07*_2019.zip";

        private static string[] dArchiveMasks =
        {
            "DPL*.zip",
            "DNPL*.zip"
        };

        private static string[] dplFileMasks =
        {
            "DPLM*.XML",
            "DNPLM*.XML"
        };

        private string lFileMask = "LM*.XML";

        private static string[] csvFileNamesTemp =
        {
            "M_All_D.csv",
            "M_All_DN.csv"
        };

        private static string[] csvFileNames =
        {
            String.Format("{0}_{1}.csv", "M_All_D", DateTime.Now.ToString("yyyyMMdd_HHmm")),
            String.Format("{0}_{1}.csv", "M_All_DN", DateTime.Now.ToString("yyyyMMdd_HHmm"))
        };

        private static string fileHeader = String.Format("Фамилия;Имя;Отчество;Дата рождения;Пол;Квартал;Месяц;Тип диспансеризации;Диагноз;Дата начала;Дата конца;ЛПУ;Комментарий");

        private static StreamWriter[] swAll = new StreamWriter[2];

        public Form1(FolderBrowserDialog folderBrowserDialog)
        {
            this.folderBrowserDialog = folderBrowserDialog;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                ProcessFiles();
        }

        private void ProcessFiles()
        {
            if (String.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
                folderBrowserDialog.SelectedPath = textBox1.Text;

            string extr_path = textBox2.Text;

            if (!Directory.Exists(extr_path))
                Directory.CreateDirectory(extr_path);

            for (int i = 0; i < Enum.GetNames(typeof(XmlFileType)).Length; i++)
            {
                String outputFilePath = Path.Combine(extr_path, csvFileNames[i]);

                swAll[i] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swAll[i].WriteLine(fileHeader);
            }

            var rootArchives = Directory.GetFiles(folderBrowserDialog.SelectedPath, rootFileMask);
            foreach (var rootArchive in rootArchives)
            {
                string outputDirectory = Path.Combine(extr_path, Path.GetFileNameWithoutExtension(rootArchive));
                if (Directory.Exists(outputDirectory))
                    Directory.Delete(outputDirectory, true);
                ZipFile.ExtractToDirectory(rootArchive, outputDirectory);

                ProcessXmlFiles(outputDirectory, XmlFileType.D);
                ProcessXmlFiles(outputDirectory, XmlFileType.DN);
            }

            for (int i = 0; i < Enum.GetNames(typeof(XmlFileType)).Length; i++)
            {
                swAll[i].Close();

                String outputFilePath = Path.Combine(extr_path, csvFileNames[i]);
                String outputFilePathTemp = Path.Combine(extr_path, csvFileNamesTemp[i]);
                System.IO.File.Copy(outputFilePath, outputFilePathTemp, true);
            }

            DialogResult dialogResult = MessageBox.Show("Файлы сформированы!\r\n\r\nВыйти из программы?", "Операция завершена", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
                Application.Exit();
        }

        private void ProcessXmlFiles(string outputDirectory, XmlFileType xmlFileType)
        {
            string archiveFile = Directory.GetFiles(outputDirectory, dArchiveMasks[(int)xmlFileType]).First();
            string outputFolder = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(archiveFile));

            ZipFile.ExtractToDirectory(archiveFile, outputFolder);

            string[] dplFiles = Directory.GetFiles(outputFolder, dplFileMasks[(int)xmlFileType]);            //переменная по свойствам диспансеризации
            string[] lFiles = Directory.GetFiles(outputFolder, lFileMask);                                  //переменная по данным пациентов

            String dplFileFirst = dplFiles.First();
            String lFileFirst = dplFiles.First();
            if (String.IsNullOrEmpty(dplFileFirst) || String.IsNullOrEmpty(lFileFirst))
                return;

            String dplFileName = Path.GetFileName(dplFileFirst);
            String lpuMO = Path.GetFileNameWithoutExtension(outputDirectory);

            int lpuId = 0;
            if (!int.TryParse(lpuMO.Substring(0, 6), out lpuId))
                return;

            //объединяем 2 XML в 1
            XDocument xdoc1 = XDocument.Load(dplFiles.First());
            XDocument xdoc2 = XDocument.Load(lFiles.First());
            //Берем данные из первого XML               
            var data1 = xdoc1.Root.Elements("ZAP")
                .Select(x => new
                {
                    Disp = (string)x.Element("DISP"),
                    PacientId = (string)x.Element("PACIENT").Element("ID_PAC"),
                    Quarter = (string)x.Element("SLUCH").Element("QUARTER"),
                    Month = (string)x.Element("SLUCH").Element("MONTH"),
                    DS = (string)x.Element("SLUCH").Element("DS"),
                    DateStart = (string)x.Element("SLUCH").Element("DATE_1"),
                    DateEnd = (string)x.Element("SLUCH").Element("DATE_2")
                });

            //Берем данные из второго XML
            var data2 = xdoc2.Root.Elements("PERS")
                .Select(x => new
                {
                    PacientId = (string)x.Element("ID_PAC"),
                    LastName = (string)x.Element("FAM"),
                    FirstName = (string)x.Element("IM"),
                    MiddleName = (string)x.Element("OT"),
                    Birthdate = (string)x.Element("DR"),
                    Sex = (int)x.Element("W")
                });

            //Сохраняем данные из первых двух XML в третий
            var data3 = data2.Join(data1, outer => outer.PacientId, inner => inner.PacientId, (outer, inner) => new { Data1 = inner, Data2 = outer });
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Data", data3.Select(x => new XElement("Item",
                            new XElement("FAM", x.Data2.LastName),
                            new XElement("IM", x.Data2.FirstName),
                            new XElement("OT", x.Data2.MiddleName),
                            new XElement("DR", x.Data2.Birthdate),
                            new XElement("W", x.Data2.Sex),
                            new XElement("ID_PAC", x.Data2.PacientId),
                            new XElement("DISP", x.Data1.Disp),
                            new XElement("QUARTER", x.Data1.Quarter),
                            new XElement("MONTH", x.Data1.Month),
                            new XElement("DS", x.Data1.DS),
                            new XElement("DATE_START", x.Data1.DateStart),
                            new XElement("DATE_END", x.Data1.DateEnd)
                        )
                    )
                )
            );
            String resultXmlFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, ".xml"));
            doc.Save(resultXmlFilePath);

            //конвертируем объединенный XML в CSV
            StringBuilder sb = new StringBuilder();
            string delimiter = ";";                                                                 //разделитель
            string resultCsvFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, ".csv"));
            XDocument.Load(resultXmlFilePath).Descendants("Item").ToList().ForEach(element => sb.Append(
                element.Element("FAM").Value + delimiter +
                element.Element("IM").Value + delimiter +
                element.Element("OT").Value + delimiter +
                element.Element("DR").Value + delimiter +
                element.Element("W").Value + delimiter +
                element.Element("QUARTER").Value + delimiter +
                element.Element("MONTH").Value + delimiter +
                element.Element("DISP").Value + delimiter +
                element.Element("DS").Value + delimiter +
                element.Element("DATE_START").Value + delimiter +
                element.Element("DATE_END").Value + delimiter +
                LPUs[lpuId] + delimiter +
                lpuId + "\r\n")
            );

            StreamWriter sw = new StreamWriter(resultCsvFilePath, false, Encoding.GetEncoding("Windows-1251"));
            sw.WriteLine(fileHeader);
            String input = sb.ToString();
            String[] inputArray = input.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String[] distinctInputArray = inputArray.Distinct().ToArray();
            StringBuilder result = new StringBuilder();
            foreach (String s in distinctInputArray)
            {
                result.Append(s);
                result.Append("\r\n");
            }
            sw.Write(result.ToString());
            sw.Close();

            swAll[(int)xmlFileType].Write(result.ToString());

            ////Удаляем xml-файлы в папке
            //string[] fileNames = Directory.GetFiles(outputFolder, "*.xml", SearchOption.AllDirectories);
            //string fileErrors = string.Empty;
            //for (int j = 0; j != fileNames.Length; j++)
            //{
            //    try
            //    {
            //        File.Delete(fileNames[j]);
            //    }
            //    catch
            //    {
            //        fileErrors = fileErrors + "\n" + fileNames[j];
            //    }
            //}
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            ProcessFiles();
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
