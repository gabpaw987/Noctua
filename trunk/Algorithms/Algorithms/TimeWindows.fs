﻿// Parameters: 26.09.2013
(*
aggN,60,120,60
s1,5,5,1
s2,100,100,1
m1,90,90,1
m2,140,140,1
l1,200,200,1
l2,220,220,1
framaNFactor,1,1,1
regrXSN,0,0,1
regrSN,0,0,1
regrLN,0,0,1
rsiN,30,30,1
rsiEmaN,20,20,1
rsiLong,60,60,1
rsiShort,40,40,1
barExtrN,150,150,1
extrN,1000,1000,500
extrPIn,27,27,2
extrPOut,34,34,2
cutlossMax,0,0,10
cutlossMin,0,0,10
cutlossDecrN,0,0,250
*)

namespace Algorithm
    module DecisionCalculator=(*010*)

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

        let aggregateBars(n:int, bars:ResizeArray<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let aggregated = new ResizeArray<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>()
            for i in 0..n..bars.Count-n do
                let range = bars.GetRange(i, n)
                aggregated.Add(
                    new System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>(
                        range.[0].Item1,
                        range.[0].Item2,
                        [for j in range -> j.Item3] |> List.max,
                        [for j in range -> j.Item4] |> List.min,
                        range.[n-1].Item5
                    )
                )
            aggregated
                    

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   TREND FOLLOWER (e.g. averages)
        //////////////////////////////////////////////////////////////////////////////////////////////////////

//        let mutable dimension = new System.Collections.Generic.List<decimal>()
//        let mutable alphas = new System.Collections.Generic.List<decimal>()
//        let mutable ns = new System.Collections.Generic.List<decimal>()

        let sma(n:int, prices:decimal[])=
            prices
            |> Seq.windowed n
            |> Seq.map Seq.average
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        let ema (n:int, prices:List<decimal>)=
            let alpha = (2.0m / (decimal n + 1.0m))
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

        let rsi (n:int, prices:decimal[]) = 
            let intervals = 
                prices
                |> Array.toSeq
                |> Seq.windowed n
                |> Seq.toArray
            let sumup = [for i in 0 .. intervals.Length - 1 do yield Array.sum [|for j in 1 .. intervals.[i].Length - 1 do yield Array.max [|intervals.[i].[j] - intervals.[i].[j - 1]; 0m|]|] ]
            let sumdown = [for i in 0 .. intervals.Length - 1 do yield - Array.sum [|for j in 1 .. intervals.[i].Length - 1 do yield Array.min [|intervals.[i].[j] - intervals.[i].[j - 1]; 0m|]|] ]
            [| for i in 0 .. sumup.Length - 1 do yield 100m * (sumup.[i]/(decimal n))/((sumup.[i]/(decimal n)) + (sumdown.[i]/(decimal n)))|]
            |> Array.append (Array.create (n - 1) 0m)

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   SPECIAL FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

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
                if (isMin) then extremes.[i] <- decimal (-1*n)
                if (isMax) then extremes.[i] <- decimal n
            extremes

        let getExtremeValues(n:int, data:decimal[], extremes:decimal[])=
            let mutable mins = new ResizeArray<decimal>()
            let mutable maxs = new ResizeArray<decimal>()
            for i in extremes.Length-1 .. -1 .. extremes.Length-n do
                if (extremes.[i] > 0m) then
                    maxs.Add(data.[i])
                else if (extremes.[i] < 0m) then
                    mins.Add(data.[i])
            mins, maxs

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   SIGNAL GENERATOR
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let blockSignals (rsiN:int, rsiEmaN:int,
                            long:int, short:int, blockLong:int, blockShort:int,
                            prices:decimal[],
                            signals:System.Collections.Generic.List<int>)=
            let rsi = rsi (rsiN, prices)
            // smooth RSI
            let rsiEma = ema (rsiEmaN, Array.toList rsi)

            signals.Add(0)
            for i in 1 .. prices.Length-1 do
                if (rsiEma.[i] > decimal long && rsiEma.[i-1] < decimal long) then
                    signals.Add(1)
                else if (rsiEma.[i] < decimal short && rsiEma.[i-1] > decimal short) then
                    signals.Add(-1) 
                else if (signals.[i-1] = 1 && rsiEma.[i] < decimal blockLong && rsiEma.[i-1] > decimal blockLong) then
                    signals.Add(0)
                else if (signals.[i-1] = -1 && rsiEma.[i] > decimal blockShort && rsiEma.[i-1] < decimal blockShort) then
                    signals.Add(0)
                else
                    signals.Add(signals.[i-1])
            signals
                

        let strategy (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,
                        signals:System.Collections.Generic.List<int>,
                        chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                        chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                        parameters:System.Collections.Generic.Dictionary<string, decimal>,
                        agg:bool)=

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
            let framaNFactor = abs parameters.["framaNFactor"]
            // Regressions
            let regrXSN = int parameters.["regrXSN"]
            let regrSN = int parameters.["regrSN"]
            let regrLN = int parameters.["regrLN"]
            // RSI
            let rsiN = int parameters.["rsiN"]
            let rsiEmaN = int parameters.["rsiEmaN"]
            let rsiLong = parameters.["rsiLong"]
            let rsiShort = parameters.["rsiShort"]
            // Price Extremes
            let barExtrN = int parameters.["barExtrN"]
            let extrN = int parameters.["extrN"]
            let extrPIn = parameters.["extrPIn"]
            let extrPOut = parameters.["extrPOut"]
            // Cutloss
            let cutlossMax = abs parameters.["cutlossMax"]
            let mutable cutloss = cutlossMax
            let cutlossMin = abs parameters.["cutlossMin"]
            let cutlossDecrN = abs (int parameters.["cutlossDecrN"])

//            let s1 = even 5m //15m
//            let s2 = even 100m
//            let m1 = even 90m
//            let m2 = even 140m
//            let l1 = even 200m
//            let l2 = even 220m
//            let framaNFactor = 1m
//
//            let regrXSN = 0
//            let regrSN = 0
//            let regrLN = 0
//
//            let rsiN = 30
//            let rsiEmaN = 20
//            let rsiLong = 60m
//            let rsiShort = 40m
//
//            let barExtrN = 150
//            let extrN = 1000
//            let extrPIn = 27m
//            let extrPOut = 34m
//
//            let cutlossMax = 0m
//            let mutable cutloss = cutlossMax
//            let cutlossMin = 0m
//            let cutlossDecrN = 0

            // Chart Lines
            if (not agg) then
                chart1.Add("FRAMAs;#FF0000", new System.Collections.Generic.List<decimal>())
                chart1.Add("FRAMAm;#0000FF", new System.Collections.Generic.List<decimal>())
                chart1.Add("FRAMAl;#999999", new System.Collections.Generic.List<decimal>())
                chart2.Add("W%R;#FF0000", new System.Collections.Generic.List<decimal>())
                chart2.Add("W%R_os;#0000FF", new System.Collections.Generic.List<decimal>())
                chart2.Add("W%R_ob;#0000FF", new System.Collections.Generic.List<decimal>())
                chart2.Add("LocalExtremes;#00FFFF", new System.Collections.Generic.List<decimal>())
                chart2.Add("regrLSlope;#00FF00", new System.Collections.Generic.List<decimal>())
                chart2.Add("RSI;#FF0000", new System.Collections.Generic.List<decimal>())
                chart2.Add("RSI_long;#0000FF", new System.Collections.Generic.List<decimal>())
                chart2.Add("RSI_short;#0000FF", new System.Collections.Generic.List<decimal>())
                chart2.Add("framaSig;#00FF00", new System.Collections.Generic.List<decimal>())

            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]
            // list of typical prices
            let tPrices =
                [| for i in prices -> (i.Item3 + i.Item4 + i.Item5)/3m |]

            let useFramas = if (s1 <> 0 && s2 <> 0 && m1 <> 0 && m2 <> 0 && l1 <> 0 && l2 <> 0) then true else false
            // calculate FRAMAs
            let framaS = if (useFramas) then frama(even ((decimal(s2+s1)*framaNFactor)/2m), s1, s2, prices) else Array.empty
            let framaM = if (useFramas) then frama(even ((decimal(m2+m1)*framaNFactor)/2m), m1, m2,  prices) else Array.empty
            let framaL = if (useFramas) then frama(even ((decimal(l2+l1)*framaNFactor)/2m), l1, l2, prices) else Array.empty
            if (not agg) then
                for i in 0..framaS.Length-1 do chart1.["FRAMAs;#FF0000"].Add(framaS.[i])
                for i in 0..framaM.Length-1 do chart1.["FRAMAm;#0000FF"].Add(framaM.[i])
                for i in 0..framaL.Length-1 do chart1.["FRAMAl;#999999"].Add(framaL.[i])

            // how long ago frama has given a signal
            let mutable framaSinceSig = 0
            // indicates that the long averages have given a signal (waiting for short)
            let mutable framaPreSig = 0
            let mutable framaSig = 0

            // calculate regressions
            let regrXS = if (regrXSN <> 0) then regression(regrXSN, cPrices) else Array.empty
            let regrS = if (regrSN <> 0) then regression(regrSN, cPrices)  else Array.empty
            let regrL = if (regrLN <> 0) then regression(regrLN, cPrices)  else Array.empty

            let useRsi = if (rsiN <> 0 && rsiEmaN <> 0) then true else false
            // calculate RSI
            let rsi = if (useRsi) then rsi (rsiN, tPrices) else Array.empty
            // smooth RSI
            let rsiEma = if (useRsi) then ema (rsiEmaN, Array.toList rsi) else Array.empty
            if (not agg) then
                for i in 0..rsiEma.Length-1 do chart2.["RSI;#FF0000"].Add(rsiEma.[i])
                for i in 0..rsiEma.Length-1 do chart2.["RSI_long;#0000FF"].Add(rsiLong)
                for i in 0..rsiEma.Length-1 do chart2.["RSI_short;#0000FF"].Add(rsiShort)
            
            // try to find n bar price extrema
            let localExtrema = findExtremes (barExtrN, cPrices)
            // add to chart2
            if (not agg) then
                for i in 0..localExtrema.Length-1 do chart2.["LocalExtremes;#00FFFF"].Add(localExtrema.[i])

            // price at trade entry (long or short)
            let mutable entryPrice = 0m
            // price extreme in trade for cut loss
            let mutable priceExtreme = cPrices.[0]

            // first index with all data
            let firstI = ([ m1; m2; s1; s2; l1; l2 ] |> List.max) - 1
            let mutable missingData = firstI+1

            signals.Clear();
            for i in 0 .. prices.Count-1 do
                // one bar more available
                missingData <- missingData - 1

                // Not all neccessary data available yet
                // (.. or new day)
                if i < firstI || missingData > 0 then
                    signals.Add(0)
                    if (not agg) then
                        chart2.["framaSig;#00FF00"].Add(0m)
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

                    if (useFramas) then
                        if (framaM.[i] > framaL.[i] && framaM.[i-1] < framaL.[i-1]) then
                            framaPreSig <- 1
                        else if (framaM.[i] < framaL.[i] && framaM.[i-1] > framaL.[i-1]) then
                            framaPreSig <- -1

                        if (framaPreSig <> 0) then
                            if (framaPreSig = sign (framaS.[i] - framaM.[i])) then
                                framaSig <- framaPreSig
                                framaPreSig <- 0

                    if (not agg) then
                        chart2.["framaSig;#00FF00"].Add(decimal framaSig*100m)

                    (*
                     * // RSI entry
                     *)
                    let mutable rsiSig = 0

                    if (useRsi) then
                        if (rsiEma.[i] > rsiLong && rsiEma.[i-1] < rsiLong) then
                            rsiSig <- 1
                        else if (rsiEma.[i] < rsiShort && rsiEma.[i-1] > rsiShort) then
                            rsiSig <- -1

                    (*
                     * // entry decision
                     *)
                    if (useRsi) then
                        if (sign rsiSig <> sign signals.[i]) then
                            if (sign rsiSig = sign framaSig) then
                                entry <- rsiSig * 2
                            else
                                entry <- rsiSig
                        // entry level 2 decision
                        if (abs signals.[i] = 1 && sign framaSig = signals.[i]) then
                            entry <- 2*framaSig
                    else if (useFramas) then
                        if (framaSig <> signals.[i-1]) then
                            entry <- framaSig
                    
                    (*
                     * // Check regressions
                     *)
                    // don't decide against short term trend! (7)
                    // or very short term trend (3)
                    // new signal contradicts short term price trend
                    if (regrSN <> 0 && sign regrS.[i] <> sign entry) then
                        entry <- 0
                    if (regrXSN <> 0 && sign regrXS.[i] <> sign entry) then
                        entry <- 0

                    (*
                     * // don't enter in extreme price situations
                     *)
                    if (i >= extrN-1 && (extrN <> 0 && (extrPIn > 0m || extrPOut > 0m))) then
                        // maximum minus minimum price in range
                        let priceBreadth = ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item3] |> List.max) - ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item4] |> List.min)
                        let mins, maxs = getExtremeValues(extrN, cPrices, localExtrema)
                        if (entry > 0) then
                            for max in maxs do
                                // don't go long in maxs
                                if (cPrices.[i] > max-(extrPIn*0.01m*priceBreadth/2m) && cPrices.[i] < max+(extrPOut*0.01m*priceBreadth/2m)) then
                                    entry <- 0
                        if (entry < 0) then
                            for min in mins do
                                // don't short in mins
                                if (cPrices.[i] > min-(extrPOut*0.01m*priceBreadth/2m) && cPrices.[i] < min+(extrPIn*0.01m*priceBreadth/2m)) then
                                    entry <- 0

                    // changed signal
