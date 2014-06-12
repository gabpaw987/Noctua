namespace Algorithm
    module DecisionCalculator3=
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

        let signalgeber(n:decimal,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            //for i = 0 to n-1 do
            //    signals.Add(0)
            let ergebnis1 = ema(n,list2D)
            for i = 0 to ergebnis1.Length-1 do
                if list2D.[i].Item5 < ergebnis1.[i] then
                    signals.Add(-1)
                if list2D.[i].Item5 > ergebnis1.[i] then
                    signals.Add(1)
                if list2D.[i].Item5 = ergebnis1.[i] then
                    signals.Add(0)
            signals

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (0.4m, list2D, signals)