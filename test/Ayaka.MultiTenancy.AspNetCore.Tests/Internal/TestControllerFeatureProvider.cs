// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Internal;

using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

internal sealed class TestControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly Type[] _controllerTypes;

    public TestControllerFeatureProvider(Type[] controllerTypes)
    {
        _controllerTypes = controllerTypes;
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        foreach (var controllerType in _controllerTypes)
        {
            feature.Controllers.Add(controllerType.GetTypeInfo());
        }
    }
}
