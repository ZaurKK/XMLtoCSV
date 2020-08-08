using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using OfficeOpenXml;

namespace XMLtoCSVconvertor
{
    class XmlFilesProcessor
    {
        private static int YEAR = 2020;
        private static int MIN_AGE = 18;

        private static readonly string delimiter = ";";

        private static readonly string rootFileMask = string.Format("07*_{0}.zip", YEAR);

        private static readonly Dictionary<int, string> LPUs = new Dictionary<int, string>()
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

        private static readonly string lFileMask = "LM*.XML";

        private static readonly string resultFolder = DateTime.Now.ToString("yyyyMMdd_HHmm");

        public enum XmlFileType
        {
            D = 0,
            DN = 1
        };

        private enum Extension
        {
            CSV,
            XLSX
        };

        private enum DispType
        {
            DV4,
            OPV,
            DN1,
            DN2
        };

        private static readonly Dictionary<Extension, string> ExtensionString = new Dictionary<Extension, string>
        {
            { Extension.CSV, ".csv" },
            { Extension.XLSX, ".xlsx" }
        };

        private static readonly Dictionary<DispType, string> DispTypeString =  new Dictionary<DispType, string>
        {
            { DispType.DV4, "ДВ4" },
            { DispType.OPV, "ОПВ" },
            { DispType.DN1, "ДН1" },
            { DispType.DN2, "ДН2" }
        };

