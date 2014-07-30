namespace Algorithm
    module DecisionCalculator10=
        let regression(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
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
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.Forecasting,"linear ,"+(string n)+", true, true","historicalData:Y3","indicator")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            ergebnis1
        
        let regression2(liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable xy = 0m
            let mutable xx = 0m
            let mutable x = 0m
            let mutable y = 0m
            for i = 0 to liste2D.Count do 
                x <- x+decimal i
                y <- y + liste2D.[i].Item5
                xy <- xy + liste2D.[i].Item5 * decimal i
                xx <- xx + decimal i* decimal i
            let b = (decimal liste2D.Count * xy - (x*y))/(decimal liste2D.Count * xx - x*x)
            //http://www.easycalculation.com/statistics/learn-regression.php
            1

        (*  *)
        let signalgeber(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            //for i = 0 to 2 do
            //    signals.Add(0)
            let regressions = regression(n,list2D)
            let temp = regressions.[0]
            for i = 0 to regressions.Length - 1 do
                let temp = regressions.[i]
                if regressions.[i]>0m then
                    signals.Add(1)
                else if regressions.[i]<0m then
                    signals.Add(-1)    
            signals

        let startCalculation (n:int, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (20, list2D, signals)