namespace Algorithm
    module DecisionCalculator44=
        
        let alphaToN (a)=
            (2.0m/a)-1.0m
            
        let nToAlpha (n)=
            (2.0m / (n+1.0m))

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

        let bollinger(n:int, sigma:decimal, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let s1 = new System.Windows.Forms.DataVisualization.Charting.Series("historicalData")
            s1.ChartType <- System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick
            let s2 = new System.Windows.Forms.DataVisualization.Charting.Series("bbh")
            let s3 = new System.Windows.Forms.DataVisualization.Charting.Series("bbl")
            for i = 0 to prices.Count - 1 do
                let mutable temp:int = s1.Points.AddXY(prices.[i].Item1,double prices.[i].Item3)
                let mutable liste = [|double prices.[i].Item4;double prices.[i].Item2;double prices.[i].Item5|]
                s1.Points.[i].YValues <- Array.append s1.Points.[i].YValues liste
                temp <- temp+1
            let c1 = new System.Windows.Forms.DataVisualization.Charting.Chart()
            c1.Series.Add(s1)
            c1.Series.Add(s2)
            c1.Series.Add(s3)
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.BollingerBands, string n + "," + string sigma, "historicalData:Y3", "bbh:Y,bbl:Y")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["bbh"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [[decimal c1.Series.["bbh"].Points.[i].YValues.[0]; decimal c1.Series.["bbl"].Points.[i].YValues.[0]]]
            ergebnis1

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

        // let mutable ers:System.Collections.Generic.List<decimal> = new System.Collections.Generic.List<decimal>()

        // C = [(ER * (SCF – SCS)) + SCS]2
        // Where:
        // SCF is the exponential constant for the fastest EMA allowable (usually 2)
        // SCS is the exponential constant for the slowest EMA allowable (often 30)
        // ER is the efficiency ratio that was noted above
        let c (n1:int, n2:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            // ers.Add (er (prices))
            let er = er (prices)
            //pown (er * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2))) 2
            // double(ers.[ers.Count-1] * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2)))**0.75
            double(er * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2)))**0.75

        let ama (periodLength:int, n1:int, n2:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable ama = []
            //for i = periodLength to prices.Count-1 do
            for i = (if periodLength>n2 then periodLength else n2) to prices.Count do
                //let c = decimal(c (n1, n2, prices.GetRange (i, periodLength)))
                let c = decimal(c (n1, n2, prices.GetRange (i-periodLength, periodLength)))
                let n = int((2.0m/c)-1.0m)
                let firstPrice =
                    if (i - 2*n > 0) then
                        i-2*n
                    else
                        0
                let ema = (ema (n, prices.GetRange (firstPrice, i-firstPrice)))
                ama <- List.append ama [ema.[ema.Length-1]]
            ama

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

        (*
         * Calculates the Directional Movement 
         * dependent on which char is given(+/-) the positive or negative dm
         *)
        let calculateDm(decision:char, n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos & neg)
            let mutable dm = []
            for i = prices.Count - n to prices.Count - 1 do
                if(prices.[i].Item5>prices.[i-1].Item5) then 
                    if(decision.Equals('+')) then                  
                        dm <- List.append dm [prices.[i].Item3-prices.[i-1].Item3]
                    else 
                        dm <- List.append dm [0m]
                else if (prices.[i].Item5<prices.[i-1].Item5) then 
                    if(decision.Equals('-')) then
                        dm <- List.append dm [prices.[i-1].Item4-prices.[i].Item4]
                    else 
                        dm <- List.append dm [0m]
                else
                    dm <- List.append dm [0m]
            dm

        (*
         * Calculates the True Range
         * the tr is the highest of:
         * Today's High - Today's Low,
         * Today's High - Yesterday's Close, and
         * Yesterday's Close - Today's Low
         *)
        let calculateTr(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos & neg)
            let mutable tr = []
            for i = prices.Count - n to prices.Count - 1 do
                let tr1 = prices.[i].Item3-prices.[i].Item4
                let tr2 = prices.[i].Item3-prices.[i-1].Item5
                let tr3 = prices.[i-1].Item5-prices.[i].Item4
                let max = [tr1;tr2;tr3] |> List.max
                tr <- List.append tr [max]
                if (max = 0m) then 
                    tr <- List.append tr [max]
            tr

//        let adx(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
//            // calculate the directional movements (pos & neg)
//            let temp = calculateDm('+', n, prices)
//            let posdm14 = ema(nToAlpha(decimal (n)), calculateDm('+', n, prices))
//            let negdm14 = ema( nToAlpha(decimal (n)), calculateDm('-', n, prices))
//            let tr14 = ema( nToAlpha(decimal (n)), calculateTr(n,prices))
//            // calculate the directional indicators
//            // [ for x in 0 .. posdm14.Length -> posdm14.[x]/ tr14.[x] ]
//            let posdi14 = [ for x in 0 .. n - 1 -> posdm14.[x]/ tr14.[x] ]
//            let negdi14 = [ for x in 0 .. n - 1 -> negdm14.[x]/ tr14.[x] ]
//            // calculate the difference between the two indicators as a positive number
//            let difdi =  [ for x in 0 .. n - 1 -> decimal((double (posdi14.[x] - negdi14.[x])**2.0)**0.5) ]
//            let adx = ema(nToAlpha(decimal (n)), difdi)
//            if(adx.[adx.Length - 1] >= 0.60m) then 
//                3
//            else if(adx.[adx.Length - 1] >= 0.30m) then
//                2
//            else if(adx.[adx.Length - 1] >= 0.20m) then 
//                1
//            else
//                0

        let strategy(erp:int, s1:int, s2:int, m1:int, m2:int, l1:int, l2:int, n:int, sigma:decimal, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            let bollinger = bollinger(n, sigma, prices)
            let mutable lastCross = 0
            // bollinger band breadth
            let mutable breadth:System.Collections.Generic.List<decimal> = new System.Collections.Generic.List<decimal>()

            // amas for triple crossing
            let short = ama(erp, s1, s2, prices)
            let middle = ama(erp, m1, m2, prices)
            let long = ama(erp, l1, l2, prices)

            let mutable countTo = 
                if (bollinger.Length < long.Length) then
                    (bollinger.Length - 1)
                elif (erp > l2) then
                    prices.Count - erp - 1
                else
                    long.Length - 1

            for i = 0 to countTo do
                // calculate breadth of the bands
                //let breadth = bollinger.[i].[0] - bollinger.[i].[1]
                breadth.Add(bollinger.[i].[0] - bollinger.[i].[1])

                let er = er (prices.GetRange(i, n))
                
                // if er indicates sideways markets
                if er < 0.4m then
                    // price over higher bb
                    if prices.[i+n-1].Item5 > bollinger.[i].[0] then
                        lastCross <- -1
                        signals.Add(0)
                    // price under lower bb
                    else if prices.[i+n-1].Item5 < bollinger.[i].[1] then
                        lastCross <- 1
                        signals.Add(0)
                    // price between bbs
                    else
                        if lastCross = 1 then
                            signals.Add(1)
                        else if lastCross = -1 then
                            signals.Add(-1)
                        else
                            signals.Add(0)
                // trending market
                else
                    //signals.Add(0)
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
            let erp = 48
            let l2  = 48
            let n   = 20
            
            let countTo =
                if (n > erp) then
                    n-2
                elif (erp > l2) then
                    erp-2
                else
                    l2-2

            for i = 0 to countTo do
                signals.Add(0)

            strategy (erp, 8, 12, 16, 24, 32, l2, n, 2m, prices, signals)