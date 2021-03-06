﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace WamsApi.Tests
{
    public class when_stopping_a_program : TestBase
    {
        private static IProgram _program;
        private static IAsset _asset;
        private static IChannel _channel;

        private Establish context = () =>
        {
            _channel = CloudMediaContext.Channels.CreateAsync(Guid.NewGuid().ToString("N"), ChannelSize.Large, new ChannelSettings()
            {
                Ingest = new IngestEndpointSettings()
                {
                    Security = new IngestEndpointSecuritySettings()
                    {
                        IPv4AllowList = new List<Ipv4>()
                                            {
                                                new Ipv4(){IP = "0.0.0.0/0", Name = "my"}
                                            }
                    }
                }
            }).Result;
            _channel.StartAsync().Await();
            _asset =
                CloudMediaContext.Assets.CreateAsync(Guid.NewGuid().ToString("N"), AssetCreationOptions.None,
                                                          new CancellationToken()).Result;
            _program = _channel.Programs.CreateAsync(Guid.NewGuid().ToString("N"), "", true, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), _asset.Id).Result;
            _program.StartAsync().Await();
        };

        private Because of = () => MediaApi.StopProgram(_program.Id, ConnectionString).Await();

        private It the_program_should_be_started =
            () => CloudMediaContext.Programs.Where(p => p.Id == _program.Id).First().State.ShouldEqual(ProgramState.Stopped);

        private Cleanup cleanup =
            () =>
            {
                if (_program != null)
                {
                    _program.StopAsync().Await();
                    _program.DeleteAsync().Await();
                }
                if (_asset != null) _asset.DeleteAsync().Await();
                if (_channel != null)
                {
                    _channel.StopAsync().Await();
                    _channel.DeleteAsync().Await();
                }
            };
    }
}
