using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

using MaxMind.GeoIP2;

namespace SeaPort
{
    class Program
    {
        static void Main(string[] args)
        {
            string input_file_path = "telnet.csv";
            string output_file = "telnet-output.csv";
            int ip_count = 0;

            List<string> ip_list = new List<string>();
            List<string> proc_ip_list = new List<string>();

            using (TextFieldParser csvParser = new TextFieldParser(input_file_path))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    string[] fields = csvParser.ReadFields();
                    string port = fields[1];
                    ip_list.Add(port);
                }
            }

            try
            {
                using (var reader = new DatabaseReader("GeoLite2-City.mmdb"))
                {
                    foreach (string ip in ip_list)
                    {
                        try
                        {

                            var city = reader.City(ip);
                            string country = city.Country.IsoCode;
                            if (country == "NP")
                            {
                                proc_ip_list.Add(ip);
                                ip_count++;

                            }
                        }
                        catch (Exception e)
                        {
                            // Skip IPs not present in DB.
                            continue;
                        }

                    }
                }

            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("The MaxMind Database does not exist!!");
                Console.WriteLine("Exiting...");
            }

            File.WriteAllText(output_file, "IP\n");
            File.AppendAllLines(output_file, proc_ip_list);
            Console.WriteLine($"Total number of IPs: {ip_count}");

        }
    }
}
