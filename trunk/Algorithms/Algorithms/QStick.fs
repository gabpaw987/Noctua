namespace Algorithm
    module DecisionCalculator=
        
        let QStick (n:int, oc:(decimal * decimal)[]):decimal[]= 
            let qs = [| for i in 0 .. oc.Length - 1 do yield ((fst oc.[i]) - (snd oc.[i]))|]
            Array.append (Array.zeroCreate n) [| for i in 0 .. qs.Length - n - 1 do yield (Array.sum (qs.[i .. i + n])) / decimal n |]

        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            for i in QStick(14, [| for i in 0.. list2D.Count - 1 do yield (list2D.[i].Item2,list2D.[i].Item5)|]) do printf "%f \n" i
            //QStick(14, [| for i in 0.. list2D.Count do yield (list2D.[i].Item2,list2D.[i].Item5)|])
            for i in list2D do signals.Add 0
            signals
            