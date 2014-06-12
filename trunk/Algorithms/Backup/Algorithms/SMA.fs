
// Weitere Informationen zu F# unter "http://fsharp.net".
namespace Algorithm
    module DecisionCalculator56=        
        (* This function calculates the simple moving average for a list of lists of decimal and returns the calculated values in a list of decimals*)
        let sma(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let mutable sma = [];
            for i = n to liste2D.Count - 1 do
                let mutable mom = 0m
                for j = i - (n-1) to i do
                    mom <- mom + liste2D.[j].Item5
                sma <- List.append sma [mom / decimal n]
            sma

        let sma3(n:int, prices:decimal array)=
            [|for i = n to prices.Length - 1 do 
                yield 
                    Array.sub prices (i - n) n
                    |> Array.average 
            |]

        let wma(n:int, prices:decimal array)=
            [|for i = n to prices.Length - 1 do 
                let nom = ref 0m
                let denom = ref 0m
                let nprices = Array.sub prices (i - n) n
                for j in nprices.Length - 1 .. 0 do
                    nom := !nom + decimal(j + 1) * nprices.[j]
                    denom := !denom + decimal (j + 1)
                yield !nom / !denom
            |]
        let tma(n1:int, n2:int, prices:decimal array)=
            sma3 (n2, (sma3(n1, prices)))

        let sma2(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>)=
            let s1 = new System.Windows.Forms.DataVisualization.Charting.Series("historicalData")
            s1.ChartType <- System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Candlestick
            let s2 = new System.Windows.Forms.DataVisualization.Charting.Series("incdicator")
            for i = 0 to liste2D.Count - 1 do
                let mutable temp:int = s1.Points.AddXY(liste2D.[i].Item1,double liste2D.[i].Item3)
                let mutable liste = [|double liste2D.[i].Item4;double liste2D.[i].Item2;double liste2D.[i].Item5|]
                s1.Points.[i].YValues <- Array.append s1.Points.[i].YValues liste
                temp <- temp+1
            let c1 = new System.Windows.Forms.DataVisualization.Charting.Chart()
            c1.Series.Add(s1)
            c1.Series.Add(s2)
            c1.DataManipulator.FinancialFormula(System.Windows.Forms.DataVisualization.Charting.FinancialFormula.MovingAverage,(string n),"historicalData:Y3","indicator")
            let mutable ergebnis1 = []
            for i = 0 to c1.Series.["indicator"].Points.Count - 1 do 
                ergebnis1 <- List.append ergebnis1 [decimal c1.Series.["indicator"].Points.[i].YValues.[0]]
            ergebnis1
        
        (*
            This function calculates the momentum over the give time period
        *)
        let momentum(period:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime,decimal, decimal, decimal, decimal>>)=
            let mutable result = []
            for i = period to prices.Count - 1 do
                result <- List.append result [prices.[i].Item5 - prices.[i - period].Item5]
            result
        
        (*
            This function calls the momentum function and interprets its results
        *)
        let momentumInterpreter(period:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime,decimal, decimal, decimal, decimal>>)=
            let moments = momentum(period, prices)
            // mom pos und steigend +3
            let mutable m1 = 0
            // mom pos und fallend +1
            let mutable m2 = 0
            // mom neg und fallend -3
            let mutable m3 = 0
            // mom neg und steigend -1
            let mutable m4 = 0
            for i = 1 to moments.Length - 1 do 
                if(moments.[i]>0m&&moments.[i-1]<moments.[i]) then
                    m1 <- m1 + 1
                if(moments.[i]>0m&&moments.[i-1]>moments.[i]) then
                    m2 <- m2 + 1
                if(moments.[i]<0m&&moments.[i-1]>moments.[i]) then
                    m3 <- m3 + 1
                if(moments.[i]<0m&&moments.[i-1]<moments.[i]) then
                    m4 <- m4 + 1
            if ((m1>m2&&m1>m3&&m1>m4)||m3>m1&&m3>m2&&m3>m4) then
                3
            else if ((m2>m1&&m2>m3&&m2>m4)||m4>m1&&m4>m2&&m4>m3) then
                1
            else
                0

        (*  two sma´s
            param:
                n1: smaller range of sma
                n2: higher range of sma     
        *)
        let signalgeber1(n1:int, n2:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n2-1 do
                signals.Add(0)
            let ergebnis1 = sma(n1,list2D)
            let ergebnis2 = sma(n2,list2D)
            let mover = n2 - n1
            for i = 0 to ergebnis2.Length-1 do
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals

        (* 
            SMA vs. Price
        *)
        let signalgeber4(n:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
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
        (*
            this signalgenerator adds a percentage fall to the calcuation system to cut loss
        *)
        let signalgeber3(n1:int, n2:int,percentage:double, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            for i = 0 to n2-1 do
                signals.Add(0)
            let ergebnis1 = sma(n1,list2D)
            let ergebnis2 = sma(n2,list2D)
            let mutable max = 1.0;
            let mover = n2 - n1
            let mutable activatedPercentageFall = false
            for i = 0 to ergebnis2.Length-1 do
                if max < double ( ergebnis1.[i+mover] - ergebnis2.[i]) then 
                    max <- double (ergebnis1.[i+mover] - ergebnis2.[i])
                if (double (ergebnis1.[i+mover] - ergebnis2.[i]))/max > 1.0-percentage then 
                     activatedPercentageFall <- true
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    if activatedPercentageFall then 
                        signals.Add(-1)
                    else 
                        signals.Add(1)
                    activatedPercentageFall <- false
                    max <- 1.0
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals

        (*  two sma´s
            param:
                n1: smaller range of sma
                n2: higher range of sma     
        *)
        let signalgeber(n1:int, n2:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            // add zeros
            for i = 0 to n2-2 do
                signals.Add(0)
            // calculate the two sma´s
            let ergebnis1 = sma2(n1,list2D)
            let ergebnis2 = sma2(n2,list2D)
            
            // the number of indices the smaller is ahead
            let mover = n2 - n1
            for i = 0 to ergebnis2.Length-1 do
                if ergebnis1.[i+mover] > ergebnis2.[i] then
                    signals.Add(-1)
                if ergebnis1.[i+mover] < ergebnis2.[i] then
                    signals.Add(1)
                if ergebnis1.[i+mover] = ergebnis2.[i] then
                    signals.Add(0)
            signals
        

        let tripleCrossed(n1:int,n2:int,n3:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to n3-1 do
                signals.Add(0)
            let shorts = List.append [for i in 0..n1-1 -> 0m] (sma(n1,list2D))
            let middle = List.append [for i in 0..n2-1 -> 0m] (sma(n2,list2D))
            let longs = List.append [for i in 0..n3-1 -> 0m] (sma(n3,list2D))
            for i = n3 to longs.Length-1 do
                if shorts.[i] < middle.[i] && middle.[i] < longs.[i] then
                    if(signals.[signals.Count-1]=3)then signals.Add(3)
                    else signals.Add(-1*momentumInterpreter(5,list2D.GetRange(i-n2,n2)))
                else if shorts.[i] > middle.[i] && middle.[i] > longs.[i] then
                    if(signals.[signals.Count-1]= -3)then signals.Add(-3)
                    else signals.Add(1*momentumInterpreter(5,list2D.GetRange(i-n2,n2)))
                else
                    // add the last again
                    // signals.Add(signals.[signals.Count-1])
                    // add zero
                    signals.Add(0)
            signals

        let readIndex (path:string)= 
            let mutable ownTupleList = new System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>()
            let str = System.IO.File.ReadAllText(path)
            let mutable strArray:List<string> = List.ofArray(str.Split('\n'))
            let mutable fullySplittedStringArrayArray:List<List<string>> = []
            for s:string in strArray do
                fullySplittedStringArrayArray <- List.append fullySplittedStringArrayArray [List.ofArray(s.Split(','))]
            
            // nun haben wir eine beschissene Liste aus den eingelesenen Daten jetzt noch in eine Tupleliste

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
            decimal liste2D.Count*b + a

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            //let index = readIndex("C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/GOOG_1mBar_20130110.csv")
            tripleCrossed (10,40,90, list2D, signals)