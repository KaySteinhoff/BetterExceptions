using System;
using System.Threading.Tasks;

namespace BetterExceptions.Interfaces
{
    public interface IUpdateHandler
    {
        Version GetLatestVersion();
        Version[] GetVersionList();
        Task<bool> InstallVersion(Version version);
    }
}
