#region Copyright
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
using System.Threading;
using NUnit.Framework;
using TickZoom.Api;
using TickZoom.FIX;
using TickZoom.MBTFIX;
using TickZoom.MBTQuotes;
using TickZoom.Test;

namespace Test
{
	[TestFixture]
	public class FIXSimulatorTest : ProviderTests
	{
		public static readonly Log log = Factory.SysLog.GetLogger(typeof(EquityLevel1));
		private FIXSimulator fixServer;
		public FIXSimulatorTest()
		{
			SetSymbol("SPY");
			SetTickTest(TickTest.Level1);
			SetProviderAssembly("MBTFIXProvider/Simulate");
			IsTestSeperate = true;
		}
		
		public override void Setup()
		{
			base.Setup();
			fixServer = new MBTFIXSimulator("");
		}
		
		
		public override void TearDown()
		{
            //fixServer.Dispose();
			base.TearDown();
		}
		
//		public override Provider CreateProvider(bool inProcessFlag) {
//			Provider provider;
//			if( inProcessFlag) {
//				provider = ProviderFactory();
//			} else {
//				// Set to use the Simulate configuration files.
//				var providerAssembly = ProviderAssembly + "/Simulate";
//				provider = Factory.Provider.ProviderProcess("127.0.0.1",6492,providerAssembly);
//			}
//			return provider;
//		}
		
		public override Agent ProviderFactory()
		{
            return Factory.Parallel.SpawnProvider(typeof(MBTProvider), "Simulate");
		}		
	}
}
