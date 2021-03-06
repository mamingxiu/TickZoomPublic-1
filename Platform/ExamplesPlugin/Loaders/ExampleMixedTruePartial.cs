﻿using TickZoom.Api;

namespace TickZoom.Examples
{
    public class ExampleMixedTruePartial : ExampleMixedLoader
    {
        public ExampleMixedTruePartial()
        {
            category = "Test";
            name = "True Partial: Multi-Symbol, Multi-Strategy";
            IsVisibleInGUI = false;
        }
        public override void OnLoad(ProjectProperties properties)
        {
#pragma warning disable 612,618
            foreach (var symbol in properties.Starter.SymbolProperties)
#pragma warning restore 612,618
            {
                symbol.PartialFillSimulation = PartialFillSimulation.PartialFillsIncomplete;
            }
            base.OnLoad(properties);
        }
    }
}