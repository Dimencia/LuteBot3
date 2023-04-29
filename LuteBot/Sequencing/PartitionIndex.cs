using LuteBot.IO.Files;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuteBot.LuteMod.Indexing
{
    public class PartitionIndex
    {
        public static readonly string Header = "PartitionIndex";

        private List<string> partitionNames;
        private bool loaded;

        public List<string> PartitionNames { get => partitionNames; set => partitionNames = value; }
        public bool Loaded { get => loaded;}

        public PartitionIndex()
        {
            PartitionNames = new List<string>();
        }

        public void LoadIndex()
        {
            string temp = SaveManager.ReadSavFile(SaveManager.SaveFilePath + @"PartitionIndex");
            if (temp.Length == 0)
            {
                loaded = false;
                return;
            }
            FromString(temp);
            loaded = true;
        }

        Regex fileReg = new Regex(@"([^\[]*)\[[0-9]*\].sav");
        public void AddFileInIndex(string fileName)
        {
            var name = Path.GetFileName(fileName);
            if (fileReg.IsMatch(name))
            {
                FileIO.CopyPasteFile(fileName, SaveManager.SaveFilePath + name);
                var filteredName = fileReg.Match(name).Groups[1].Value;
                if (!partitionNames.Contains(filteredName) && filteredName != "PartitionIndex")
                {
                    partitionNames.Add(filteredName);
                }
            }

        }

        public void SaveIndex()
        {
            SaveManager.WriteSaveFile(SaveManager.SaveFilePath + @"PartitionIndex", ToString());
        }

        public override string ToString()
        {
            StringBuilder strbld = new StringBuilder();

            strbld.Append("|PartitionIndex|");
            foreach (string partitionName in PartitionNames)
            {
                strbld.Append(partitionName).Append("|");
            }

            return strbld.ToString();
        }

        public void FromString(string content)
        {
            string[] splitContent = content.Split('|');
            if (splitContent[1] == "PartitionIndex")
            {
                for (int i = 2; i < splitContent.Length - 1; i++)
                {
                    PartitionNames.Add(splitContent[i]);
                }
            }
        }
    }
}
