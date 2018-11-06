using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ConfigAdapter;
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
            var config = Config.From("TestFile.hjson");

            config.Read("Clave1").Should().Be("Valor1");
            config.Read("Clave2").Should().Be("Valor2");
            config.Read("Categoria:Clave3").Should().Be("Valor3");
        }

        [TestMethod]
        public void TestInvalidKey()
        {
            var config = Config.From("TestFile.hjson");
            Action act = () => config.Read("Esta:Clave:Es:Invalida");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestNonexistantKey()
        {
            var config = Config.From("TestFile.xml");

            config.Read("NoExiste:EstaClave").Should().BeNull();
        }

        [TestMethod]
        public void TestWriteOrphanKey()
        {
            var config = Config.From("TestFile.hjson");

            config.Write("Clave4", "Valor4");
            config.Write("Clave5", "Valor5", "Comentario de la clave 5");
        }

        [TestMethod]
        public void TestWriteCategorizedKey()
        {
            var config = Config.From("TestFile.hjson");

            config.Write("Categoria:Clave6", "Valor6");
            config.Write("Categoria:Clave7", "Valor7", "Comentario de la clave 7");
        }

        [TestMethod]
        public void TestReadType()
        {
            var config = Config.From("TestFile.hjson");

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
            var config = Config.From("TestFile.hjson");

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
            var config = Config.From(@"TestFile.hjson");

            config.Write("Escritura", 100, "Primera escritura");
            config.Write("Multi:Escritura", 200, "Segunda escritura");

            config.Read<int>("Multi:Escritura").Should().Be(200);
        }

        [TestMethod]
        public void TestWriteInvalidKey()
        {
            var config = Config.From(@"TestFile.hjson");
            Action act = () => config.Write("Invalid:Key:Name", "Value");

            act.Should().Throw<InvalidKeyFormatException>();
        }

        [TestMethod]
        public void TestReadInvalidType()
        {
            var config = Config.From(@"TestFile.hjson");
            Action act = () => config.Read<int>("Clave1");

            act.Should().Throw<ConversionImpossibleException>();
        }
    }
}
