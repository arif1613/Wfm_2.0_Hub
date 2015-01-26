using System;
using System.Threading.Tasks;
using CommonDomainLibrary;
using FakeItEasy;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class encoding_tests
    {
        private static AzureMediaApi _mediaApi;
        private static IAsset _asset;
        private static IJob _job;

        private const string ThumbSetting = "<?xml version=\"1.0\" encoding=\"utf-16\"?><Thumbnail Size=\"100%,*\" Type=\"Jpeg\" Filename=\"{OriginalFilename}_{ThumbnailIndex}.{DefaultExtension}\"><Time Value=\"0:0:4\"/></Thumbnail>";
        private const string MediaPackagerMp4ValidationConfig = "<taskDefinition xmlns=\"http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#\"><name>MP4 Preprocessor</name><id>859515BF-9BA3-4BDD-A3B6-400CEF07F870</id><description xml:lang=\"en\" /><inputFolder /><properties namespace=\"http://schemas.microsoft.com/iis/media/V4/TM/MP4Preprocessor#\" prefix=\"mp4p\"><property name=\"SmoothRequired\" value=\"true\" /><property name=\"HLSRequired\" value=\"true\" /></properties><taskCode><type>Microsoft.Web.Media.TransformManager.MP4PreProcessor.MP4Preprocessor_Task, Microsoft.Web.Media.TransformManager.MP4Preprocessor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type></taskCode></taskDefinition>";

        private Establish context = () =>
            {
                var AccountName = "kumo2southeastasia";
                var AccountKey = "V1Vpe8evvEtsiatWzRrVSmUMFqAmMYUQOYR2g+nO8VQ=";
                var CloudMediaContext = new CloudMediaContext(AccountName, AccountKey);
                _connectionString = "{'account':'" + AccountName + "','key':'" + AccountKey + "'}";

                var connectionClientService = A.Fake<IConnectionClientService>();
                A.CallTo(
                    () => connectionClientService.WamsClient(_connectionString))
                 .Returns(new CloudMediaContext(AccountName, AccountKey));

                _mediaApi = new AzureMediaApi(connectionClientService);

                _asset = _mediaApi.GetAssetByName("upload asset 1c315ae7-0b79-4597-d94c-8a6a59340a9b", _connectionString).Result;

                _job = _mediaApi.StartEncodingAsset("MyId", _asset, "H264 Adaptive Bitrate MP4 Set 720p", ThumbSetting, MediaPackagerMp4ValidationConfig,
                                             _connectionString).Result;
            };

        private Because of = () =>
            {
                while (!_mediaApi.JobIsDone(_job))
                {
                    Task.Delay(TimeSpan.FromSeconds(5)).Await();
                    _job = _mediaApi.GetJob(_job.Id, _connectionString).Result;
                }
                if (_job.State == JobState.Finished || _job.State == JobState.Error)
                {
                    Console.WriteLine(_job);
                }

            };

        private It should = () => true.ShouldBeTrue();
        private static string _connectionString;
    }
}
