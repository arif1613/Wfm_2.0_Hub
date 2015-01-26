using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    [Behaviors]
    class behaves_like_an_asset
    {
        protected static IAsset _originalAsset;
        protected static IAsset _asset;
        protected static List<IAssetFile> _assetFiles;

        private It the_alternate_id_is_the_same_as_the_original_asset =
            () => _asset.AlternateId.ShouldEqual(_originalAsset.AlternateId);
        private It the_asset_files_are_the_same = () =>
            {
                foreach (var assetFile in _assetFiles)
                {
                   _assetFiles.ShouldContain(assetFile); 
                }
            };
        private It the_id_is_the_same = () => _asset.Id.ShouldEqual(_originalAsset.Id);
        private It the_name_is_the_same = () => _asset.Name.ShouldEqual(_originalAsset.Name);
        private It the_state_is_the_same = () => _asset.State.ShouldEqual(_originalAsset.State);
        private It the_uri_is_the_same = () => _asset.Uri.ShouldEqual(_originalAsset.Uri);
    }
}
