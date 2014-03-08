namespace Algorithm
    module DecisionCalculator12341123=(*007*)
        let findExtremas (range:int, bars:decimal[]) = 
            let ret = Array.zeroCreate(bars.Length) //(0m,0m)
            for i in range .. bars.Length - 1 do
                let mutable max = 0m
                let mutable min = 10000000m
                for j in (i - range) + 2 .. i - 1 do 
                    if bars.[j - 1] < bars.[j] && bars.[j] > bars.[j + 1] then 
                        if max < bars.[j] then max <- bars.[j]
                    if bars.[j - 1] > bars.[j] && bars.[j] < bars.[j + 1] then 
                        if min > bars.[j] then min <- bars.[j]
                ret.[i] <- (max,min)
            ret

        let first (a , b) = a
        let second (a, b) = b
 
        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>
                              ,parameter:System.Collections.Generic.Dictionary<string, decimal>)=
            chart2.Clear();
            let closes = [|for i in prices do yield i.Item5|]
            let extremas = findExtremas((int)parameter.["extremaRange"], closes)
            for i in 0 .. prices.Count - 1 do 
                signals.Add(0)
                if i>(int) parameter.["extremaRange"] then
                    if first(extremas.[i]) = prices.[i].Item5 then 
                        signals.[i] <-  -1
                    if second(extremas.[i]) = prices.[i].Item5 then 
                        signals.[i] <-  1
            signals