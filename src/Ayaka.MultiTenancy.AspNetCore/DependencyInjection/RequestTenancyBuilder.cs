// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using System.Diagnostics;
using Ayaka.MultiTenancy.AspNetCore;
using Ayaka.MultiTenancy.AspNetCore.Detection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     A builder for configuring <see cref="RequestTenancyOptions"/> used by the <see cref="RequestTenancyMiddleware"/>.
/// </summary>
public sealed class RequestTenancyBuilder
{
    private readonly List<Action<RequestTenancyOptions, IServiceProvider>> _configureActions = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestTenancyBuilder"/> class.
    /// </summary>
    /// <param name="multiTenancy">The <see cref="IMultiTenancyBuilder" /> to configure request tenancy on.</param>
    internal RequestTenancyBuilder(IMultiTenancyBuilder multiTenancy)
    {
        MultiTenancy = multiTenancy;
    }

    /// <summary>
    ///     Gets the <see cref="IMultiTenancyBuilder"/> where request tenancy is configured.
    /// </summary>
    public IMultiTenancyBuilder MultiTenancy { get; }

    /// <summary>
    ///     Configures the request tenancy to detect the tenant from a request header with the specified
    ///     <paramref name="headerName"/>.
    /// </summary>
    /// <param name="headerName">The name of the header to read the tenant from.</param>
    /// <returns>The same <see cref="RequestTenancyBuilder"/>.</returns>
    public RequestTenancyBuilder DetectFromRequestHeader(string headerName)
        => AddStrategy(sp => CreateStrategyInstance(sp, typeof(FromRequestHeaderStrategy), headerName));

    /// <summary>
    ///     Configures the request tenancy to detect the tenant from the request host.
    /// </summary>
    /// <remarks>
    ///     Takes the left-most part of the host name as tenant identifier.
    ///     <list type="table">
    ///        <item>
    ///           <term>example.com</term>
    ///            <description>=> example</description>
    ///       </item>
    ///           <item>
    ///           <term>sub.example.com</term>
    ///           <description>=> sub</description>
    ///       </item>
    ///       <item>
    ///           <term>deep-sub.sub.example.com</term>
    ///           <description>=> deep-sub</description>
    ///       </item>
    ///     </list>
    ///     <para>
    ///         Please note that <c>localhost</c> and host names with only one part are not considered valid tenant
    ///         identifiers.
    ///     </para>
    /// </remarks>
    /// <returns>The same <see cref="RequestTenancyBuilder"/>.</returns>
    public RequestTenancyBuilder DetectFromRequestHost()
        => AddStrategy(sp => CreateStrategyInstance(sp, typeof(FromRequestHostStrategy)));

    /// <summary>
    ///     Configures the request tenancy to detect the tenant using an instance of the specified
    ///     <typeparamref name="TDetectionStrategy"/> strategy.
    /// </summary>
    /// <remarks>
    ///     Automatically injects constructor dependencies during instantiation.
    /// </remarks>
    /// <typeparam name="TDetectionStrategy">The type of the strategy to use.</typeparam>
    /// <returns>The same <see cref="RequestTenancyBuilder"/>.</returns>
    public RequestTenancyBuilder DetectUsing<TDetectionStrategy>()
        where TDetectionStrategy : ITenantDetectionStrategy
        => AddStrategy(sp => CreateStrategyInstance(sp, typeof(TDetectionStrategy)));

    /// <summary>
    ///    Configures the request tenancy to detect the tenant using the specified <paramref name="strategy"/>.
    /// </summary>
    /// <param name="strategy">The strategy to use.</param>
    /// <returns>The same <see cref="RequestTenancyBuilder"/>.</returns>
    public RequestTenancyBuilder DetectUsing(ITenantDetectionStrategy strategy)
        => AddStrategy(_ => strategy);


    /// <summary>
    ///     Configures the request tenancy to use the specified <paramref name="tagName"/> as the name of the tag
    ///     written to the request's <see cref="Activity"/> after the middleware successfully detected a tenant.
    /// </summary>
    /// <param name="tagName">The name of the tag to write to the request's <see cref="Activity"/>.</param>
    /// <returns>The same <see cref="RequestTenancyBuilder"/>.</returns>
    public RequestTenancyBuilder UseCustomActivityTagName(string tagName)
    {
        _configureActions.Add(
            (options, _) => options.ActivityTagName = tagName);

        return this;
    }

    internal void ConfigureServices()
    {
        // Ability to resolve IOptions<RequestTenancyOptions>
        var optionBuilder = MultiTenancy.Services.AddOptions<RequestTenancyOptions>();

        if (_configureActions.Count > 0)
        {
            // Apply them custom configuration actions
            _ = optionBuilder.Configure<IServiceProvider>((opts, sp) =>
            {
                foreach (var action in _configureActions)
                {
                    action(opts, sp);
                }
            });
        }
    }

    private static ITenantDetectionStrategy CreateStrategyInstance(
        IServiceProvider serviceProvider,
        Type strategyType,
        params object[] args)
        => (ITenantDetectionStrategy)ActivatorUtilities.CreateInstance(
            serviceProvider,
            strategyType,
            args);

    private RequestTenancyBuilder AddStrategy(Func<IServiceProvider, ITenantDetectionStrategy> factory)
    {
        _configureActions.Add(
            (opts, sp) =>
                opts.DetectionStrategies
                    .Add(factory(sp)));

        return this;
    }
}
