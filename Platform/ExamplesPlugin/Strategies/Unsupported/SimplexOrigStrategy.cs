using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using TickZoom.Api;
using TickZoom.Common;
using TickZoom.Interceptors;

namespace TickZoom.Examples
{
    public enum Direction
    {
        UpTrend,
        DownTrend,
        Sideways,
    }

    public class SimplexOrigStrategy: Strategy
    {
        private Direction direction = Direction.Sideways;
        IndicatorCommon bidLine;
        IndicatorCommon askLine;
        IndicatorCommon position;
        IndicatorCommon movement;
        IndicatorCommon averagePrice;
        private int displaceSMA = 10;
        bool isFirstTick = true;
        private int maxLevels = 5;
        double minimumTick;
        private int increaseSpreadInTicks = 10;
        private int reduceSpreadInTicks = 5;
        private double reduceSpread;
        private double increaseSpread;
        private int lotSize = 1000;
        double ask, marketAsk;
        double bid, marketBid;
        private Action<SimplexOrigStrategy> onDirectionChange;
        private bool isVisible = false;
        private long totalVolume = 0;
        private int maxLots = 20;
        private int lastSize = 0;
        private ActiveList<LocalFill> fills = new ActiveList<LocalFill>();
        private SMA sma;
        private IndicatorCommon displacedSMA;

        public SimplexOrigStrategy()
        {
            RequestUpdate(Intervals.Second1);
        }

        public override void OnInitialize()
        {
            Performance.Equity.GraphEquity = false; // Graphed by portfolio.
            Performance.GraphTrades = isVisible;

            sma = Formula.SMA(Seconds.Close, 30);
            sma.Drawing.IsVisible = false;
            sma.IntervalDefault = Intervals.Second1;

            displacedSMA = Formula.Indicator();
            displacedSMA.Name = "DisplacedSMA";
            displacedSMA.Drawing.IsVisible = isVisible;
            displacedSMA.IntervalDefault = Intervals.Second1;
            displacedSMA.Drawing.Color = Color.Blue;

            minimumTick = Data.SymbolInfo.MinimumTick;
            increaseSpread = increaseSpreadInTicks*minimumTick;
            reduceSpread = reduceSpreadInTicks*minimumTick;

            askLine = Formula.Indicator();
            askLine.Name = "Ask";
            askLine.Drawing.IsVisible = isVisible;

            bidLine = Formula.Indicator();
            bidLine.Name = "Bid";
            bidLine.Drawing.IsVisible = isVisible;

            averagePrice = Formula.Indicator();
            averagePrice.Name = "BE";
            averagePrice.Drawing.IsVisible = true;
            averagePrice.Drawing.Color = Color.Black;

            position = Formula.Indicator();
            position.Name = "Position";
            position.Drawing.PaneType = PaneType.Secondary;
            position.Drawing.GroupName = "Position";
            position.Drawing.IsVisible = isVisible;

            movement = Formula.Indicator();
            movement.Name = "Movement";
            movement.Drawing.PaneType = PaneType.Secondary;
            movement.Drawing.IsVisible = isVisible;
            movement.Drawing.GroupName = "Movement";
            movement.Drawing.Color = Color.Blue;
        }

        private void UpdateIndicators(Tick tick)
        {
            var comboTrades = Performance.ComboTrades;
            displacedSMA[0] = sma[displaceSMA];
            if (comboTrades.Count > 0)
            {
                var comboTrade = comboTrades.Tail;
                var averageEntry = CalcAveragePrice(comboTrade);
                var avgDivergence = Math.Abs(tick.Bid - averageEntry)/minimumTick;
                if( avgDivergence > 150 || Position.IsFlat)
                {
                    averagePrice[0] = double.NaN;
                }
                else
                {
                    averagePrice[0] = averageEntry;
                }
            }
            else
            {
                averagePrice[0] = double.NaN;
            }

            if (bidLine.Count > 0)
            {
                position[0] = Position.Current / lotSize;
            }

            switch( direction)
            {
                case Direction.Sideways:
                    movement[0] = 0;
                    break;
                case Direction.UpTrend:
                    movement[0] = 1;
                    break;
                case Direction.DownTrend:
                    movement[0] = -1;
                    break;
            }
        }

        public override bool OnProcessTick(Tick tick)
        {
            if (!tick.IsQuote)
            {
                throw new InvalidOperationException("This strategy requires bid/ask quote data to operate.");
            }

            Orders.SetAutoCancel();

            UpdateIndicators(tick);

            CheckForDirectionChange(tick);

            switch (direction)
            {
                case Direction.DownTrend:
                    OnProcessDownTrend(tick);
                    break;
                case Direction.UpTrend:
                    OnProcessUpTrend(tick);
                    break;
                case Direction.Sideways:
                    ProcessSideways(tick);
                    break;
            }
            return true;
        }

