﻿namespace Algorithm
    module DecisionCalculator=(*45*)

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

            // pivot points
            let pvpts = pivotpointcalcultor(14,prices)

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
            let rsiN = 14
            let rsi = rsi(rsiN, ocPrices)
            // add rsi to chart2
            for i in 0..long.Length-1 do chart2.["RSI;#0095FF"].Add(rsi.[i])

            // rsi regression
            let rsiRegrN = 10
            let rsiRegr = 
                [|for i in rsiRegrN+rsiN - 2..rsi.Length-1 -> regression(rsi.[i-rsiRegrN+1..i])|]
                |> Array.append (Array.zeroCreate (rsiRegrN+rsiN-2))

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
            
            // adx 7 (short)
            let adxS = adx (7, prices)
            // add adx7 to chart2
            for i in 0..long.Length-1 do chart2.["ADXShort;#FF006E"].Add(adxS.[i])
            // adx 14 (medium)
            let adxM = adx (14, prices)
            // add adx14 to chart2
            for i in 0..long.Length-1 do chart2.["ADXMiddle;#4C0021"].Add(adxM.[i])
            printfn "Finished ADX"

            // erp 14 (medium)
            let erMN = 14
            let erM = 
                [| for i in erMN-1..cPrices.Length-1 -> er (cPrices.[i-erMN+1..i]) |]
                |> Array.append (Array.zeroCreate (erMN-1))

            // test: add to chart2
            for i in 0..erM.Length-1 do chart2.["ER14;#FF0000"].Add(erM.[i])

            // erp 14 (long)
            let erLN = 30
            let erL = 
                [| for i in erLN-1..cPrices.Length-1 -> er (cPrices.[i-erLN+1..i]) |]
                |> Array.append (Array.zeroCreate (erLN-1))

            // first index with all data
            let firstI = [erp-1; l2-1; n-1] |> List.max
            let mutable missingData = firstI+1
            
            // price at trade entry (long or short)
            let mutable entryPrice = 0m
            // price extreme in trade for cut loss
            let mutable priceExtreme = cPrices.[0]

            let mutable sw = 0
            let mutable trend = 0
            let mutable cutlossCount = 0

            let mutable rsiSig = 0
            let mutable amaSig = 0
            // additional space between AMAs before signal
            let signalFilter = 0.0m
            printfn "Signal count: %d" signals.Count;
            printfn "Prices count: %d" prices.Count;

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

                // RSI
                if (rsiRegr.[i] > 0m && rsi.[i] < 80m) then
                    rsiSig <- 1 
                else if (rsiRegr.[i] < 0m && rsi.[i] > 20m) then
                    rsiSig <- -1
                else
                    rsiSig <- 0

                // one bar more available
                missingData <- missingData - 1

                // Not all neccessary data available yet
                // (.. or new day)
                if i < firstI || missingData > 0 then
                    signals.Add(0)
                else
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
                            // or without PVPTS:
//                            if lastCross = 1 && bInd.[i] < 0.0m then
//                                signals.Add(1)
//                            else if lastCross = -1 && bInd.[i] > 0.0m then
//                                signals.Add(-1)
//                            else
//                                signals.Add(0)
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

                            // follow (strongest) price trend
                            // ..either short or medium term
                            signals.Add(
                                // stronger short term trend
                                if (abs regrS.[i] > abs regrM.[i]) then
                                    if (regrS.[i] > 0m) then 1
                                    else -1
                                // stronger medium term trend
                                else
                                    if (regrM.[i] > 0m) then 1
                                    else -1
                            )
                        else
                            // don't revert last decision if short term price trend is still active
                            // last signal fits short term price trend
                            if (sign regrS.[i] = signals.[i-1]) then
                                // keep last signal
                                signals.Add(signals.[i-1])
                            else
                                signals.Add(sign amaSig)

                        // using rsi for ama prevalence
//                        if (abs amaSig = 1 && amaSig = rsiSig) then
//                            signals.Add(amaSig)
//                        else if (abs amaSig = 2) then
//                            signals.Add(sign amaSig)
//                        else
//                            signals.Add(0)

                    // TODO: REMOVE!
//                    else
//                        signals.Add(0)

                    // Signal Smoothing:    signals can only get bigger, neutral or in the other direction
                    // .. not needed for -1/0/1 signals
                    // Cutloss:             neutralise if loss is too big (% of price movement!)

                    // same sign: signal now and last bar
                    if (sign signals.[i] = sign signals.[i-1]) then
                        // cut loss: price extreme
                        if (decimal(sign signals.[i]) * cPrices.[i] > decimal(sign signals.[i-1]) * cPrices.[i-1]) then
                            priceExtreme <- cPrices.[i]

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
                        signals.[signals.Count-1] <- 0
                        // start trading on new day only with enough new-day data
                        missingData <- firstI + 1

                    // TODO: REMOVE!
//                    if (erM.[i] > 0.3m && erL.[i] > 0.3m) then
//                        signals.[i] <- 1
//                    else
//                        signals.[i] <- 0

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
            
            chart2.Add("RSI;#0095FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("ER14;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("ADXShort;#FF006E", new System.Collections.Generic.List<decimal>())
            chart2.Add("ADXMiddle;#4C0021", new System.Collections.Generic.List<decimal>())
            
            //       erp  s1 s2  m1  m2  l1  l2  bN  sig cutloss
            //strategy (50, 5, 10, 10, 20, 20, 40, 20, 2m, 1m, prices, signals)
            //strategy (50, 10, 15, 20, 30, 30, 40, 20, 2m, 0.1m, prices, signals)
            //strategy (60, 10, 20, 15, 30, 30, 60, 20, 2m, 0.1m, prices, signals) // .. not good
            strategy (50,
                        10, 15, // s
                        18, 25, // m
                        30, 40, // l
                        20, 2m, // bollinger
                        0.3m, prices, signals,
                        chart1, chart2)