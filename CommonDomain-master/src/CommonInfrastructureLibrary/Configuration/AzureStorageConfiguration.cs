using Microsoft.WindowsAzure;

namespace CommonInfrastructureLibrary.Configuration
{
    public class AzureStorageConfiguration : IStorageConfiguration
    {
        public string ConnectionString
        {
            //get { return "DefaultEndpointsProtocol=https;AccountName=atstest;AccountKey=CJpU9bjBJAx6+hgtQ0SU20fMOmK88UzMQNU0HymcwWUXq0LXnGcloU60YVzaCOniV5Wi9sikXuN+4K8mHMtyQQ=="; } //Prod
            //get { return "DefaultEndpointsProtocol=https;AccountName=kumo2test;AccountKey=2dmUC8x6AHagAqk7nUFSaMBkuiTydHg2u6oXvMR6RvH+JHFShozFdFDkXgwYMkBdtqPTE/0NZJq9Q/C0d+h7Gw=="; } //Test
            //get { return "UseDevelopmentStorage=true"; }
            //RoleEnvironment.GetConfigurationSettingValue("Storage.ConnectionString"); }
            get { return CloudConfigurationManager.GetSetting("AzureStorageAcountConnectionString"); }
        }
    }
}