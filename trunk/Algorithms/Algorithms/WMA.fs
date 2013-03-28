namespace Algorithm
    module DecisionCalculator88=
        (* This method calculates the sum from 1 to n *)
        let sum (n:int)= 
            let mutable result = 0m
            for i = 0 to int n do
                result <- result + decimal i
            result

        let wma2(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
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
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.WeightedMovingAverage,(string n),"historicalData:Y3","indicator")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            ergebnis1

        let wma (n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable position = n
            let mutable wma = []
            for i = n to liste2D.Count - 1 do
                let mutable mma = 0m
                let mutable weight = n
                for j = i-n to i do
                    mma <- mma + (liste2D.[j].Item5 * decimal weight)
                    weight <- weight - 1
                mma <- mma/(sum n)
                wma<- List.append wma [mma]
                position <- position + 1
            wma

        let signalgeber1(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
                    for i = 0 to n-1 do
                        signals.Add(0)
                    let ergebnis1 = wma(n,list2D)
                    for i = 0 to ergebnis1.Length-1 do
                        if list2D.[i+n-1].Item5 < ergebnis1.[i] then
                            signals.Add(-1)
                        if list2D.[i+n-1].Item5 > ergebnis1.[i] then
                            signals.Add(1)
                        if list2D.[i+n-1].Item5 = ergebnis1.[i] then
                            signals.Add(0)
                    signals
        
        let tripleCrossed(n1:int,n2:int,n3:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to n3-1 do
                signals.Add(0)
            let shorts = List.append [for i in 0..n1-2 -> 0m] (wma2(n1,list2D))
            let middle = List.append [for i in 0..n2-2 -> 0m] (wma2(n2,list2D))
            let longs = List.append [for i in 0..n3-2 -> 0m] (wma2(n3,list2D))
            for i = n3 to longs.Length-1 do
                if shorts.[i] < middle.[i] && middle.[i] < longs.[i] then
                    signals.Add(-1)
                else if shorts.[i] > middle.[i] && middle.[i] > longs.[i] then
                    signals.Add(1)
                else
                    // add the last again
                    // signals.Add(signals.[signals.Count-1])
                    // add zero
                    signals.Add(0)
            signals

        let signalgeber(shortn:int, longn:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to longn - 1 do
                signals.Add(0)
            let short = wma(shortn, list2D)
            let long = wma(longn, list2D)
            let mover = longn - shortn
            let mutable abw = 0m
            for i = 0 to long.Length-1 do
                if short.[i+mover] > long.[i] then
                    signals.Add(1)
                if short.[i+mover] < long.[i] then
                    signals.Add(-1)
                if short.[i+mover] = long.[i] then
                    signals.Add(0)
            signals

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            tripleCrossed (10,40, 90, list2D, signals)