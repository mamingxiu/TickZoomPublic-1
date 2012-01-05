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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TickZoom.Api;

namespace TickZoom.FIX
{
    public enum ServerState
    {
        Startup,
        LoggedIn,
        Recovered,
    }

    public abstract class FIXSimulatorSupport : FIXSimulator, LogAware
	{
		private string localAddress = "0.0.0.0";
		private static Log log = Factory.SysLog.GetLogger(typeof(FIXSimulatorSupport));
        private volatile bool debug;
        private volatile bool trace;
        private volatile bool verbose;
        public virtual void RefreshLogLevel()
        {
            debug = log.IsDebugEnabled;
            trace = log.IsTraceEnabled;
            verbose = log.IsVerboseEnabled;
        }
        private SimpleLock symbolHandlersLocker = new SimpleLock();
		private FIXTFactory1_1 fixFactory;
		private long realTimeOffset;
		private object realTimeOffsetLocker = new object();
		private YieldMethod MainLoopMethod;
	    private int heartbeatDelay = 1; 
        private ServerState fixState = ServerState.Startup;
        private readonly int maxFailtures = 5;
        private bool allTests;
        private int simulateDisconnectCounter;
        private int simulateOrderServerOfflineCounter;
        private int simulateOrderBlackHoleCounter;
        private bool simulateDisconnect;
        protected bool simulateSendOrderServerOffline;
        protected bool simulateRecvOrderServerOffline;
        private bool simulateOrderBlackHole;
        private bool simulateReceiveFailed;
        private bool simulateSendFailed;
        private int simulateOrderBlackHoleFrequency = 20;
        private int simulateDisconnectFrequency = 50;
        private int simulateRecvOrderServerOfflineFrequency = 50;
        protected int simulateSendOrderServerOfflineFrequency = 50;
        private int nextSendDisconnectSequence = 100;
        private int nextRecvDisconnectSequence = 100;
        protected int nextSendOrderServerOfflineSequence = 100;
        private int nextRecvOrderServerOfflineSequence = 100;

        private bool isOrderServerOnline = false;
        private bool isConnectionLost = false;

		// FIX fields.
		private ushort fixPort = 0;
		private Socket fixListener;
		protected Socket fixSocket;
		private Message _fixReadMessage;
		private Message _fixWriteMessage;
		private Task task;
		private bool isFIXSimulationStarted = false;
		private MessageFactory _fixMessageFactory;

		// Quote fields.
		private ushort quotesPort = 0;
		private Socket quoteListener;
		protected Socket quoteSocket;
		private Message _quoteReadMessage;
		private Message _quoteWriteMessage;
		private bool isQuoteSimulationStarted = false;
		private MessageFactory _quoteMessageFactory;
		private FastQueue<Message> fixPacketQueue = Factory.Parallel.FastQueue<Message>("SimulatorFIX");
		protected FastQueue<Message> quotePacketQueue = Factory.Parallel.FastQueue<Message>("SimulatorQuote");
		private Dictionary<long, SimulateSymbol> symbolHandlers = new Dictionary<long, SimulateSymbol>();
        private TrueTimer heartbeatTimer;
        private TimeStamp isHeartbeatPending = TimeStamp.MaxValue;

		public FIXSimulatorSupport(string mode, ushort fixPort, ushort quotesPort, MessageFactory _fixMessageFactory, MessageFactory _quoteMessageFactory)
		{
		    var randomSeed = new Random().Next(int.MaxValue);
		    if (randomSeed != 1234)
		    {
		        Console.WriteLine("Random seed for fix simulator:" + randomSeed);
		        log.Info("Random seed for fix simulator:" + randomSeed);
		    }
		    random = new Random(randomSeed);
		    log.Register(this);
		    switch (mode)
		    {
		        case "PlayBack":
		            break;
                case "Negative":
		            allTests = true;
		            break;
                default:
		            break;
		    }
            simulateDisconnect = allTests;
            simulateSendOrderServerOffline = allTests;
            simulateRecvOrderServerOffline = allTests;
            simulateOrderBlackHole = allTests;
            simulateReceiveFailed = allTests;
            simulateSendFailed = allTests;
			this._fixMessageFactory = _fixMessageFactory;
			this._quoteMessageFactory = _quoteMessageFactory;
			ListenToFIX(fixPort);
			ListenToQuotes(quotesPort);
			MainLoopMethod = MainLoop;
            if( debug) log.Debug("Starting FIX Simulator.");
            if( allTests)
            {
                if (!simulateDisconnect)
                {
                    log.Error("SimulateDisconnect is disabled.");
                }
                if (!simulateSendOrderServerOffline)
                {
                    log.Error("SimulateSendOrderServerOffline is disabled.");
                }
                if (!simulateRecvOrderServerOffline)
                {
                    log.Error("SimulateRecvOrderServerOffline is disabled.");
                }
                if (!simulateReceiveFailed)
                {
                    log.Error("SimulateReceiveFailed is disabled.");
                }
                if (!simulateOrderBlackHole)
                {
                    log.Error("SimulateOrderBlackHole is disabled.");
                }
                if (!simulateSendFailed)
                {
                    log.Error("SimulateSendFailed is disabled.");
                }
            }
        }

        public void DumpHistory()
        {
            for (var i = 0; i <= FixFactory.LastSequence; i++)
            {
                FIXTMessage1_1 message;
                if( FixFactory.TryGetHistory(i, out message))
                {
                    log.Info(message.ToString());
                }
            }
        }

