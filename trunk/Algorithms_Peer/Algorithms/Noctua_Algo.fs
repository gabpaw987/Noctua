namespace Algorithm
    module DecisionCalculator=(*44*)

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
            totalPriceChange / cumulativePriceChange

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
            double(er * (alpha1 - alpha2) + alpha2)**0.75

        let ama (erp:int, n1:int, n2:int, prices:decimal[])=
            // initial period
            let n = if (erp > n2) then erp else n2
            // calculate alphas for er
            let alpha1 = nToAlpha n1
            let alpha2 = nToAlpha n2
            // t-1: calculate average of first n-1 elements as initial value for the ema
            let tm1 =
                prices
                |> Seq.take (n-1)
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
                    ama.[i] <- c * p + (1m - c) * ama.[i-1]
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
        let calculateDm(decision:char, n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos / neg)
            let dm = Array.zeroCreate prices.Count
            dm.[0] <- 0m
            for i = 1 to prices.Count - 1 do
                if(prices.[i].Item5 > prices.[i-1].Item5) then 
                    if(decision.Equals('+')) then
                        dm.[i] <- prices.[i].Item3 - prices.[i-1].Item3
                    else 
                        dm.[i] <- 0m
                else if (prices.[i].Item5 < prices.[i-1].Item5) then 
                    if(decision.Equals('-')) then
                        dm.[i] <- prices.[i-1].Item4 - prices.[i].Item4
                    else 
                        dm.[i] <- 0m
                else
                    dm.[i] <- 0m
            Array.toList dm

        (*
         * Calculates the True Range
         * the tr is the highest of:
         * Today's High - Today's Low,
         * Today's High - Yesterday's Close, and
         * Yesterday's Close - Today's Low
         *)
        let calculateTr(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            // calculate the directional movements (pos & neg)
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
            let posdm = ema (n, calculateDm ('+', n, prices))
            let negdm = ema (n, calculateDm ('-', n, prices))
            let tr    = ema (n, calculateTr (n, prices))

            let posdi = Array.zeroCreate prices.Count
            let negdi = Array.zeroCreate prices.Count
            for i in 0 .. posdm.Length-1 do
                posdi.[i] <- 100m*(posdm.[i] |> divideZero tr.[i])
                negdi.[i] <- 100m*(negdm.[i] |> divideZero tr.[i])

//            let l = (posdm14, negdm14, tr14) |||> List.zip3
//            let posdi14 = Array.zeroCreate prices.Count
//            let negdi14 = Array.zeroCreate prices.Count
//            let mutable i = 0
//            for (pos, neg, tr) in l do
//                posdi14.[i] <- 100m*(pos |> divideZero tr)
//                negdi14.[i] <- 100m*(neg |> divideZero tr)
//                i <- i+1

//            // calculate the directional indicators
//            // [ for x in 0 .. posdm14.Length -> posdm14.[x]/ tr14.[x] ]
//            let posdi14 = [ for x in 0 .. prices.Count - 1 -> 100m*(posdm14.[x] |> divideZero tr14.[x]) ]
//            let negdi14 = [ for x in 0 .. prices.Count - 1 -> 100m*(negdm14.[x] |> divideZero tr14.[x]) ]

            // calculate the difference between the two indicators as a positive number
            let dx = [ for x in 0 .. prices.Count - 1 -> 100m*(abs(posdi.[x] - negdi.[x])) |> divideZero (abs(posdi.[x] + negdi.[x])) ]
            let adx = ema(n, dx)
            adx

        (*
         * This Bollinger Bands Indicator relativises the price's position
         * between the upper and lower band on a range between -100 to 100
         *)
        let bInd(prices:decimal[], bollinger:(decimal*decimal)[]) =
            let bInd = Array.zeroCreate prices.Length
            for i in 0 .. prices.Length-1 do
                // ((price-lbb)/(breadth)) * 200 -100
                bInd.[i] <- ((prices.[i] - (bollinger.[i] |> snd)) |> divideZero ((bollinger.[i] |> fst) - (bollinger.[i] |> snd))) * 200m - 100m
            bInd

        let strategy(erp:int, s1:int, s2:int, m1:int, m2:int, l1:int, l2:int, n:int, sigma:decimal, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            // skip already calculated signals
            let skip = if signals.Count-n+1 > 0 then signals.Count-n+1 else 0

            // list of closing prices (skipped)
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.skip skip
                |> Seq.toArray
            // list of open/close price tuples
            let ocPrices = 
                [ for i in prices -> (i.Item2, i.Item5) ]
                |> Seq.skip skip
                |> Seq.toArray

            // skiped list of prices
            let prices = prices.GetRange(skip, prices.Count-skip)
            printfn "Skipped %d" skip

            // bollinger bands
            let bollinger = bollinger(n, sigma, prices)
            // bollinger band breadth
            let breadth = [| for i in bollinger -> (i |> fst) - (i |> snd) |]
            // position of price between bollinger bands
            let bInd = (cPrices, bollinger) |> bInd
            //let bInd = cPrices bollinger
            let mutable lastCross = 0
            printfn "Finished Bollinger Bands"

            // amas for triple crossing in trend phases
            let short = ama(erp, s1, s2, cPrices)
            printfn "Finished short AMA"
            let middle = ama(erp, m1, m2, cPrices)
            printfn "Finished middle AMA"
            let long = ama(erp, l1, l2, cPrices)
            printfn "Finished long AMA"

            // rsi
            let rsiN = 14
            let rsi = rsi(rsiN, ocPrices)

            // TODO: short regression
            let regrS = [|for i in 0..cPrices.Length-1 -> 0|]
            
            // adx
            let adx = adx (14, prices)
            printfn "Finished ADX"

            // first index with all data
            let firstI = [erp-1; l2-1; n-1] |> List.max
            
            let mutable sw = 0
            let mutable trend = 0
            let mutable rsiSig = 0
            let mutable amaSig = 0
            for i in signals.Count .. prices.Count-1 do
                // Bollinger
                // check if price has crossed Bollinger Bands
                if bollinger.[i] |> fst <> 0m then
                    if cPrices.[i] > (bollinger.[i] |> fst) then
                        lastCross <- -1
                    else if cPrices.[i] < (bollinger.[i] |> snd) then
                        lastCross <- 1

                // AMA
                amaSig <-
                    if short.[i] < middle.[i] && middle.[i] < long.[i] then
                        -1
                    else if short.[i] > middle.[i] && middle.[i] > long.[i] then
                        1
                    else
                        0

                // RSI
                rsiSig <-
                    if (rsi.[i] < 30m) then 
                        1 
                    else if(rsi.[i] > 70m) then
                        -1
                    else 0

                // Not all neccessary data available yet
                if i < firstI then
                    signals.Add(0)
                else
                    // if er indicates sideways markets
                    if adx.[i] < 20m then
                        sw <- sw+1
                        // price between bbs
                        if ((bollinger.[i] |> snd) < cPrices.[i] && cPrices.[i] < (bollinger.[i] |> fst)) then
                            if lastCross = 1 && bInd.[i]<0m then
                                signals.Add(1)
                            else if lastCross = -1 && bInd.[i]>0m then
                                signals.Add(-1)
                            else
                                signals.Add(0)
                        else
                            signals.Add(0)
                    // trending market
                    else
                        trend <- trend+1
                        // contradictory ama and rsi signals
                        if (amaSig + rsiSig = 0) then
                            signals.Add(0)
                        else
                            // + 3
                            // ama and rsi on buy and price upwards
                            if (amaSig = 1 && rsiSig = 1 && regrS.[i] > 0) then
                                signals.Add(3)
                            // -3
                            // same for sell
                            else if (amaSig = -1 && rsiSig = -1 && regrS.[i] < 0) then
                                signals.Add(-3)
                            // +/- 2
                            else if (amaSig = rsiSig) then
                                signals.Add(amaSig*2)
                            // +/- 1
                            else
                                signals.Add(amaSig)
                        // signals can only get bigger, neutral or in the other direction:
                        // same sign
                        if (sign signals.[signals.Count-1] = sign signals.[signals.Count-2]) then
                            // weaker signal than before
                            if (abs signals.[signals.Count-1] < abs signals.[signals.Count-2]) then
                                // save stronger signal from before as present signal
                                signals.[signals.Count-1] <- signals.[signals.Count-2]
                        printfn "Signal: %d\t AMA:%d\t RSI:%d\t ADX:%f" signals.[signals.Count-1] amaSig rsiSig adx.[i]
            printfn "Trending decisions: %d" trend
            printfn "Sideways decisions: %d" sw
            signals

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, signals:System.Collections.Generic.List<int>)= 
            //       erp  s1  s2  m1  m2  l1  l2   bN  sig 
            strategy (90, 10, 20, 40, 50, 90, 120, 20, 2m, prices, signals)