using System;
using System.IO;
using JetBrains.Application;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

public interface IReqnrollUserIdStore
{
    string GetUserId();
}
    
[ShellComponent]
public class ReqnrollUserIdStore : IReqnrollUserIdStore
{
    private static readonly string AppDataFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
    public static readonly string UserIdFilePath = Path.Combine(AppDataFolder, "Reqnroll", "userid");

    private readonly Lazy<string> _lazyUniqueUserId;

    public ReqnrollUserIdStore()
    {
        _lazyUniqueUserId = new Lazy<string>(FetchAndPersistUserId);
    }

    public string GetUserId()
    {
        return _lazyUniqueUserId.Value;
    }

    private string FetchAndPersistUserId()
    {
        if (File.Exists(UserIdFilePath))
        {
            var userIdStringFromFile = File.ReadAllText(UserIdFilePath);
            if (!userIdStringFromFile.IsNullOrEmpty() && IsValidGuid(userIdStringFromFile))
            {
                return userIdStringFromFile;
            }
        }

        return GenerateAndPersistUserId();
    }

    private string GenerateAndPersistUserId()
    {
        var newUserId = Guid.NewGuid().ToString();

        PersistUserId(newUserId);

        return newUserId;
    }

    private void PersistUserId(string userId)
    {
        var directoryName = Path.GetDirectoryName(UserIdFilePath);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName.NotNull());
        }

        File.WriteAllText(UserIdFilePath, userId);
    }

    private bool IsValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }
}