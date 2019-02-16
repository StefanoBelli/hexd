using System;
using System.IO;
using System.Collections.Generic;

namespace hexd
{
    class Program
    {

        private static int GetPrintableBytesNumber(bool alsoAddress, int maximumBytes)
        {
            int oneLineBytes = Console.WindowWidth;
            if (alsoAddress)
                oneLineBytes -= 10;

            oneLineBytes /= 3;
            if (maximumBytes > oneLineBytes)
                return --oneLineBytes;

            return maximumBytes;
        }

        private static void PrintHexDump(string file, bool alsoPrintAddress, int userShowMaxBytes)
        {
            try
            {
                using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int b;
                    int counter = GetPrintableBytesNumber(alsoPrintAddress, userShowMaxBytes);
                    int loc = 0;

                    while ((b = stream.ReadByte()) != -1)
                    {
                        if (counter - GetPrintableBytesNumber(alsoPrintAddress, userShowMaxBytes) == 0)
                        {
                            Console.WriteLine();

                            if(alsoPrintAddress)
                                Console.Write("{0:X8}: ", loc);

                            counter = 0;
                        }

                        Console.Write("{0:X2} ", b);

                        counter++;
                        loc++;
                    }
                }
            }
            catch (IOException)
            {
                Console.Write("ignoring {0}: does not exist", file);
            }

            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("missing argument: filename [filenames...] [-n] [-b int]");
                return;
            }

            bool showAddress = true;
            int maxBytes = 32;
            List<string> filenames = new List<string>();

            for(int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-n")
                    showAddress = false;
                else if (args[i] == "-b")
                {
                    ++i;
                    maxBytes = Convert.ToInt32(args[i]);
                }
                else
                    filenames.Add(args[i]);
            }

            foreach(var filename in filenames)
            {
                Console.WriteLine();
                Console.WriteLine(filename);
                PrintHexDump(filename, showAddress, maxBytes);
            }
        }
    }
}
