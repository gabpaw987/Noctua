
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator=
        open System.IO
        let readLines (filePath:string) = seq {
            use sr = new StreamReader (filePath)
            while not sr.EndOfStream do
                yield sr.ReadLine ()
        }
        
        (* This function calculates the simple moving average for a list of lists of decimal and returns the calculated values in a list of decimals*)
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable position = n
            let mutable resultlist:List<decimal> = [0m]
            for i = n to liste2D.Count-1 do
                let mutable mma = 0m
                for j = i downto i - n do
                    mma <- mma + liste2D.[j].Item5
                    // divides by MA-length
                mma <- mma/(decimal n)
                // desides weither the resultList is set yet or not
                if resultlist.[0] = 0m then
                    // sets the first result
                    resultlist <- [mma]
                else 
                    let temp = [mma]
                    // adds a new result to the list
                    resultlist <- List.append resultlist temp
            resultlist

        let signalgeber(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n-1 do
                signals.Add(0)
            let ergebnis1 = sma(n,list2D)
            for i = 0 to ergebnis1.Length-1 do
                if list2D.[i+n-1].Item5 < ergebnis1.[i] then
                    signals.Add(-1)
                if list2D.[i+n-1].Item5 > ergebnis1.[i] then
                    signals.Add(1)
                if list2D.[i+n-1].Item5 = ergebnis1.[i] then
                    signals.Add(0)
            signals

        let startCalculation (n:int, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            signalgeber (n, list2D, signals)