        private void ListenToFIX(ushort fixPort)
		{
			this.fixPort = fixPort;
            fixListener = Factory.Provider.Socket(typeof(FIXSimulatorSupport).Name, localAddress, fixPort);
			fixListener.Bind();
			fixListener.Listen( 5);
			fixListener.OnConnect = OnConnectFIX;
			fixListener.OnDisconnect = OnDisconnectFIX;
			fixPort = fixListener.Port;
			log.Info("Listening for FIX to " + localAddress + " on port " + fixPort);
		}

		private void ListenToQuotes(ushort quotesPort)
		{
			this.quotesPort = quotesPort;
            quoteListener = Factory.Provider.Socket(typeof(FIXSimulatorSupport).Name, localAddress, quotesPort);
			quoteListener.Bind();
			quoteListener.Listen( 5);
			quoteListener.OnConnect = OnConnectQuotes;
			quoteListener.OnDisconnect = OnDisconnectQuotes;
			quotesPort = quoteListener.Port;
			log.Info("Listening for Quotes to " + localAddress + " on port " + quotesPort);
		}

		protected virtual void OnConnectFIX(Socket socket)
		{
			fixSocket = socket;
            fixState = ServerState.Startup;
            fixSocket.MessageFactory = _fixMessageFactory;
			log.Info("Received FIX connection: " + socket);
			StartFIXSimulation();
			TryInitializeTask();
			fixSocket.ReceiveQueue.ConnectInbound( task);
            fixSocket.SendQueue.ConnectOutbound(task);
		}

		protected virtual void OnConnectQuotes(Socket socket)
		{
			quoteSocket = socket;
			quoteSocket.MessageFactory = _quoteMessageFactory;
			log.Info("Received quotes connection: " + socket);
			StartQuoteSimulation();
			TryInitializeTask();
			quoteSocket.ReceiveQueue.ConnectInbound( task);
            quoteSocket.SendQueue.ConnectOutbound(task);
		}

        private QueueFilter filter;
		private void TryInitializeTask() {
			if( task == null) {
				task = Factory.Parallel.Loop("FIXSimulator", OnException, MainLoop);
			    filter = task.GetFilter();
			    task.Scheduler = Scheduler.EarliestTime;
                quotePacketQueue.ConnectInbound(task);
                fixPacketQueue.ConnectInbound(task);
                heartbeatTimer = Factory.Parallel.CreateTimer("Heartbeat", task, HeartbeatTimerEvent);
                var startTime = TimeStamp.UtcNow;
                startTime.AddSeconds(1);
                heartbeatTimer.Start(startTime);
                task.Start();
            }
        }

        private Yield HeartbeatTimerEvent()
        {
            var currentTime = TimeStamp.UtcNow;
            if (verbose) log.Verbose("Heartbeat occurred at " + currentTime);
            if (isConnectionLost)
            {
                if (debug) log.Debug("FIX connection was lost, closing FIX socket.");
                CloseFIXSocket();
                return Yield.NoWork.Repeat;
            }
            if (fixState != ServerState.Startup)
            {
                var now = TimeStamp.UtcNow;
                if( now > isHeartbeatPending)
                {
                    log.Error("HeartBeat response was never received.\n" + Factory.Parallel.GetStats());
                    log.Error("All stack traces follow...");
                    Factory.Parallel.StackTrace();
                    throw new ApplicationException("HeartBeat response was never received.");
                }
                isHeartbeatPending = TimeStamp.UtcNow;
                isHeartbeatPending.AddSeconds(15);
                OnHeartbeat();
            }
            currentTime.AddSeconds(1);
            if (verbose) log.Verbose("Setting next heartbeat at " + currentTime);
            heartbeatTimer.Start(currentTime);
            return Yield.DidWork.Repeat;
        }
        
        private void OnDisconnectFIX(Socket socket)
		{
			if (this.fixSocket.Equals(socket)) {
				log.Info("FIX socket disconnect: " + socket);
                StopFIXSimulation();
                CloseFIXSocket();
			}
		}

        protected virtual void StopFIXSimulation()
        {
            isFIXSimulationStarted = false;
        }

        private void OnDisconnectQuotes(Socket socket)
		{
			if (this.quoteSocket.Equals(socket)) {
				log.Info("Quotes socket disconnect: " + socket);
			    ShutdownHandlers();
				CloseQuoteSocket();
			}
		}

        protected void CloseFIXSocket()
        {
            if (fixPacketQueue != null && fixPacketQueue.Count > 0)
            {
                fixPacketQueue.Clear();
            }
            if (fixSocket != null)
            {
                fixSocket.Dispose();
            }
        }

        protected void CloseQuoteSocket()
        {
            if (quotePacketQueue != null)
            {
                quotePacketQueue.Clear();
            }
            if (quoteSocket != null)
            {
                quoteSocket.Dispose();
            }
        }

		protected void CloseSockets()
		{
            if (task != null)
            {
                task.Stop();
                task.Join();
            }
            CloseFIXSocket();
            CloseQuoteSocket();
		}

		public virtual void StartFIXSimulation()
		{
			isFIXSimulationStarted = true;
            isConnectionLost = false;
        }

		public virtual void StartQuoteSimulation()
		{
			isQuoteSimulationStarted = true;
		}
		
