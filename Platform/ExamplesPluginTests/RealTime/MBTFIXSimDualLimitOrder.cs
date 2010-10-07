﻿#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion


using System;
using System.Configuration;
using System.IO;

using Loaders;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.Starters;

namespace MockProvider
{
#if SIMULATOR
	[TestFixture]
	public class MBTFIXSimDualLimitOrder : DualStrategyLimitOrder {
		public MBTFIXSimDualLimitOrder() {
			SyncTicks.Enabled = true;
			ConfigurationManager.AppSettings.Set("ProviderAddress","InProcess");
			DeleteFiles();
			CreateStarterCallback = CreateStarter;
			Symbols = "USD/JPY,EUR/USD";
			MatchTestResultsOf(typeof(DualStrategyLimitOrder));
			ShowCharts = false;
			StoreKnownGood = false;
		}
	
		public override void RunStrategy()
		{
			var fixServer = (FIXSimulator) Factory.FactoryLoader.Load(typeof(FIXSimulator),"MBTFIXProvider");
			try {
				base.RunStrategy();
				LoadReconciliation();
			} finally {
				fixServer.Dispose();
			}
		}
		
		[Test]
		public void PerformReconciliationTest() {
			PerformReconciliation();
		}
	
		public Starter CreateStarter()
		{
			ushort servicePort = 6490;
			SetupWarehouseConfig("MBTFIXProvider/Simulate",servicePort);
			Starter starter = new RealTimeStarter();
			starter.ProjectProperties.Engine.SimulateRealTime = true;
			starter.Config = "WarehouseTest.config";
			starter.Port = servicePort;
			return starter;
		}
		
		private void DeleteFiles() {
			while( true) {
				try {
					string appData = Factory.Settings["AppDataFolder"];
		 			File.Delete( appData + @"\Test\\ServerCache\EURUSD.tck");
		 			File.Delete( appData + @"\Test\\ServerCache\USDJPY.tck");
					break;
				} catch( Exception) {
				}
			}
		}	
	}
#endif
}
