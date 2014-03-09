(*
rsiN,30,30,1
rsiEmaN,20,20,1
rsiLong,60,60,1
rsiShort,40,40,1
barExtrN,100,100,50
extrN,1000,1000,1
extrPIn,15,15,1
extrPOut,15,15,1
cutlossMax,5,5,1
cutlossMin,0,0,1
cutlossDecrN,100,100,1
*)

namespace Algorithm
    module DecisionCalculator=(*017*)

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

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   OSCILLATORS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

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
         * @n: number of bars an extremum has to be more extreme on both sides
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

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>
                              //parameters:System.Collections.Generic.Dictionary<string, decimal>
                              )=

            let rsiN = 30
            let rsiEmaN = 20
            let rsiLong = 60m
            let rsiShort = 40m

            let barExtrN = 100
            let extrN = 1000
            let extrPIn = 15m
            let extrPOut = 15m

            let cutlossMax = 5m
            let mutable cutloss = cutlossMax
            let cutlossMin = 0m
            let cutlossDecrN = 100

            (*
             * Read Parameters
             *
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
            *)
            
            // Chart Lines
            chart2.Add("LocalExtremes;#00FFFF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_long;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_short;#0000FF", new System.Collections.Generic.List<decimal>())
            
            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]
            // list of typical prices
            let tPrices =
                [| for i in prices -> (i.Item3 + i.Item4 + i.Item5)/3m |]

            let useRsi = if (rsiN <> 0 && rsiEmaN <> 0) then true else false
            // calculate RSI
            let rsi = if (useRsi) then rsi (rsiN, tPrices) else Array.empty
            // smooth RSI
            let rsiEma = if (useRsi) then ema (rsiEmaN, Array.toList rsi) else Array.empty
            for i in 0..rsiEma.Length-1 do chart2.["RSI;#FF0000"].Add(rsiEma.[i])
            for i in 0..rsiEma.Length-1 do chart2.["RSI_long;#0000FF"].Add(rsiLong)
            for i in 0..rsiEma.Length-1 do chart2.["RSI_short;#0000FF"].Add(rsiShort)

            // try to find n bar price extrema
            let localExtrema = findExtremes (barExtrN, cPrices)
            // add to chart2
            for i in 0..localExtrema.Length-1 do chart2.["LocalExtremes;#00FFFF"].Add(localExtrema.[i])

            // price at trade entry (long or short)
            let mutable entryPrice = 0m
            // price extreme in trade for cut loss
            let mutable priceExtreme = cPrices.[0]

            // first index with all data
            let firstI = ([ rsiN; rsiEmaN ] |> List.max) - 1
            let mutable missingData = firstI+1

            signals.Clear();
            for i in 0 .. prices.Count-1 do
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
                            entry <- rsiSig

                    // open position / add to position
                    if (entry <> 0) then
                        signals.[i] <- entry
                    
                    /////////////////////////////////////
                    //////   EXIT SIGNAL
                    /////////////////////////////////////
                    let mutable exit = 4

                    (*
                     * // PRICE EXTREMES
                     *)

//                    // try to find n bar price extremes
//                    let firstExtrI = if (i-extrN+1 > 0) then (i-extrN+1) else 0
//                    printfn "%d before" i
//                    let localExtrema = findExtremes (barExtrN, cPrices.[firstExtrI..i])
//                    // add to chart2
//                    chart2.["LocalExtremes;#00FFFF"].Add(localExtrema.[localExtrema.Length-1])

                    // don't enter in extreme price situations
                    if (i >= extrN-1 && (extrN <> 0 && (extrPIn > 0m || extrPOut > 0m))) then
                        let firstExtrI = if (i-extrN+1 > 0) then (i-extrN+1) else 0

                        // maximum minus minimum price in range
                        let priceBreadth = ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item3] |> List.max) - ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item4] |> List.min)
                        let mins, maxs = getExtremeValues(extrN, cPrices.[firstExtrI..i], localExtrema.[firstExtrI..i])
                        if (entry > 0) then
                            for max in maxs do
                                let extrInVal = max-(extrPIn*0.01m*priceBreadth/2m)
                                let extrOutVal = max+(extrPOut*0.01m*priceBreadth/2m)
                                // don't go long in maxs
                                if (cPrices.[i] > extrInVal && cPrices.[i] < extrOutVal) then
                                    entry <- 0
                                    // neutral instead of new position
                                    if (signals.[i-1] < 0) then
                                        exit <- 0
                        else if (entry < 0) then
                            for min in mins do
                                let extrInVal = min-(extrPOut*0.01m*priceBreadth/2m)
                                let extrOutVal = min+(extrPIn*0.01m*priceBreadth/2m)
                                // don't short in mins
                                if (cPrices.[i] > extrInVal && cPrices.[i] < extrOutVal) then
                                    entry <- 0
                                    // neutral instead of new position
                                    if (signals.[i-1] > 0) then
                                        exit <- 0

                    (*
                     * // CUTLOSS: neutralise if loss is too big (% of price movement!)
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


                    (*
                     * // TRADING TIMES
                     *)

                    // Trading Times ignoring short pauses
                    // Monday to Friday: 0:00 - 22:10
                    if (match prices.[i].Item1.DayOfWeek with 
                        | System.DayOfWeek.Monday | System.DayOfWeek.Tuesday | System.DayOfWeek.Wednesday | System.DayOfWeek.Thursday | System.DayOfWeek.Friday
                            -> true
                        | _ -> false) then
                            if (prices.[i].Item1.Hour > 22 || (prices.[i].Item1.Hour = 22 && prices.[i].Item1.Minute > 10)) then
                                signals.[i] <- 0
                    // Saturday, Sunday (no trading)
                    else
                        signals.[i] <- 0
                    
            signals