		private enum State { Start, ProcessFIX, WriteFIX, ProcessQuotes, WriteQuotes, Return };
		private State state = State.Start;
		private bool hasQuotePacket = false;
		private bool hasFIXPacket = false;
		private Yield MainLoop()
		{
		    EventItem eventItem;
            if( filter.Receive(out eventItem))
            {
                switch( (EventType) eventItem.EventType)
                {
                    case EventType.RemoteShutdown:
                        Dispose();
                        filter.Pop();
                        break;
                    default:
                        throw new ApplicationException("Unexpected event: " + eventItem);
                }
            }
            if( isConnectionLost)
            {
                CloseFIXSocket();
                return Yield.NoWork.Repeat;
            }
			var result = false;
			switch( state) {
				case State.Start:
					if( !fixPacketQueue.IsFull && FIXReadLoop())
					{
						result = true;
					}
				ProcessFIX:
					hasFIXPacket = ProcessFIXPackets();
					if( hasFIXPacket ) {
						result = true;
					}
				WriteFIX:
					if( hasFIXPacket) {
						if( !WriteToFIX()) {
							state = State.WriteFIX;
							return Yield.NoWork.Repeat;
						}
						if( fixPacketQueue.Count > 0) {
							state = State.ProcessFIX;
							return Yield.DidWork.Repeat;
						}
					}
					if( QuotesReadLoop()) {
						result = true;
					}
				ProcessQuotes: 
					hasQuotePacket = ProcessQuotePackets();
					if( hasQuotePacket) {
						result = true;
					}
				WriteQuotes:
					if( hasQuotePacket) {
						if( !WriteToQuotes()) {
							state = State.WriteQuotes;
							return Yield.NoWork.Repeat;
						}
						if( quotePacketQueue.Count > 0) {
							state = State.ProcessQuotes;
							return Yield.DidWork.Invoke(MainLoopMethod);
						}
					}
					break;
				case State.ProcessFIX:
					goto ProcessFIX;
				case State.WriteFIX:
					goto WriteFIX;
				case State.WriteQuotes:
					goto WriteQuotes;
				case State.ProcessQuotes:
					goto ProcessQuotes;
			}
			state = State.Start;
			if( result) {
				return Yield.DidWork.Repeat;
			} else {
				return Yield.NoWork.Repeat;
			}
		}

		private bool ProcessFIXPackets() {
			if( _fixWriteMessage == null && fixPacketQueue.Count == 0) {
				return false;
			}
			if( trace) log.Trace("ProcessFIXPackets( " + fixPacketQueue.Count + " packets in queue.)");
			if( fixPacketQueue.TryDequeue(out _fixWriteMessage)) {
				return true;
			} else {
				return false;
			}
		}

        private FIXTMessage1_1 GapFillMessage(int currentSequence)
        {
            var message = FixFactory.Create(currentSequence);
            message.SetGapFill();
            message.SetNewSeqNum(currentSequence + 1);
            message.AddHeader("4");
            return message;
        }

        private bool HandleResend(MessageFIXT1_1 messageFIX)
        {
            int end = messageFIX.EndSeqNum == 0 ? FixFactory.LastSequence : messageFIX.EndSeqNum;
            if (debug) log.Debug("Found resend request for " + messageFIX.BegSeqNum + " to " + end + ": " + messageFIX);
            for (int i = messageFIX.BegSeqNum; i <= end; i++)
            {
                FIXTMessage1_1 textMessage;
                var gapFill = false;
                if (!FixFactory.TryGetHistory(i, out textMessage))
                {
                    gapFill = true;
                    textMessage = GapFillMessage(i);
                }
                else
                {
                    switch (textMessage.Type)
                    {
                        case "A": // Logon
                        case "0": // Heartbeat
                        case "1": // Heartbeat
                        case "2": // Resend request.
                        case "4": // Reset sequence.
                            textMessage = GapFillMessage(i);
                            gapFill = true;
                            break;
                        default:
                            textMessage.SetDuplicate(true);
                            break;
                    }

                }

                if (gapFill)
                {
                    if (debug)
                    {
                        var fixString = textMessage.ToString();
                        string view = fixString.Replace(FIXTBuffer.EndFieldStr, "  ");
                        log.Debug("Sending Gap Fill message " + i + ": \n" + view);
                    }
                    ResendMessageProtected(textMessage);
                }
                else
                {
                    ResendMessage(textMessage);
                }
            }
            return true;
        }

        protected abstract void ResendMessage(FIXTMessage1_1 textMessage);
        protected abstract void RemoveTickSync(MessageFIXT1_1 textMessage);
        protected abstract void RemoveTickSync(FIXTMessage1_1 textMessage);

        private bool ProcessQuotePackets()
        {
			if( _quoteWriteMessage == null && quotePacketQueue.Count == 0) {
				return false;
			}
			if( trace) log.Trace("ProcessQuotePackets( " + quotePacketQueue.Count + " packets in queue.)");
			if( quotePacketQueue.TryDequeue(out _quoteWriteMessage)) {
				return true;
			} else {
				return false;
			}
		}

        public bool SendSessionStatus(string status)
        {
            switch( status)
            {
                case "2":
                    SetOrderServerOnline();
                    break;
                case "3":
                    SetOrderServerOffline();
                    break;
                default:
                    throw new ApplicationException("Unknown session status:" + status);
            }
            var mbtMsg = FixFactory.Create();
            mbtMsg.AddHeader("h");
            mbtMsg.SetTradingSessionId("TSSTATE");
            mbtMsg.SetTradingSessionStatus(status);
            if (debug) log.Debug("Sending order server status: " + mbtMsg);
            SendMessage(mbtMsg);
            return true;
        }

