namespace Algorithm
    module DecisionCalculator=(*007*)

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   GENERIC FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        (*
         * rounds the given number to the nearest even number
         *)
        let even (x:decimal)=
            let s = sign x
            let x = abs x
            let a = decimal(x) % 2m
            if a < 1m then
                s*(int (decimal(x) - a))
            else
                s*(int (decimal(x) + (2m-a)))

        let alphaToN (a) : int=
            int (round ((2.0m/a)-1.0m))

        let alphaToNDec (a) : decimal=
            (2.0m/a)-1.0m
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

        let nToAlphaDec (n:decimal) : decimal=
            (2.0m / (n + 1.0m))

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   TREND FOLLOWER (e.g. averages)
        //////////////////////////////////////////////////////////////////////////////////////////////////////

//        let mutable dimension = new System.Collections.Generic.List<decimal>()
//        let mutable alphas = new System.Collections.Generic.List<decimal>()
//        let mutable ns = new System.Collections.Generic.List<decimal>()

        // α = EXP(W*(D – 1))
        // D = (Log(HL1 + HL2) – Log(HL)) / Log(2)
        // Note: Log(2) = Log(N / (½N))
        // HL1 = (Max(High,½N..N) – Min(Low,½N..N)) / ½N
        // HL2 = (Max(High,½N) – Min(Low,½N)) / ½N
        // HL = (Max(High,N) – Min(Low,N)) / N
        // N = FRAMA Period, must be an even number.
        let framaA (w:decimal, prices:System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>[])=
            let n = prices.Length

            let max1 =
                [ for i in 0..(n/2)-1 -> prices.[i].Item3 ]
                |> List.max
            let min1 =
                [ for i in 0..(n/2)-1 -> prices.[i].Item4 ]
                |> List.min
            let hl1 = double(max1 - min1) / double(n/2)

            let max2 =
                [ for i in n/2..n-1 -> prices.[i].Item3 ]
                |> List.max
            let min2 =
                [ for i in n/2..n-1 -> prices.[i].Item4 ]
                |> List.min
            let hl2 = double(max2 - min2) / double(n/2)

            let max =
                [ for i in prices -> i.Item3 ]
                |> List.max
            let min =
                [ for i in prices -> i.Item4 ]
                |> List.min
            let hl = double(max - min) / double(n)

            let mutable d = (log10 (hl1 + hl2) - log10 (hl)) / log10(2.0)
            if (d < 1.0) then
                d <- 1.0
            else if (d > 2.0) then
                d <- 2.0
            // TODO:remove
//            dimension.Add(decimal(d))
            let alpha = decimal (exp ((double w)*(d - 1.0)))
            // TODO:remove
