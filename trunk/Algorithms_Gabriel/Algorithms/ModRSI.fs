namespace Algorithm
    module DecisionCalculator=
        let first ((a,b)) = a
        let second ((a,b)) = b
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
        
        let rsi2(n:int, prices:decimal[])=
            let gl = [| for i in 0 .. prices.Length - 1 do 
                        if i = 0 then 
                            yield (0m, 0m)
                        else 
                            let dif = prices.[i] - prices.[i - 1]
                            if dif > 0m then yield (dif,0m)
                            else if dif < 0m then yield (0m,dif)
                            else yield (0m,0m)
            |]
            let g = [|for j in gl do yield first j|]
            let l = [|for j in gl do yield second j|]
            let agl = [| for i in 0 .. prices.Length - 1 do
                            if i < n then
                                yield (0m, 0m)
                            else 
                                let ag = Array.average  (Array.sub g (i - n) n)
                                let al = Array.average  (Array.sub l (i - n) n) 
                                yield (ag,al)
                      |]
            let ag = [|for j in agl do yield first j|]
            let al = [|for j in agl do yield -1m * second j|]
            let rsi = Array.zeroCreate prices.Length
            [|for i in 0 .. prices.Length - 1 do 
                if i < n then yield 0m
                else if al.[i] = 0m then yield 100m
                else if ag.[i] = 0m then yield 1m
                else if ag.[i] = al.[i] then yield 50m
                //else yield 100m - (100m / (1m - (ag.[i]/al.[i]) ) )
                else yield  100m * ag.[i]/(ag.[i] + al.[i])    
            |]
            

        let divideZero denom nom =
            match denom with
            | 0m -> 0m
            | _ -> nom/denom
        
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


        
(*
rsi,14,30,2
ema,2,10,1
rsio,60,80,5
rsiu,20,40,5
extremaDeviation,0.25,0.75,0.25
extremaRange,100,1000,100
winnings,1,10,1
stopLoss,1,10,1
*)
        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>
                              ,parameter:System.Collections.Generic.Dictionary<string, decimal>
                              )=
            chart1.Clear();
            chart2.Clear();
//            let parameter = new System.Collections.Generic.Dictionary<string, decimal>();
//            //TODO: Hier kannst du's aendern 
//            parameter.Add("ema", 20m)
//            parameter.Add("rsi", 20m)
//            parameter.Add("rsio", 60m)
//            parameter.Add("rsiu", 40m)
//            parameter.Add("pts", 3m)
            
            // RSI Lines
            let rsi40 = new System.Collections.Generic.List<decimal>();
            for i in 0 .. prices.Count - 1 do rsi40.Add(40m)
            let rsi60 = new System.Collections.Generic.List<decimal>();
            for i in 0 .. prices.Count - 1 do rsi60.Add(60m)
            chart2.Add("RSI40;#0000FF", rsi40) 
            chart2.Add("RSI60;#0000FF", rsi60)

            let rsi = ema ((int) parameter.["ema"], Array.toList ( rsi2(((int) parameter.["rsi"]), [| for i in 0 .. prices.Count - 1 do yield ((prices.[i].Item3) + (prices.[i].Item4) + (prices.[i].Item5))/3m|])))
            //let rsi2 = ema ((int) parameter.["ema"], Array.toList ( rsi2(((int) parameter.["rsi"]), [| for i in 0 .. prices.Count - 1 do yield ((prices.[i].Item3) + (prices.[i].Item4) + (prices.[i].Item5))/3m|])))
            
            let rsiC = new System.Collections.Generic.List<decimal>();
            for i in 0 .. prices.Count - 1 do rsiC.Add(rsi.[i])
            chart2.Add("RSI;#00FF00", rsiC)

            let mutable extremas = new System.Collections.Generic.List<decimal>();

            let mutable old = 0
            signals.Add (0)

            let mutable lastBoughtPrice = 0m
            
            for i in 1 .. prices.Count - 1 do 
                
                signals.Add (0)
                if rsi.[i] > parameter.["rsio"] then
                    signals.[i] <- 1
                else if rsi.[i] < parameter.["rsiu"] then
                    signals.[i] <- -1
                else 
                    signals.[i] <- signals.[i - 1]
                if rsi.[i] = 0m then 
                    signals.[i] <- 0


                // positionsaver
                if signals.[i] <> signals.[i - 1] then 
                    lastBoughtPrice <- prices.[i].Item5


                // cutloss
                if signals.[i] = signals.[i - 1] && signals.[i] <> 0 && rsi.[i]<parameter.["rsio"] && rsi.[i]>parameter.["rsiu"] then
                    if signals.[i] = 1 then 
                        if  lastBoughtPrice - prices.[i].Item5 >= parameter.["pts"] then
                             signals.[i] <- 0
                    else 
                        if  prices.[i].Item5 - lastBoughtPrice >= parameter.["pts"] then
                             signals.[i] <- 0

                // Trading Times ignoring short pauses
                    // Monday to Friday: 0:00 - 22:10
                    if (match prices.[i].Item1.DayOfWeek with 
                        | System.DayOfWeek.Monday | System.DayOfWeek.Tuesday | System.DayOfWeek.Wednesday | System.DayOfWeek.Thursday | System.DayOfWeek.Friday
                            -> true
                        | _ -> false) then
                            if (prices.[i].Item1.Hour > 22 || (prices.[i].Item1.Hour = 22 && prices.[i].Item1.Minute > 10) || (prices.[i].Item1.Hour = 0 && prices.[i].Item1.Minute > (int) parameter.["rsi"])) then
                                signals.[i] <- 0
                    // Saturday, Sunday (no trading)
                    else
                        signals.[i] <- 0
                    
            signals
