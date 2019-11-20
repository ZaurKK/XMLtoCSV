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

        private static string[] dFileMasks =
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

        private static StreamWriter[] swResultAll = new StreamWriter[2];

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

                swResultAll[i] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultAll[i].WriteLine(fileHeader);
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
                swResultAll[i].Close();

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
            string archiveFile = Directory.GetFiles(outputDirectory, dArchiveMasks[(int)xmlFileType]).FirstOrDefault();
            if (string.IsNullOrEmpty(archiveFile))
                return;
            string outputFolder = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(archiveFile));

            ZipFile.ExtractToDirectory(archiveFile, outputFolder);

            string[] dFiles = Directory.GetFiles(outputFolder, dFileMasks[(int)xmlFileType]);            //переменная по свойствам диспансеризации
            string[] lFiles = Directory.GetFiles(outputFolder, lFileMask);                                  //переменная по данным пациентов

            String dFileFirst = dFiles.First();
            String lFileFirst = dFiles.First();
            if (String.IsNullOrEmpty(dFileFirst) || String.IsNullOrEmpty(lFileFirst))
                return;

            String dplFileName = Path.GetFileName(dFileFirst);
            String lpuMO = Path.GetFileNameWithoutExtension(outputDirectory);

            int lpuId = 0;
            if (!int.TryParse(lpuMO.Substring(0, 6), out lpuId))
                return;

            //объединяем 2 XML в 1
            XDocument dXDoc = XDocument.Load(dFiles.First());
            XDocument lXDoc = XDocument.Load(lFiles.First());
            //Берем данные из первого XML               
            var dData = dXDoc.Root.Elements("ZAP")
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
            var lData = lXDoc.Root.Elements("PERS")
                .Select(x => new
                {
                    PacientId = (string)x.Element("ID_PAC"),
                    LastName = (string)x.Element("FAM"),
                    FirstName = (string)x.Element("IM"),
                    MiddleName = (string)x.Element("OT"),
                    Birthdate = (string)x.Element("DR"),
                    Sex = (int)x.Element("W")
                })
                .ToList()
                .Distinct();

            //Сохраняем данные из первых двух XML в третий
            var resultData = dData
                .Join(lData, outerL => outerL.PacientId, innerD => innerD.PacientId, (innerD, outerL) => new { outerL, innerD });
            XDocument resultDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Data", resultData.Select(res => new XElement("Item",
                            new XElement("FAM", res.outerL.LastName),
                            new XElement("IM", res.outerL.FirstName),
                            new XElement("OT", res.outerL.MiddleName),
                            new XElement("DR", res.outerL.Birthdate),
                            new XElement("W", res.outerL.Sex),
                            new XElement("ID_PAC", res.innerD.PacientId),
                            new XElement("DISP", res.innerD.Disp),
                            new XElement("QUARTER", res.innerD.Quarter),
                            new XElement("MONTH", res.innerD.Month),
                            new XElement("DS", res.innerD.DS),
                            new XElement("DATE_START", res.innerD.DateStart),
                            new XElement("DATE_END", res.innerD.DateEnd)
                        )
                    )
                )
            );
            String resultXmlFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, ".xml"));
            resultDoc.Save(resultXmlFilePath);

            var lDataHash = new HashSet<string>(lData.Select(l => l.PacientId));
            var noResultData = dData
                .Where(d => !lDataHash.Contains(d.PacientId))
                .ToList();
            XDocument noResultDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Data", noResultData.Select(res => new XElement("Item",
                            new XElement("ID_PAC", res.PacientId),
                            new XElement("DISP", res.Disp),
                            new XElement("QUARTER", res.Quarter),
                            new XElement("MONTH", res.Month),
                            new XElement("DS", res.DS),
                            new XElement("DATE_START", res.DateStart),
                            new XElement("DATE_END", res.DateEnd)
                        )
                    )
                )
            );
            String noResultXmlFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, "_missed.xml"));
            noResultDoc.Save(noResultXmlFilePath);

            //конвертируем объединенный XML в CSV
            string delimiter = ";";                                                                 //разделитель
            StringBuilder sbResult = new StringBuilder();
            var elements = XDocument
                .Load(resultXmlFilePath)
                .Descendants("Item");
            elements
                .Where(element => (2019 - DateTime.Parse(element.Element("DR").Value).Year) >= 18)
                .ToList()
                .ForEach(element => sbResult.Append(
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
            String[] inputArray = sbResult
                .ToString()
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder results = new StringBuilder();
            foreach (String s in inputArray.Distinct().ToArray())
            {
                results.Append(s);
                results.Append("\r\n");
            }

            string resultCsvFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, ".csv"));
            StreamWriter swResult = new StreamWriter(resultCsvFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swResult.WriteLine(fileHeader);
            swResult.Write(results.ToString());
            swResult.Close();

            swResultAll[(int)xmlFileType].Write(results.ToString());


            StringBuilder sbResultChildren = new StringBuilder();
            elements
                .Where(element => (2019 - DateTime.Parse(element.Element("DR").Value).Year) < 18)
                .ToList()
                .ForEach(element => sbResultChildren.Append(
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
            String[] inputArrayChildren = sbResultChildren
                .ToString()
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            results.Clear();
            foreach (String s in inputArrayChildren)
            {
                results.Append(s);
                results.Append("\r\n");
            }
            string resultCsvChildrenDetFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, "_children.csv"));
            StreamWriter swResultChildren = new StreamWriter(resultCsvChildrenDetFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swResultChildren.WriteLine(fileHeader);
            swResultChildren.Write(results.ToString());
            swResultChildren.Close();


            var inputArrayDuplicates = inputArray
                .GroupBy(p => p)
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    Str = g.Key,
                    Count = g.Count()
                })
                .ToArray();

            results.Clear();
            foreach (var s in inputArrayDuplicates)
            {
                results.Append(s.Str + delimiter + s.Count);
                results.Append("\r\n");
            }
            string resultCsvDuplicatesFilePath = Path.Combine(outputFolder, String.Concat(lpuMO, "_duplicates.csv"));
            StreamWriter swDuplicates = new StreamWriter(resultCsvDuplicatesFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swDuplicates.WriteLine(string.Format("{0};Дубликаты", fileHeader));
            swDuplicates.Write(results.ToString());
            swDuplicates.Close();

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
