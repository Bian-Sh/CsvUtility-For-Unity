using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using Assert = NUnit.Framework.Assert;
using zFramework.Extension;

namespace Tests
{
    public class CsvUtilityTests
    {
        private string testCsvPath;

        [SetUp]
        public void Setup()
        {
            testCsvPath = Path.Combine(Path.GetTempPath(), "test.csv");
            File.WriteAllText(testCsvPath, "index,size_x,size_y,width,height\n0,2.5,2.9,1920,1080\n1,3.5,3.9,1921,1081");
        }

        [Test]
        public void TestReadAll()
        {
            List<DisplayConfiguration> result = CsvUtility.Read<DisplayConfiguration>(testCsvPath);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(0, result[0].index);
            Assert.AreEqual(2.5f, result[0].size_x);
            Assert.AreEqual(2.9f, result[0].size_y);
            Assert.AreEqual(1920, result[0].width);
            Assert.AreEqual(1080, result[0].height);
            Assert.AreEqual(1, result[1].index);
            Assert.AreEqual(3.5f, result[1].size_x);
            Assert.AreEqual(3.9f, result[1].size_y);
            Assert.AreEqual(1921, result[1].width);
            Assert.AreEqual(1081, result[1].height);
        }
        [Test]
        public void TestWrite()
        {
            var data = new List<DisplayConfiguration>
            {
                new DisplayConfiguration { index = 2, size_x = 4.5f, size_y = 4.9f, width = 1922, height = 1082 },
                new DisplayConfiguration { index = 3, size_x = 5.5f, size_y = 5.9f, width = 1923, height = 1083 }
            };
            CsvUtility.Write(data, testCsvPath);

            var lines = File.ReadAllLines(testCsvPath);
            Assert.AreEqual("index,size_x,size_y,width,height", lines[0]);
            Assert.AreEqual("2,4.5,4.9,1922,1082", lines[1]);
            Assert.AreEqual("3,5.5,5.9,1923,1083", lines[2]);
        }

        [Test]
        public void TestReadWithFilter()
        {
            DisplayConfiguration result = CsvUtility.Read<DisplayConfiguration>(testCsvPath, "index", 1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.index);
            Assert.AreEqual(3.5f, result.size_x);
            Assert.AreEqual(3.9f, result.size_y);
            Assert.AreEqual(1921, result.width);
            Assert.AreEqual(1081, result.height);
        }

        [Test]
        public void TestFromCsvOverwrite()
        {
            var target = new DisplayConfiguration() { index = 1 };
            CsvUtility.FromCsvOverwrite(testCsvPath, target, "index");
            Assert.AreEqual(1, target.index);
            Assert.AreEqual(3.5f, target.size_x);
            Assert.AreEqual(3.9f, target.size_y);
            Assert.AreEqual(1921, target.width);
            Assert.AreEqual(1081, target.height);
        }

        [Test]
        public void TestWriteUpdate()
        {
            var target = new DisplayConfiguration { index = 1, size_x = 4.5f, size_y = 4.9f, width = 1925, height = 1085 };

            CsvUtility.Write(target, testCsvPath, "index", KeyinType.Update);

            var lines = File.ReadAllLines(testCsvPath);
            Assert.AreEqual("index,size_x,size_y,width,height", lines[0]);
            Assert.AreEqual("0,2.5,2.9,1920,1080", lines[1]);
            Assert.AreEqual("1,4.5,4.9,1925,1085", lines[2]);
        }

        [Test]
        public void TestWriteAppend()
        {
            var target = new DisplayConfiguration { index = 4, size_x = 1.1f, size_y = 6.6f, width = 1928, height = 1088 };
            CsvUtility.Write(target, testCsvPath, "index", KeyinType.Append);

            var lines = File.ReadAllLines(testCsvPath);
            Assert.AreEqual("index,size_x,size_y,width,height", lines[0]);
            Assert.AreEqual("0,2.5,2.9,1920,1080", lines[1]);
            Assert.AreEqual("1,3.5,3.9,1921,1081", lines[2]);
            Assert.AreEqual("4,1.1,6.6,1928,1088", lines[3]);
        }

        [Test]
        public void CheckCsvIgnore()
        {
            var target = new DisplayConfiguration {name="test", index = 4, size_x = 1.1f, size_y = 6.6f, width = 1928, height = 1088 };
            CsvUtility.Write(target, testCsvPath, "index", KeyinType.Append);
            var lines = File.ReadAllLines(testCsvPath);
            Assert.AreEqual("index,size_x,size_y,width,height", lines[0]);
            Assert.AreEqual("4,1.1,6.6,1928,1088", lines[3]);
        }


        [TearDown]
        public void TearDown()
        {
            File.Delete(testCsvPath);
        }
    }

    public class DisplayConfiguration
    {
        [CsvIgnore]
        public string name;
        public int index;
        public float size_x;
        public float size_y;
        public int width;
        public int height;
        public override string ToString()
        {
            return @$"name = {name}
index = {index}
size_x = {size_x}
size_y = {size_y}
width = {width}
height = {height}";
        }
    }
}
