namespace Algorithm

    module List =
       let rec skip n xs = 
          match (n, xs) with
          | 0, _ -> xs
          | _, [] -> []
          | n, _::xs -> skip (n-1) xs

    module DecisionCalculator16=

        let alphaToN (a) : int=
            int ((2.0m/a)-1.0m)
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

        let ema (n:int, prices:List<decimal>)=
            let alpha = nToAlpha n
            // t-1: calculate average of first n-1 elements as initial value for the ema
            let tm1 =
                prices
                |> Seq.take (n-1)
                |> Seq.average
            // create output array
            let ema : decimal array = Array.zeroCreate (List.length prices)
            // put initial ema value into output as first t-1 value
            ema.[n-2] <- tm1
            // calculate ema for each price in the list
            prices
            |> List.iteri (fun i p -> 
                match i with
                //| _ when i < n-2 -> ema.[i] <- 0m
                | _ when i > n-2 -> ema.[i] <- alpha * p + (1m - alpha) * ema.[i-1]
                | _              -> ignore i)
            // set initial ema value (sma) to 0
            ema.[n-2] <- 0m
            Array.toList ema

        let emaFor (n:int, prices:List<decimal>)=
            let alpha = nToAlpha n
            // t-1: calculate average of first n-1 elements as initial value for the ema
            let tm1 =
                prices
                |> Seq.take (n-1)
                |> Seq.average
            // create output array
            let ema : decimal array = Array.zeroCreate (List.length prices)
            // put initial ema value into output as first t-1 value
            ema.[n-2] <- tm1
            for i in n-1 .. List.length prices - 1 do
                let c = prices.[i]
                let prev = ema.[i-1]
                ema.[i] <- alpha * c + (1m - alpha) * prev
            // set initial ema value (sma) to 0
            ema.[n-2] <- 0m
            Array.toList ema

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
            let mutable ergebnis1 = [ for _ in 0 .. (n - 2) -> 0m ]
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            ergebnis1

        let startCalculation (prices : System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, signals : System.Collections.Generic.List<int>)= 
            let n = 20
            let skip = if signals.Count-n-1 > 0 then signals.Count+1-n else 0

            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> List.ofSeq
                |> Seq.skip skip
                |> Seq.toList

            // Self made
            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema1 = ema (n, cPrices)
            for i in ema1 do
                i |> ignore
            stopWatch.Stop()
            printfn "List: %f" (stopWatch.Elapsed.TotalMilliseconds / 1000.0)

            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema2 = emaFor (n, cPrices)
            for i in ema2 do
                i |> ignore
            stopWatch.Stop()
            printfn "For: %f" (stopWatch.Elapsed.TotalMilliseconds / 1000.0)

            // Financial Formula
            let stopWatch = System.Diagnostics.Stopwatch.StartNew()
            let ema3 = emaFinancialFormula (n, prices.GetRange(skip, prices.Count-skip))
            for i in ema3 do
                i |> ignore
            stopWatch.Stop()
            printfn "FinFormula: %f" (stopWatch.Elapsed.TotalMilliseconds / 1000.0)

//            printfn "Signals i: %d %e" signals.[50] (Seq.nth (50-signals.Count) ema1)
//
//            for i in 0 .. 50 do
//                printfn "%e" (Seq.nth i ema1 - ema2.[i])

            new System.Collections.Generic.List<int>()