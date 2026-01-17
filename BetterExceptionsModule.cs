using System;
using MBEasyMod.Debugging;
using TaleWorlds.MountAndBlade;
using BetterExceptions.Services;
using BetterExceptions.Interfaces;
using MBEasyMod.Notification;
using System.IO;
using MBEasyMod.Services;
using MBEasyMod.Interfaces;
using System.Threading.Tasks;

namespace BetterExceptions
{
	public class BetterExceptionsModule : MBSubModuleBase
	{
		bool initialized = false;
		protected override void OnSubModuleLoad()
		{
			try
			{
				Init();
			}
			catch(Exception ex)
			{
				if(!ServiceManager.TryGetService(out ILogger logger))
					return;
				logger.Log("Failed to properly initialize BetterExceptions! Your experience may be compromised!");
				logger.LogException(ex);
			}
		}
		private void Init()
		{
			DebugManager.Instance.EnableDebugMode();
			DebugManager.Instance.UnhandledExceptionThrown += ExceptionThrown;
			ServiceManager.RegisterService<ILogger, Logger>(new Logger());
			ILogger logger = null;
			ConfigManager configManager = new ConfigManager("../../Modules/BetterExceptions/ModuleData/config.dat");
			GithubUpdateHandler updateHandler = new GithubUpdateHandler();
			try
			{
				configManager.ReadConfigs();
			}catch
			{
				if(!ServiceManager.TryGetService(out logger))
					return;
				logger.Log("Failed to read BetterExceptions config file! Default settings will be used!");
				configManager.TrySetConfigValue("use_gui_handler", false);
				configManager.TrySetConfigValue("selected_version", "latest");
				if(!File.Exists("../../Modules/BetterExceptions/ModuleData/config.dat"))
				{
					logger.Log("Creating default config file...");
					configManager.SaveConfigs();
				}
			}

			ServiceManager.RegisterService<IConfigManager, ConfigManager>(configManager);
			ServiceManager.RegisterService<IUpdateHandler, GithubUpdateHandler>(updateHandler);
			if(configManager.TryGetConfigValue("use_gui_handler", out bool useGuiHandler) && useGuiHandler)
				ServiceManager.RegisterService<IExceptionHandler, GUIExceptionHandler>(new GUIExceptionHandler());
			else
				ServiceManager.RegisterService<IExceptionHandler, HTMLExceptionHandler>(new HTMLExceptionHandler());

			if(logger != null || ServiceManager.TryGetService(out logger))
				logger.Log("BetterExceptions initialized");

			/* All important init functionality done, after this point elements may fail but it won't matter */
			initialized = true;
			Task.Run(CleanUpTmpFiles);
		}
		private void ExceptionThrown(object sender, Exception e)
		{
			if(ServiceManager.TryGetService(out ILogger logger))
				logger.Log($"Caught exception {e.GetType()}");
			if(!ServiceManager.TryGetService(out IExceptionHandler exceptionHandler))
				return;
			try
			{
				exceptionHandler.HandleException(e);
			}catch (Exception ex)
			{
				logger.LogException(ex);
			}
		}
		private void CleanUpTmpFiles()
		{
			string[] tmpFiles = Directory.GetFiles("../../Modules/BetterExceptions/bin/Win64_Shipping_Client", "*.tmp");
			for(int i = 0; i < tmpFiles.Length; ++i)
				File.Delete(tmpFiles[i]);

			tmpFiles = Directory.GetFiles("../../Modules/BetterExceptions/ModuleData", "*.tmp");
			for(int i = 0; i < tmpFiles.Length; ++i)
				File.Delete(tmpFiles[i]);
		}
		private void CheckForUpdates()
		{
			if( ServiceManager.TryGetService(out IConfigManager configManager) &&
				configManager.TryGetConfigValue("selected_version", out string selectedVersion) &&
				selectedVersion != "latest")
				return;
			if(!ServiceManager.TryGetService(out IUpdateHandler updateHandler))
				return;
			Version latest = updateHandler.GetLatestVersion();
			if(GetType().Assembly.GetName().Version.CompareTo(latest) >= 0)
				return;

			MBPopup.ShowSimplePopup(
				"Update available",
				"A new version of BetterExceptions is available! Do you wish to update?",
				"Update now",
				"Ignore",
				()=>
				{
					updateHandler.InstallVersion(latest).ContinueWith((result) =>
					{
						if(result.Result)
							MBPopup.ShowSimpleNotificationPopup("Successfully installed!", "Newest version of BetterExceptions installed! Please restart your game to apply the updates.", "OK", ()=>{});
						else
							MBPopup.ShowSimpleAlertPopup("Update failed!", "Failed to install newest version of BetterExceptions!", "OK", ()=>{});
					});
				},
				()=>{});
		}
		public override void OnInitialState()
		{
			bool abortInit = false;
			while(!initialized && !abortInit)
			{
				MBPopup.ShowSimplePopup("Initialization fail!", "Failed to properly initialize BetterExceptions! Do you wish to try and reinitialize?", "Reinitialize", "Ignore", () =>
				{
					try
					{
						Init();
					}catch { }
				},
				()=>{ abortInit = true; });
			}
			Task.Run(CheckForUpdates);
		}
		protected override void OnSubModuleUnloaded()
		{
			DebugManager.Instance.UnhandledExceptionThrown -= ExceptionThrown;
			DebugManager.Instance.DisableDebugMode();
			ILogger logger = null;
			if(ServiceManager.TryGetService(out IConfigManager configManager))
			{
				try
				{
					configManager.SaveConfigs();
				}catch(Exception e)
				{
					if(ServiceManager.TryGetService(out logger))
						logger.LogException(e);
				}
			}
			if(logger != null || ServiceManager.TryGetService(out logger))
			{
				logger.Log($"BetterExceptions successfully terminated!");
				logger.Dispose();
			}
		}
	}
}
