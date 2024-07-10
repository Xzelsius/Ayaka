// Copyright (c) Raphael Strotz. All rights reserved.

using Nuke.Common;

partial class Build
{
    T From<T>()
        where T : INukeBuild
        => (T)(object)this;
}
