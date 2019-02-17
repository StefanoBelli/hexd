using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace hexd
{
    class Program
    {
        class Arguments
        {
            private Arguments() { }

            public static Arguments ParseCmdline(string[] cargs)
            {
                Arguments parsedArguments = new Arguments();

                for(int i = 0; i < cargs.Length; ++i)
                {
                    if (cargs[i] == "-ToSystemClipboard")
                        parsedArguments.ToSystemClipboard = true;
                    else if (cargs[i] == "-NoBloatedOutput")
                        parsedArguments.NoBloatedOutput = true;
                    else if (cargs[i] == "-NoOffset")
                        parsedArguments.NoOffset = true;
                    else if (cargs[i] == "-NoSpaces")
                        parsedArguments.NoSpaces = true;
                    else if (cargs[i] == "-Lowercase")
                        parsedArguments.Lowercase = true;
                    else if (cargs[i] == "-MaxBytes")
                    {
                        try
                        {
                            parsedArguments.MaxBytes = Convert.ToInt32(cargs[++i]);
                        }
                        catch(IndexOutOfRangeException)
                        {
                            Console.WriteLine("-MaxBytes requires a value");
                            Environment.Exit(1);
                        }
                        catch(FormatException)
                        {
                            Console.WriteLine("{0} is not a number", cargs[i]);
                            Environment.Exit(1);
                        }
                    }
                    else
                        parsedArguments.Filenames.Add(cargs[i]);
                }

                return parsedArguments;
            }

            public bool ToSystemClipboard = false;
            public bool NoBloatedOutput = false;
            public bool NoOffset = false;
            public bool NoSpaces = false;
            public bool Lowercase = false;
            public int MaxBytes = 16;
            public List<string> Filenames = new List<string>();
            
        }

        private static Arguments programArgs;
        
        private static int GetPrintableBytesNumber()
        {
            if (programArgs.ToSystemClipboard)
                return programArgs.MaxBytes;

            int oneLineBytes = Console.WindowWidth;
            if (!programArgs.NoOffset)
                oneLineBytes -= 10;

            oneLineBytes /= 3;
            if (programArgs.MaxBytes > oneLineBytes)
                return --oneLineBytes;

            return programArgs.MaxBytes;
        }

        private static string GetHexDump(string file)
        {
            int b;
            int counter = GetPrintableBytesNumber();
            int loc = 0;
            string formatter;

            if (programArgs.Lowercase)
                formatter = "{0:x2}";
            else
                formatter = "{0:X2}";

            if (!programArgs.NoSpaces)
                formatter += " ";

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {

                        while ((b = stream.ReadByte()) != -1)
                        {
                            if (counter - GetPrintableBytesNumber() == 0)
                            {
                                writer.WriteLine();

                                if (!programArgs.NoOffset)
                                    writer.Write("{0:X8}: ", loc);

                                counter = 0;
                            }

                            writer.Write(formatter, b);

                            counter++;
                            loc++;
                        }
                    }
                }
                catch (IOException)
                {
                    if (!programArgs.NoBloatedOutput)
                        writer.WriteLine("\nignoring {0}: does not exist", file);
                    return writer.ToString();
                }

                writer.WriteLine();
                return writer.ToString();
            }
        }

        private static string GetFullHexDump()
        {
            using (StringWriter fullDump = new StringWriter())
            {
                foreach (var filename in programArgs.Filenames)
                {
                    if (!programArgs.NoBloatedOutput)
                        fullDump.WriteLine("\n{0}", filename);

                    fullDump.Write(GetHexDump(filename));
                }

                return fullDump.ToString();
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            programArgs = Arguments.ParseCmdline(args);

            string dump = GetFullHexDump();
            if (dump.Length > 0 && dump[dump.Length - 1] == '\n')
                dump = dump.Remove(dump.Length - 1, 1);

            if (programArgs.ToSystemClipboard)
                Clipboard.SetText(dump);
            else
                Console.WriteLine(dump);
        }
    }
}
