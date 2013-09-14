namespace Algorithm
    module DecisionCalculator007=(*007*)
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
            let a = (0m,0m,0m,0m)
            Array.toList tr

        let sec (_,c,_,_) = c
        let third (_,_,c,_) = c
        let fourth (_,_,_,c) = c
        let trueRange(newBar:(decimal * decimal * decimal * decimal), oldBar:(decimal * decimal * decimal * decimal))=
            [sec newBar - third newBar; sec newBar - fourth oldBar; fourth oldBar - third newBar]
            |> List.max


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
            let a = (y - b*x)/decimal prices.Length
            //decimal liste2D.Count*b + a
            b

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
                    ns.Add(n)
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
        

        (* Frama / Price, added Regression 4 middle- and longranged trendacceptance *)
        let startCalculation1 (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                               signals:System.Collections.Generic.List<int>,
                               chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                               chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                               parameter:System.Collections.Generic.Dictionary<string, decimal>)=

(*
FramaN,100,152,26
FramaFC,4,20,16
FramaSC,200,300,50
r1,20,60,10
r2,50,100,10
*)
            chart1.Clear();
            // Chart Lines
            chart1.Add("FRAMA;#00FF00", new System.Collections.Generic.List<decimal>()) 
            //chart2.Add("W%R;#FF0000", new System.Collections.Generic.List<decimal>())
            
            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]


            let frama = frama((int)parameter.["FramaN"],(int)parameter.["FramaFC"],(int)parameter.["FramaSC"],prices)
            for i in 0..frama.Length-1 do chart1.["FRAMA;#00FF00"].Add(frama.[i])
            
            let mtr = Array.append (Array.zeroCreate((int)parameter.["r1"])) [|for i in (int)parameter.["r1"] .. prices.Count - 1 do yield regression([|for j in i - (int)parameter.["r1"] .. i do yield cPrices.[j]|])|]  
            let ltr = Array.append (Array.zeroCreate((int)parameter.["r2"])) [|for i in (int)parameter.["r2"] .. prices.Count - 1 do yield regression([|for j in i - (int)parameter.["r2"] .. i do yield cPrices.[j]|])|]
                        
            signals.Clear();
            for i in 0 .. frama.Length-1 do
                if frama.[i] < cPrices.[i] then
                    signals.Add(1)
                    if mtr.[i] > 0.10m then
                        signals.[i] <- 2
                    if ltr.[i] > 0.10m && signals.[i] = 2 then
                        signals.[i] <- 3
                else if frama.[i] > cPrices.[i] then
                        signals.Add(-1)
                        if mtr.[i] < -0.10m then
                            signals.[i] <- -2
                        if ltr.[i] < -0.10m && signals.[i] = -2 then
                            signals.[i] <- -3
                if sign ltr.[i] <> sign mtr.[i] || frama.[i] = 0m || ltr.[i] = 0m then signals.[i] <- 0

            signals

            (* Frama / Price, added Regression 4 middle- and longranged trendacceptance *)
(*
FramaLongN,50,50,1
FramaLongFC,1,1,1
FramaLongSC,200,200,1
FramaShortN,5,5,1
FramaShortFC,1,1,1
FramaShortSC,10,10,1
r1,20,60,10
r2,50,100,10
ts,0.5,1.5,0.5
ADXS,30,40,5
*)
        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              parameter:System.Collections.Generic.Dictionary<string, decimal>)=
            chart1.Clear();
            // Chart Lines
            chart1.Add("FRAMALong;#00FF00", new System.Collections.Generic.List<decimal>()) 
            chart1.Add("FRAMAShort;#FF0000", new System.Collections.Generic.List<decimal>())
            
            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]


            let framaLong = frama((int)parameter.["FramaLongN"],(int)parameter.["FramaLongFC"],(int)parameter.["FramaLongSC"],prices)
            for i in 0..framaLong.Length-1 do chart1.["FRAMALong;#00FF00"].Add(framaLong.[i])
            
            let framaShort = frama((int)parameter.["FramaShortN"],(int)parameter.["FramaShortFC"],(int)parameter.["FramaShortSC"],prices)
            for i in 0..framaShort.Length-1 do chart1.["FRAMAShort;#FF0000"].Add(framaShort.[i])
            

            let mtr = Array.append (Array.zeroCreate((int)parameter.["r1"])) [|for i in (int)parameter.["r1"] .. prices.Count - 1 do yield regression([|for j in i - (int)parameter.["r1"] .. i do yield cPrices.[j]|])|]  
            let ltr = Array.append (Array.zeroCreate((int)parameter.["r2"])) [|for i in (int)parameter.["r2"] .. prices.Count - 1 do yield regression([|for j in i - (int)parameter.["r2"] .. i do yield cPrices.[j]|])|]
            
            let ts = parameter.["FramaShortN"]
                 
            let adx = adx(14,prices)     
                        
            signals.Clear();
            for i in 0 .. framaLong.Length-1 do
                signals.Add(0)
                if framaLong.[i] < framaShort.[i] then
                    signals.[i] <- (1)
                    if mtr.[i] > ts then
                        signals.[i] <- 2
                    if ltr.[i] > ts && signals.[i] = 2 then
                        signals.[i] <- 3
                else if framaLong.[i] > framaShort.[i] then
                        signals.[i] <- (-1)
                        if mtr.[i] < - ts then
                            signals.[i] <- -2
                        if ltr.[i] < - ts && signals.[i] = -2 then
                            signals.[i] <- -3
                if signals.Count > 2 then
                    if sign signals.[i - 1] = sign signals.[i] then
                        if sign signals.[i] = 1 then
                            if signals.[i - 1] > signals.[i] then 
                                signals.[i] <- signals.[i - 1]
                        else if sign signals.[i] = -1 then
                            if signals.[i - 1] < signals.[i] then
                                signals.[i] <- signals.[i - 1]
                if sign ltr.[i] <> sign mtr.[i] || framaLong.[i] = 0m || ltr.[i] = 0m then signals.[i] <- 0
                if mtr.[i] < 0.08m && mtr.[i] > - 0.08m then signals.[i] <- 0
                if adx.[i] < parameter.["ADXS"] then signals.[i] <- 0
            signals