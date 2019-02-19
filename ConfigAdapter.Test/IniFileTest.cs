using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using ConfigAdapter.Exceptions;
using ConfigAdapter.Ini;

namespace ConfigAdapterTest
{
    [TestClass]
    public class IniFileTest
    {
        [TestMethod]
        public void TestOrphanKey()
        {
            var config = IniConfig.From("TestFile.ini");

            config.Read("ClaveSinCategoria").Should().Be("0");
        }

        [TestMethod]
        public void TestCategorizedKey()
        {
            var config = IniConfig.From("TestFile.ini");

            config.Read("Categoria:SubClave").Should().Be("1");
        }

        [TestMethod]
        public void TestInvalidKey()
        {
            var config = IniConfig.From("TestFile.ini");
            Action act = () => config.Read("Esta:Clave:Es:Invalida");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestNonexistantKey()
        {
            var config = IniConfig.From("TestFile.ini");

            config.Read("NoExiste:EstaClave").Should().BeNull();
        }

        [TestMethod]
        public void TestWriteCategorizedKey()
        {
            var config = IniConfig.From("TestFile.ini");
            Action act = () => config.Write("Categoria:OtraClave", "2");

            act.Should().NotThrow();
            config.Read("Categoria:OtraClave").Should().Be("2");
        }

        [TestMethod]
        public void TestWriteOrphanKey()
        {
            var config = IniConfig.From("TestFile.ini");
            Action act = () => config.Write("OtraSinCategoria", "-1");

            act.Should().NotThrow();
            config.Read("OtraSinCategoria").Should().Be("-1");
        }

        [TestMethod]
        public void TestModifyKey()
        {
            var config = IniConfig.From(@"TestFile.ini");

            config.Write("Multi:Escritura", 100);
            config.Write("Multi:Escritura", 200);

            config.Read<int>("Multi:Escritura").Should().Be(200);
        }

        [TestMethod]
        public void TestReadType()
        {
            var config = IniConfig.From("TestFile.ini");

            // Números enteros
            Action act = () => config.Read<int>("Tipos:Entero");
            act.Should().NotThrow();
            config.Read<int>("Tipos:Entero").Should().Be(1);

            // Números largos
            act = () => config.Read<long>("Tipos:Largo");
            act.Should().NotThrow();
            config.Read<long>("Tipos:Largo").Should().Be(1);

            // Números dobles
            act = () => config.Read<double>("Tipos:Doble");
            act.Should().NotThrow();
            config.Read<double>("Tipos:Doble").Should().Be(1.5d);

            // Números flotantes
            act = () => config.Read<float>("Tipos:Flotante");
            act.Should().NotThrow();
            config.Read<float>("Tipos:Flotante").Should().Be(1.5f);

            // Caracteres
            act = () => config.Read<char>("Tipos:Caracter");
            act.Should().NotThrow();
            config.Read<char>("Tipos:Caracter").Should().Be('h');

            // Booleanos
            act = () => config.Read<bool>("Tipos:Booleano");
            act.Should().NotThrow();
            config.Read<bool>("Tipos:Booleano").Should().Be(false);
        }

        [TestMethod]
        public void TestWriteType()
        {
            var config = IniConfig.From("TestFile.ini");

            // Número entero
            Action act = () => config.Write("Escrito:Entero", 2);
            act.Should().NotThrow();
            config.Read<int>("Escrito:Entero").Should().Be(2);

            // Número largo
            act = () => config.Write("Escrito:Largo", 2L);
            act.Should().NotThrow();
            config.Read<long>("Escrito:Largo").Should().Be(2);

            // Número doble
            act = () => config.Write("Escrito:Doble", 2.5d);
            act.Should().NotThrow();
            config.Read<double>("Escrito:Doble").Should().Be(2.5d);

            // Número flotante
            act = () => config.Write("Escrito:Flotante", 2.5f);
            act.Should().NotThrow();
            config.Read<float>("Escrito:Flotante").Should().Be(2.5f);

            // Caracteres
            act = () => config.Write("Escrito:Caracter", 'x');
            act.Should().NotThrow();
            config.Read<char>("Escrito:Caracter").Should().Be('x');

            // Booleano
            act = () => config.Write("Escrito:Booleano", true);
            act.Should().NotThrow();
            config.Read<bool>("Escrito:Booleano").Should().Be(true);
        }

        [TestMethod]
        public void TestWriteInvalidKey()
        {
            var config = IniConfig.From(@"TestFile.ini");
            Action act = () => config.Write("Invalid:Key:Name", "Value");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestSettingsInGlobal()
        {
            var config = IniConfig.From(@"TestFile.ini");
            var elems = config.SettingsIn("");

            elems.Should().HaveCountGreaterOrEqualTo(1);
            elems.Should().ContainKey("ClaveSinCategoria");
            elems.Should().ContainValue("0");
            elems.Should().NotContainKeys("SubClave", "Entero");
        }

        [TestMethod]
        public void TestSettingsInSection()
        {
            var config = IniConfig.From(@"TestFile.ini");
            var elems = config.SettingsIn("Categoria");

            elems.Should().HaveCountGreaterOrEqualTo(1);
            elems.Should().ContainKey("SubClave");
            elems.Should().ContainValue("1");
            elems.Should().NotContainKeys("ClaveSinCategoria", "Entero");
        }
    }
}
