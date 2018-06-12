using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ayaka.Reflection
{
    /// <summary>
    ///     Provides additional functionality for <see cref="Assembly" />.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        ///     Gets the <see typeref="TAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A <see typeref="TAttribute" /> instance.</returns>
        public static TAttribute GetAttribute<TAttribute>(this Assembly assembly, bool throwIfNotFound = false)
            where TAttribute : Attribute
        {
            if (assembly == null)
            {
                return null;
            }

            var attributes = assembly.GetCustomAttributes(typeof(TAttribute), false)
                .Cast<TAttribute>()
                .ToArray();

            if (!attributes.Any())
            {
                if (throwIfNotFound)
                {
                    throw new InvalidOperationException(
                        $"Could not find '{typeof(TAttribute)}' of '{assembly.FullName}'");
                }

                return null;
            }

            return attributes.First();
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyVersionAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A <see cref="Version" /> representing the attribute's value.</returns>
        public static Version GetVersion(this Assembly assembly, bool throwIfNotFound = false)
        {
            return assembly.GetName().Version;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyFileVersionAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A <see cref="Version" /> representing the attribute's value.</returns>
        public static Version GetFileVersion(this Assembly assembly, bool throwIfNotFound = false)
        {
            var fileVersionAttribute = assembly.GetAttribute<AssemblyFileVersionAttribute>(throwIfNotFound);
            if (fileVersionAttribute == null)
            {
                return new Version(0, 0, 0, 0);
            }

            return Version.Parse(fileVersionAttribute.Version);
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyTitleAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetTitle(this Assembly assembly, bool throwIfNotFound = false)
        {
            var titleAttribute = assembly.GetAttribute<AssemblyTitleAttribute>(throwIfNotFound);
            if (titleAttribute == null)
            {
                return null;
            }

            return titleAttribute.Title;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyDescriptionAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetDescription(this Assembly assembly, bool throwIfNotFound = false)
        {
            var descriptionAttribute = assembly.GetAttribute<AssemblyDescriptionAttribute>(throwIfNotFound);
            if (descriptionAttribute == null)
            {
                return null;
            }

            return descriptionAttribute.Description;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyProductAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetProduct(this Assembly assembly, bool throwIfNotFound = false)
        {
            var productAttribute = assembly.GetAttribute<AssemblyProductAttribute>(throwIfNotFound);
            if (productAttribute == null)
            {
                return null;
            }

            return productAttribute.Product;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyCompanyAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetCompany(this Assembly assembly, bool throwIfNotFound = false)
        {
            var companyAttribute = assembly.GetAttribute<AssemblyCompanyAttribute>(throwIfNotFound);
            if (companyAttribute == null)
            {
                return null;
            }

            return companyAttribute.Company;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyCopyrightAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetCopyright(this Assembly assembly, bool throwIfNotFound = false)
        {
            var copyrightAttribute = assembly.GetAttribute<AssemblyCopyrightAttribute>(throwIfNotFound);
            if (copyrightAttribute == null)
            {
                return null;
            }

            return copyrightAttribute.Copyright;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyTrademarkAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetTrademark(this Assembly assembly, bool throwIfNotFound = false)
        {
            var trademarkAttribute = assembly.GetAttribute<AssemblyTrademarkAttribute>(throwIfNotFound);
            if (trademarkAttribute == null)
            {
                return null;
            }

            return trademarkAttribute.Trademark;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyCultureAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetCulture(this Assembly assembly, bool throwIfNotFound = false)
        {
            var cultureAttribute = assembly.GetAttribute<AssemblyCultureAttribute>(throwIfNotFound);
            if (cultureAttribute == null)
            {
                return null;
            }

            return cultureAttribute.Culture;
        }

        /// <summary>
        ///     Gets the <see cref="AssemblyConfigurationAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A string representing the attribute's value.</returns>
        public static string GetConfiguration(this Assembly assembly, bool throwIfNotFound = false)
        {
            var configurationAttribute = assembly.GetAttribute<AssemblyConfigurationAttribute>(throwIfNotFound);
            if (configurationAttribute == null)
            {
                return null;
            }

            return configurationAttribute.Configuration;
        }

        /// <summary>
        ///     Gets the <see cref="GuidAttribute" /> from the specified <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>A <see cref="Guid" /> representing the attribute's value.</returns>
        public static Guid GetGuid(this Assembly assembly, bool throwIfNotFound = false)
        {
            var guidAttribute = assembly.GetAttribute<GuidAttribute>(throwIfNotFound);
            if (guidAttribute == null)
            {
                return Guid.Empty;
            }

            return Guid.Parse(guidAttribute.Value);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="Assembly" /> is debuggable.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="throwIfNotFound">If set to <c>true</c> throws an exception if the attribute is not found.</param>
        /// <returns>
        ///     <c>true</c> if the specified is debuggable; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDebuggable(this Assembly assembly, bool throwIfNotFound = false)
        {
            var debuggableAttribute = assembly.GetAttribute<DebuggableAttribute>(throwIfNotFound);
            if (debuggableAttribute == null)
            {
                return false;
            }

            return (debuggableAttribute.DebuggingFlags & DebuggableAttribute.DebuggingModes.Default) == DebuggableAttribute.DebuggingModes.Default;
        }
    }
}
