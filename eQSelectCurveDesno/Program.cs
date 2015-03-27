using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace eQSelectCurveDesno
{
    class Program
    {
        const String INPUT_CSV_URL = "../../EngravingCurves.csv";      //URL to the input csv document
        const String API_URL = "https://airdye.io/api/transprint/EqSelectCurveDesno?DESNO="; //URL to api to get JSON
        const String OUTPUT_DESIGN_INFO_CSV_URL = "../../OutputDesignInfo.csv"; //URL to the Design info output csv file
        const String OUTPUT_CURVE_NAMES_URL = "../../OutputCurveNames.csv"; //URL to the CurveNames output file
        const String ENGRAVING_CURVE_TEXT_FILES_URL = "H:/Inkjet/Apps/1 Cheran/Engraving Curves for eQuantum"; //Url to folder containing Curve .txt files
        const int NUMBER_OF_CURVES = 10;    //Maximum number of cylinders, used for iteration when writing to the csv

        /// <summary>
        /// Gets design info from a csv, uses the design number to query an api to receive more information, then outputs
        /// all of the required information to an output csv file.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            List<string> designInfo = GetDesignNumbersFromCSV();
            List<Design> designs = GetDesignInfo(designInfo);
            PostDesignInfoToCSV(designs);
            SortedSet<string> curveNames = GetCurveNames(designs);
            PostCurveNamesToCSV(curveNames);
        }

        /// <summary>
        /// Retrieves design information (order, design number, design name)
        /// from a csv and enters them into an array
        /// </summary>
        /// <returns>Array of design numbers</returns>
        public static List<string> GetDesignNumbersFromCSV()
        {
            var reader = new StreamReader(File.OpenRead(INPUT_CSV_URL));
            var designInfo = new List<string>();
            bool skipHeaders = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (!skipHeaders)       // Skips the header of the document, first line will be omitted.
                {
                    designInfo.Add(line);
                }
                skipHeaders = false;
            }
            return designInfo;
        }

        /// <summary>
        /// Creates a list of Design objects. These are populated with data from the initial excel sheet 
        /// and information pulled down from Json object from airdye.io
        /// </summary>
        /// <param name="designInfo"></param>
        /// <returns>A list of designs</returns>
        public static List<Design> GetDesignInfo(List<string> designInfo)
        {
            var designs = new List<Design>();

            //Pushes information from the intital csv into design objets and adds them to a list of Designs
            foreach (var item in designInfo)
            {
                Design design = new Design();
                var value = item.Split(',');
                design.order = value[0];
                design.designNum = value[1];
                design.designName = value[2];
                designs.Add(design);
            }

            //Pulls down Engraving curve information from airdye.io. and pushes it into Design objects.
            //Design number is used to query the data from the API
            foreach (var item in designs)
            {
                var json = new WebClient().DownloadString(API_URL + item.designNum);
                var engCurve = JsonConvert.DeserializeObject<List<EngravingCurve>>(json);
                item.json = engCurve;
            }
            return designs;
        }

        /// <summary>
        /// Creates a csv file if it does not already exists and write the output to it.
        /// </summary>
        /// <param name="designs">List of Design Objects</param>
        public static void PostDesignInfoToCSV(List<Design> designs)
        {
            var filePath = CreateCSV(OUTPUT_DESIGN_INFO_CSV_URL);
            var csv = new StringBuilder();

            //Header for the output excel document
            csv.AppendLine("Order,Design No,Design,Curve01,Curve02,Curve03," +
                            "Curve04,Curve05,Curve06,Curve07,Curve08,Curve09,Curve10," +
                            "File01,File02,File03,File04,File05,File06,File07,File08,File09,File10");

            //Iterates through the json list in each Design object and pulls out the curves and files names.
            //Constant NUMBER_OF_CURVES used as the number of iterations for the for loop. There are always ten columns for curves
            //and file names whether or not there is one present. If there is missing information "Missing" will be appended.
            foreach (var item in designs)
            {
                int count = item.json.Count;    //Number of items in Json List
                csv.Append(item.order + ',' + item.designNum + ',' + item.designName + ',');

                //Iterate over curves
                for (int i = 0; i < NUMBER_OF_CURVES; i++)
                {
                    if (count == 0)
                    {
                        csv.Append("Missing,");
                        count++;
                    }
                    else if (i < count)
                    {
                        csv.Append(item.json[i].curve + ',');
                    }
                    else
                    {
                        csv.Append(" ,");
                    }
                }

                //Iterate over file names
                count = item.json.Count;
                for (int i = 0; i < NUMBER_OF_CURVES; i++)
                {
                    if (count == 0)
                    {
                        csv.Append("Missing,");
                        count++;
                    }
                    else if (item.json.Count > 0 && item.json.Count > i)
                    {
                        if (item.json[i].fileName == null && item.json[i].curve != null)
                        {
                            csv.Append("Missing,");
                        }
                        else if (i < count)
                        {
                            switch (item.json[i].curve)
                            {
                                case "C":
                                    csv.Append("C.txt,");
                                    break;
                                case "M":
                                    csv.Append("M.txt,");
                                    break;
                                case "Y":
                                    csv.Append("Y.txt,");
                                    break;
                                case "K":
                                    csv.Append("K.txt,");
                                    break;
                                default:
                                    csv.Append(item.json[i].fileName + ',');
                                    break;
                            }
                        }
                    }                    
                    else
                    {
                        csv.Append(" ,");
                    }
                }
                csv.AppendLine();
            }
            File.WriteAllText(filePath, csv.ToString());
        }

        /// <summary>
        /// Creates a SortedSet that pulls unique values of all curve names found in the designs
        /// from the design list passed in. Omits empty or null values
        /// </summary>
        /// <param name="designs">List of design objects</param>
        /// <returns>Sorted list of curve names</returns>
        public static SortedSet<string> GetCurveNames(List<Design> designs)
        {
            SortedSet<string> curveNames = new SortedSet<string>();
            
            foreach(var item in designs)
            {
                foreach(var curve in item.json)
                {
                    String curveName = curve.curve.ToUpper().Trim();
                    if(string.IsNullOrEmpty(curveName) == false)
                    {
                        curveNames.Add(curveName);
                    }
                }
            }

            return curveNames;
        }

        /// <summary>
        /// Compares the curve names in the curveNames set against the text files found in the INKJET folder
        /// If there is a matching Curve name and text file with the exact name, the curve name and FOUND are 
        /// written to a csv file. If not found, Missing it written.
        /// </summary>
        /// <param name="curveNames">List of curve names</param>
        public static void PostCurveNamesToCSV(SortedSet<string> curveNames)
        {
            var filePath = CreateCSV(OUTPUT_CURVE_NAMES_URL);

            var csv = new StringBuilder();

            List<string> fileNames = Directory.GetFiles(ENGRAVING_CURVE_TEXT_FILES_URL, "*.txt")
                                     .Select(path => Path.GetFileName(path))
                                     .ToList();

            bool notFound = true;
            foreach(var curve in curveNames)
            {
                foreach(var file in fileNames)
                {
                    notFound = false;
                    int indexOfExtension = file.IndexOf("."); //Cuts the .txt off of the file name
                    if (curve.ToUpper().Trim() == file.Substring(0, indexOfExtension).ToUpper().Trim())
                    {
                        csv.AppendLine(curve.Trim().ToUpper() + ", FOUND");
                        break;
                    }
                    else
                    {
                        notFound = true;
                    }
                }
                if(notFound == true)
                {
                    csv.AppendLine(curve.Trim().ToUpper() + ", MISSING");
                }
            }
            File.WriteAllText(filePath, csv.ToString());
        }

        /// <summary>
        /// Creates a csv file to the passed in location and returns the same filepath 
        /// </summary>
        /// <param name="URL"></param>
        /// <returns>Url to file location</returns>
        public static String CreateCSV(String URL)
        {
            string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = URL;

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            return filePath;
        }
    }
}

