﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.App.Abstractions.Models;
using NeoMonitor.App.ViewModels;
using NeoMonitor.Basics.Models;
using NeoMonitor.Caches;

namespace NeoMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NodesController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly NodeDataCache _nodeDataCache;

        public NodesController(
            IMapper mapper,
            NodeDataCache nodeDataCache
            )
        {
            _mapper = mapper;
            _nodeDataCache = nodeDataCache;
        }

        [HttpGet]
        public ActionResult<IEnumerable<NodeViewModel>> Get()
        {
            NodeException[] nodeExps = _nodeDataCache.NodeExceptions;
            NodeViewModel[] nodes = _mapper.Map<NodeViewModel[]>(_nodeDataCache.Nodes);
            if (nodeExps.Length > 0)
            {
                foreach (var node in nodes)
                {
                    node.ExceptionCount = nodeExps.Count(ex => ex.Url == node.Url);
                }
            }
            else
            {
                foreach (var node in nodes)
                {
                    node.ExceptionCount = 0;
                }
            }
            return Ok(nodes);
        }

        // GET api/nodes/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<NodeExceptionViewModel>> Get(int id)
        {
            var nodeExps = _nodeDataCache.NodeExceptions;
            if (nodeExps.Length < 1)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var nodes = _nodeDataCache.Nodes;
            if (nodes.Length < 1)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var node = Array.Find(nodes, n => n.Id == id);
            if (node is null)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var result = _mapper.Map<IEnumerable<NodeExceptionViewModel>>(nodeExps.Where(ex => ex.Url == node.Url));
            return Ok(result);
        }

        [HttpGet("rawmempool/list")]
        public ActionResult<IEnumerable<RawMemPoolSizeModel>> GetMemPoolList()
        {
            var nodes = _nodeDataCache.Nodes;
            var result = nodes.Select(p => new RawMemPoolSizeModel() { Id = p.Id, MemoryPool = p.MemoryPool });
            return Ok(result);
        }
    }
}