namespace Algorithm
    module DecisionCalculator15=

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
        
        let tmaDoubleCross(shortn:int, longn:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
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
        
        let readIndex (path:string)= 
            let mutable ownTupleList = new System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>()
            let str = System.IO.File.ReadAllText(path)
            let mutable strArray:List<string> = List.ofArray(str.Split('\n'))
            let mutable fullySplittedStringArrayArray:List<List<string>> = []
            for s:string in strArray do
                fullySplittedStringArrayArray <- List.append fullySplittedStringArrayArray [List.ofArray(s.Split(','))]
            let mutable notfirst = false
            for list:List<string> in fullySplittedStringArrayArray do
                if(notfirst&&list.Length=7) then
                    let date = System.DateTime.ParseExact(list.Item(1)+" "+list.Item(2),"MM/dd/yy HH:mm", new System.Globalization.CultureInfo("en-US"))
                    let opendec = decimal (list.Item(3))
                    let highdec = decimal (list.Item(4))
                    let lowdec = decimal (list.Item(5))
                    let closedec = decimal (list.Item(6))
                    ownTupleList.Add(new System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>(date, opendec, highdec, lowdec, closedec))
                else
                    notfirst <- true
            ownTupleList    
        
        let signalgenerator(prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,index:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=

            let indexSignals = tmaDoubleCross (20, 80, index, new System.Collections.Generic.List<int>())
            let priceSignals = tmaDoubleCross (20, 80, prices, new System.Collections.Generic.List<int>())

            if (indexSignals.Count = priceSignals.Count) then
                // length of lists
                let len = indexSignals.Count;
                // count of matches for each possible time shift
                let matchCount:System.Collections.Generic.List<int> = new System.Collections.Generic.List<int>()
                let matchRel:System.Collections.Generic.List<decimal> = new System.Collections.Generic.List<decimal>()

                // shift up to 50% to each side
                // i = time shift between data
                let minNegShift:int = -1 * int (floor((float len)/2.0))
                let maxPosShift:int = len - (len%2) - int (floor((float len)/2.0))
                let mutable bestMatch = 0.0m
                let mutable bestShift = 0
                
                for i=minNegShift to maxPosShift do
                    //System.Console.WriteLine(i)
                    matchCount.Add(0)

                    let numComp = ceil((float len)/2.0)
                    for j=0 to int numComp-1 do
                        // first half: shift price back
                        if (i < 0) then
                            if (indexSignals.[j+System.Math.Abs(i)] = priceSignals.[j]) then
                                matchCount.[matchCount.Count-1] <- matchCount.[matchCount.Count-1]+1
                        // no shift (direct comparison)
                        elif (i = 0) then
                            if (indexSignals.[j] = priceSignals.[j]) then
                                matchCount.[matchCount.Count-1] <- matchCount.[matchCount.Count-1]+1
                        // second half: shift price to future
                        else
                            if (indexSignals.[j+System.Math.Abs(i)] = priceSignals.[j]) then
                                matchCount.[matchCount.Count-1] <- matchCount.[matchCount.Count-1]+1

                    // divide absolute count by number of compared pairs
                    matchRel.Add(decimal matchCount.[matchCount.Count-1] / decimal numComp)

                    // save best match and the associated time shift
                    if (matchRel.[matchCount.Count-1] > bestMatch) then
                        bestMatch <- matchRel.[matchCount.Count-1]
                        bestShift <- i

                if (bestMatch >= 0.8m) then 
                    printfn "Best match is: %e with a time shift of: %d" bestMatch bestShift
                else
                    System.Console.WriteLine("Not so good match is: {0:D} with a time shift of: {1:D}", bestMatch, bestShift);
            signals
        
        (* returns the slope of the given points, uses the close of all given values*)
        let regression2(liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable xy = 0m
            let mutable xx = 0m
            let mutable x = 0m
            let mutable y = 0m
            for i = 0 to liste2D.Count - 1 do 
                x <- x+decimal i
                y <- y + liste2D.[i].Item5
                xy <- xy + liste2D.[i].Item5 * decimal i
                xx <- xx + decimal i* decimal i
            let b = (decimal liste2D.Count * xy - (x*y))/(decimal liste2D.Count * xx - x*x)
            let a = (y - b*x)/decimal liste2D.Count
            decimal b

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            let index = readIndex("C:/noctua/trunk/Input_Data/SPX_1dBar_20130220.csv")
            signalgenerator (list2D,index, signals)