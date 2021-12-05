using NHunspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DictionaryBruteForce
{
    class Program
    {
        static List<string> list = new List<string>();
        static List<string> correctedList = new List<string>();
        static readonly string DICT_AFF_NAME = "de_CH_frami.aff";
        static readonly string DICT_DIC_NAME = "de_CH_frami.dic";

        /*
        static void Main(string[] args)
        {
            //String of all characters given
            string input = "esistwahr";

            //array of word lengths
            int[] lengths = { 2, 3, 4};

            //running program
            methodA(input, lengths, true);
        }
        */


        public static void methodA(string input, int[] wordlengths, bool verbose)
        {
            Console.WriteLine("Do you want to load a list from a file? (y/n)");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine("\nEnter Filename");
                string filename = Console.ReadLine();

                Console.WriteLine("Starting Deserialization");
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
                Console.WriteLine("Running spell check");
                int n = input.Length;
                //creating list with all possible orders
                permute(input, 0, n - 1, verbose);


                if (verbose)
                    Console.WriteLine("\n\n");


                Console.WriteLine("Do you want to save the list to a file? (y/n)");
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    Console.WriteLine("\nEnter Filename");
                    string filename = Console.ReadLine();

                    Console.WriteLine("Starting Serialization");
                    try
                    {
                        //Serializer.SerializeObject(list, filename);
                        Console.WriteLine(Serializer.SerializeObject(list, filename + ".xml").FullName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error");
                        Console.WriteLine(e.Message);
                    }
                }
            }

            //split characters into words with given length
            List<string[]> splittedWordList = splitListIntoWords(list, wordlengths);

            Console.WriteLine("\n\nChecking list with " + splittedWordList.Count + " possible sentences");

            //running spellcheck on every sentence
            foreach (string[] stra in splittedWordList)
            {
                if (checkSpelling(stra, verbose))
                {
                    correctedList.Add(string.Join(" ", stra));
                }
            }

            Console.WriteLine("\n\nCorrectly spelled sentences\n");

            //printing correct sentences
            foreach (string str in correctedList)
            {
                Console.WriteLine(str);
            }


            Console.WriteLine("Do you want to save the result to a file? (y/n)");
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
