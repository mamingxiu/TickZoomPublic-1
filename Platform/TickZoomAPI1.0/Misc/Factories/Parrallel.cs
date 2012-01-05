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
using System.Collections.Generic;

namespace TickZoom.Api
{
		
	public enum YieldStatus {
		None,
		Invoke,
		DelayInvoke,
		Return,
		Repeat,
		Pause,
		Terminate,
	}
	
	public delegate Yield YieldMethod();
	
	public struct Yield {
		public bool IsIdle;
		public YieldMethod Method;
		public YieldStatus Status;
		
		public Yield Invoke(YieldMethod method) {
			Method = method;
			Status = YieldStatus.Invoke;
			return this;
		}
		
		public Yield DelayInvoke(YieldMethod method) {
			Method = method;
			Status = YieldStatus.DelayInvoke;
			return this;
		}
		
		public Yield Return {
			get {
				Status = YieldStatus.Return;
				return this;
			}
		}
		
		public Yield Repeat {
			get {
				Status = YieldStatus.Repeat;
				return this;
			}
		}

	    private static Yield blank;
		public static Yield Terminate {
			get
			{
			    Yield yield = blank;
				yield.Status = YieldStatus.Terminate;
				return yield;
			}
		}
		
		public static Yield Pause {
			get
			{
			    Yield yield = blank;
				yield.Status = YieldStatus.Pause;
				return yield;
			}
		}
		
		public static Yield DidWork {
			get { return blank; }
		}
		
		public static Yield NoWork {
			get
			{
			    var yield = blank;
			    yield.IsIdle = true;
			    return yield;
			}
		}
	}
	
	public interface ParallelStarter
	{
		Task Loop(object creator, Action<Exception> onException, YieldMethod loop);
	}
	
	public interface ForLoop {
		void For(Action<int> loop);
	}
		
	public enum ParallelMode {
		Normal,
		RealTime
	}
	
	public interface TrueTimer : IDisposable {
		bool Start( TimeStamp startTime);
		void Cancel();
		TimeStamp StartTime { get; }
		bool Active { get; }
	}
	
	public interface Parallel : ParallelStarter, IDisposable
	{
		void Yield();
		void Sleep(int millis);
		string GetStats();
		TrueTimer CreateTimer(string name, Task task, YieldMethod timerAction);
		void SetMode( ParallelMode mode);
		ParallelMode Mode {
			get;
		}
		Task CurrentTask {
			get;
		}
		Task[] Tasks {
			get;
		}
		long TickCount { get; }
        long MicroCount { get; }
        long TimerCount { get; }
	    int ThreadCount { get; }
	    TimeStamp UtcNow { get; }
	    bool IsWorkerThread { get; }
	    bool IsFinalized { get; }
	    void ReleaseIOTasks();
	    void ExcuteAsync( Action asyncAction);
	    void StackTrace();
	    void Exec(Action action);
	    void Exec(Queue queue, Action action);

        /// Queue related factories.
        string GetQueueStats();
        Pool<TickBinaryBox> TickPool(SymbolInfo symbol);
        TickQueue TickQueue(Type type);
        TickQueue TickQueue(string name);
        FastFillQueue FastFillQueue(string name, int maxSize);
        FastEventQueue FastEventQueue(string name, int maxSize);
        FastQueue<T> FastQueue<T>(string name);
        Pool<T> Pool<T>() where T : new();
        Pool<T> PoolChecked<T>() where T : new();
        EventQueue EventQueue(SymbolInfo symbol, string name);
        EventQueue EventQueue(string name);

	    void Release();
	    Provider SpawnProvider( Type typeToSpawn, params object[] args);
        Provider SpawnProvider( string assemblyName, string className, params object[] args);
	}
}
