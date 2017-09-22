// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine.Configuration
{
	/// <summary>
	/// A command line based <see cref="ConfigurationProvider"/>.
	/// </summary>
	public class CommandLineExConfigurationProvider : ConfigurationProvider
	{
		private readonly Func<CommandLineApplication> _applicationFactory;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="args">The command line args.</param>
		public CommandLineExConfigurationProvider(IEnumerable<string> args, Func<CommandLineApplication> applicationFactory)
		{
			if(args == null)
			{
				throw new ArgumentNullException(nameof(args));
			}

			if(applicationFactory == null)
				throw new ArgumentNullException(nameof(applicationFactory));

			Args = args;
			_applicationFactory = applicationFactory;

		}

		/// <summary>
		/// The command line arguments.
		/// </summary>
		protected IEnumerable<string> Args
		{
			get; private set;
		}

		/// <summary>
		/// Loads the configuration data from the command line args.
		/// </summary>
		public override void Load()
		{
			var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			var app = _applicationFactory();
			var previous = app.Invoke;
			app.Execute(Args.ToArray());
			foreach(var opt in app.Options.Where(o => o.HasValue()))
			{
				if(opt.OptionType == CommandOptionType.BoolValue)
					data[opt.LongName] = opt.BoolValue.Value.ToString();
				if(opt.OptionType == CommandOptionType.NoValue)
					data[opt.LongName] = true.ToString();
				if(opt.OptionType == CommandOptionType.SingleValue)
					data[opt.LongName] = opt.Value();
				if(opt.OptionType == CommandOptionType.MultipleValue)
					throw new NotSupportedException("MultiValue options are not supported");
			}
			foreach(var arg in app.Arguments.Where(o => o.Value != null))
			{
				throw new NotSupportedException("Arguments not supported");
			}
			Data = data;
		}
	}
}
