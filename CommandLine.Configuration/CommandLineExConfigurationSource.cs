// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace CommandLine.Configuration
{
    /// <summary>
    /// Represents command line arguments as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class CommandLineExConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Gets or sets the switch mappings.
        /// </summary>
        public Func<CommandLineApplication> ApplicationFactory { get; set; }

        /// <summary>
        /// Gets or sets the command line args.
        /// </summary>
        public IEnumerable<string> Args { get; set; }

        /// <summary>
        /// Builds the <see cref="CommandLineExConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="CommandLineExConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new CommandLineExConfigurationProvider(Args, ApplicationFactory);
        }
    }
}
