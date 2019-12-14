using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

namespace VanityKristConsole
{
    public class CommandLineArgs
    {
        public int Threads {get; set;} = 4;
        public string Term {get; set;} = "";
        public string RegEx {get; set;} = "";
        public string Output {get; set;} = "output.txt";
        public bool Numbers {get; set;} = false;

        public CommandLineArgs(string[] args)
        {
            var a = string.Join(' ', args);

            var thMatch = Regex.Match(a, @"-t (\d{1,3})");
            if (thMatch.Success)
            {   
                Console.WriteLine(thMatch.Captures[0].Value);
                Threads = Convert.ToInt32(thMatch.Captures[0].Value.Substring(3));
            }
            
            var trMatch = Regex.Match(a, @"-m ([A-z0-9]{1,10})");
            if (trMatch.Success)
                Term = trMatch.Groups[0].Value.Substring(3);

            var rMatch = Regex.Match(a, @"-r (.+)");
            if (rMatch.Success)
                RegEx = rMatch.Groups[0].Value.Substring(3);

            var oMatch = Regex.Match(a, @"-o (.+)");
            var invalid = new List<char>();
            invalid.AddRange(Path.GetInvalidFileNameChars());
            invalid.AddRange(Path.GetInvalidPathChars());

            if (oMatch.Success)
            {
                var output = oMatch.Groups[0].Value.Substring(3);
                output = output.Trim(invalid.ToArray());
                Output = output;
            }

            var nMatch = Regex.Match(a, "-n");
            if (nMatch.Success)
                Numbers = true;
        }
    }
}