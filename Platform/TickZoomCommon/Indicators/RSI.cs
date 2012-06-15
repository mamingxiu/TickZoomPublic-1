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
using System.IO;
using TickZoom.Api;
 
 
namespace TickZoom.Common
{
	/// <summary>
	/// Description of SMA.
	/// </summary>
	public class RSI : IndicatorCommon
	{
		int period;
		SMA avgPrice;
		Doubles price;
		object anyPrice;
		Integers gain;
		Integers loss;
		SMA avgGain;
		SMA avgLoss;
		 
		public RSI() : this( null, 5)
		{
		}
		 
		public RSI(object anyPrice, int period)
		{
			this.period = period;
			this.anyPrice = anyPrice;
			Drawing.PaneType = PaneType.Secondary;
		}
		 
		public override void OnInitialize() {
			if( anyPrice == null) {
				price = Doubles(Bars.Close);
			} else {
				price = Doubles(anyPrice);
			}
			avgPrice = new SMA( Bars.Close, period);
			avgPrice.Name="AvgPrice";
			avgPrice.IntervalDefault = IntervalDefault;
			gain = Integers();
			loss = Integers();
			avgGain = new SMA( gain, period);
			avgGain.Name="AvgGain";
			avgLoss = new SMA( loss, period);
			avgLoss.Name="AvgLoss";
			AddIndicator(avgPrice);
			AddIndicator(avgGain);
			AddIndicator(avgLoss);
		}
		 
		private void BeforeIntervalClose() {
			if( price.BarCount > 1 && avgPrice.Count>1) {
				int currPrice = (int) (price[0] - avgPrice[0]);
				int lastPrice = (int) (price[1] - avgPrice[1]);
				int change = currPrice - lastPrice;
				if( IsTrace) Log.TraceFormat("{0}: price[0]={1},avgPrice[0]={2},price[1]={3},avgPrice[1]={4}", Name, price[0], avgPrice[0], price[1], avgPrice[1]);
				if( change > 0) {
					gain.Add( change);
					loss.Add(0);
					Log.DebugFormat("gain.Add({0})", change);
					Log.DebugFormat("loss.Add(0)");
				} else {
					gain.Add(0);
					loss.Add( -change);
					Log.DebugFormat("gain.Add(0)");
					Log.DebugFormat("loss.Add({0})", (-change));
			}
			} else {
				gain.Add(0);
				loss.Add(0);
				if( IsTrace) Log.TraceFormat("gain.Add(0)");
				if( IsTrace) Log.TraceFormat("loss.Add(0)");
			}
		}
		 
		public override bool OnIntervalClose() {
			BeforeIntervalClose();
			if (Count == 1 ) {
				this[0] = 50;
			} else {
				double ag = avgGain[0];
				double al = avgLoss[0];
				double rs = (ag / al);
				double x = 100 / (rs + 1);
				double result = 100 - x;
				this[0] = result;
				if( IsTrace) Log.TraceFormat("{0}: BarCount={1},ag={2},al={3},rs={4},x={5},this={6}", Name, BarCount, avgGain[0], avgLoss[0], rs, x, this[0]);
			}
			return true;
		}
		 
		public int Period {
			get { return period; }
			set { period = value; }
		}
	}
}
 

