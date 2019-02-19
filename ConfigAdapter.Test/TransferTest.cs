using ConfigAdapter.Xml;
using ConfigAdapter.Ini;
using ConfigAdapter.HJson;
using ConfigAdapter.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfigAdapterTest
{
    [TestClass]
    public class TransferTest
    {
        [TestMethod]
        public void IniToIniTransfer()
        {
            var iniCio = IniConfig.From("TestFile.ini");
            var iniFin = iniCio.TransferTo(IniConfig.From("CopyFile.ini"));
        }

        [TestMethod]
        public void XmlToXmlTransfer()
        {
            var iniCio = XmlConfig.From("TestFileOld.xml");
            var iniFin = iniCio.TransferTo(XmlConfig.From("TestFileNew.xml"));
        }

        [TestMethod]
        public void JsonToJsonTranfer()
        {
            var iniCio = HJsonConfig.From("TestFileOld.hjson");
            var iniFin = iniCio.TransferTo(HJsonConfig.From("TestFileNew.hjson"));
        }

        [TestMethod]
        public void UnknownFileTransfer()
        {
            var iniCio = IniConfig.From("TestFile.ini");
            Action act = () => iniCio.TransferTo(XmlConfig.From("Incorrecto"));

            act.Should().Throw<InvalidFileFormatException>();
        }

        [TestMethod]
        public void InvalidFileTransfer()
        {
            var iniCio = IniConfig.From("TestFile.ini");
            Action act = () => iniCio.TransferTo(HJsonConfig.From("Incorrecto.json"));

            act.Should().Throw<InvalidFileFormatException>();

            act = () => iniCio.TransferTo(XmlConfig.From("Incorrecto.txt"));

            act.Should().Throw<InvalidFileFormatException>();
        }
    }
}
