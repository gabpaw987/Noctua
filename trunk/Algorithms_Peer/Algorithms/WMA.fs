namespace Algorithm
    module DecisionCalculator4=
        (* This method calculates the sum from 1 to n *)
        let sum (n:int)= 
            let mutable result = 0m
            for i = 0 to int n do
                result <- result + decimal i
            result

        let wma (n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable position = n
            let mutable wma = []
            for i = n to liste2D.Count - 1 do
                let mutable mma = 0m
                let mutable weight = n
                for j = i-n to i do
                    mma <- mma + (liste2D.[j].Item5 * decimal weight)
                    weight <- weight - 1
                mma <- mma/(sum n)
                wma<- List.append wma [mma]
                position <- position + 1
            wma

        let signalgeber1(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
                    for i = 0 to n-1 do
                        signals.Add(0)
                    let ergebnis1 = wma(n,list2D)
                    for i = 0 to ergebnis1.Length-1 do
                        if list2D.[i+n-1].Item5 < ergebnis1.[i] then
                            signals.Add(-1)
                        if list2D.[i+n-1].Item5 > ergebnis1.[i] then
                            signals.Add(1)
                        if list2D.[i+n-1].Item5 = ergebnis1.[i] then
                            signals.Add(0)
                    signals
        
        let signalgeber(shortn:int, longn:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to 2 * longn - 1 do
                signals.Add(0)
            let short = wma(shortn, list2D)
            let long = wma(longn, list2D)
            let mover = longn - shortn
            let mutable abw = 0m
            for i = 0 to long.Length-1 do

                abw <- decimal (sqrt ((double (short.[i+mover] - long.[i]))**2.0))
                let mutable multiplicator = int (abw % 10m)
                if multiplicator > 3 then 
                    multiplicator <- 3
                if multiplicator = 0 then 
                    multiplicator <- 1
                if short.[i+mover] > long.[i] then
                    signals.Add(1*multiplicator)
                if short.[i+mover] < long.[i] then
                    signals.Add(-1*multiplicator)
                if short.[i+mover] = long.[i] then
                    signals.Add(0)
            signals

        let startCalculation (n:int, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (10, 90, list2D, signals)