        private static readonly Dictionary<XmlFileType, string> dArchiveMasks = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "DPL*.zip" },
            { XmlFileType.DN, "DNPL*.zip" }
        };

        private static readonly Dictionary<XmlFileType, string> dFileMasks = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "DPLM*.XML" },
            { XmlFileType.DN, "DNPLM*.XML" }
        };

        private static readonly Dictionary<XmlFileType, string> csvFileNamesFull = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "M_D_Full.csv" },
            { XmlFileType.DN, "M_DN_Full.csv" }
        };

        private static readonly Dictionary<XmlFileType, string> csvFileNamesAll = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "M_D_All.csv" },
            { XmlFileType.DN, "M_DN_All.csv" }
        };

        private static readonly Dictionary<XmlFileType, string> csvFileNamesChildren = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "M_D_Children.csv" },
            { XmlFileType.DN, "M_DN_Children.csv" }
        };

        private static readonly Dictionary<XmlFileType, string> csvFileNamesDuplicates = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "M_D_Duplicates.csv" },
            { XmlFileType.DN, "M_DN_Duplicates.csv" }
        };

        private static readonly string fileHeader = string.Format("Фамилия;Имя;Отчество;Дата рождения;Пол;Квартал;Месяц;Тип диспансеризации;Диагноз;Дата начала;Дата конца;ЛПУ;Комментарий");
        private static readonly string fileHeaderDuplicates = string.Format("{0};Дубликаты", fileHeader);

        private static readonly Dictionary<XmlFileType, StreamWriter> swResultFull = new Dictionary<XmlFileType, StreamWriter>(Enum.GetNames(typeof(XmlFileType)).Length);
        private static readonly Dictionary<XmlFileType, StreamWriter> swResultAll = new Dictionary<XmlFileType, StreamWriter>(Enum.GetNames(typeof(XmlFileType)).Length);
        private static readonly Dictionary<XmlFileType, StreamWriter> swResultChildrenAll = new Dictionary<XmlFileType, StreamWriter>(Enum.GetNames(typeof(XmlFileType)).Length);
        private static readonly Dictionary<XmlFileType, StreamWriter> swResultDuplicatesAll = new Dictionary<XmlFileType, StreamWriter>(Enum.GetNames(typeof(XmlFileType)).Length);

        HashList HashList { get; set; } = new HashList();

        public bool ProcessFiles(string inputPath, string outputPath)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            string outputCsvPath = Path.Combine(outputPath, resultFolder);
            Directory.CreateDirectory(outputCsvPath);

            foreach(XmlFileType xmlFileType in Enum.GetValues(typeof(XmlFileType)))
            {
                string outputFilePath = Path.Combine(outputCsvPath, csvFileNamesFull[xmlFileType]);
                swResultFull[xmlFileType] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultFull[xmlFileType].WriteLine(fileHeader);

                outputFilePath = Path.Combine(outputCsvPath, csvFileNamesAll[xmlFileType]);
                swResultAll[xmlFileType] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultAll[xmlFileType].WriteLine(fileHeader);

                outputFilePath = Path.Combine(outputCsvPath, csvFileNamesChildren[xmlFileType]);
                swResultChildrenAll[xmlFileType] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultChildrenAll[xmlFileType].WriteLine(fileHeader);

                outputFilePath = Path.Combine(outputCsvPath, csvFileNamesDuplicates[xmlFileType]);
                swResultDuplicatesAll[xmlFileType] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultDuplicatesAll[xmlFileType].WriteLine(fileHeaderDuplicates);
            }

            var rootArchives = Directory.GetFiles(inputPath, rootFileMask);
            foreach (var rootArchive in rootArchives)
            {
                string outputDirectory = Path.Combine(outputCsvPath, Path.GetFileNameWithoutExtension(rootArchive));
                if (Directory.Exists(outputDirectory))
                    Directory.Delete(outputDirectory, true);
                ZipFile.ExtractToDirectory(rootArchive, outputDirectory);

                if (!ProcessXmlFiles(outputDirectory, XmlFileType.D))
                    continue;
                if (!ProcessXmlFiles(outputDirectory, XmlFileType.DN))
                    continue;
            }

            foreach (XmlFileType xmlFileType in Enum.GetValues(typeof(XmlFileType)))
            {
                swResultFull[xmlFileType].Close();
                string outputFilePath = Path.Combine(outputCsvPath, csvFileNamesFull[xmlFileType]);
                string outputFilePathTemp = Path.Combine(outputPath, csvFileNamesFull[xmlFileType]);
                File.Copy(outputFilePath, outputFilePathTemp, true);

                swResultAll[xmlFileType].Close();
                outputFilePath = Path.Combine(outputCsvPath, csvFileNamesAll[xmlFileType]);
                outputFilePathTemp = Path.Combine(outputPath, csvFileNamesAll[xmlFileType]);
                File.Copy(outputFilePath, outputFilePathTemp, true);

                swResultChildrenAll[xmlFileType].Close();
                outputFilePath = Path.Combine(outputCsvPath, csvFileNamesChildren[xmlFileType]);
                outputFilePathTemp = Path.Combine(outputPath, csvFileNamesChildren[xmlFileType]);
                File.Copy(outputFilePath, outputFilePathTemp, true);

                swResultDuplicatesAll[xmlFileType].Close();
                outputFilePath = Path.Combine(outputCsvPath, csvFileNamesDuplicates[xmlFileType]);
                outputFilePathTemp = Path.Combine(outputPath, csvFileNamesDuplicates[xmlFileType]);
                File.Copy(outputFilePath, outputFilePathTemp, true);
            }

            return true;
        }

        public bool ProcessXmlFiles(string outputDirectory, XmlFileType xmlFileType)
        {
            string archiveFile = Directory.GetFiles(outputDirectory, dArchiveMasks[xmlFileType]).FirstOrDefault();
            if (!HashList.IsFileValid(archiveFile))
                return false;
            string outputFolder = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(archiveFile));

            ZipFile.ExtractToDirectory(archiveFile, outputFolder);

            string[] dFiles = Directory.GetFiles(outputFolder, dFileMasks[xmlFileType]);            //переменная по свойствам диспансеризации
            string[] lFiles = Directory.GetFiles(outputFolder, lFileMask);                          //переменная по данным пациентов

            string dFileFirst = dFiles.First();
            string lFileFirst = lFiles.First();
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
                    lpuId + Environment.NewLine)
                );
            string[] inputArrayFull = sbResultFull
                .ToString()
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder resultFull = new StringBuilder();
            foreach (string s in inputArrayFull.ToArray())
            {
                resultFull.Append(s);
                resultFull.Append(Environment.NewLine);
            }
            swResultFull[xmlFileType].Write(resultFull.ToString());


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
                    lpuId + Environment.NewLine)
                );
            string[] inputArrayAll = sbResultAll
                .ToString()
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder resultsAll = new StringBuilder();
            foreach (string s in inputArrayAll.Distinct().ToArray())
            {
                resultsAll.Append(s);
                resultsAll.Append(Environment.NewLine);
            }
            string resultCsvFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, ".csv"));
            StreamWriter swResult = new StreamWriter(resultCsvFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swResult.WriteLine(fileHeader);
            swResult.Write(resultsAll.ToString());
            swResult.Close();

            swResultAll[xmlFileType].Write(resultsAll.ToString());


            StringBuilder sbResultChildren = new StringBuilder();
            elements
                .Where(element => element.Element("DISP").Value.Equals(DispTypeString[DispType.DN1]) && (YEAR - DateTime.Parse(element.Element("DR").Value).Year) < MIN_AGE)
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
                    lpuId + Environment.NewLine)
                );
            string[] inputArrayChildren = sbResultChildren
                .ToString()
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder resultsChildrenAll = new StringBuilder();
            foreach (string s in inputArrayChildren)
            {
                resultsChildrenAll.Append(s);
                resultsChildrenAll.Append(Environment.NewLine);
            }
            string resultCsvChildrenDetFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, "_children.csv"));
            StreamWriter swResultChildren = new StreamWriter(resultCsvChildrenDetFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swResultChildren.WriteLine(fileHeader);
            swResultChildren.Write(resultsChildrenAll.ToString());
            swResultChildren.Close();

            swResultChildrenAll[xmlFileType].Write(resultsChildrenAll.ToString());


            var inputArrayDuplicates = inputArrayAll
                .GroupBy(p => p)
                .Where(g => g.Count() > 1)
                .Select(g => new
                {
                    Str = g.Key,
                    Count = g.Count()
                })
                .ToArray();

            StringBuilder resultsDuplicatesAll = new StringBuilder();
            foreach (var s in inputArrayDuplicates)
            {
                resultsDuplicatesAll.Append(s.Str + delimiter + s.Count);
                resultsDuplicatesAll.Append(Environment.NewLine);
            }
            string resultCsvDuplicatesFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, "_duplicates.csv"));
            StreamWriter swDuplicates = new StreamWriter(resultCsvDuplicatesFilePath, false, Encoding.GetEncoding("Windows-1251"));
            swDuplicates.WriteLine(fileHeaderDuplicates);
            swDuplicates.Write(resultsDuplicatesAll.ToString());
            swDuplicates.Close();

            swResultDuplicatesAll[xmlFileType].Write(resultsDuplicatesAll.ToString());

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
