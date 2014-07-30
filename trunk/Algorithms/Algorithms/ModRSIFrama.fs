namespace Algorithm
    module DecisionCalculator2007=(*007*)
        (*
         * rounds the given number to the nearest even number
         *)
        let even (x)=
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

        let mutable dimension = new System.Collections.Generic.List<decimal>()
        let mutable alphas = new System.Collections.Generic.List<decimal>()
        let mutable ns = new System.Collections.Generic.List<decimal>()

        // α = EXP(W*(D – 1))
        // D = (Log(HL1 + HL2) – Log(HL)) / Log(2)
        // Note: Log(2) = Log(N / (½N))
        // HL1 = (Max(High,½N..N) – Min(Low,½N..N)) / ½N
        // HL2 = (Max(High,½N) – Min(Low,½N)) / ½N
        // HL = (Max(High,N) – Min(Low,N)) / N
        // N = FRAMA Period, must be an even number.
        let framaA (w:decimal, prices:decimal[])=
            let n = prices.Length

            let max1 =
                [ for i in 0..(n/2)-1 -> prices.[i] ]
                |> List.max
            let min1 =
                [ for i in 0..(n/2)-1 -> prices.[i] ]
                |> List.min
            let hl1 = double(max1 - min1) / double(n/2)

            let max2 =
                [ for i in n/2..n-1 -> prices.[i] ]
                |> List.max
            let min2 =
                [ for i in n/2..n-1 -> prices.[i] ]
                |> List.min
            let hl2 = double(max2 - min2) / double(n/2)

            let max =
                [ for i in prices -> i ]
                |> List.max
            let min =
                [ for i in prices -> i ]
                |> List.min
            let hl = double(max - min) / double(n)

            let mutable d = 0.
            if(hl1 <> 0. && hl2 <> 0. && hl <> 0.) then 
                d <- (log10 (hl1 + hl2) - log10 (hl)) / log10(2.0)
            if (d < 1.0) then
                d <- 1.0
            else if (d > 2.0) then
                d <- 2.0
            dimension.Add(decimal(d))
            let alpha = decimal (exp ((double w)*(d - 1.0)))
            alphas.Add(alpha)
            alpha

        // W = LN(2 / (SC + 1))
        // New N = ((SC – FC) * ((Original N – 1) / (SC – 1))) + FC
        let frama (n:int, fc:int, sc:int, prices:decimal[])=
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
            frama.[n-2] <- [| for i in prices.[(n-1)-h..n-2] -> i |] |> Array.average
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
                    ns.Add(n)
                    // revert new N to new alpha
                    a <- nToAlphaDec n

                    // check w for out of range alpha values (modified FRAMA rule)
                    if (a > 1m) then
                        a <- 1m
                    if (a < 2m / decimal(sc + 1)) then
                        a <- 2m / decimal(sc + 1)

                    frama.[i] <- a * bar + (1m - a) * frama.[i-1]
                | _              -> ignore i)
            // set initial frama value (sma) to 0
            frama.[n-2] <- 0m
            frama
        

       
        let sma (n: int, prices:decimal[])  =
            let intervals = 
                prices
                |> Array.toSeq
                |> Seq.windowed n
            [|for i in intervals -> Array.average i|]
            |> Array.append (Array.zeroCreate (n - 1)) 
            

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


        
        (* Frama / Price, added Regression 4 middle- and longranged trendacceptance *)
(*
rsi,29,29,2
n,18,18,1
sc,40,40,1
fc,5,5,1
rsio,60,60,5
rsiu,40,40,5
*)
        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>
                              ,parameter:System.Collections.Generic.Dictionary<string, decimal>)=
//                              )=
            chart1.Clear();
            chart2.Clear();
//            parameter.Add("ema", 3m)
//            parameter.Add("rsi", 18m)
            // RSI Lines
            let rsi40 = new System.Collections.Generic.List<decimal>();
            for i in 0 .. prices.Count - 1 do rsi40.Add(40m)
            let rsi60 = new System.Collections.Generic.List<decimal>();
            for i in 0 .. prices.Count - 1 do rsi60.Add(60m)
            chart2.Add("RSI40;#0000FF", rsi40) 
            chart2.Add("RSI60;#0000FF", rsi60)

            let rsi = frama ((int) parameter.["n"],(int) parameter.["fc"],(int) parameter.["sc"], ( rsi((int parameter.["rsi"]), [| for i in 0 .. prices.Count - 1 do yield ((prices.[i].Item3) + (prices.[i].Item4) + (prices.[i].Item5))/3m|])))
            
            let rsiC = new System.Collections.Generic.List<decimal>();
            for i in 0 .. prices.Count - 1 do rsiC.Add(rsi.[i])
            chart2.Add("RSI;#00FF00", rsiC)

            signals.Add (0)
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

            signals