## About

Contracts and models for multi-tenant .NET applications. Reference it from libraries that work with
tenants without choosing how an application stores or selects them. It registers no services;
applications use [`Ayaka.MultiTenancy`](https://www.nuget.org/packages/Ayaka.MultiTenancy).

## Key Features

* The current tenant and the accessor to read it
* Tenant records and the contracts for managing, storing, and caching them
* Builder interfaces for your own registration extensions

## How to Use

* [Overview](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy) — how the pieces fit together
* [Tenant context](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/tenant-context) — read the current tenant
* [Provide a store](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/management/tenants#provide-a-store) — implement your own tenant store
* [Replace the cache](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/management/caching#replace-the-cache) — implement your own tenant cache

## Feedback & Contributing

`Ayaka` is an open-source project and welcomes contributions. If you have any ideas, improvements or issues, please open an issue
or a pull request at [the GitHub repository](https://github.com/Xzelsius/Ayaka).
