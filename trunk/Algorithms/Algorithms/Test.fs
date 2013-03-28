namespace Algorithm
    module DecisionCalculator=
        open System.IO
        let startCalculation (list2D:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal>>,signals:System.Collections.Generic.List<int>)= 
            let array = Array.zeroCreate 20
            let temp = 
                array
                |>Array.toSeq
                |>Seq.windowed 10
                |>Seq.iter(fun window -> Array.max(window))

            //"C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/CAD_1mBar_20110924.csv"
            //"C:/Users/Josefs/Dropbox/Projekte/Diplomprojekt/CAD_1mBar_20110924.csv"
            let splittedData = (File.ReadAllText("C:/Users/Josefs/Documents/Schule/PPM/noctua/trunk/Input_Data/NKD_1mBar_20110809.csv")).Split([|"\r\n"|], System.StringSplitOptions.RemoveEmptyEntries)
            let test = Array.init (splittedData.Length-2) (fun index -> 
                let a = (splittedData.[index+1]).Split([|','|])
                (System.DateTime.ParseExact(a.[1]+" "+a.[2],"MM/dd/yy HH:mm", new System.Globalization.CultureInfo("en-US")),decimal(a.[3]),decimal(a.[4]),decimal(a.[5]),decimal(a.[6])))
           
            //let prices = [|for line in splittedData -> (fun -> let entries = line.Split([|','|]) ( date, (decimal)entries.[4], (decimal)entries.[5], (decimal)entries.[6], (decimal)entries.[7] ))|]
            
            signals