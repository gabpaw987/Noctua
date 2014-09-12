
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator6=
                
        (* This function calculates the simple moving average for a list of lists of decimal and returns the calculated values in a list of decimals*)
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable sma = [];
            for i = n to liste2D.Count - 1 do
                let mutable mom = 0m
                for j = i - (n-1) to i do
                    mom <- mom + liste2D.[j].Item5
                sma <- List.append sma [mom / decimal n]
            sma
        let tma (n:int, sma:decimal list)= 
            let mutable tma = [];
            for i = n to sma.Length - 1 do
                let mutable mom = 0m
                for j = i - (n-1) to i do
                    mom <- mom + sma.[j]
                tma <- List.append tma [mom / decimal n]
            tma
        
        let signalgeber1(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to 2 * n - 1 do
                signals.Add(0)
            let smas = sma(n,list2D)
            let ergebnis1 = tma(n, smas)
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
            let shortsmas = sma(shortn,list2D)
            let short = tma(shortn, shortsmas)
            let longsmas = sma(longn,list2D)
            let long = tma(longn, longsmas)
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