using EIR_9209_2.DataStore;
using EIR_9209_2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Net;
using System.Net.Http;

namespace EIR_9209_2.Service
{
    internal class CameraEndPointServices : BaseEndpointService
    {
        private readonly IInMemoryCamerasRepository _cameraList;
        private readonly IInMemorySiteInfoRepository _siteInfo;
        public CameraEndPointServices(ILogger<BaseEndpointService> logger, IHttpClientFactory httpClientFactory, Connection endpointConfig, IConfiguration configuration, IHubContext<HubServices> hubContext, IInMemoryConnectionRepository connection, IInMemorySiteInfoRepository siteInfo, IInMemoryCamerasRepository cameraList)
                : base(logger, httpClientFactory, endpointConfig, configuration, hubContext, connection)
        {
            _siteInfo = siteInfo;
            _cameraList = cameraList;
        }
        protected override async Task FetchDataFromEndpoint(CancellationToken stoppingToken)
        {
            try
            {
                _endpointConfig.Status = EWorkerServiceState.Running;
                _endpointConfig.LasttimeApiConnected = DateTime.Now;
                _endpointConfig.ApiConnected = true;
                await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", _endpointConfig, cancellationToken: stoppingToken);

                IQueryService queryService;
                string FormatUrl = "";
                if (_endpointConfig.MessageType == "Cameras")
                {
                    SiteInformation siteinfo = _siteInfo.GetByNASSCode(_configuration["ApplicationConfiguration:NassCode"]!.ToString());
                    string Fdbid = siteinfo.FdbId;
                    //process tag data
                    FormatUrl = string.Format(_endpointConfig.Url, Fdbid);
                    queryService = new QueryService(_logger, _httpClientFactory, jsonSettings, new QueryServiceSettings(new Uri(FormatUrl)));

                    var result = (await queryService.GetCameraData(stoppingToken));
                    // Process the data as needed
                    _ = Task.Run(async () => await ProcessCameraData(result), stoppingToken);
                }
                if (_endpointConfig.MessageType == "getCameraStills")
                {
                    //process camera list and get pictures
                    foreach (CameraMarker camera in _cameraList.GetAll())
                    {
                        FormatUrl = string.Format(_endpointConfig.Url, camera.CameraData.CameraName);
                        try
                        {
                            using (var httpClient = new HttpClient())
                            {
                                //Issue the GET request to a URL and read the response into a 
                                //stream that can be used to load the image
                                var fileInfo = new FileInfo(FormatUrl);
                                byte[] result = await httpClient.GetByteArrayAsync(FormatUrl);
                                _ = Task.Run(async () => await ProcessPictureData(result, camera.CameraData.CameraName, true), stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            byte[] noresult = Array.Empty<byte>();
                            _ = Task.Run(async () => await ProcessPictureData(noresult, camera.CameraData.CameraName, false), stoppingToken);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from {Url}", _endpointConfig.Url);
                _endpointConfig.ApiConnected = false;
                _endpointConfig.Status = EWorkerServiceState.ErrorPullingData;
                var updateCon = _connection.Update(_endpointConfig).Result;
                if (updateCon != null)
                {
                    await _hubContext.Clients.Group("Connections").SendAsync("updateConnection", updateCon, cancellationToken: stoppingToken);
                }
            }
        }
        private async Task ProcessCameraData(JToken result)
        {
            try
            {
                if (result is not null)
                {
                    await Task.Run(() => _cameraList.LoadCameraData(result));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        private async Task ProcessPictureData(byte[] result, string id, bool picload)
        {
            try
            {
                await Task.Run(() => _cameraList.LoadPictureData(result, id, picload));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
