
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator5=
                
        (* This function calculates the simple moving average for a list of lists of decimal and returns the calculated values in a list of decimals*)
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable sma = [];
            for i = n to liste2D.Count - 1 do
                let mutable mom = 0m
                for j = i - (n-1) to i do
                    mom <- mom + liste2D.[j].Item5
                sma <- List.append sma [mom / decimal n]
            sma

        let signalgeber(n1:int, n2:int,teiler:decimal, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) = 
            for i = 0 to n2-1 do
                signals.Add(0)
            let short = sma(n1,list2D)
            let long = sma(n2,list2D)
            let mover = n2 - n1
            let mutable abw = 0m
            for i = 0 to long.Length-1 do
                abw <- decimal (sqrt ((double (short.[i+mover] - long.[i]))**2.0))
                let mutable multiplicator = int (abw % teiler)
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

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (10, 90, 10m, list2D, signals)