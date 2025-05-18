// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     Represents an exception that is thrown when an error occurs during tenant management operations.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TenantManagementException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TenantManagementException"/> class.
    /// </summary>
    public TenantManagementException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TenantManagementException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TenantManagementException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TenantManagementException"/> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TenantManagementException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
