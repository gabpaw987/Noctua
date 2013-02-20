
namespace Algorithm
    module DecisionCalculator2=
        let rsi(liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            let mutable sumup = 0m
            let mutable sumdown = 0m
            for i = 1 to liste2D.Count - 1 do 
                if(liste2D.[i].Item5>liste2D.[i-1].Item5)then
                    sumup <- sumup + liste2D.[i].Item5
                else if(liste2D.[i].Item5<liste2D.[i-1].Item5)then
                    sumdown <- sumdown - liste2D.[i].Item5
            //sumup/(decimal liste2D.Count)
            //sumdown/(decimal liste2D.Count)
            (sumup/(decimal liste2D.Count))/((sumup/(decimal liste2D.Count))+(sumdown/(decimal liste2D.Count)))
            