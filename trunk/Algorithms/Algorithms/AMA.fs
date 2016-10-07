﻿// http://www.investopedia.com/articles/trading/08/adaptive-moving-averages.asp#axzz2JT9BYz6o

namespace Algorithm
    module DecisionCalculator14=
        let ema(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let s1 = new System.Windows.Forms.DataVisualization.Charting.Series("historicalData")
            s1.ChartType <- System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick
            let s2 = new System.Windows.Forms.DataVisualization.Charting.Series("indicator")
            for i = 0 to prices.Count - 1 do
                let mutable temp:int = s1.Points.AddXY(prices.[i].Item1,double prices.[i].Item3)
                let mutable liste = [|double prices.[i].Item4;double prices.[i].Item2;double prices.[i].Item5|]
                s1.Points.[i].YValues <- Array.append s1.Points.[i].YValues liste
                temp <- temp+1
            let c1 = new System.Windows.Forms.DataVisualization.Charting.Chart()
            c1.Series.Add(s1)
            c1.Series.Add(s2)
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.ExponentialMovingAverage,(string n),"historicalData:Y3","indicator")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            ergebnis1

        let alphaToN (a)=
            (2.0m/a)-1.0m
            
        let nToAlpha (n)=
            (2.0m / (n+1.0m))

        // Efficiency Ratio
        // ER = (total price change for period) / (sum of absolute price changes for each bar)
        let er (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            let totalPriceChange = abs (prices.[prices.Count-1].Item5 - prices.[0].Item5)
            let mutable cumulativePriceChange = 0m
            let mutable oldPrice = prices.[0].Item5
            for i = 1 to prices.Count-1 do
                cumulativePriceChange <- cumulativePriceChange + abs (prices.[i].Item5 - oldPrice)
                oldPrice <- prices.[i].Item5
            totalPriceChange / cumulativePriceChange

        // C = [(ER * (SCF – SCS)) + SCS]2
        // Where:
        // SCF is the exponential constant for the fastest EMA allowable (usually 2)
        // SCS is the exponential constant for the slowest EMA allowable (often 30)
        // ER is the efficiency ratio that was noted above
        let c (n1:int, n2:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            let er = er (prices)
            //pown (er * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2))) 2
            double(er * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2)))**0.75

        let ama (periodLength:int, n1:int, n2:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable ama = []
            //for i = periodLength to prices.Count-1 do
            for i = (if periodLength>n2 then periodLength else n2) to prices.Count do
                //let c = decimal(c (n1, n2, prices.GetRange (i, periodLength)))
                let c = decimal(c (n1, n2, prices.GetRange (i-periodLength, periodLength)))
                let n = int((2.0m/c)-1.0m)
                let ema = (ema (n, prices.GetRange (0, i)))
                ama <- List.append ama [ema.[ema.Length-1]]
            ama

        let signalgeber(n:int, n1:int, n2:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, signals:System.Collections.Generic.List<int>) =
            // add 0 to signals for first 'n' prices (no ama available)
            for i = 0 to n-1 do
                signals.Add(0)
            let ama = ama(n, n1, n2, prices)
            for i = 0 to ama.Length-1 do
                if prices.[i+n-1].Item5 < ama.[i] then
                    signals.Add(-1)
                if prices.[i+n-1].Item5 > ama.[i] then
                    signals.Add(1)
                if prices.[i+n-1].Item5 = ama.[i] then
                    signals.Add(0)
            signals

        (*
            This function calculates the momentum over the give time period
        *)
        let momentum(period:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime,decimal, decimal, decimal, decimal>>)=
            let mutable result = []
            for i = period to prices.Count - 1 do
                result <- List.append result [prices.[i].Item5 - prices.[i - period].Item5]
            result
        
        (*
            This function calls the momentum function and interprets its results
        *)
        let momentumInterpreter(period:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime,decimal, decimal, decimal, decimal>>)=
            let moments = momentum(period, prices)
            // mom pos und steigend +3
            let mutable m1 = 0
            // mom pos und fallend +1
            let mutable m2 = 0
            // mom neg und fallend -3
            let mutable m3 = 0
            // mom neg und steigend -1
            let mutable m4 = 0
            for i = 1 to moments.Length - 1 do 
                if(moments.[i]>0m&&moments.[i-1]<moments.[i]) then
                    m1 <- m1 + 1
                if(moments.[i]>0m&&moments.[i-1]>moments.[i]) then
                    m2 <- m2 + 1
                if(moments.[i]<0m&&moments.[i-1]>moments.[i]) then
                    m3 <- m3 + 1
                if(moments.[i]<0m&&moments.[i-1]<moments.[i]) then
                    m4 <- m4 + 1
            if ((m1>m2&&m1>m3&&m1>m4)||m3>m1&&m3>m2&&m3>m4) then
                3
            else if ((m2>m1&&m2>m3&&m2>m4)||m4>m1&&m4>m2&&m4>m3) then
                1
            else
                0

        // erp...efficiency ratio period
        let tripleCrossed(erp:int, s1:int, s2:int, m1:int, m2:int, l1:int, l2:int,prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
        //let tripleCrossed(n1:int,n2:int,n3:int,prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to erp-2 do
                signals.Add(0)
            let short = ama(erp, s1, s2, prices)
            let middle = ama(erp, m1, m2, prices)
            let long = ama(erp, l1, l2, prices)
            for i = 0 to long.Length - 1 do
                if short.[i] < middle.[i] && middle.[i] < long.[i] then
                    signals.Add(-1*momentumInterpreter(5, prices.GetRange(i+erp-10,10)))
                else if short.[i] > middle.[i] && middle.[i] > long.[i] then
                    signals.Add(1*momentumInterpreter(5, prices.GetRange(i+erp-10,10)))
                else
                    // add the last again
                    // signals.Add(signals.[signals.Count-1])
                    // add zero
                    signals.Add(0)
            signals

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            //signalgeber (126, 2, 30, prices, signals)
            tripleCrossed(64, 8, 12, 16, 24, 22, 38, prices, signals)