using System;
using System.Diagnostics;
using System.IO;
using BetterExceptions.Web;
using BetterExceptions.Interfaces;
using System.Collections;
using MBEasyMod.Services;
using MBEasyMod.Interfaces;

namespace BetterExceptions.Services
{
	public class HTMLExceptionHandler : IExceptionHandler
	{
		// Replace this with template parsing!!!
		private void GenerateHtmlFromException(HTMLDoc doc, ref int idx, Exception e)
		{
			doc.OpenElement(idx++, "span"); // Toggle button
			doc.AddAttribute(idx++, "class", "caret");
			doc.AddContent(idx++, "Exception");
			doc.CloseElement(); // Toggle button	

			doc.OpenElement(idx++, "div"); // Table
			doc.AddAttribute(idx++, "class", "tbl nested");	

			doc.OpenElement(idx++, "div"); // Type row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "Exception Type");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, e.GetType());
			doc.CloseElement(); // Row Content	

			doc.CloseElement(); // Type row	

			doc.OpenElement(idx++, "div"); // Message row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "Message");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, e.Message?.Replace(Environment.NewLine, "<br>"));
			doc.CloseElement(); // Row Content	

			doc.CloseElement(); // Message row	

			doc.OpenElement(idx++, "div"); // Source row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "Source");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, e.Source);
			doc.CloseElement(); // Row Content	

			doc.CloseElement(); // Source row	

			doc.OpenElement(idx++, "div"); // HResult row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "HResult");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, e.HResult);
			doc.CloseElement(); // Row Content	

			doc.CloseElement(); // HResult row	

			doc.OpenElement(idx++, "div"); // Stacktrace row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "Stacktrace");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, e.StackTrace?.Replace(Environment.NewLine, "<br>"));
			doc.CloseElement(); // Stacktrace Content	

			doc.CloseElement(); // Source row	

			doc.OpenElement(idx++, "div"); // Data row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "Data");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");	

			doc.OpenElement(idx++, "div"); // Data list
			doc.AddAttribute(idx++, "class", "tbl");	
			foreach(DictionaryEntry de in e.Data)
			{
				doc.OpenElement(idx++, "div"); // KeyValuePair row
				doc.AddAttribute(idx++, "class", "tbl-row");	

				doc.OpenElement(idx++, "div"); // Row description
				doc.AddAttribute(idx++, "class", "tbl-cell");
				doc.AddContent(idx++, de.Key);
				doc.CloseElement(); // Row description	

				doc.OpenElement(idx++, "div"); // Row content
				doc.AddAttribute(idx++, "class", "tbl-cell");
				doc.AddContent(idx++, de.Value);
				doc.CloseElement(); // Row content	

				doc.CloseElement(); // KeyValuePair row
			}	
			doc.CloseElement(); // Data list
			doc.CloseElement(); // Row content	
			doc.CloseElement(); // Data row	

			if(e.InnerException == null)
			{
				doc.CloseElement(); // Table
				return;
			}	

			doc.OpenElement(idx++, "div"); // InnerException row
			doc.AddAttribute(idx++, "class", "tbl-row");	

			doc.OpenElement(idx++, "div"); // Row description
			doc.AddAttribute(idx++, "class", "tbl-cell");
			doc.AddContent(idx++, "InnerException");
			doc.CloseElement(); // Row description	

			doc.OpenElement(idx++, "div"); // Row content
			doc.AddAttribute(idx++, "class", "tbl-cell");	
			GenerateHtmlFromException(doc, ref idx, e.InnerException);	
			doc.CloseElement(); // Row Content	

			doc.CloseElement(); // InnerException row	
			doc.CloseElement(); // Table
		}	
		public void HandleException(Exception exception)
		{
			string htmlFile = $"../../Modules/BetterExceptions/ModuleData/CrashReports/{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.html";
			string style = 
			"<style>" + 
			".tbl { display: table; border-top: 1px solid black; border-left: 1px solid black; width: 100%; min-width: 10vw; }" +
			".tbl-row { display: table-row; }" +
			".tbl-cell { display: table-cell; border-bottom: 1px solid black; border-right: 1px solid black; }" +
			".caret { cursor: pointer; user-select: none; }" +
			".caret::before { content: \"\\25B6\"; color: black; display: inline-block; margin-left: 6px; margin-right: 6px; }" +
			".caret-down::before { transform: rotate(90deg); }" +
			".nested { display: none; }" +
			".active { display: table; }" +
			"</style>";
			string js = 
			"<script>" +
			"var toggler = document.getElementsByClassName(\"caret\");" +
			"var i = 0;" +
			"for (i = 0; i < toggler.length; i++) {" +
			"toggler[i].addEventListener(\"click\", function() {" +
			"this.parentElement.querySelector(\".nested\").classList.toggle(\"active\");" +
			"this.classList.toggle(\"caret-down\");" +
			"});" +
			"}" +
			"</script>";
			int idx = 0;
			HTMLDoc doc = new HTMLDoc();
			doc.OpenElement(idx++, "html");
			doc.OpenElement(idx++, "head");
			doc.AddContent(idx++, style);
			doc.CloseElement(); // head
			doc.OpenElement(idx++, "body");	
			try
			{
				GenerateHtmlFromException(doc, ref idx, exception);
			}catch(Exception ex)
			{
				if(ServiceManager.TryGetService(out ILogger logger))
					logger.LogException(ex);
			}	
			doc.AddContent(idx++, js);
			doc.CloseElement(); // body
			doc.CloseElement(); // html	
			StreamWriter writer = new StreamWriter(htmlFile, false);
			try
			{
				writer.Write(doc.ToString());
			}catch(Exception ex)
			{
				if(ServiceManager.TryGetService(out ILogger logger))
					logger.LogException(ex);
			}
			writer.Close();	
			try
			{
				Process.Start($"file://{Path.GetFullPath(htmlFile)}");
			} catch
			{
				try
				{
					Process.Start(Path.GetFullPath(htmlFile));
				}catch (Exception ex)
				{
					if(!ServiceManager.TryGetService(out ILogger logger))
						return;	
					logger.Log("Exsausted all options to open the crash report HTML file!");
					logger.LogException(ex);
				}
			}
		}
	}
}
