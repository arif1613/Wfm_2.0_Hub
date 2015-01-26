using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;
using WamsApi.Tests.Shared;

namespace WamsApi.Tests
{
    public class when_encoding_an_asset : TestBase
    {
        protected static IAsset _encodedAsset;
        protected static string _encodingIdentifier;
        protected static IAsset _thumnbNailAsset;
        protected static IAsset _asset;
        protected static IJob _job;
        protected static string _encodingPreset;
        protected static IAccessPolicy _policy;

        private Establish context = () =>
            {
                _encodingIdentifier = Guid.NewGuid().ToString("N");
                var sasContainer = MediaApi.CreateIngestFolderSasLocator("andreastest",
                                        "iBe8sh4Q6QSbSfySLrVDPZishY0GIo0avw8BfjOe4RyOpWBRkcj39myF7BP1y+Xc4tqaW6A/lEfguPrpuz4Czg==",
                                        "ingest").Result;

                _asset = MediaApi.CopyBlobToWams("azure.wmv", sasContainer, "VodTest", ConnectionString).Result;
                _encodingPreset = "H264 Smooth Streaming 720p";

            };

        private const string ThumbSetting = "<?xml version=\"1.0\" encoding=\"utf-16\"?><Thumbnail Size=\"100%,*\" Type=\"Jpeg\" Filename=\"{OriginalFilename}_{ThumbnailIndex}.{DefaultExtension}\"><Time Value=\"0:0:4\"/></Thumbnail>";

        private Because of = () =>
            {
                _job = MediaApi.StartEncodingAsset(_encodingIdentifier, _asset, _encodingPreset, ThumbSetting, null, ConnectionString).Result;

                while (!MediaApi.JobIsDone(_job))
                {
                    Task.Delay(TimeSpan.FromSeconds(5)).Await();
                    _job = MediaApi.GetJob(_job.Id, ConnectionString).Result;
                }
                if (_job.State == JobState.Finished)
                {
                    _encodedAsset = _job.OutputMediaAssets[0];
                    _thumnbNailAsset = _job.OutputMediaAssets[1];
                    var policyName = Guid.NewGuid().ToString("N");
                    var policyId = MediaApi.CreateAccessPolicy(policyName, TimeSpan.FromHours(1), AccessPermissions.Read, ConnectionString).Result;
                    _policy = MediaApi.GetAccessPolicyById(policyId, ConnectionString).Result;
                    string url = MediaApi.PublishAsset(_thumnbNailAsset, _policy, ConnectionString).Await();
                    Console.WriteLine(url);
                }
            };

        private Behaves_like<behaves_like_an_encoded_result> encoded_asset_result_check;

        private Cleanup clean = () =>
            {
                if (_asset != null)
                {
                    _asset.Delete();
                }
                if (_encodedAsset != null)
                {
                    _encodedAsset.Delete();
                }
                if (_thumnbNailAsset != null)
                {
                    _thumnbNailAsset.Delete();
                }
                if (_job != null)
                {
                    _job.Delete();
                }
                if (_policy != null)
                {
                    _policy.Delete();
                }
                MediaApi.RemoveIngestFolderSasPermissions("andreastest",
                                          "iBe8sh4Q6QSbSfySLrVDPZishY0GIo0avw8BfjOe4RyOpWBRkcj39myF7BP1y+Xc4tqaW6A/lEfguPrpuz4Czg==",
                                          "ingest").Await();
            };

    }
}
