
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator7=
        open System.IO
        let readLines (filePath:string) = seq {
            use sr = new StreamReader (filePath)
            while not sr.EndOfStream do
                yield sr.ReadLine ()
        }
        
        (* This function calculates the simple moving average for a list of lists of decimal and returns the calculated values in a list of decimals*)
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable sma = [];
            for i = n to liste2D.Count - 1 do
                let mutable mom = 0m
                for j = i - (n-1) to i do
                    mom <- mom + liste2D.[j].Item5
                sma <- List.append sma [mom / decimal n]
            sma

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
        
        (* smaklein gegen smagross *)
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

            (* sma gegen Preis*)
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

        (* Neu mit FinancialFormulars*)
        let signalgeber2(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n-2 do
                signals.Add(0)
            let s1 = new System.Windows.Forms.DataVisualization.Charting.Series("historicalData")
            s1.ChartType <- System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick
            let s2 = new System.Windows.Forms.DataVisualization.Charting.Series("incdicator")
            for i = 0 to list2D.Count - 1 do
                let mutable temp:int = s1.Points.AddXY(list2D.[i].Item1,double list2D.[i].Item3)
                let mutable liste = [|double list2D.[i].Item4;double list2D.[i].Item2;double list2D.[i].Item5|]
                s1.Points.[i].YValues <- Array.append s1.Points.[i].YValues liste
                temp <- temp+1
            let c1 = new System.Windows.Forms.DataVisualization.Charting.Chart()
            c1.Series.Add(s1)
            c1.Series.Add(s2)
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.MovingAverage,"90","historicalData:Y3","indicator")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            for i = 0 to ergebnis1.Length-1 do
                if list2D.[i+n-1].Item5 < ergebnis1.[i] then
                    signals.Add(-1)
                if list2D.[i+n-1].Item5 > ergebnis1.[i] then
                    signals.Add(1)
                if list2D.[i+n-1].Item5 = ergebnis1.[i] then
                    signals.Add(0)
            signals
        
        (* smaklein gegen smagross *)
        let signalgeber(n1:int, n2:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n2-1 do
                signals.Add(0)
            let ergebnis1 = sma2(n1,list2D)
            let ergebnis2 = sma2(n2,list2D)
            let mover = n2 - n1
            for i = 0 to ergebnis2.Length-1 do
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    signals.Add(1)
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals

        let startCalculation (n:int, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (10,90, list2D, signals)