        private bool Resend(MessageFIXT1_1 messageFix)
		{
			var mbtMsg = FixFactory.Create();
			mbtMsg.AddHeader("2");
			mbtMsg.SetBeginSeqNum(remoteSequence);
			mbtMsg.SetEndSeqNum(0);
			if( debug) log.Debug("Sending resend request: " + mbtMsg);
            SendMessage(mbtMsg);
		    return true;
		}

        private Random random;
		private int remoteSequence = 1;
        private int recoveryRemoteSequence = 1;
		private bool FIXReadLoop()
		{
			if (isFIXSimulationStarted) {
				if (!fixSocket.TryGetMessage(out _fixReadMessage))
				{
				    return false;
				}
                var packetFIX = (MessageFIXT1_1)_fixReadMessage;
                IncreaseHeartbeat();
                if (debug) log.Debug("Received FIX message: " + packetFIX);
			    isHeartbeatPending = TimeStamp.MaxValue;
                try
                {
                    switch( fixState)
                    {
                        case ServerState.Startup:
                            if (packetFIX.MessageType != "A")
                            {
                                throw new InvalidOperationException("Invalid FIX message type " + packetFIX.MessageType + ". Not yet logged in.");
                            }
                            if (!packetFIX.IsResetSeqNum && packetFIX.Sequence < remoteSequence)
                            {
                                throw new InvalidOperationException("Login sequence number was " + packetFIX.Sequence + " less than expected " + remoteSequence + ".");
                            }
                            HandleFIXLogin(packetFIX);
                            if (packetFIX.Sequence > remoteSequence)
                            {
                                if (debug) log.Debug("Login packet sequence " + packetFIX.Sequence + " was greater than expected " + remoteSequence);
                                recoveryRemoteSequence = packetFIX.Sequence;
                                return Resend(packetFIX);
                            }
                            else
                            {
                                remoteSequence = packetFIX.Sequence + 1;
                                SendSessionStatusOnline();
                                fixState = ServerState.Recovered;
                                // Setup disconnect simulation.
                            }
                            break;
                        case ServerState.LoggedIn:
                        case ServerState.Recovered:
                            switch (packetFIX.MessageType)
                            {
                                case "A":
                                    throw new InvalidOperationException("Invalid FIX message type " + packetFIX.MessageType + ". Already logged in.");
                                case "2":
                                    HandleResend(packetFIX);
                                    break;
                            }
                            if (packetFIX.Sequence > remoteSequence)
                            {
                                if (debug) log.Debug("packet sequence " + packetFIX.Sequence + " greater than expected " + remoteSequence);
                                return Resend(packetFIX);
                            }
                            if (packetFIX.Sequence < remoteSequence)
                            {
                                if (debug) log.Debug("Already received packet sequence " + packetFIX.Sequence + ". Ignoring.");
                                return true;
                            }
                            else
                            {
                                if (fixState == ServerState.LoggedIn && packetFIX.Sequence >= recoveryRemoteSequence)
                                {
                                    // Sequences are synchronized now. Send TradeSessionStatus.
                                    fixState = ServerState.Recovered;
                                    SendSessionStatusOnline();
                                    // Setup disconnect simulation.
                                    nextRecvDisconnectSequence = packetFIX.Sequence + random.Next(simulateDisconnectFrequency * symbolHandlers.Count) + simulateDisconnectFrequency;
                                    if (debug) log.Debug("Set next disconnect sequence for receive = " + nextRecvDisconnectSequence);
                                    nextSendDisconnectSequence = FixFactory.LastSequence + random.Next(simulateDisconnectFrequency * symbolHandlers.Count) + simulateDisconnectFrequency;
                                    if (debug) log.Debug("Set next disconnect sequence for send = " + nextSendOrderServerOfflineSequence);
                                }
                                switch (packetFIX.MessageType)
                                {
                                    case "2": // resend request
                                        // already handled prior to sequence checking.
                                        remoteSequence = packetFIX.Sequence + 1;
                                        break;
                                    case "4":
                                        return HandleGapFill(packetFIX);
                                    default:
                                        return ProcessMessage(packetFIX);
                                }
                            }
                            break;
                    }
                }
                finally
                {
                    fixSocket.MessageFactory.Release(_fixReadMessage);
                }
			}
			return false;
		}

        public void SendSessionStatusOnline()
        {
            if (debug) log.Debug("Sending session status online.");
            SwitchBrokerState("online");
            SendSessionStatus("2");
            var handlers = new List<SimulateSymbol>();
            using( symbolHandlersLocker.Using())
            {
                if (debug) log.Debug("Flushing all fill queues.");
                foreach (var kvp in symbolHandlers)
                {
                    handlers.Add(kvp.Value);
                }
            }
            foreach(var handler in handlers) {
                handler.FillSimulator.FlushFillQueue();
            }
            if (debug) log.Debug("Current FIX Simulator orders.");
            foreach( var handler in handlers)
            {
                var orders = handler.FillSimulator.GetActiveOrders(handler.Symbol);
                for( var current = orders.First; current != null; current = current.Next)
                {
                    var order = current.Value;
                    if(debug) log.Debug(order.ToString());
                }
            }
        }

