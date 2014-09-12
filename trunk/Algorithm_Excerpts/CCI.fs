// Weitere Informationen zu F# unter "http://fsharp.net".

namespace Algorithm
    module DecisionCalculator1=
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable position = n
            let mutable resultlist:List<decimal> = [0.0m]
            for i = n to liste2D.Count-1 do
                let mutable mma = 0.0m
                for j = i downto i - n  do
                    mma <- mma + liste2D.[j].Item4
                 // divides by MA-length
                mma <- mma/(decimal n)
                // desides weither the resultList is set yet or not
                if resultlist.[0] = 0.0m then
                    // sets the first result
                    resultlist <- [mma]
                else 
                    let temp = [mma]
                    // adds a new result to the list
                    resultlist <- List.append resultlist temp
            resultlist

        let deviation(liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let length = liste2D.Count
            let mutable avg = 0.0
            for i = 0 to liste2D.Count-1 do 
               avg <- avg + double (liste2D.[i].Item2+liste2D.[i].Item3+liste2D.[i].Item4)/3.0
            avg <- avg / (double length)
            let mutable sa = 0.0
            for i = 0 to liste2D.Count-1 do
                sa <- double (sa) + (double((liste2D.[i].Item2+liste2D.[i].Item3+liste2D.[i].Item4)/3m) - (avg))**2.0
            //avg
            sqrt sa*1.0/((double length)-1.0)

        let cci(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            //calculate the average high low close over the given period [t]
            let mutable tp = []
            let mutable smas = []
            let mutable deviations = []
            for i = liste2D.Count-n to liste2D.Count-1 do 
                tp <- List.append tp [(liste2D.[i].Item2+liste2D.[i].Item3+liste2D.[i].Item4)/3m]
                smas <- List.append smas (sma(n-1,liste2D.GetRange(i-n-1,n)))
                deviations <- List.append deviations [deviation(liste2D.GetRange(i-n-1,n))]
            let mutable resultList:List<decimal> = []
            let a = 1m/(0.015m)
            let b = (tp.[1]-smas.[1])
            let c = (decimal deviations.[1])
            let d = b/c
            for i = 0 to tp.Length-1 do
                resultList <- List.append resultList ([((tp.[i]-smas.[i])/(decimal deviations.[i]*0.015m))])
            resultList