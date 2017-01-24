using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PeanutButter.Utils;

namespace HyperLeech.Tests
{
    internal class TestSecrets
    {
        public string Site { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
    }

    internal static class SecretFetcher
    {
        public static TestSecrets GetSecrets()
        {
            var localPath = new Uri(typeof(SecretFetcher).Assembly.CodeBase).LocalPath;
            var localFolder = Path.GetDirectoryName(localPath);
            var secretsFile = Path.Combine(localFolder, "secrets.txt");
            if (!File.Exists(secretsFile))
            {
                throw new Exception("No secrets found! Go make your own!");
            }
            var result = new TestSecrets();
            var actions = new Dictionary<string, Action<string>>()
            {
                {"user", v => result.User = v},
                {"pass", v => result.Pass = v},
                {"site", v => result.Site = v}
            };
            File.ReadAllLines(secretsFile).ForEach(line =>
            {
                var parts = line.Split(':');
                var key = Enumerable.First<string>(parts).Trim();
                Action<string> toRun;
                if (!actions.TryGetValue(key, out toRun))
                    return;
                var value = string.Join(":", Enumerable.Skip<string>(parts, 1)).Trim();
                toRun(value);
            });

            return result;
        }
    }
}