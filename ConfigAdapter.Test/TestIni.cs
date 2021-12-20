using ConfigAdapter.Core.Model;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigAdapter.Test;

[TestClass]
public class TestIni
{
    [TestMethod]
    public void TestParseFile()
    {
        IniConfigurationProvider.RegisterProvider();
        var tree = Configuration.From("TestFile.ini");

        var globalSetting1 = tree.Retrieve("GlobalSetting1");
        globalSetting1.Value.Should().BeOfType<ConfigAdapterValue.StringValue>();
        string value = globalSetting1.Value;
        value.Should().Be("Global Value 1");
        globalSetting1.Comment.Should().Be("Global comment 1");
    }
}
