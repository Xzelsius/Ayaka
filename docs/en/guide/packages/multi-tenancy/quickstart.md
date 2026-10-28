# Quickstart

Build an API that returns the current tenant, rejects unknown tenants, and exposes a health
endpoint that works without a tenant. You need a terminal and the latest stable .NET SDK; see
[Compatibility](../../compatibility.md).

## 1. Create the application

```sh
dotnet new web --name TenantApi
cd TenantApi
dotnet add package Ayaka.MultiTenancy --prerelease
dotnet add package Ayaka.MultiTenancy.AspNetCore --prerelease
```

## 2. Register services and endpoints

Replace `Program.cs` with:

```csharp
using Ayaka.MultiTenancy;
using Ayaka.MultiTenancy.AspNetCore;
using Ayaka.MultiTenancy.DependencyInjection;
using Ayaka.MultiTenancy.Management;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddMultiTenancy()
    .ConfigureRequestTenancy(request => request
        .DetectFromRequestHeader("X-Tenant")
        .ResolveWithTenantManagement())
    .AddTenantManagement()
    .UseInMemoryStore();

var app = builder.Build();

var tenants = app.Services.GetRequiredService<ITenantManager>();
await tenants.AddAsync(new Tenant("acme", "Acme"));
await tenants.AddAsync(new Tenant("contoso", "Contoso"));

app.UseRouting();
app.UseMultiTenancy();

app.MapGet("/tenant", (ITenantContextAccessor accessor) => Results.Ok(accessor.TenantContext));

app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithMetadata(new DisableMultiTenancyAttribute());

await app.RunAsync();
```

What each part does:

- `AddMultiTenancy()` registers the [tenant context accessor](./tenant-context.md).
- `ConfigureRequestTenancy(...)` [detects](./aspnetcore/detection.md) the tenant from the `X-Tenant`
  header and [resolves](./aspnetcore/resolution.md) it through tenant management, so unknown tenants
  are rejected.
- `AddTenantManagement().UseInMemoryStore()` adds the [tenant manager](./management/tenants.md) with a
  store that lives in the running process. The application seeds it before accepting requests.
- `UseRouting()` selects an endpoint before `UseMultiTenancy()` checks its metadata. The
  [middleware](./aspnetcore/pipeline.md) then makes the tenant available through `ITenantContextAccessor`.

## 3. Run it

```sh
dotnet run --no-launch-profile --urls http://localhost:5080
```

In another terminal, send a request:

::: code-group

```sh [bash]
curl -i -H "X-Tenant: acme" http://localhost:5080/tenant
```

```powershell [pwsh]
Invoke-WebRequest http://localhost:5080/tenant -Headers @{ 'X-Tenant' = 'acme' } -SkipHttpErrorCheck
```

:::

You should receive **200 OK** with:

```json
{ "id": "acme", "displayName": "Acme" }
```

Change the header to `X-Tenant: contoso` to get Contoso's context. Try these cases as well:

| Request | Expected result |
| --- | --- |
| `/tenant` without `X-Tenant` | **400**, problem title `Tenant could not be detected.` |
| `/tenant` with `X-Tenant: unknown` | **404**, problem title `Tenant is unknown.` |
| `/health` without `X-Tenant` | **200**, because the endpoint disables multi-tenancy. |

The problem responses come from `AddProblemDetails()`. Without a problem-details service,
the middleware still sets the status code but does not write a response body.

## Next steps

This example loses its tenants when the process restarts, and it selects tenants without
authenticating callers. To go further:

- [Database-backed tenant store](./guides/database-store.md) — persist tenants and cache lookups.
- [Tenant context](./tenant-context.md) — how the current tenant flows through your code.
- [Request pipeline](./aspnetcore/pipeline.md) — where to place the middleware in a real application.
