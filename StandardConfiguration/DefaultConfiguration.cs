using CommandLine;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace StandardConfiguration
{
	public abstract class DefaultConfiguration
	{
		public ILogger Logger
		{
			get; set;
		} = NullLogger.Instance;
		protected abstract CommandLineApplication CreateCommandLineApplicationCore();
		protected abstract string GetDefaultDataDir(IConfiguration conf);
		protected abstract string GetDefaultConfigurationFile(IConfiguration conf);
		protected abstract string GetDefaultConfigurationFileTemplate(IConfiguration conf);

		public abstract string EnvironmentVariablePrefix
		{
			get;
		}

		protected abstract IPEndPoint GetDefaultEndpoint(IConfiguration conf);

		public CommandLineApplication CreateCommandLineApplication()
		{
			var app = CreateCommandLineApplicationCore();
			app.Option("-c | --conf", $"The configuration file", CommandOptionType.SingleValue);
			app.Option("-p | --port", $"The port on which to listen", CommandOptionType.SingleValue);
			app.Option("-b | --bind", $"The address on which to bind", CommandOptionType.MultipleValue);
			app.Option("-d | --datadir", $"The data directory", CommandOptionType.SingleValue);
			return app;
		}


		public IConfiguration CreateConfiguration(string[] args)
		{
			var app = CreateCommandLineApplication();

			bool executed = false;
			app.OnExecute(() =>
			{
				executed = true;
				return 1;
			});
			app.Execute(args);

			if(!executed)
				return null;


			var conf = new ConfigurationBuilder()
				.AddEnvironmentVariables(EnvironmentVariablePrefix)
				.AddCommandLineEx(args, CreateCommandLineApplication)
				.Build();

			var confFile = conf["conf"];
			if(confFile != null && File.Exists(confFile))
			{
				conf = new ConfigurationBuilder()
				.AddEnvironmentVariables(EnvironmentVariablePrefix)
				.AddIniFile(confFile)
				.AddCommandLineEx(args, CreateCommandLineApplication)
				.Build();
			}

			var datadir = conf["datadir"] ?? GetDefaultDataDir(conf);
			if(!Directory.Exists(datadir))
				Directory.CreateDirectory(datadir);
			Logger.LogInformation($"Data Directory: " + Path.GetFullPath(datadir));
			confFile = conf["conf"] ?? GetDefaultConfigurationFile(conf);
			Logger.LogInformation($"Configuration File: " + Path.GetFullPath(confFile));

			EnsureConfigFileExists(confFile, conf);


			var finalConf = new ConfigurationBuilder()
				.AddEnvironmentVariables(EnvironmentVariablePrefix)
				.AddIniFile(confFile)
				.AddCommandLineEx(args, CreateCommandLineApplication)
				.Build();

			var binds = app.Options.Where(o => o.LongName == "bind").SelectMany(o => o.Values.SelectMany(v => v.Split(';'))).ToList();

			var confBind = finalConf["bind"];
			if(!string.IsNullOrEmpty(confBind))
				binds.Add(confBind);

			List<KeyValuePair<string, string>> additionalSettings = new List<KeyValuePair<string, string>>();
			if(finalConf["port"] != null || binds.Count != 0 || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
			{
				var defaultEndpoint = GetDefaultEndpoint(conf);
				if(!int.TryParse(finalConf["port"] ?? "", out int defaultPort))
					defaultPort = defaultEndpoint.Port;
				if(binds.Count == 0)
				{
					binds.Add($"{defaultEndpoint.Address}:{defaultPort}");
				}
				for(int i = 0; i < binds.Count; i++)
				{
					var endpoint = ConvertToEndpoint(binds[i], defaultPort);
					binds[i] = $"{endpoint.Address}:{endpoint.Port}";
				}
				additionalSettings.Add(new KeyValuePair<string, string>("urls", string.Join(";", binds.Select(l => $"http://{l}/"))));
			}

			finalConf = new ConfigurationBuilder()
			.AddEnvironmentVariables(EnvironmentVariablePrefix)
			.AddIniFile(confFile)
			.AddInMemoryCollection(additionalSettings.ToArray())
			.AddCommandLineEx(args, CreateCommandLineApplication)
			.Build();

			return finalConf;
		}

		private void EnsureConfigFileExists(string confFile, IConfigurationRoot conf)
		{
			if(!File.Exists(confFile))
			{
				Logger.LogInformation("Creating configuration file");
				File.WriteAllText(confFile, GetDefaultConfigurationFileTemplate(conf));
			}
		}

		public static IPEndPoint ConvertToEndpoint(string str, int defaultPort)
		{
			var portOut = defaultPort;
			var hostOut = "";
			int colon = str.LastIndexOf(':');
			// if a : is found, and it either follows a [...], or no other : is in the string, treat it as port separator
			bool fHaveColon = colon != -1;
			bool fBracketed = fHaveColon && (str[0] == '[' && str[colon - 1] == ']'); // if there is a colon, and in[0]=='[', colon is not 0, so in[colon-1] is safe
			bool fMultiColon = fHaveColon && (str.LastIndexOf(':', colon - 1) != -1);
			if(fHaveColon && (colon == 0 || fBracketed || !fMultiColon))
			{
				int n;
				if(int.TryParse(str.Substring(colon + 1), out n) && n > 0 && n < 0x10000)
				{
					str = str.Substring(0, colon);
					portOut = n;
				}
			}
			if(str.Length > 0 && str[0] == '[' && str[str.Length - 1] == ']')
				hostOut = str.Substring(1, str.Length - 2);
			else
				hostOut = str;

			IPAddress ip = null;

			if(!IPAddress.TryParse(hostOut, out ip))
			{
				try
				{
					ip = Dns.GetHostEntry(hostOut).AddressList.FirstOrDefault();
				}
				catch { }
				if(ip == null)
					throw new FormatException("Invalid IP Endpoint");
			}

			return new IPEndPoint(ip, portOut);
		}
	}
}
