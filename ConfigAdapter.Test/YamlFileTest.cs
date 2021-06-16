using ConfigAdapter.Exceptions;
using ConfigAdapter.Yaml;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ConfigAdapterTest
{
    [TestClass]
    public class YamlFileTest
    {
        [TestMethod]
        public void TestOrphanKey()
        {
            var config = YamlConfig.From("TestFile.yaml");

            config.Read("GlobalSetting1").Should().Be("Global Value 1");
        }

        [TestMethod]
        public void TestCategorizedKey()
        {
            var config = YamlConfig.From("TestFile.yaml");

            config.Read("Section1:LocalSetting1").Should().Be("Local Value 1");
        }

        [TestMethod]
        public void TestInvalidKey()
        {
            var config = YamlConfig.From("TestFile.yaml");
            Action act = () => config.Read("Esta:Clave:Es:Invalida");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestNonexistantKey()
        {
            var config = YamlConfig.From("TestFile.yaml");

            config.Read("NoExiste:EstaClave").Should().Be("");
        }

        [TestMethod]
        public void TestWriteOrphanKey()
        {
            var config = YamlConfig.From("TestFile.yaml");
            Action act = () => config.Write("GlobalSetting3", "Global Value 3");

            act.Should().NotThrow();
            config.Read("GlobalSetting3").Should().Be("Global Value 3");
        }

        [TestMethod]
        public void TestWriteCategorizedKey()
        {
            var config = YamlConfig.From("TestFile.yaml");
            Action act = () => config.Write("Section3:LocalValue1", "Local Value 4");

            act.Should().NotThrow();
            config.Read("Section3:LocalValue1").Should().Be("Local Value 4");
        }

        [TestMethod]
        public void TestWriteType()
        {
            var config = YamlConfig.From("TestFile.yaml");

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
        public void TestModifyKey()
        {
            var config = YamlConfig.From(@"TestFile.yaml");

            config.Write("Multi:Escritura", 100, "Primera escritura");
            config.Write("Multi:Escritura", 200, "Segunda escritura");

            config.Read<int>("Multi:Escritura").Should().Be(200);
        }

        [TestMethod]
        public void TestUpdateGlobal()
        {
            var config = YamlConfig.From(@"TestFile.yaml");
            config.Write("GlobalKey", "NewGlobalValue", "Global Comment");

            config.Read("GlobalKey").Should().Be("NewGlobalValue");
        }

        [TestMethod]
        public void TestWriteInvalidKey()
        {
            var config = YamlConfig.From(@"TestFile.yaml");
            Action act = () => config.Write("Invalid:Key:Name", "Value");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestSettingsInGlobal()
        {
            var config = YamlConfig.From(@"TestFile.yaml");
            IDictionary<string, string> settings = null;

            Action act = () => settings = config.SettingsIn("");
            act.Should().NotThrow();

            settings.Should().HaveCountGreaterOrEqualTo(1);
            settings.Should().ContainKey("GlobalSetting1");
            settings.Should().ContainValue("250");
            settings.Should().NotContainKeys("LocalSetting1", "LocalSetting2", "Section1");
        }

        [TestMethod]
        public void TestSettingsInSection()
        {
            var config = YamlConfig.From(@"TestFile.yaml");
            IDictionary<string, string> settings = null;

            Action act = () => settings = config.SettingsIn("Section2");
            act.Should().NotThrow();

            settings.Should().HaveCountGreaterOrEqualTo(2);
            settings.Should().ContainKeys("LocalSetting1", "LocalSetting2");
            settings.Should().ContainValue("Local Value 3");
            settings.Should().NotContainKeys("GlobalSetting1", "GlobalSetting2");
        }

        [TestMethod]
        public void TestDeleteKey()
        {
            var config = YamlConfig.From(@"TestFile.yaml");

            // Global key
            config.Write("GlobalKeyToDelete", "ValueToDelete");
            config.Read("GlobalKeyToDelete").Should().Be("ValueToDelete");

            Action act = () => config.DeleteKey("GlobalKeyToDelete");
            act.Should().NotThrow();

            config.Read("GlobalKeyToDelete").Should().Be("");

            // Local key
            config.Write("Local:KeyToDelete", "ValueToDelete");
            config.Write("Local:KeyToKeep", "ValueToKeep");
            config.Read("Local:KeyToDelete").Should().Be("ValueToDelete");
            config.Read("Local:KeyToKeep").Should().Be("ValueToKeep");

            act = () => config.DeleteKey("Local:KeyToDelete");
            act.Should().NotThrow();

            config.Read("Local:KeyToDelete").Should().Be("");
            config.Read("Local:KeyToKeep").Should().Be("ValueToKeep");
        }

        [TestMethod]
        public void TestDeleteSection()
        {
            var config = YamlConfig.From(@"TestFile.yaml");

            config.Write("SectionToDelete:Key1", "Foo");
            config.Write("SectionToDelete:Key2", "Bar");

            Action act = () => config.DeleteSection("SectionToDelete");
            act.Should().NotThrow();

            config.Read("SectionToDelete:Key1").Should().Be("");
            config.Read("SectionToDelete:Key2").Should().Be("");
        }
    }
}
