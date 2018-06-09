// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Moq;
using Xunit;

namespace Ayaka.Routing
{
    public class RouteBuilderExtensionsTest
    {
        [Fact]
        public void Can_register_routes_within_appdomain()
        {
            var routeBuilder = CreateRouteBuilder();

            routeBuilder.Scan();

            Assert.Equal(3, routeBuilder.Routes.Count);
        }

        [Fact]
        public void Can_register_routes_from_assembly()
        {
            var routeBuilder = CreateRouteBuilder();

            routeBuilder.Scan(typeof(RouteBuilderExtensionsTest).Assembly);

            Assert.Equal(3, routeBuilder.Routes.Count);
        }

        [Fact]
        public void Does_respect_call_order()
        {
            var routeBuilder = CreateRouteBuilder();

            routeBuilder.Scan();

            Assert.Equal(3, routeBuilder.Routes.Count);
            Assert.Equal("default", ((Route) routeBuilder.Routes[0]).Name);
        }

        private IRouteBuilder CreateRouteBuilder()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            // Required by RouteBuilder to verify services are loaded
            serviceProvider
                .Setup(sp => sp.GetService(typeof(RoutingMarkerService)))
                .Returns(new RoutingMarkerService());

            // Required to create Route
            var inlineConstraintResolver = new Mock<IInlineConstraintResolver>();

            serviceProvider
                .Setup(sp => sp.GetService(typeof(IInlineConstraintResolver)))
                .Returns(inlineConstraintResolver.Object);

            var applicationBuilder = new Mock<IApplicationBuilder>();
            applicationBuilder
                .SetupGet(ab => ab.ApplicationServices)
                .Returns(serviceProvider.Object);

            var router = new Mock<IRouter>();

            return new RouteBuilder(applicationBuilder.Object, router.Object);
        }

        public class RouteRegistrar1 : IRouteRegistrar
        {
            public int Order => 1;

            public void ConfigureRoutes(IRouteBuilder builder)
            {
                builder.MapRoute("login", "login");
                builder.MapRoute("logout", "logout");
            }
        }

        public class RouteRegistrar2 : IRouteRegistrar
        {
            public int Order => 0;

            public void ConfigureRoutes(IRouteBuilder builder)
            {
                builder.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            }
        }
    }
}
