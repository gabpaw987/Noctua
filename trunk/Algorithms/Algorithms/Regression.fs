namespace Algorithm
    module DecisionCalculator=
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

        (*  *)
        let signalgeber(n:int,prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            // skip already calculated signals
            let skip = if signals.Count-n+1 > 0 then signals.Count-n+1 else 0
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.skip skip
                |> Seq.toArray
            
            let longregressions = Array.append (Array.zeroCreate(20)) [|for i in 20 .. prices.Count - 1 do yield regression([|for j in i - 20 .. i do yield cPrices.[j]|])|]
            let shortregressions = Array.append (Array.zeroCreate(5)) [|for i in 5 .. prices.Count - 1 do yield regression([|for j in i - 5 .. i do yield cPrices.[j]|])|]
            for i in 0 .. prices.Count - 1 do 
                if(longregressions.[i]>0.6m && shortregressions.[i]>0m) then 
                    signals.Add(3)
                else if(longregressions.[i]>0.4m && shortregressions.[i]>0m) then 
                    signals.Add(2)
                else if(longregressions.[i]>0.2m && shortregressions.[i]>0m) then 
                    signals.Add(1)
                else if(longregressions.[i] > -0.6m && shortregressions.[i]<0m) then 
                    signals.Add(-3)
                else if(longregressions.[i] > -0.4m && shortregressions.[i]<0m) then 
                    signals.Add(-2)
                else if(longregressions.[i] > -0.2m && shortregressions.[i]<0m) then 
                    signals.Add(-1)
                else signals.Add(0)

            signals

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (20, list2D, signals)
            