namespace Algorithm
    module DecisionCalculator8=
        let nToAlpha (n)=
            (2.0m / (n+1.0m))

        let rec emaRec(i:int,alpha:decimal,list2D:List<decimal>)=
            if(i = -1) then
                0.0m
            else
                alpha*list2D.[i]+(1.0m-alpha) * (emaRec (i-1,alpha,list2D))

        let ema (alpha:decimal, list2D:List<decimal>)=
            let length = list2D.Length
            let mutable resultList:List<decimal> = []
            let mutable mma = 0.0m
            for i = 0 to length-1 do
                mma <- emaRec(i,alpha,list2D)
                // adds a new result to the list
                resultList <- List.append resultList [mma]
            resultList
        
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

        let adx(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos & neg)
            let temp = calculateDm('+', n, prices)
            let posdm14 = ema(nToAlpha(decimal (n)), calculateDm('+', n, prices))
            let negdm14 = ema( nToAlpha(decimal (n)), calculateDm('-', n, prices))
            let tr14 = ema( nToAlpha(decimal (n)), calculateTr(n,prices))
            // calculate the directional indicators
            // [ for x in 0 .. posdm14.Length -> posdm14.[x]/ tr14.[x] ]
            let posdi14 = [ for x in 0 .. n - 1 -> posdm14.[x]/ tr14.[x] ]
            let negdi14 = [ for x in 0 .. n - 1 -> negdm14.[x]/ tr14.[x] ]
            // calculate the difference between the two indicators as a positive number
            let difdi =  [ for x in 0 .. n - 1 -> decimal((double (posdi14.[x] - negdi14.[x])**2.0)**0.5) ]
            let adx = ema(nToAlpha(decimal (n)), difdi)
            if(adx.[adx.Length - 1] >= 0.60m) then 
                3
            else if(adx.[adx.Length - 1] >= 0.30m) then
                2
            else if(adx.[adx.Length - 1] >= 0.20m) then 
                1
            else
                0
        
        let sma2(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let s1 = new System.Windows.Forms.DataVisualization.Charting.Series("historicalData")
            s1.ChartType <- System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick
            let s2 = new System.Windows.Forms.DataVisualization.Charting.Series("incdicator")
            for i = 0 to liste2D.Count - 1 do
                let mutable temp:int = s1.Points.AddXY(liste2D.[i].Item1,double liste2D.[i].Item3)
                let mutable liste = [|double liste2D.[i].Item4;double liste2D.[i].Item2;double liste2D.[i].Item5|]
                s1.Points.[i].YValues <- Array.append s1.Points.[i].YValues liste
                temp <- temp+1
            let c1 = new System.Windows.Forms.DataVisualization.Charting.Chart()
            c1.Series.Add(s1)
            c1.Series.Add(s2)
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.MovingAverage,(string n),"historicalData:Y3","indicator")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            ergebnis1

        let tripleCrossed(n1:int,n2:int,n3:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to n3-1 do
                signals.Add(0)
            let shorts = sma2(n1,list2D)
            let shortmover = (n3 - n1)
            let middle = sma2(n2,list2D)
            let middlemover = (n2 - n1) 
            let longs = sma2(n3,list2D)
            for i = 0 to longs.Length - 1 do
                if shorts.[i + shortmover] < middle.[i + middlemover] && middle.[i + middlemover] < longs.[i] then
                    signals.Add(-1*adx(5,list2D.GetRange(i+n3-14,14)))
                else if shorts.[i + shortmover] > middle.[i + middlemover] && middle.[i + middlemover] > longs.[i] then
                    signals.Add(1*adx(5,list2D.GetRange(i+n3-14,14)))
                else
                    // add the last again
                    // signals.Add(signals.[signals.Count-1])
                    // add zero
                    signals.Add(0)
            signals.RemoveAt(signals.Count-1)
            signals

        

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            tripleCrossed (10,15,20, list2D, signals)