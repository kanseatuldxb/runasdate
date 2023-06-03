using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
namespace runasdate
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }

        // Used to set the system time
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        // Used to get the system time
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);

        [DllImport("shell32.dll")]
        public static extern int ShellExecute(IntPtr hwnd, StringBuilder lpszOp, StringBuilder lpszFile, StringBuilder lpszParams, StringBuilder lpszDir, int FsShowCmd);

        [DllImport("kernel32")]//Returns the length of the obtained string buffer
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]//Return 0 for failure, non-zero for success
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

      
        //.\runasdate.exe -d:1 -m:1 -y:2022 -a:"C:\Program Files\Notepad++\notepad++.exe" -s:100
        static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(args);

            int d = argumentParser.GetArgumentAsInt("d");
            int m = argumentParser.GetArgumentAsInt("m");
            int y = argumentParser.GetArgumentAsInt("y");
            string a = argumentParser.GetArgumentAsString("a");
            int s = argumentParser.GetArgumentAsInt("s");



            Console.WriteLine($"day: {d}");
            Console.WriteLine($"month: {m}");
            Console.WriteLine($"year: {y}");
            Console.WriteLine($"application: {a}");
            Console.WriteLine($"sleep: {s}");

            string exe = a;
            int year = y;
            int month = m;
            int day = d;
            int sleepSecond = s;

            if (d < 1 && d > 31)
            {
                Console.WriteLine($"Please Correct -d in range of 1-31");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
            }
            if (m < 1 && m > 12)
            {
                Console.WriteLine($"Please Correct -m in range of 1-12");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
            }
            if (y < 1971 && y > 2199)
            {
                Console.WriteLine($"Please Correct -y in range of 1971-2199");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
            }
            if (string.IsNullOrEmpty(a) && !File.Exists(a))
            {
                Console.WriteLine($"Please Correct -a to file path of executable : {a}");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
            }
            if (s < 0 && s > 999)
            {
                Console.WriteLine($"Please Correct -s in range of 0-999");
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
            }

            //Console.ReadLine();

            SystemTime systemTime = new SystemTime();
            GetLocalTime(ref systemTime);

            SystemTime modTime = systemTime;
            modTime.wYear = (ushort)year;
            modTime.wMonth = (ushort)month;
            modTime.wDay = (ushort)day;
            SetLocalTime(ref modTime);
            Console.WriteLine(string.Format("Rollback Time Settting ->  {2}/{1}/{0} ", modTime.wYear, modTime.wMonth, modTime.wDay));

            Console.WriteLine("Strating Application {0}", exe);

            ShellExecute(IntPtr.Zero, new StringBuilder("Open"), new StringBuilder(exe), new StringBuilder(""), new StringBuilder(""), 1);

            Console.WriteLine("Sleep for {0} seconds，Wait for the game to start successfully，{0} seconds will automatically exit", sleepSecond);
            // Sleep for 10 seconds, then restore the time
            Thread.Sleep(sleepSecond * 1000);
            Console.WriteLine("restore time and exit");
            SetLocalTime(ref systemTime);

            //Console.ReadKey();
        }

    }


    class ArgumentParser
    {
        private Dictionary<string, string> arguments;

        public ArgumentParser(string[] args)
        {
            arguments = new Dictionary<string, string>();

            ParseArguments(args);
        }

        private void ParseArguments(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    string argument = arg.Substring(1);
                    string[] parts = argument.Split(new[] { ':' }, 2);
                    string key = parts[0];
                    string value = (parts.Length > 1) ? parts[1] : null;
                    arguments[key] = value;
                }
            }
        }

        public string GetArgumentAsString(string key)
        {
            if (arguments.ContainsKey(key))
                return arguments[key];

            return null;
        }

        public int GetArgumentAsInt(string key)
        {
            string value = GetArgumentAsString(key);

            if (value != null && int.TryParse(value, out int intValue))
                return intValue;

            return 0; 
        }
    }
}
