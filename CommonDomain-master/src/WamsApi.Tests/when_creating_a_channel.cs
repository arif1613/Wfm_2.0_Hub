using System;
using System.Linq;
using Machine.Specifications;

namespace WamsApi.Tests
{
    public class when_creating_a_channel : TestBase
    {
        private static string _name;
        private static string _channelId;

        private Establish context = () =>
            {
                _name = Guid.NewGuid().ToString("N");
            };

        private Because of = () =>
            {
                _channelId = MediaApi.CreateChannel(_name, ConnectionString).Await();
            };

        private It a_new_channel_should_be_created_with_that_name =
            () => CloudMediaContext.Channels.Where(c => c.Name == _name).Count().ShouldEqual(1);

        private It the_proper_channel_id_should_be_returned =
            () => CloudMediaContext.Channels.Where(c => c.Name == _name).First().Id.ShouldEqual(_channelId);

        private Cleanup cleanup =
            () =>
                {
                    if (CloudMediaContext.Channels.Where(c => c.Name == _name).Count() > 0)
                    {
                        CloudMediaContext.Channels.Where(c => c.Name == _name).First().DeleteAsync();
                    }
                };
    }
}
