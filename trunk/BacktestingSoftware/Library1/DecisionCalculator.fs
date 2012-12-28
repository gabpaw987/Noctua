
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator=
        open System.IO
        let readLines (filePath:string) = seq {
            use sr = new StreamReader (filePath)
            while not sr.EndOfStream do
                yield sr.ReadLine ()
        }
        
        (* This function calculates the simple moving average for a list of lists of double and returns the calculated values in a list of doubles*)
        let sma (n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, double, double, double, double>>)=
            // saves the startposition
            let mutable position = n
            let mutable resultlist:List<double> = [0.0]
            for i = 0 to n do
                
                // calculated MovingAverage at the moment
                let mutable mma = 0.0
                for j = position-1 downto position-n do
                    // adds the close to mma
                    mma <- mma + liste2D.[j].Item5
                // divides by MA-length
                mma <- mma/(float n)
                // desides weither the resultList is set yet or not
                if resultlist.[0] = 0.0 then
                    // sets the first result
                    resultlist <- [mma]
                else 
                    let temp = [mma]
                    // adds a new result to the list
                    resultlist <- List.append resultlist temp
                // increments the position in the array
                position <- position + 1
            //returns the resultarray
            resultlist

        let signalgeber(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, double, double, double, double>>,signals:System.Collections.Generic.List<int>) =
            let ergebnis1 = sma(n,list2D)
            for i = 0 to ergebnis1.Length-1 do
                if list2D.[i+n-1].Item5 < ergebnis1.[i] then
                    signals.Add(-1)
                if list2D.[i+n-1].Item5 > ergebnis1.[i] then
                    signals.Add(1)
                if list2D.[i+n-1].Item5 = ergebnis1.[i] then
                    signals.Add(0)
            signals

        let startCalculation (n:int, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, double, double, double, double>>,signals:System.Collections.Generic.List<int>)= 
            //let o = new System.Random()
            //let mutable list2D:List<List<double>> = [[0.0;0.0;0.0;10.0]] 
            //for i = 0 to 100 do
            //    let test:double = o.NextDouble()
            //   if i = 0 then
            //        list2D <- [[0.0;0.0;0.0;(test*100.1)]]
            //    else 
            //        list2D <- List.append list2D [[0.0;0.0;0.0;(test*100.1)]]
            signalgeber (n, list2D, signals)