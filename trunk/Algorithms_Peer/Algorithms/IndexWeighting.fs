namespace Algorithm
    module DecisionCalculator15=(*15*)

        open System.IO
            
        (* This function calculates the simple moving average for a list of lists of decimal and returns the calculated values in a list of decimals*)
        let sma (n:int) (prices:decimal[]) =
            Array.append (Array.zeroCreate (n-1)) (Seq.windowed n prices
                |> Seq.map Array.average
                |> Seq.toArray)

        let tma (n:int) (prices:decimal[]) =
            let m = int(ceil ((float n)/2.0))
            let tma = prices |> sma n |> sma m
            for i in 0..(n+m-2) do
                tma.[i] <- 0m
            tma
            //Array.sub (sma n prices) (m-1) (prices.Length-m-1)
            //|> sma m

        let tmaDoubleCross (n1:int, n2:int, prices:decimal[]) =
            let signals = Array.zeroCreate(prices.Length)
            
            let tma1 = tma n1 prices
            let tma2 = tma n2 prices

            for i in 0..tma1.Length-1 do
                if tma1.[i] > tma2.[i] then
                    signals.[i] <- 1
                else if tma1.[i] < tma2.[i] then
                    signals.[i] <- -1
                else
                    signals.[i] <- 0
            signals

        let readIndex (path:string)= 
            let splittedData = (File.ReadAllText(path)).Split([|"\r\n"|], System.StringSplitOptions.RemoveEmptyEntries)
            let data = Array.init (splittedData.Length-2) (fun index -> 
                let a = (splittedData.[index+1]).Split([|','|])
                (System.DateTime.ParseExact(a.[1]+" "+a.[2], "MM/dd/yy HH:mm", new System.Globalization.CultureInfo("en-US")), decimal(a.[3]), decimal(a.[4]), decimal(a.[5]), decimal(a.[6])))
           
            //let prices = [|for line in splittedData -> (fun -> let entries = line.Split([|','|]) ( date, (decimal)entries.[4], (decimal)entries.[5], (decimal)entries.[6], (decimal)entries.[7] ))|]
            data
        
        // DEPRECATED
//        let readIndex (path:string)= 
//            let mutable ownTupleList = new System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>()
//            let str = System.IO.File.ReadAllText(path)
//            let mutable strArray:List<string> = List.ofArray(str.Split('\n'))
//            let mutable fullySplittedStringArrayArray:List<List<string>> = []
//            for s:string in strArray do
//                fullySplittedStringArrayArray <- List.append fullySplittedStringArrayArray [List.ofArray(s.Split(','))]
//            let mutable notfirst = false
//            for list:List<string> in fullySplittedStringArrayArray do
//                if(notfirst&&list.Length=7) then
//                    let date = System.DateTime.ParseExact(list.Item(1)+" "+list.Item(2),"MM/dd/yy HH:mm", new System.Globalization.CultureInfo("en-US"))
//                    let opendec = decimal (list.Item(3))
//                    let highdec = decimal (list.Item(4))
//                    let lowdec = decimal (list.Item(5))
//                    let closedec = decimal (list.Item(6))
//                    ownTupleList.Add(new System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>(date, opendec, highdec, lowdec, closedec))
//                else
//                    notfirst <- true
//            ownTupleList    
        
        let indexMatching(prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, index:(System.DateTime*decimal*decimal*decimal*decimal)[], signals:System.Collections.Generic.List<int>)=
            // list of closing prices (skipped)
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.toArray
            let fifth (_, _, _, _, c) = c
            let cIndex = 
                [ for i in index -> fifth i ]
                |> Seq.toArray

            let mutable indexSignals = tmaDoubleCross (20, 80, cIndex)
            let mutable priceSignals = tmaDoubleCross (20, 80, cPrices)

            let skip = 80+40-2;

            indexSignals <- Array.sub indexSignals (skip+1) (indexSignals.Length-skip-1)
            priceSignals <- Array.sub priceSignals (skip+1) (priceSignals.Length-skip-1)

            if (indexSignals.Length = priceSignals.Length) then
                // length of lists
                let len = indexSignals.Length;
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
                    matchRel.Add((decimal matchCount.[matchCount.Count-1] / decimal numComp) * 100m)

                    // save best match and the associated time shift
                    if (matchRel.[matchCount.Count-1] > bestMatch) then
                        bestMatch <- matchRel.[matchCount.Count-1]
                        bestShift <- i

                if (bestMatch >= 0.8m) then 
                    printfn "Best match is: %f with a time shift of: %d" bestMatch bestShift
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

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            printfn "Reading index..."
            let index = readIndex("C:/noctua/trunk/Input_Data/MSFT_1dBar_20130220.csv")
            printfn "Index loaded..."

            indexMatching (prices, index, signals)