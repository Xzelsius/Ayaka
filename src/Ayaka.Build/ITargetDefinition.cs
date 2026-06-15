// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build;

public interface ITargetDefinition
{
    ITargetDefinition Executes(Action action);

    ITargetDefinition Executes(Func<Task> action);
}
