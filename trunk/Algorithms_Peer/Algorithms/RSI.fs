
namespace Algorithm
    module DecisionCalculator2=
        let rsi(n:int, liste2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>) =
            let mutable rsi = []
            for i = liste2D.Count - n to liste2D.Count - 1 do
                let mutable tempListe = liste2D.GetRange(i - n, n)
                let mutable wins = []
                let mutable losses = []
                for j = 0 to tempListe.Count - 1 do 
                    if tempListe.[j].Item2 < tempListe.[j].Item5 then 
                        wins <- List.append wins [tempListe.[j].Item5-tempListe.[j].Item2]
                    if tempListe.[j].Item5 < tempListe.[j].Item2 then 
                        losses <- List.append losses [tempListe.[j].Item5-tempListe.[j].Item2]
                rsi <- List.append rsi [100m*(List.sum(wins)/(decimal wins.Length))/((List.sum(wins)/(decimal wins.Length))-(List.sum(losses)/(decimal losses.Length)))]
            rsi
            