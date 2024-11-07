# Tenant Context

At the heart of the multi-tenancy functionality is the concept of a tenant context. The tenant context is
a way to tell the application for which tenant the current execution is happening.

This is done by setting the current tenant context at the beginning of your execution flow, which then
can be used by the application to

* Filter data based on the current tenant when reading from a database in case you follow the `shared database` pattern
* Switch to an entirely different database in case you follow the `database per tenant` pattern
* Use a different external service when interacting with external systems
* and much more

## ITenantContextAccessor

The `ITenantContextAccessor` and its default implementation, the `AsyncLocalTenantContextAccessor` is inspired by
ASP.NET's `IHttpContextAccessor` and provides a way to store and retrieve the current tenant context.

You need to register it in the dependency injection container to have it available in your .NET application.
After that you can inject it into your services and access the current tenant context.

::: code-group

```bash [Package Installation]
dotnet add package Ayaka.MultiTenancy
```

```csharp [Service Registration]
services.AddTenantContextAccessor();
```

```csharp {3,5,7,12} [Usage]
public class MyService
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public MyService(ITenantContextAccessor tenantContextAccessor)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public void DoSomething()
    {
        var tenantContext = _tenantContextAccessor.TenantContext;

        // Use the tenant context
    }
}
```

:::

::: warning
The default implementation of the `ITenantContextAccessor` uses `AsyncLocal<T>` to store the tenant context.

This can have a negative performance impact on async calls. It also creates a dependency on "ambient state" which
can make testing more difficult.

It also means, that the tenant context is only available in the same execution flow and not across asynchronous
boundaries.
:::
