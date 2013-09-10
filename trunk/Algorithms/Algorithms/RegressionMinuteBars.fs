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
        (* Linear Weighted Regression Method *)
        let lwrm(n:int, prices:decimal array)=
            let mutable xy = 0m
            let mutable xx = 0m
            let mutable x = 0m
            let mutable y = 0m
            let w = [|for i in 1 .. prices.Length do yield decimal i*(decimal n / decimal prices.Length)|]
            let wsum = Array.sum w
            for i = 0 to prices.Length - 1 do 
                x <- x + (((decimal i)*w.[i])/wsum)
                y <- y + (((prices.[i])*w.[i])/wsum)
            for i = 0 to prices.Length - 1 do 
            // (lr.X - xAvg) * (lr.Y - yAvg) * lr.Weight);
                xy <- xy + ((decimal i - x) * (prices.[i] - y) * w.[i])
            // System.Math.Pow(Convert.ToDouble(lr.X - xAvg), 2)) * lr.Weight)
                xx <- xx + decimal ((float i - float x)**2.0 ) * w.[i]
            
            xy / xx
            
        
                
        let signalgeber(n:int,bars:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>,ind:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>, osz:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>) =
            let values = [ for i in 0 .. bars.Count - 1 do yield bars.[i].Item5]
            let prices = values
            let cPrices = values
//            signals.Add(0)
            //for longterm trends
            let regression20 = Array.append (Array.zeroCreate(20)) [|for i in 20 .. prices.Length - 1 do yield 100m * regression([|for j in i - 20 .. i do yield values.[j]|])|]
            for i in osz.["80;#FFFF00"].Count .. regression20.Length - 1 do osz.["80;#FFFF00"].Add(regression20.[i])
            //confirms the regression90
            let regression10 = Array.append (Array.zeroCreate(10)) [|for i in 10 .. prices.Length - 1 do yield regression([|for j in i - 10 .. i do yield values.[j]|])|]
            let wregression10 = Array.append (Array.zeroCreate(10)) [|for i in 10 .. prices.Length - 1 do yield lwrm(2,[|for j in i - 10 .. i do yield values.[j]|])|]
            
            for i in osz.["60;#00FF00"].Count .. regression10.Length - 1 do osz.["60;#00FF00"].Add(regression10.[i])
            //deals with shortterm trends
//            let regression30 = Array.append (Array.zeroCreate(31)) [|for i in 30 .. prices.Length - 1 do yield regression([|for j in i - 30 .. i do yield cPrices.[j]|])|]
//            for i in osz.["30;#FFFF00"].Count .. regression30.Length - 1 do osz.["30;#FFFF00"].Add(regression30.[i])
            let regression8 = Array.append (Array.zeroCreate(8)) [|for i in 8 .. prices.Length - 1 do yield 100m * regression([|for j in i - 8 .. i do yield values.[j]|])|]
            for i in osz.["20;#000000"].Count .. regression8.Length - 1 do osz.["20;#000000"].Add(regression8.[i])
            //purpose for trend raising and less cutlos
            let regression5 = Array.append (Array.zeroCreate(5)) [|for i in 5 .. prices.Length - 1 do yield 100m * regression([|for j in i - 5 .. i do yield values.[j]|])|]
            for i in osz.["10;#0000FF"].Count .. regression5.Length - 1 do osz.["10;#0000FF"].Add(regression5.[i])
            //deals mainly to cutloss
//            let regression5 = Array.append (Array.zeroCreate(6)) [|for i in 5 .. prices.Length - 1 do yield regression([|for j in i - 5 .. i do yield cPrices.[j]|])|]
//            for i in osz.["5;#0000FF"].Count .. regression5.Length - 1 do osz.["5;#0000FF"].Add(regression5.[i])
            let regression2 = Array.append (Array.zeroCreate(2)) [|for i in 2 .. prices.Length - 1 do yield 100m * regression([|for j in i - 2 .. i do yield values.[j]|])|]
            let mutable topprice = 0m
            for i in osz.["2;#FF0000"].Count .. regression2.Length - 1 do osz.["2;#FF0000"].Add(regression2.[i])
            for i in 0 .. prices.Length - 1 do 
                signals.Add(0)

//                // long trending
                if regression20.[i] > 10m || regression20.[i] < -10m then
                    if regression10.[i] > 30m || regression10.[i] < -30m then
                        signals.[i] <- sign regression10.[i]
                    if regression8.[i] > 20m || regression8.[i] < -20m then
                        if signals.[i] <> 0 then
                            signals.[i] <- sign regression8.[i]
                    if sign regression5.[i] <> sign regression2.[i] && (regression5.[i] - regression2.[i]) * (regression5.[i] - regression2.[i]) > 25m then 
                        signals.[i] <- sign regression2.[i]
                    //signals.[i] <- 0
//                // short trending
//                if regression20.[i] > 0.2m || regression20.[i] < - 0.2m && signals.[i] = 0 then
//                    signals.[i] <- sign regression20.[i]
//                if sign regression10.[i] <> sign regression20.[i] then 
//                    signals.[i] <- 0
//                // cutloss
//                if i > 2 then
//                    if regression2.[i] - regression2.[i - 1] > 2m || regression2.[i] - regression2.[i - 1] <  - 2m then 
//                        signals.[i] <- signals.[i - 1] * -1
//                // TODO: highest price
                
            signals

        

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>, ind:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>, osz:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>)= 
            let a = signals.Clear
            if osz.Count = 0 then 
                osz.Add("80;#FFFF00",new System.Collections.Generic.List<decimal>())
                osz.Add("60;#00FF00",new System.Collections.Generic.List<decimal>())
                osz.Add("30;#FFFF00",new System.Collections.Generic.List<decimal>())
                osz.Add("20;#000000",new System.Collections.Generic.List<decimal>())
                osz.Add("10;#0000FF",new System.Collections.Generic.List<decimal>())
                osz.Add("5;#0000FF",new System.Collections.Generic.List<decimal>())                
                osz.Add("2;#FF0000",new System.Collections.Generic.List<decimal>())
            signalgeber (10, list2D, signals,ind, osz)
            
//            let sigs = signalgeber (10, list2D, signals,ind, osz)
//            for i in 0 .. signals.Count - 1 do 
//                if sign signals.[i] = 0 then
//                    signals.[i] <- signals.[i] - ((sign signals.[i]) * (-1))
//                signals.[i] <- sign signals.[i]
//            signals
            //for i in 0 .. signals.
//        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
//            signals.AddRange(seq{for i in 0 .. list2D.Count - 1 do yield 0})
//            signals
            