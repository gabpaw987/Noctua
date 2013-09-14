namespace Algorithm
    module DecisionCalculator0007=(*99*)

        (*
         * Divides one value by another
         * Returns 0 if denominator is 0
         *)
        let divideZero denom nom =
            match denom with
            | 0m -> 0m
            | _ -> nom/denom
        
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
                | _ when i > n-2 -> ema.[i] <- alpha * p + (1m - alpha) * ema.[i-1]
                | _              -> ignore i)
            // set initial ema value (sma) to 0
            ema.[n-2] <- 0m
            ema

        // Efficiency Ratio
        // ER = (total price change for period) / (sum of absolute price changes for each bar)
        let er (prices:decimal[]) =
            let totalPriceChange = abs (prices.[prices.Length-1] - prices.[0])
            let mutable cumulativePriceChange = 0m
            let mutable oldPrice = prices.[0]
            for i = 1 to prices.Length-1 do
                cumulativePriceChange <- cumulativePriceChange + abs (prices.[i] - oldPrice)
                oldPrice <- prices.[i]
            if cumulativePriceChange <> 0m then
                totalPriceChange / cumulativePriceChange
            else
                0m

        // let mutable ers:System.Collections.Generic.List<decimal> = new System.Collections.Generic.List<decimal>()

        // C = [(ER * (SCF – SCS)) + SCS]2
        // Where:
        // SCF is the exponential constant for the fastest EMA allowable (usually 2)
        // SCS is the exponential constant for the slowest EMA allowable (often 30)
        // ER is the efficiency ratio that was noted above
        let c (alpha1:decimal, alpha2:decimal, prices:decimal[]) =
            // ers.Add (er (prices))
            let er = er (prices)
            //pown (er * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2))) 2
            // double(ers.[ers.Count-1] * (nToAlpha (decimal(n1)) - nToAlpha (decimal(n2))) + nToAlpha (decimal(n2)))**0.75
            double(er * (alpha1 - alpha2) + alpha2)**1.00

        let ama (erp:int, n1:int, n2:int, prices:decimal[])=
            // initial period
            let n = if (erp > n2) then erp else n2
            // calculate alphas for er
            let alpha1 = nToAlpha n1
            let alpha2 = nToAlpha n2
            // t-1: calculate average of first n2-1 elements as initial value for the ema
            let tm1 =
                prices
                |> Seq.take (n2-1)
                |> Seq.average
            // create output array
            let ama : decimal array = Array.zeroCreate (prices.Length)
            // put initial ama value (sma) into output as first t-1 value
            ama.[n-2] <- tm1
            // calculate ama
            prices
            |> Array.iteri (fun i p -> 
                match i with
                | _ when i > n-2 ->
                    let c = decimal (c (alpha1, alpha2, prices.[i-erp+1..i]))
                    //printfn "%d-%d\t c: %d" n1 n2 (alphaToN c)
                    ama.[i] <- c * p + (1m - c) * ama.[i-1]
                    //ama.[i] <- decimal(alphaToN c)
                    //printfn "%d" i
                | _              -> ignore i)
            // set initial ama value (sma) to 0
            ama.[n-2] <- 0m
            ama

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
            let ergebnis1 = Array.create prices.Count (0m,0m)
            for i in 0 .. (n - 2) do
                ergebnis1.[i] <- (0m, 0m)
            for i in 0 .. c1.Series.["bbh"].Points.Count - 1 do
                ergebnis1.[i+n-1] <- (decimal c1.Series.["bbh"].Points.[i].YValues.[0], decimal c1.Series.["bbl"].Points.[i].YValues.[0])
                //ergebnis1 <- List.append ergebnis1 [[decimal c1.Series.["bbh"].Points.[i].YValues.[0]; decimal c1.Series.["bbl"].Points.[i].YValues.[0]]]
            ergebnis1

        (* return [supportlevel2, supportlevel1, pivotpoint, resistancelevel1, resistancelevel2]*)
        let pivotpointcalcultor(n : int, prices : System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) = 
            //let pvpts : decimal[][] = Array.zeroCreate ((int)(floor((decimal prices.Count)/(decimal n))))
            let pvpts : decimal[][] = Array.zeroCreate prices.Count
            for i in 0..n-1 do
                pvpts.[i] <- [|0m; 0m; 0m; 0m; 0m|]
            for i in 0..n..prices.Count-n do
                let bars = prices.GetRange(i,n)

                let o = bars.[0].Item2
                let h = List.max [for j in bars -> j.Item3]
                let l = List.min [for j in bars -> j.Item4]
                let c = bars.[bars.Count-1].Item5
                let pivot = (h+l+c)/3m
                let supportlevel1 = 2m*pivot - h
                let resistancelevel1 = 2m*pivot - l
                let supportlevel2 = pivot - (resistancelevel1 - supportlevel1)
                let resistancelevel2 = (pivot - supportlevel1) + resistancelevel1

                if (i + 2*(n-1) < pvpts.Length) then
                    for j in 0..n-1 do
                        pvpts.[n-1 + i + j] <- [|supportlevel2; supportlevel1; pivot; resistancelevel1; resistancelevel2|]
                else
                    for j in 0..(pvpts.Length - (n-1 + i) - 1) do
                        pvpts.[n-1 + i + j] <- [|supportlevel2; supportlevel1; pivot; resistancelevel1; resistancelevel2|]
            pvpts

        (*
            calculates the momentum over the given time period
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

        (*
         * RSI using Welles Wilder's original smoothing technique and close-open
         * price changes
         *)
        let rsi(n:int, prices:(decimal*decimal)[])=
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
            ||> Array.map2 (fun u d -> 
                if (u = 0m && d = 0m) then
                    50m
                else
                    100m*u/(u+d))
            |> Array.append (Array.zeroCreate (n-1))

        (*
         * Calculates the Directional Movement 
         * dependent on which char is given(+/-) the positive or negative dm
         *)
        let calculateDm(decision:char, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos / neg)
            let dm = Array.zeroCreate prices.Count
            dm.[0] <- 0m
            for i = 1 to prices.Count - 1 do
                if(decision.Equals('+')) then
                    if(prices.[i].Item5 > prices.[i-1].Item5) then
                        dm.[i] <- prices.[i].Item3 - prices.[i-1].Item3
                    else 
                        dm.[i] <- 0m
                else if(decision.Equals('-')) then
                    if (prices.[i].Item5 < prices.[i-1].Item5) then 
                        dm.[i] <- prices.[i-1].Item4 - prices.[i].Item4
                    else 
                        dm.[i] <- 0m
                else 
                    dm.[i] <- 0m
                if(dm.[i] < 0m) then
                    dm.[i] <- 0m
            Array.toList dm

        (*
         * Calculates the True Range
         * the tr is the highest of:
         * Today's High - Today's Low,
         * Today's High - Yesterday's Close, and
         * Yesterday's Close - Today's Low
         *)
        let calculateTr(prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let tr = Array.zeroCreate prices.Count
            tr.[0] <- 0m
            for i = 1 to prices.Count - 1 do
                let tr1 = prices.[i].Item3-prices.[i].Item4
                let tr2 = prices.[i].Item3-prices.[i-1].Item5
                let tr3 = prices.[i-1].Item5-prices.[i].Item4
                let max = [tr1;tr2;tr3] |> List.max
                tr.[i] <- max
            Array.toList tr

        let adx(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos & neg)
            let posdm = ema (n, calculateDm ('+', prices))
            let negdm = ema (n, calculateDm ('-', prices))
            let tr    = ema (n, calculateTr (prices))

            let posdi = Array.zeroCreate (prices.Count)
            let negdi = Array.zeroCreate prices.Count
            for i in n .. posdm.Length-1 do
                posdi.[i] <- 100m*(posdm.[i] |> divideZero tr.[i])
                negdi.[i] <- 100m*(negdm.[i] |> divideZero tr.[i])


            // calculate the difference between the two indicators as a positive number
            let dx = [ for x in 0 .. prices.Count - 1 ->   100m*((abs(posdi.[x] - negdi.[x]))) |> divideZero (abs(posdi.[x] + negdi.[x])) ]
            let adx = ema(n*2, dx)
            adx

        (*
         * This Bollinger Bands Indicator relativises the price's position
         * between the upper and lower band on a range between -100 to 100
         *)
        let bInd(prices:decimal[], bollinger:(decimal*decimal)[]) =
            let bInd = Array.zeroCreate prices.Length
            for i in 0 .. prices.Length-1 do
                // ((price-lbb)/(breadth)) * 200 -100
                bInd.[i] <- (((prices.[i] - (bollinger.[i] |> snd)) |> divideZero ((bollinger.[i] |> fst) - (bollinger.[i] |> snd))) * 200m) - 100m
            bInd

        let regression(prices:decimal array)=
            let mutable xy = 0m
            let mutable xx = 0m
            let mutable x = 0m
            let mutable y = 0m
            for i = 0 to prices.Length - 1 do 
                x <- x+decimal i
                y <- y + prices.[i]
                xy <- xy + prices.[i] * decimal i
                xx <- xx + decimal i* decimal i
            let b = (decimal prices.Length * xy - (x*y))/(decimal prices.Length * xx - x*x)
            //let a = (y - b*x)/decimal prices.Length
            //decimal liste2D.Count*b + a
            b

        (* Linear Weighted Regression Method *)
        let lwrm(n:int, prices:decimal array)=
            let mutable xy = 0m
            let mutable xx = 0m
            let mutable x = 0m
            let mutable y = 0m
            let w = [|for i in 1 .. prices.Length do yield decimal i*(decimal n / decimal prices.Length)|]
            let wsum = Array.sum w
            for i = 0 to prices.Length - 1 do 
                x <- x + (((decimal i)*w.[i])/wsum)
                y <- y + (((prices.[i])*w.[i])/wsum)
            for i = 0 to prices.Length - 1 do 
            // (lr.X - xAvg) * (lr.Y - yAvg) * lr.Weight);
                xy <- xy + ((decimal i - x) * (prices.[i] - y) * w.[i])
            // System.Math.Pow(Convert.ToDouble(lr.X - xAvg), 2)) * lr.Weight)
                xx <- xx + decimal ((float i - float x)**2.0 ) * w.[i]
            
            xy / xx

        (*
         * Calculated the mean deviation
         *)
        let std(data:decimal[])=
            let avg = Array.average data
            let var = (data |> Array.sumBy (fun x -> pown (x-avg) (2))) / (decimal)data.Length
            decimal (sqrt (double var))

        let movingStd(n:int, data:decimal[])=
            let windows = Seq.windowed n data
            Seq.map std windows
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        (*
         * returns the absolute maximum (w.o. sign) for the last n bars
         * for each element
         *)
        let mExtremum(n:int, data:decimal[])=
            let windows = Seq.windowed n [| for i in data -> abs i |]
            Seq.map Seq.max windows
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        (*
         * returns the maximum and minimum for the last n bars
         * for each element
         *)
        let mExtrema(n:int, data:decimal[])=
            let windows = Seq.windowed n data |> Seq.toArray

            let maxVals = 
                Array.map Array.max windows
            let minVals = 
                Array.map Array.min windows

            [| for i in 0..windows.Length-1 -> (minVals.[i], maxVals.[i]) |]
            |> Array.append (Array.create (n-1) (0m, 0m))

        (*
         * calculates the average bar size from the last n bars
         *)
        let avgBarSize(n:int, prices:(decimal*decimal)[])=
            let windows = Seq.windowed n [| for i in prices -> abs (snd i - fst i) |]
            Seq.map Seq.average windows
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        let strategy(erp:int,
                     s1:int, s2:int,
                     m1:int, m2:int,
                     l1:int, l2:int,
                     n:int, sigma:decimal,
                     cutloss:decimal, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,
                     signals:System.Collections.Generic.List<int>,
                     chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                     chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>)=
            // skip already calculated signals
            let skip = if signals.Count-erp+1 > 0 then signals.Count-erp+1 else 0

            // list of closing prices (skipped)
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                //|> Seq.skip skip
                |> Seq.toArray
            // list of open/close price tuples
            let ocPrices = 
                [ for i in prices -> (i.Item2, i.Item5) ]
                //|> Seq.skip skip
                |> Seq.toArray
            // list of high/low price tuples
            let hlPrices =
                [| for i in prices -> (i.Item3, i.Item4) |]

            // skiped list of prices
            //let prices = prices.GetRange(skip, prices.Count-skip)
            printfn "Skipped %d" skip

            // bollinger bands
            let bollinger = bollinger(n, sigma, prices)
            // add bollinger bands to chart1
            for i in 0..bollinger.Length-1 do chart1.["BollingerTop;#4769FF"].Add(fst bollinger.[i])
            for i in 0..bollinger.Length-1 do chart1.["BollingerBottom;#4769FF"].Add(snd bollinger.[i])
            // bollinger band breadth
            let breadth = [| for i in bollinger -> (i |> fst) - (i |> snd) |]
            // position of price between bollinger bands
            let bInd = (cPrices, bollinger) |> bInd
            //let bInd = cPrices bollinger
            let mutable lastCross = 0
            printfn "Finished Bollinger Bands"

            // mean deviation
            let std = movingStd (30, cPrices)
            // change of mean deviation
            let stdDiff = [ for i in 0..std.Length-1 -> (if (i=0 || std.[i-1] = 0m) then (0m) else (100m*(std.[i] - std.[i-1])/std.[i-1])) ]
            // exponentially smoothed change of mean deviation
            let phase = ema (30, stdDiff)
            // add to chart2
            //for i in 0..phase.Length-1 do chart2.["StdDiff;#FF0000"].Add(phase.[i])

            // pivot points
            let pvpts = pivotpointcalcultor(14,prices)
            for i in 0..pvpts.Length-1 do chart1.["PvptsTop1;#FFA600"].Add(pvpts.[i].[1])
            for i in 0..pvpts.Length-1 do chart1.["PvptsBottom1;#FFA600"].Add(pvpts.[i].[3])

            // amas for triple crossing in trend phases
            let short = ama(erp, s1, s2, cPrices)
            printfn "Finished short AMA"
            let middle = ama(erp, m1, m2, cPrices)
            printfn "Finished middle AMA"
            let long = ama(erp, l1, l2, cPrices)
            printfn "Finished long AMA"
            // add AMAs to chart1
            for i in 0..short.Length-1 do chart1.["AMAShort;#FF0000"].Add(short.[i])
            for i in 0..middle.Length-1 do chart1.["AMAMiddle;#8A0000"].Add(middle.[i])
            for i in 0..long.Length-1 do chart1.["AMALong;#737373"].Add(long.[i])

            // rsi
            let rsiN = 30
            let rsi = rsi(rsiN, ocPrices)
            // add rsi to chart2
            //for i in 0..long.Length-1 do chart2.["RSI;#0095FF"].Add(rsi.[i])

            // rsi regression
            let rsiRegrN = 10
            let rsiRegr = 
                [|for i in rsiRegrN+rsiN - 2..rsi.Length-1 -> regression(rsi.[i-rsiRegrN+1..i])|]
                |> Array.append (Array.zeroCreate (rsiRegrN+rsiN-2))
            // add rsiRegr to chart2
            for i in 0..long.Length-1 do chart2.["RSI;#0095FF"].Add(rsiRegr.[i])

            // 7 bar short regression of prices
            let regrSN = 7
            let regrS =
                [|for i in regrSN-1..cPrices.Length-1 -> regression(cPrices.[i-regrSN+1..i])|]
                |> Array.append (Array.zeroCreate (regrSN-1))
            
            // 14 bar medium regression of prices
            let regrMN = 14
            let regrM =
                [|for i in regrMN-1..cPrices.Length-1 -> regression(cPrices.[i-regrMN+1..i])|]
                |> Array.append (Array.zeroCreate (regrMN-1))
            
            // 60 bar long regression of prices
            let regrLN = 60
            let regrL =
                [|for i in regrLN-1..cPrices.Length-1 -> regression(cPrices.[i-regrLN+1..i])|]
                |> Array.append (Array.zeroCreate (regrLN-1))
            
            // add regressions to chart
//            for i in 0..regrS.Length-1 do chart2.["R1;#0095FF"].Add(regrS.[i]*100m)
//            for i in 0..regrM.Length-1 do chart2.["R2;#FF0000"].Add(regrM.[i]*100m)
//            for i in 0..regrL.Length-1 do chart2.["R3;#00FF00"].Add(regrL.[i]*100m)

            // maximum slope of 14 day regression over 20 days
            let slopeM = [| for i in mExtremum (20, regrM) -> i*100m |]
            for i in 0..slopeM.Length-1 do chart2.["slopeM;#0095FF"].Add(slopeM.[i])
            
            // adx 7 (short)
            let adxS = adx (7, prices)
            // add adx7 to chart2
            //for i in 0..long.Length-1 do chart2.["ADXShort;#FF006E"].Add(adxS.[i])
            // adx 14 (medium)
            let adxM = adx (20, prices)
            // add adx14 to chart2
            //for i in 0..long.Length-1 do chart2.["ADXMiddle;#4C0021"].Add(adxM.[i])
            printfn "Finished ADX"

            // erp 14 (medium)
            let erMN = 14
            let erM = 
                [| for i in erMN-1..cPrices.Length-1 -> er (cPrices.[i-erMN+1..i]) |]
                |> Array.append (Array.zeroCreate (erMN-1))
            // add to chart2
            //for i in 0..erM.Length-1 do chart2.["ERM;#FF0000"].Add(erM.[i])

            // erp 30 (long)
            let erLN = 30
            let erL = 
                [| for i in erLN-1..cPrices.Length-1 -> er (cPrices.[i-erLN+1..i]) |]
                |> Array.append (Array.zeroCreate (erLN-1))
            // add to chart2
            //for i in 0..erL.Length-1 do chart2.["ERL;#00FF00"].Add(erL.[i])

            // price extrema
            let pExtrema = mExtrema (60, cPrices)
            // ((price-minimum)/(breadth)) * 200 -100
            let pPos = [| for i in 0..cPrices.Length-1 -> ((cPrices.[i]-fst pExtrema.[i]) |> divideZero (snd pExtrema.[i]-fst pExtrema.[i]))* 100m |]

            // relative size of current bar compared to avg
            let mutable relBarSize = 0m
            // average bar size
            let avgBars = avgBarSize (14, hlPrices)

            // first index with all data
            let firstI = [erp-1; l2-1; n-1] |> List.max
            let mutable missingData = firstI+1
            // no signals in the list yet
            let mutable allZero = true
            
            // price at trade entry (long or short)
            let mutable entryPrice = 0m
            // price extreme in trade for cut loss
            let mutable priceExtreme = cPrices.[0]

            let mutable sw = 0
            let mutable trend = 0
            let mutable cutlossCount = 0

            let mutable rsiSig = 0
            let mutable amaSig = 0
            let mutable exit = false
            // additional space between AMAs before signal
            let signalFilter = 0m
            // maximum space between short and middle AMA
            let mutable maxAMADiff = 0m

            printfn "Signal count: %d" signals.Count;
            printfn "Prices count: %d" prices.Count;

            let mutable posDur = 0
            let mutable savedSignal = 0

            // TODO: remove!
            // recalculate all signals:
            signals.Clear();
            for i in signals.Count .. prices.Count-1 do
                
                // SIGNAL CALCULATION
                
                // Bollinger
                // check if price has crossed Bollinger Bands
                if bollinger.[i] |> fst <> 0m then
                    if cPrices.[i] > (bollinger.[i] |> fst) then
                        lastCross <- -1
                    else if cPrices.[i] < (bollinger.[i] |> snd) then
                        lastCross <- 1

                // AMA
                amaSig <-
                    // short under middle and long
                    if short.[i] + (cPrices.[i]*signalFilter) < middle.[i] && short.[i] + (cPrices.[i]*signalFilter) < long.[i] then
                        // & middle under long
                        if middle.[i] < long.[i] then
                            -2
                        else
                            -1
                    // short over middle and long
                    else if short.[i] - (cPrices.[i]*signalFilter) > middle.[i] && short.[i] - (cPrices.[i]*signalFilter) > long.[i] then
                        // & middle over long
                        if middle.[i] > long.[i] then
                            2
                        else
                            1
                    else
                        0

//                // AMA signal in the same direction as before (<> 0)
//                if (amaSig <> 0 && (sign amaSig = sign maxAMADiff || maxAMADiff = 0m)) then
//                    // difference between short and middle AMA grew bigger
//                    if (abs (short.[i] - middle.[i]) > abs maxAMADiff) then
//                        maxAMADiff <- short.[i] - middle.[i]
//                // changed direction or 0
//                else
//                    maxAMADiff <- 0m
//
//                // AMA difference smaller than 50% of max
//                if abs (short.[i] - middle.[i])*4m < abs maxAMADiff then
//                    amaSig <- (sign amaSig) * -1

                // RSI
                if (rsiRegr.[i] > 1m && rsi.[i] < 80m) then
                    rsiSig <- 1 
                else if (rsiRegr.[i] < -1m && rsi.[i] > 20m) then
                    rsiSig <- -1
                else
                    rsiSig <- 0

                // EXIT SIGNAL (take profit)

                relBarSize <- if (avgBars.[i] = 0m) then 0m else (fst hlPrices.[i] - snd hlPrices.[i]) / avgBars.[i]

                if i <> 0 then
                    exit <- false
                    if (signals.[i-1] <> 0) then
                        // long
                        if (sign signals.[i-1] = 1) then
                            // rsi in overbought and currently positive profit and down movement of 0.8*averageBarSize
                            // or more than 80 profit
                            if (fst ocPrices.[i] > snd ocPrices.[i]) && (rsi.[i] > 70m && cPrices.[i] > entryPrice && relBarSize > 0.8m) || (cPrices.[i]-entryPrice > 0.8m) then
                                exit <- true
                        // short
                        else if (sign signals.[i-1] = -1) then
                            // rsi in oversold and currently positive profit and up movement of 0.8*averageBarSize
                            if (fst ocPrices.[i] < snd ocPrices.[i]) && (rsi.[i] < 30m && cPrices.[i] < entryPrice && relBarSize > 0.8m) || (cPrices.[i]-entryPrice < -0.8m) then
                                exit <- true

                // one bar more available
                missingData <- missingData - 1

                // Not all neccessary data available yet
                // (.. or new day)
                if i < firstI || missingData > 0 then
                    signals.Add(0)
                else
                    // TODO: sideways
                    // if ADX indicates sideways markets
                    if adxS.[i] < 20m && adxM.[i] < 20m then

                        sw <- sw+1
                        // print price between support2 and resistance2
                        //printfn "%f\t%f\t%f" pvpts.[i].[0] cPrices.[i] pvpts.[i].[4]
                        // price between bbs
                        if ((bollinger.[i] |> snd) < cPrices.[i] && cPrices.[i] < (bollinger.[i] |> fst)) then
                            // either with PVPTS:
                            // price recently was below lbb and now below 80 bInd and pvpts rl1
                            if lastCross = 1 && bInd.[i] < 80m && cPrices.[i] < pvpts.[i].[3] then
                                signals.Add(1)
                            // price recently was above hbb and now above -80 bInd and pvpts sl1
                            else if lastCross = -1 && bInd.[i] > -80m && cPrices.[i] > pvpts.[i].[1] then
                                signals.Add(-1)
                            else
                                signals.Add(0)
                        else
                            signals.Add(0)

                        // TODO: REMOVE!
                        signals.[i] <- 0

                    // trending market
//                    else if false then
                    else
                        
                        trend <- trend+1

                        // ama and rsi signal contradictory
                        if (abs rsiSig = 1 && sign rsiSig <> sign amaSig) then
                            //signals.Add(0)
                            //signals.Add(sign (amaSig * -1))

                            // strong AMA signal and short and medium price regressions in same direction
                            if (amaSig = 2 && regrS.[i] > 0m && regrM.[i] > 0m) || (amaSig = -2 && regrS.[i] < 0m && regrM.[i] < 0m)  then
                                signals.Add(sign amaSig)
                            else
                                // follow (strongest) price trend
                                // ..either short or medium term
                                signals.Add(
                                    // short and long term trend coincide
                                    if (regrS.[i] > 0m && regrM.[i] > 0m) then
                                        1
                                    else if (regrS.[i] < 0m && regrM.[i] < 0m) then
                                        -1
                                    // stronger short term trend
                                    else if (abs regrS.[i] > 1.5m*(abs regrM.[i])) then
                                        if (regrS.[i] > 0m) then 
                                            1
                                        else
                                            -1
                                    // stronger medium term trend
                                    else if (abs regrM.[i] > 1.5m*(abs regrS.[i])) then
                                        if (regrM.[i] > 0m) then
                                            1
                                        else
                                            -1
                                    else
                                        0
                                )
                        // AMA and RSI signal have same sign
                        else
                            // don't revert last decision if short term price trend is still active
                            // last signal fits short term price trend
                            if (sign regrS.[i] = signals.[i-1]) then
                                // keep last signal
                                signals.Add(signals.[i-1])
                            else
                                signals.Add(sign amaSig)

                        // don't decide against long term trend! (60)
                        // (better than only with changed signal)
                        if (sign regrL.[i] <> sign signals.[i]) then
                            // 0 (better than to keep the old signal)
                            signals.[i] <- 0

                        // changing signal (not 0)
                        if (signals.[i-1] <> signals.[i] && signals.[i] <> 0) then

                            // don't decide against short term trend! (7)
                            // new signal contradicts short term price trend
                            if (sign regrS.[i] <> sign signals.[i]) then
                                signals.[i] <- 0

                            // don't open new positions in price extremes
                            if (signals.[i] = 1 && pPos.[i] > 90m) || (signals.[i] = -1 && pPos.[i] < 10m) then
                                signals.[i] <- 0
                            
                        // don't decide against medium term trend for first signal
                        // check if signal is first signal
                        if allZero then
                            for j in signals do
                                if (j <> 0) then allZero <- false
                        
                        if (allZero && sign regrM.[i] <> sign signals.[i]) then
                            signals.[i] <- 0

                    // try to get out of the position at a profit
                    // signal change
//                    if (signals.[i-1] <> signals.[i] && abs signals.[i] = 1) then
//                        let tradeDiff = cPrices.[i] - entryPrice
//                        // signal changing to sell
//                        if (signals.[i] = -1 && tradeDiff < 0m && tradeDiff > -0.1m*cPrices.[i]) then
//                            // stay on buy
//                            signals.[i] <- 1
//                        // signal changing to buy
//                        else if (signals.[i] = 1 && tradeDiff > 0m && tradeDiff < 0.1m*cPrices.[i]) then
//                            // stay on sell
//                            signals.[i] <- -1

                    // Signal Smoothing:    signals can only get bigger, neutral or in the other direction
                    // .. not needed for -1/0/1 signals
                    // Cutloss:             neutralise if loss is too big (% of price movement!)

                    // same sign: signal now and last bar
                    if (sign signals.[i] = sign signals.[i-1]) then
                        // cut loss: price extreme
                        if (decimal(sign signals.[i]) * cPrices.[i] > decimal(sign signals.[i-1]) * cPrices.[i-1]) then
                            priceExtreme <- cPrices.[i]

//                        // signal smoothing:
//                        // weaker signal than before
//                        if (abs signals.[i] < abs signals.[i-1]) then
//                            // save stronger signal from before as present signal
//                            signals.[i] <- signals.[i-1]

                    // new buy or sell signal (different direction)
                    if (signals.[i] <> 0 && sign signals.[i] <> sign signals.[i-1]) then
                        entryPrice <- cPrices.[i]
                        // reset priceExtreme for new trade
                        priceExtreme <- cPrices.[i]
                    // same trading direction (-/+)
                    else if (signals.[i] <> 0) then
                        // check cut loss:
                        if (abs (priceExtreme - cPrices.[i]) > cutloss*0.01m*entryPrice) then
                            // neutralise -> liquidate
                            signals.[i] <- 0
                            cutlossCount <- cutlossCount + 1
                            printfn "Cut loss with loss of %f > cut loss of %f" (priceExtreme - cPrices.[i]) (cutloss*0.01m*entryPrice)

                    // exit end of day (EOD0)
                    if (prices.[i].Item1.Hour = 21 && prices.[i].Item1.Minute = 59) then
                        signals.[i] <- 0
                        // start trading on new day only with enough new-day data
                        missingData <- firstI + 1
                    // only trade between 15:30 and 22:00
                    if (prices.[i].Item1.Hour < 15 || prices.[i].Item1.Hour > 21 || (prices.[i].Item1.Hour = 15 && prices.[i].Item1.Minute < 30)) then
                        signals.[i] <- 0
                        // start trading on new day only with enough new-day data
                        missingData <- firstI + 1

                    //printfn "Signal: %d\t AMA:%d\t RSI:%d\t ADX:%f" signals.[signals.Count-1] amaSig rsiSig adx.[i]
            printfn "Trending decisions: %d" trend
            printfn "Sideways decisions: %d" sw
            printfn "Cut Losses: %d" cutlossCount
            signals

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>)=
            
            chart1.Add("AMAShort;#FF0000", new System.Collections.Generic.List<decimal>()) 
            chart1.Add("AMAMiddle;#8A0000", new System.Collections.Generic.List<decimal>()) 
            chart1.Add("AMALong;#737373", new System.Collections.Generic.List<decimal>()) 
            chart1.Add("BollingerTop;#4769FF", new System.Collections.Generic.List<decimal>())
            chart1.Add("BollingerBottom;#4769FF", new System.Collections.Generic.List<decimal>())
            chart1.Add("PvptsTop1;#FFA600", new System.Collections.Generic.List<decimal>())
            chart1.Add("PvptsBottom1;#FFA600", new System.Collections.Generic.List<decimal>())
            
            chart2.Add("R1;#0095FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("R2;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("R3;#00FF00", new System.Collections.Generic.List<decimal>())
            chart2.Add("slopeM;#0095FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI;#0095FF", new System.Collections.Generic.List<decimal>())
            //chart2.Add("StdDiff;#FF0000", new System.Collections.Generic.List<decimal>())
            //chart2.Add("ERM;#FF0000", new System.Collections.Generic.List<decimal>())
            //chart2.Add("ERL;#00FF00", new System.Collections.Generic.List<decimal>())
            //chart2.Add("ADXShort;#FF006E", new System.Collections.Generic.List<decimal>())
            //chart2.Add("ADXMiddle;#4C0021", new System.Collections.Generic.List<decimal>())
            
            //       erp  s1 s2  m1  m2  l1  l2  bN  sig cutloss
            //strategy (50, 5, 10, 10, 20, 20, 40, 20, 2m, 1m, prices, signals)
            //strategy (50, 10, 15, 20, 30, 30, 40, 20, 2m, 0.1m, prices, signals)
            //strategy (60, 10, 20, 15, 30, 30, 60, 20, 2m, 0.1m, prices, signals) // .. not good
            strategy (60,
                        // standard
                        10, 15, // s
                        18, 25, // m
                        30, 40, // l
                        // adapt to shorter
//                        5, 15, // s
//                        10, 25, // m
//                        20, 40, // l
                        // longer
//                        25, 40, // s
//                        30, 60, // m
//                        60, 90, // l
                        20, 2m, // bollinger
                        0.3m, prices, signals,
                        chart1, chart2)