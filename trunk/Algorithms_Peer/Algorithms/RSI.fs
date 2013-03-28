namespace Algorithm
    module DecisionCalculator=(*2*)
        
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

        let rsi_peer(n:int, prices:decimal list)=
            //let up = Array.zeroCreate pr
            let up = [|
                for v in Seq.windowed n prices ->
                    v
                    |> Array.mapi (fun i p ->
                        match i with
                        | _ when i>0 ->
                            List.max [v.[i]-v.[i-1] ; 0m]
                        | _          -> 0m)
                    |> Array.sum|]

            let down = [|
                for v in Seq.windowed n prices ->
                    -1m * (v
                    |> Array.mapi (fun i p ->
                        match i with
                        | _ when i>0 ->
                            List.min [v.[i]-v.[i-1] ; 0m]
                        | _          -> 0m)
                    |> Array.sum)|]

            (up, down) 
            ||> Array.map2 (fun x y -> 100m-100m / (1m + x/y))
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

            let rsi1 = rsiFinancialFormula(n, prices)
            let rsi2 = rsi(n, prices)
            let rsi3 = rsi_peer(n, cPrices)
            
            signals