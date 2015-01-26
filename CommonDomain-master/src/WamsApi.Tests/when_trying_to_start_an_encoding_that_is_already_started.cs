using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_trying_to_start_an_encoding_that_is_already_started : TestBase
    {
        private static string _encodingIdentifier;
        private static IAsset _asset;
        private static IJob _job;
        private static string _encodingPreset;

        private Establish context = () =>
            {
                _encodingIdentifier = Guid.NewGuid().ToString("N");
                _encodingPreset = "H264 Smooth Streaming 720p";
                _asset = CloudMediaContext.Assets.Create("someAssetName", AssetCreationOptions.None);
                _asset.AssetFiles.Create("azure.wmv");
                _job = MediaApi.StartEncodingAsset(_encodingIdentifier, _asset, _encodingPreset, ThumbSetting, null, ConnectionString).Result;
            };

        private const string ThumbSetting = "<?xml version=\"1.0\" encoding=\"utf-16\"?><Thumbnail Size=\"100%,*\" Type=\"Jpeg\" Filename=\"{OriginalFilename}_{ThumbnailIndex}.{DefaultExtension}\"><Time Value=\"0:0:4\"/></Thumbnail>";

        private Because of = () =>
            {
                _job = MediaApi.StartEncodingAsset(_encodingIdentifier, _asset, _encodingPreset, ThumbSetting, null, ConnectionString).Result;
            };

        private It no_new_encoding_job_should_be_created = () =>
            {
                var noJobs = CloudMediaContext.Jobs.Where(j => j.Name == _encodingIdentifier).Count();
                noJobs.ShouldEqual(1);
            };

        private Cleanup clean = () =>
            {
                if (_asset != null)
                {
                    _asset.Delete();
                }
                if (_job != null)
                {
                    _job.Delete();
                }
            };

    }
}
