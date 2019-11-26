using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMLtoCSVconvertor
{
    class XmlFilesProcessor
    {
        private static string[] hashStrings =
        {
            "2019-10-09 15:13:03,DPLM070201S0700401.ZIP,698dbf95c40dc385cfe4ceb6646b1f98,md5_file (php)",
            "2019-10-09 15:13:12,DNPLM070201S0700401.ZIP,668b902464389adc26727edeb92454d5,md5_file (php)",
            "2019-10-03 11:58:49,DPLM070807S0700401.ZIP,cc37a830a447e70fa915803586e03d89,md5_file (php)",
            "2019-10-03 11:59:00,DNPLM070807S0700401.ZIP,7d01b230e182212a3fb7734836af08ab,md5_file (php)",
            "2019-10-16 01:25:42,DPLM070906S0700401.ZIP,65a49a8fa86c507d46c65f1208373157,md5_file (php)",
            "2019-10-16 01:25:54,DNPLM070906S0700401.ZIP,8454882ec01af157e3bd4250d80e6924,md5_file (php)",
            "2019-10-03 14:48:11,DPLM071001S0700401.ZIP,0b9138aa73839e07255e3433e2b78b65,md5_file (php)",
            "2019-10-03 14:48:15,DNPLM071001S0700401.ZIP,fb1d93a48f6fc23e5c30fbe1680202b0,md5_file (php)",
            "2019-11-07 15:07:36,DPLM070571S0700401.ZIP,32b1e920f17dd68e143d04fe7d239126,md5_file (php)",
            "2019-11-07 15:07:50,DNPLM070571S0700401.ZIP,8839f69fe3df918762fdbeb4dbc6dccc,md5_file (php)",
            "2019-10-09 09:47:34,DPLM070510S0700401.ZIP,5eb145ef6da4987091a035d61ed2c2cc,md5_file (php)",
            "2019-10-09 09:48:12,DNPLM070510S0700401.ZIP,4f19bd34111d3d72b1dc1a465c9b58c4,md5_file (php)"
        };

        private static int YEAR = 2019;
        private static int MIN_AGE = 18;

        private static string delimiter = ";";

        private static string rootFileMask = string.Format("07*_{0}.zip", YEAR);

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

        public enum XmlFileType
        {
            D = 0,
            DN = 1
        };

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

        private static string[] csvFileNamesFullTemp =
        {
            "M_D_Full.csv",
            "M_DN_Full.csv"
        };

        private static string[] csvFileNamesFull =
        {
            string.Format("{0}_{1}.csv", "M_D_Full", DateTime.Now.ToString("yyyyMMdd_HHmm")),
            string.Format("{0}_{1}.csv", "M_DN_Full", DateTime.Now.ToString("yyyyMMdd_HHmm"))
        };

        private static string[] csvFileNamesAllTemp =
        {
            "M_D_All.csv",
            "M_DN_All.csv"
        };

        private static string[] csvFileNamesAll =
        {
            string.Format("{0}_{1}.csv", "M_D_All", DateTime.Now.ToString("yyyyMMdd_HHmm")),
            string.Format("{0}_{1}.csv", "M_DN_All", DateTime.Now.ToString("yyyyMMdd_HHmm"))
        };

        private static string[] csvFileNamesChildrenTemp =
        {
            "M_D_Children.csv",
            "M_DN_Children.csv"
        };

        private static string[] csvFileNamesChildren =
        {
            string.Format("{0}_{1}.csv", "M_D_Children", DateTime.Now.ToString("yyyyMMdd_HHmm")),
            string.Format("{0}_{1}.csv", "M_DN_Children", DateTime.Now.ToString("yyyyMMdd_HHmm"))
        };

        private static string[] csvFileNamesDuplicatesTemp =
        {
            "M_D_Duplicates.csv",
            "M_DN_Duplicates.csv"
        };

        private static string[] csvFileNamesDuplicates =
        {
            string.Format("{0}_{1}.csv", "M_D_Duplicates", DateTime.Now.ToString("yyyyMMdd_HHmm")),
            string.Format("{0}_{1}.csv", "M_DN_Duplicates", DateTime.Now.ToString("yyyyMMdd_HHmm"))
        };

        private static string fileHeader = string.Format("Фамилия;Имя;Отчество;Дата рождения;Пол;Квартал;Месяц;Тип диспансеризации;Диагноз;Дата начала;Дата конца;ЛПУ;Комментарий");
        private static string fileHeaderDuplicates = string.Format("{0};Дубликаты", fileHeader);

        private static StreamWriter[] swResultFull = new StreamWriter[2];
        private static StreamWriter[] swResultAll = new StreamWriter[2];
        private static StreamWriter[] swResultChildrenAll = new StreamWriter[2];
        private static StreamWriter[] swResultDuplicatesAll = new StreamWriter[2];

        public bool ProcessFiles(string inputPath, string outputPath)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            for (int i = 0; i < Enum.GetNames(typeof(XmlFileType)).Length; i++)
            {
                string outputFilePath = Path.Combine(outputPath, csvFileNamesFull[i]);
                swResultFull[i] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultFull[i].WriteLine(fileHeader);

                outputFilePath = Path.Combine(outputPath, csvFileNamesAll[i]);
                swResultAll[i] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultAll[i].WriteLine(fileHeader);

                outputFilePath = Path.Combine(outputPath, csvFileNamesChildren[i]);
                swResultChildrenAll[i] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultChildrenAll[i].WriteLine(fileHeader);

                outputFilePath = Path.Combine(outputPath, csvFileNamesDuplicates[i]);
                swResultDuplicatesAll[i] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultDuplicatesAll[i].WriteLine(fileHeaderDuplicates);
            }

            //var rootArchives = Directory.GetFiles(folderBrowserDialog.SelectedPath, rootFileMask);
            var rootArchives = Directory.GetFiles(inputPath, rootFileMask);
            foreach (var rootArchive in rootArchives)
            {
                string outputDirectory = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(rootArchive));
                if (Directory.Exists(outputDirectory))
                    Directory.Delete(outputDirectory, true);
                ZipFile.ExtractToDirectory(rootArchive, outputDirectory);

                if (!ProcessXmlFiles(outputDirectory, XmlFileType.D))
                    return false;
                if (!ProcessXmlFiles(outputDirectory, XmlFileType.DN))
                    return false;
            }

            for (int i = 0; i < Enum.GetNames(typeof(XmlFileType)).Length; i++)
            {
                swResultFull[i].Close();
                string outputFilePath = Path.Combine(outputPath, csvFileNamesFull[i]);
                string outputFilePathTemp = Path.Combine(outputPath, csvFileNamesFullTemp[i]);
                System.IO.File.Copy(outputFilePath, outputFilePathTemp, true);

                swResultAll[i].Close();
                outputFilePath = Path.Combine(outputPath, csvFileNamesAll[i]);
                outputFilePathTemp = Path.Combine(outputPath, csvFileNamesAllTemp[i]);
                System.IO.File.Copy(outputFilePath, outputFilePathTemp, true);

                swResultChildrenAll[i].Close();
                outputFilePath = Path.Combine(outputPath, csvFileNamesChildren[i]);
                outputFilePathTemp = Path.Combine(outputPath, csvFileNamesChildrenTemp[i]);
                System.IO.File.Copy(outputFilePath, outputFilePathTemp, true);

                swResultDuplicatesAll[i].Close();
                outputFilePath = Path.Combine(outputPath, csvFileNamesDuplicates[i]);
                outputFilePathTemp = Path.Combine(outputPath, csvFileNamesDuplicatesTemp[i]);
                System.IO.File.Copy(outputFilePath, outputFilePathTemp, true);
            }

            return true;
        }

        public bool ProcessXmlFiles(string outputDirectory, XmlFileType xmlFileType)
        {
            string archiveFile = Directory.GetFiles(outputDirectory, dArchiveMasks[(int)xmlFileType]).FirstOrDefault();
            if (string.IsNullOrEmpty(archiveFile))
                return false;
            string outputFolder = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(archiveFile));

            ZipFile.ExtractToDirectory(archiveFile, outputFolder);

            string[] dFiles = Directory.GetFiles(outputFolder, dFileMasks[(int)xmlFileType]);            //переменная по свойствам диспансеризации
            string[] lFiles = Directory.GetFiles(outputFolder, lFileMask);                               //переменная по данным пациентов

            string dFileFirst = dFiles.First();
            string lFileFirst = dFiles.First();
            if (string.IsNullOrEmpty(dFileFirst) || string.IsNullOrEmpty(lFileFirst))
                return false;

            string dplFileName = Path.GetFileName(dFileFirst);
            string lpuMO = Path.GetFileNameWithoutExtension(outputDirectory);

            int lpuId = 0;
            if (!int.TryParse(lpuMO.Substring(0, 6), out lpuId))
                return false;

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
            string resultXmlFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, ".xml"));
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
            string noResultXmlFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, "_missed.xml"));
            noResultDoc.Save(noResultXmlFilePath);

            //конвертируем объединенный XML в CSV
            var elements = XDocument
                .Load(resultXmlFilePath)
                .Descendants("Item");

            StringBuilder sbResultFull = new StringBuilder();
            elements
                .ToList()
                .ForEach(element => sbResultFull.Append(
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
            string[] inputArrayFull = sbResultFull
                .ToString()
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder resultFull = new StringBuilder();
            foreach (string s in inputArrayFull.ToArray())
            {
                resultFull.Append(s);
                resultFull.Append("\r\n");
            }
            swResultFull[(int)xmlFileType].Write(resultFull.ToString());


            StringBuilder sbResultAll = new StringBuilder();
            elements
                .Where(element => (YEAR - DateTime.Parse(element.Element("DR").Value).Year) >= MIN_AGE)
                .ToList()
                .ForEach(element => sbResultAll.Append(
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
            string[] inputArrayAll = sbResultAll
                .ToString()
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder results = new StringBuilder();
            foreach (string s in inputArrayAll.Distinct().ToArray())
            {
                results.Append(s);
                results.Append("\r\n");
            }
            string resultCsvFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, ".csv"));
            StreamWriter swResult = new StreamWriter(resultCsvFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swResult.WriteLine(fileHeader);
            swResult.Write(results.ToString());
            swResult.Close();

            swResultAll[(int)xmlFileType].Write(results.ToString());


            StringBuilder sbResultChildren = new StringBuilder();
            elements
                .Where(element => (YEAR - DateTime.Parse(element.Element("DR").Value).Year) < MIN_AGE)
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
            string[] inputArrayChildren = sbResultChildren
                .ToString()
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            results.Clear();
            foreach (string s in inputArrayChildren)
            {
                results.Append(s);
                results.Append("\r\n");
            }
            string resultCsvChildrenDetFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, "_children.csv"));
            StreamWriter swResultChildren = new StreamWriter(resultCsvChildrenDetFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swResultChildren.WriteLine(fileHeader);
            swResultChildren.Write(results.ToString());
            swResultChildren.Close();

            swResultChildrenAll[(int)xmlFileType].Write(results.ToString());


            var inputArrayDuplicates = inputArrayAll
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
            string resultCsvDuplicatesFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, "_duplicates.csv"));
            StreamWriter swDuplicates = new StreamWriter(resultCsvDuplicatesFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swDuplicates.WriteLine(fileHeaderDuplicates);
            swDuplicates.Write(results.ToString());
            swDuplicates.Close();

            swResultDuplicatesAll[(int)xmlFileType].Write(results.ToString());

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

            return true;
        }
    }
}
