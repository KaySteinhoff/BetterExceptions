using System;
using MBEasyMod.Debugging;
using TaleWorlds.MountAndBlade;
using BetterExceptions.Services;
using BetterExceptions.Interfaces;
using MBEasyMod.Notification;

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
            ServiceManager.RegisterService<IExceptionHandler, HTMLExceptionHandler>(new HTMLExceptionHandler());
            ServiceManager.RegisterService<ILogger, Logger>(new Logger());

            if(ServiceManager.TryGetService(out ILogger logger))
                logger.Log("BetterExceptions initialized");
            initialized = true;
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

        private void CheckForUpdates()
        {
            if(!ServiceManager.TryGetService(out IUpdateHandler updateHandler))
                return;

            Version latest = updateHandler.GetLatestVersion();
            if(GetType().Assembly.GetName().Version.CompareTo(latest) >= 0)
                return;
            
            MBPopup.ShowSimplePopup(
                "Update available", 
                "A new version of BetterException is available! Do you wish to update?", 
                "Update now", 
                "Ignore", 
                ()=>
                {
                    updateHandler.InstallVersion(latest).ContinueWith((result) =>
                    {
                        if(result.Result)
                            MBPopup.ShowSimpleNotificationPopup("Successfully installed!", "Newest version of BetterExceptions installed! Please restart your game to apply the updates.", "OK", ()=>{});
                        else
                            MBPopup.ShowSimpleAlertPopup("Update failed!", "Failed to install wewest version of BetterExceptions!", "OK", ()=>{});
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

            CheckForUpdates();
        }

        protected override void OnSubModuleUnloaded()
        {
            DebugManager.Instance.UnhandledExceptionThrown -= ExceptionThrown;
            DebugManager.Instance.DisableDebugMode();

            if(ServiceManager.TryGetService(out ILogger logger))
            {
                logger.Log($"BetterExceptions successfully terminated!");
                logger.Dispose();
            }
        }
    }
}
