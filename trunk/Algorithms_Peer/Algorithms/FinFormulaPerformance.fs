namespace Algorithm
    module DecisionCalculator=

        let alphaToN (a) : int=
            int ((2.0m/a)-1.0m)
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

        let ema (n:int, prices:List<decimal>)=
            let alpha = nToAlpha n
            // t-1: calculate average of first n-1 elements as initial value for the ema
            let tm1 =
                prices
                |> List.ofSeq
                |> Seq.take (n-1)
                |> Seq.average
            // create output list
            let ema : decimal array = Array.zeroCreate (List.length prices)
            // put initial ema value into output as first t-1 value
            ema.[n-2] <- tm1
            for i in n-1 .. List.length prices - 1 do
                let c = prices.[i]
                let prev = ema.[i-1]
                ema.[i] <- alpha * c + (1m - alpha) * prev
            Array.toSeq ema

        let emaFinancialFormula (n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
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

        let startCalculation (prices : System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, signals : System.Collections.Generic.List<int>)= 
            let n = 20

            let cPrices = [ for i in prices -> i.Item5 ]
            
            // Self-made function
            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema1 = ema (n, cPrices)
            stopWatch.Stop()
            printfn "Selfmade: %f" stopWatch.Elapsed.TotalMilliseconds

            // Financial Formula
            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema2 = emaFinancialFormula (n, prices)
            stopWatch.Stop()
            printfn "FinFormula: %f" stopWatch.Elapsed.TotalMilliseconds

            new System.Collections.Generic.List<int>()