        protected bool HandleFIXLogin(MessageFIXT1_1 packet)
        {
            if (fixState != ServerState.Startup)
            {
                throw new InvalidOperationException("Invalid login request. Already logged in: \n" + packet);
            }
            fixState = ServerState.LoggedIn;
            if (packet.IsResetSeqNum)
            {
                if( packet.Sequence != 1)
                {
                    throw new InvalidOperationException("Found reset sequence number flag is true but sequence was " + packet.Sequence + " instead of 1.");
                }
                if (debug) log.Debug("Found reset seq number flag. Resetting seq number to " + packet.Sequence);
                FixFactory = CreateFIXFactory(packet.Sequence, packet.Target, packet.Sender);
                remoteSequence = packet.Sequence;
            }
            else if (FixFactory == null)
            {
                throw new InvalidOperationException(
                    "FIX login message specified tried to continue with sequence number " + packet.Sequence +
                    " but simulator has no sequence history.");
            }

            nextSendDisconnectSequence = FixFactory.LastSequence + random.Next(simulateDisconnectFrequency * symbolHandlers.Count) + simulateDisconnectFrequency;
            if (debug) log.Debug("Set next disconnect sequence for send = " + nextSendOrderServerOfflineSequence);
            nextRecvDisconnectSequence = packet.Sequence + random.Next(simulateDisconnectFrequency * symbolHandlers.Count) + simulateDisconnectFrequency;
            if (debug) log.Debug("Set next disconnect sequence for receive = " + nextRecvDisconnectSequence);
            nextSendOrderServerOfflineSequence = FixFactory.LastSequence + random.Next(simulateSendOrderServerOfflineFrequency * symbolHandlers.Count) + simulateSendOrderServerOfflineFrequency;
            if (debug) log.Debug("Set next order server offline sequence for send = " + nextSendOrderServerOfflineSequence);
            nextRecvOrderServerOfflineSequence = packet.Sequence + random.Next(simulateRecvOrderServerOfflineFrequency * symbolHandlers.Count) + simulateRecvOrderServerOfflineFrequency;
            if (debug) log.Debug("Set next order server offline sequence for receive = " + nextRecvOrderServerOfflineSequence);

            var mbtMsg = (FIXMessage4_4)FixFactory.Create();
            mbtMsg.SetEncryption(0);
            mbtMsg.SetHeartBeatInterval(HeartbeatDelay);
            mbtMsg.AddHeader("A");
            mbtMsg.SetSendTime(new TimeStamp(1800,1,1));
            if (debug) log.Debug("Sending login response: " + mbtMsg);
            SendMessage(mbtMsg);
            return true;
        }

        protected virtual FIXTFactory1_1 CreateFIXFactory(int sequence, string target, string sender)
        {
            throw new NotImplementedException();
        }

	    private bool HandleGapFill( MessageFIXT1_1 packetFIX)
        {
            if (!packetFIX.IsGapFill)
            {
                throw new InvalidOperationException("Only gap fill sequence reset supportted: \n" + packetFIX);
            }
            if (packetFIX.NewSeqNum <= remoteSequence)  // ResetSeqNo
            {
                throw new InvalidOperationException("Reset new sequence number must be greater than current sequence: " + remoteSequence + ".\n" + packetFIX);
            }
            remoteSequence = packetFIX.NewSeqNum;
            if (debug) log.Debug("Received gap fill. Setting next sequence = " + remoteSequence);
            return true;
        }

        private void SwitchBrokerState(string description)
        {
            foreach (var kvp in symbolHandlers)
            {
                var symbolBinary = kvp.Key;
                var tickSync = SyncTicks.GetTickSync(symbolBinary);
                tickSync.AddSwitchBrokerState(description);
            }
        }

