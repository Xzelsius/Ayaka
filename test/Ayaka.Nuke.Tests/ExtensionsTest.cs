// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests;

public sealed class ExtensionsTest
{
    public sealed class WhenNotNull
    {
        [Fact]
        public void Does_invoke_configurator_if_obj_is_not_null()
        {
            var settings = new TestSettings(false);
            var obj = new object();

            var actual = settings.WhenNotNull(obj, (s, o) => s.EnableFlag());

            actual.Should().BeEquivalentTo(new TestSettings(true));
        }

        [Fact]
        public void Does_not_invoke_configurator_if_obj_is_null()
        {
            var settings = new TestSettings(false);
            object? obj = null;

            var actual = settings.WhenNotNull(obj, (s, o) => s.EnableFlag());

            actual.Should().BeEquivalentTo(new TestSettings(false));
        }

        private sealed record TestSettings(bool Flag)
        {
            public TestSettings EnableFlag()
                => this with { Flag = true };
        }
    }


}
