﻿using MPINET.Bank;
using System;
using System.Collections.Generic;
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
            return (int.Parse(b1.BankId).CompareTo(int.Parse(b2.BankId)) == 0) ?
                (int.Parse(b1.AccountId).CompareTo(int.Parse(b2.AccountId))) :
                ((int.Parse(b1.BankId)).CompareTo(int.Parse(b2.BankId)));

        }

        private static void PrintList(List<BankCheck> list)
        {
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine("BankId= {0}, AccountId= {1}, CheckNumber= {2}", int.Parse(list[i].BankId), int.Parse(list[i].AccountId), int.Parse(list[i].CheckNumber));
        }

        /* merge two sorted arrays v1, v2 of lengths n1, n2, respectively */
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
                    int n = input.Count;

                    int chunkSize = (n % comm.Size != 0) ? n / comm.Size + 1 : n / comm.Size;


                    List<List<BankCheck>> result = new List<List<BankCheck>>();

                    List<BankCheck> l = new List<BankCheck>();
                    for (int i = 0; i < chunkSize; i++)
                        l.Add(input[i]);

                    Quick_Sort(l, 0, chunkSize - 1);
                    result.Add(l);

                    int rank = 1;
                    int index = chunkSize;
                    while (rank < comm.Size)
                    {

                        List<BankCheck> chunkSend = new List<BankCheck>();
                        for (int numberOfElementsTaken = 0; numberOfElementsTaken < chunkSize && index < n; numberOfElementsTaken++, index++)
                            chunkSend.Add(input[index]);

                        comm.Send(chunkSend, rank, 0);
                        List<BankCheck> list = comm.Receive<List<BankCheck>>(rank, 1);
                        result.Add(list);
                        ++rank;

                    }

                    List<BankCheck> resultFinal = result[0];
                    for (int resultI = 1; resultI < result.Count; resultI++)
                        resultFinal = Merge(resultFinal, result[resultI]);

                    PrintList(resultFinal);
                    Console.WriteLine();
                    comm.Dispose();
                }
                else
                {
                    List<BankCheck> chunk = comm.Receive<List<BankCheck>>(0, 0);
                    Quick_Sort(chunk, 0, chunk.Count - 1);
                    comm.Send(chunk, 0, 1);
                }


            });
        }
    }
}
