// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build.Execution;

using System.Reflection;

/// <summary>
///     Represents a <see cref="BuildDefinition"/> analyzed into its set of targets.
/// </summary>
internal sealed class BuildModel
{
    private readonly Dictionary<string, BuildTarget> _targets;

    private BuildModel(Dictionary<string, BuildTarget> targets)
    {
        _targets = targets;
    }

    /// <summary>
    ///     Gets the defined targets in the build definition.
    /// </summary>
    /// <value>A <see cref="IReadOnlyCollection{T}" /> containing the targets.</value>
    public IReadOnlyCollection<BuildTarget> Targets => _targets.Values;

    /// <summary>
    ///     Creates a <see cref="BuildModel"/> by analyzing the targets declared by
    ///     the specified <paramref name="definition"/>.
    /// </summary>
    /// <param name="definition">The build definition to analyze.</param>
    /// <returns>A new <see cref="BuildModel"/> instance.</returns>
    public static BuildModel FromDefinition(BuildDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var buildType = definition.GetType();
        var properties = ResolveTargetProperties(buildType);

        var targets = new Dictionary<string, BuildTarget>(StringComparer.Ordinal);

        foreach (var group in properties.GroupBy(property => property.Name, StringComparer.Ordinal))
        {
            if (group.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"Target '{group.Key}' is declared by more than one component. Overriding targets is not supported.");
            }

            var property = group.Single();
            var target = (Target)property.GetValue(definition)!;

            var configuration = new TargetDefinition(property);
            _ = target(configuration);

            targets.Add(configuration.Name, new BuildTarget(configuration));
        }

        return new BuildModel(targets);
    }

    private static IEnumerable<PropertyInfo> ResolveTargetProperties(Type buildType)
    {
        // Targets can be declared directly on the build class or contributed by component
        // interfaces (e.g. on an IHave*-style interface, default-implemented on an ICan*).
        var scannableTypes = buildType.GetInterfaces().Append(buildType);

        // Search for the Target properties
        var properties = scannableTypes
            .SelectMany(type =>
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Where(
                property =>
                    property.PropertyType == typeof(Target)
                    // Must have a getter
                    && property.CanRead
                    // Ignore indexer properties (e.g. this[int index])
                    && property.GetIndexParameters().Length == 0
                    // Ignore explicit interface implementations (dotted names)
                    && !property.Name.Contains('.'));

        return properties;
    }
}