        private bool ProcessMessage(MessageFIXT1_1 packetFIX)
        {
            if( isConnectionLost)
            {
                if (debug) log.Debug("Ignoring message: " + packetFIX);
                RemoveTickSync(packetFIX);
                return true;
            }
            if (simulateDisconnectCounter < maxFailtures && simulateDisconnect && FixFactory != null && packetFIX.Sequence >= nextRecvDisconnectSequence)
            {
                if (debug) log.Debug("Sequence " + packetFIX.Sequence + " >= receive disconnect sequence " + nextRecvDisconnectSequence + " so ignoring AND disconnecting.");
                if (debug) log.Debug("Ignoring message: " + packetFIX);
                nextRecvDisconnectSequence = packetFIX.Sequence + random.Next(simulateDisconnectFrequency * symbolHandlers.Count) + simulateDisconnectFrequency;
                if (debug) log.Debug("Set next disconnect sequence for receive = " + nextRecvDisconnectSequence);
                // Ignore this message. Pretend we never received it AND disconnect.
                // This will test the message recovery.)
                SwitchBrokerState("disconnect");
                isConnectionLost = true;
                ++simulateDisconnectCounter;
                return true;
            }
            if (simulateReceiveFailed && FixFactory != null && random.Next(50) == 1)
            {
                // Ignore this message. Pretend we never received it.
                // This will test the message recovery.
                if (debug) log.Debug("Ignoring fix message sequence " + packetFIX.Sequence);
                return Resend(packetFIX);
            }
            if (simulateOrderBlackHoleCounter < maxFailtures && simulateRecvOrderServerOffline && IsRecovered && FixFactory != null && packetFIX.Sequence >= nextRecvOrderServerOfflineSequence)
            {
                if (debug) log.Debug("Skipping sequence " + packetFIX.Sequence + " because >= recv order server offline " + nextRecvOrderServerOfflineSequence + " so making session status offline. " + packetFIX);
                nextRecvOrderServerOfflineSequence = packetFIX.Sequence + random.Next(simulateRecvOrderServerOfflineFrequency * symbolHandlers.Count) + simulateRecvOrderServerOfflineFrequency;
                if (debug) log.Debug("Set next recv order server offline sequence = " + nextRecvOrderServerOfflineSequence);
                SwitchBrokerState("disconnect");
                SetOrderServerOffline();
                SendSessionStatus("3"); //offline
                ++simulateOrderServerOfflineCounter;
                return true;
            }
            remoteSequence = packetFIX.Sequence + 1;
            switch (packetFIX.MessageType)
            {
                case "G":
                    if (simulateOrderBlackHoleCounter < maxFailtures && simulateOrderBlackHole && FixFactory != null && random.Next(simulateOrderBlackHoleFrequency * symbolHandlers.Count) == 1)
                    {
                        if (debug) log.Debug("Simulating order 'black hole' of 35=" + packetFIX.MessageType + " by incrementing sequence to " + remoteSequence + " but ignoring message with sequence " + packetFIX.Sequence);
                        ++simulateOrderBlackHoleCounter;
                        var message = (MessageFIX4_4) packetFIX;
                        var symbol = Factory.Symbol.LookupSymbol(message.Symbol);
                        var tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
                        tickSync.AddBlackHole(message.ClientOrderId);
                        return true;
                    }
                    break;
                case "D":
                    if (simulateOrderBlackHoleCounter < maxFailtures && simulateOrderBlackHole && FixFactory != null && random.Next(simulateOrderBlackHoleFrequency * symbolHandlers.Count) == 1)
                    {
                        if (debug) log.Debug("Simulating order 'black hole' of 35=" + packetFIX.MessageType + " by incrementing sequence to " + remoteSequence + " but ignoring message with sequence " + packetFIX.Sequence);
                        ++simulateOrderBlackHoleCounter;
                        var message = (MessageFIX4_4)packetFIX;
                        var symbol = Factory.Symbol.LookupSymbol(message.Symbol);
                        var tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
                        tickSync.AddBlackHole(message.ClientOrderId);
                        return true;
                    }
                    break;
                case "F":
                    if (simulateOrderBlackHoleCounter < maxFailtures && simulateOrderBlackHole && FixFactory != null && random.Next(3) == 1)
                    {
                        if (debug) log.Debug("Simulating order 'black hole' of 35=" + packetFIX.MessageType + " by incrementing sequence to " + remoteSequence + " but ignoring message with sequence " + packetFIX.Sequence);
                        ++simulateOrderBlackHoleCounter;
                        var message = (MessageFIX4_4)packetFIX;
                        var symbol = Factory.Symbol.LookupSymbol(message.Symbol);
                        var tickSync = SyncTicks.GetTickSync(symbol.BinaryIdentifier);
                        tickSync.AddBlackHole(message.ClientOrderId);
                        return true;
                    }
                    break;
            }
            ParseFIXMessage(_fixReadMessage);
            return true;
        }


	    public long GetRealTimeOffset( long utcTime) {
			lock( realTimeOffsetLocker) {
				if( realTimeOffset == 0L) {
					var currentTime = TimeStamp.UtcNow;
					var tickUTCTime = new TimeStamp(utcTime);
				   	log.Info("First historical playback tick UTC tick time is " + tickUTCTime);
				   	log.Info("Current tick UTC time is " + currentTime);
				   	realTimeOffset = currentTime.Internal - utcTime;
				   	var microsecondsInMinute = 1000L * 1000L * 60L;
                    var extra = realTimeOffset % microsecondsInMinute;
                    realTimeOffset -= extra;
                    realTimeOffset += microsecondsInMinute;
				   	var elapsed = new Elapsed( realTimeOffset);
				   	log.Info("Setting real time offset to " + elapsed);
				}
			}
			return realTimeOffset;
		}

		public void AddSymbol(string symbol, Action<Message, SymbolInfo, Tick> onTick, Action<PhysicalFill> onPhysicalFill, Action<CreateOrChangeOrder,bool,string> onOrderReject)
		{
			var symbolInfo = Factory.Symbol.LookupSymbol(symbol);
            using (symbolHandlersLocker.Using())
            {
                if (!symbolHandlers.ContainsKey(symbolInfo.BinaryIdentifier))
                {
                    if( SyncTicks.Enabled)
                    {
                        var symbolHandler = new SimulateSymbolSyncTicks(this, symbol, onTick, onPhysicalFill, onOrderReject);
                        symbolHandlers.Add(symbolInfo.BinaryIdentifier, symbolHandler);
                    }
                    else
                    {
                        var symbolHandler = new SimulateSymbolPlayback(this, symbol, onTick, onPhysicalFill, onOrderReject);
                        symbolHandlers.Add(symbolInfo.BinaryIdentifier, symbolHandler);
                    }
                }
            }
            if (IsOrderServerOnline)
            {
                SetOrderServerOnline();
            }
        }

        public void SetOrderServerOnline()
        {
            using (symbolHandlersLocker.Using())
            {
                foreach (var kvp in symbolHandlers)
                {
                    var handler = kvp.Value;
                    handler.IsOnline = true;
                }
            }
            isOrderServerOnline = true;
        }

