using System;
using BetterExceptions.Interfaces;
using NativeWindows.Win32.Wrappers;
using NativeWindows.Cross.Interfaces;
using NativeWindows.Win32;

namespace BetterExceptions.Services
{
	public class GUIExceptionHandler : IExceptionHandler
	{
		public void HandleException(Exception exception)
		{
			NWWin32Window window = new NWWin32Window("BetterExceptions - Crash Report Generated");
			/* Generate gui based on exception here */
			window.Show();
		}
	}
}
