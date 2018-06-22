// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Ayaka.Infrastructure
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void Can_register_services_within_appdomain()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();

            serviceCollection.Scan(configuration.Object);

            Assert.Equal(3, serviceCollection.Count);
            Assert.All(serviceCollection, sc => Assert.Equal(ServiceLifetime.Singleton, sc.Lifetime));
        }

        [Fact]
        public void Can_register_services_from_assembly()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();

            serviceCollection.Scan(configuration.Object, typeof(ServiceCollectionExtensionsTest).Assembly);

            Assert.Equal(3, serviceCollection.Count);
            Assert.All(serviceCollection, sc => Assert.Equal(ServiceLifetime.Singleton, sc.Lifetime));
        }

        [Fact]
        public void Does_respect_call_order()
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new Mock<IConfiguration>();

            serviceCollection.Scan(configuration.Object);

            Assert.Equal(3, serviceCollection.Count);
            Assert.Equal(typeof(IService3), serviceCollection.First().ServiceType);
        }

        [Fact]
        public void Passes_configuration_to_registrars()
        {
            var serviceCollection = new ServiceCollection();

            Assert.Throws<NullReferenceException>(() => serviceCollection.Scan(null));
        }

        public class ServiceRegistrar1 : IServiceRegistrar
        {
            public int Order => 1;

            public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                services.AddSingleton<IService1, Service1>();
                services.AddSingleton<IService2, Service2>();
            }
        }

        public class ServiceRegistrar2 : IServiceRegistrar
        {
            public int Order => 0;

            public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                services.AddSingleton<IService3, Service3>();

                configuration["Foo"] = "Bar";
            }
        }

        public interface IService1
        {
        }

        public class Service1 : IService1
        {
        }

        public interface IService2
        {
        }

        public class Service2 : IService2
        {
        }

        public interface IService3
        {
        }

        public class Service3 : IService3
        {
        }
    }
}
