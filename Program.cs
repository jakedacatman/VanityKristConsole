using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace VanityKristConsole
{
    class Program
    {
        int counter = 0;
        static void Main(string[] args) => new Program().Start(args).GetAwaiter().GetResult();
        bool nums = true;
        string term = "";
        TextWriter tw;
        Regex reg = null;

        private async Task Start(string[] args)
        {
            try
            {
                var o = new CommandLineArgs(args);
                if (o.Term == "" && o.RegEx == "")
                {
                    Console.WriteLine("The term or regex must be set.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                if (o.Term != "" && o.RegEx != "")
                {
                    Console.WriteLine("The term and regex cannot both be set at the same time.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                CancellationTokenSource cts = new CancellationTokenSource();

                ulong basepasswd = RandUlong();

                tw = new StreamWriter(o.Output);
                nums = o.Numbers;
                term = o.Term;
                if (o.RegEx != string.Empty) reg = new Regex(o.RegEx);

                Console.WriteLine($"args: {string.Join(' ', args)}");
                Console.WriteLine($"starting with base {basepasswd:x}");
                Console.WriteLine($"using {o.Threads} threads");

                ulong perThread = (ulong.MaxValue - basepasswd) / (ulong)o.Threads;

                var bp = basepasswd;

                for (int i = 0; i < o.Threads; i++)
                {
                    SHA256 h = SHA256.Create();
                    Console.WriteLine($"spawned thread {i}, working from {bp:x} to {(bp + perThread):x}");
                    Task.Run(() => MinerThread(i, perThread, bp, reg, h));
                    bp += perThread;
                }
                Console.WriteLine();

                Task.Run(() => UpdateCounter(cts.Token));
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }

        private Task MinerThread(int id, ulong workSize, ulong basepasswd, Regex reg, SHA256 h)
        {
            if (reg == null)
            {
                if (nums)
                    for (ulong curr = basepasswd; curr < (basepasswd + workSize); curr++)
                    {
                        var passwd = $"{curr:x}";
                        var address = ToV2(passwd, h);

                        counter++;

                        if (!address.Contains(term))
                        continue;

                        Write(id, address, passwd);
                    }
                else
                    for (ulong curr = basepasswd; curr < (basepasswd + workSize); curr++)
                    {
                        var passwd = $"{curr:x}";
                        var address = ToV2(passwd, h);

                        counter++;

                        if (HasNumbers(address))
                            continue;
                        else if (!address.Contains(term))
                            continue;

                        Write(id, address, passwd);
                    }
            }
            else
            {
                for (ulong curr = basepasswd; curr < (basepasswd + workSize); curr++)
                {
                    var passwd = $"{curr:x}";
                    var address = ToV2(passwd, h);

                    counter++;

                    if (!reg.Match(address).Success)
                        continue;

                    Write(id, address, passwd);
                }
            }
            return Task.CompletedTask;
        }

        private bool HasNumbers(string input)
        {
            for (int i = 0; i < input.Length; i++)
                if (char.IsDigit(input[i])) return true;
            return false;
        }

        private string ToV2(string passwd, SHA256 h)
        {
            var protein = new string[9];
            var stick = BytesToHex(DoubleHash(Encoding.UTF8.GetBytes(passwd), h));
            int n;
            for (n = 0; n < 9; n++)
            {
                protein[n] = string.Empty + stick[0] + stick[1];
                stick = BytesToHex(DoubleHash(Encoding.UTF8.GetBytes(stick), h));
            }
            n = 0;
            var v2 = new StringBuilder(10);
            v2.Append('k');
            int link;
            while (n < 9)
            {
                link = Convert.ToInt32(string.Empty + stick[2 * n] + stick[2 * n + 1], 16) % 9;
                if (protein[link] != string.Empty)
                {
                    v2.Append(base36[(int)Math.Floor(Convert.ToByte(protein[link], 16) / 7d)]);
                    protein[link] = string.Empty;
                    n++;
                }
                else stick = BytesToHex(h.ComputeHash(Encoding.UTF8.GetBytes(stick)));
            } 
            return v2.ToString();
        }

        private byte[] DoubleHash(byte[] input, SHA256 h)
        {
            return h.ComputeHash(Encoding.UTF8.GetBytes(BytesToHex(h.ComputeHash(input))));
        }

        private static readonly char[] base36 = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
            'u', 'v', 'w', 'x', 'y', 'z', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e',
            'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'
        };

        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        private static string BytesToHex(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        private ulong RandUlong()
        {
            var r = new Random();
            var buffer = new byte[sizeof(ulong)];
            r.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        private void Write(int id, string address, string passwd)
        {
            Console.CursorTop -= 1;
            Console.WriteLine($"\nthread {id} found {address}, with pw {passwd}");
            TextWriter.Synchronized(tw).WriteLine($"{address}:{passwd}");
        }

        private Task UpdateCounter(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return Task.CompletedTask;
                Console.CursorLeft = 0;
                Console.Write($"{counter} A/s");
                counter = 0;
                Thread.Sleep(1000);
            }
        }
    }
    public static class StringExtensions
    {
        public static string MakeAlphanumeric(this string text)
        {
            var s = text.Where(x => char.IsLetterOrDigit(x)).ToArray();
            return new string(s);
        }
    }
}
