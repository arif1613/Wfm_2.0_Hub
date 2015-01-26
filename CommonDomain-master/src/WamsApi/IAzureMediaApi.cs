using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi
{
    public interface IAzureMediaApi
    {
        Task<string> CreateOrigin(string name, int units, string mpp5ConnectionString);
        Task DeleteOrigin(string id, string mpp5ConnectionString);
        Task<IOrigin> GetOriginById(string id, string mpp5ConnectionString);
        Task<IOrigin> GetOriginByName(string name, string mpp5ConnectionString);
        Task StartOrigin(string id, string mpp5ConnectionString);
        Task StopOrigin(string id, string mpp5ConnectionString);
        Task ScaleOrigin(string id, int size, string mpp5ConnectionString);
        Task<string> CreateChannel(string name, string mpp5ConnectionString);
        Task DeleteChannel(string id, string mpp5ConnectionString);
        Task<IChannel> GetChannelById(string id, string mpp5ConnectionString);
        Task<IChannel> GetChannelByName(string name, string mpp5ConnectionString);
        Task StartChannel(string id, string mpp5ConnectionString);
        Task StopChannel(string id, string mpp5ConnectionString);
        Task<string> CreateAsset(string name, string mpp5ConnectionString);
        Task DeleteAsset(string id, string mpp5ConnectionString);
        Task<IAsset> GetAssetById(string id, string mpp5ConnectionString);
        Task<IAsset> GetAssetByName(string name, string mpp5ConnectionString);
        Task<string> CreateProgram(string channelId, string name, TimeSpan duration, string assetId, bool archived, string mpp5ConnectionString);
        Task DeleteProgram(string id, string mpp5ConnectionString);
        Task<IProgram> GetProgramById(string id, string mpp5ConnectionString);
        Task<IProgram> GetProgramByName(string name, string mpp5ConnectionString);
        Task StartProgram(string id, string mpp5ConnectionString);
        Task StopProgram(string id, string mpp5ConnectionString);
        Task<string> CreateAccessPolicy(string name, TimeSpan duration, AccessPermissions permissions, string mpp5ConnectionString);
        Task DeleteAccessPolicy(string id, string mpp5ConnectionString);
        Task<IAccessPolicy> GetAccessPolicyById(string id, string mpp5ConnectionString);
        Task<IAccessPolicy> GetAccessPolicyByName(string name, string mpp5ConnectionString);
        Task<string> CreateLocator(string assetId, string policyId, string mpp5ConnectionString);
        Task DeleteLocator(string id, string mpp5ConnectionString);
        Task<ILocator> GetLocatorById(string id, string mpp5ConnectionString);
        Task<ILocator> GetLocator(string assetId, string policyId, string mpp5ConnectionString);
        Task<ILocator> CreateAssetWriteLocation(IAsset asset, IAccessPolicy writePolicy, String mpp5ConnectionString);
        Task<IAssetFile> CreateAssetFile(String assetId, String assetFileName, bool isPrimaryFile, String mpp5ConnectionString);
        Task<String> PublishVodAsset(IAsset asset, IAccessPolicy policy, string mpp5ConnectionString);
        Task<IJob> StartEncodingAsset(String encodingId, IAsset asset, String preset,
                                      String thumbnailGenerationXml, String packageXml, string mpp5ConnectionString);
        Task<IJob> GetJob(string jobId, string mpp5ConnectionString);
        Task<String> PublishAsset(IAsset asset, IAccessPolicy policy, string mpp5ConnectionString);
    }
}