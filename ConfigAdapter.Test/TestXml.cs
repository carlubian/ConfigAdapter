using ConfigAdapter.Core.Exceptions;
using ConfigAdapter.Core.Model;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConfigAdapter.Test
{
    [TestClass]
    public class TestXml
    {
        [TestMethod]
        public void TestParseFile()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            var globalSetting1 = tree.Retrieve("GlobalSetting1");
            globalSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
            string value = globalSetting1.Value;
            value.Should().Be("Global Value 1");
            globalSetting1.Comment.Should().Be("Global comment 1");
        }

        [TestMethod]
        public void TestParseSection()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            tree.Enumerate("Section 1").Should().NotBeNull()
                .And.HaveCount(1);

            tree.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
                .And.HaveCount(2);

            var localSetting1 = tree.Retrieve("Section 1:LocalSetting1");
            localSetting1.Should().NotBeNull();

            localSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
            string value = localSetting1.Value;
            value.Should().Be("Local Value 1");
            localSetting1.Comment.Should().Be("Local comment 1");

            var localSetting2 = tree.Retrieve("Section 1:Section 1.1:LocalSetting1.2");
            localSetting2.Should().NotBeNull();

            localSetting2.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
            value = localSetting2.Value;
            value.Should().Be("Local Value 1.2");
            localSetting2.Comment.Should().Be("Local comment 1.2");
        }

        [TestMethod]
        public void TestParseArray()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            var localSetting = tree.Retrieve("Section 1:Section 1.3:LocalSetting1.3");
            localSetting.Should().NotBeNull();

            List<string> values = localSetting.Value;
            values.Should().NotBeNullOrEmpty()
                .And.HaveCount(4);
        }

        [TestMethod]
        public void TestExtensionMethods()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            // String retrieved as string
            Action act = () => tree.GetValue("Section 1:LocalSetting1");
            act.Should().NotThrow();

            tree.GetValue("Section 1:LocalSetting1").Should().Be("Local Value 1");

            // String retrieved as array
            act = () => tree.GetValues("Section 1:LocalSetting1");
            act.Should().Throw<ValueMismatchException>();

            // Array retrieved as array
            act = () => tree.GetValues("Section 1:Section 1.3:LocalSetting1.3");
            act.Should().NotThrow();

            tree.GetValues("Section 1:Section 1.3:LocalSetting1.3").Should()
                .NotBeNullOrEmpty().And.HaveCount(4);

            // Array retrieved as string
            act = () => tree.GetValues("Section 1:LocalSetting1");
            act.Should().Throw<ValueMismatchException>();
        }

        [TestMethod]
        public void TestNonexistantKeys()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            Action act = () => tree.Enumerate("NotExists:Also missing");
            act.Should().NotThrow();

            tree.Enumerate("NotExists:Also missing").Should()
                .NotBeNull().And.BeEmpty();

            act = () => tree.Retrieve("Missing section:Missing setting");
            act.Should().NotThrow();

            tree.Retrieve("Missing section:Missing setting").Should()
                .BeNull();
        }

        [TestMethod]
        public void TestExceptionEvents()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            var localSetting = tree.Retrieve("Section 1:LocalSetting1");
            Action act = () => ((List<string>)localSetting.Value).Count();

            act.Should().Throw<ValueMismatchException>();
        }

        [TestMethod]
        public void TestWritingFile()
        {
            // Write to new file
            File.Copy("TestFile.xml", "TestFile2.xml", true);
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile2.xml");

            tree.Store("Global Setting 3", "Global Value 3");
            tree.Store("Section 1:LocalSetting2", "Local value foo", "Local comment foo");
            tree.Store("Section 1:Section 1.4:Local setting 22", "Local value 327");

            Action act = () => tree.Persist();
            act.Should().NotThrow();

            // Check created file
            tree = Configuration.From("TestFile2.xml");

            tree.Enumerate().Should().NotBeNull()
                .And.HaveCount(3);

            var setting = tree.Retrieve("Global Setting 3");
            setting.Should().NotBeNull();

            setting.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
            string value = setting.Value;
            value.Should().Be("Global Value 3");
            setting.Comment.Should().BeNull();

            tree.Enumerate("Section 1").Should().NotBeNull()
                .And.HaveCount(2);

            setting = tree.Retrieve("Section 1:LocalSetting2");
            setting.Should().NotBeNull();

            setting.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
            value = setting.Value;
            value.Should().Be("Local value foo");
            setting.Comment.Should().Be("Local comment foo");

            tree.Enumerate("Section 1:Section 1.4").Should().NotBeNull()
                .And.HaveCount(1);

            setting = tree.Retrieve("Section 1:Section 1.4:Local setting 22");
            setting.Should().NotBeNull();

            setting.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
            value = setting.Value;
            value.Should().Be("Local value 327");
            setting.Comment.Should().BeNull();

        }

        [TestMethod]
        public void TestDeleteStuff()
        {
            XmlConfigurationProvider.RegisterProvider();
            var tree = Configuration.From("TestFile.xml");

            // Delete a global setting
            tree.Store("ToDelete Global", "Fake value");
            tree.Delete("ToDelete Global");

            tree.Enumerate().Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            tree.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
                .And.HaveCount(2);

            // Delete a 1-deep setting
            tree.Store("Test section:Setting 1", "Value");
            tree.Store("Test section:Setting 2", "Value");
            tree.Delete("Test section:Setting 1");

            tree.Enumerate("Test section").Should().NotBeNullOrEmpty()
                .And.HaveCount(1);
            tree.Enumerate().Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            tree.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
                .And.HaveCount(2);

            // Delete a 1-deep section
            tree.Store("Test section:Setting 3", "Value");
            tree.Delete("Test section");

            tree.Enumerate("Test section").Should().BeEmpty();
            tree.Enumerate().Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            tree.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
                .And.HaveCount(2);

            // Delete a 2-deep setting
            tree.Store("Test section:Setting 1", "Value");
            tree.Store("Test section:Subsection:Setting 2", "Value");
            tree.Store("Test section:Subsection:Setting 3", "Value");
            tree.Delete("Test section:Subsection:Setting 2");

            tree.Enumerate().Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            tree.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
                .And.HaveCount(2);
            tree.Enumerate("Test section:Subsection").Should().NotBeNull()
                .And.HaveCount(1);
            tree.Enumerate("Test section").Should().NotBeNull()
                .And.HaveCount(1);

            // Delete a 2-deep section
            tree.Store("Test section 2:Setting 1", "Value");
            tree.Store("Test section 2:Subsection:Setting 2", "Value");
            tree.Store("Test section 2:Subsection:Setting 3", "Value");
            tree.Delete("Test section 2:Subsection");

            tree.Enumerate().Should().NotBeNullOrEmpty()
                .And.HaveCount(2);
            tree.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
                .And.HaveCount(2);
            tree.Enumerate("Test section 2:Subsection").Should().BeEmpty();
            tree.Enumerate("Test section 2").Should().NotBeNull()
                .And.HaveCount(1);
        }
    }
}
