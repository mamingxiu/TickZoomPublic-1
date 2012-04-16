﻿using System;
using TickZoom.Api;

namespace TickZoom.FIX
{
    public class SyntheticOrderRouter : PhysicalOrderHandler
    {
        private Agent receiver;
        private Agent self;
        private SymbolInfo symbol;
        public SyntheticOrderRouter( SymbolInfo symbol, Agent self, Agent receiver)
        {
            this.receiver = receiver;
            this.self = self;
            this.symbol = symbol;
        }

        public bool OnChangeBrokerOrder(CreateOrChangeOrder order)
        {
            receiver.SendEvent(new EventItem(self, EventType.SyntheticOrder, order));
            return true;
        }

        public bool OnCreateBrokerOrder(CreateOrChangeOrder order)
        {
            receiver.SendEvent(new EventItem(self, EventType.SyntheticOrder, order));
            return true;
        }

        public bool OnCancelBrokerOrder(CreateOrChangeOrder order)
        {
            receiver.SendEvent(new EventItem(self, EventType.SyntheticOrder, order));
            return true;
        }

        public int ProcessOrders()
        {
            return 0;
        }

        public void Clear()
        {
            receiver.SendEvent(new EventItem(self, EventType.SyntheticClear, symbol));
        }

        public bool IsChanged
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

    }
}