using System;
using System.Collections.Generic;
using MPI;
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

                while (Int32.Parse(arr[left].BankId) < Int32.Parse(pivot.BankId))
                {
                    left++;
                }

                while (Int32.Parse(arr[right].BankId) > Int32.Parse(pivot.BankId))
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

        private static void PrintList(List<BankCheck> list)
        {
            for (int i = 0; i < list.Count; i++)
                Console.WriteLine("BankId= {0}, AccountId= {1}, CheckNumber= {2}", list[i].BankId, list[i].AccountId, list[i].CheckNumber);
        }

        /* merge two sorted arrays v1, v2 of lengths n1, n2, respectively */
        private static List<BankCheck> Merge(List<BankCheck> v1, List<BankCheck> v2)
        {
            int a = 0, b = 0;
            //int i = 0;
            int count1 = v1.Count;
            int count2 = v2.Count;
            List<BankCheck> result = new List<BankCheck>();
            while (a < count1 && b < count2)
            {
                if (Int32.Parse(v1[a].BankId) <= Int32.Parse(v2[b].BankId))
                {
                    //result[i++] = v1[a++];
                    result.Add(v1[a++]);
                }
                else
                {
                    //result[i++] = v2[b++];
                    result.Add(v2[b++]);
                }
            }

            if (a < count1)
            {
                for (int j = a; j < count1; j++)
                {
                    //result[i++] = v1[j];
                    result.Add(v1[j]);
                }
            }
            else
            {
                for (int j = b; j < count2; j++)
                {
                    //result[i++] = v2[j];
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
                    if (comm.Size <= 0)
                    {
                        Console.WriteLine("Nothing to do..");
                        return;
                    }

                    Console.Write("Enter number of checks: ");
                    int n = Convert.ToInt32(Console.ReadLine());
                    List<BankCheck> input = new List<BankCheck>();
                    for (int i = 0; i < n; i++)
                    {
                        Console.WriteLine("BankId AccountId CheckNumber");
                        string inp = Console.ReadLine();
                        string[] data = inp.Split(' ');
                        BankCheck bankCheck = new BankCheck(data[0], data[1], data[2]);
                        input.Add(bankCheck);
                    }

                    int chunkSize = (n % comm.Size != 0) ? n / comm.Size + 1 : n / comm.Size;
                    Console.WriteLine("Chunk size = {0}", chunkSize);


                    List<List<BankCheck>> result = new List<List<BankCheck>>();

                    List<BankCheck> l = new List<BankCheck>();
                    for (int i = 0; i < chunkSize; i++)
                        l.Add(input[i]);

                    Quick_Sort(l, 0, chunkSize - 1);
                    result.Add(l);
                    PrintList(l);
                    /*
                    int rank = 0;
                    // int tag = 0;
                    for (int i = chunkSize; i < n && rank < comm.Size;)
                    {
                        rank++;
                        List<BankCheck> chunk = new List<BankCheck>();
                        int k = 0;
                        while (k < chunkSize && i < n)
                        {
                            ++k;
                            chunk.Add(input[i]);
                            ++i;
                            Console.WriteLine("k = {0}, i = {1}", k, i);
                        }
                        Console.WriteLine("Rank={0}", rank);
                        PrintList(chunk);
                        comm.Send(chunk, rank, 0);
                        List<BankCheck> list = comm.Receive<List<BankCheck>>(rank, 1);
                        Console.WriteLine("here");
                        result.Add(list);

                        Console.WriteLine("Rank={0}", rank);
                    }
                    */

                    int rank = 1;
                    int index = chunkSize;
                    while (rank < comm.Size)
                    {

                        List<BankCheck> chunkSend = new List<BankCheck>();
                        for (int numberOfElementsTaken = 0; numberOfElementsTaken < chunkSize && index < n; numberOfElementsTaken++, index++)
                        {
                            chunkSend.Add(input[index]);
                            Console.WriteLine("value of index in for : {0}", index);
                        }
                        comm.Send(chunkSend, rank, 0);
                        List<BankCheck> list = comm.Receive<List<BankCheck>>(rank, 1);
                        Console.WriteLine("Rank= {0}", rank);
                        PrintList(list);
                        result.Add(list);
                        ++rank;

                    }

                    Console.WriteLine();

                    foreach (var sublist in result)
                    {
                        PrintList(sublist);
                    }

                    Console.WriteLine();
                    List<BankCheck> resultFinal = result[0];
                    for (int resultI = 1; resultI < result.Count; resultI++)
                    {
                        resultFinal = Merge(resultFinal, result[resultI]);
                    }
                    PrintList(resultFinal);
                    Console.WriteLine();
                    
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
