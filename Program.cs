using System.Globalization;
using System.Text;
using Sharprompt;

namespace date_converter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            string filepath = "";
            string filename = "";

            OpenFileDialog dialog = new OpenFileDialog();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                filepath = dialog.FileName;
                filename = new FileInfo(filepath).Name;
            }

        separators:
            char[] commonSeparators = { ',', ';', '\t', '|', ':' };
            string[] lines = File.ReadAllLines(filepath, encoding: Encoding.Unicode);

            char separator = Prompt.Select("Chose separator", commonSeparators, null, detectSeparator(lines[0], commonSeparators));
            //  char separator = detectSeparator(lines[0], commonSeparators);



            if (!lines[0].Contains(separator))
            {
                Console.WriteLine("Chosen operator does not append in file");
                goto separators;
            }
            string[] options = lines[0].Split(separator);

          //  var defualtColumn = "Date";
            var column = Prompt.Select("Choose column", options);
            int columnIndex = Array.IndexOf(options, column);
            // int columnIndex = 0;
            //var defaultDateOutputFormat = "dd-MM-yyyy";
            var dateOutputFormat = Prompt.Select("Choose output data format ", new String[] { "dd.MM.yyyy", "dd-MM-yyyy", "yyyy-MM-dd" });

            var quotingAsk = Prompt.Select("Should data be surrounded by quotation marks?", new String[] { "Yes", "No" }, null, detectIfValuesQuoted(lines[0], separator));
            // var quotingAsk = detectIfValuesQuoted(lines[0], separator);
            bool quoting;
            if (quotingAsk.Equals("Tak"))
            {
                quoting = true;
            }
            else
            {
                quoting = false;
            }
            //   Console.WriteLine(lines.Length + " " + columnIndex + " " + defaultDateOutputFormat + " " + quoting);

            List<string[]> formated = formatFile(lines, columnIndex, dateOutputFormat, quoting);
            formated.Insert(0, lines[0].Split(','));

            List<String> result = new List<string>();

            foreach (string[] s in formated)
            {
                string r = "";
                foreach (string c in s)
                {
                    r += c + separator;
                }
                result.Add(r);
            }

            SaveFileDialog sdialog = new SaveFileDialog();
            sdialog.Filter = "Csv file | *.csv";
            sdialog.DefaultExt = ".csv";
            sdialog.RestoreDirectory = true;
            sdialog.FileName = "CONVERTED " + filename;

            if (DialogResult.OK == sdialog.ShowDialog())
            {
                File.WriteAllLines(sdialog.FileName, result, encoding: Encoding.Unicode);
            }
        }

        static char detectSeparator(string line, char[] separators)
        {
            var separatorCounts = separators
                .Select(sep => new { Separator = sep, Count = line.Count(c => c == sep) })
                .OrderByDescending(x => x.Count)
                .ToList();
            return separatorCounts.First().Separator;
        }

        static string detectIfValuesQuoted(string line, char separator)
        {
            string[] columns = line.Split(separator);

            if (columns.All(column => column.StartsWith("\"") && column.EndsWith("\"")))
            {
                return "Tak";
            }
            else
            {
                return "Nie";
            }
        }

        static string souroundQuotes(string input)
        {
            return '"' + input + '"';
        }



        static List<string[]> formatFile(string[] lines, int columnIndex, string dateOutputFormat, bool quotes)
        {
            //  Debug.WriteLine(lines.Length + " " + columnIndex + " " + dateOutputFormat + " " + quotes);
            List<string[]> result = new List<string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                string rawLine = lines[i];
                string[] lines2 = rawLine.Split(",");

                string processed = lines2[columnIndex].Trim('"');
                if (quotes)
                {
                    lines2[columnIndex] = souroundQuotes(reformatDate(processed, dateOutputFormat));
                }
                else
                {
                    lines2[columnIndex] = reformatDate(processed, dateOutputFormat);
                }
                result.Add(lines2);
            }
            return result;
        }

        static string reformatDate(string inputData, string dateFormat)
        {
            try
            {
                var dtt = DateTime.Parse(inputData, CultureInfo.InvariantCulture);

              //  var dt = DateTime.ParseExact(inputData, "M/d/yyyy", CultureInfo.InvariantCulture);
                return dtt.Date.ToString(dateFormat);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return inputData;
            }


        }

    }
}