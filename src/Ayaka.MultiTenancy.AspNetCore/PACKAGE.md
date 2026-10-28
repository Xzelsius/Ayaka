## About

Selects the tenant for every ASP.NET Core request.

## Key Features

* Detect the tenant from a header, the host, or your own source
* Reject requests without a tenant (400) or with an unknown one (404)
* Let selected endpoints, such as health checks, run without a tenant
* Plug in your own detection and resolution

Selecting a tenant doesn't authorize the caller; your authorization policies decide that.

## How to Use

* [Quickstart](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/quickstart) — a working tenant-aware API in a few minutes
* [Request pipeline](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/aspnetcore/pipeline) — register and place the middleware
* [Tenant detection](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/aspnetcore/detection) — where the tenant ID comes from
* [Tenant resolution](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/aspnetcore/resolution) — accept or reject detected tenants
* [Troubleshooting](https://xzelsius.github.io/Ayaka/guide/packages/multi-tenancy/troubleshooting) — start from a response or exception you see

## Feedback & Contributing

`Ayaka` is an open-source project and welcomes contributions. If you have any ideas, improvements or issues, please open an issue
or a pull request at [the GitHub repository](https://github.com/Xzelsius/Ayaka).
