namespace Algorithm
    module DecisionCalculator018=(*018*)
        let fst (a,b) = a
        let sec (a,b) = b

        let ema (n:int, prices:List<decimal>)=
            let alpha = (2.0m / (decimal n + 1.0m))
            // return original prices if n = 1
            if (n = 1) then
                List.toArray prices
            else
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
            [| for i in 0 .. sumup.Length - 1 do yield if (sumup.[i] = sumdown.[i]) then 50m else 100m * (sumup.[i]/(decimal n))/((sumup.[i]/(decimal n)) + (sumdown.[i]/(decimal n)))|]
            |> Array.append (Array.create (n - 1) 0m)



        (*
            Finds the extreme price values in every n priceData in a simple way
        *)
        let findExtremeValues (n:int, prices:decimal[]) = 
            [|
                for i in n .. prices.Length - 1 do 
                    yield (Array.max(prices.[i - n .. i]),Array.min(prices.[i - n .. i]))
            |]
            


        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              parameters:System.Collections.Generic.Dictionary<string, decimal>
                              )=
            //retrieve extrenal parameters
            let rsiEmaN = if parameters.Count <> 0 then parameters.["emaN"] else 12m
            let rsiN = if parameters.Count <> 0 then parameters.["rsiN"] else 12m
            let rsiOb = if parameters.Count <> 0 then parameters.["rsiOb"] else 60m
            let rsiUn = if parameters.Count <> 0 then parameters.["rsiUn"] else 40m
            let rsiExtrOb = if parameters.Count <> 0 then parameters.["rsiExtrOb"] else 50m
            let rsiExtrUn = if parameters.Count <> 0 then parameters.["rsiExtrOb"] else 50m
            let rsiExtrN = if parameters.Count <> 0 then parameters.["rsiExtrN"] else 12m
            //let rsiEmaN = if parameters.Count <> 0 then parameters.["emaN"] else 12m
            let extrN = if parameters.Count <> 0 then parameters.["extrN"] else 100m
            let extrDiv = if parameters.Count <> 0 then parameters.["extrDiv"] else 5m
            let timeZone = if parameters.ContainsKey("timeZone") then int parameters.["extrDiv"] else 1

            let cPrices = [|for i in prices do yield i.Item5|]
            
            let extremes = findExtremeValues((int)extrN,cPrices)
            
            // add to chart 
            chart1.Add("max;#FF0000",new System.Collections.Generic.List<decimal>())
            chart1.Add("min;#0000FF",new System.Collections.Generic.List<decimal>())
            chart1.Add("maxob;#808080",new System.Collections.Generic.List<decimal>())
            chart1.Add("minob;#808080",new System.Collections.Generic.List<decimal>())
            chart1.Add("maxunt;#808080",new System.Collections.Generic.List<decimal>())
            chart1.Add("minunt;#808080",new System.Collections.Generic.List<decimal>())
            chart2.Add("rsiUn;#808000",new System.Collections.Generic.List<decimal>())
            chart2.Add("rsiOb;#808000",new System.Collections.Generic.List<decimal>())
            chart2.Add("rsiEma;#808000",new System.Collections.Generic.List<decimal>())
            chart2.Add("rsiExtrUn;#000080",new System.Collections.Generic.List<decimal>())
            chart2.Add("rsiExtrOb;#000080",new System.Collections.Generic.List<decimal>())
            chart2.Add("rsiExtr;#000080",new System.Collections.Generic.List<decimal>())
            
            

            let rsiExtr = rsi (int rsiExtrN, cPrices)
            let rsiEma = ema (int rsiEmaN, Array.toList (rsi (int rsiN, cPrices)))

            for i in 0 .. prices.Count - 1 do 
                chart2.["rsiUn;#808000"].Add(rsiUn)
                chart2.["rsiOb;#808000"].Add(rsiOb)
                chart2.["rsiEma;#808000"].Add(rsiEma.[i])
                chart2.["rsiExtrUn;#000080"].Add(rsiExtrUn)
                chart2.["rsiExtrOb;#000080"].Add(rsiExtrOb)
                chart2.["rsiExtr;#000080"].Add(rsiExtr.[i])

            for i in 0 .. cPrices.Length - 1 do
                signals.Add(0)
                let mutable entry = 0
                let mutable exit = 4
                if rsiExtr.[i] > rsiExtrOb && rsiExtr.[i] < rsiExtrUn then
                    if rsiEma.[i] > rsiOb then entry <- 1 else if rsiEma.[i] < rsiUn then entry <- -1

                if i > (int)extrN then
                    chart1.["max;#FF0000"].Add(fst(extremes.[i - (int) extrN]))
                    chart1.["min;#0000FF"].Add(sec(extremes.[i - (int) extrN]))
                    let momPrice = cPrices.[i]
                    let maxunt = fst(extremes.[i - (int) extrN - 1]) * (1m - (extrDiv/100m))
                    let maxob = fst(extremes.[i - (int) extrN - 1]) * (1m + (extrDiv/100m))
                    let minunt = sec(extremes.[i - (int) extrN - 1]) * (1m - (extrDiv/100m))
                    let minob = sec(extremes.[i - (int) extrN - 1]) * (1m + (extrDiv/100m))
                    chart1.["maxob;#808080"].Add(maxob)
                    chart1.["maxunt;#808080"].Add(maxunt)
                    chart1.["minob;#808080"].Add(minob)
                    chart1.["minunt;#808080"].Add(minunt)
                    if rsiExtr.[i] < 50m && rsiExtr.[i] > 30m then
                        if maxunt < cPrices.[i] && cPrices.[i] < maxob then
                            entry <- -1
                        else if minunt < cPrices.[i] && cPrices.[i] < minob then
                            entry <- 1
                    else
                        exit <- 0
                    if minob > maxunt then exit <- 4
                else 
                    chart1.["maxob;#808080"].Add(cPrices.[i])
                    chart1.["maxunt;#808080"].Add(cPrices.[i])
                    chart1.["minob;#808080"].Add(cPrices.[i])
                    chart1.["minunt;#808080"].Add(cPrices.[i])
                    chart1.["max;#FF0000"].Add(cPrices.[i])
                    chart1.["min;#0000FF"].Add(cPrices.[i])
                    
                if entry <> 0 then signals.[i] <- entry else if i <> 0 then signals.[i] <- signals.[i - 1]
                if exit <> 4 then signals.[i] <- exit

                if (match prices.[i].Item1.DayOfWeek with 
                    | System.DayOfWeek.Monday | System.DayOfWeek.Tuesday | System.DayOfWeek.Wednesday | System.DayOfWeek.Thursday | System.DayOfWeek.Friday
                        -> true
                    | _ -> false) then
                        if (prices.[i].Item1.Hour > 22 - 1 + timeZone || prices.[i].Item1.Hour < 8 - 1 + timeZone || (prices.[i].Item1.Hour = 22 - 1 + timeZone && prices.[i].Item1.Minute > 10)) then
                            signals.[i] <- 0
                // Saturday, Sunday (no trading)
                else
                    signals.[i] <- 0
                // currently the only supported time zones are -7 to +2 (trading MO - FR)
                // other settings will produce 0 signals
                if (timeZone > 2 || timeZone < -7) then
                    signals.[i] <- 0
            
            signals