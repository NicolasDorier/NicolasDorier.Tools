// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using CommandLine.Configuration;
using CommandLine;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for registering <see cref="CommandLineExConfigurationProvider"/> with <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public static class CommandLineExConfigurationExtensions
    {
		/// <summary>
		/// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the command line using the specified switch mappings.
		/// </summary>
		/// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
		/// <param name="args">The command line args.</param>
		/// <param name="applicationFactory">The command line definition.</param>
		/// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
		public static IConfigurationBuilder AddCommandLineEx(
            this IConfigurationBuilder configurationBuilder,
            string[] args,
            Func<CommandLineApplication> applicationFactory)
        {
            configurationBuilder.Add(new CommandLineExConfigurationSource { Args = args, ApplicationFactory = applicationFactory });
            return configurationBuilder;
        }

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the command line.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddCommandLineEx(this IConfigurationBuilder builder, Action<CommandLineExConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
