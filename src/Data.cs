using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnimalCrossingFlowers
{
    public static class Data
    {
        private static string name;
        private static Dictionary<string, FlowerColor> flowerColors = new Dictionary<string, FlowerColor>();
        private static Dictionary<string, Flower> flowers = new Dictionary<string, Flower>();
        private static List<BreedingPairSection> breedingPairs = new List<BreedingPairSection>();

        public static void Read(string path)
        {
            string[][][][] data = Regex.Split(File.ReadAllText(path).Replace("\r", string.Empty), "\\n(?:\\s*\\n)+").Select(
                (section) => section.Split('\n').Select(
                    (line) => line.Trim().Split(',').Select(
                        (item) => item.Trim().Split(' ')
                    ).ToArray()
                ).ToArray()
            ).ToArray();

            if (data.Length == 0)
            {
                throw new InputException("Data file is empty");
            }
            name = data[0][0][0][0];
            if (name.Length == 0)
            {
                throw new InputException("Please specify flower name at start of file");
            }
            if (data.Length < 2)
            {
                throw new InputException("Please list flower colors in 2nd section");
            }
            ParseFlowerColors(data[1][0]);
            if (data.Length < 3)
            {
                throw new InputException("Please list flower variants in 3rd section");
            }
            ParseFlowers(data[2]);
            if (data.Length < 4)
            {
                throw new InputException("No breeding pairs have been listed");
            }
            for (int k = 3; k < data.Length; k++)
            {
                ParseBreedingPairSection(data[k]);
            }
        }

        private static void ParseFlowerColors(string[][] data)
        {
            foreach (string[] item in data)
            {
                if (item.Length < 3)
                {
                    throw new InputException("Flower color must contain ID and a pair of coordinates");
                }
                string id = item[0];
                if (flowerColors.ContainsKey(id))
                {
                    throw new RegistryException("Flower Color", id);
                }
                flowerColors[id] = new FlowerColor(id, int.Parse(item[1]), int.Parse(item[2]));
            }
        }

        private static void ParseFlowers(string[][][] data)
        {
            foreach (string[][] line in data)
            {
                string[] item = line[0];
                if (item.Length < 3)
                {
                    throw new InputException("Flower variant must contain ID, flower color ID, and background color ID");
                }
                string id = item[0];
                if (flowers.ContainsKey(id))
                {
                    throw new RegistryException("Flower", id);
                }
                bool pure = false;
                string source = null;
                for (int k = 3; k < item.Length; k++)
                {
                    if (item[k] == "pure")
                    {
                        pure = true;
                    }
                    else
                    {
                        source = item[k];
                    }
                }
                flowers[id] = new Flower(id, item[1], item[2], pure, source);
            }
        }

        private static void ParseBreedingPairSection(string[][][] data)
        {
            List<BreedingPair> pairs = new List<BreedingPair>();
            BreedingPair lastPair = null;
            if (data[0][0].Length == 0)
            {
                throw new InputException("Breeding pair section must start with target flower");
            }
            string[] targets = data[0][0];
            if (data[0].Length < 2 || data[0][1].Length == 0)
            {
                throw new InputException("Breeding pair section must specify background color");
            }
            string background = data[0][1][0];
            string border = null;
            if (data[0][1].Length > 1)
            {
                border = data[0][1][1];
            }
            if (data.Length < 2)
            {
                throw new InputException("Breeding pair section must list breeding pairs");
            }
            for (int k = 1; k < data.Length; k++)
            {
                string[][] line = data[k];
                if (line[0].Length == 0)
                {
                    throw new InputException("Breeding pair must start with both parents");
                }
                if (line[0][0] == "-")
                {
                    lastPair.AddBreedingTest(ParseBreedingTest(line));
                }
                else if (line[0][0] == "!")
                {
                    if (line[0].Length < 2)
                    {
                        throw new InputException("Please specify impossible flower");
                    }
                    lastPair = new BreedingPair(GetFlower(line[0][1]));
                    pairs.Add(lastPair);
                }
                else
                {
                    lastPair = ParseBreedingPair(line);
                    pairs.Add(lastPair);
                }
            }
            breedingPairs.Add(new BreedingPairSection(pairs, targets, background, border));
        }

        private static BreedingPair ParseBreedingPair(string[][] data)
        {
            if (data.Length < 2 || data[0].Length == 0 || data[1].Length == 0)
            {
                throw new InputException("Breeding pair must start with both parents");
            }
            BreedingPair pair = new BreedingPair(GetFlower(data[0][0]), GetFlower(data[1][0]));
            if (data.Length < 3)
            {
                throw new InputException("Breeding pair must list one or more results");
            }
            for (var k = 2; k < data.Length; k++)
            {
                string[] item = data[k];
                if (item.Length < 1)
                {
                    throw new InputException("Breeding result must specify result flower ID");
                }
                if (item.Length < 2)
                {
                    throw new InputException("Breeding result must specify result chance in percentage");
                }
                pair.AddBreedingResult(GetFlower(item[0]), int.Parse(item[1]));
            }
            return pair;
        }

        private static BreedingTest ParseBreedingTest(string[][] data)
        {
            if (data.Length < 2)
            {
                throw new InputException("Breeding test must list at least two tests");
            }
            BreedingTest test = new BreedingTest();
            for (int k = 0; k < data.Length; k++)
            {
                int offset = k == 0 ? 1 : 0;
                string[] item = data[k];
                if (item.Length < 2 + offset)
                {
                    throw new InputException("Testing pair must start with both parents");
                }
                TestPair pair = new TestPair(GetFlower(item[offset]), GetFlower(item[offset + 1]));
                if (item.Length < 3 + offset)
                {
                    throw new InputException("Testing pair must list one or more results");
                }
                if ((item.Length - offset) % 2 == 1)
                {
                    throw new InputException("Each testing result must specify a chance in percentage");
                }
                for (int j = offset + 2; j < item.Length; j += 2)
                {
                    pair.AddTestResult(GetFlowerColor(item[j]), int.Parse(item[j + 1]));
                }
                test.AddTestPair(pair);
            }
            return test;
        }

        public static string GetName()
        {
            return name;
        }

        public static FlowerColor GetFlowerColor(string id)
        {
            return flowerColors[id];
        }

        public static Flower GetFlower(string id)
        {
            if (!flowers.ContainsKey(id))
            {
                throw new Exception("Flower ID does not exist: " + id);
            }
            return flowers[id];
        }

        public static IEnumerable<BreedingPairSection> GetBreedingPairSections()
        {
            return breedingPairs;
        }
    }
}
