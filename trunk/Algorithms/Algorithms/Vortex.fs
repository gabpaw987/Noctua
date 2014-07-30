
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator=        
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
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

        (*  Normaly 14 Days, provide this method with at least 1 day more than you want
            to calculate.
        *)
        let vortex(prices:System.Collections.Generic.List<System.Tuple<System.DateTime,decimal, decimal, decimal, decimal>>)=
            // calculate the positve and negative trendmovement:
            let mutable sumpVM = 0m
            let mutable sumnVM = 0m
            let mutable sumTR = 0m
            for i = 1 to prices.Count - 1 do 
                sumpVM <- sumpVM + (prices.[i].Item3 - prices.[i - 1].Item4)
                sumnVM <- sumnVM + (prices.[i].Item4 - prices.[i - 1].Item3)
                // calculate the true range for the prices
                    //current high less current low
                let chlcl = prices.[i].Item3 - prices.[i].Item4
                    // current high less previous close
                let chlpc = prices.[i].Item3 - prices.[i - 1].Item5
                    // current low less previous Close
                let cllpc = prices.[i].Item4 - prices.[i - 1].Item5
                if chlcl < chlpc && chlpc < cllpc then
                    sumTR <- sumTR + cllpc
                else if chlcl > chlpc && chlpc > cllpc then
                    sumTR <- sumTR + chlcl
                else if chlcl < chlpc && chlpc > cllpc then
                    sumTR <- sumTR + chlpc
            let pVI = sumpVM / sumTR
            let nVI = sumnVM / sumTR
            4


        (*  two sma´s
            param:
                n1: smaller range of sma
                n2: higher range of sma     
        *)
        let signalgeber1(n1:int, n2:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n2-1 do
                signals.Add(0)
            let ergebnis1 = sma(n1,list2D)
            let ergebnis2 = sma(n2,list2D)
            let mover = n2 - n1
            for i = 0 to ergebnis2.Length-1 do
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    signals.Add(1)
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals

        (* 
            SMA vs. Price
        *)
        let signalgeber4(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n-1 do
                signals.Add(0)
            let ergebnis1 = sma(n,list2D)
            for i = 0 to ergebnis1.Length-1 do
                if list2D.[i+n-1].Item5 < ergebnis1.[i] then
                    signals.Add(-1)
                if list2D.[i+n-1].Item5 > ergebnis1.[i] then
                    signals.Add(1)
                if list2D.[i+n-1].Item5 = ergebnis1.[i] then
                    signals.Add(0)
            signals
        (*
            this signalgenerator adds a percentage fall to the calcuation system to cut loss
        *)
        let signalgeber3(n1:int, n2:int,percentage:double, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n2-1 do
                signals.Add(0)
            let ergebnis1 = sma(n1,list2D)
            let ergebnis2 = sma(n2,list2D)
            let mutable max = 1.0;
            let mover = n2 - n1
            let mutable activatedPercentageFall = false
            for i = 0 to ergebnis2.Length-1 do
                if max < double ( ergebnis1.[i+mover] - ergebnis2.[i]) then 
                    max <- double (ergebnis1.[i+mover] - ergebnis2.[i])
                if (double (ergebnis1.[i+mover] - ergebnis2.[i]))/max > 1.0-percentage then 
                     activatedPercentageFall <- true
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    if activatedPercentageFall then 
                        signals.Add(-1)
                    else 
                        signals.Add(1)
                    activatedPercentageFall <- false
                    max <- 1.0
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals

        (*  two sma´s
            param:
                n1: smaller range of sma
                n2: higher range of sma     
        *)
        let signalgeber(n1:int, n2:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            // add zeros
            for i = 0 to n2-2 do
                signals.Add(0)
            // calculate the two sma´s
            let ergebnis1 = sma(n1,list2D)
            let ergebnis2 = sma(n2,list2D)
            // the number of indices the smaller is ahead
            let mover = n2 - n1
            for i = 0 to ergebnis2.Length-1 do
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    signals.Add(1)
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals
        

        let tripleCrossed(n1:int,n2:int,n3:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to n3-1 do
                signals.Add(0)
            let shorts = sma(n1,list2D)
            let shortmover = (n3 - n1)
            let middle = sma(n2,list2D)
            let middlemover = (n2 - n1) 
            let longs = sma(n3,list2D)
            for i = 0 to longs.Length - 1 do
                if shorts.[i + shortmover] < middle.[i + middlemover] && middle.[i + middlemover] < longs.[i] then
                    signals.Add(-1*momentumInterpreter(5,list2D.GetRange(i+n3-10,10)))
                else if shorts.[i + shortmover] > middle.[i + middlemover] && middle.[i + middlemover] > longs.[i] then
                    signals.Add(1*momentumInterpreter(5,list2D.GetRange(i+n3-10,10)))
                else
                    // add the last again
                    // signals.Add(signals.[signals.Count-1])
                    // add zero
                    signals.Add(0)
            signals.RemoveAt(signals.Count-1)
            signals

        

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            let test = vortex(list2D.GetRange(list2D.Count - 16,14))
            tripleCrossed (10,15,20, list2D, signals)