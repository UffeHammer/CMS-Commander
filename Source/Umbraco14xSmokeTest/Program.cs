using System;
using System.Linq;
using SitecoreConverter.Core;

namespace SitecoreConverter.Umbraco14xSmokeTest
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                string url      = GetArg(args, "--url")       ?? Environment.GetEnvironmentVariable("UMBRACO14_URL");
                string username = GetArg(args, "--username")  ?? Environment.GetEnvironmentVariable("UMBRACO14_USERNAME");
                string password = GetArg(args, "--password")  ?? Environment.GetEnvironmentVariable("UMBRACO14_PASSWORD");

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    Console.Error.WriteLine("Usage: Umbraco14xSmokeTest --url <base-url> --username <user> --password <pwd>");
                    Console.Error.WriteLine("       or set UMBRACO14_URL / UMBRACO14_USERNAME / UMBRACO14_PASSWORD env vars");
                    return 2;
                }

                var creds = new Credentials { UserName = username, Password = password };
                var api = new Umbraco14xAPI(url, creds);
                var options = new ConverterOptions { Language = "en-US" };
                var root = Umbraco14xItem.GetRoot(api, options);

                Console.WriteLine("Root: " + root.Name + " (" + root.ID + ")");
                foreach (var branch in root.GetChildren())
                    Console.WriteLine("  Branch: " + branch.Name);

                var content = root.GetChildren().FirstOrDefault(c => c.Name == "Content");
                if (content == null) { Console.Error.WriteLine("No Content branch"); return 3; }

                Console.WriteLine("\nTop-level Content items:");
                var topContent = content.GetChildren();
                foreach (var i in topContent) Console.WriteLine("  - " + i.Name + " (" + i.ID + ")");

                if (!topContent.Any())
                {
                    Console.WriteLine("No content items found; exiting successfully after traversal.");
                    return 0;
                }

                var first = topContent.First();
                Console.WriteLine("\nReading first content item's fields:");
                foreach (var f in first.Fields)
                    Console.WriteLine("  " + f.Name + " [" + f.Type + "] = " +
                        (f.Content?.Length > 60 ? f.Content.Substring(0, 60) + "..." : f.Content));

                var probe = first.Fields.FirstOrDefault(f =>
                    f.Type == "Umbraco.TextBox" || f.Type == "Umbraco.TextArea");
                if (probe != null)
                {
                    string originalValue = probe.Content;
                    string marker = "smoke-test-" + DateTime.UtcNow.Ticks;
                    probe.Content = originalValue + "\n" + marker;
                    Console.WriteLine("\nSaving round-trip change on field '" + probe.Name + "'...");
                    first.Save();

                    // Re-read and verify
                    var refreshed = (Umbraco14xItem)((Umbraco14xItem)content).GetItem(first.ID);
                    var refreshedField = refreshed.Fields.FirstOrDefault(f =>
                        string.Equals(f.Name, probe.Name, StringComparison.OrdinalIgnoreCase));
                    bool roundTripped = refreshedField != null && refreshedField.Content.Contains(marker);
                    Console.WriteLine("Round-trip " + (roundTripped ? "OK" : "FAILED"));

                    // Restore original
                    refreshedField.Content = originalValue;
                    refreshed.Save();
                    return roundTripped ? 0 : 4;
                }
                Console.WriteLine("\nNo text field found on first item to probe round-trip; tree + field read succeeded.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("SMOKE TEST FAILED: " + ex);
                return 1;
            }
        }

        private static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
