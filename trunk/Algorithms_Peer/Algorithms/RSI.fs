namespace Algorithm
    module DecisionCalculator2=(*2*)
        
        let rsi(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            let mutable rsi = []
            for i = liste2D.Count - n to liste2D.Count - 1 do
                let mutable tempListe = liste2D.GetRange(i - n, n)
                let mutable wins = []
                let mutable losses = []
                for j = 0 to tempListe.Count - 1 do 
                    if tempListe.[j].Item2 < tempListe.[j].Item5 then 
                        wins <- List.append wins [tempListe.[j].Item5-tempListe.[j].Item2]
                    if tempListe.[j].Item5 < tempListe.[j].Item2 then 
                        losses <- List.append losses [tempListe.[j].Item5-tempListe.[j].Item2]
                rsi <- List.append rsi [100m*(List.sum(wins)/(decimal wins.Length))/((List.sum(wins)/(decimal wins.Length))-(List.sum(losses)/(decimal losses.Length)))]
            rsi

        let rsi_close(n:int, prices:decimal list)=
            let up = Array.zeroCreate (prices.Length-n)
            let down = Array.zeroCreate (prices.Length-n)
            
            let firstUp = Array.zeroCreate (n)
            let firstDown = Array.zeroCreate (n)
            // price changes from one bar to the next
            let diffs = Array.zeroCreate prices.Length
            for i in 1..diffs.Length-1 do
                diffs.[i] <- prices.[i] - prices.[i-1]
            
            // calculate first value for up and down    
            for i in 0..n-1 do
                firstUp.[i] <- List.max [diffs.[i+1] ; 0m]
                firstDown.[i] <- List.max [-1m*diffs.[i+1] ; 0m]
            // first value is the n+1.th; e.g. 15. (because of use of price changes)
            // -> n price changes are available
            up.[0] <- Array.average firstUp
            down.[0] <- Array.average firstDown

            // calculate all remaining values using welles wilder' smoothing method
            up
            |> Array.iteri (fun i p -> 
                match i with
                | _ when i > 0 ->
                    up.[i]   <- (up.[i-1]*decimal(n-1)   + List.max [diffs.[i] ; 0m])/decimal(n)
                    down.[i] <- (down.[i-1]*decimal(n-1) + List.max [-1m*diffs.[i] ; 0m])/decimal(n)
                | _            -> ignore i)
            
            // 
            (up, down) 
            ||> Array.map2 (fun u d -> 100m*u/(u+d))
            |> Array.append (Array.zeroCreate (n))

        let rsi_neu(n:int, prices:(decimal*decimal)[])=
            let priceChanges = prices |> Array.map (fun bar -> snd bar - fst bar)
            
            let up = Array.zeroCreate (prices.Length-n+1)
            let down = Array.zeroCreate (prices.Length-n+1)
            let firstUp = Array.zeroCreate (n)
            let firstDown = Array.zeroCreate (n)
            
            // calculate first value for up and down    
            for i in 0..n-2 do
                firstUp.[i]   <- List.max [    priceChanges.[i]   ; 0m]
                firstDown.[i] <- List.max [-1m*priceChanges.[i+1] ; 0m]
            // first value is the n.th; e.g. 14.
            up.[0]   <- Array.average firstUp
            down.[0] <- Array.average firstDown

            // calculate all remaining values using welles wilder' smoothing method
            up
            |> Array.iteri (fun i p -> 
                match i with
                | _ when i > 0 ->
                    up.[i]   <- (up.[i-1]  *decimal(n-1) + List.max [    priceChanges.[i] ; 0m])/decimal(n)
                    down.[i] <- (down.[i-1]*decimal(n-1) + List.max [-1m*priceChanges.[i] ; 0m])/decimal(n)
                | _ -> ignore i)
            
            // 
            (up, down) 
            ||> Array.map2 (fun u d -> 100m*u/(u+d))
            |> Array.append (Array.zeroCreate (n-1))

        let rsi_peer(n:int, prices:decimal list)=
            //let up = Array.zeroCreate prices.Length

            let up = [|
                for v in Seq.windowed n prices ->
                    v
                    |> Array.mapi (fun i p ->
                        match i with
                        | _ when i>0 ->
                            List.max [v.[i]-v.[i-1] ; 0m]
                        | _          -> 0m) //).[n-1]|]
                    |> Array.average |]

            let down = [|
                for v in Seq.windowed n prices ->
                    -1m * (v
                    |> Array.mapi (fun i p ->
                        match i with
                        | _ when i>0 ->
                            List.min [v.[i]-v.[i-1] ; 0m]
                        | _          -> 0m) //).[n-1]|]
                    |> Array.average)|]

            (up, down) 
            ||> Array.map2 (fun u d -> 100m*u/(u+d))
            //||> Array.map2 (fun x y -> 100m-100m / (1m + (x / y)))
            |> Array.append (Array.zeroCreate (n-1))
            
        let rsiFinancialFormula(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
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
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.RelativeStrengthIndex,(string n),"historicalData:Y3","indicator")
            let ergebnis1 = Array.create prices.Count 0m
            for i in 0 .. (n - 2) do
                ergebnis1.[i] <- 0m
            for i in 0 .. c1.Series.["indicator"].Points.Count - 1 do
                ergebnis1.[i+n-1] <- decimal c1.Series.["indicator"].Points.[i].YValues.[0]
                //ergebnis1 <- List.append ergebnis1 [[decimal c1.Series.["bbh"].Points.[i].YValues.[0]; decimal c1.Series.["bbl"].Points.[i].YValues.[0]]]
            ergebnis1

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

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, signals:System.Collections.Generic.List<int>)= 
            let n=14

            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.toList
            let ocPrices =
                [ for i in prices -> (i.Item2, i.Item5) ]
                |> Seq.toArray

            let rsi1 = rsiFinancialFormula(n, prices)
            let rsi2 = rsi(n, prices)
            let rsi3 = rsi_peer(n, cPrices)
            let rsi4 = rsi_neu(n, ocPrices)

            let mutable s1 = 0m
            let mutable s2 = 0m
            for i in 0..100 do
                printfn "%d: %f\t %f\t %f\t new: %f %f" i rsi1.[i] rsi3.[i] (rsi1.[i] - rsi3.[i]) rsi4.[i] (rsi1.[i] - rsi4.[i])
                s1 <- s1 + (rsi1.[i] - rsi3.[i])
                s2 <- s2 + (rsi1.[i] - rsi4.[i])

            s1 <- s1/100m
            s2 <- s2/100m
            printfn "Mean deviation: %f\t%f" s1 s2
            
            signals