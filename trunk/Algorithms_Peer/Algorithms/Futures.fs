namespace Algorithm
    module DecisionCalculator=(*007*)

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
            int ((2.0m/a)-1.0m)
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

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
                [| for i in 0..(n/2)-1 -> prices.[i].Item3 |]
                |> Array.max
            let min1 =
                [| for i in 0..(n/2)-1 -> prices.[i].Item4 |]
                |> Array.max
            let hl1 = double(max1 - min1) / double(n/2)

            let max2 =
                [| for i in n/2..n-1 -> prices.[i].Item3 |]
                |> Array.max
            let min2 =
                [| for i in n/2..n-1 -> prices.[i].Item4 |]
                |> Array.min
            let hl2 = double(max2 - min2) / double(n/2)

            let max =
                [| for i in prices -> i.Item3 |]
                |> Array.max
            let min =
                [| for i in prices -> i.Item4 |]
                |> Array.min
            let hl = double(max - min) / double(n)

            let d = (log10 (hl1 + hl2) - log10 (hl)) / log10(2.0)
            decimal (exp ((double w)*(d - 1.0)))

        // W = LN(2 / (SC + 1))
        // New N = ((SC – FC) * ((Original N – 1) / (SC – 1))) + FC
        let frama (n:int, fc:int, sc:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let prices = [| for i in prices -> i |]
            
            // calculate original FRAMA w (constant over price range!)
            let origW = decimal (log (2.0 / double(sc + 1)))
            // convert original FRAMA w to original frama N
            let origWN = alphaToN origW
            // move the WN according to the given minimum N (FC)
            let wn = (sc - fc) * int (round (decimal(origWN - 1) / decimal(sc - 1))) + fc
            // calculate modified w
            let mutable w = nToAlpha wn
            // check w for out of range values
            if (w > 1m) then
                w <- 1m
            if (w < decimal (2/ (sc + 1))) then
                w <- decimal(2/ (sc + 1))
            let w = w

            let frama : decimal array = Array.zeroCreate (prices.Length)

            // calculate first FRAMA value
            // H = EVEN( ((SC – FC) / 2) ) + FC
            let mutable h = even ((sc - fc) / 2) + fc
            // check H for out of range values
            if (wn - 1 < even ((sc - fc) / 2) + fc) then
                h <- wn-1

            // calculate the first FRAMA value
            // FRAMA(N-1) = (CLOSE + H)/H
            frama.[n-2] <- (prices.[n-2].Item5 + decimal(h))/decimal(h)
            // calculate all FRAMA values
            prices
            |> Array.iteri (fun i bar -> 
                match i with
                | _ when i > n-2 ->
                    let a = framaA (w, prices.[i-n+1..i])
                    frama.[i] <- a * bar.Item5 + (1m - a) * frama.[i-1]
                | _              -> ignore i)
            // set initial frama value (sma) to 0
            frama.[n-2] <- 0m
            frama

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

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>)=

            // Chart Lines
            chart1.Add("FRAMA;#FF0000", new System.Collections.Generic.List<decimal>()) 
            chart2.Add("W%R;#FF0000", new System.Collections.Generic.List<decimal>())
            
            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]

            let frama = frama(200,10,200,prices)
            for i in 0..frama.Length-1 do chart1.["FRAMA;#FF0000"].Add(frama.[i])
            
            let w = williamsR(14, prices)
            for i in 0..w.Length-1 do chart2.["W%R;#FF0000"].Add(w.[i])

            signals.Clear();
            for i in signals.Count .. prices.Count-1 do
                // keep last signal
                signals.Add(if (i=0) then 0 else signals.[i-1])

            signals