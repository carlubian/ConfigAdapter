using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConfigAdapter.Xml;
using System;
using FluentAssertions;
using ConfigAdapter.Exceptions;
using ConfigAdapter.Ini;
using ConfigAdapter.HJson;

namespace ConfigAdapterTest
{
    [TestClass]
    public class FileFormatTest
    {
        [TestMethod]
        public void TestUnknownFormat()
        {
            Action act = () => XmlConfig.From(@"path\to\invalid\file");

            act.Should().Throw<InvalidFileFormatException>();
        }

        [TestMethod]
        public void TestInvalidFormat()
        {
            Action act = () => XmlConfig.From(@"path\to\invalid\format.pdf");

            act.Should().Throw<InvalidFileFormatException>();
        }

        [TestMethod]
        public void TestValidFile()
        {
            Action act = () => IniConfig.From(@"TestFile.ini");

            act.Should().NotThrow();
        }

        [TestMethod]
        public void TestNewIniFile()
        {
            Action act = () => IniConfig.From(@"NewFile.ini");

            act.Should().NotThrow();

            System.IO.File.Delete("NewFile.ini");
        }

        [TestMethod]
        public void TestNewXmlFile()
        {
            Action act = () => XmlConfig.From(@"NewFile.xml");

            act.Should().NotThrow();

            System.IO.File.Delete("NewFile.xml");
        }

        [TestMethod]
        public void TestNewJsonFile()
        {
            Action act = () => HJsonConfig.From(@"NewFile.hjson");

            act.Should().NotThrow();

            System.IO.File.Delete("NewFile.hjson");
        }

        [TestMethod]
        public void TestNewDottedFile()
        {
            Action act = () => XmlConfig.From(@"New.File.xml");

            act.Should().NotThrow();

            System.IO.File.Delete("New.File.xml");
        }

        [TestMethod]
        public void TestInvalidJsonFile()
        {
            Action act = () => XmlConfig.From("TestFile.json");

            act.Should().Throw<InvalidFileFormatException>();
        }
    }
}
