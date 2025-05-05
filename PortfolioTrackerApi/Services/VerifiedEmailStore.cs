using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace PortfolioTrackerApi.Services
{
    public static class VerifiedEmailStore
    {
        private static readonly string FilePath = 
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "verified_emails.json");
        private static readonly ReaderWriterLockSlim Lock = new();
        private static HashSet<string> _verifiedEmails = new();

        static VerifiedEmailStore()
        {
            EnsureDataFolderExists();
            LoadVerifiedEmailsFromDisk();
        }

        private static void EnsureDataFolderExists()
        {
            var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
        }

        private static void LoadVerifiedEmailsFromDisk()
        {
            Lock.EnterWriteLock();
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    _verifiedEmails = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
                }
                else
                {
                    _verifiedEmails = new HashSet<string>();
                }
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public static void SaveVerifiedEmail(string email)
        {
            var normalizedEmail = email.ToLowerInvariant();
            Lock.EnterWriteLock();
            try
            {
                if (_verifiedEmails.Add(normalizedEmail))
                {
                    var json = JsonSerializer.Serialize(_verifiedEmails, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(FilePath, json);
                }
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        public static bool IsEmailVerified(string email)
        {
            var normalizedEmail = email.ToLowerInvariant();
            Lock.EnterReadLock();
            try
            {
                return _verifiedEmails.Contains(normalizedEmail);
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }
    }
}