//                    if (entry <> 0 && sign entry <> sign signals.[i-1]) then
//                        framaSig <- 0

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
                    
                    if (useFramas) then
                        // long
                        if (signals.[i] > 0) then
                            if (framaS.[i] < framaM.[i] && framaS.[i-1] > framaM.[i-1]) then
                                if (signals.[i] = 2) then
                                    exit <- 1
                                    framaSig <- 0
                        // short
                        else if (signals.[i] < 0) then
                            if (framaS.[i] > framaM.[i] && framaS.[i-1] < framaM.[i-1]) then
                                if (signals.[i] = -2) then
                                    exit <- -1
                                    framaSig <- 0

                        // exit if RSI and FRAMA are contradictory
    //                    if (abs framaSig = 1 && rsiSig + framaSig = 0) then
    //                        exit <- 0

                    (*
                     * // Cutloss: neutralise if loss is too big (% of price movement!)
                     *)

                    if (cutlossMax <> 0m) then
                        // same sign: signal now and last bar
                        if (sign signals.[i] = sign signals.[i-1]) then
                            // decrease cutloss over time until it reaches the given minimum
                            // e.g. <- 2 - (2-1)/100
                            cutloss <- cutloss - (cutloss-cutlossMin)/(decimal cutlossDecrN)
                            if (cutloss < cutlossMin) then
                                cutloss <- cutlossMin
                            // cut loss: price extreme
                            if (decimal(sign signals.[i]) * cPrices.[i] > decimal(sign signals.[i-1]) * cPrices.[i-1]) then
                                priceExtreme <- cPrices.[i]

                        // new buy or sell signal (different direction)
                        if (signals.[i] <> 0 && sign signals.[i] <> sign signals.[i-1]) then
                            // reset cutloss to maximum for new trade
                            cutloss <- cutlossMax
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

                    if (prices.[i].Item1.Month < 6 || (prices.[i].Item1.Month = 6 && prices.[i].Item1.Day < 16)) then
                        signals.[i] <- 0
            signals

        let combineSignals(signals:ResizeArray<int>, aggSignals:int[])=
            for i in 0..aggSignals.Length-1 do
                if (aggSignals.[i] = 2) then
                    if (signals.[i] >= 0) then
                        signals.[i] <- signals.[i] + 1
                    else
                        signals.[i] <- 0
                else if (aggSignals.[i] = 1 && signals.[i] < 0) then
                    signals.[i] <- if (signals.[i] = -1) then 0 else -1
                else if (aggSignals.[i] = -2) then
                    if (signals.[i] <= 0) then
                        signals.[i] <- signals.[i] - 1
                    else
                        signals.[i] <- 0
                else if (aggSignals.[i] = -1 && signals.[i] > 0) then
                    signals.[i] <- if (signals.[i] = 1) then 0 else 1
            signals

        let combineBlockSignals(signals:ResizeArray<int>, aggSignals:ResizeArray<int>)=
            for i in 0..aggSignals.Count-1 do
                if (aggSignals.[i] <> 0 && sign aggSignals.[i] <> sign signals.[i]) then
                    signals.[i] <- sign aggSignals.[i]

        let startCalculation (prices:ResizeArray<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,
                                signals:System.Collections.Generic.List<int>,
                                chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                                chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                                parameters:System.Collections.Generic.Dictionary<string, decimal>)=
                
            let signals = strategy (prices, signals, chart1, chart2, parameters, false)

            // aggregated prices
            let n = int parameters.["aggN"]
            let aggPrices = aggregateBars(n, prices)
            let cAggPrices = [| for i in aggPrices -> i.Item5 |]

            let aggSignals = new ResizeArray<int>()
            let aggSignalsDil = new ResizeArray<int>()
            let aggSignals = blockSignals(30, 20, 60, 40, 45, 55, cAggPrices, aggSignals)
            //signals.Clear()

//            let aggSignals = strategy (aggPrices, new ResizeArray<int>(), chart1, chart2, parameters, true)
            //let aggSignals1 = [|for i in aggSignals do for j in 0..n-1 -> i|]
            for i in aggSignals do for j in 0..n-1 do aggSignalsDil.Add(i)
            for i in 0 .. prices.Count - signals.Count - 1 do aggSignalsDil.Add(0)
            //combineSignals(signals, aggSignals)
            combineBlockSignals(signals, aggSignalsDil)
            //let ret = new ResizeArray<int>(aggSignals1 |> Array.append (Array.zeroCreate (prices.Count - aggSignals1.Length)))
            //ret
            signals