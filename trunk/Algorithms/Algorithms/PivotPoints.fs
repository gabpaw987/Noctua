namespace Algorithm
    module DecisionCalculator45=
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
        
        (* return [supportlevel2, supportlevel1, pivotpoint, resistancelevel1, resistancelevel2]*)
        let pivotpointcalcultor(bar:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) = 
            let o = bar.[0].Item2
            let h = List.max [for i in bar -> i.Item3]
            let l = List.min [for i in bar -> i.Item4]
            let c = bar.[bar.Count-1].Item5
            let pivot = (h+l+c)/3m
            let supportlevel1 = 2m*pivot - h
            let resistancelevel1 = 2m*pivot - l
            let supportlevel2 = pivot - (resistancelevel1 - supportlevel1)
            let resistancelevel2 = (pivot - supportlevel1) + resistancelevel1
            let ret = List.append [] [supportlevel2; supportlevel1; pivot; resistancelevel1; resistancelevel2]
            ret 
        
//        let signalgeber (sr:int, n1:int, n2:int, n3:int, list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
//            for i = 0 to n3-1 do
//                signals.Add(0)
//            let shorts = sma2(n1,list2D)
//            let shortmover = (n3 - n1)
//            let middle = sma2(n2,list2D)
//            let middlemover = (n2 - n1) 
//            let longs = sma2(n3,list2D)
//            //let mutable supres = pivotpointcalcultor(list2D.Item(0))
//            for i = 0 to longs.Length - 1 do
//              //  if(i % sr = 0)then
//             //       supres <- pivotpointcalcultor(list2D.Item(i+n3-10))
//              //  let mutable todo = 0
//                if shorts.[i + shortmover] < middle.[i + middlemover] && middle.[i + middlemover] < longs.[i] then
//                    todo <- (-1*momentumInterpreter(5,list2D.GetRange(i+n3-10,10)))
//                else if shorts.[i + shortmover] > middle.[i + middlemover] && middle.[i + middlemover] > longs.[i] then
//                    todo <- (1*momentumInterpreter(5,list2D.GetRange(i+n3-10,10)))
//                if(list2D.Item(i+n3-10).Item4<supres.Item(0)||list2D.Item(i+n3-10).Item4>supres.Item(4))then 
//                    signals.Add(0)
//                else
//                    signals.Add(todo)
//            
//            signals.RemoveAt(signals.Count-1)
//            signals

        let tripleCrossed(n1:int,n2:int,n3:int,list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)=
            for i = 0 to n3-1 do
                signals.Add(0)
            let shorts = List.append [for i in 0..n1-2 -> 0m] (sma2(n1,list2D))
            let middle = List.append [for i in 0..n2-2 -> 0m] (sma2(n2,list2D))
            let longs = List.append [for i in 0..n3-2 -> 0m] (sma2(n3,list2D))
            let mutable supres = []
            let mutable todo = 0
            let mutable highest = 0m
            for i = n3 to longs.Length-1 do
                if(highest<list2D.[i].Item5) then highest <- list2D.[i].Item5
                if(i % n1 = 0) then 
                    supres <- pivotpointcalcultor(list2D.GetRange(i-n1,n1))
                if shorts.[i] < middle.[i] && middle.[i] < longs.[i] then                
                    highest <- 0m
                    if(signals.[signals.Count-1]= -3)then todo <- (-3)
                    else todo <- (-1*momentumInterpreter(5,list2D.GetRange(i-n2,n2)))

                else if shorts.[i] > middle.[i] && middle.[i] > longs.[i] then
                    if(0.95m*highest>list2D.[i].Item5) then todo <- (0)
                    else if(signals.[signals.Count-1]= 3)then todo <- (3)
                         else todo <- (1*momentumInterpreter(5,list2D.GetRange(i-n2,n2)))
                else
                    todo <- 0
                if(list2D.Item(i).Item4<supres.Item(0)||list2D.Item(i).Item4>supres.Item(4))then 
                    signals.Add(0)
                    printf "Yo seas supres is am start"
                else
                    signals.Add(todo)
            signals
        
        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            tripleCrossed (10, 40, 90, list2D, signals)