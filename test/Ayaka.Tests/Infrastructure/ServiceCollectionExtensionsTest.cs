// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ayaka.Infrastructure
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void Can_register_services_within_appdomain()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.Scan();

            Assert.Equal(3, serviceCollection.Count);
            Assert.All(serviceCollection, sc => Assert.Equal(ServiceLifetime.Singleton, sc.Lifetime));
        }

        [Fact]
        public void Can_register_services_from_assembly()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.Scan(typeof(ServiceCollectionExtensionsTest).Assembly);

            Assert.Equal(3, serviceCollection.Count);
            Assert.All(serviceCollection, sc => Assert.Equal(ServiceLifetime.Singleton, sc.Lifetime));
        }

        [Fact]
        public void Does_respect_call_order()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.Scan();

            Assert.Equal(3, serviceCollection.Count);
            Assert.Equal(typeof(IService3), serviceCollection.First().ServiceType);
        }

        public class ServiceRegistrar1 : IServiceRegistrar
        {
            public int Order => 1;

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IService1, Service1>();
                services.AddSingleton<IService2, Service2>();
            }
        }

        public class ServiceRegistrar2 : IServiceRegistrar
        {
            public int Order => 0;

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IService3, Service3>();
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
