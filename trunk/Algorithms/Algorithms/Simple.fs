namespace Algorithm
    module DecisionCalculator=(*46*)

        let sma(n:int, prices:decimal[])=
            let sma = Array.zeroCreate prices.Length
            
            let mutable firstVal =
                prices
                |> Seq.take (n-1)
                |> Seq.average
            sma.[n-1] <- firstVal

            for i in n..sma.Length-1 do
                sma.[i] <- ((decimal(n)-1m)*sma.[i-1] + prices.[i] - prices.[i-n])/decimal(n-1)

            sma

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              parameter:System.Collections.Generic.Dictionary<string, decimal>)=
            
            let shortLength = int parameter.["SR"]
            let longLength  = int parameter.["LR"]

            chart1.Add("SMA10;#FF0000", new System.Collections.Generic.List<decimal>()) 
            chart1.Add("SMA20;#0000FF", new System.Collections.Generic.List<decimal>()) 
            
            // list of closing prices
            let cPrices = 
                [ for i in prices -> i.Item5 ]
                |> Seq.toArray

            let sma10 = sma(shortLength, cPrices)
            let sma20 = sma(longLength, cPrices)

            for i in 0..sma10.Length-1 do chart1.["SMA10;#FF0000"].Add(sma10.[i])
            for i in 0..sma20.Length-1 do chart1.["SMA20;#0000FF"].Add(sma20.[i])

            signals.Clear();
            for i in signals.Count .. prices.Count-1 do
                signals.Add(
                    if (sma10.[i] = 0m || sma20.[i] = 0m) then
                        0
                    else if (sma10.[i] > sma20.[i]) then
                        1
                    else if (sma10.[i] < sma20.[i]) then
                        -1
                    else
                        signals.[i-1] * -1
                )

            signals