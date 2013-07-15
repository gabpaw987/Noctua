namespace Algorithm
    module DecisionCalculator=
        let divideZero denom nom =
            match denom with
            | 0m -> 0m
            | _ -> nom/denom
        
        let alphaToN (a) : int=
            int ((2.0m/a)-1.0m)
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

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


        let tema (n:int, prices:List<decimal>)=
            let ema1 = ema(n, prices)
            let ema2 = ema(n, Array.toList (ema1))
            let ema3 = ema(n, Array.toList (ema2))
            [| for i in 0 .. ema1.Length - 1 -> (3m * ema1.[i] - 3m * ema2.[i] + ema3.[i]) |]
            
        let tripleCrossed(n1:int,n2:int,n3:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to n3-1 do
                signals.Add(0)
            let shorts = (* List.append [for i in 0..n1-1 -> 0m]*) (tema(n1,[for i in 0 .. list2D.Count-1 -> list2D.[i].Item5]))
            let middle = (*List.append [for i in 0..n2-1 -> 0m]*) (tema(n2,[for i in 0 .. list2D.Count-1 -> list2D.[i].Item5]))
            let longs = (*List.append [for i in 0..n3-1 -> 0m]*) (tema(n3,[for i in 0 .. list2D.Count-1 -> list2D.[i].Item5]))
            for i = n3 to longs.Length-1 do
                if shorts.[i] < middle.[i] && middle.[i] < longs.[i] then
                    if(signals.[signals.Count-1]=3)then signals.Add(3)
                    else signals.Add(-1)//*momentumInterpreter(5,list2D.GetRange(i-n2,n2)))
                else if shorts.[i] > middle.[i] && middle.[i] > longs.[i] then
                    if(signals.[signals.Count-1]= -3)then signals.Add(-3)
                    else signals.Add(1)//*momentumInterpreter(5,list2D.GetRange(i-n2,n2)))
                else
                    // add the last again
                    // signals.Add(signals.[signals.Count-1])
                    // add zero
                    signals.Add(0)
            signals


        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            tripleCrossed(10,15,20,list2D,signals)
            //signalgeber (20, list2D, signals)
