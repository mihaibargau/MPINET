using MPI;
using MPINET.Bank;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace MPINET
{
    class Program
    {
        private static void Quick_Sort(List<BankCheck> arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);

                if (pivot > 1)
                {
                    Quick_Sort(arr, left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    Quick_Sort(arr, pivot + 1, right);
                }
            }

        }
        private static int Partition(List<BankCheck> arr, int left, int right)
        {
            BankCheck pivot = arr[left];
            while (true)
            {
                while (CompareBankCheck(arr[left], pivot) < 0)
                {
                    left++;
                }

                while (CompareBankCheck(arr[right], pivot) > 0)
                {
                    right--;
                }

                if (left < right)
                {
                    if (arr[left] == arr[right]) return right;

                    BankCheck temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = temp;

                }
                else
                {
                    return right;
                }
            }
        }

        public static int CompareBankCheck(BankCheck b1, BankCheck b2)
        {

            //   return (String.Compare(b1.BankId, b2.BankId) == 0) ? String.Compare(b1.AccountId, b2.AccountId) : String.Compare(b1.BankId, b2.BankId);
            return (b1.BankId.CompareTo(b2.BankId) == 0) ?
                b1.AccountId.CompareTo(b2.AccountId) :
                b1.BankId.CompareTo(b2.BankId);
        }

        private static void PrintList(List<BankCheck> list)
        {
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine("BankId= {0}, AccountId= {1}, CheckNumber= {2}", list[i].BankId, list[i].AccountId, list[i].CheckNumber);
        }

        private static List<BankCheck> Merge(List<BankCheck> v1, List<BankCheck> v2)
        {
            int a = 0, b = 0;
            int count1 = v1.Count;
            int count2 = v2.Count;
            List<BankCheck> result = new List<BankCheck>();
            while (a < count1 && b < count2)
            {
                if (CompareBankCheck(v1[a], v2[b]) < 0)
                {
                    result.Add(v1[a++]);
                }
                else
                {
                    result.Add(v2[b++]);
                }
            }

            if (a < count1)
            {
                for (int j = a; j < count1; j++)
                {
                    result.Add(v1[j]);
                }
            }
            else
            {
                for (int j = b; j < count2; j++)
                {
                    result.Add(v2[j]);
                }
            }
            return result;
        }



        static void Main(string[] args)
        {
            MPI.Environment.Run(ref args, comm =>
            {
                if (comm.Rank == 0)
                {
                    string filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "input.txt");
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("Input file not found...");
                        comm.Abort(1);
                    }
                    string[] lines = File.ReadAllLines(filePath);
                    List<BankCheck> input = new List<BankCheck>();
                    foreach (string line in lines)
                    {
                        string[] col = line.Split(" ");
                        // Bankid AccountId CheckNumber
                        input.Add(new BankCheck(col[0], col[1], col[2]));

                    }
                    Console.WriteLine("Input file reading done!");
                    int n = input.Count;
                    Console.WriteLine("Number of bank check(s) read= {0}", n);

                    int chunkSize = (n % comm.Size != 0) ? n / comm.Size + 1 : n / comm.Size;

                    List<List<BankCheck>> result = new List<List<BankCheck>>();

                    List<BankCheck> l = new List<BankCheck>();
                    for (int i = 0; i < chunkSize; i++)
                        l.Add(input[i]);
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    Quick_Sort(l, 0, chunkSize - 1);
                    result.Add(l);

                    int rank = 1;
                    int index = chunkSize;
                    while (rank < comm.Size)
                    {

                        List<BankCheck> chunkSend = new List<BankCheck>();
                        for (int numberOfElementsTaken = 0; numberOfElementsTaken < chunkSize && index < n; numberOfElementsTaken++, index++)
                            chunkSend.Add(input[index]);

                        comm.ImmediateSend(chunkSend, rank, 0);
                        ++rank;
                    }
                    rank = 1;
                    while (rank < comm.Size)
                    {
                        result.Add((List<BankCheck>)comm.ImmediateReceive<List<BankCheck>>(rank, 1).GetValue());
                        ++rank;
                    }

                    List<BankCheck> resultFinal = result[0];
                    for (int resultI = 1; resultI < result.Count; resultI++)
                        resultFinal = Merge(resultFinal, result[resultI]);
                    Console.WriteLine("Answer...");
                    PrintList(resultFinal);
                    stopWatch.Stop();
                    Console.WriteLine();
                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    Console.WriteLine("Run Time " + elapsedTime);
                    comm.Dispose();
                }
                else
                {
                    ReceiveRequest recv = comm.ImmediateReceive<List<BankCheck>>(0, 0);
                    List<BankCheck> chunk = (List<BankCheck>)recv.GetValue();
                    Quick_Sort(chunk, 0, chunk.Count - 1);
                    comm.ImmediateSend(chunk, 0, 1);
                }
            });
        }
    }
}