        public void SetOrderServerOffline()
        {
            using (symbolHandlersLocker.Using())
            {
                foreach (var kvp in symbolHandlers)
                {
                    var handler = kvp.Value;
                    handler.IsOnline = false;
                }
            }
            isOrderServerOnline = false;
        }

        public int GetPosition(SymbolInfo symbol)
		{
            // Don't lock. This call always wrapped in a locked using clause.
            SimulateSymbol symbolSyncTicks;
            if( symbolHandlers.TryGetValue(symbol.BinaryIdentifier, out symbolSyncTicks))
            {
                return symbolSyncTicks.ActualPosition;
            }
            return 0;
		}

        public void CreateOrder(CreateOrChangeOrder order)
        {
            SimulateSymbol symbolSyncTicks;
            using (symbolHandlersLocker.Using())
            {
                symbolHandlers.TryGetValue(order.Symbol.BinaryIdentifier, out symbolSyncTicks);
            }
            if( symbolSyncTicks != null) {
                symbolSyncTicks.CreateOrder(order);
            }
        }

        public void TryProcessAdustments(CreateOrChangeOrder order)
        {
            SimulateSymbol symbolSyncTicks;
            using (symbolHandlersLocker.Using())
            {
                symbolHandlers.TryGetValue(order.Symbol.BinaryIdentifier, out symbolSyncTicks);
            }
            if (symbolSyncTicks != null)
            {
                symbolSyncTicks.TryProcessAdjustments();
            }
        }

        public void ChangeOrder(CreateOrChangeOrder order)
        {
            SimulateSymbol symbolSyncTicks;
            using (symbolHandlersLocker.Using())
            {
                symbolHandlers.TryGetValue(order.Symbol.BinaryIdentifier, out symbolSyncTicks);
            }
            if( symbolSyncTicks != null) {
                symbolSyncTicks.ChangeOrder(order);
            }
        }

        public void CancelOrder(CreateOrChangeOrder order)
        {
            SimulateSymbol symbolSyncTicks;
            using (symbolHandlersLocker.Using())
            {
                symbolHandlers.TryGetValue(order.Symbol.BinaryIdentifier, out symbolSyncTicks);
            }
            if( symbolSyncTicks != null) {
                symbolSyncTicks.CancelOrder(order);
            }
        }

        public CreateOrChangeOrder GetOrderById(SymbolInfo symbol, string clientOrderId) {
            SimulateSymbol symbolSyncTicks;
            using (symbolHandlersLocker.Using())
            {
                symbolHandlers.TryGetValue(symbol.BinaryIdentifier, out symbolSyncTicks);
            }
            if( symbolSyncTicks != null) {
                return symbolSyncTicks.GetOrderById(clientOrderId);
            }
            else
            {
                throw new ApplicationException("StartSymbol was never called for " + symbol + " so now symbol handler was found.");
            }
       }

		private bool QuotesReadLoop()
		{
			if (isQuoteSimulationStarted) {
				if (quoteSocket.TryGetMessage(out _quoteReadMessage)) {
					if (verbose)	log.Verbose("Local Read: " + _quoteReadMessage);
					ParseQuotesMessage(_quoteReadMessage);
                    quoteSocket.MessageFactory.Release(_quoteReadMessage);
					return true;
				}
			}
			return false;
		}

		public virtual void ParseFIXMessage(Message message)
		{
		}

		public virtual void ParseQuotesMessage(Message message)
		{
			if (debug) log.Debug("Received Quotes message: " + message);
		}

		public bool WriteToFIX()
		{
			if (!isFIXSimulationStarted || _fixWriteMessage == null) return true;
		    var result = SendMessageInternal(_fixWriteMessage);
            if( result)
            {
                _fixWriteMessage = null;
            }
		    return result;
		}

        protected void ResendMessageProtected(FIXTMessage1_1 fixMessage)
        {
            if (isConnectionLost)
            {
                return;
            }
            var writePacket = fixSocket.MessageFactory.Create();
            var message = fixMessage.ToString();
            writePacket.DataOut.Write(message.ToCharArray());
            writePacket.SendUtcTime = TimeStamp.UtcNow.Internal;
            if (debug) log.Debug("Resending simulated FIX Message: " + fixMessage);
            SendMessageInternal(writePacket);
        }

        private bool SendMessageInternal( Message message)
        {
            if (fixSocket.TrySendMessage(message))
            {
                IncreaseHeartbeat();
                if (trace) log.Trace("Local Write: " + message);
                return true;
            }
            else
            {
                log.Error("Failed to Write: " + message);
                return false;
            }
        }

		public bool WriteToQuotes()
		{
			if (!isQuoteSimulationStarted || _quoteWriteMessage == null) return true;
			if( quoteSocket.TrySendMessage(_quoteWriteMessage)) {
				if (trace) log.Trace("Local Write: " + _quoteWriteMessage);
				_quoteWriteMessage = null;
				return true;
			} else {
				return false;
			}
		}

		private void IncreaseHeartbeat()
		{
		    var timeStamp = TimeStamp.UtcNow;
		    timeStamp.AddSeconds(HeartbeatDelay);
            if (verbose) log.Verbose("Setting next heartbeat for " + timeStamp);
            heartbeatTimer.Start(timeStamp);
		}		