//            alphas.Add(alpha)
            alpha

        // W = LN(2 / (SC + 1))
        // New N = ((SC – FC) * ((Original N – 1) / (SC – 1))) + FC
        let frama (n:int, fc:int, sc:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let prices = [| for i in prices -> i |]
            
            // calculate FRAMA w (constant over price range!)
            let w = decimal (log (2.0 / double(sc + 1)))
            // move the WN according to the given minimum N (FC)
            
            // create empty FRAMA array
            let frama : decimal array = Array.zeroCreate (prices.Length)

            // H is the number of prices being averaged for the first value
            // H = EVEN( ((SC – FC) / 2) ) + FC
            let mutable h = even (decimal(sc - fc) / 2m) + fc
            // check H for out of range values (modified FRAMA rule)
            if (n - 1 < even (decimal(sc - fc) / 2m) + fc) then
                h <- n-1
            let h = h

            // calculate the first FRAMA value:
            // FRAMA(N-1) = avg of the last H closing prices
            frama.[n-2] <- [| for i in prices.[(n-1)-h..n-2] -> i.Item5 |] |> Array.average
            // calculate all FRAMA values
            prices
            |> Array.iteri (fun i bar -> 
                match i with
                | _ when i > n-2 ->
                    // calculate original FRAMA alpha
                    let mutable a = framaA (w, prices.[i-n+1..i])
                    // convert to original N
                    let origN = alphaToNDec a
                    // shift to modified N
                    let n = decimal(sc - fc) * ((origN - 1m) / decimal(sc - 1)) + decimal fc
                    // TODO:remove
//                    ns.Add(n)
                    // revert new N to new alpha
                    a <- nToAlphaDec n

                    // check w for out of range alpha values (modified FRAMA rule)
                    if (a > 1m) then
                        a <- 1m
                    if (a < 2m / decimal(sc + 1)) then
                        a <- 2m / decimal(sc + 1)

                    frama.[i] <- a * bar.Item5 + (1m - a) * frama.[i-1]
                | _              -> ignore i)
            // set initial frama value (sma) to 0
            frama.[n-2] <- 0m
            frama

        (*
         * Calculates the slope of a regression over the given data.
         * The data points are therefore assumed to be of successive nature, i.e. x = 1,2,3,..
         *)
        let regressionSlope(prices:decimal array)=
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

        (*
         * Calculates a "moving" regression over the given data,
         * resulting in a slope of the regression over the last n values for each data point
         *)
        let regression(n:int, prices:decimal[])=
            [|for i in n-1..prices.Length-1 -> regressionSlope(prices.[i-n+1..i])|]
            |> Array.append (Array.zeroCreate (n-1))

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   OSCILLATORS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        (*
         * Williams%R:
         * %R = (Highest High - Close)/(Highest High - Lowest Low) * -100
         *
         *  Lowest Low = lowest low for the look-back period
         *  Highest High = highest high for the look-back period
         *  %R is multiplied by -100 correct the inversion and move the decimal.
         *)
        let williamsRVal(bars:System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>[])=
            let mutable high = 0m
            let mutable low = 0m
            for bar in bars do
                if (bar.Item3 > high) then 
                    high <- bar.Item3
                if (bar.Item4 < low || low = 0m) && bar.Item4 <> 0m then
                    low <- bar.Item4
            ((high - bars.[bars.Length-1].Item5)/(high - low)) * -100m

        let williamsR(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let windows = Seq.windowed n prices
            Seq.map williamsRVal windows
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   SPECIAL FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let sma(n:int, prices:decimal[])=
            prices
            |> Seq.windowed n
            |> Seq.map Seq.average
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        (*
         * Tries to identify minima and maxima in the given array
         * @n: number of bars an extremum has to be extremer on both sides
         *)
        let findExtremes(n:int, data:decimal[])=
            let extremes = Array.zeroCreate data.Length
            let mutable isMin = false
            let mutable isMax = false
            for i in n..data.Length-1-n do
                isMin <- true
                isMax <- true
                for j in -n..n do
                    if (data.[i+j] < data.[i]) then
                        isMin <- false
                    if (data.[i+j] > data.[i]) then
                        isMax <- false
                if (isMin) then extremes.[i] <- -100m
                if (isMax) then extremes.[i] <- 100m
            extremes

        let getExtremeValues(n:int, data:decimal[], extremes:decimal[])=
            let mutable mins = new ResizeArray<decimal>()
            let mutable maxs = new ResizeArray<decimal>()
            for i in extremes.Length-1..extremes.Length-1-n do
                if (extremes.[i] = 100m) then
                    maxs.Add(data.[i])
                else if (extremes.[i] = -100m) then
                    mins.Add(data.[i])
            mins, maxs

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   SIGNAL GENERATOR
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // 20-100
        // 50-200

        let sw1 = new System.Diagnostics.Stopwatch()
        let sw2 = new System.Diagnostics.Stopwatch()
        let sw3 = new System.Diagnostics.Stopwatch()
        let sw4 = new System.Diagnostics.Stopwatch()
        let sw5 = new System.Diagnostics.Stopwatch()

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              parameters:System.Collections.Generic.Dictionary<string, decimal>)=

            sw1.Start()

            (*
             * Read Parameters
             *)
            // FRAMA
            let s1 = even parameters.["s1"]
            let s2 = even parameters.["s2"]
            let m1 = even parameters.["m1"]
            let m2 = even parameters.["m2"]
            let l1 = even parameters.["l1"]
            let l2 = even parameters.["l2"]
            // Regressions
            let regrXSN = int parameters.["regrXSN"]
            let regrSN = int parameters.["regrSN"]
            // Williams%R
            let wn = int parameters.["wn"]
            // Price Extremes
            let extrN = int parameters.["extrN"]
            let extrP = parameters.["extrP"]
            // Cutloss
            let cutloss = abs parameters.["cutloss"]

//            let s1 = even 10m
//            let s2 = even 20m
//            let m1 = even 10m
//            let m2 = even 200m
//            let l1 = even 200m
//            let l2 = even 300m
//            let wn = 20
//            let extrN = 500
//            let extrP = 10m
//            let cutloss = 1m

            // Chart Lines
            chart1.Add("FRAMAs;#FF0000", new System.Collections.Generic.List<decimal>())
            chart1.Add("FRAMAm;#0000FF", new System.Collections.Generic.List<decimal>())
            chart1.Add("FRAMAl;#999999", new System.Collections.Generic.List<decimal>())
            chart2.Add("W%R;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("W%R_os;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("W%R_ob;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("LocalExtremes;#00FFFF", new System.Collections.Generic.List<decimal>())
            
            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]

            sw2.Start()
            // calculate FRAMAs
            let framaS = frama(even (decimal((s2-s1))/2m), s1, s2, prices)
            for i in 0..framaS.Length-1 do chart1.["FRAMAs;#FF0000"].Add(framaS.[i])
            let framaM = frama(even (decimal((m2-m1))/2m), m1, m2,  prices)
            for i in 0..framaM.Length-1 do chart1.["FRAMAm;#0000FF"].Add(framaM.[i])
            let framaL = frama(even (decimal((l2-l1))/2m), l2, l2, prices)
            for i in 0..framaL.Length-1 do chart1.["FRAMAl;#999999"].Add(framaL.[i])
            sw2.Stop()

            // calculate regressions
            let regrXS = regression(regrXSN, cPrices)
            let regrS = regression(regrSN, cPrices)
            
            sw3.Start()
            // calculate Williams%R
            let w = williamsR(wn, prices)
            sw3.Stop()
            for i in 0..w.Length-1 do chart2.["W%R;#FF0000"].Add(w.[i])
            let mutable wLastCross = 0
            let mutable wSinceCross = 0
            // Williams%R threshholds
            // oversold
            let wOS = -80m
            // overbought
            let wOB = -20m
            let wChannel = 20m
            for i in 0..w.Length-1 do chart2.["W%R_os;#0000FF"].Add(wOS)
            for i in 0..w.Length-1 do chart2.["W%R_ob;#0000FF"].Add(wOB)

            sw4.Start()
            // try to find 20 bar price extrema
            let localExtrema = findExtremes (20, cPrices)
            sw4.Stop()
            // add to chart2
            for i in 0..localExtrema.Length-1 do chart2.["LocalExtremes;#00FFFF"].Add(localExtrema.[i])

            // price at trade entry (long or short)
            let mutable entryPrice = 0m
            // price extreme in trade for cut loss
            let mutable priceExtreme = cPrices.[0]

            // first index with all data
            let firstI = ([ m1; m2; s1; s2; l1; l2; wn ] |> List.max) - 1
            let mutable missingData = firstI+1

            signals.Clear();
            for i in signals.Count .. prices.Count-1 do
                // one bar more available
                missingData <- missingData - 1

                // Not all neccessary data available yet
                // (.. or new day)
                if i < firstI || missingData > 0 then
                    signals.Add(0)
                else
                    (*
                     * // standard behaviour is to keep the last signal
                     *)
                    signals.Add(if (i=0) then 0 else signals.[i-1])

                    /////////////////////////////////////
                    //////   ENTRY SIGNAL
                    /////////////////////////////////////
                    let mutable entry = 0

                    (*
                     * // FRAMA entry
                     *)
                    let mutable framaSig = 0
//                    // short over middle and long (and has just crossed one of them)
//                    if (framaS.[i] > framaM.[i]         && framaM.[i] > framaL.[i]) &&
//                       not (framaS.[i-1] > framaM.[i-1] && framaM.[i-1] > framaL.[i-1]) then
//                        framaSig <- 1
//                    // short under middle and long (and has just crossed one of them)
//                    else if (framaS.[i] < framaM.[i]         && framaM.[i] < framaL.[i]) &&
//                        not (framaS.[i-1] < framaM.[i-1] && framaM.[i-1] < framaL.[i-1]) then
//                        framaSig <- -1
                    if (framaM.[i] > framaL.[i] && framaM.[i-1] < framaL.[i-1]) then
                        framaSig <- 1
                    else if (framaM.[i] < framaL.[i] && framaM.[i-1] > framaL.[i-1]) then
                        framaSig <- -1

                    (*
                     * // Williams%R entry
                     *)
                    let mutable wSig = 0

                    // Williams%R either overbought or oversold
                    if (w.[i] < wOS || w.[i] > wOB) then
                        wLastCross <- 0
                        wSinceCross <- 0

                    // Williams%R went over oversold
                    if (w.[i] > wOS && w.[i-1] < wOS) then
                        wLastCross <- 1
                        wSinceCross <- 0
                    // Williams%R went below overbought
                    else if (w.[i] < wOB && w.[i-1] > wOB) then
                        wLastCross <- -1
                        wSinceCross <- 0
                    
                    // Williams%R recently crossed from overbought/oversold and is still in the signal channel
                    if (wLastCross = 1 && (w.[i] < wOS + wChannel || wSinceCross <= 3)) || (wLastCross = -1 && (w.[i] > wOB - wChannel || wSinceCross <= 3)) then
                        wSig <- wLastCross
                        
                    wSinceCross <- wSinceCross + 1

                    (*
                     * // entry decision
                     *)
                    if (framaSig <> 0) then
                        if (sign wSig = sign framaSig) then
                            entry <- framaSig * 2
                        else
                            entry <- framaSig

                    (*
                     * // Check regressions
                     *)
                    // don't decide against short term trend! (7)
                    // or very short term trend (3)
                    // new signal contradicts short term price trend
                    if (sign regrS.[i] <> sign entry || sign regrXS.[i] <> sign entry) then
                        entry <- 0

                    (*
                     * // don't enter in extreme price situations
                     *)
                    if (i >= extrN-1) then
                        sw5.Start()
                        
                        // maximum minus minimum price in range
                        let priceBreadth = ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item3] |> List.max) - ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item4] |> List.min) 
                        let mins, maxs = getExtremeValues(extrN, cPrices, localExtrema)
                        if (entry > 0) then
                            for max in maxs do
                                // don't go long in maxs
                                if (cPrices.[i] > max-(max*extrP*priceBreadth/2m) && cPrices.[i] < max+(max*extrP*priceBreadth/2m)) then
                                    entry <- 0
                        if (entry < 0) then
                            for min in mins do
                                // don't short in mins
                                if (cPrices.[i] > min-(min*extrP*priceBreadth/2m) && cPrices.[i] < min+(min*extrP*priceBreadth/2m)) then
                                    entry <- 0

                        sw5.Stop()

                    // open position / add to position
                    if (entry <> 0) then
                        signals.[i] <- entry

                    /////////////////////////////////////
                    //////   EXIT SIGNAL
                    /////////////////////////////////////
                    let mutable exit = 4

                    // FRAMA exit
