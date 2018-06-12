// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Moq;
using Xunit;

namespace Ayaka.Reflection
{
    public class AssemblyExtensionsTest
    {
        [Fact]
        public void Can_get_file_version()
        {
            var fileVersion = PrepareMock(fileVersion: "10.0.7.1")
                .GetFileVersion(true);

            Assert.NotNull(fileVersion);
            Assert.Equal(new Version("10.0.7.1"), fileVersion);
        }

        [Fact]
        public void Can_get_version()
        {
            var version = PrepareMock(version: "10.0.0.7")
                .GetVersion(true);

            Assert.NotNull(version);
            Assert.Equal(new Version("10.0.0.7"), version);
        }

        [Fact]
        public void Can_get_title()
        {
            var title = PrepareMock(title: "FakeAssembly")
                .GetTitle(true);

            Assert.NotNull(title);
            Assert.Equal("FakeAssembly", title);
        }

        [Fact]
        public void Can_get_description()
        {
            var description = PrepareMock(desciption: "Fake Assembly")
                .GetDescription(true);

            Assert.NotNull(description);
            Assert.Equal("Fake Assembly", description);
        }

        [Fact]
        public void Can_get_product()
        {
            var product = PrepareMock(product: "FakeAssembly")
                .GetProduct(true);

            Assert.NotNull(product);
            Assert.Equal("FakeAssembly", product);
        }

        [Fact]
        public void Can_get_company()
        {
            var company = PrepareMock(company: "Contoso")
                .GetCompany(true);

            Assert.NotNull(company);
            Assert.Equal("Contoso", company);
        }

        [Fact]
        public void Can_get_copyright()
        {
            var copyright = PrepareMock(copyright: "Copyright © Contoso 2017")
                .GetCopyright(true);

            Assert.NotNull(copyright);
            Assert.Equal("Copyright © Contoso 2017", copyright);
        }

        [Fact]
        public void Can_get_trademark()
        {
            var trademark = PrepareMock(trademark: "FakeAssembly")
                .GetTrademark(true);

            Assert.NotNull(trademark);
            Assert.Equal("FakeAssembly", trademark);
        }

        [Fact]
        public void Can_get_culture()
        {
            var culture = PrepareMock(culture: "en-US")
                .GetCulture(true);

            Assert.NotNull(culture);
            Assert.Equal("en-US", culture);
        }

        [Fact]
        public void Can_get_configuration()
        {
            var configuration = PrepareMock(configuration: "DEBUG")
                .GetConfiguration(true);

            Assert.NotNull(configuration);
            Assert.Equal("DEBUG", configuration);
        }


        [Fact]
        public void Can_get_guid()
        {
            var expected = Guid.NewGuid();
            var actual = PrepareMock(guid: expected.ToString())
                .GetGuid(true);

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Can_get_debuggable_flag()
        {
            var debuggable = PrepareMock(debuggable: true)
                .IsDebuggable(true);

            Assert.True(debuggable);
        }

        [Fact]
        public void Throws_when_missing_attribute()
        {
            Assert.Throws<InvalidOperationException>(() => PrepareMock()
                .GetCopyright(true));
        }

        private Assembly PrepareMock(
            string name = "FakeAssembly",
            string version = "1.0.0.0",
            string fileVersion = null,
            string title = null,
            string desciption = null,
            string product = null,
            string company = null,
            string copyright = null,
            string trademark = null,
            string culture = null,
            string configuration = null,
            string guid = null,
            bool? debuggable = null)
        {
            var mock = new Mock<Assembly>();

            mock.Setup(a => a.GetName())
                .Returns(new AssemblyName($"{name}, Version=\"{version}\""));

            Setup<AssemblyVersionAttribute>(version);
            Setup<AssemblyFileVersionAttribute>(fileVersion);
            Setup<AssemblyTitleAttribute>(title);
            Setup<AssemblyDescriptionAttribute>(desciption);
            Setup<AssemblyProductAttribute>(product);
            Setup<AssemblyCompanyAttribute>(company);
            Setup<AssemblyCopyrightAttribute>(copyright);
            Setup<AssemblyTrademarkAttribute>(trademark);
            Setup<AssemblyCultureAttribute>(culture);
            Setup<AssemblyConfigurationAttribute>(configuration);
            Setup<GuidAttribute>(guid);
            Setup<DebuggableAttribute>(debuggable.HasValue && debuggable.Value
                ? DebuggableAttribute.DebuggingModes.Default
                : DebuggableAttribute.DebuggingModes.DisableOptimizations);

            void Setup<TAttribute>(object argument)
            {
                if (argument != null)
                {
                    mock.Setup(a => a.GetCustomAttributes(typeof(TAttribute), false))
                        .Returns(new[] {Activator.CreateInstance(typeof(TAttribute), argument)});
                }
            }

            return mock.Object;
        }
    }
}
