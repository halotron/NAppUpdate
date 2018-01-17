using System;
using System.IO;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Common;


namespace DotNetCoreSample
{
	class Program
	{
		static void Main(string[] args)
		{
			ShowAppVersion();

			var updManager = UpdateManager.Instance;
			updManager.UpdateSource = PrepareUpdateSource();
			updManager.ReinstateIfRestarted();

			CheckForUpdates();

			Console.ReadKey();
		}

		private static void ShowAppVersion()
		{
			Console.WriteLine("Current app version: " + AppVersion);
		}

		public static string AppVersion
		{
			get
			{
				if (File.Exists("CurrentVersion.txt"))
					return File.ReadAllText("CurrentVersion.txt");
				return "1.0";
			}
		}

		private static NAppUpdate.Framework.Sources.IUpdateSource PrepareUpdateSource()
		{
			// Normally this would be a web based source.
			// But for the demo app, we prepare an in-memory source.
			var source = new NAppUpdate.Framework.Sources.MemorySource(File.ReadAllText("SampleAppUpdateFeed.xml"));
			source.AddTempFile(new Uri("http://SomeSite.com/Files/NewVersion.txt"), "NewVersion.txt");

			return source;
		}

		private static void CheckForUpdates()
		{
			UpdateManager updManager = UpdateManager.Instance;

			updManager.BeginCheckForUpdates(asyncResult =>
			{

				if (asyncResult.IsCompleted)
				{
					// still need to check for caught exceptions if any and rethrow
					((UpdateProcessAsyncResult)asyncResult).EndInvoke();

					// No updates were found, or an error has occured. We might want to check that...
					if (updManager.UpdatesAvailable == 0)
					{
						Console.WriteLine("All is up to date!");
						return;
					}
				}

				try
				{
					UpdateManager.Instance.PrepareUpdates();
				}
				catch
				{
					UpdateManager.Instance.CleanUp();
					return;
				}
				UpdateManager.Instance.ApplyUpdates(false);

				if (UpdateManager.Instance.State == UpdateManager.UpdateProcessState.AppliedSuccessfully)
				{
					ShowAppVersion();
				}
			}, null);
		}



	}
}
