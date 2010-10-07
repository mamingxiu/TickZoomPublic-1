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
using System.Threading;

namespace TickZoom.Api
{
	public class TickSync : SimpleLock {
		private static readonly Log log = Factory.SysLog.GetLogger(typeof(TickSync));
		private static readonly bool debug = log.IsDebugEnabled;		
		private static readonly bool trace = log.IsTraceEnabled;		
		private volatile bool completedTick = false;
		private int completedOrders = 0;
		public bool CompletedTick {
			get { return completedTick; }
			set { completedTick = value; }
		}		
		public void ClearPositionChange() {
			var value = Interlocked.Exchange(ref completedOrders, 0);
			if( trace) log.Trace("ClearPositionChange("+value+")");
		}
		public void SentPositionChange() {
			var value = Interlocked.Increment( ref completedOrders);
			if( trace) log.Trace("SentPositionChange("+value+")");
		}
		public void CompletedPositionChange() {
			var value = Interlocked.Decrement( ref completedOrders);
			if( trace) log.Trace("CompletedPositionChange("+value+")");
			if( value < 0) {
				log.Warn("Completed: value below zero: " + completedOrders);
			}
		}
		public bool CompletedOrders {
			get { return completedOrders == 0; }
		}
		
	}
}
