using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using System.Threading;
using BetterExceptions.Interfaces;
using BetterExceptions.Web;
using MBEasyMod.Interfaces;
using MBEasyMod.Services;

namespace BetterExceptions.Services
{
	[Serializable]
	public class GithubUpdateHandler : IUpdateHandler
	{
		private struct GithubTag
		{
			internal struct GitCommit
			{
				public string sha {get;set;}
				public string url {get;set;}
			}
			public string name {get;set;}
			public string zipball_url {get;set;}
			public string tarball_url {get;set;}
			public GitCommit commit {get;set;}
			public string node_id {get;set;}
		}

		private const string moduleFolder = "../../Modules/BetterExceptions";

		AppDomain appDomain = null;

		public GithubUpdateHandler()
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup();
			appDomainSetup.ShadowCopyFiles = "true";
			appDomainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "../../Modules/BetterExceptions/bin/Win64_Shipping_Client/";

			appDomain = AppDomain.CreateDomain("BetterExceptionsDomain", null, appDomainSetup);
		}

		public Version GetLatestVersion()
		{
			ServiceManager.TryGetService(out ILogger logger);

			logger?.Log($"Fetching current version...");
			RespondBodyHelper resp = new RespondBodyHelper($"https://raw.githubusercontent.com/KaySteinhoff/BetterExceptions/master/BetterExceptions.csproj");
			byte[] buf = resp.FetchBody();
			string xml = Encoding.UTF8.GetString(buf, 0, buf.Length);
			logger?.Log("*.csproj file fetched! Parsing...");

			XDocument doc = XDocument.Parse(xml);

			XElement versionNode = doc.Root.Element("PropertyGroup").Element("Version");
			return new Version(versionNode.Value);
		}

		public Version[] GetVersionList()
		{
			ServiceManager.TryGetService(out ILogger logger);
			Version[] versions = null;
			try
			{
				RespondBodyHelper resp = new RespondBodyHelper($"https://api.github.com/repos/KaySteinhoff/BetterExceptions/tags");
				byte[] response = resp.FetchBody(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("user-agent", "anything") });
				GithubTag[] tags = JsonSerializer.Deserialize<GithubTag[]>(response);
				versions = new Version[tags.Length];
				for(int i = 0; i < tags.Length; ++i)
					versions[i] = new Version(tags[i].name);
			}catch(Exception e)
			{
				logger.LogException(e);
			}

			return versions;
		}

		public Task<bool> InstallVersion(Version version)
		{
			return Task.Run(()=>{
				ServiceManager.TryGetService(out ILogger logger);
				try
				{
					// Download zip
					logger.Log($"Downloading zip file of version {version}...");
					RespondBodyHelper resp = new RespondBodyHelper($"https://github.com/KaySteinhoff/BetterExceptions/releases/download/{version}/BetterExceptions.zip");
					Stream target = File.Open($"{moduleFolder}{version}.zip", FileMode.Create);
					byte[] data = resp.FetchBody(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("user-agent", "anything") });
					target.Write(data, 0, data.Length);
					target.Close();
					logger.Log($"Version {version} downloaded! Extracting...");

					// Extract data
					ZipFile.ExtractToDirectory($"{moduleFolder}{version}.zip", moduleFolder + version);
				}catch(Exception ex)
				{
					if(logger == null)
						return false;

					File.Delete($"{moduleFolder}{version}.zip");
					logger.Log("An error occured while extracting version {version}!");
					logger.LogException(ex);
					return false;
				}

				try
				{
					// Replace existing files with downloaded files(keep old config file)
					CopyDirectoryContent($"{moduleFolder}{version}/BetterExceptions/bin/Win64_Shipping_Client", $"{moduleFolder}/bin/Win64_Shipping_Client/", ".");
					CopyDirectoryContent($"{moduleFolder}{version}/BetterExceptions/ModuleData", $"{moduleFolder}/ModuleData/", "^(?!.+\\/?config\\.dat).+$");
					logger.Log("Files extracted! Cleaning up...");

					// Clean up
					File.Delete($"{moduleFolder}{version}.zip");
					Directory.Delete(moduleFolder + version, true);
				}
				catch (Exception e)
				{
					// Clean up
					File.Delete($"{moduleFolder}{version}.zip");
					Directory.Delete(moduleFolder + version, true);

					logger.LogException(e);
					return false;
				}

				return true;
			});
		}

		private void CopyDirectoryContent(string sourceDirectory, string destinationDirectory, string fileMatchRegex)
		{
			string[] files = Directory.GetFiles(sourceDirectory);
			for (int i = 0; i < files.Length; ++i)
			{
				if (!Regex.IsMatch(files[i], fileMatchRegex))
					continue;

				string destName = destinationDirectory + Path.GetFileName(files[i]);
				if(File.Exists(destName))
					File.Move(destName, destName + ".tmp");
				File.Copy(files[i], destName, true);
			}
		}
	}
}
