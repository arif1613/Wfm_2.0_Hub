using System.Linq;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests.Shared
{
    [Behaviors]
    class behaves_like_an_encoded_result
    {
        protected static IAsset _encodedAsset;
        protected static IAsset _thumnbNailAsset;
        protected static IAsset _asset;
        protected static IJob _job;
        
        private It the_encoding_job_should_finish = () => _job.State.ShouldEqual(JobState.Finished);

        private It an_encoded_asset_should_be_created = () => _encodedAsset.ShouldNotBeNull();

        private It the_encoded_asset_has_the_same_name_as_the_source_asset =
            () => _encodedAsset.Name.ShouldEqual(_asset.Name);

        private It the_encoded_asset_should_contain_a_manifest_file =
            () => _encodedAsset.AssetFiles.Where(f => f.Name.EndsWith(".ism")).FirstOrDefault().ShouldNotBeNull();

        private It a_thumbnail_asset_should_be_created = () => _thumnbNailAsset.ShouldNotBeNull();

        private It the_thumbnail_asset_should_contain_an_image = () =>
        {
            _thumnbNailAsset.AssetFiles.Where(
                f =>
                f.Name.EndsWith(".jpg"))
                            .FirstOrDefault()
                            .ShouldNotBeNull();
        };

        private It the_thumbnail_asset_has_the_correct_name =
            () => _thumnbNailAsset.Name.ShouldEqual(_asset.Name + "_thumbs");
    }
}
