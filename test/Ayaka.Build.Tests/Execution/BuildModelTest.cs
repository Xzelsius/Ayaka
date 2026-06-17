// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build.Tests.Execution;

using Ayaka.Build;
using Ayaka.Build.Execution;

public sealed class BuildModelTest
{
    public sealed class FromDefinition
    {
        [Fact]
        public void Does_discover_targets_declared_on_the_build_class()
        {
            var model = BuildModel.FromDefinition(new ClassTargetBuild());

            model.Targets.Select(target => target.Name).Should().Contain("Default");
        }

        [Fact]
        public void Does_discover_targets_contributed_by_a_component_interface()
        {
            var model = BuildModel.FromDefinition(new ComponentBuild());

            model.Targets.Select(target => target.Name).Should().Contain("Foo");
        }

        [Fact]
        public void Does_discover_class_and_component_targets_together()
        {
            var model = BuildModel.FromDefinition(new MixedBuild());

            model.Targets.Select(target => target.Name).Should().BeEquivalentTo("Default", "Foo");
        }

        [Fact]
        public void Throws_InvalidOperationException_when_a_target_is_declared_twice()
        {
            var act = () => BuildModel.FromDefinition(new ConflictingBuild());

            act.Should().Throw<InvalidOperationException>().WithMessage("*more than one component*");
        }

        [Fact]
        public void Throws_InvalidOperationException_when_a_component_redeclares_a_target_with_the_new_keyword()
        {
            var act = () => BuildModel.FromDefinition(new RedeclaredComponentBuild());

            act.Should().Throw<InvalidOperationException>().WithMessage("*more than one component*");
        }
    }

    private interface IHaveFoo : IBuildDefinition
    {
        Target Foo { get; }
    }

    private interface ICanFoo : IHaveFoo
    {
        Target IHaveFoo.Foo => target => target.Executes(() => { });
    }

    private interface IHaveOtherFoo : IBuildDefinition
    {
        Target Foo { get; }
    }

    private interface ICanOtherFoo : IHaveOtherFoo
    {
        Target IHaveOtherFoo.Foo => target => target.Executes(() => { });
    }

    private interface IHaveFooAgain : IHaveFoo
    {
        new Target Foo { get; }
    }

    private sealed class ClassTargetBuild : BuildDefinition
    {
        private Target Default => target => target.Executes(() => { });
    }

    private sealed class ComponentBuild : BuildDefinition, ICanFoo
    {
    }

    private sealed class MixedBuild : BuildDefinition, ICanFoo
    {
        private Target Default => target => target.Executes(() => { });
    }

    private sealed class ConflictingBuild : BuildDefinition, ICanFoo, ICanOtherFoo
    {
    }

    private sealed class RedeclaredComponentBuild : BuildDefinition, IHaveFooAgain
    {
        Target IHaveFoo.Foo => target => target.Executes(() => { });

        Target IHaveFooAgain.Foo => target => target.Executes(() => { });
    }
}
