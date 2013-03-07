namespace Algorithm
    module DecisionCalculator10=        
        
        let signalgenerator(prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,index:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>) =
            //TODO: do the ama with prices list
            //      do the ama with index list

            //TODO: look for the index difference in patterns of the calculated ama´s
            let mutable difference:int = new int()

            if (difference>=0) then 
                //TODO: weight the decisions of the endalgorithm with the index regression
                difference <- 0
            else
                //TODO: just do the normal endalgorithm without the index regression 
                difference <- 0
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
            let index = readIndex("C:\Users\Josefs\Documents\Schule\PPM\noctua\trunk\Input_Data\GOOG_1mBar_20130110.csv")
            signalgenerator (list2D,index, signals)