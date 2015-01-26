using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;
using WamsApi.Tests.Shared;

namespace WamsApi.Tests
{
    class when_encoding_to_a_mp4_adaptive_asset : TestBase
    {
        protected static IAsset _encodedAsset;
        protected static string _encodingIdentifier;
        protected static IAsset _thumnbNailAsset;
        protected static IAsset _asset;
        protected static IJob _job;
        protected static string _encodingPreset;
        protected static IAsset _packagedAsset;

        private Establish context = () =>
        {
            _encodingIdentifier = Guid.NewGuid().ToString("N");
            var sasContainer = MediaApi.CreateIngestFolderSasLocator("andreastest",
                                    "iBe8sh4Q6QSbSfySLrVDPZishY0GIo0avw8BfjOe4RyOpWBRkcj39myF7BP1y+Xc4tqaW6A/lEfguPrpuz4Czg==",
                                    "ingest").Result;

            _asset = MediaApi.CopyBlobToWams("azure.wmv", sasContainer, "VodTest", ConnectionString).Result;
            _encodingPreset = "H264 Adaptive Bitrate MP4 Set 720p";

        };

        private const string ThumbSetting = "<?xml version=\"1.0\" encoding=\"utf-16\"?><Thumbnail Size=\"100%,*\" Type=\"Jpeg\" Filename=\"{OriginalFilename}_{ThumbnailIndex}.{DefaultExtension}\"><Time Value=\"0:0:4\"/></Thumbnail>";
        private const string MediaPackagerMp4ValidationConfig = "<taskDefinition xmlns=\"http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#\"><name>MP4 Preprocessor</name><id>859515BF-9BA3-4BDD-A3B6-400CEF07F870</id><description xml:lang=\"en\" /><inputFolder /><properties namespace=\"http://schemas.microsoft.com/iis/media/V4/TM/MP4Preprocessor#\" prefix=\"mp4p\"><property name=\"SmoothRequired\" value=\"true\" /><property name=\"HLSRequired\" value=\"true\" /></properties><taskCode><type>Microsoft.Web.Media.TransformManager.MP4PreProcessor.MP4Preprocessor_Task, Microsoft.Web.Media.TransformManager.MP4Preprocessor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type></taskCode></taskDefinition>";

        private Because of = () =>
        {
            _job = MediaApi.StartEncodingAsset(_encodingIdentifier, _asset, _encodingPreset, ThumbSetting, MediaPackagerMp4ValidationConfig, ConnectionString).Result;

            while (!MediaApi.JobIsDone(_job))
            {
                Task.Delay(TimeSpan.FromSeconds(5)).Await();
                _job = MediaApi.GetJob(_job.Id, ConnectionString).Result;
            }
            if (_job.State == JobState.Finished)
            {
                _encodedAsset = _job.OutputMediaAssets[0];
                _thumnbNailAsset = _job.OutputMediaAssets[1];
                _packagedAsset = _job.OutputMediaAssets[2];
            }

        };

        private Behaves_like<behaves_like_an_encoded_result> encoded_asset_result_check;

        private It the_asset_should_be_validated_by_the_media_packager = () => _packagedAsset.ShouldNotBeNull();

        private It the_packaged_asset_has_the_same_name_as_the_source_asset =
            () => _encodedAsset.Name.ShouldEqual(_asset.Name);

        private It the_packaged_asset_should_contain_a_manifest_file =
            () => _encodedAsset.AssetFiles.Where(f => f.Name.EndsWith(".ism")).FirstOrDefault().ShouldNotBeNull();

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
            if (_packagedAsset != null)
            {
                _packagedAsset.Delete();
            }
            if (_job != null)
            {
                _job.Delete();
            }
            MediaApi.RemoveIngestFolderSasPermissions("andreastest",
                                      "iBe8sh4Q6QSbSfySLrVDPZishY0GIo0avw8BfjOe4RyOpWBRkcj39myF7BP1y+Xc4tqaW6A/lEfguPrpuz4Czg==",
                                      "ingest").Await();
        };

    }
}
