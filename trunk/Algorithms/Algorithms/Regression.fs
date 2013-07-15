namespace Algorithm
    module DecisionCalculator12345=
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
        let regression2(prices:decimal array)=
            let x = 
                [|0 .. prices.Length - 1|] 
                |> Array.sum 
                |> decimal
            let y = 
                [| for i in 0 .. prices.Length - 1 -> 
                    prices.[i]|] 
                |> Array.sum
            let xy = 
                [| for i in 0 .. prices.Length - 1 -> 
                    prices.[i] * decimal i|] 
                |> Array.sum
            let xx = 
                [| for i in 0 .. prices.Length - 1 -> 
                    i*i|] 
                |> Array.sum 
                |> decimal
            let b = (decimal prices.Length * xy - (x*y))/
                    (decimal prices.Length * xx - x*x)
            let a = (y - b*x)/decimal prices.Length
            //decimal liste2D.Count*b + a
            b
        (*  *)
        let signalgeber(n:int,prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            // skip already calculated signals
            let skip = if signals.Count-n+1 > 0 then signals.Count-n+1 else 0
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.skip skip
                |> Seq.toArray
            //for longterm trends
            let regression90 = Array.append (Array.zeroCreate(80)) [|for i in 80 .. prices.Count - 1 do yield regression([|for j in i - 80 .. i do yield cPrices.[j]|])|]
            //confirms the regression90
            let regression60 = Array.append (Array.zeroCreate(60)) [|for i in 60 .. prices.Count - 1 do yield regression([|for j in i - 60 .. i do yield cPrices.[j]|])|]
            //deals with shortterm trends
            let regression30 = Array.append (Array.zeroCreate(30)) [|for i in 30 .. prices.Count - 1 do yield regression([|for j in i - 30 .. i do yield cPrices.[j]|])|]
            let regression20 = Array.append (Array.zeroCreate(20)) [|for i in 20 .. prices.Count - 1 do yield regression([|for j in i - 20 .. i do yield cPrices.[j]|])|]
            //purpose for trend raising and less cutlos
            let regression10 = Array.append (Array.zeroCreate(10)) [|for i in 10 .. prices.Count - 1 do yield regression([|for j in i - 10 .. i do yield cPrices.[j]|])|]
            //deals mainly to cutloss
            let regression5 = Array.append (Array.zeroCreate(5)) [|for i in 5 .. prices.Count - 1 do yield regression([|for j in i - 5 .. i do yield cPrices.[j]|])|]
            let regression2 = Array.append (Array.zeroCreate(2)) [|for i in 2 .. prices.Count - 1 do yield regression([|for j in i - 2 .. i do yield cPrices.[j]|])|]
            let mutable strength = 1
            for i in 0 .. prices.Count - 1 do 
                //if market breaks in
                let mutable extremePriceFluctuation = false
                // for longterm strength
                let mutable strength = 1
                if(regression90.[i] > 1m || regression90.[i] < -1m) then
                    strength <- strength + 1
                if(regression60.[i] > 1m || regression60.[i] < -1m) then
                    strength <- strength + 1                
                if(signals.Count > 2) then
                    if sign regression10.[i] <> sign signals.[i-1] && sign signals.[i-1] <> 0 && sign regression5.[i] <> sign signals.[i-1] then 
                        if abs regression10.[i] + abs regression5.[i] > 0.5m  then
                            extremePriceFluctuation <- true
                            printf "Cut"
//                    else if sign regression2.[i] <> sign signals.[i - 1] && regression2.[i] > decimal(50 * sign regression2.[i])  then 
//                        extremePriceFluctuation <- true
//                        printf "Cut"
                // if 5 10 20 30 are going to the same direction   && sign regression20.[i] = sign regression30.[i]
                if sign regression10.[i] = sign regression20.[i]  && sign regression20.[i] = sign regression30.[i] then 
                    signals.Add(1*sign regression10.[i]*strength)
                    if signals.Count > 2 then if signals.[i-1] = 0 then extremePriceFluctuation <- false
                    if(regression5.[i]>1m && regression10.[i]>1m && sqrt(float(strength)**2.0) < 2.0) then
                        signals.[i] <- 2 * sign regression5.[i]
                else if sign regression20.[i] = sign regression30.[i] then signals.Add(signals.[i-1])
                else signals.Add(0)
//                if extremePriceFluctuation then 
//                    signals.[i] <- 0 
//                    extremePriceFluctuation <- false
                if signals.Count > 2 then 
                    if sign signals.[i] = sign signals.[i-1] then 
                        if sign signals.[i] = 1 then 
                            if signals.[i] < signals.[i - 1] then 
                                signals.[i] <- signals.[i-1]
                        if sign signals.[i] = -1 then 
                            if signals.[i] > signals.[i - 1] then 
                                signals.[i] <- signals.[i-1]
                    if -0.5m < regression10.[i] && regression10.[i] < 0.5m && signals.[i - 1] = 0 then 
                        signals.[i] <- 0
                        printf "Market isn't volatile enough \n"
//                    if -1m > regression5.[i] && sign signals.[i - 1] = sign 1 then 
//                        signals.[i] <- 0
//                        printf "Market will go down \n"

                let mutable highestk = 0m
                if highestk < regression5.[i] && sign regression5.[i] = 1 then highestk <- regression5.[i]
                if regression5.[5] > highestk * 0.95m  && sign regression5.[i] <> 1 then 
                    signals.[i] <- 0
                    highestk <- 0m                        
//                let mutable lowestk = 0m
//                if lowestk > regression5.[i] && sign regression5.[i] = -1 then lowestk <- regression5.[i]
//                if regression5.[5] < lowestk * 0.95m  && sign regression5.[i] <> -1 then 
//                    //signals.[i] <- 0
//                    lowestk <- 0m  
            signals

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (20, list2D, signals)
//        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
//            signals.AddRange(seq{for i in 0 .. list2D.Count - 1 do yield 0})
//            signals
            