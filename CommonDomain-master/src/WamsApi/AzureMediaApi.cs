using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonDomainLibrary;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using NLog;

namespace WamsApi
{
    public class AzureMediaApi : IAzureMediaApi
    {
        private readonly IConnectionClientService _connectionClientService;

        private static readonly RetryPolicy RetryPolicy = new RetryPolicy<TransientErrorDetectionStrategy>(
            new ExponentialBackoff("Retry policy", 5, TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(30), true));
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AzureMediaApi(IConnectionClientService connectionClientService)
        {
            _connectionClientService = connectionClientService;
        }

        public async Task<string> CreateOrigin(string name, int units, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Creating origin with name '{0}' and '{1}' units", name, units);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IOrigin origin = null;
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        origin =
                            await
                            RetryPolicy.ExecuteAsync(
                                async () =>
                                await
                                Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Name == name).First()));
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorException("Error checking for origin existance", ex);
                        throw;
                    }

                    if (origin != null)
                    {
                        return;
                    }
                    origin = await cloudMediaContext.Origins.CreateAsync(name, units);
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed creating origin with name '{0}' and '{1}' units", name, units), ex);
                throw;
            }
            Logger.Info("END: Creating origin with name '{0}' and '{1}' units", name, units);
            return origin.Id;
        }

        public async Task DeleteOrigin(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Deleting origin with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IOrigin origin;
                    try
                    {
                        origin =
                            await
                            RetryPolicy.ExecuteAsync(
                                async () =>
                                await
                                Task.Factory.StartNew(
                                    () => cloudMediaContext.Origins.Where(o => o.Id == id).First()));
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    if (origin != null)
                    {
                        await origin.DeleteAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed deleting origin with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Deleting origin with id '{0}'", id);
        }

        public async Task<IOrigin> GetOriginById(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving origin with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IOrigin origin;
            try
            {
                origin =
                await
                RetryPolicy.ExecuteAsync(
                    async () =>
                    await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Id == id).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Origin with id '{0}' not found", id);
                Logger.Info("END: Retrieving origin with id '{0}'", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving origin with id '{0}'", id), ex);
                throw;
            }

            Logger.Info("END: Retrieving origin with id '{0}'", id);
            return origin;
        }

        public async Task<IOrigin> GetOriginByName(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving origin with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IOrigin origin;
            try
            {
                origin =
                await
                RetryPolicy.ExecuteAsync(
                    async () =>
                    await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Name == name).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Origin with name '{0}' not found", name);
                Logger.Info("END: Retrieving origin with name '{0}'", name);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving origin with name '{0}'", name), ex);
                throw;
            }

            Logger.Info("END: Retrieving origin with name '{0}'", name);
            return origin;
        }

        public async Task StartOrigin(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Starting origin with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IOrigin origin;

                    origin =
                    await
                    RetryPolicy.ExecuteAsync(
                        async () =>
                        await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Id == id).First()));


                    if (origin.State == OriginState.Running)
                    {
                        return;
                    }
                    if (origin.State == OriginState.Starting)
                    {
                        while (origin.State != OriginState.Running)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));

                            origin =
                            await
                            RetryPolicy.ExecuteAsync(
                                async () =>
                                await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Id == id).First()));
                        }
                        return;
                    }

                    await origin.StartAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed starting origin with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Starting origin with id '{0}'", id);
        }

        public async Task StopOrigin(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Stopping origin with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IOrigin origin = await
                                     RetryPolicy.ExecuteAsync(
                                         async () =>
                                         await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Id == id).First()));


                    if (origin.State == OriginState.Stopped)
                    {
                        return;
                    }
                    if (origin.State == OriginState.Stopping)
                    {
                        while (origin.State != OriginState.Stopped)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));

                            origin =
                            await
                            RetryPolicy.ExecuteAsync(
                                async () =>
                                await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Id == id).First()));
                        }
                        return;
                    }

                    await origin.StopAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed stopping origin with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Stopping origin with id '{0}'", id);
        }

        public async Task ScaleOrigin(string id, int size, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Scaling origin with id '{0}' to new size '{1}'", id, size);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IOrigin origin = await
                                     RetryPolicy.ExecuteAsync(
                                         async () =>
                                         await Task.Factory.StartNew(() => cloudMediaContext.Origins.Where(o => o.Id == id).First()));

                    await origin.ScaleAsync(size);
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed scaling origin with id '{0}' to new size '{1}'", id, size), ex);
                throw;
            }
            Logger.Info("END: Scaling origin with id '{0}' to new size '{1}'", id, size);
        }

        public async Task<string> CreateChannel(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Creating channel with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IChannel channel = null;
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Name == name).First()));
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    if (channel != null)
                    {
                        return;
                    }

                    channel = await cloudMediaContext.Channels.CreateAsync(name, ChannelSize.Large, new ChannelSettings
                    {
                        Ingest = new IngestEndpointSettings
                        {
                            Security = new IngestEndpointSecuritySettings
                            {
                                IPv4AllowList = new List<Ipv4>
                                            {
                                                new Ipv4 {IP = "0.0.0.0/0", Name = "my"}
                                            }
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create channel with name '{0}'", name), ex);
                throw;
            }
            Logger.Info("END: Creating channel with name '{0}'", name);
            return channel.Id;
        }

        public async Task DeleteChannel(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Deleting channel with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IChannel channel;
                    try
                    {
                        channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == id).First()));
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    if (channel != null)
                    {
                        await channel.DeleteAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed deleting channel with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Deleting channel with id '{0}'", id);
        }

        public async Task<IChannel> GetChannelById(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving channel with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IChannel channel;
            try
            {
                channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == id).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Channel with id '{0}' not found", id);
                Logger.Info("END: Retrieving channel with id '{0}'", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving channel information for id '{0}'", id), ex);
                throw;
            }

            Logger.Info("END: Retrieving channel with id '{0}'", id);
            return channel;
        }

        public async Task<IChannel> GetChannelByName(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving channel with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IChannel channel;
            try
            {
                channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Name == name).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Channel with name '{0}' not found", name);
                Logger.Info("END: Retrieving channel with name '{0}'", name);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving channel information for name '{0}'", name), ex);
                throw;
            }

            Logger.Info("END: Retrieving channel with name '{0}'", name);
            return channel;
        }

        public async Task StartChannel(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Starting channel with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IChannel channel;
                    channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == id).First()));

                    if (channel.State == ChannelState.Running)
                    {
                        return;
                    }
                    if (channel.State == ChannelState.Starting)
                    {
                        while (channel.State != ChannelState.Running)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));
                            channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == id).First()));
                        }
                        return;
                    }

                    await channel.StartAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed starting channel with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Starting channel with id '{0}'", id);
        }

        public async Task StopChannel(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Stopping channel with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IChannel channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == id).First()));

                    if (channel.State == ChannelState.Stopped)
                    {
                        return;
                    }
                    if (channel.State == ChannelState.Stopping)
                    {
                        while (channel.State != ChannelState.Stopped)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));

                            channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == id).First()));
                        }
                        return;
                    }

                    await channel.StopAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed stopping channel with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Stopping channel with id '{0}'", id);
        }

        public async Task<string> CreateAsset(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Creating asset with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IAsset asset = null;
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        asset = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Assets.Where(c => c.Name == name).FirstOrDefault()));
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    if (asset != null)
                    {
                        return;
                    }

                    asset = await cloudMediaContext.Assets.CreateAsync(name, AssetCreationOptions.None, new CancellationToken());
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create asset with name '{0}'", name), ex);
                throw;
            }
            Logger.Info("END: Creating asset with name '{0}'", name);
            return asset.Id;
        }

        public async Task DeleteAsset(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Deleting asset with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IAsset asset;
                    try
                    {
                        asset = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Assets.Where(c => c.Id == id).First()));
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    if (asset != null)
                    {
                        await asset.DeleteAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed deleting asset with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Deleting asset with id '{0}'", id);
        }

        public async Task<IAsset> GetAssetById(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving asset with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IAsset asset;
            try
            {
                asset = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Assets.Where(c => c.Id == id).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Asset with id '{0}' not found", id);
                Logger.Info("END: Retrieving asset with id '{0}'", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving asset information for id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Retrieving asset with id '{0}'", id);
            return asset;
        }

        public async Task<IAsset> GetAssetByName(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving asset with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IAsset asset;
            try
            {
                asset = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Assets.Where(c => c.Name == name).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Asset with name '{0}' not found", name);
                Logger.Info("END: Retrieving asset with name '{0}'", name);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving asset information for name '{0}'", name), ex);
                throw;
            }

            Logger.Info("END: Retrieving asset with name '{0}'", name);
            return asset;
        }

        private static TimeSpan GetDvrWindowLength(bool archive)
        {
            if (archive)
            {
                // Max DVR Window for archived events are 6h
                return TimeSpan.FromHours(6);
            }
            // Max DVR Window for events are 24h
            return TimeSpan.FromHours(24);
        }

        public async Task<string> CreateProgram(string channelId, string name, TimeSpan duration, string assetId, bool archived, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Creating program with name '{0}', duration '{1}', asset id '{2}' on channel with id '{3}'", name, duration, assetId, channelId);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IProgram program = null;
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IChannel channel = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Channels.Where(c => c.Id == channelId).First()));

                    try
                    {
                        program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => channel.Programs.Where(p => (p.Name == name && p.AssetId == assetId)).FirstOrDefault()));
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    if (program != null) return;

                    program = await channel.Programs.CreateAsync(name, "", archived, GetDvrWindowLength(archived), duration, assetId);
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create program with name '{0}', duration '{1}', asset id '{2}' on channel id '{3}'", name, duration, assetId, channelId), ex);
                throw;
            }
            Logger.Info("END: Creating program with name '{0}', duration '{1}', asset id '{2}' on channel with id '{3}'", name, duration, assetId, channelId);
            return program.Id;
        }

        public async Task DeleteProgram(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Deleting program with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IProgram program;
                    try
                    {
                        program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(p => p.Id == id).First()));
                    }
                    catch (InvalidOperationException)
                    {
                        Logger.Info("Program with id '{0}' not found", id);
                        return;
                    }

                    if (program != null) await program.DeleteAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed deleting program with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Deleting program with id '{0}'", id);
        }

        public async Task<IProgram> GetProgramById(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving program with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IProgram program;
            try
            {
                program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(c => c.Id == id).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Program with id '{0}' not found", id);
                Logger.Info("END: Retrieving program with id '{0}'", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving program information for id '{0}'", id), ex);
                throw;
            }

            if (program == null) return null;

            Logger.Info("END: Retrieving program with id '{0}'", id);
            return program;
        }

        public async Task<IProgram> GetProgramByName(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving program with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IProgram program;
            try
            {
                program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(c => c.Name == name).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Program with name '{0}' not found", name);
                Logger.Info("END: Retrieving program with name '{0}'", name);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving program information for name '{0}'", name), ex);
                throw;
            }

            Logger.Info("END: Retrieving program with name '{0}'", name);
            return program;
        }

        public async Task StartProgram(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Starting program with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IProgram program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(p => p.Id == id).First()));

                    if (program.State == ProgramState.Running) return;

                    if (program.State == ProgramState.Starting)
                    {
                        while (program.State != ProgramState.Running)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));

                            program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(p => p.Id == id).First()));
                        }
                        return;
                    }

                    var asset = await GetAssetById(program.AssetId, mpp5ConnectionString);
                    if (asset.AssetFiles.Count()>0)
                    {
                        foreach (var file in asset.AssetFiles)
                        {
                            await file.DeleteAsync();
                        }
                    }

                    await program.StartAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed starting program with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Starting program with id '{0}'", id);
        }

        public async Task StopProgram(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Stopping program with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IProgram program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(p => p.Id == id).First()));

                    if (program.State == ProgramState.Stopped)
                    {
                        return;
                    }
                    if (program.State == ProgramState.Stopping)
                    {
                        while (program.State != ProgramState.Stopped)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));

                            program = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Programs.Where(p => p.Id == id).First()));
                        }
                        return;
                    }

                    await program.StopAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed stopping program with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Stopping program with id '{0}'", id);
        }

        public async Task<string> CreateAccessPolicy(string name, TimeSpan duration, AccessPermissions permissions, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Creating access policy with name '{0}', duration '{1}', permissions '{2}'", name, duration, permissions);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IAccessPolicy accessPolicy = null;
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    accessPolicy = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.AccessPolicies.Where(p => p.Name == name).ToList().FirstOrDefault(p => p.Duration == duration && p.Permissions == permissions)));

                    if (accessPolicy != null) return;

                    accessPolicy = await cloudMediaContext.AccessPolicies.CreateAsync(name, duration, permissions);
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create access policy with name '{0}', duration '{1}', permissions '{2}'", name, duration, permissions), ex);
                throw;
            }
            Logger.Info("END: Creating access policy with name '{0}', duration '{1}', permissions '{2}'", name, duration, permissions);
            return accessPolicy.Id;
        }

        public async Task DeleteAccessPolicy(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Deleting access policy with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IAccessPolicy accessPolicy;
                    try
                    {
                        accessPolicy = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.AccessPolicies.Where(p => p.Id == id).First()));
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    if (accessPolicy != null)
                    {
                        await accessPolicy.DeleteAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed deleting policy with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Deleting access policy with id '{0}'", id);
        }

        public async Task<IAccessPolicy> GetAccessPolicyById(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving access policy with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IAccessPolicy accessPolicy;
            try
            {
                accessPolicy = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.AccessPolicies.Where(p => p.Id == id).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Read access with id '{0}' not found", id);
                Logger.Info("END: Retrieving access policy with id '{0}'", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving policy information for id '{0}'", id), ex);
                throw;
            }

            Logger.Info("END: Retrieving access policy with id '{0}'", id);

            return accessPolicy;
        }

        public async Task<IAccessPolicy> GetAccessPolicyByName(string name, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving access policy with name '{0}'", name);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            IAccessPolicy accessPolicy;
            try
            {
                accessPolicy = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.AccessPolicies.Where(p => p.Name == name).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Access policy with name '{0}' not found", name);
                Logger.Info("END: Retrieving access policy with name '{0}'", name);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving policy information for name '{0}'", name), ex);
                throw;
            }

            Logger.Info("END: Retrieving access policy with name '{0}'", name);

            return accessPolicy;
        }

        public async Task<string> CreateLocator(string assetId, string policyId, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Creating locator for asset id '{0}' with policy id '{1}'", assetId, policyId);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            ILocator locator = null;
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    IAccessPolicy accessPolicy = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.AccessPolicies.Where(p => p.Id == policyId).First()));

                    IAsset asset = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Assets.Where(c => c.Id == assetId).First()));

                    try
                    {
                        locator = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Locators.Where(l => l.AssetId == assetId && l.AccessPolicyId == policyId).First()));
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    if (locator != null)
                    {
                        return;
                    }

                    locator = await cloudMediaContext.Locators.CreateLocatorAsync(LocatorType.OnDemandOrigin, asset, accessPolicy);
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create locator for asset id '{0}' with policy id '{1}'", assetId, policyId), ex);
                throw;
            }
            Logger.Info("END: Creating locator for asset id '{0}' with policy id '{1}'", assetId, policyId);
            return locator.Id;
        }

        public async Task DeleteLocator(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Deleting locator with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    ILocator locator;
                    try
                    {
                        locator = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Locators.Where(l => l.Id == id).First()));
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    if (locator != null)
                    {
                        await locator.DeleteAsync();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Error deleting locator with id '{0}'", id), ex);
                throw;
            }
            Logger.Info("END: Deleting locator with id '{0}'", id);
        }

        public async Task<ILocator> GetLocatorById(string id, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving locator with id '{0}'", id);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            ILocator locator;
            try
            {
                locator = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Locators.Where(l => l.Id == id).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Locator with id '{0}' not found", id);
                Logger.Info("END: Retrieving locator with id '{0}'", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving locator information for id '{0}'", id), ex);
                throw;
            }

            Logger.Info("END: Retrieving locator with id '{0}'", id);
            return locator;
        }
        
        public async Task<ILocator> GetLocator(string assetId, string policyId, string mpp5ConnectionString)
        {
            Logger.Info("BEGIN: Retrieving locator with asset id '{0}' and policy id '{1}'", assetId, policyId);
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            ILocator locator;
            try
            {
                locator = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Locators.Where(l => l.AssetId == assetId && l.AccessPolicyId == policyId).First()));
            }
            catch (InvalidOperationException)
            {
                Logger.Info("Locator with asset id '{0}' and policy id '{1}' not found", assetId, policyId);
                Logger.Info("END: Retrieving locator with asset id '{0}' and policy id '{1}'", assetId, policyId);
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving locator information for asset id '{0}' and policy id '{1}'", assetId, policyId), ex);
                throw;
            }

            Logger.Info("END: Retrieving locator with asset id '{0}' and policy id '{1}'", assetId, policyId);
            return locator;
        }

        public async Task<IJob> StartEncodingAsset(String encodingId, IAsset asset, String preset, String thumbnailGenerationXml, string packageXml, string mpp5ConnectionString)
        {
            IAsset outputAsset = null;
            IAsset thumbAsset = null;
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                IJob job = await GetJobFromIdentifier(encodingId, cloudMediaContext);

                if (job != null)
                {
                    return job;
                }

                job = cloudMediaContext.Jobs.Create(encodingId);

                IMediaProcessor processor =
                    await
                    RetryPolicy.ExecuteAsync(
                        (async () =>
                         await
                         Task.Factory.StartNew(
                             () =>
                             cloudMediaContext.MediaProcessors.Where(p => p.Name == "Windows Azure Media Encoder")
                                               .ToList()
                                               .OrderBy(p => new Version(p.Version))
                                               .LastOrDefault())));
                ITask task = job.Tasks.AddNew(String.Format("Encoding task for asset {0}", asset.Id),
                                              processor,
                                              preset,
                                              TaskOptions.None);

                task.InputAssets.Add(asset);
                outputAsset = task.OutputAssets.AddNew(asset.Name, AssetCreationOptions.None);

                if (!string.IsNullOrEmpty(thumbnailGenerationXml))
                {
                    ITask thumbTask = job.Tasks.AddNew(String.Format("Thumbnail task for asset {0}", asset.Id),
                                                       processor,
                                                       thumbnailGenerationXml,
                                                       TaskOptions.None);

                    thumbTask.InputAssets.Add(asset);
                    thumbAsset = thumbTask.OutputAssets.AddNew(asset.Name + "_thumbs", AssetCreationOptions.None);
                }

                if (!string.IsNullOrEmpty(packageXml))
                {
                    await ValidateMP4Asset(job, outputAsset, cloudMediaContext, packageXml);
                }                

                await job.SubmitAsync();

                return job;
                
                //await WaitForJobToComplete(job.Id, mpp5ConnectionString);
                //job = await GetJob(job.Id, mpp5ConnectionString);
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed encoding asset {0} {1} with preset {2}", asset.Name, asset.Id, preset), ex);
                if (outputAsset != null)
                {
                    outputAsset.DeleteAsync();
                }
                if (thumbAsset != null)
                {
                    thumbAsset.DeleteAsync();
                }
                throw;
            }
            // Get a reference to the output asset from the job.
            //IAsset outputAsset = job.OutputMediaAssets[0];

            //return outputAsset;
        }

        private async Task<IJob> GetJobFromIdentifier(String encodingIdentifier, CloudMediaContext cloudMediaContext)
        {
            IJob job;
            try
            {
                job = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => cloudMediaContext.Jobs.Where(p => p.Name == encodingIdentifier).FirstOrDefault()));
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed retrieving job with name '{0}'", encodingIdentifier), ex);
                throw;
            }

            return job;

        }

        public async Task<IJob> GetJob(string jobId, string mpp5ConnectionString)
        {
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            var job =
                await
                RetryPolicy.ExecuteAsync(
                    (async () =>
                     await
                     Task.Factory.StartNew(
                         () => cloudMediaContext.Jobs.Where(j => j.Id == jobId))));

            var theJob = job.SingleOrDefault();

            if (theJob != null)
            {
                return theJob;
            }

            Logger.Info("Could not find job {0}", jobId);

            return null;
        }
       
        public bool JobIsDone(IJob job)
        {
            return job.State == JobState.Canceled || job.State == JobState.Error || job.State == JobState.Finished;
        }

        private async Task WaitForJobToComplete(string jobId, string mpp5ConnectionString)
        {
            await RetryPolicy.ExecuteAsync(async () =>
                {
                    IJob job = await GetJob(jobId, mpp5ConnectionString);

                    while (!JobIsDone(job))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));

                        job = await GetJob(jobId, mpp5ConnectionString);
                    }
            });
        }


        public async Task<String> PublishAsset(IAsset asset, IAccessPolicy policy, string mpp5ConnectionString)
        {
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                IAssetFile firstFile = asset.AssetFiles.FirstOrDefault();
                if (firstFile == null)
                {
                    throw new ArgumentException("No file found for asset " + asset.Name);
                }

                ILocator originLocator =
                    await
                    RetryPolicy.ExecuteAsync(
                        async () =>
                        await
                        Task.Factory.StartNew(
                            () =>
                            cloudMediaContext.Locators.Where(
                                l => l.AssetId == asset.Id && l.AccessPolicyId == policy.Id).FirstOrDefault()));

                if (originLocator == null)
                {
                    originLocator =
                        await cloudMediaContext.Locators.CreateLocatorAsync(LocatorType.Sas, asset,
                                                                            policy,
                                                                            DateTime.UtcNow.AddMinutes(-5));
                }

                return originLocator.BaseUri + "/" + firstFile.Name + originLocator.ContentAccessComponent;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed publishing asset {0} {1}", asset.Name, asset.Id), ex);
                throw;
            }   
        }

        public async Task<String> PublishVodAsset(IAsset asset, IAccessPolicy policy, string mpp5ConnectionString)
        {
            var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);
            try
            {
                IAssetFile manifestFile =
                    (await
                     RetryPolicy.ExecuteAsync(
                         async () =>
                         await Task.Factory.StartNew(() => asset.AssetFiles.Where(f => f.Name.EndsWith(".ism")))))
                        .SingleOrDefault();

                if (manifestFile == null)
                {
                    throw new ArgumentException("No manifest file found for asset " + asset.Name);
                }

                ILocator originLocator =
                    await
                    RetryPolicy.ExecuteAsync(
                        async () =>
                        await
                        Task.Factory.StartNew(
                            () =>
                            cloudMediaContext.Locators.Where(
                                l => l.AssetId == asset.Id && l.AccessPolicyId == policy.Id).FirstOrDefault()));

                if (originLocator == null)
                {
                    originLocator =
                        await cloudMediaContext.Locators.CreateLocatorAsync(LocatorType.OnDemandOrigin, asset,
                                                                            policy,
                                                                            DateTime.UtcNow.AddMinutes(-5));
                }

                return originLocator.Path + manifestFile.Name + "/manifest";
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed publishing asset {0} {1}", asset.Name, asset.Id), ex);
                throw;
            }
        }

        public class SasContainer
        {
            public Uri ContainerUri;
            public String SasToken;

            public String SasLocator
            {
                get { return ContainerUri + SasToken; }
            }
        }

        public async Task<SasContainer> CreateIngestFolderSasLocator(String storageName, String storageAccountKey,
                                                               String ingestFolderName)
        {
            try
            {
                var storageAccount = new CloudStorageAccount(new StorageCredentials(storageName, storageAccountKey),
                                                             true);
                var blobClient = storageAccount.CreateCloudBlobClient();

                var container = blobClient.GetContainerReference(ingestFolderName);
                await container.CreateIfNotExistsAsync();

                var sharedPolicy = new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.MaxValue,
                    Permissions =
                        SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write |
                        SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Delete
                };

                var permissions = new BlobContainerPermissions();
                permissions.SharedAccessPolicies.Clear();
                permissions.SharedAccessPolicies.Add(ingestFolderName, sharedPolicy);
                container.SetPermissions(permissions);

                var sasContainerToken = container.GetSharedAccessSignature(null, ingestFolderName);

                return new SasContainer { ContainerUri = container.Uri, SasToken = sasContainerToken };
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed creating SAS Locator {0} {1}", storageName, ingestFolderName), ex);
                throw;
            }
        }

        public async Task RemoveIngestFolderSasPermissions(String storageName, String storageAccountKey, String ingestFolderName)
        {
            try
            {
                var storageAccount = new CloudStorageAccount(new StorageCredentials(storageName, storageAccountKey),
                                             true);
                var blobClient = storageAccount.CreateCloudBlobClient();

                var container = blobClient.GetContainerReference(ingestFolderName);

                if (await container.ExistsAsync())
                {
                    await container.SetPermissionsAsync(new BlobContainerPermissions());
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed removing SAS Permissions {0} {1}", storageName, ingestFolderName), ex);
                throw;
            }
        }

        /*
        public async Task SetWamsCors(String storageName, String storageAccountKey, List<String> allowedOrigins)
        {
            //Add a new rule.
            var corsRule = new CorsRule()
            {
                AllowedHeaders = new List<string> { "x-ms-*", "content-type", "accept" },
                AllowedMethods = CorsHttpMethods.Put,//Since we'll only be calling Put Blob, let's just allow PUT verb
                AllowedOrigins = allowedOrigins,//This is the URL of our application.
                MaxAgeInSeconds = 1 * 60 * 60,//Let the browswer cache it for an hour
            };


<Cors>    
      <CorsRule>
            <AllowedOrigins>*</AllowedOrigins>
            <AllowedMethods>PUT</AllowedMethods>
            <AllowedHeaders>x-ms-*,content-type,accept</AllowedHeaders>
            <MaxAgeInSeconds>86400</MaxAgeInSeconds>
    </CorsRule>
<Cors>

            //First get the service properties from storage to ensure we're not adding the same CORS rule again.
            var storageAccount = new CloudStorageAccount(new StorageCredentials(storageName, storageAccountKey), true);
            var client = storageAccount.CreateCloudBlobClient();
            var serviceProperties = client.GetServiceProperties();
            var corsSettings = serviceProperties.Cors;

            corsSettings.CorsRules.Add(corsRule);
            //Save the rule
            await client.SetServicePropertiesAsync(serviceProperties);            
        }
         */

        public async Task<IAssetFile> CreateAssetFile(String assetId, String assetFileName, bool isPrimaryFile, String mpp5ConnectionString)
        {
            IAssetFile assetFile = null;
            try
            {
                var asset = await GetAssetById(assetId, mpp5ConnectionString);
                await RetryPolicy.ExecuteAsync(async () =>
                {
                    assetFile = await RetryPolicy.ExecuteAsync(async () => await Task.Factory.StartNew(() => asset.AssetFiles.Where(p => p.Name == assetFileName).FirstOrDefault()));

                    if (assetFile != null)
                    {
                        return assetFile;
                    }
                    
                    assetFile = await asset.AssetFiles.CreateAsync(assetFileName, new CancellationToken());
                    assetFile.IsPrimary = isPrimaryFile;
                    return assetFile;
                });
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create asset file {0} for asset '{0}', duration '{1}'", assetFileName, assetId), ex);
                throw;
            }

            return assetFile;
        }

        public async Task<ILocator> CreateAssetWriteLocation(IAsset asset, IAccessPolicy writePolicy, String mpp5ConnectionString)
        {
            try
            {
                var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);

                ILocator locator =
                    await
                    RetryPolicy.ExecuteAsync(
                        async () =>
                        await
                        Task.Factory.StartNew(
                            () =>
                            cloudMediaContext.Locators.Where(
                                l => l.AssetId == asset.Id && l.AccessPolicyId == writePolicy.Id).FirstOrDefault()));

                if (locator != null)
                {
                    return locator;
                }

                return await cloudMediaContext.Locators.CreateSasLocatorAsync(asset, writePolicy);
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed to create write location for asset '{0}'", asset.Name), ex);
                throw;
            }
        }

        public async Task<IAsset> CopyBlobToWams(String sourceFileName, SasContainer sourceContainer, String assetName, String mpp5ConnectionString)
        {
            IAsset asset = null;
            ILocator locator = null;
            IAccessPolicy policy = null;
            try
            {
                var sourceBlob =
                    new CloudBlockBlob(
                        new Uri(sourceContainer.ContainerUri + "/" + sourceFileName + sourceContainer.SasToken));

                if (!await sourceBlob.ExistsAsync())
                {
                    throw new ArgumentException(String.Format("Source file {0} could not be found",
                                                              sourceContainer.ContainerUri + "/" + sourceFileName));
                }

                var cloudMediaContext = _connectionClientService.WamsClient(mpp5ConnectionString);

                asset =
                    await
                    cloudMediaContext.Assets.CreateAsync(assetName, AssetCreationOptions.None, new CancellationToken());

                policy =
                    await
                    cloudMediaContext.AccessPolicies.CreateAsync("Write", TimeSpan.FromHours(10),
                                                                  AccessPermissions.Write);
                locator = await cloudMediaContext.Locators.CreateSasLocatorAsync(asset, policy);

                var assetFile = await asset.AssetFiles.CreateAsync(sourceFileName, new CancellationToken());
                assetFile.IsPrimary = true;

                var uri = locator.BaseUri + "/" + sourceFileName + locator.ContentAccessComponent;

                var destinationBlob = new CloudBlockBlob(new Uri(uri));

                var sourceStream = sourceBlob.OpenRead();

                await destinationBlob.UploadFromStreamAsync(sourceStream);

                /*
                await destinationBlob.StartCopyFromBlobAsync(sourceBlob);

                await MonitorCopy(destinationBlob);
                */
            }
            catch (Exception ex)
            {
                Logger.ErrorException(string.Format("Failed copying a blob {0}/{1} to a WAMS asset {2}", sourceContainer.ContainerUri, sourceFileName, assetName), ex);
                if (asset != null)
                {
                    asset.DeleteAsync();
                }
                throw;
            }
            finally
            {
                if (locator != null)
                {
                    locator.DeleteAsync();
                }
                if (policy != null)
                {
                    policy.DeleteAsync();
                }
            }
            return asset;
        }

        /*
        <taskDefinition xmlns="http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#">
    <name>MP4 Preprocessor</name>
    <id>859515BF-9BA3-4BDD-A3B6-400CEF07F870</id>
    <description xml:lang="en" />
    <inputFolder />
    <properties namespace="http://schemas.microsoft.com/iis/media/V4/TM/MP4Preprocessor#" prefix="mp4p">
    <property name="SmoothRequired" value="true" />
    <property name="HLSRequired" value="true" />
    </properties>
    <taskCode>
  <type>Microsoft.Web.Media.TransformManager.MP4PreProcessor.MP4Preprocessor_Task, Microsoft.Web.Media.TransformManager.MP4Preprocessor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type>
    </taskCode>
</taskDefinition>
        */        

        private async Task<ITask> ValidateMP4Asset(IJob job, IAsset asset, CloudMediaContext cloudMediaContext, String packageXml)
        {
            //Windows Azure Media Packager
            IMediaProcessor processor =
                await
                RetryPolicy.ExecuteAsync(
                    (async () =>
                     await
                     Task.Factory.StartNew(
                         () =>
                         cloudMediaContext.MediaProcessors.Where(p => p.Name == "Windows Azure Media Packager")
                                           .ToList()
                                           .OrderBy(p => new Version(p.Version))
                                           .LastOrDefault())));

            ITask task = job.Tasks.AddNew("Mp4 Validation Task",
                processor,
                packageXml,
                TaskOptions.None);

            task.InputAssets.Add(asset);

            task.OutputAssets.AddNew(asset.Name, AssetCreationOptions.None);

            return task;
        }
    }
}
