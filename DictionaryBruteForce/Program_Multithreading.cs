using NHunspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DictionaryBruteForce
{
    class Program_Multithreading
    {
        #region Don't Change
        static bool verbose;
        static int[] lengths;
        static string input;
        static int progress = 0;
        static int runningThreads = 0;
        #endregion

        #region Dict Settings
        static readonly string DICT_AFF_NAME = "de_CH_frami.aff";
        static readonly string DICT_DIC_NAME = "de_CH_frami.dic";
        #endregion

        #region lists
        static List<string> list = new List<string>();
        static List<string> correctedList = new List<string>();
        #endregion

        #region User-Changable
        static int THREAD_COUNT = 8;
        #endregion

        static void Main(string[] args)
        {
            //running program
            methodA(input, lengths, verbose, THREAD_COUNT, 3);
            Console.WriteLine("\nStarting Spellcheck\n");
            Console.WriteLine("\nthreadID\tStartingPosition\tLength");
            // Creating and initializing threads
            List<Thread> threads = new List<Thread>();
            
            for (int i = 0; i < THREAD_COUNT; i++)
            {
                int id = i;
                Thread thrd = new Thread(() => spellCheckingOfWordsList(lengths, THREAD_COUNT, id, verbose));
                threads.Add(thrd);
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
                runningThreads++;
            }

            

            while(runningThreads != 0)
            {
                int x = Console.CursorLeft;
                int y = Console.CursorTop;
                Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
                Console.Write(new string(' ', Console.WindowWidth));
                Console.Write("\r{0}", progress + " / " + list.Count);
                Console.SetCursorPosition(x, y);
                Thread.Sleep(500);
            }
            Console.Write(new string(' ', Console.WindowWidth));
            Console.WriteLine("\nSpellcheck Finished");
            methodB();
        }

        public static void methodA(string input, int[] wordlengths, bool verbose, int threadsCount, int threadID)
        {
            Console.WriteLine("Enter the string of characters");
            input = Console.ReadLine();

            Console.WriteLine("\nEnter the number of characters per word");
            string nums = Console.ReadLine();
            lengths = nums.Split(' ').Select(int.Parse).ToList().ToArray();

            Console.WriteLine("\nEnter the number of Threads you want to use");
            THREAD_COUNT = int.Parse(Console.ReadLine());

            Console.WriteLine("\nDo you want to load a list from a file? (y/n)");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine("\n\nEnter Filename");
                string filename = Console.ReadLine();

                Console.WriteLine("\nStarting Deserialization\n");
                try
                {
                    list = Serializer.Deserialize(filename + ".xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("\n\nRunning spell check");
                Console.WriteLine("Please wait... (might take some time)");
                int n = input.Length;
                //creating list with all possible orders
                permute(input, 0, n - 1, verbose);


                if (verbose)
                    Console.WriteLine("\n\n");

                Console.WriteLine("\nFinished");

                Console.WriteLine("\nDo you want to save the list to a file? (y/n)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.WriteLine("\nEnter Filename");
                    string filename = Console.ReadLine();

                    Console.WriteLine("Starting Serialization");
                    try
                    {
                        Console.WriteLine(Serializer.SerializeObject(list, filename + ".xml").FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error");
                        Console.WriteLine(e.Message);
                    }
                }
            } 
        }

        public static void spellCheckingOfWordsList(int[] wordlengths, int threadsCount, int threadID, bool verbose)
        {
            int length = list.Count / threadsCount;
            List<string> partialList;
            Console.WriteLine("id:" + threadID + "\t\t" + threadID * length + "\t\t\t" + length);
            if (threadID + 1 == threadsCount)
                partialList = list.GetRange(threadID * length, list.Count - threadID * length);
            else
                partialList = list.GetRange(threadID * length, length);

            List<string[]> splittedWordList = splitListIntoWords(partialList, wordlengths);

            //running spellcheck on every sentence
            foreach (string[] stra in splittedWordList)
            {
                progress++;
                if (checkSpelling(stra, verbose))
                {
                    correctedList.Add(string.Join(" ", stra));
                }
            }
            Interlocked.Decrement(ref runningThreads);
        }

        public static void methodB()
        {
            Console.WriteLine("\n\nCorrectly spelled sentences\n");
            //printing correct sentences
            foreach (string str in correctedList)
            {
                Console.WriteLine(str);
            }

            Console.WriteLine("\nDo you want to save the result to a file? (y/n)");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine("\nEnter Filename");
                string filename = Console.ReadLine();

                Console.WriteLine("Starting Serialization");
                try
                {
                    Console.WriteLine(Serializer.SerializeObject(correctedList, filename + ".xml").FullName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error");
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static List<string[]> splitListIntoWords(List<string> list, int[] wordLength)
        {
            List<string[]> finalList = new List<string[]>();
            int wordCount = wordLength.Length;

            for (int i = 0; i < list.Count; i++)
            {
                string[] s = new string[wordCount];
                int curBeg = 0;
                for (int j = 0; j < wordCount; j++)
                {
                    s[j] = list.ElementAt(i).Substring(curBeg, wordLength[j]);
                    curBeg += wordLength[j];
                }
                finalList.Add(s);
            }
            return finalList;
        }

        static bool checkSpelling(string word)
        {
            using (Hunspell hunspell = new Hunspell(DICT_AFF_NAME, DICT_DIC_NAME))
            {
                bool correct = hunspell.Spell(word);
                return (correct ? true : false);
            }
        }
        static bool checkSpelling(string[] words, bool verbose)
        {
            using (Hunspell hunspell = new Hunspell("de_CH_frami.aff", "de_CH_frami.dic"))
            {
                if (verbose)
                    Console.WriteLine(string.Join(" ", words));
                for (int i = 0; i < words.Length; i++)
                {
                    if (!hunspell.Spell(words[i]))
                    {
                        return false;
                    }
                }
                
                return true;
            }
        }

        // The main recursive method
        // to print all possible
        // strings of length k
        static void printAllKLengthRec(char[] set, String prefix, int n, int k)
        {
            // Base case: k is 0,
            // print prefix
            if (k == 0)
            {
                Console.WriteLine(prefix);
                return;
            }
            // One by one add all characters
            // from set and recursively
            // call for k equals to k-1
            for (int i = 0; i < n; ++i)
            {
                // Next character of input added
                String newPrefix = prefix + set[i];
                // k is decreased, because
                // we have added a new character
                printAllKLengthRec(set, newPrefix,
                                        n, k - 1);
            }
        }

        /**
        * permutation function
        * @param str string to
        calculate permutation for
        * @param l starting index
        * @param r end index
        */
        private static void permute(String str, int l, int r, bool verbose)
        {
            if (l == r)
            {
                if (verbose)
                {
                    Console.WriteLine(str);
                }

                if (!list.Contains(str))
                {
                    list.Add(str);
                }
                /*
                if (checkSpelling(str))
                    correctList.Add(str);
                */
            }
            else
            {
                for (int i = l; i <= r; i++)
                {
                    str = swap(str, l, i);
                    permute(str, l + 1, r, verbose);
                    str = swap(str, l, i);
                }
            }
        }

        /**
        * Swap Characters at position
        * @param a string value
        * @param i position 1
        * @param j position 2
        * @return swapped string
        */
        public static String swap(String a, int i, int j)
        {
            char temp;
            char[] charArray = a.ToCharArray();
            temp = charArray[i];
            charArray[i] = charArray[j];
            charArray[j] = temp;
            string s = new string(charArray);
            return s;
        }
    }
}