        private void CheckForDirectionChange(Tick tick)
        {
            var midpoint = (tick.Ask + tick.Bid) / 2;
            // Check for switch to trend.
            switch (direction)
            {
                case Direction.Sideways:
                    var lots = Position.Size / lotSize;
                    if (lots >= maxLots)
                    {
                        direction = midpoint < displacedSMA[0] ? Direction.DownTrend : Direction.UpTrend;
                    }
                    break;
                case Direction.DownTrend:
                    if (midpoint > displacedSMA[0] && Position.IsLong)
                    {
                        direction = Direction.Sideways;
                    }
                    break;
                case Direction.UpTrend:
                    if (midpoint < displacedSMA[0] && Position.IsShort)
                    {
                        direction = Direction.Sideways;
                    }
                    break;
            }
        }

        private void ProcessSideways(Tick tick)
        {
            if (Position.IsFlat)
            {
                OnProcessFlat(tick);
            }
            else if (Position.IsLong)
            {
                OnProcessLong(tick);
            }
            else if (Position.IsShort)
            {
                OnProcessShort(tick);
            }

        }

        private double lastMidpoint;
        private void OnProcessFlat(Tick tick)
        {
            var midpoint = (tick.Ask + tick.Bid) / 2;
            if (isFirstTick)
            {
                isFirstTick = false;
                lastMidpoint = midpoint;
                SetFlatBidAsk();
            }
            var comboTrades = Performance.ComboTrades;
            var lots = Position.Size/lotSize;
            var levels = Math.Min(maxLots - lots, maxLevels);
            if(comboTrades.Count == 0 || comboTrades.Tail.Completed)
            {
                Orders.Enter.ActiveNow.BuyLimit(bid, lotSize);
                Orders.Change.ActiveNow.BuyLimit(bid - increaseSpread, lotSize, levels - 1, increaseSpreadInTicks);

                Orders.Enter.ActiveNow.SellLimit(ask, lotSize);
                Orders.Change.ActiveNow.SellLimit(ask+increaseSpread, lotSize, levels-1, increaseSpreadInTicks);
            }
            else
            {
                Orders.Change.ActiveNow.BuyLimit(bid, lotSize, levels, increaseSpreadInTicks);
                Orders.Change.ActiveNow.SellLimit(ask, lotSize, levels, increaseSpreadInTicks);
            }
        }

        private double CalcAveragePrice(TransactionPairBinary comboTrade)
        {
            var lots = Position.Current/lotSize;
            var size = Math.Abs(comboTrade.CurrentPosition);
            var sign = -Math.Sign(comboTrade.CurrentPosition);
            var openPnL = comboTrade.AverageEntryPrice*size;
            var closedPnl = sign * comboTrade.ClosedPoints;
            var result = ( openPnL + closedPnl) / size;
            //result = comboTrade.AverageEntryPrice;
            return result;
        }

        private void OnProcessUpTrend(Tick tick)
        {
            var midpoint = (tick.Ask + tick.Bid) / 2;
            if( midpoint > displacedSMA[0])
            {
                PegBidToMidPoint(midpoint);
            }
            else
            {
                PegAskToMidPoint(midpoint);
            }
        }

        private void OnProcessDownTrend(Tick tick)
        {
            var midpoint = (tick.Ask + tick.Bid) / 2;
            if (midpoint < displacedSMA[0])
            {
                PegAskToMidPoint(midpoint);
            }
            else
            {
                PegBidToMidPoint(midpoint);
            }
        }

        private void PegBidToMidPoint(double midpoint)
        {
            // Peg to the midpoint
            if (midpoint > lastMidpoint || double.IsNaN(lastMidpoint))
            {
                lastMidpoint = midpoint;
                bid = midpoint - minimumTick;
                bidLine[0] = bid;
            }
            var lots = Position.Current / lotSize;
            var levels = Math.Min(maxLots - lots, maxLevels);
            if (lots < maxLots)
            {
                Orders.Change.ActiveNow.BuyLimit(bid, lotSize, levels, increaseSpreadInTicks);
            }
        }

        private void PegAskToMidPoint(double midpoint)
        {
            if (midpoint < lastMidpoint || double.IsNaN(lastMidpoint))
            {
                lastMidpoint = midpoint;
                ask = midpoint + minimumTick;
                askLine[0] = ask;
            }
            var lots = Position.Current / lotSize;
            var levels = Math.Min(maxLots + lots, maxLevels);
            if (lots > -maxLots)
            {
                Orders.Change.ActiveNow.SellLimit(ask, lotSize, levels, increaseSpreadInTicks);
            }
        }

        private void OnProcessLong(Tick tick)
        {
            var lots = Position.Size / lotSize;
            var levels = Math.Min(maxLots - lots, maxLevels);
            if( levels > 0)
            {
                Orders.Change.ActiveNow.BuyLimit(bid, lotSize, levels, increaseSpreadInTicks);
            }
            if (lots == 1)
            {
                Orders.Reverse.ActiveNow.SellLimit(ask, lotSize);
                Orders.Change.ActiveNow.SellLimit(ask+reduceSpread, lotSize, maxLevels-1, reduceSpreadInTicks);
            }
            else
            {
                Orders.Change.ActiveNow.SellLimit(ask, lotSize, maxLevels, reduceSpreadInTicks);
            }
        }