//                    // long
//                    if (signals.[i] > 0) then
//                        // exit if S is between M and L and Williams is overbought
//                        if (framaS.[i] < framaM.[i] && framaS.[i] > framaL.[i] && w.[i] > wOB) then
//                            exit <- 0
//                        // FRAMAs point to falling prices
//                        if (framaS.[i] < framaM.[i] && framaS.[i] < framaL.[i] && framaM.[i] < framaL.[i]) then
//                            exit <- 0
//                    // short
//                    else if (signals.[i] < 0) then
//                        // exit if S is between M and L and Williams is oversold
//                        if (framaS.[i] > framaM.[i] && framaS.[i] < framaL.[i] && w.[i] < wOS) then
//                            exit <- 0
//                        // FRAMAs point to rising prices
//                        if (framaS.[i] > framaM.[i] && framaS.[i] > framaL.[i] && framaM.[i] > framaL.[i]) then
//                            exit <- 0
                    // long
                    if (signals.[i] > 0) then
                        if (framaS.[i] < framaM.[i] && framaS.[i-1] > framaM.[i-1]) then
                            exit <- 0
                    // short
                    else if (signals.[i] < 0) then
                        if (framaS.[i] > framaM.[i] && framaS.[i-1] < framaM.[i-1]) then
                            exit <- 0

                    (*
                     * // Cutloss: neutralise if loss is too big (% of price movement!)
                     *)

                    // same sign: signal now and last bar
                    if (sign signals.[i] = sign signals.[i-1]) then
                        // cut loss: price extreme
                        if (decimal(sign signals.[i]) * cPrices.[i] > decimal(sign signals.[i-1]) * cPrices.[i-1]) then
                            priceExtreme <- cPrices.[i]

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
                            exit <- 0

                    (*
                     * // close position / liquidate part
                     *)
                    if (exit <> 4) then
                        signals.[i] <- exit

            sw1.Stop()
            printfn "Total: %f" (sw1.Elapsed.TotalMilliseconds / 1000.0)
            printfn "FRAMAs: %f" (sw2.Elapsed.TotalMilliseconds / 1000.0)
            printfn "WilliamsR: %f" (sw3.Elapsed.TotalMilliseconds / 1000.0)
            printfn "Extrema: %f" (sw4.Elapsed.TotalMilliseconds / 1000.0)
            printfn "Extrema check: %f" (sw4.Elapsed.TotalMilliseconds / 1000.0)
            signals