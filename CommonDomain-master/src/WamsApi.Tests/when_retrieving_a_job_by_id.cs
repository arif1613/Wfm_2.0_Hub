using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_retrieving_a_job_by_id : TestBase
    {
        private static IJob _job;
        private static IJob _originalJob;
        private static IAsset _asset;

        private Establish context = () =>
        {
            var encodingPreset = "H264 Smooth Streaming 720p";
            _asset = CloudMediaContext.Assets.Create("someAssetName", AssetCreationOptions.None);
            _asset.AssetFiles.Create("azure.wmv");
            _originalJob = MediaApi.StartEncodingAsset("someIdentifier", _asset, encodingPreset, "", null, ConnectionString).Result;
        };

        private Because of = () => _job = MediaApi.GetJob(_originalJob.Id, ConnectionString).Result;

        private It the_right_job_should_be_returned = () => _job.Id.ShouldEqual(_originalJob.Id);

        private Cleanup cleanup = () =>
            {
                if (_originalJob != null)
                {
                    _originalJob.Delete();
                }
                if (_asset != null)
                {
                    _asset.Delete();
                }
            };
    }
}
