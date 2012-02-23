﻿using System;
using System.IO;
using System.Text;
using TickZoom.Api;

namespace TickZoom.Common
{
    public class MonteCarloPass
    {
        public double MaxRunUp;
        public double MaxDrawDown;
        public double MaxInventorySize;
        public double ProfitLoss;
        private SymbolInfo symbol;
        private InventoryGroupTest.PriceChange[] priceChanges;

        public override string ToString()
        {
            return Round(MaxRunUp) + "," + Round(MaxDrawDown) + "," + MaxInventorySize + "," + Round(ProfitLoss);
        }

        public MonteCarloPass(SymbolInfo symbol, InventoryGroupTest.PriceChange[] priceChanges)
        {
            this.symbol = symbol;
            this.priceChanges = priceChanges;
        }

        public void RandomPass(Random random, bool writeOutput)
        {
            RandomPass(random,writeOutput,false);
        }

        public void RandomPass(Random random, bool writeOutput, bool debug)
        {
            if( debug) writeOutput = true;
            var inventory = (InventoryGroup) new InventoryGroupDefault(symbol);
            inventory.Retrace = .60;
            inventory.StartingLotSize = 1000;
            inventory.RoundLotSize = 1000;
            inventory.MaximumLotSize = int.MaxValue;
            inventory.MinimumLotSize = 1000;
            inventory.Goal = 5000;
            var first = true;
            var sb = writeOutput ? new StringBuilder() : null;
            var line = "Price,Bid,Offer,Spread,BidQuantity,OfferCuantity,Change,Position,BreakEven,PandL,CumPandL";
            if (writeOutput) sb.AppendLine(line);
            var price = 1.7000D;
            var lastPrice = price;
            var spread = symbol.MinimumTick * 10;
            var bid = Math.Round(price - spread, 5);
            var offer = Math.Round(price + spread, 5);
            try
            {

                for (var i = 0; i < 5000; i++)
                {
                    if (price < lastPrice)
                    {
                        offer = Math.Round(price + spread, 5);
                    }
                    if (price > lastPrice)
                    {
                        bid = Math.Round(price - spread, 5);
                    }

                    var amountToOffer = 0;
                    inventory.CalculateOffer(offer, out offer, out amountToOffer);
                    var amountToBid = 0;
                    inventory.CalculateBid(bid, out bid, out amountToBid);

                    lastPrice = price;
                    price = Math.Round(NextPrice(random, price), 5);

                    var change = 0;
                    if (price <= bid)
                    {
                        change = amountToBid;
                        bid = Math.Round(price - spread, 5);
                    }
                    else if (price >= offer)
                    {
                        change = -amountToOffer;
                        offer = Math.Round(price + spread, 5);
                    }

                    if (change != 0)
                    {
                        inventory.Change(price, change);
                    }

                    var pandl = Round(inventory.CurrentProfitLoss(price));
                    var cumulativeProfit = Round(inventory.CumulativeProfit);
                    var totalPanL = pandl + cumulativeProfit;
                    if (totalPanL > MaxRunUp)
                    {
                        MaxRunUp = totalPanL;
                    }
                    if (totalPanL < MaxDrawDown)
                    {
                        MaxDrawDown = totalPanL;
                    }
                    if( Math.Abs(inventory.Size) >  MaxInventorySize)
                    {
                        MaxInventorySize = Math.Abs(inventory.Size);
                    }
                    var actualSpread = Round(offer - bid);
                    line = Round(price) + "," + bid + "," + offer + "," + actualSpread + "," + amountToBid + "," + amountToOffer + "," +
                           change + "," + inventory.Size + "," + Round(inventory.BreakEven) + "," + Round(pandl) + "," +
                           cumulativeProfit;
                    if (writeOutput) sb.AppendLine(line);
                    if (writeOutput && !debug) Console.WriteLine(line);
                }
                ProfitLoss = inventory.CumulativeProfit + inventory.CurrentProfitLoss(price);
            }
            finally
            {
                if (writeOutput)
                {
                    if( !debug || MaxInventorySize > 10000000)
                    {
                        var appDataFolder = Factory.Settings["AppDataFolder"];
                        var file = appDataFolder + Path.DirectorySeparatorChar + "Random.csv";
                        File.WriteAllText(file, sb.ToString());
                    }
                    if( debug && MaxInventorySize > 1000000)
                    {
                        throw new ApplicationException("MaxInventory was " + MaxInventorySize);
                    }
                }
            }
        }

        private double NextPrice(Random r, double last)
        {
            var index = r.Next(priceChanges.Length);
            var pc = priceChanges[index];
            return last + pc.Change;
        }

        public double Round(double price)
        {
            return Math.Round(price, symbol.MinimumTickPrecision);
        }

    }
}