﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Hubs;
using NeoMonitor.Services.Data;

namespace NeoMonitor.Services
{
    public sealed class NodeSyncHostService : BackgroundService
    {
        private readonly ILogger<NodeSyncHostService> _logger;
        private readonly IHubContext<NodeHub> _nodeHub;

        private readonly IMapper _mapper;

        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly INodeDataCache _nodeDataCache;

        public NodeSyncHostService(
            ILogger<NodeSyncHostService> logger,
            IHubContext<NodeHub> nodeHub,
            IMapper mapper,
            NodeSynchronizer nodeSynchronizer,
            INodeDataCache nodeDataCache
            )
        {
            _logger = logger;
            _nodeHub = nodeHub;
            _mapper = mapper;
            _nodeSynchronizer = nodeSynchronizer;
            _nodeDataCache = nodeDataCache;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _nodeSynchronizer.StartAsync();
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            _logger.LogDebug("[Service]--> {0} Executing.", nameof(NodeSyncHostService));
            Stopwatch sw = new Stopwatch();
            while (!cancelToken.IsCancellationRequested)
            {
                _logger.LogDebug("[{0}] Syncing... ...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sw.Restart();
                await _nodeSynchronizer.UpdateNodesInformationAsync();
                // await BroadcastToClientsAsync();
                sw.Stop();
                _logger.LogDebug("[{0}] UpdateBlockCountAsync: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), sw.Elapsed.ToString());
                await Task.Delay(5000, cancelToken);
            }
        }

        private Task BroadcastToClientsAsync()
        {
            return Task.CompletedTask;
        }
    }
}