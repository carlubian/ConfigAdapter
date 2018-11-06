using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConfigAdapter;
using System;
using FluentAssertions;
using ConfigAdapter.Exceptions;

namespace ConfigAdapterTest
{
    [TestClass]
    public class FileFormatTest
    {
        [TestMethod]
        public void TestUnknownFormat()
        {
            Action act = () => Config.From(@"path\to\invalid\file");

            act.Should().Throw<UnknownFileFormatException>();
        }

        [TestMethod]
        public void TestInvalidFormat()
        {
            Action act = () => Config.From(@"path\to\invalid\format.pdf");

            act.Should().Throw<InvalidFileFormatException>();
        }

        [TestMethod]
        public void TestValidFile()
        {
            Action act = () => Config.From(@"TestFile.ini");

            act.Should().NotThrow();
        }

        [TestMethod]
        public void TestNewIniFile()
        {
            Action act = () => Config.From(@"NewFile.ini");

            act.Should().NotThrow();

            System.IO.File.Delete("NewFile.ini");
        }

        [TestMethod]
        public void TestNewXmlFile()
        {
            Action act = () => Config.From(@"NewFile.xml");

            act.Should().NotThrow();

            System.IO.File.Delete("NewFile.xml");
        }

        [TestMethod]
        public void TestNewJsonFile()
        {
            Action act = () => Config.From(@"NewFile.hjson");

            act.Should().NotThrow();

            System.IO.File.Delete("NewFile.hjson");
        }

        [TestMethod]
        public void TestNewDottedFile()
        {
            Action act = () => Config.From(@"New.File.xml");

            act.Should().NotThrow();

            System.IO.File.Delete("New.File.xml");
        }

        [TestMethod]
        public void TestInvalidJsonFile()
        {
            Action act = () => Config.From("TestFile.json");

            act.Should().Throw<InvalidFileFormatException>();
        }
    }
}
