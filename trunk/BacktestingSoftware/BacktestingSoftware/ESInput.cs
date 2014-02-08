using System;
using System.Collections.Generic;
using IESignal;

namespace BacktestingSoftware
{
    internal class ESInput
    {
        public Hooks ESHook { get; private set; }

        public int Id { get; set; }

        public bool IsConnected { get; set; }

        private List<Tuple<DateTime, decimal, decimal, decimal, decimal>> ListOfBars;

        public string Barsize { get; private set; }

        private bool IsFuture;

        private string Symbol;
        private int HistoryHandle;

        public ESInput(int id, List<Tuple<DateTime, decimal, decimal, decimal, decimal>> LOB, string contractSymbol, string barsize, bool isFuture)
        {
            this.Id = id;

            ListOfBars = LOB;

            this.Barsize = barsize;

            this.IsFuture = isFuture;

            /*
            if (this.IsFuture)*/

            this.IsConnected = false;
        }

        private void start()
        {
            setupEsignal();

            if (this.ESHook.IsEntitled == 0)
            {
                Console.WriteLine("Not entitled");
            }
            else
            {
                this.ESHook.ReleaseAllHistory();
                this.ESHook.ReleaseAllTimeSales();

                this.ESHook.DoSymbolLink(this.Symbol);
                this.HistoryHandle = this.ESHook.get_RequestHistory(this.Symbol, "D",
                        IESignal.barType.btDAYS, 0, 0, 0);
            }
        }

        private void setupEsignal()
        {
            this.ESHook = new IESignal.Hooks();
            this.ESHook.OnBarsReceived += new IESignal._IHooksEvents_OnBarsReceivedEventHandler(ESHook_OnBarsReceived);
            this.ESHook.OnBarsChanged += new IESignal._IHooksEvents_OnBarsChangedEventHandler(ESHook_OnBarsChanged);
            this.ESHook.SetApplication("ceon88wh");
        }

        private void ESHook_OnBarsReceived(int lHandle)
        {
            if (lHandle == this.HistoryHandle)
            {
                Console.WriteLine("Recvd " + this.Symbol);
                if (this.ESHook.get_GetNumBars(lHandle) > 0)
                {
                    loadData(lHandle);
                }
            }
        }

        private void ESHook_OnBarsChanged(int lHandle)
        {
            if (lHandle == this.HistoryHandle)
            {
                if (this.ESHook.get_GetNumBars(lHandle) > 0)
                {
                    loadData(lHandle);
                }
            }
        }

        private void loadData(int lHandle)
        {
            IESignal.BarData bd = this.ESHook.get_GetBar(lHandle, 0);
            Console.Write(bd.dOI.ToString());
            Console.Write("\t");
            Console.Write(bd.dOpen.ToString());
            Console.Write("\t");
            Console.Write(bd.dHigh.ToString());
            Console.Write("\t");
            Console.Write(bd.dLow.ToString());
            Console.Write("\t");
            Console.Write(bd.dClose.ToString());
            Console.Write("\t");
            Console.Write(bd.dVolume.ToString());
            Console.Write("\t");
            Console.Write(bd.dTickTrade.ToString());
            Console.WriteLine();
        }
    }
}