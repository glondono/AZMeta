using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZMeta
{
    public class CsvParser
    {
        public string[] Parse(string path, bool header)
        {
           string[] csv = File.ReadAllLines(path);
           
            if(csv == null)
            {
                return null;
            }

           if(csv.Length > 1 && header)
           {
               return ParseWithHeader(csv);
           }
           else
           {
               return csv;
           }
        }

        private string[] ParseWithHeader(string[] csv)
        {
            string[] response = new string[csv.Length - 1];
            string[] header = csv[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 1; i < csv.Length; i++)
            {
                string json = "{";
                string[] values = csv[i].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                for(int j = 0; j < header.Length; j++)
                {
                    json += string.Format("{0} : {1}", header[j], values[j]);
                    json += Environment.NewLine;
                }
                json += "}";
                response[i] = json;
            }
            
            return response;
        }

        public CsvParser()
        {
            
        }


    }
}
