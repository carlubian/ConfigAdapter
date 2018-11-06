using ConfigAdapter;
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
            var iniCio = Config.From("TestFile.ini");
            var iniFin = iniCio.TransferTo("CopyFile.ini");
        }

        [TestMethod]
        public void XmlToXmlTransfer()
        {
            var iniCio = Config.From("TestFileOld.xml");
            var iniFin = iniCio.TransferTo("TestFileNew.xml");
        }

        [TestMethod]
        public void JsonToJsonTranfer()
        {
            var iniCio = Config.From("TestFileOld.hjson");
            var iniFin = iniCio.TransferTo("TestFileNew.hjson");
        }

        [TestMethod]
        public void UnknownFileTransfer()
        {
            var iniCio = Config.From("TestFile.ini");
            Action act = () => iniCio.TransferTo("Incorrecto");

            act.Should().Throw<UnknownFileFormatException>();
        }

        [TestMethod]
        public void InvalidFileTransfer()
        {
            var iniCio = Config.From("TestFile.ini");
            Action act = () => iniCio.TransferTo("Incorrecto.json");

            act.Should().Throw<InvalidFileFormatException>();

            act = () => iniCio.TransferTo("Incorrecto.txt");

            act.Should().Throw<InvalidFileFormatException>();
        }

        [TestMethod]
        [TestCategory("SkipWhenLiveUnitTesting")]
        public void Foobar()
        {
            Config.From("VolumenConveyor.hjson").TransferTo("VolumenConveyor.ini");
        }
    }
}
