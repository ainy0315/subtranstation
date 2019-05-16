﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Tets
{
    class Classes
    {
        public static DataTable CheckSubFile(string filePath)
        {
            string[] supportedSubs = { ".srt", ".ass" };

            string ext = Path.GetExtension(filePath);
            if (supportedSubs.Contains(ext))
            {
                DataTable loadedSub = new DataTable();
                switch (ext)
                {
                    case ".srt":
                        var subFile = File.ReadAllText(filePath);
                        loadedSub = ParseSubRip(subFile);
                        break;
                }

                return loadedSub;
            }
            else
            {
                string fileName = Path.GetFileName(filePath);
                string errorMsg = String.Format("Subtitle TranStation could not open \"{0}\" because it is not a supported file type.", fileName);
                MessageBox.Show(errorMsg, "File Not Supported",MessageBoxButton.OK,MessageBoxImage.Error);
                return null;
            }
        }


        private static DataTable ParseSubRip(string subFile)
        {
            try
            {
                DataTable loadedSubs = new DataTable();
                loadedSubs.Columns.Add("Start");
                loadedSubs.Columns.Add("End");
                loadedSubs.Columns.Add("Dialog");
                loadedSubs.Columns.Add("Translation");
                loadedSubs.Columns["Translation"].DefaultValue = String.Empty;


                var lines = Regex.Split(subFile, "\r\n\r\n");
                if(lines.Length < 2)
                    lines = Regex.Split(subFile, "\n\n");

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string subPart = line;
                    while (subPart.IndexOf("\n") < 1)
                    {
                        subPart = subPart.Remove(0, 1);
                    }
                    string[] elements = Regex.Split(subPart, "\n");

                    int x;
                    bool isNumeric = Int32.TryParse(elements[0], out x);
                    if (!isNumeric)
                        throw new Exception();

                    DataRow dialog = loadedSubs.NewRow();
                    string[] delimiters = { "-->", "- >", "->" };
                    string[] times = elements[1].Split(delimiters, StringSplitOptions.None);

                    if(ValidateTimeCodes(times))
                        throw new Exception();

                    dialog[0] = times[0].Trim();
                    dialog[1] = times[1].Trim();

                    string text = "";
                    for (int i = 2; i < elements.Length; i++)
                    {
                        text += elements[i].Trim() + "\\n";
                    }

                    text = text.Remove(text.Length - 2);
                    dialog[2] = text;

                    loadedSubs.Rows.Add(dialog);
                }

                return loadedSubs;
            }
            catch (Exception)
            {
                string errorMsg = @"This is not a valid SubRip file. Please try again.";
                MessageBox.Show(errorMsg, "File Not Supported", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private static bool ValidateTimeCodes(string[] times)
        {
            TimeSpan initial = new TimeSpan(0, 0, 0);
            TimeSpan final = new TimeSpan(0, 0, 0);

            bool isTime;

            for (int i = 0; i < 2; i++){
                var match = Regex.Match(times[i], "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
                if (!match.Success)
                    return false;
                switch (i)
                {
                    case 0:
                        isTime = TimeSpan.TryParse(times[i], out initial);
                        if (!isTime)
                            return false;
                        break;
                    case 1:
                        isTime = TimeSpan.TryParse(times[i], out final);
                        if (!isTime)
                            return false;
                        break;
                }                
            }

            if (final.TotalMilliseconds < initial.TotalMilliseconds)
                return false;

            return true;
        }
    }
}