using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Controllers
{
    [TestClass]
    public class ParallelPocessingTest
    {
        [TestMethod]
        public void ParallelProccesing()
        {
            object locker = new object();
            DataTable dt = new DataTable();
            dt.Columns.Add("Test1");
            var r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var p = dt.NewRow();
                p["Test1"] = r.Next(0, 1000);
                dt.Rows.Add(p);
            }
            List<DataRow> newTable = dt.AsEnumerable().ToList();
            StringBuilder built = new StringBuilder();
            int numberOfThread = 10;
            var jobsList = new List<TestClass>();
            Parallel.ForEach(
               newTable,
               new ParallelOptions { MaxDegreeOfParallelism = numberOfThread },
                   (row, n, i) =>
                   {
                       // lock (locker)
                       {
                           int index = (int)i;
                           DataRow dataRow = newTable[index];
                           var item = new TestClass()
                           {
                               CurrentTaskId = Task.CurrentId.Value,
                               Index = index,
                               JobId = dataRow["Test1"].ToString()
                           };
                           jobsList.Add(item);
                           Console.WriteLine(item.CurrentTaskId + " " + item.Index + " = " + item.JobId);
                       }
                       //  Thread.Sleep(sleepTime);
                   }
            );
            Console.WriteLine("Test");
            Thread.Sleep(10000);
            foreach (var item in jobsList)
            {
                built.AppendLine(item.CurrentTaskId + " " + item.Index + " = " + item.JobId);
            }
            File.WriteAllText(@"D:\ProjectEY\Test\task.txt", built.ToString());
            Console.ReadLine();
        }

        [TestMethod]
        public void ParalelProcessing2()
        {
            string[] colors = {
                                  "1. Red",
                                  "2. Green",
                                  "3. Blue",
                                  "4. Yellow",
                                  "5. White",
                                  "6. Black",
                                  "7. Violet",
                                  "8. Brown",
                                  "9. Orange",
                                  "10. Pink"
                              };
            Console.WriteLine("Traditional foreach loop\n");
            //start the stopwatch for "for" loop
            var sw = Stopwatch.StartNew();
            foreach (string color in colors)
            {
                Console.WriteLine("{0}, Thread Id= {1}", color, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(10);
            }
            Console.WriteLine("foreach loop execution time = {0} seconds\n", sw.Elapsed.TotalSeconds);
            Console.WriteLine("Using Parallel.ForEach");
            //start the stopwatch for "Parallel.ForEach"
            sw = Stopwatch.StartNew();
            Parallel.ForEach(colors, color =>
            {
                Console.WriteLine("{0}, Thread Id= {1}", color, Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(10);
            }
            );
            Console.WriteLine("Parallel.ForEach() execution time = {0} seconds", sw.Elapsed.TotalSeconds);
            // Console.Read();
        }

        [TestMethod]
        public void ParallelPocessing3()
        {
            ConcurrentBag<string> monitor = new ConcurrentBag<string>();
            ConcurrentBag<string> monitorOut = new ConcurrentBag<string>();
            var arrayStrings = new string[1000];
            var options = new ParallelOptions { MaxDegreeOfParallelism = int.MaxValue };
            Parallel.ForEach<string>(arrayStrings, options, someString =>
            {
                monitor.Add(DateTime.UtcNow.Ticks.ToString());
                //monitor.TryTake(out string result);
                //monitorOut.Add(result);
            });

            var startTimes = monitorOut.OrderBy(x => x.ToString()).ToList();
            Console.WriteLine(string.Join(Environment.NewLine, startTimes.Take(10)));
        }
    }
}