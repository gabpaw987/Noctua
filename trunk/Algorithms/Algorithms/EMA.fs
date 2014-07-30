namespace Algorithm
    module DecisionCalculator3=(*3*)
        let rec emaRec(i:int,alpha:decimal,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            if(i = -1) then
                0.0m
            else
                alpha*list2D.[i].Item5+(1.0m-alpha) * (emaRec (i-1,alpha,list2D))

        let ema (alpha:decimal, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let length = list2D.Count
            let mutable resultList:List<decimal> = []
            let mutable mma = 0.0m
            for i = 0 to length-1 do
                mma <- emaRec(i,alpha,list2D)
                // adds a new result to the list
                resultList <- List.append resultList [mma]
            resultList

        let alphaToN (a) : int=
            int ((2.0m/a)-1.0m)
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

        let ema_peer (n:int, prices:List<decimal>)=
            let alpha = nToAlpha n
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

        let signalgeber_price(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            //for i = 0 to n-1 do
            //    signals.Add(0)
            let skip = 0

            // list of closing prices (skipped)
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.skip skip
                |> Seq.toList

            let ema = ema_peer(n,cPrices)

            for i = 0 to cPrices.Length-1 do
                if (cPrices.[i] > ema.[i]) then
                    signals.Add(1)
                else if (cPrices.[i] < ema.[i]) then
                    signals.Add(-1)
                else
                    signals.Add(0)
            signals

        let signalgeber_double(n1:int, n2:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            //for i = 0 to n-1 do
            //    signals.Add(0)
            let skip = 0

            // list of closing prices (skipped)
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.skip skip
                |> Seq.toList

            let short   = ema_peer(n1,cPrices)
            let long    = ema_peer(n2,cPrices)

            for i = 0 to cPrices.Length-1 do
                if (short.[i] > long.[i]) then
                    signals.Add(1)
                else if (short.[i] < long.[i]) then
                    signals.Add(-1)
                else
                    signals.Add(0)
            signals

        let signalgeber_triple(n1:int, n2:int, n3:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            //for i = 0 to n-1 do
            //    signals.Add(0)
            let skip = 0

            // list of closing prices (skipped)
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.skip skip
                |> Seq.toList

            let short   = ema_peer(n1,cPrices)
            let middle  = ema_peer(n2,cPrices)
            let long    = ema_peer(n3,cPrices)

            for i = 0 to cPrices.Length-1 do
                if (short.[i] < middle.[i] && middle.[i] < long.[i]) then
                    signals.Add(-1)
                else if (short.[i] > middle.[i] && middle.[i] > long.[i]) then
                    signals.Add(1)
                else
                    signals.Add(0)
            signals

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber_price (90, list2D, signals)