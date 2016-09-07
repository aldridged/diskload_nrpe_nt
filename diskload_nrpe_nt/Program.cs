using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;


namespace diskload_nrpe_nt
{
    class Program
    {
        // Performance Counters
        static PerformanceCounter diskReadsPerformanceCounter = new PerformanceCounter();
        static PerformanceCounter diskWritesPerformanceCounter = new PerformanceCounter();

        static void Main(string[] args)
        {
            try
            {
                //Globals
                double warn_level = 100.0;
                double crit_level = 200.0;
                double utilization_iops = 0.0;

                //Check Args
                if (args.Length < 1)
                {
                    PrintUsage();
                    Environment.Exit(0);
                }

                if (args.Length < 2 || args.Length > 2)
                {
                    PrintUsage();
                    Environment.Exit(0);
                }
                else
                {
                    //Parse args
                    warn_level = Convert.ToDouble(args[0]);
                    crit_level = Convert.ToDouble(args[1]);
                }

                //Get stats
                InitCounters();
                System.Threading.Thread.Sleep(1000);  // Wait for counters to init
                double read_iops = diskReadsPerformanceCounter.NextValue();
                double write_iops = diskWritesPerformanceCounter.NextValue();

                //Sample over 5 s taking average
                for (int i = 0; i < 5; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    read_iops += diskReadsPerformanceCounter.NextValue();
                    write_iops += diskWritesPerformanceCounter.NextValue();
                }
                read_iops = Math.Round(read_iops / 6.0,4);
                write_iops = Math.Round(write_iops / 6.0,4);

                if (read_iops > write_iops)
                {
                    utilization_iops = read_iops;
                }
                else
                {
                    utilization_iops = write_iops;
                }

                //Compare to levels and exit with correct status
                if (utilization_iops >= crit_level)
                {
                    Console.WriteLine("CRITICAL - Disk IO Utilization {0}/{1} iops", read_iops, write_iops);
                    Environment.Exit(2);
                }
                else if (utilization_iops >= warn_level)
                {
                    Console.WriteLine("WARNING - Disk IO Utilization {0}/{1} iops", read_iops, write_iops);
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("OK - Disk IO Utilization {0}/{1} iops", read_iops, write_iops);
                    Environment.Exit(0);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred - " + ex.ToString());
            }
        }

        static void InitCounters()
        {
            // Init Physical Disk counters
            diskReadsPerformanceCounter.CategoryName = "PhysicalDisk";
            diskReadsPerformanceCounter.CounterName = "Disk Reads/sec";
            diskReadsPerformanceCounter.InstanceName = "_Total";

            diskWritesPerformanceCounter.CategoryName = "PhysicalDisk";
            diskWritesPerformanceCounter.CounterName = "Disk Writes/sec";
            diskWritesPerformanceCounter.InstanceName = "_Total";
        }

        static void PrintUsage()
        {
            //Print command usage info
            Console.WriteLine("Usage: diskload_nrpe_nt.exe <warning iops> <critical iops>");
        }
    }
}
