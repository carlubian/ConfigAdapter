using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using ConfigAdapter.Xml;
using ConfigAdapter.Exceptions;

namespace ConfigAdapterTest
{
    [TestClass]
    public class XmlFileTest
    {
        [TestMethod]
        public void TestOrphanKey()
        {
            var config = XmlConfig.From("TestFile.xml");

            config.Read("ClaveSinCategoria").Should().Be("0");
        }

        [TestMethod]
        public void TestCategorizedKey()
        {
            var config = XmlConfig.From("TestFile.xml");

            config.Read("Categoria:SubClave").Should().Be("1");
        }

        [TestMethod]
        public void TestInvalidKey()
        {
            var config = XmlConfig.From("TestFile.xml");
            Action act = () => config.Read("Esta:Clave:Es:Invalida");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestNonexistantKey()
        {
            var config = XmlConfig.From("TestFile.xml");

            config.Read("NoExiste:EstaClave").Should().BeNull();
        }

        [TestMethod]
        public void TestWriteCategorizedKey()
        {
            var config = XmlConfig.From("TestFile.xml");
            Action act = () => config.Write("Categoria:OtraClave", "2");

            act.Should().NotThrow();
            config.Read("Categoria:OtraClave").Should().Be("2");
        }

        [TestMethod]
        public void TestWriteOrphanKey()
        {
            var config = XmlConfig.From("TestFile.xml");
            Action act = () => config.Write("OtraSinCategoria", "-1");

            act.Should().NotThrow();
            config.Read("OtraSinCategoria").Should().Be("-1");
        }

        [TestMethod]
        public void TestWriteCommentedKey()
        {
            var config = XmlConfig.From("TestFile.xml");
            Action act = () => config.Write("Categoria:Cosa Compleja", "Foo",
                "Este ajuste es muy complejo");

            act.Should().NotThrow();
            config.Read("Categoria:Cosa Compleja").Should().Be("Foo");
        }

        [TestMethod]
        public void TestReadType()
        {
            var config = XmlConfig.From("TestFile.xml");

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
            var config = XmlConfig.From("TestFile.xml");

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
        public void TestWrongFormat()
        {
            Action act = () => XmlConfig.From("WrongFile.xml");
            act.Should().Throw<IncompatibleXmlFormatException>();
        }

        [TestMethod]
        public void TestModifyKey()
        {
            var config = XmlConfig.From(@"TestFile.xml");

            config.Write("Multi:Escritura", 100, "Primera escritura");
            config.Write("Multi:Escritura", 200, "Segunda escritura");

            config.Read<int>("Multi:Escritura").Should().Be(200);
        }

        [TestMethod]
        public void TestNewPath()
        {
            Action act = () => XmlConfig.From(@"Settings\TestFile.xml");
            act.Should().NotThrow();

            var config = XmlConfig.From(@"Settings\TestFile.xml");
            config.Write("Ajuste:Prueba", "Valor");

            config.Read("Ajuste:Prueba").Should().Be("Valor");

            System.IO.Directory.Delete("Settings", true);
        }

        [TestMethod]
        public void TestUpdateComment()
        {
            var config = XmlConfig.From(@"TestFile.xml");
            config.Write("Categoria:Comentada", "False", "Nuevo comentario");

            config.Write("Comentarios:Primera", "Foo", "Comentario 1");
            config.Write("Comentarios:Primera", "Bar", "Comentario 2");
        }

        [TestMethod]
        public void TestNewGlobalCommented()
        {
            var config = XmlConfig.From(@"TestFile.xml");
            config.Write("GlobalKey", "GlobalValue", "Global Comment");

            config.Read("GlobalKey").Should().Be("GlobalValue");
        }

        [TestMethod]
        public void TestUpdateGlobal()
        {
            var config = XmlConfig.From(@"TestFile.xml");
            config.Write("GlobalKey", "NewGlobalValue", "Global Comment");

            config.Read("GlobalKey").Should().Be("NewGlobalValue");
        }

        [TestMethod]
        public void TestWriteInvalidKey()
        {
            var config = XmlConfig.From(@"TestFile.xml");
            Action act = () => config.Write("Invalid:Key:Name", "Value");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestSettingsInGlobal()
        {
            var config = XmlConfig.From(@"TestFile.xml");
            IDictionary<string, string> settings = null;

            Action act = () => settings = config.SettingsIn("");
            act.Should().NotThrow();

            settings.Should().HaveCountGreaterOrEqualTo(1);
            settings.Should().ContainKey("ClaveSinCategoria");
            settings.Should().ContainValue("0");
            settings.Should().NotContainKeys("SubClave", "Comentada", "Doble");
        }

        [TestMethod]
        public void TestSettingsInSection()
        {
            var config = XmlConfig.From(@"TestFile.xml");
            IDictionary<string, string> settings = null;

            Action act = () => settings = config.SettingsIn("Categoria");
            act.Should().NotThrow();

            settings.Should().HaveCountGreaterOrEqualTo(2);
            settings.Should().ContainKeys("SubClave", "Comentada");
            settings.Should().ContainValue("1");
            settings.Should().NotContainKeys("ClaveSinCategoria", "Doble");
        }

        [TestMethod]
        public void TestDeleteKey()
        {
            var config = XmlConfig.From(@"TestFile.xml");

            // Global key
            config.Write("GlobalKeyToDelete", "ValueToDelete");
            config.Read("GlobalKeyToDelete").Should().Be("ValueToDelete");

            Action act = () => config.DeleteKey("GlobalKeyToDelete");
            act.Should().NotThrow();

            config.Read("GlobalKeyToDelete").Should().Be(null);

            // Local key
            config.Write("Local:KeyToDelete", "ValueToDelete");
            config.Write("Local:KeyToKeep", "ValueToKeep");
            config.Read("Local:KeyToDelete").Should().Be("ValueToDelete");
            config.Read("Local:KeyToKeep").Should().Be("ValueToKeep");

            act = () => config.DeleteKey("Local:KeyToDelete");
            act.Should().NotThrow();

            config.Read("Local:KeyToDelete").Should().Be(null);
            config.Read("Local:KeyToKeep").Should().Be("ValueToKeep");
        }

        [TestMethod]
        public void TestDeleteSection()
        {
            var config = XmlConfig.From(@"TestFile.xml");

            config.Write("SectionToDelete:Key1", "Foo");
            config.Write("SectionToDelete:Key2", "Bar");

            Action act = () => config.DeleteSection("SectionToDelete");
            act.Should().NotThrow();

            config.Read("SectionToDelete:Key1").Should().Be(null);
            config.Read("SectionToDelete:Key2").Should().Be(null);
        }
    }
}