        public void SendMessage(FIXTMessage1_1 fixMessage)
        {

            FixFactory.AddHistory(fixMessage);
            if (isConnectionLost)
            {
                RemoveTickSync(fixMessage);
                return;
            }
            if (simulateDisconnectCounter > maxFailtures && simulateDisconnect && fixMessage.Sequence >= nextSendDisconnectSequence)
            {
                if (debug) log.Debug("Sequence " + fixMessage.Sequence + " >= send disconnect sequence " + nextSendDisconnectSequence + " so ignoring AND disconnecting.");
                if (debug) log.Debug("Ignoring message: " + fixMessage);
                nextSendDisconnectSequence = fixMessage.Sequence + random.Next(simulateDisconnectFrequency * symbolHandlers.Count) + simulateDisconnectFrequency;
                if (trace) log.Trace("Set next disconnect sequence for send = " + nextSendDisconnectSequence);
                SwitchBrokerState("disconnect");
                isConnectionLost = true;
                ++simulateDisconnectCounter;
                return;
            }
            if (simulateSendFailed && IsRecovered && random.Next(50) == 4)
            {
                if (debug) log.Debug("Skipping send of sequence # " + fixMessage.Sequence + " to simulate lost message. " + fixMessage);
                if( fixMessage.Type == "1")
                {
                    isHeartbeatPending = TimeStamp.MaxValue;
                }
                if (debug) log.Debug("Message type is: " + fixMessage.Type);
                return;
            }
            var writePacket = fixSocket.MessageFactory.Create();
            var message = fixMessage.ToString();
            writePacket.DataOut.Write(message.ToCharArray());
            writePacket.SendUtcTime = TimeStamp.UtcNow.Internal;
            if( debug) log.Debug("Simulating FIX Message: " + fixMessage);
            try
            {
                fixPacketQueue.Enqueue(writePacket, writePacket.SendUtcTime);
            }
            catch( QueueException ex)
            {
                if( ex.EntryType == EventType.Terminate)
                {
                    if( debug) log.Debug("fix packet queue returned queue exception " + ex.EntryType + ". Dropping message due to dispose.");

                }
                else
                {
                    throw;
                }
            }
            if (simulateOrderServerOfflineCounter < maxFailtures && simulateSendOrderServerOffline && IsRecovered && FixFactory != null && fixMessage.Sequence >= nextSendOrderServerOfflineSequence)
            {
                if (debug) log.Debug("Skipping sequence " + fixMessage.Sequence + " because >= send order server offline for send " + nextSendOrderServerOfflineSequence + " so making session status offline. " + fixMessage);
                nextSendOrderServerOfflineSequence = fixMessage.Sequence + random.Next(simulateSendOrderServerOfflineFrequency * symbolHandlers.Count) + simulateSendOrderServerOfflineFrequency;
                if (trace) log.Trace("Set next send order server offline sequence for send = " + nextSendOrderServerOfflineSequence);
                SwitchBrokerState("offline");
                SetOrderServerOffline();
                ++simulateOrderServerOfflineCounter;
                SendSessionStatus("3");
            }
        }

        protected virtual Yield OnHeartbeat()
        {
			if( fixSocket != null && FixFactory != null)
			{
				var mbtMsg = (FIXMessage4_4) FixFactory.Create();
				mbtMsg.AddHeader("1");
				if( trace) log.Trace("Requesting heartbeat: " + mbtMsg);
                SendMessage(mbtMsg);
			}
			return Yield.DidWork.Return;
		}
		
		public void OnException(Exception ex)
		{
			log.Error("Exception occurred", ex);
		}

		protected volatile bool isDisposed = false;

        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

        private void ShutdownHandlers()
        {
            if( debug) log.Debug("ShutdownHandlers()");
            if (symbolHandlers != null)
            {
                using( symbolHandlersLocker.Using()) {
                    if (debug) log.Debug("There are " + symbolHandlers.Count + " symbolHandlers.");
                    foreach (var kvp in symbolHandlers)
                    {
                        var handler = kvp.Value;
                        if (debug) log.Debug("Disposing symbol handler " + handler);
                        handler.Dispose();
                    }
                    symbolHandlers.Clear();
                }
            }
            else
            {
                if (debug) log.Debug("symbolHandlers is null.");
            }
        }

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed) {
				isDisposed = true;
				if (disposing) {
                    if (debug) log.Debug("Dispose()");
                    ShutdownHandlers();
                    CloseSockets();
                    if (fixListener != null)
                    {
                        fixListener.Dispose();
                    }
                    if (quoteListener != null)
                    {
                        quoteListener.Dispose();
                    }
                }
			}
		}

        public bool IsRecovered
        {
            get { return fixState == ServerState.Recovered;  }
        }

		public ushort FIXPort {
			get { return fixPort; }
		}

		public ushort QuotesPort {
			get { return quotesPort; }
		}
		
		public long RealTimeOffset {
			get { return realTimeOffset; }
		}
		
		public Socket QuoteSocket {
			get { return quoteSocket; }
		}
		
		public FastQueue<Message> QuotePacketQueue {
			get { return quotePacketQueue; }
		}

	    public int HeartbeatDelay
	    {
	        get { return heartbeatDelay; }
	    }

        public bool IsOrderServerOnline
        {
            get { return isOrderServerOnline; }
        }

        public FIXTFactory1_1 FixFactory
        {
            get { return fixFactory; }
            set {
                if( fixFactory != null && value == null)
                {
                    log.Warn("FixFactory set to null.\n" + Environment.StackTrace);
                } else if( value == null)
                {
                    log.Info("FixFactory already null and set to null.\n" + Environment.StackTrace);
                }
                fixFactory = value;
            }
        }
	}
}