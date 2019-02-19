using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ConfigAdapter.HJson;
using FluentAssertions;
using ConfigAdapter.Exceptions;

namespace ConfigAdapterTest
{
    [TestClass]
    public class JsonFileTest
    {
        [TestMethod]
        public void TestReadKeys()
        {
            var config = HJsonConfig.From("TestFile.hjson");

            config.Read("Clave1").Should().Be("Valor1");
            config.Read("Clave2").Should().Be("Valor2");
            config.Read("Categoria:Clave3").Should().Be("Valor3");
        }

        [TestMethod]
        public void TestInvalidKey()
        {
            var config = HJsonConfig.From("TestFile.hjson");
            Action act = () => config.Read("Esta:Clave:Es:Invalida");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestNonexistantKey()
        {
            var config = HJsonConfig.From("TestFile.hjson");

            config.Read("NoExiste:EstaClave").Should().BeNull();
        }

        [TestMethod]
        public void TestWriteOrphanKey()
        {
            var config = HJsonConfig.From("TestFile.hjson");

            config.Write("Clave4", "Valor4");
            config.Write("Clave5", "Valor5", "Comentario de la clave 5");
        }

        [TestMethod]
        public void TestWriteCategorizedKey()
        {
            var config = HJsonConfig.From("TestFile.hjson");

            config.Write("Categoria:Clave6", "Valor6");
            config.Write("Categoria:Clave7", "Valor7", "Comentario de la clave 7");
        }

        [TestMethod]
        public void TestReadType()
        {
            var config = HJsonConfig.From("TestFile.hjson");

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
            var config = HJsonConfig.From("TestFile.hjson");

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
            var config = HJsonConfig.From(@"TestFile.hjson");

            config.Write("Escritura", 100, "Primera escritura");
            config.Write("Multi:Escritura", 200, "Segunda escritura");

            config.Read<int>("Multi:Escritura").Should().Be(200);
        }

        [TestMethod]
        public void TestWriteInvalidKey()
        {
            var config = HJsonConfig.From(@"TestFile.hjson");
            Action act = () => config.Write("Invalid:Key:Name", "Value");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestReadInvalidType()
        {
            var config = HJsonConfig.From(@"TestFile.hjson");
            Action act = () => config.Read<int>("Clave1");

            act.Should().Throw<ConversionImpossibleException>();
        }

        [TestMethod]
        public void TestSettingsInGlobal()
        {
            var config = HJsonConfig.From(@"TestFile.hjson");
            var elems = config.SettingsIn("");

            elems.Should().HaveCountGreaterOrEqualTo(2);
            elems.Should().ContainKeys("Clave1", "Clave2");
            elems.Should().ContainValues("Valor1", "Valor2");
            elems.Should().NotContainKeys("Clave3", "Largo", "Flotante");
        }

        [TestMethod]
        public void TestSettingsInSection()
        {
            var config = HJsonConfig.From(@"TestFile.hjson");
            var elems = config.SettingsIn("Tipos");

            elems.Should().HaveCountGreaterOrEqualTo(6);
            elems.Should().ContainKeys("Entero", "Doble", "Booleano");
            elems.Should().ContainValues("1", "1.5", "False");
            elems.Should().NotContainKeys("Clave1", "Clave3");
        }
    }
}