        private void OnProcessShort(Tick tick)
        {
            var lots = Position.Size/lotSize;
            var levels = Math.Min(maxLots - lots, maxLevels);
            if( levels > 0)
            {
                Orders.Change.ActiveNow.SellLimit(ask, lotSize, levels, increaseSpreadInTicks);
            }
            if (lots == 1)
            {
                Orders.Reverse.ActiveNow.BuyLimit(bid, lotSize);
                Orders.Change.ActiveNow.BuyLimit(bid - reduceSpread, lotSize, maxLevels - 1, reduceSpreadInTicks);
            }
            else
            {
                Orders.Change.ActiveNow.BuyLimit(bid, lotSize, maxLevels, reduceSpreadInTicks);
            }
        }

        public override void OnEndHistorical()
        {
            Log.Notice("Total volume was " + totalVolume + ". With commission paid of " + ((totalVolume / 1000) * 0.02D));
        }

        public class LocalFill
        {
            public int Size;
            public double Price;
            public TimeStamp Time;
            public LocalFill( LogicalFill fill)
            {
                Size = Math.Abs(fill.Position);
                Price = fill.Price;
                Time = fill.Time;
            }
            public LocalFill(int size, double price, TimeStamp time)
            {
                Size = size;
                Price = price;
                Time = time;
            }
            public override string ToString()
            {
                return Size + " at " + Price;
            }
        }

        private void SetupBidAsk(double price)
        {
            var lots = Position.Size/lotSize;
            increaseSpread = increaseSpreadInTicks * minimumTick;
            reduceSpread = reduceSpreadInTicks * minimumTick;
            var tick = Ticks[0];
            CheckForDirectionChange(tick);
            if (direction != Direction.Sideways) return;
            var myAsk = Position.IsShort ? price + increaseSpread : price + reduceSpread;
            var myBid = Position.IsLong ? price - increaseSpread : price - reduceSpread;
            marketAsk = Math.Max(tick.Ask, tick.Bid);
            marketBid = Math.Min(tick.Ask, tick.Bid);
            ask = Math.Max(myAsk, marketAsk);
            bid = Math.Min(myBid, marketBid);
            bidLine[0] = bid;
            askLine[0] = ask;
        }

        public override void OnEnterTrade(TransactionPairBinary comboTrade, LogicalFill fill, LogicalOrder filledOrder)
        {
            lastSize = Math.Abs(comboTrade.CurrentPosition);
            fills.AddFirst(new LocalFill(fill));
            SetupBidAsk(fill.Price);
        }

        public override void OnChangeTrade(TransactionPairBinary comboTrade, LogicalFill fill, LogicalOrder filledOrder)
        {
            lastMidpoint = double.NaN;
            if (fill.Position % lotSize != 0) return;
            var size = Math.Abs(comboTrade.CurrentPosition);
            var change = size - lastSize;
            totalVolume += change;
            lastSize = size;
            if (change > 0)
            {
                fills.AddFirst(new LocalFill(change, fill.Price, fill.Time));
                SetupBidAsk(fill.Price);
            }
            else
            {
                change = Math.Abs(change);
                for (var current = fills.First; current != null; current = current.Next)
                {
                    var prevFill = current.Value;
                    if (change > prevFill.Size)
                    {
                        change -= prevFill.Size;
                        fills.Remove(current);
                        if (fills.Count > 0)
                        {
                            SetupBidAsk(fill.Price);
                        }
                    }
                    else
                    {
                        prevFill.Size -= change;
                        if (prevFill.Size == 0)
                        {
                            fills.Remove(current);
                            if( fills.Count > 0)
                            {
                                SetupBidAsk(fill.Price);
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void SetFlatBidAsk()
        {
            var tick = Ticks[0];
            var midPoint = (tick.Bid + tick.Ask)/2;
            var myAsk = midPoint + increaseSpread;
            var myBid = midPoint - increaseSpread;
            var marketAsk = Math.Max(tick.Ask, tick.Bid);
            var marketBid = Math.Min(tick.Ask, tick.Bid);
            ask = Math.Max(myAsk, marketAsk);
            bid = Math.Min(myBid, marketBid);
            bidLine[0] = bid;
            askLine[0] = ask;
        }

        public override void OnExitTrade(TransactionPairBinary comboTrade, LogicalFill fill, LogicalOrder filledOrder)
        {
            lastMidpoint = double.NaN;
            direction = Direction.Sideways;
            fills.Clear();
            SetFlatBidAsk();
            if (!comboTrade.Completed)
            {
                throw new InvalidOperationException("Trade must be completed.");
            }
            totalVolume += comboTrade.Volume;
        }
        public Action<SimplexOrigStrategy> OnDirectionChange
        {
            get { return onDirectionChange; }
            set { onDirectionChange = value; }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

    }
}
