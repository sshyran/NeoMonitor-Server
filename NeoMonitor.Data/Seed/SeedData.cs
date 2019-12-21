﻿using System;
using System.IO;
using System.Linq;
using NeoMonitor.Data.Models;
using NeoState.Common;
using NeoState.Common.Tools;
using NodeMonitor.ViewModels;

namespace NeoMonitor.Data.Seed
{
	public class SeedData
	{
		private readonly NeoMonitorContext _ctx;

		public SeedData(NeoMonitorContext ctx)
		{
			_ctx = ctx;
		}

		public void Init()
		{
			SeedNodes();
		}

		private void SeedNodes()
		{
			if (!_ctx.Nodes.Any())
			{
				SeedNodesByNetType(NetConstants.MAIN_NET);
				SeedNodesByNetType(NetConstants.TEST_NET);
			}
		}

		private void SeedNodesByNetType(string net)
		{
			string seedjson = File.ReadAllText($@"seed-{net.ToLower()}.json");
			var mainNodes = JsonTool.DeserializeObject<NodeViewModel[]>(seedjson);
			if (mainNodes is null || mainNodes.Length < 1)
			{
				return;
			}
			_ctx.Nodes.AddRange(mainNodes.Select(viewModel => new Node()
			{
				Url = viewModel.Url,
				IP = viewModel.IP,
				Type = Enum.Parse<NodeAddressType>(viewModel.Type),
				Locale = viewModel.Locale,
				Location = viewModel.Location,
				Net = net
			}));
			_ctx.SaveChanges();
		}
	}
}