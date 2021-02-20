using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using OfficeOpenXml;

namespace XMLtoCSVconvertor
{
    class XmlFilesProcessor
    {
        private static int YEAR = 2021;
        private static int MIN_AGE = 18;

        private static readonly string delimSemicolon = ";";
        private static readonly string delimComma = ",";

        private static readonly string rootFileMask = string.Format("07*_{0}.zip", YEAR);

        private static readonly Dictionary<string, string> LPUs = new Dictionary<string, string>()
        {
            { "070510", "ГП 1 г.Нальчик" },
            { "070570", "ГП 2 г.Нальчик" },
            { "070571", "ГП 3 г.Нальчик" },
            { "070103", "Баксанской ЦРБ" },
            { "070105", "РБ с.Заюково" },
            { "070201", "Зольской ЦРБ" },
            { "070405", "Майской ЦРБ" },
            { "070616", "Прохладненской ЦРБ" },
            { "070714", "Терской ЦРБ" },
            { "070807", "ММБ г.Нарткала" },
            { "070906", "Чегемской ЦРБ" },
            { "071001", "Черекской ЦРБ" },
            { "071103", "Эльбрусской ЦРБ" },
            { "071106", "УБ с.Эльбрус" }
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

        private static readonly Dictionary<XmlFileType, string> dPrefix = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "DPL" },
            { XmlFileType.DN, "DNPL" }
        };

