using ConfigAdapter.Core.Model;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ConfigAdapter.Test;

[TestClass]
public class TestMigrations
{
    [TestMethod]
    public void MigrateXmlToYaml()
    {
        XmlConfigurationProvider.RegisterProvider();
        YamlConfigurationProvider.RegisterProvider();

        var tree = Configuration.From("TestFile.xml");
        var migration = Configuration.Migrate(tree, "Migrated.yaml");

        // Check for existing settings and sections
        var globalSetting1 = migration.Retrieve("GlobalSetting1");
        globalSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        string value = globalSetting1.Value;
        value.Should().Be("Global Value 1");
        globalSetting1.Comment.Should().Be("Global comment 1");

        migration.Enumerate("Section 1").Should().NotBeNull()
                .And.HaveCount(1);

        migration.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
            .And.HaveCount(2);

        var localSetting1 = migration.Retrieve("Section 1:LocalSetting1");
        localSetting1.Should().NotBeNull();

        localSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        value = localSetting1.Value;
        value.Should().Be("Local Value 1");
        localSetting1.Comment.Should().Be("Local comment 1");

        var localSetting2 = migration.Retrieve("Section 1:Section 1.1:LocalSetting1.2");
        localSetting2.Should().NotBeNull();

        localSetting2.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        value = localSetting2.Value;
        value.Should().Be("Local Value 1.2");
        localSetting2.Comment.Should().Be("Local comment 1.2");

        var localSetting = migration.Retrieve("Section 1:Section 1.3:LocalSetting1.3");
        localSetting.Should().NotBeNull();

        List<string> values = localSetting.Value;
        values.Should().NotBeNullOrEmpty()
            .And.HaveCount(4);
    }

    [TestMethod]
    public void MigrateJsonToXml()
    {
        XmlConfigurationProvider.RegisterProvider();
        JsonConfigurationProvider.RegisterProvider();

        var tree = Configuration.From("TestFile.json");
        var migration = Configuration.Migrate(tree, "Migrated.xml");

        // Check for existing settings and sections
        var globalSetting1 = migration.Retrieve("GlobalSetting1");
        globalSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        string value = globalSetting1.Value;
        value.Should().Be("Global Value 1");
        globalSetting1.Comment.Should().Be("Global comment 1");

        migration.Enumerate("Section 1").Should().NotBeNull()
                .And.HaveCount(1);

        migration.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
            .And.HaveCount(2);

        var localSetting1 = migration.Retrieve("Section 1:LocalSetting1");
        localSetting1.Should().NotBeNull();

        localSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        value = localSetting1.Value;
        value.Should().Be("Local Value 1");
        localSetting1.Comment.Should().Be("Local comment 1");

        var localSetting2 = migration.Retrieve("Section 1:Section 1.1:LocalSetting 1.2");
        localSetting2.Should().NotBeNull();

        localSetting2.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        value = localSetting2.Value;
        value.Should().Be("Local Value 1.2");
        localSetting2.Comment.Should().Be("Local comment 1.2");

        var localSetting = migration.Retrieve("Section 1:Section 1.3:LocalSetting1.3");
        localSetting.Should().NotBeNull();

        List<string> values = localSetting.Value;
        values.Should().NotBeNullOrEmpty()
            .And.HaveCount(4);
    }

    [TestMethod]
    public void MigrateYamlToJson()
    {
        JsonConfigurationProvider.RegisterProvider();
        YamlConfigurationProvider.RegisterProvider();

        var tree = Configuration.From("TestFile.yaml");
        var migration = Configuration.Migrate(tree, "Migrated.json");

        // Check for existing settings and sections
        var globalSetting1 = migration.Retrieve("GlobalSetting1");
        globalSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        string value = globalSetting1.Value;
        value.Should().Be("Global Value 1");
        globalSetting1.Comment.Should().Be("Global comment 1");

        migration.Enumerate("Section 1").Should().NotBeNull()
                .And.HaveCount(1);

        migration.Enumerate("Section 1:Section 1.1").Should().NotBeNull()
            .And.HaveCount(2);

        var localSetting1 = migration.Retrieve("Section 1:LocalSetting1");
        localSetting1.Should().NotBeNull();

        localSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        value = localSetting1.Value;
        value.Should().Be("Local Value 1");
        localSetting1.Comment.Should().Be("Local comment 1");

        var localSetting2 = migration.Retrieve("Section 1:Section 1.1:LocalSetting 1.2");
        localSetting2.Should().NotBeNull();

        localSetting2.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        value = localSetting2.Value;
        value.Should().Be("Local Value 1.2");
        localSetting2.Comment.Should().Be("Local comment 1.2");

        var localSetting = migration.Retrieve("Section 1:Section 1.3:LocalSetting 1.3");
        localSetting.Should().NotBeNull();

        List<string> values = localSetting.Value;
        values.Should().NotBeNullOrEmpty()
            .And.HaveCount(4);
    }
}
