namespace Algorithm
    module DecisionCalculator12=(*12*)
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

        let fadingStrategy(n:int, sigma:decimal, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            let bollinger = bollinger(n, sigma, prices)
            let mutable lastCross = 0
            // bollinger band breadth
            let mutable breadth:System.Collections.Generic.List<decimal> = new System.Collections.Generic.List<decimal>()
            // efficiency ratios
            let mutable ers:System.Collections.Generic.List<decimal> = new System.Collections.Generic.List<decimal>()

            // TODO: correct for counting
            for i = 0 to prices.Count - n do
                ers.Add (er (prices.GetRange (i, (int)n)))

            for i = 0 to bollinger.Length - 1 do
                // calculate bredth of the bands
                //let breadth = bollinger.[i].[0] - bollinger.[i].[1]
                breadth.Add(bollinger.[i].[0] - bollinger.[i].[1])
                
                // if er indicates sideways marktet
                if ers.[i] < 0.4m then
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
                else
                    signals.Add(0)
            signals

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            let n=20
            
            for i = 0 to n-2 do
                signals.Add(0)

            fadingStrategy (n, 1.5m, prices, signals)