        private static readonly Dictionary<XmlFileType, string> dbTable = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "InformData" },
            { XmlFileType.DN, "InformData" }
        };

        private static readonly Dictionary<XmlFileType, string> dArchiveMasks = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, $"{dPrefix[XmlFileType.D]}*.zip" },
            { XmlFileType.DN, $"{dPrefix[XmlFileType.DN]}*.zip" }
        };

        private static readonly Dictionary<XmlFileType, string> dFileMasks = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, $"{dPrefix[XmlFileType.D]}M*.XML" },
            { XmlFileType.DN, $"{dPrefix[XmlFileType.DN]}M*.XML" }
        };

        private static readonly Dictionary<XmlFileType, string> csvFileNamesComplete = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, "M_D_Complete.csv" },
            { XmlFileType.DN, "M_DN_Complete.csv" }
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

        private static readonly string insertInto = "INSERT INTO ";
        //private static readonly string insertValues = "([Фамилия], [Имя], [Отчество], [Дата рождения], [Пол], [Квартал], [Месяц], [Тип диспансеризации], [Диагноз], [Дата начала], [Дата конца], [ЛПУ], [Комментарий], [Год], [DateTimeOfAddition]) VALUES(";
        private static readonly string insertValues = "([Surname], [Name], [Patronymic], [Birthdate], [Sex], [Quarter], [Month], [DispType], [DS], [DateBegin], [DateEnd], [LPU], [Comment], [Year], [DateTimeOfAddition]) VALUES(";
        private static readonly Dictionary<XmlFileType, string> insertCommand = new Dictionary<XmlFileType, string>
        {
            { XmlFileType.D, $"{insertInto}{dbTable[XmlFileType.D]}{insertValues}" },
            { XmlFileType.DN, $"{insertInto}{dbTable[XmlFileType.DN]}{insertValues}" }
        };

        private static readonly string fileHeader = string.Format("Фамилия;Имя;Отчество;Дата рождения;Пол;Квартал;Месяц;Тип диспансеризации;Диагноз;Дата начала;Дата конца;ЛПУ;Комментарий");
        private static readonly string fileHeaderDuplicates = string.Format("{0};Дубликаты", fileHeader);

        private static readonly Dictionary<XmlFileType, StreamWriter> swResultComplete = new Dictionary<XmlFileType, StreamWriter>(Enum.GetNames(typeof(XmlFileType)).Length);
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
                string outputFilePath = Path.Combine(outputCsvPath, csvFileNamesComplete[xmlFileType]);
                swResultComplete[xmlFileType] = new StreamWriter(outputFilePath, true, Encoding.GetEncoding("Windows-1251"));
                swResultComplete[xmlFileType].WriteLine(fileHeader);

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

                ProcessXmlFiles(outputDirectory, XmlFileType.D);
                ProcessXmlFiles(outputDirectory, XmlFileType.DN);
            }

            foreach (XmlFileType xmlFileType in Enum.GetValues(typeof(XmlFileType)))
            {
                swResultComplete[xmlFileType].Close();
                string outputFilePath = Path.Combine(outputCsvPath, csvFileNamesComplete[xmlFileType]);
                string outputFilePathTemp = Path.Combine(outputPath, csvFileNamesComplete[xmlFileType]);
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
            try
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

                string lpuId = lpuMO.Substring(0, 6);

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

                StringBuilder sbResultComplete = new StringBuilder();
                elements
                    .Select(element => new
                    {
                        FAM = element.Element("FAM").Value,
                        IM = element.Element("IM").Value,
                        OT = element.Element("OT").Value,
                        DR = element.Element("DR").Value,
                        W = element.Element("W").Value,
                        QUARTER = element.Element("QUARTER").Value == "" ? "null" : element.Element("QUARTER").Value,
                        MONTH = element.Element("MONTH").Value,
                        DISP = element.Element("DISP").Value,
                        DS = element.Element("DS").Value,
                        DATE_START = element.Element("DATE_START").Value == "" ? "null" : element.Element("DATE_START").Value,
                        DATE_END = element.Element("DATE_END").Value == "" ? "null" : element.Element("DATE_END").Value,
                        LPU = lpuId,
                        COMMENT = LPUs[lpuId],
                        YEAR,
                        DATETIME_OF_ADDITION = $"'{Process.GetCurrentProcess().StartTime:s}')"
                    })
                    .ToList()
                    .ForEach(element => sbResultComplete.Append(
                        $"{element.FAM}{delimSemicolon}" +
                        $"{element.IM}{delimSemicolon}" +
                        $"{element.OT}{delimSemicolon}" +
                        $"{element.DR}{delimSemicolon}" +
                        $"{element.W}{delimSemicolon}" +
                        $"{element.QUARTER}{delimSemicolon}" +
                        $"{element.MONTH}{delimSemicolon}" +
                        $"{element.DISP}{delimSemicolon}" +
                        $"{element.DS}{delimSemicolon}" +
                        $"{element.DATE_START}{delimSemicolon}" +
                        $"{element.DATE_END}{delimSemicolon}" +
                        $"{element.LPU}{delimSemicolon}" +
                        $"{element.COMMENT}{delimSemicolon}" +
                        Environment.NewLine)
                    );
                string[] inputArrayComplete = sbResultComplete
                    .ToString()
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder resultComplete = new StringBuilder();
                foreach (string s in inputArrayComplete.ToArray())
                {
                    resultComplete.Append(s);
                    resultComplete.Append(Environment.NewLine);
                }
                swResultComplete[xmlFileType].Write(resultComplete.ToString());


                StringBuilder sbResultAll = new StringBuilder();
                elements
                    .Where(element => (YEAR - DateTime.Parse(element.Element("DR").Value).Year) >= MIN_AGE)
                    .Select(element => new
                    {
                        FAM = element.Element("FAM").Value,
                        IM = element.Element("IM").Value,
                        OT = element.Element("OT").Value,
                        DR = $"{element.Element("DR").Value:s}",
                        W = element.Element("W").Value,
                        QUARTER = element.Element("QUARTER").Value == "" ? "null" : element.Element("QUARTER").Value,
                        MONTH = element.Element("MONTH").Value,
                        DISP = element.Element("DISP").Value,
                        DS = element.Element("DS").Value,
                        DATE_START = element.Element("DATE_START").Value == "" ? "null" : $"'{element.Element("DATE_START").Value}'",
                        DATE_END = element.Element("DATE_END").Value == "" ? "null" : $"'{element.Element("DATE_END").Value}'",
                        LPU = lpuId,
                        COMMENT = LPUs[lpuId],
                        YEAR,
                        DATETIME_OF_ADDITION = $"{Process.GetCurrentProcess().StartTime:s}"
                    })
                    .ToList()
                    .ForEach(element => sbResultAll.Append(
                        $"{element.FAM}{delimSemicolon}" +
                        $"{element.IM}{delimSemicolon}" +
                        $"{element.OT}{delimSemicolon}" +
                        $"{element.DR}{delimSemicolon}" +
                        $"{element.W}{delimSemicolon}" +
                        $"{element.QUARTER}{delimSemicolon}" +
                        $"{element.MONTH}{delimSemicolon}" +
                        $"{element.DISP}{delimSemicolon}" +
                        $"{element.DS}{delimSemicolon}" +
                        $"{element.DATE_START}{delimSemicolon}" +
                        $"{element.DATE_END}{delimSemicolon}" +
                        $"{element.LPU}{delimSemicolon}" +
                        $"{element.COMMENT}{delimSemicolon}" +

                        $"{insertCommand[xmlFileType]}" +
                        $"'{element.FAM}'{delimComma}" +
                        $"'{element.IM}'{delimComma}" +
                        $"'{element.OT}'{delimComma}" +
                        $"'{element.DR}'{delimComma}" +
                        $"{element.W}{delimComma}" +
                        $"{element.QUARTER}{delimComma}" +
                        $"{element.MONTH}{delimComma}" +
                        $"'{element.DISP}'{delimComma}" +
                        $"'{element.DS}'{delimComma}" +
                        $"{element.DATE_START}{delimComma}" +
                        $"{element.DATE_END}{delimComma}" + 
                        $"'{element.LPU}'{delimComma}" +
                        $"'{element.COMMENT}'{delimComma}" +
                        $"{element.YEAR}{delimComma}" +
                        $"'{element.DATETIME_OF_ADDITION}')" +
                        Environment.NewLine)
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
                    .Select(element => new
                    {
                        FAM = element.Element("FAM").Value,
                        IM = element.Element("IM").Value,
                        OT = element.Element("OT").Value,
                        DR = element.Element("DR").Value,
                        W = element.Element("W").Value,
                        QUARTER = element.Element("QUARTER").Value == "" ? "null" : element.Element("QUARTER").Value,
                        MONTH = element.Element("MONTH").Value,
                        DISP = element.Element("DISP").Value,
                        DS = element.Element("DS").Value,
                        DATE_START = element.Element("DATE_START").Value == "" ? "null" : element.Element("DATE_START").Value,
                        DATE_END = element.Element("DATE_END").Value == "" ? "null" : element.Element("DATE_END").Value,
                        LPU = lpuId,
                        COMMENT = LPUs[lpuId],
                        YEAR,
                        DATETIME_OF_ADDITION = $"'{Process.GetCurrentProcess().StartTime:s}')"
                    })
                    .ToList()
                    .ForEach(element => sbResultChildren.Append(
                        $"{element.FAM}{delimSemicolon}" +
                        $"{element.IM}{delimSemicolon}" +
                        $"{element.OT}{delimSemicolon}" +
                        $"{element.DR}{delimSemicolon}" +
                        $"{element.W}{delimSemicolon}" +
                        $"{element.QUARTER}{delimSemicolon}" +
                        $"{element.MONTH}{delimSemicolon}" +
                        $"{element.DISP}{delimSemicolon}" +
                        $"{element.DS}{delimSemicolon}" +
                        $"{element.DATE_START}{delimSemicolon}" +
                        $"{element.DATE_END}{delimSemicolon}" +
                        $"{element.LPU}{delimSemicolon}" +
                        $"{element.COMMENT}{delimSemicolon}" +
                        Environment.NewLine)
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
                    resultsDuplicatesAll.Append(s.Str + delimSemicolon + s.Count);
                    resultsDuplicatesAll.Append(Environment.NewLine);
                }
                string resultCsvDuplicatesFilePath = Path.Combine(outputFolder, string.Concat(lpuMO, "_duplicates.csv"));
                StreamWriter swDuplicates = new StreamWriter(resultCsvDuplicatesFilePath, false, Encoding.GetEncoding("Windows-1251"));
                swDuplicates.WriteLine(fileHeaderDuplicates);
                swDuplicates.Write(resultsDuplicatesAll.ToString());
                swDuplicates.Close();

                swResultDuplicatesAll[xmlFileType].Write(resultsDuplicatesAll.ToString());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
