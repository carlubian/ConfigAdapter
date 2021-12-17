using ConfigAdapter.Core.Model;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ConfigAdapter.Test
{
    [TestClass]
    public class TestModel
    {
        [TestMethod]
        public void TestCreateAndStore()
        {
            var setting1 = new ConfigAdapterSetting()
            {
                Name = "New setting",
                Value = "New Value"
            };

            var mock = new MockConfigurationProvider();

            Action store = () => mock.Store(setting1);
            store.Should().NotThrow();

            store = () => mock.Store("Key 2", "Value 2", "Comment 2");
            store.Should().NotThrow();
        }

        [TestMethod]
        public void TestRetrieve()
        {
            var mock = new MockConfigurationProvider();

            Action retrieve = () => mock.Retrieve("Setting key");
            retrieve.Should().NotThrow();

            var setting = mock.Retrieve("Key 1");
            setting.Should().NotBeNull();

            var (key, value, comment) = mock.Retrieve("Key 2");
            key.Should().NotBeNullOrWhiteSpace();
            value.Should().NotBeNull();
            comment.Should().NotBeNullOrWhiteSpace();
        }
    }
}
